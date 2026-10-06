"""General PC operations for an authenticated PRIVATE relay only.

Never put returned file content, command output, screenshots, or path inventory
in a public repository. This module itself has no network APIs.
"""
import base64
import ctypes
import hashlib
import json
import os
from pathlib import Path
import re
import secrets
import shutil
import subprocess
import sys
import time
import uuid
import process_runner as runner

MAX_BYTES=512*1024
OWNED_RUNNERS={}
HASH=re.compile(r'^[a-f0-9]{64}$')
ACTIONS=frozenset(('read_file','write_file','list_dir','run_command','start_process',
    'process_status','stop_process','capabilities','ui_control'))

class ActionError(Exception):pass

def sha(data):return hashlib.sha256(data).hexdigest()

def file_sha(path):
    if not path.is_file():return None
    h=hashlib.sha256()
    with path.open('rb') as source:
        for block in iter(lambda:source.read(1024*1024),b''):h.update(block)
    return h.hexdigest()

def absolute_path(value):
    if not isinstance(value,str) or '\x00' in value or not Path(value).is_absolute():raise ActionError('absolute_path_required')
    return Path(value).resolve()

def protected_roots(config):
    roots=config.get('protectedRoots',[])
    # PS pipelines serialize a one-item array as a scalar. Preserve protection.
    if isinstance(roots,str):roots=[roots]
    if not isinstance(roots,list):raise ActionError('invalid_protected_roots')
    for root in roots:absolute_path(root)
    return roots

def protected(path,config):
    # Resolve junctions/symlinks before comparison. Also protect this user's
    # actual known Desktop even if caller forgot to add it to configuration.
    roots=list(protected_roots(config))
    if os.name=='nt':
        roots.extend(str(Path(os.environ.get('USERPROFILE',str(Path.home()))) / name) for name in ('Desktop','OneDrive/Desktop'))
        try:
            from ctypes import wintypes
            buf=ctypes.create_unicode_buffer(32768)
            shell=ctypes.WinDLL('shell32');shell.SHGetFolderPathW.argtypes=[wintypes.HWND,ctypes.c_int,wintypes.HANDLE,wintypes.DWORD,wintypes.LPWSTR]
            if shell.SHGetFolderPathW(None,0x10,None,0,buf)==0:roots.append(buf.value)
        except OSError:pass
    for value in roots:
        root=absolute_path(value)
        if path==root or path.is_relative_to(root):return True
    return False

def file_lock(path):
    """Per-file lock; no unsafe stale lock deletion or global file locks."""
    return FileLock(path)

class FileLock:
    def __init__(self,path):self.path=path;self.file=None
    def __enter__(self):
        self.path.parent.mkdir(parents=True,exist_ok=True);self.file=self.path.open('a+b')
        if self.file.seek(0,2)==0:self.file.write(b'0');self.file.flush()
        self.file.seek(0)
        try:
            if os.name=='nt':
                import msvcrt;msvcrt.locking(self.file.fileno(),msvcrt.LK_NBLCK,1)
            else:
                import fcntl;fcntl.flock(self.file,fcntl.LOCK_EX|fcntl.LOCK_NB)
        except OSError:self.file.close();raise ActionError('file_busy')
        return self
    def __exit__(self,*args):
        if os.name=='nt':
            import msvcrt;self.file.seek(0);msvcrt.locking(self.file.fileno(),msvcrt.LK_UNLCK,1)
        self.file.close()

def bounded_read(path,maximum=MAX_BYTES):
    if type(maximum) is not int or not 1<=maximum<=MAX_BYTES:raise ActionError('invalid_max_bytes')
    if not path.is_file():raise ActionError('file_not_found')
    size=path.stat().st_size
    if size>maximum:raise ActionError('file_too_large')
    with path.open('rb') as source:data=source.read(maximum+1)
    if len(data)>maximum:raise ActionError('file_too_large')
    return data

def read_chunk(path,maximum=MAX_BYTES,offset=0):
    if type(maximum) is not int or not 1<=maximum<=MAX_BYTES:raise ActionError('invalid_max_bytes')
    if type(offset) is not int or offset<0:raise ActionError('invalid_offset')
    if not path.is_file():raise ActionError('file_not_found')
    before=path.stat();size=before.st_size
    if offset>size:raise ActionError('offset_beyond_end')
    with path.open('rb') as source:source.seek(offset);data=source.read(maximum)
    whole_hash=file_sha(path)
    after=path.stat()
    if (before.st_size,before.st_mtime_ns,before.st_ctime_ns)!=(after.st_size,after.st_mtime_ns,after.st_ctime_ns):raise ActionError('file_changed_during_read')
    return data,size,whole_hash

def redact(value):
    value=re.sub(r'\b(?:gh[pousr]_[A-Za-z0-9_]{20,}|github_pat_[A-Za-z0-9_]{20,}|sk-[A-Za-z0-9_-]{20,})\b','[credential redacted]',value)
    for key,secret in os.environ.items():
        if any(word in key.upper() for word in ('TOKEN','PASSWORD','SECRET','API_KEY')) and len(secret)>=8:value=value.replace(secret,'[credential redacted]')
    return value

def command(args,config):
    if set(args)-{'argv','powershell','python','cwd','timeoutSeconds'}:raise ActionError('unknown_command_field')
    kinds=[k for k in ('argv','powershell','python') if k in args]
    if len(kinds)!=1:raise ActionError('one_command_kind_required')
    cwd=absolute_path(args.get('cwd'))
    if not cwd.is_dir():raise ActionError('cwd_not_found')
    if protected(cwd,config):raise ActionError('protected_cwd')
    if kinds[0]=='argv':
        argv=args['argv']
        if not isinstance(argv,list) or not 1<=len(argv)<=128 or any(not isinstance(a,str) or '\x00' in a or len(a)>32768 for a in argv):raise ActionError('invalid_argv')
        executable=absolute_path(argv[0])
        if not executable.is_file():raise ActionError('executable_not_found')
        argv=[str(executable),*argv[1:]]
    else:
        source=args[kinds[0]]
        if not isinstance(source,str) or not 1<=len(source)<=256*1024 or '\x00' in source:raise ActionError('invalid_command_source')
        if kinds[0]=='python':
            executable=absolute_path(config.get('python',sys.executable))
            argv=[str(executable),'-c',source]
        else:
            executable=shutil.which('powershell.exe') or shutil.which('pwsh')
            if not executable:raise ActionError('powershell_unavailable')
            # Source is private authorized code, argv data avoids shell escaping.
            argv=[executable,'-NoLogo','-NoProfile','-NonInteractive','-Command',source]
    text=' '.join(argv).lower()
    # These operations have a separate exact-owned-process API. Do not accept
    # broad name/all-process kill commands through the generic command action.
    if re.search(r'\b(?:taskkill|killall|pkill|stop-process|terminateprocess|wmic\s+process)\b',text):raise ActionError('use_owned_stop_process')
    for root in protected_roots(config):
        normalized=str(absolute_path(root)).lower()
        if normalized in text or normalized.replace('\\','/') in text:raise ActionError('protected_path_in_command')
    timeout=args.get('timeoutSeconds',300)
    if type(timeout) not in (int,float) or not 1<=timeout<=86400:raise ActionError('invalid_timeout')
    return argv,str(cwd),float(timeout)

def folder_for(state_root,process_id):
    if not isinstance(process_id,str) or not re.fullmatch(r'[a-f0-9]{32}',process_id):raise ActionError('invalid_process_id')
    return Path(state_root)/'processes'/process_id

def process_status(process_id,state_root,maximum=65536):
    folder=folder_for(state_root,process_id);ticket=runner.load(folder/'ticket.json')
    if not isinstance(ticket,dict) or ticket.get('processId')!=process_id:raise ActionError('process_not_owned')
    status=runner.load(folder/'status.json',{})
    if status and status.get('ownerNonce')!=ticket['ownerNonce']:raise ActionError('process_owner_mismatch')
    launch=runner.load(folder/'launch.json',{})
    if status.get('stage') in ('starting','running') and status.get('runnerBirth') is not None and not runner.alive(status.get('runnerPid'),status.get('runnerBirth')):
        # Owner can atomically publish completion and exit between the first
        # read and kernel liveness probe. Re-read before reporting interruption.
        latest=runner.load(folder/'status.json',{})
        if latest.get('ownerNonce')==ticket['ownerNonce'] and latest.get('stage') not in ('starting','running'):
            status=latest
        else:status={**status,'stage':'interrupted','error':'owner_runner_not_live'}
    if not status:status={'stage':'starting' if runner.alive(launch.get('runnerPid'),launch.get('runnerBirth')) else 'failed_to_start'}
    output={**status,'processId':process_id,'running':status.get('stage') in ('starting','running'),
        'done':status.get('stage') not in ('starting','running'),
        'kernelIdentityAvailable':status.get('runnerBirth') is not None,
        'succeeded':status.get('stage')=='completed' and status.get('returnCode')==0}
    for name in ('stdout','stderr'):
        path=folder/(name+'.log');size=path.stat().st_size if path.is_file() else 0
        if path.is_file():
            with path.open('rb') as source:source.seek(max(0,size-maximum));data=source.read(maximum)
        else:data=b''
        output[name]=redact(data.decode('utf-8',errors='replace'));output[name+'Truncated']=size>maximum
        output[name+'Path']=str(path.resolve())
    output.pop('ownerNonce',None)
    owned=OWNED_RUNNERS.get(process_id)
    if output['done'] and owned is not None and owned.poll() is not None:
        owned.wait();OWNED_RUNNERS.pop(process_id,None)
    return output

def start_process(args,config,state_root):
    argv,cwd,timeout=command(args,config)
    identity=uuid.uuid4().hex;folder=folder_for(state_root,identity);folder.mkdir(parents=True)
    ticket={'processId':identity,'ownerNonce':secrets.token_hex(32),'argv':argv,'cwd':cwd,'timeoutSeconds':timeout,'createdAt':time.time()}
    runner.atomic(folder/'ticket.json',ticket)
    python=absolute_path(config.get('python',sys.executable))
    script=Path(__file__).with_name('process_runner.py').resolve()
    child=subprocess.Popen([str(python),str(script),str(folder)],cwd=str(script.parent),
        stdin=subprocess.DEVNULL,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,
        creationflags=(0x00000008|0x00000200) if os.name=='nt' else 0,start_new_session=os.name!='nt')
    runner.atomic(folder/'launch.json',{'runnerPid':child.pid,'runnerBirth':runner.birth(child.pid),'ownerNonce':ticket['ownerNonce']})
    OWNED_RUNNERS[identity]=child
    # Keep handle bookkeeping local; the detached runner persists after caller loss.
    # Do not wait here: this detached owner persists without the caller.
    for _ in range(20):
        if (folder/'status.json').exists():break
        time.sleep(.025)
    return process_status(identity,state_root)

def stop_process(process_id,state_root):
    folder=folder_for(state_root,process_id);status=process_status(process_id,state_root)
    ticket=runner.load(folder/'ticket.json');launch=runner.load(folder/'launch.json',{})
    private_status=runner.load(folder/'status.json',{})
    pid=private_status.get('runnerPid',launch.get('runnerPid'));born=private_status.get('runnerBirth',launch.get('runnerBirth'))
    if private_status.get('ownerNonce',launch.get('ownerNonce'))!=ticket.get('ownerNonce'):raise ActionError('process_owner_mismatch')
    if private_status.get('stage') in ('starting','running') and not runner.alive(pid,born):raise ActionError('process_owner_not_live')
    if status['stage'] not in ('starting','running'):return {**status,'stopRequested':False}
    if not runner.alive(pid,born):raise ActionError('process_owner_not_live')
    # Only the exact owner receives a flag; no remote PID or process-name kills.
    (folder/'stop.flag').write_text('stop requested by authenticated private action',encoding='utf-8')
    return {**status,'stopRequested':True}

def perform(job,config,state_root):
    action=job.get('action');args=job.get('args',{})
    if action not in ACTIONS or not isinstance(args,dict):raise ActionError('unsupported_action')
    state_root=Path(state_root)
    if action=='capabilities':
        if args:raise ActionError('unexpected_args')
        return {'actions':sorted(ACTIONS),'channelRequired':'authenticated_private','maxFileBytes':MAX_BYTES,
            'uiHelperConfigured':bool(config.get('uiHelper')),'platform':sys.platform}
    if action=='read_file':
        if set(args)-{'path','maxBytes','offsetBytes'}:raise ActionError('unknown_read_field')
        path=absolute_path(args.get('path'));offset=args.get('offsetBytes',0)
        data,size,whole_hash=read_chunk(path,args.get('maxBytes',MAX_BYTES),offset)
        result={'path':str(path),'bytes':len(data),'sha256':whole_hash,'chunkSha256':sha(data),
            'totalBytes':size,'offsetBytes':offset,'eof':offset+len(data)>=size}
        try:result.update(encoding='utf-8',contentUtf8=redact(data.decode('utf-8-sig')))
        except UnicodeError:result.update(encoding='base64',contentBase64=base64.b64encode(data).decode())
        return result
    if action=='write_file':
        if set(args)-{'path','expectedSha256','contentUtf8','contentBase64'} or 'expectedSha256' not in args:raise ActionError('write_precondition_required')
        path=absolute_path(args.get('path'))
        if protected(path,config):raise ActionError('protected_write_path')
        expected=args['expectedSha256']
        if expected is not None and (not isinstance(expected,str) or not HASH.fullmatch(expected)):raise ActionError('invalid_expected_hash')
        kinds=[k for k in ('contentUtf8','contentBase64') if k in args]
        if len(kinds)!=1 or not isinstance(args[kinds[0]],str):raise ActionError('one_content_kind_required')
        try:data=args[kinds[0]].encode('utf-8') if kinds[0]=='contentUtf8' else base64.b64decode(args[kinds[0]],validate=True)
        except (ValueError,UnicodeError):raise ActionError('invalid_content')
        if len(data)>MAX_BYTES:raise ActionError('file_too_large')
        identity=sha(str(path).encode());lock=state_root/'file_locks'/(identity+'.lock')
        with file_lock(lock):
            if path.exists() and not path.is_file():raise ActionError('not_a_regular_file')
            current=file_sha(path)
            if current!=expected:raise ActionError('file_sha_conflict')
            backup=None
            if path.is_file():
                backup=state_root/'file_backups'/identity/(str(time.time_ns())+'_'+current+'.bak')
                backup.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(path,backup)
                runner.atomic(backup.with_suffix('.json'),{'path':str(path),'sha256':current,'createdAt':time.time()})
            path.parent.mkdir(parents=True,exist_ok=True)
            temporary=path.with_name(path.name+'.bridge-'+secrets.token_hex(8)+'.tmp')
            try:
                with temporary.open('xb') as out:out.write(data);out.flush();os.fsync(out.fileno())
                if protected(path.resolve(),config):raise ActionError('protected_write_path')
                if file_sha(path)!=current:raise ActionError('file_changed_during_write')
                os.replace(temporary,path)
            finally:
                if temporary.exists():temporary.unlink()
        return {'path':str(path),'sha256':sha(data),'bytes':len(data),'backup':str(backup) if backup else None}
    if action=='list_dir':
        if set(args)-{'path','limit'}:raise ActionError('unknown_list_field')
        path=absolute_path(args.get('path'));limit=args.get('limit',1000)
        if type(limit) is not int or not 1<=limit<=5000:raise ActionError('invalid_limit')
        if not path.is_dir():raise ActionError('directory_not_found')
        entries=sorted(path.iterdir(),key=lambda p:p.name.lower());rows=[]
        for p in entries[:limit]:
            try:rows.append({'name':p.name,'directory':p.is_dir(),'bytes':p.stat().st_size if p.is_file() else None,'symlink':p.is_symlink()})
            except OSError:rows.append({'name':p.name,'unreadable':True})
        return {'path':str(path),'entries':rows,'truncated':len(entries)>limit}
    if action in ('start_process','run_command'):
        result=start_process(args,config,state_root)
        if action=='start_process':return result
        # Detached runner startup and completion publication are separate from
        # its child runtime timeout; account for Windows executable scanning.
        deadline=time.monotonic()+args.get('timeoutSeconds',300)+45
        while result['stage'] in ('starting','running') and time.monotonic()<deadline:
            time.sleep(.15);result=process_status(result['processId'],state_root)
        return result
    if action in ('process_status','stop_process'):
        if set(args)!={'processId'}:raise ActionError('invalid_process_args')
        return process_status(args['processId'],state_root) if action=='process_status' else stop_process(args['processId'],state_root)
    if action=='ui_control':
        helper=config.get('uiHelper')
        argv=[helper] if isinstance(helper,str) else helper
        if not isinstance(argv,list) or not argv or any(not isinstance(v,str) for v in argv):raise ActionError('ui_helper_not_configured')
        absolute_path(argv[0])
        if set(args)!={'action','args'} or not isinstance(args.get('args'),dict):raise ActionError('invalid_ui_request_schema')
        if args.get('action') not in ('screen_capture','window_list','mouse_click','mouse_scroll',
                'type_text','send_keys','focus_window'):raise ActionError('unsupported_ui_action')
        raw=json.dumps(args,ensure_ascii=False)
        if len(raw.encode())>32768:raise ActionError('ui_args_too_large')
        completed=subprocess.run(argv,input=raw,text=True,encoding='utf-8',errors='replace',capture_output=True,
            timeout=30,creationflags=0x08000000 if os.name=='nt' else 0)
        if len(completed.stdout.encode())>8*1024*1024:raise ActionError('ui_result_too_large')
        try:
            result=json.loads(completed.stdout)
            if not isinstance(result,dict) or type(result.get('ok')) is not bool:raise ActionError('ui_result_invalid')
            if completed.returncode and result['ok']:raise ActionError('ui_helper_inconsistent_result')
            return result
        except ValueError:raise ActionError('ui_result_invalid')
    raise ActionError('unsupported_action')
