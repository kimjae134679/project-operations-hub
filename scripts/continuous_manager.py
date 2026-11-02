"""Bounded, reviewed-plan runner. Trusted argv is NOT an operating-system sandbox."""
import argparse
import contextlib
import datetime
import hashlib
import json
import math
import os
from pathlib import Path
import re
import stat
import subprocess
import sys
import threading
import time
import uuid


SAFE_ID = re.compile(r'^[A-Za-z0-9][A-Za-z0-9_-]{0,79}$')
AI_LINE_BYTES = 65536
AI_OUTPUT_BYTES = 512 * 1024
AI_ARTIFACT_BYTES = 1024 * 1024
GUARD = ('Work only in the approved project workspace. Read its current project guide, '
         'AGENTS/README and new notices before acting; never acknowledge notices for other AIs. '
         'Do not touch the Desktop, other projects, accounts or permissions. '
         'No new payments, paid services, Astra, external publication or bulk original deletion. '
         'Stop and report a blocker if the scoped task requires any of those. '
         'Preserve existing remote connections and unrelated running work.\n\n')


def now():
    return datetime.datetime.now(datetime.timezone.utc).isoformat()


def plan_hash(plan):
    return hashlib.sha256(json.dumps(plan, sort_keys=True, ensure_ascii=False,
                                     separators=(',', ':'), allow_nan=False).encode()).hexdigest()


def safe_path(value, root=None):
    p = Path(value)
    if '..' in p.parts:
        raise ValueError('parent traversal is not permitted')
    if not p.is_absolute():
        if root is None:
            raise ValueError('approved root must be absolute')
        p = root / p
    if any(part.casefold() in ('desktop', '바탕화면') for part in p.parts):
        raise ValueError('Desktop paths are not permitted')
    for item in (p, *p.parents):
        if item.exists() or item.is_symlink():
            info = item.lstat()
            if stat.S_ISLNK(info.st_mode) or getattr(info, 'st_file_attributes', 0) & 0x400:
                raise ValueError('symlinks/reparse points are not permitted: ' + str(item))
    p = p.resolve()
    if root is not None and not p.is_relative_to(root):
        raise ValueError('path escapes approved root')
    return p


def number(value, maximum, label, integer=False):
    if isinstance(value, bool) or not isinstance(value, (int, float)):
        raise ValueError(label + ' must be a positive number')
    if not math.isfinite(value) or not 0 < value <= maximum or (integer and not isinstance(value, int)):
        raise ValueError(label + ' is out of bounds')


def strings(values, label):
    if not isinstance(values, list) or not values or len(values) > 100:
        raise ValueError(label + ' must be a nonempty bounded argv array')
    if any(not isinstance(v, str) or not v or '\x00' in v or len(v) > 16384 for v in values):
        raise ValueError(label + ' contains invalid strings')


def executable(value):
    p = Path(value)
    if not p.is_absolute() or not p.is_file() or p.suffix.casefold() in ('.cmd', '.bat', '.ps1'):
        raise ValueError('executable must be an existing absolute binary path (no shell wrapper)')


def explicit_model(model):
    return (isinstance(model, str) and re.fullmatch(r'gpt-[0-9][A-Za-z0-9._:-]{0,95}', model)
            and 'astra' not in model.casefold() and 'router' not in model.casefold())


def completion_contract(step, root):
    declared = step.get('completion_contract')
    if declared is None:
        return {'adapter': 'codex-jsonl-v1', 'model': step['model']} if step['type'] == 'codex' else None
    if not isinstance(declared, dict) or set(declared) - {'adapter', 'model', 'required_report_path'}:
        raise ValueError('invalid explicit AI completion contract')
    adapter, model = declared.get('adapter'), declared.get('model', step.get('model'))
    if adapter not in ('codex-jsonl-v1', 'wrapper-json-v1') or not explicit_model(model):
        raise ValueError('explicit supported AI adapter and concrete non-Astra model required')
    if step['type'] == 'codex' and model != step['model']:
        raise ValueError('completion contract model must match declared agent model')
    result = dict(declared, adapter=adapter, model=model)
    if 'required_report_path' in result:
        value = result['required_report_path']
        if not isinstance(value, str) or not value or len(value) > 4096 or '\x00' in value:
            raise ValueError('invalid required report path')
        path = safe_path(value, root)
        if path == root or ':' in str(path)[len(path.anchor):]:
            raise ValueError('required report must be a contained regular file')
    return result


def validate_plan(plan):
    if not isinstance(plan, dict):
        raise ValueError('plan must be an object')
    required = {'schema_version', 'id', 'project', 'approved_root', 'max_steps',
                'max_run_seconds', 'max_output_bytes', 'steps'}
    if set(plan) != required or plan['schema_version'] != 1:
        raise ValueError('unsupported or incomplete plan schema')
    if not isinstance(plan['id'], str) or not SAFE_ID.fullmatch(plan['id']):
        raise ValueError('invalid plan id')
    if not isinstance(plan['project'], str) or not plan['project'].strip() or len(plan['project']) > 100:
        raise ValueError('project must be an explicit bounded name')
    root = safe_path(plan['approved_root'])
    if not root.is_dir() or root == Path(root.anchor):
        raise ValueError('approved root must be an existing project directory, not a drive root')
    number(plan['max_steps'], 1000, 'max_steps', True)
    number(plan['max_run_seconds'], 86400, 'max_run_seconds')
    number(plan['max_output_bytes'], 64 * 1024 * 1024, 'max_output_bytes', True)
    steps = plan['steps']
    if not isinstance(steps, list) or not 0 < len(steps) <= plan['max_steps']:
        raise ValueError('steps exceed approved maximum')
    ids = set()
    for step in steps:
        if not isinstance(step, dict):
            raise ValueError('step must be an object')
        base = {'id', 'type', 'cwd', 'depends_on', 'timeout_seconds'}
        fields = {'command': {'argv'}, 'codex': {'executable', 'model', 'prompt'}}
        kind = step.get('type')
        if kind not in fields or not base | fields[kind] <= set(step):
            raise ValueError('unsupported or incomplete step type')
        allowed = base | fields[kind] | {'completion_contract'} | ({'prefix_argv'} if kind == 'codex' else set())
        if set(step) - allowed:
            raise ValueError('unknown step field (no implicit permissions or retry policy)')
        name = step['id']
        if not isinstance(name, str) or not SAFE_ID.fullmatch(name) or name in ids:
            raise ValueError('invalid or duplicate step id')
        ids.add(name)
        cwd = safe_path(step['cwd'], root)
        if not cwd.is_dir():
            raise ValueError('step cwd must exist')
        number(step['timeout_seconds'], 86400, 'timeout_seconds')
        deps = step['depends_on']
        if not isinstance(deps, list) or any(not isinstance(v, str) for v in deps) or len(deps) != len(set(deps)):
            raise ValueError('depends_on must contain unique step ids')
        if kind == 'command':
            strings(step['argv'], 'argv')
            executable(step['argv'][0])
        else:
            executable(step['executable'])
            model = step['model']
            if not explicit_model(model):
                raise ValueError('an explicit non-Astra model is required')
            prompt = step['prompt']
            if not isinstance(prompt, str) or not prompt.strip() or len(prompt) > 12000 or '\x00' in prompt:
                raise ValueError('agent prompt must contain 1..12000 characters')
            if 'prefix_argv' in step:
                strings(step['prefix_argv'], 'prefix_argv')
                # Prefix is only a reviewed local CLI script, never arbitrary CLI flags.
                if len(step['prefix_argv']) != 1 or not Path(step['prefix_argv'][0]).is_absolute() or not Path(step['prefix_argv'][0]).is_file():
                    raise ValueError('prefix_argv supports one absolute local CLI script only')
        completion_contract(step, root)
    visited = set()
    while len(visited) < len(steps):
        eligible = [s for s in steps if s['id'] not in visited and set(s['depends_on']) <= visited]
        if not eligible:
            raise ValueError('missing dependency or dependency cycle')
        visited.update(s['id'] for s in eligible)
    return root


def step_command(step, root):
    if step['type'] == 'command':
        return step['argv'], None
    return ([step['executable'], *step.get('prefix_argv', []), 'exec', '--model', step['model'],
             '--sandbox', 'workspace-write', '-c', 'approval_policy="never"',
             '--cd', str(safe_path(step['cwd'], root)), '--color', 'never', '--json', '-'],
            GUARD + step['prompt'])


def atomic_json(path, value):
    safe_path(path)
    temporary = path.with_name(path.name + '.' + uuid.uuid4().hex + '.tmp')
    try:
        with temporary.open('x', encoding='utf-8') as stream:
            json.dump(value, stream, ensure_ascii=False, indent=2, allow_nan=False)
            stream.flush()
            os.fsync(stream.fileno())
        # Windows status readers may briefly hold a non-delete-sharing handle.
        # Retry only this same atomic rename, never the executed command.
        deadline = time.monotonic() + 1
        while True:
            try:
                os.replace(temporary, path)
                break
            except PermissionError:
                if os.name != 'nt' or time.monotonic() >= deadline:
                    raise
                time.sleep(0.02)
    finally:
        if temporary.exists():
            temporary.unlink()


@contextlib.contextmanager
def exclusive_lock(path):
    safe_path(path)
    stream = path.open('a+b')
    try:
        if stream.tell() == 0:
            stream.write(b'0')
            stream.flush()
        stream.seek(0)
        try:
            if os.name == 'nt':
                import msvcrt
                msvcrt.locking(stream.fileno(), msvcrt.LK_NBLCK, 1)
            else:
                import fcntl
                fcntl.flock(stream.fileno(), fcntl.LOCK_EX | fcntl.LOCK_NB)
        except OSError as exc:
            raise ValueError('another runner owns this state directory') from exc
        yield
    finally:
        # Closing releases the OS lock, even after a crash. Never unlink its inode.
        stream.close()


def initial_state(plan):
    return {'schema_version': 1, 'plan_id': plan['id'], 'project': plan['project'],
            'approved_root': str(validate_plan(plan)), 'plan_sha256': plan_hash(plan),
            'status': 'pending', 'updated_at': now(),
            'steps': {s['id']: {'status': 'pending', 'attempts': 0} for s in plan['steps']}}


def event(directory, kind, **details):
    path = safe_path(directory / 'events.private.jsonl')
    with path.open('a', encoding='utf-8') as stream:
        stream.write(json.dumps({'time': now(), 'event': kind, **details}, ensure_ascii=False) + '\n')
        stream.flush()
        os.fsync(stream.fileno())


def request_stop(directory):
    directory = safe_path(directory)
    state = json.loads(safe_path(directory / 'state.json').read_text(encoding='utf-8'))
    root = safe_path(state['approved_root'])
    safe_path(directory, root)
    atomic_json(directory / 'stop.request.json', {'plan_sha256': state['plan_sha256'], 'time': now()})


def stop_requested(directory, digest):
    path = safe_path(directory / 'stop.request.json')
    if not path.exists():
        return False
    return json.loads(path.read_text(encoding='utf-8')).get('plan_sha256') == digest


class OwnedJob:
    """Windows job object owns descendants; POSIX owns the child process group."""
    def __init__(self, process):
        self.process = process
        self.handle = None
        if os.name == 'nt':
            import ctypes
            from ctypes import wintypes
            self.kernel = ctypes.WinDLL('kernel32', use_last_error=True)
            self.kernel.CreateJobObjectW.argtypes = [ctypes.c_void_p, wintypes.LPCWSTR]
            self.kernel.CreateJobObjectW.restype = wintypes.HANDLE
            self.kernel.AssignProcessToJobObject.argtypes = [wintypes.HANDLE, wintypes.HANDLE]
            self.kernel.AssignProcessToJobObject.restype = wintypes.BOOL
            self.kernel.TerminateJobObject.argtypes = [wintypes.HANDLE, wintypes.UINT]
            self.kernel.CloseHandle.argtypes = [wintypes.HANDLE]
            self.kernel.SetInformationJobObject.argtypes = [wintypes.HANDLE, ctypes.c_int, ctypes.c_void_p, wintypes.DWORD]
            self.kernel.SetInformationJobObject.restype = wintypes.BOOL
            class BasicLimits(ctypes.Structure):
                _fields_ = [('process_time', ctypes.c_longlong), ('job_time', ctypes.c_longlong),
                            ('flags', wintypes.DWORD), ('min_working_set', ctypes.c_size_t),
                            ('max_working_set', ctypes.c_size_t), ('active_process_limit', wintypes.DWORD),
                            ('affinity', ctypes.c_size_t), ('priority', wintypes.DWORD), ('scheduling', wintypes.DWORD)]
            class ExtendedLimits(ctypes.Structure):
                _fields_ = [('basic', BasicLimits), ('io', ctypes.c_ulonglong * 6),
                            ('process_memory', ctypes.c_size_t), ('job_memory', ctypes.c_size_t),
                            ('peak_process_memory', ctypes.c_size_t), ('peak_job_memory', ctypes.c_size_t)]
            limits = ExtendedLimits()
            limits.basic.flags = 0x2000  # JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE; no breakaway.
            self.handle = self.kernel.CreateJobObjectW(None, None)
            if (not self.handle or not self.kernel.SetInformationJobObject(self.handle, 9, ctypes.byref(limits), ctypes.sizeof(limits))
                    or not self.kernel.AssignProcessToJobObject(self.handle, int(process._handle))):
                if self.handle:
                    self.kernel.CloseHandle(self.handle)
                self.handle = None
                process.kill()
                process.wait(timeout=5)
                raise OSError('cannot establish own-child Windows job containment')

    def resume(self):
        if os.name != 'nt':
            return
        # Popen closes its primary thread handle. Find only our suspended child's
        # threads through documented Toolhelp APIs, after assigning the job.
        import ctypes
        from ctypes import wintypes
        class ThreadEntry(ctypes.Structure):
            _fields_ = [('size', wintypes.DWORD), ('usage', wintypes.DWORD),
                        ('thread_id', wintypes.DWORD), ('process_id', wintypes.DWORD),
                        ('base_priority', wintypes.LONG), ('delta_priority', wintypes.LONG), ('flags', wintypes.DWORD)]
        self.kernel.CreateToolhelp32Snapshot.argtypes = [wintypes.DWORD, wintypes.DWORD]
        self.kernel.CreateToolhelp32Snapshot.restype = wintypes.HANDLE
        self.kernel.Thread32First.argtypes = [wintypes.HANDLE, ctypes.POINTER(ThreadEntry)]
        self.kernel.Thread32Next.argtypes = [wintypes.HANDLE, ctypes.POINTER(ThreadEntry)]
        self.kernel.OpenThread.argtypes = [wintypes.DWORD, wintypes.BOOL, wintypes.DWORD]
        self.kernel.OpenThread.restype = wintypes.HANDLE
        self.kernel.ResumeThread.argtypes = [wintypes.HANDLE]
        self.kernel.ResumeThread.restype = wintypes.DWORD
        snapshot = self.kernel.CreateToolhelp32Snapshot(4, 0)
        if snapshot == ctypes.c_void_p(-1).value:
            raise OSError('cannot enumerate owned suspended child thread')
        entry = ThreadEntry()
        entry.size = ctypes.sizeof(entry)
        resumed = False
        try:
            available = self.kernel.Thread32First(snapshot, ctypes.byref(entry))
            while available:
                if entry.process_id == self.process.pid:
                    handle = self.kernel.OpenThread(2, False, entry.thread_id)
                    if handle:
                        try:
                            resumed = self.kernel.ResumeThread(handle) != 0xffffffff
                        finally:
                            self.kernel.CloseHandle(handle)
                    break
                available = self.kernel.Thread32Next(snapshot, ctypes.byref(entry))
        finally:
            self.kernel.CloseHandle(snapshot)
        if not resumed:
            raise OSError('cannot resume owned child after establishing containment')

    def terminate(self):
        if os.name == 'nt':
            self.kernel.TerminateJobObject(self.handle, 1)
        else:
            import signal
            try:
                os.killpg(self.process.pid, signal.SIGKILL)
            except ProcessLookupError:
                pass
        self.process.wait(timeout=5)

    def close(self):
        # Also contain descendants after their direct parent exits.
        self.terminate()
        if self.handle:
            self.kernel.CloseHandle(self.handle)
            self.handle = None


def read_usage(path):
    usage = None
    total = 0
    with path.open('rb') as stream:
        for line in iter(lambda: stream.readline(AI_LINE_BYTES + 1), b''):
            total += len(line)
            if len(line) > AI_LINE_BYTES or total > AI_OUTPUT_BYTES:
                break
            try:
                item = json.loads(line.decode('utf-8'), object_pairs_hook=_unique_json_object)
                if isinstance(item, dict) and item.get('type') == 'turn.completed' and isinstance(item.get('usage'), dict):
                    usage = {key: value for key, value in item['usage'].items()
                             if key in ('input_tokens', 'output_tokens', 'cached_input_tokens')
                             and type(value) is int and 0 <= value <= 10**12}
            except (ValueError, UnicodeError, TypeError, RecursionError):
                continue
    return usage


def _unique_json_object(pairs):
    value = {}
    for key, item in pairs:
        if key in value:
            raise ValueError('duplicate JSON property')
        value[key] = item
    return value


def ai_receipt(contract, stdout, result, root, started_ns, identity, preflight=None):
    receipt = {'schemaVersion': 1, 'adapter': contract['adapter'], 'executionId': identity['planId'] + '/' + identity['stepId'],
               'requestedModel': contract['model'], 'status': 'failed', 'processExitCode': result.get('returncode'),
               'terminalObserved': False, 'reportObserved': False, 'reportRequired': 'required_report_path' in contract,
               'artifactVerified': False if 'required_report_path' in contract else None, 'artifactSha256': None,
               'failureCode': preflight, **identity}
    failure, terminals, wrappers, total = preflight, 0, 0, 0
    try:
        with stdout.open('rb') as stream:
            for line in iter(lambda: stream.readline(AI_LINE_BYTES + 1), b''):
                total += len(line)
                if len(line) > AI_LINE_BYTES or total > AI_OUTPUT_BYTES:
                    failure = failure or 'output_limit'
                    break
                if not line.strip():
                    continue
                try:
                    item = json.loads(line.decode('utf-8'), object_pairs_hook=_unique_json_object,
                                      parse_constant=lambda _: (_ for _ in ()).throw(ValueError('nonfinite JSON')))
                    if not isinstance(item, dict):
                        raise ValueError('event must be object')
                    if contract['adapter'] == 'wrapper-json-v1':
                        wrappers += 1
                        receipt['terminalObserved'] = item.get('state') in ('succeeded', 'completed', 'failed')
                        receipt['reportObserved'] = item.get('reportPresent') is True
                        if (wrappers != 1 or item.get('state') not in ('succeeded', 'completed')
                                or type(item.get('exitCode')) is not int or item['exitCode'] != 0
                                or item.get('reportPresent') is not True or item.get('model') != contract['model']):
                            failure = failure or 'wrapper_failed'
                    else:
                        kind = item.get('type')
                        if kind in ('error', 'turn.failed') or item.get('error') is not None:
                            failure = failure or 'ai_failed'
                        child = item.get('item')
                        # Explicit item errors fail the turn contract; ordinary tool
                        # exit/status alone may be recovered by the model.
                        if (isinstance(kind, str) and kind.startswith('item.') and isinstance(child, dict)
                                and (child.get('type') == 'error' or child.get('error') is not None)):
                            failure = failure or 'ai_failed'
                        if kind == 'turn.completed':
                            terminals += 1
                            receipt['terminalObserved'] = True
                            if terminals != 1:
                                failure = failure or 'contradictory_terminal'
                        if (kind == 'item.completed' and isinstance(child, dict) and child.get('type') == 'agent_message'
                                and isinstance(child.get('text'), str) and child['text'].strip()):
                            receipt['reportObserved'] = True
                        if not isinstance(kind, str):
                            failure = failure or 'invalid_event'
                        if 'model' in item and item['model'] != contract['model']:
                            failure = failure or 'model_mismatch'
                except (ValueError, UnicodeError, RecursionError):
                    failure = failure or 'invalid_json'
    except OSError:
        failure = failure or 'stdout_unavailable'
    if result['status'] != 'succeeded' or result.get('returncode') != 0:
        failure = 'process_' + result['status']
    if not receipt['terminalObserved']:
        failure = failure or 'terminal_missing'
    if not receipt['reportObserved']:
        failure = failure or 'report_missing'
    if 'required_report_path' in contract and failure is None:
        try:
            path = safe_path(contract['required_report_path'], root)
            before = path.stat()
            if not stat.S_ISREG(before.st_mode) or not 0 < before.st_size <= AI_ARTIFACT_BYTES or before.st_mtime_ns < started_ns:
                raise ValueError('artifact missing stale or oversized')
            with path.open('rb') as artifact:
                opened = os.fstat(artifact.fileno())
                content = artifact.read(AI_ARTIFACT_BYTES + 1)
                after = os.fstat(artifact.fileno())
            final = safe_path(contract['required_report_path'], root).stat()
            signature = lambda s: (s.st_dev, s.st_ino, s.st_size, s.st_mtime_ns)
            if len(content) != before.st_size or not signature(before) == signature(opened) == signature(after) == signature(final):
                raise ValueError('artifact changed')
            receipt['artifactVerified'] = True
            receipt['artifactSha256'] = hashlib.sha256(content).hexdigest()
        except (OSError, ValueError):
            failure = 'artifact_invalid'
    receipt['failureCode'] = failure
    receipt['status'] = 'failed' if failure else 'succeeded'
    return receipt


def execute_step(step, root, directory, digest, deadline, limit, plan_id=''):
    contract = completion_contract(step, root)
    started_ns = time.time_ns()
    identity = {'planId': plan_id, 'stepId': step['id'], 'planSha256': digest}
    preflight = None
    if contract and 'required_report_path' in contract and safe_path(contract['required_report_path'], root).exists():
        preflight = 'report_preexisting'
        result = {'status': 'failed', 'returncode': None, 'finished_at': now()}
        result['aiReceipt'] = ai_receipt(contract, directory / 'no-stdout', result, root, started_ns, identity, preflight)
        return result
    argv, prompt = step_command(step, root)
    logs = safe_path(directory / 'logs', root)
    logs.mkdir(exist_ok=True)
    stdout = safe_path(logs / (step['id'] + '.stdout.private.log'), root)
    stderr = safe_path(logs / (step['id'] + '.stderr.private.log'), root)
    step_deadline = min(deadline, time.monotonic() + step['timeout_seconds'])
    process = None
    job = None
    readers = []
    exceeded = threading.Event()
    output_lock = threading.Lock()
    written = [0]
    reader_errors = []
    def drain(pipe, output):
        try:
            while True:
                chunk = pipe.read1(8192)
                if not chunk:
                    break
                with output_lock:
                    keep = min(len(chunk), max(0, limit - written[0]))
                    output.write(chunk[:keep])
                    output.flush()
                    written[0] += keep
                    if keep < len(chunk):
                        exceeded.set()
        except OSError as exc:
            reader_errors.append(str(exc))
            exceeded.set()
        finally:
            pipe.close()
    result = {'status': 'failed', 'returncode': None, 'finished_at': now()}
    try:
        with contextlib.ExitStack() as stack:
            out = stack.enter_context(stdout.open('xb'))
            err = stack.enter_context(stderr.open('xb'))
            stdin = subprocess.DEVNULL
            if prompt:
                # File-backed bounded prompt avoids blocking the watchdog when
                # the CLI crashes/hangs before reading its input pipe.
                prompt_path = safe_path(logs / (step['id'] + '.prompt.private.txt'), root)
                with prompt_path.open('x', encoding='utf-8') as prompt_file:
                    prompt_file.write(prompt)
                stdin = stack.enter_context(prompt_path.open('rb'))
            flags = (subprocess.CREATE_NO_WINDOW | 0x4) if os.name == 'nt' else 0  # CREATE_SUSPENDED
            process = subprocess.Popen(argv, cwd=str(safe_path(step['cwd'], root)), shell=False,
                                       stdin=stdin, stdout=subprocess.PIPE, stderr=subprocess.PIPE, creationflags=flags,
                                       start_new_session=os.name != 'nt')
            job = OwnedJob(process)
            for pipe, output in ((process.stdout, out), (process.stderr, err)):
                reader = threading.Thread(target=drain, args=(pipe, output), daemon=True)
                readers.append(reader)
                reader.start()
            job.resume()
            event(directory, 'child_started', step=step['id'], owned_pid=process.pid)
            while True:
                if exceeded.is_set():
                    result['status'] = 'output_limit'
                    break
                if stop_requested(directory, digest):
                    result['status'] = 'stopped'
                    break
                if time.monotonic() >= step_deadline:
                    result['status'] = 'timed_out'
                    break
                if process.poll() is not None:
                    result['returncode'] = process.returncode
                    result['status'] = 'succeeded' if process.returncode == 0 else 'failed'
                    break
                time.sleep(0.05)
            job.close()
            job = None
            for reader in readers:
                reader.join(timeout=5)
            if exceeded.is_set():
                result['status'] = 'output_limit'
            if reader_errors or any(reader.is_alive() for reader in readers):
                result.update(status='failed', error='private log capture incomplete')
            result['returncode'] = process.returncode
    except (OSError, subprocess.SubprocessError) as exc:
        result['error'] = type(exc).__name__ + ': ' + str(exc)
    finally:
        if job:
            job.close()
        elif process and process.poll() is None:
            process.kill()
            process.wait(timeout=5)
        for reader in readers:
            reader.join(timeout=5)
        result['finished_at'] = now()
    if stdout.exists():
        usage = read_usage(stdout)
        if usage is not None:
            result['usage'] = usage
    if contract:
        result['aiReceipt'] = ai_receipt(contract, stdout, result, root, started_ns, identity)
        if result['status'] == 'succeeded' and result['aiReceipt']['status'] != 'succeeded':
            result['status'] = 'output_limit' if result['aiReceipt']['failureCode'] == 'output_limit' else 'failed'
    return result


def run_plan(plan, directory, approved_digest):
    root = validate_plan(plan)
    digest = plan_hash(plan)
    if approved_digest != digest:
        raise ValueError('explicit approved SHA-256 does not match this immutable plan')
    directory = safe_path(directory, root)
    if directory == root:
        raise ValueError('state directory must be a dedicated child of approved root')
    directory.mkdir(parents=True, exist_ok=True)
    with exclusive_lock(directory / 'runner.lock'):
        state_path = safe_path(directory / 'state.json', root)
        state = json.loads(state_path.read_text(encoding='utf-8')) if state_path.exists() else initial_state(plan)
        if state.get('plan_sha256') != digest or set(state.get('steps', {})) != {s['id'] for s in plan['steps']}:
            raise ValueError('state belongs to a different immutable plan; use a new plan/state directory')
        for value in state['steps'].values():
            if value['status'] == 'running':
                value.update(status='blocked', reason='previous execution interrupted; outcome ambiguous; no automatic retry')
        deadline = time.monotonic() + plan['max_run_seconds']
        state['status'] = 'running'
        state['updated_at'] = now()
        atomic_json(state_path, state)
        event(directory, 'run_started', plan_sha256=digest)
        try:
            while True:
                pending = [s for s in plan['steps'] if state['steps'][s['id']]['status'] == 'pending']
                if not pending:
                    break
                if stop_requested(directory, digest) or time.monotonic() >= deadline:
                    for step in pending:
                        state['steps'][step['id']].update(status='blocked', reason='stop requested or run watchdog expired')
                    break
                eligible = []
                for step in pending:
                    deps = [state['steps'][d]['status'] for d in step['depends_on']]
                    if any(v not in ('pending', 'running', 'succeeded') for v in deps):
                        state['steps'][step['id']].update(status='blocked', reason='prerequisite did not succeed')
                    elif all(v == 'succeeded' for v in deps):
                        eligible.append(step)
                if not eligible:
                    # A validated DAG with no running or eligible step can only
                    # have descendants of a non-successful prerequisite left.
                    for step in pending:
                        if state['steps'][step['id']]['status'] == 'pending':
                            state['steps'][step['id']].update(status='blocked', reason='prerequisite chain did not succeed')
                    break
                step = eligible[0]
                value = state['steps'][step['id']]
                value.update(status='running', attempts=value['attempts'] + 1, started_at=now())
                state['updated_at'] = now()
                atomic_json(state_path, state)  # Durable BEFORE spawn: unknown completion never auto-retries.
                event(directory, 'step_started', step=step['id'])
                value.update(execute_step(step, root, directory, digest, deadline, plan['max_output_bytes'], plan['id']))
                state['updated_at'] = now()
                atomic_json(state_path, state)
                event(directory, 'step_finished', step=step['id'], status=value['status'], returncode=value.get('returncode'))
        except BaseException:
            state['status'] = 'interrupted'
            state['updated_at'] = now()
            atomic_json(state_path, state)
            raise
        state['status'] = 'succeeded' if all(v['status'] == 'succeeded' for v in state['steps'].values()) else 'blocked'
        state['updated_at'] = now()
        atomic_json(state_path, state)
        event(directory, 'run_finished', status=state['status'])
        return 0 if state['status'] == 'succeeded' else 1


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    sub = parser.add_subparsers(dest='operation', required=True)
    for operation in ('validate', 'run'):
        cmd = sub.add_parser(operation)
        cmd.add_argument('--plan', required=True)
        if operation == 'run':
            cmd.add_argument('--state-dir', required=True)
            cmd.add_argument('--approve-sha256', required=True)
    for operation in ('status', 'stop'):
        sub.add_parser(operation).add_argument('--state-dir', required=True)
    args = parser.parse_args(argv)
    try:
        if args.operation in ('validate', 'run'):
            plan = json.loads(Path(args.plan).read_text(encoding='utf-8-sig'))
            validate_plan(plan)
            if args.operation == 'validate':
                print(json.dumps({'valid': True, 'sha256': plan_hash(plan), 'steps': len(plan['steps']),
                                  'warning': 'Review argv and scope before approval; trusted commands are not OS-sandboxed.'}))
                return 0
            result = run_plan(plan, Path(args.state_dir), args.approve_sha256)
            print(json.dumps({'status': 'succeeded' if result == 0 else 'blocked', 'state_dir': args.state_dir}))
            return result
        directory = safe_path(args.state_dir)
        if args.operation == 'stop':
            request_stop(directory)
            print(json.dumps({'stop_requested': True, 'state_dir': str(directory)}))
        else:
            print(safe_path(directory / 'state.json').read_text(encoding='utf-8'))
        return 0
    except (ValueError, OSError, KeyError) as exc:
        print(json.dumps({'error': str(exc)}), file=sys.stderr)
        return 2


if __name__ == '__main__':
    raise SystemExit(main())
