"""Project Bridge 2.0: public audiobook operations plus a private PC relay.

General commands, files and screen results use the private channel. No model API
is called. Credentials stay in the local gh/git credential manager and memory.
"""
import argparse
import base64
import contextlib
import datetime as dt
import hashlib
import http.server
import json
import os
from pathlib import Path, PurePosixPath
import re
import shutil
import secrets
import subprocess
import sys
import time
import threading
import urllib.error
import urllib.parse
import urllib.request

VERSION = '2.0.0'
HUB = 'kimjae134679/project-operations-hub'
PROJECT = 'Mushoku-Audiobook'
QUEUE_PATH = '04_COMMUNICATION/remote-bridge/queue.json'
RESULT_BASE = '04_COMMUNICATION/remote-bridge/results'
ACTIONS = frozenset(('status', 'refresh_listening_index', 'organize_listening',
    'build_listener', 'run_static_qa', 'locate_reference_inventory', 'apply_reviewed_bundle', 'audio_reference_audit',
    'render_voice_repair_previews'))
# Files reviewed for this narrowly scoped listening/QA deployment. Production
# worker/runtime/cast/source/model/config files cannot be replaced by this relay.
CODE_ALLOWLIST = frozenset(('.gitignore', 'src/user_library.py', 'src/project_paths.py',
    'src/build_all_audio_catalog.py', 'src/production_queue.py', 'src/audio_reference_audit.py',
    'src/render_voice_repair_previews.py',
    'ui/listening_library.html', 'ui/application_shell.html',
    'ui/audiobook_reader.html', 'ui/production_library.html', 'ui/audio_catalog.html',
    'app/AudiobookLauncher.cs', 'tests/test_user_library.py', 'tests/test_audio_reference_audit.py',
    'tests/test_voice_repair_previews.py',
    'docs/USER_LISTENING_LAYOUT.md', '00_START_HERE/CURRENT_HANDOFF.md',
    'docs/AUDIO_QA_20261006.md', 'docs/REMOTE_PROJECT_BRIDGE.md',
    'docs/SOURCE_PRODUCTION_EDITION.md', '프로젝트_사용안내.md'))
JOB_ID = re.compile(r'^[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}$')
HASH = re.compile(r'^[a-f0-9]{64}$')
KNOWN_ROLES = ('루데우스', '루데우스 내면', '나레이터', '올스테드', '에리스',
    '실피', '록시', '나나호시', '실바릴', '자노바', '페르기우스', '루이젤드',
    '아리엘', '엘리나리제', '엘리나리자', '길레느', '리랴', '아이샤')

class BridgeError(Exception):
    """Message is a bounded public-safe code, never a subprocess body or token."""

def now(): return dt.datetime.now(dt.timezone.utc).isoformat()

def digest(data): return hashlib.sha256(data).hexdigest()

def load(path, default=None):
    try: return json.loads(Path(path).read_text(encoding='utf-8-sig'))
    except (OSError, ValueError): return default

def atomic(path, value):
    path = Path(path); path.parent.mkdir(parents=True, exist_ok=True)
    tmp = path.with_name(path.name + '.tmp')
    tmp.write_text(json.dumps(value, ensure_ascii=False, indent=2), encoding='utf-8')
    os.replace(tmp, path)

def relative_path(value):
    if not isinstance(value, str) or '\\' in value or ':' in value or '\x00' in value:
        raise BridgeError('invalid_relative_path')
    path = PurePosixPath(value)
    if path.is_absolute() or '..' in path.parts or not path.parts or str(path) != value:
        raise BridgeError('invalid_relative_path')
    return path

def inside(root, relative):
    relative_path(relative)
    root = Path(root).resolve(); path = (root / relative).resolve()
    if not path.is_relative_to(root): raise BridgeError('path_outside_root')
    # Existing parents must not be Windows junctions/symlinks into another tree.
    for parent in [path, *path.parents]:
        if parent == root: break
        if parent.is_symlink() or (hasattr(parent, 'is_junction') and parent.is_junction()):
            raise BridgeError('reparse_path_refused')
    return path

def parse_time(value):
    try:
        result = dt.datetime.fromisoformat(value.replace('Z', '+00:00'))
        if result.tzinfo is None: raise ValueError()
        return result
    except (AttributeError, ValueError): raise BridgeError('invalid_timestamp')

def validate_job(job, at=None):
    if not isinstance(job, dict) or set(job) not in (
            {'id', 'target', 'action', 'createdAt', 'expiresAt', 'args'},
            {'id', 'target', 'action', 'createdAt', 'expiresAt', 'args', 'dependsOn'}):
        raise BridgeError('invalid_job_schema')
    if not isinstance(job['id'], str) or not JOB_ID.fullmatch(job['id']): raise BridgeError('invalid_job_id')
    if job['target'] != PROJECT or job['action'] not in ACTIONS: raise BridgeError('unsupported_job')
    at = at or dt.datetime.now(dt.timezone.utc)
    created, expiry = parse_time(job['createdAt']), parse_time(job['expiresAt'])
    if created > at + dt.timedelta(minutes=5) or expiry <= at or expiry <= created:
        raise BridgeError('expired_or_future_job')
    if expiry - created > dt.timedelta(days=14): raise BridgeError('job_lifetime_too_long')
    deps = job.get('dependsOn', [])
    if not isinstance(deps, list) or len(deps) > 20 or any(
            not isinstance(i,str) or not JOB_ID.fullmatch(i) or i == job['id'] for i in deps) or len(deps) != len(set(deps)):
        raise BridgeError('invalid_job_dependencies')
    args = job['args']
    if not isinstance(args, dict): raise BridgeError('invalid_args')
    if job['action'] == 'apply_reviewed_bundle':
        if set(args) != {'bundlePath', 'bundleSha256'}: raise BridgeError('invalid_bundle_args')
        path = str(relative_path(args['bundlePath']))
        if not path.startswith('04_COMMUNICATION/remote-bridge/bundles/') or not path.endswith('.json'):
            raise BridgeError('bundle_path_refused')
        if not isinstance(args['bundleSha256'], str) or not HASH.fullmatch(args['bundleSha256']):
            raise BridgeError('invalid_bundle_hash')
    elif args: raise BridgeError('unexpected_args')
    return job

def job_hash(job):
    return digest(json.dumps(job, sort_keys=True, ensure_ascii=False, separators=(',', ':')).encode())

def quiet_run(args, cwd=None, input_text=None, timeout=120):
    try:
        environment = dict(os.environ, GIT_TERMINAL_PROMPT='0', GCM_INTERACTIVE='never', PYTHONIOENCODING='utf-8')
        return subprocess.run(args, cwd=cwd, input=input_text, text=True,
            encoding='utf-8', errors='replace', stdout=subprocess.PIPE, stderr=subprocess.PIPE,
            timeout=timeout, env=environment, creationflags=0x08000000 if os.name == 'nt' else 0)
    except (OSError, subprocess.TimeoutExpired): raise BridgeError('local_command_unavailable_or_timed_out')

def github_token():
    # Never persist the token or return stderr to UI/results.
    if shutil.which('gh'):
        result = quiet_run(['gh', 'auth', 'token', '--hostname', 'github.com'], timeout=15)
        if result.returncode == 0 and result.stdout.strip(): return result.stdout.strip()
    if shutil.which('git'):
        result = quiet_run(['git', 'credential', 'fill'],
            input_text='protocol=https\nhost=github.com\n\n', timeout=20)
        if result.returncode == 0:
            fields = dict(line.split('=', 1) for line in result.stdout.splitlines() if '=' in line)
            if fields.get('password'): return fields['password']
    raise BridgeError('github_login_required')

class GitHub:
    def __init__(self, repository=HUB, branch=None):
        if not re.fullmatch(r'kimjae134679/[A-Za-z0-9_.-]+',repository): raise BridgeError('unsupported_repository')
        self.repository=repository; self.token = None; self.branch = branch; self.cache = {}; self.auth_checked = False
    def request(self, method, path, value=None, authenticated=False):
        if not self.token and (not self.auth_checked or authenticated):
            try: self.token = github_token()
            except BridgeError:
                if authenticated: raise
            self.auth_checked = True
        headers = {'Accept': 'application/vnd.github+json', 'User-Agent': 'ProjectBridge/'+VERSION,
            'X-GitHub-Api-Version': '2022-11-28'}
        if self.token: headers['Authorization'] = 'Bearer ' + self.token
        if method == 'GET' and path in self.cache: headers['If-None-Match'] = self.cache[path][0]
        body = None if value is None else json.dumps(value).encode()
        if body: headers['Content-Type'] = 'application/json'
        req = urllib.request.Request('https://api.github.com/repos/'+self.repository+'/'+path,
            data=body, method=method, headers=headers)
        try:
            with urllib.request.urlopen(req, timeout=30) as response:
                data = response.read(12 * 1024 * 1024 + 1)
                etag = response.headers.get('ETag')
            if len(data) > 12 * 1024 * 1024: raise BridgeError('github_response_too_large')
            parsed = json.loads(data)
            if method == 'GET' and etag: self.cache[path] = (etag, parsed)
            return parsed
        except urllib.error.HTTPError as e:
            if e.code == 304 and path in self.cache: return self.cache[path][1]
            if e.code == 401:
                self.token = None; self.auth_checked = False
            raise BridgeError('github_http_' + str(e.code))
        except (urllib.error.URLError, ValueError, TimeoutError): raise BridgeError('github_network_error')
    def default_branch(self):
        if self.branch is None:
            self.branch = self.request('GET', '')['default_branch']
        return self.branch
    def read_bytes(self, path):
        relative_path(path)
        row = self.request('GET', 'contents/'+urllib.parse.quote(path, safe='/')+'?ref='+
            urllib.parse.quote(self.default_branch(), safe=''))
        if row.get('encoding') != 'base64' or row.get('type') != 'file': raise BridgeError('not_github_file')
        try: return base64.b64decode(row['content'], validate=False)
        except (KeyError, ValueError): raise BridgeError('invalid_github_file')
    def publish(self, path, value):
        content = json.dumps(value, ensure_ascii=False, sort_keys=True, indent=2).encode()
        try:
            old = self.read_bytes(path)
            if digest(old) == digest(content): return
            # Results are append-only; no unbounded remote overwrite.
            raise BridgeError('remote_result_conflict')
        except BridgeError as e:
            if str(e) != 'github_http_404': raise
        self.request('PUT', 'contents/'+urllib.parse.quote(path, safe='/'),
            {'message':'Project Bridge sanitized result '+str(value.get('jobId',value.get('recordId',PROJECT))),
             'content':base64.b64encode(content).decode(), 'branch':self.default_branch()}, authenticated=True)
    def upsert(self, path, value):
        relative_path(path)
        content=json.dumps(value,ensure_ascii=False,sort_keys=True,indent=2).encode()
        api='contents/'+urllib.parse.quote(path,safe='/')
        current=None
        try: current=self.request('GET',api+'?ref='+urllib.parse.quote(self.default_branch(),safe=''))
        except BridgeError as e:
            if str(e)!='github_http_404': raise
        payload={'message':'Project Bridge device status','content':base64.b64encode(content).decode(),'branch':self.default_branch()}
        if current: payload['sha']=current['sha']
        self.request('PUT',api,payload,authenticated=True)

def safe_stage(value):
    return value if value in ('unknown', 'idle', 'paused', 'preparing', 'validating', 'generating',
        'checking', 'publishing', 'completed', 'complete', 'complete_with_holds', 'failed',
        'needs_review', 'ready', 'waiting', 'watching') else 'other'

def chapter_number(value):
    return int(value) if isinstance(value, (int, str)) and re.fullmatch(r'\d{1,7}', str(value)) else None

def status_snapshot(root):
    pointer = load(root / 'output/production_active.json', {})
    try: run = inside(root, pointer.get('run_relative', 'output/production_15_24'))
    except BridgeError: raise BridgeError('invalid_active_edition')
    state = load(run / 'status.json', {})
    campaign = load(run / 'campaign.json', {})
    chapters = state.get('chapters', {})
    ready = sorted(n for n in (chapter_number(k) for k, row in chapters.items()
        if isinstance(row, dict) and row.get('status') == 'ready') if n is not None)
    shelf = load(root / 'output/user_library/last_refresh.json', {})
    progress = load(run / 'worker_progress.json', {})
    updated = progress.get('updated')
    return {'stage':safe_stage(state.get('stage')), 'currentChapter':chapter_number(state.get('current')),
        'selectedChapters':len(campaign.get('chapters', [])), 'readyChapterNumbers':ready,
        'readyChapters':len(ready), 'needsReviewChapters':sum(isinstance(r, dict) and r.get('status') == 'needs_review' for r in chapters.values()),
        'paused':(run / 'pause.flag').exists(), 'lastProgressAt':updated if isinstance(updated, (int,float)) else None,
        'listeningShelfReady':shelf.get('ready') if isinstance(shelf.get('ready'), int) else None,
        'shelfVerificationIssueCount':len(shelf.get('verification_issues', []))}

def reference_inventory(root):
    locations = ('voice_resource_pack/rebuild', 'voices', 'voices_clean', 'voice_candidates',
        'comparison/01_CURRENT', 'reference_lab', 'anime_sources')
    rows = []
    for relative in locations:
        path = inside(root, relative); counts = {'.wav':0, '.mp3':0, '.mp4':0, '.mkv':0, '.json':0}
        if path.exists():
            for file in path.rglob('*'):
                if file.is_file() and file.suffix.lower() in counts: counts[file.suffix.lower()] += 1
        rows.append({'location':relative, 'counts':counts})
    return {'locations':rows, 'note':'Counts only. No media, transcripts, source text or unrestricted filenames uploaded.'}

def validate_bundle(raw, approved, root):
    sha = digest(raw)
    if sha not in approved: raise BridgeError('bundle_not_locally_approved')
    try: bundle = json.loads(raw)
    except ValueError: raise BridgeError('invalid_bundle_json')
    if not isinstance(bundle, dict) or set(bundle) != {'schemaVersion', 'project', 'files'} or bundle['schemaVersion'] != 1 or bundle['project'] != PROJECT:
        raise BridgeError('invalid_bundle_schema')
    if not isinstance(bundle['files'], list) or not 1 <= len(bundle['files']) <= len(CODE_ALLOWLIST):
        raise BridgeError('invalid_bundle_files')
    checked = []; seen = set()
    for row in bundle['files']:
        if not isinstance(row, dict) or set(row) != {'path', 'baseSha256', 'newSha256', 'contentBase64'}:
            raise BridgeError('invalid_bundle_file_schema')
        path = row['path']
        if path not in CODE_ALLOWLIST or path in seen: raise BridgeError('code_path_not_allowlisted')
        seen.add(path); destination = inside(root, path)
        if row['baseSha256'] is not None and (not isinstance(row['baseSha256'], str) or not HASH.fullmatch(row['baseSha256'])):
            raise BridgeError('invalid_base_hash')
        try: content = base64.b64decode(row['contentBase64'], validate=True)
        except (ValueError, TypeError): raise BridgeError('invalid_bundle_content')
        if len(content) > 1024*1024 or not HASH.fullmatch(str(row['newSha256'])) or digest(content) != row['newSha256']:
            raise BridgeError('invalid_new_hash_or_size')
        try: content.decode('utf-8-sig')
        except UnicodeError: raise BridgeError('non_utf8_code_refused')
        existing = digest(destination.read_bytes()) if destination.is_file() else None
        if existing not in (row['baseSha256'], row['newSha256']): raise BridgeError('local_file_changed')
        checked.append((destination, row, content, existing))
    return sha, checked

def apply_bundle(raw, approved, root, state_root):
    sha, checked = validate_bundle(raw, approved, root)
    backup = state_root / 'backups' / sha
    manifest = []
    # Validate every file before mutating any, then retain preimages and intent.
    for path, row, content, existing in checked:
        if existing == row['newSha256']: continue
        if existing is not None:
            saved = backup / row['path']; saved.parent.mkdir(parents=True, exist_ok=True)
            if not saved.exists(): shutil.copy2(path, saved)
        manifest.append({'path':row['path'], 'before':existing, 'after':row['newSha256']})
    prior = load(backup / 'manifest.json', {})
    retained = {r['path']:r for r in prior.get('files', [])}
    for r in manifest: retained.setdefault(r['path'], r)
    atomic(backup / 'manifest.json', {'bundleSha256':sha, 'files':list(retained.values()),
        'startedAt':prior.get('startedAt', now())})
    for path, row, content, existing in checked:
        if existing == row['newSha256']: continue
        # Recheck immediately before replace: local edits are never silently lost.
        if (digest(path.read_bytes()) if path.is_file() else None) != existing:
            raise BridgeError('local_file_changed_during_apply')
        path.parent.mkdir(parents=True, exist_ok=True)
        tmp = path.with_name(path.name + '.bridge-tmp'); tmp.write_bytes(content)
        os.replace(tmp, path)
    return {'bundleSha256':sha, 'changedFiles':len(manifest), 'backupRetained':True,
        'productionSettingsChanged':False}

def perform(job, config, state_root, client):
    root = Path(config['projectRoot']).resolve()
    action = job['action']
    if action == 'status': return status_snapshot(root)
    if action == 'locate_reference_inventory': return reference_inventory(root)
    if action == 'apply_reviewed_bundle':
        raw = client.read_bytes(job['args']['bundlePath'])
        if digest(raw) != job['args']['bundleSha256']: raise BridgeError('downloaded_bundle_hash_mismatch')
        return apply_bundle(raw, config.get('approvedBundleSha256', []), root, state_root)
    python = config['python']; src = root / 'src'
    def run_local(args, timeout):
        result = quiet_run(args, root, timeout=timeout)
        folder = state_root / 'jobs' / job['id']; folder.mkdir(parents=True, exist_ok=True)
        # These files are private and never included in operational relay results.
        # Truncate large progress logs and mask common token encodings.
        for name, body in [('stdout.log',result.stdout),('stderr.log',result.stderr)]:
            body = re.sub(r'\b(?:gh[pousr]_[A-Za-z0-9_]{20,}|github_pat_[A-Za-z0-9_]{20,})\b','[credential redacted]',body)
            (folder / name).write_text(body[-2*1024*1024:], encoding='utf-8')
        return result
    if action in ('refresh_listening_index', 'organize_listening'):
        script = src / 'user_library.py'
        if not script.is_file(): raise BridgeError('listening_update_not_installed')
        result = run_local([python, str(script)] + (['--organize'] if action == 'organize_listening' else []), timeout=300)
        if result.returncode: raise BridgeError('listening_refresh_failed')
        try: refreshed = json.loads(result.stdout.strip().splitlines()[-1])
        except (ValueError, IndexError): raise BridgeError('listening_refresh_result_invalid')
        if refreshed.get('busy'): raise BridgeError('listening_refresh_busy_requeue')
        if action == 'organize_listening' and (not refreshed.get('organization',{}).get('applied') or not refreshed.get('shortcuts',{}).get('applied')):
            raise BridgeError('listening_organization_partial')
        return {'refreshSucceeded':True, 'organized':action == 'organize_listening', 'production':status_snapshot(root)}
    if action == 'run_static_qa':
        test = root / 'tests/test_user_library.py'
        if not test.is_file(): raise BridgeError('qa_test_not_installed')
        count = 0
        for pattern in ('test_user_library.py','test_audio_reference_audit.py','test_voice_repair_previews.py'):
            if not (root / 'tests' / pattern).is_file(): continue
            result = run_local([python, '-m', 'unittest', 'discover', '-s', 'tests', '-p', pattern], timeout=180)
            if result.returncode: raise BridgeError('static_qa_failed')
            ran = re.search(r'Ran (\d+) tests?', result.stderr)
            if not ran or int(ran.group(1)) < 1: raise BridgeError('static_qa_no_tests_ran')
            count += int(ran.group(1))
        return {'passed':True, 'testsPassed':count, 'testSet':'listening_and_reference_audit'}
    if action in ('audio_reference_audit','render_voice_repair_previews'):
        script = root / 'src' / (action+'.py')
        if not script.is_file(): raise BridgeError('audio_reference_audit_not_installed')
        result = run_local([python, str(script)], timeout=7800 if action=='render_voice_repair_previews' else 600)
        if result.returncode: raise BridgeError('audio_operation_failed')
        try: report=json.loads(result.stdout.strip().splitlines()[-1])
        except (ValueError, IndexError): raise BridgeError('audio_operation_result_invalid')
        public = {k:v for k,v in report.items() if k in ('roles_audited','roles_rendered','clips_generated',
            'voice_changes_applied','reference_candidates','source_files_scanned','preserved_roles',
            'generatedPreviews','heldRoles','liveCastChanged','perugiusPreserved','cast_unchanged_verified')
            and (type(v) in (int,bool) or isinstance(v,list) and all(r in KNOWN_ROLES for r in v))}
        for key in ('candidate_counts','verified_candidate_counts'):
            counts=report.get(key)
            if isinstance(counts,dict) and all(k in KNOWN_ROLES and type(v) is int and 0<=v<=1000000 for k,v in counts.items()):
                public[key]=counts
        return {'auditCompleted':True, 'findingsRemainLocal':True, 'summary':public}
    if action == 'build_listener':
        source = root / 'app/AudiobookLauncher.cs'
        if not source.is_file(): raise BridgeError('listener_source_missing')
        csc = find_csc()
        temporary = state_root / 'build/무직전생.exe'; temporary.parent.mkdir(parents=True, exist_ok=True)
        result = run_local([str(csc), '/nologo', '/target:winexe', '/reference:System.Windows.Forms.dll',
            '/reference:System.Drawing.dll', '/reference:System.Web.Extensions.dll', '/out:'+str(temporary), str(source)], timeout=120)
        if result.returncode or not temporary.is_file(): raise BridgeError('listener_build_failed')
        target = root / '무직전생.exe'
        if target.exists():
            saved = state_root / 'backups/launcher' / (str(time.time_ns())+'.exe'); saved.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(target, saved)
        try: os.replace(temporary, target)
        except OSError: raise BridgeError('listener_in_use_close_and_requeue')
        return {'built':True, 'launcherSha256':digest(target.read_bytes()), 'backupRetained':True}
    raise BridgeError('unsupported_job')

def find_csc():
    windows = Path(os.environ.get('WINDIR', r'C:\Windows'))
    for relative in ('Microsoft.NET/Framework64/v4.0.30319/csc.exe', 'Microsoft.NET/Framework/v4.0.30319/csc.exe'):
        p = windows / relative
        if p.is_file(): return p
    raise BridgeError('dotnet_framework_compiler_missing')

class Worker:
    def __init__(self, home, client=None):
        self.home = Path(home); self.state = self.home / 'state'; self.client = client or GitHub()
        self.config = load(self.home / 'config.json', {})
        self.device = self.config.get('deviceId', '')
        if not JOB_ID.fullmatch(self.device): raise BridgeError('invalid_device_id')
        if self.config.get('hubRepository') != HUB: raise BridgeError('unsupported_hub_repository')
        self.journal_path = self.state / 'journal.json'
        self.journal = load(self.journal_path, {})
    def ui(self, stage, **metadata):
        atomic(self.state / 'ui_status.json', {'bridgeVersion':VERSION, 'stage':stage, 'updatedAt':now(),
            'device':self.device, 'workerPid':os.getpid(), **metadata})
    def checkpoint(self): atomic(self.journal_path, self.journal)
    def result_path(self, job_id): return RESULT_BASE+'/'+self.device+'/'+job_id+'.json'
    def publish_record(self, job_id, record):
        self.client.publish(self.result_path(job_id), record['result'])
        # Canonical task_exchange lets the existing communication view read
        # operational responses, in addition to the dedicated result directory.
        result=record['result']; action=record.get('action','status')
        finished=result['finishedAt']; ok=result['outcome']=='completed'
        record_id='bridge-'+job_id
        names={'status':'제작 상태 확인','refresh_listening_index':'듣기 목록 갱신',
            'organize_listening':'듣기 폴더 정리','build_listener':'청취 실행기 빌드',
            'run_static_qa':'청취 QA 검사','locate_reference_inventory':'표본 자료 개수 확인',
            'apply_reviewed_bundle':'검토 코드 적용','audio_reference_audit':'원본 표본 점검',
            'render_voice_repair_previews':'음색 수정 비교 생성'}
        title=names.get(action,'프로젝트 작업')
        item={'schemaVersion':1,'recordType':'task_exchange','recordId':record_id,'revision':1,
            'title':title,'projectId':PROJECT,'actorId':'ProjectBridge/'+self.device,
            'sessionId':self.device,'receivedAt':record.get('createdAt',record.get('startedAt',finished)),
            'updatedAt':finished,'request':{'summary':title,'details':'사용자가 요청한 프로젝트 전용 작업 큐',
                'source':QUEUE_PATH},'response':{'summary':title+(' 완료' if ok else ' 확인 필요'),
                'details':json.dumps(result['data'],ensure_ascii=False,sort_keys=True),
                'source':'project_bridge_operational_result'},'status':'completed' if ok else 'blocked',
            'workDone':[title] if ok else [],'verification':[{'name':'로컬 작업 결과',
                'result':'pass' if ok else 'fail','evidence':[self.result_path(job_id)]}],
            'nextActions':[] if ok else ['로컬 기록 확인 후 새 작업 ID로 요청'],
            'blockers':[] if ok else [result['data'].get('code','operation_incomplete')],
            'supersedes':[]}
        raw=json.dumps(item,ensure_ascii=False,sort_keys=True,indent=2).encode()
        content_hash=digest(raw); path='04_COMMUNICATION/project-inbox/'+PROJECT+'/'+content_hash
        self.client.publish(path+'/content.json',item)
        actor_hash=digest(json.dumps([PROJECT,item['actorId'],record_id],ensure_ascii=False).encode())
        metadata={'projectId':PROJECT,'contentSha256':content_hash,
            'sourceName':'task-'+actor_hash+'-r1.json','centralPath':path+'/content.json',
            'collectedAt':finished}
        self.client.publish(path+'/item.json',metadata)
    def publish_pending(self):
        for job_id, record in list(self.journal.items()):
            if record.get('state') == 'started':
                # A crash is ambiguous. Never replay a mutation automatically.
                record['state'] = 'finished'
                record['result'] = self.result(job_id, record['jobHash'], 'interrupted', {'code':'local_interruption_requires_new_job_id'})
                self.checkpoint()
            if record.get('state') == 'finished':
                self.publish_record(job_id, record)
                record['state'] = 'published'; self.checkpoint()
    def result(self, job_id, hashed, outcome, data):
        return {'schemaVersion':1, 'project':PROJECT, 'device':self.device, 'bridgeVersion':VERSION,
            'jobId':job_id, 'jobSha256':hashed, 'finishedAt':now(), 'outcome':outcome, 'data':data}
    def tick(self):
        if (self.state / 'disconnected.flag').exists(): self.ui('disconnected'); return
        self.publish_pending()
        raw = self.client.read_bytes(QUEUE_PATH)
        try: queue = json.loads(raw)
        except ValueError: raise BridgeError('invalid_queue_json')
        if not isinstance(queue, dict) or set(queue) != {'schemaVersion', 'jobs'} or queue['schemaVersion'] != 1 or not isinstance(queue['jobs'], list) or len(queue['jobs']) > 100:
            raise BridgeError('invalid_queue_schema')
        pending = []
        seen = set()
        for row in queue['jobs']:
            if isinstance(row, dict) and row.get('target') != PROJECT: continue
            try: job = validate_job(row)
            except BridgeError: continue
            if job['id'] in seen: raise BridgeError('duplicate_queue_job_id')
            seen.add(job['id']); hashed = job_hash(job); old = self.journal.get(job['id'])
            if old:
                if old['jobHash'] != hashed: raise BridgeError('job_id_reused_with_different_content')
                continue
            pending.append(job)
        self.ui('connected', pending=[{'id':j['id'], 'action':j['action']} for j in pending], production=status_snapshot(Path(self.config['projectRoot'])))
        for job in pending:
            if (self.state / 'disconnected.flag').exists() or (self.state / 'stop.flag').exists(): break
            deps = [self.journal.get(i) for i in job.get('dependsOn', [])]
            if any(d is None for d in deps): continue
            failed_prerequisite = any(d.get('result',{}).get('outcome') != 'completed' for d in deps)
            hashed = job_hash(job); job_id = job['id']
            self.journal[job_id] = {'jobHash':hashed, 'state':'started', 'startedAt':now(),
                'action':job['action'],'createdAt':job['createdAt']}; self.checkpoint()
            self.ui('running', currentJob=job_id, action=job['action'])
            try:
                if failed_prerequisite: raise BridgeError('prerequisite_failed')
                data = perform(job, self.config, self.state, self.client); outcome = 'completed'
            except BridgeError as e: data = {'code':str(e)}; outcome = 'failed'
            except Exception: data = {'code':'local_operation_failed'}; outcome = 'failed'
            record = self.journal[job_id]; record['result'] = self.result(job_id, hashed, outcome, data)
            record['state'] = 'finished'; self.checkpoint()
            self.publish_record(job_id, record)
            record['state'] = 'published'; self.checkpoint()
        remaining = [{'id':j['id'],'action':j['action']} for j in pending if j['id'] not in self.journal]
        self.ui('connected', pending=remaining, production=status_snapshot(Path(self.config['projectRoot'])))

@contextlib.contextmanager
def process_lock(home):
    path = Path(home) / 'state/worker.lock'; path.parent.mkdir(parents=True, exist_ok=True)
    with path.open('a+b') as lease:
        if lease.seek(0, 2) == 0: lease.write(b'0'); lease.flush()
        lease.seek(0)
        if os.name == 'nt':
            import msvcrt
            try: msvcrt.locking(lease.fileno(), msvcrt.LK_NBLCK, 1)
            except OSError: raise BridgeError('already_running')
        else:
            import fcntl
            try: fcntl.flock(lease, fcntl.LOCK_EX | fcntl.LOCK_NB)
            except OSError: raise BridgeError('already_running')
        try: yield
        finally:
            if os.name == 'nt': lease.seek(0); msvcrt.locking(lease.fileno(), msvcrt.LK_UNLCK, 1)
            else: fcntl.flock(lease, fcntl.LOCK_UN)

FEEDBACK_TAGS = frozenset(('voice', 'emotion', 'speed', 'pronunciation', 'echo',
    'volume', 'continuity', 'source', 'good'))

def validate_feedback(row):
    if not isinstance(row, dict) or set(row) != {'edition','chapter','speaker','score','tags','note'}:
        raise BridgeError('invalid_feedback_schema')
    if not isinstance(row['edition'], str) or not re.fullmatch(r'[a-zA-Z0-9_-]{1,100}', row['edition']):
        raise BridgeError('invalid_feedback_edition')
    if row['chapter'] is not None and (type(row['chapter']) is not int or not 0 < row['chapter'] <= 9999999):
        raise BridgeError('invalid_feedback_chapter')
    if not isinstance(row['speaker'], str) or not 1 <= len(row['speaker']) <= 40 or any(ord(c)<32 for c in row['speaker']):
        raise BridgeError('invalid_feedback_speaker')
    if type(row['score']) is not int or not 1 <= row['score'] <= 10: raise BridgeError('invalid_feedback_score')
    if not isinstance(row['tags'], list) or len(row['tags']) > 9 or any(t not in FEEDBACK_TAGS for t in row['tags']):
        raise BridgeError('invalid_feedback_tags')
    if not isinstance(row['note'], str) or len(row['note']) > 1000: raise BridgeError('invalid_feedback_note')
    return {**row, 'tags':sorted(set(row['tags']))}

class FeedbackServer:
    def __init__(self, root):
        self.root = Path(root); self.token = secrets.token_urlsafe(32)
        self.lock = threading.Lock()
        self.path = self.root / 'output/listening_feedback/events.jsonl'
        self.path.parent.mkdir(parents=True, exist_ok=True)
        owner = self
        class Handler(http.server.BaseHTTPRequestHandler):
            def log_message(self, *args): pass
            def permitted(self, auth=True):
                origin = self.headers.get('Origin')
                base = 'http://127.0.0.1:'+str(owner.server.server_port)
                if origin not in (None, 'null', base): return False
                return not auth or secrets.compare_digest(self.headers.get('X-Bridge-Token',''), owner.token)
            def answer(self, code, data):
                raw = json.dumps(data, ensure_ascii=False).encode()
                self.send_response(code)
                origin = self.headers.get('Origin')
                if origin: self.send_header('Access-Control-Allow-Origin', origin)
                self.send_header('Vary', 'Origin')
                self.send_header('Content-Type','application/json; charset=utf-8')
                self.send_header('Cache-Control','no-store')
                self.send_header('Content-Length', str(len(raw))); self.end_headers(); self.wfile.write(raw)
            def do_OPTIONS(self):
                if self.path != '/feedback' or not self.permitted(False): self.answer(403, {'error':'forbidden'}); return
                self.send_response(204)
                self.send_header('Access-Control-Allow-Origin', self.headers.get('Origin','null'))
                self.send_header('Access-Control-Allow-Methods','GET, POST, OPTIONS')
                self.send_header('Access-Control-Allow-Headers','X-Bridge-Token, Content-Type')
                self.send_header('Access-Control-Allow-Private-Network','true')
                self.send_header('Vary','Origin'); self.end_headers()
            def do_GET(self):
                if self.path != '/feedback' or not self.permitted(): self.answer(403, {'error':'forbidden'}); return
                with owner.lock: rows = owner.read_rows()
                self.answer(200, {'records':rows})
            def do_POST(self):
                if self.path != '/feedback' or not self.permitted(): self.answer(403, {'error':'forbidden'}); return
                try:
                    length = int(self.headers.get('Content-Length', '0'))
                    if not 0 < length <= 16384: raise BridgeError('feedback_size_refused')
                    if self.headers.get('Content-Type','').split(';')[0] != 'application/json': raise BridgeError('json_required')
                    payload = validate_feedback(json.loads(self.rfile.read(length)))
                    with owner.lock:
                        key = digest(json.dumps(payload, ensure_ascii=False, sort_keys=True).encode())
                        old = next((r for r in reversed(owner.read_rows()) if r.get('payloadHash') == key), None)
                        if old: record = old
                        else:
                            record = {**payload, 'id':secrets.token_hex(12), 'updatedAt':now(), 'payloadHash':key}
                            with owner.path.open('a', encoding='utf-8') as out:
                                out.write(json.dumps(record, ensure_ascii=False)+'\n'); out.flush(); os.fsync(out.fileno())
                    self.answer(200, {'record':record})
                except (ValueError, UnicodeError, BridgeError): self.answer(400, {'error':'invalid_feedback'})
        self.server = http.server.ThreadingHTTPServer(('127.0.0.1',0), Handler)
        self.server.daemon_threads = True
        base = 'http://127.0.0.1:'+str(self.server.server_port)
        private_js = self.root / 'output/user_library/feedback_bridge.js'
        private_js.parent.mkdir(parents=True, exist_ok=True)
        tmp = private_js.with_suffix('.tmp')
        tmp.write_text('window.ListeningFeedbackBridge='+json.dumps({'baseUrl':base,'token':self.token})+';', encoding='utf-8')
        os.replace(tmp, private_js)
        self.thread = threading.Thread(target=self.server.serve_forever, daemon=True); self.thread.start()
    def read_rows(self):
        # Bounded read, no arbitrary local file API and no public result forwarding.
        if not self.path.is_file(): return []
        with self.path.open('rb') as source:
            source.seek(0,2); source.seek(max(0, source.tell()-8*1024*1024))
            data = source.read().decode('utf-8', errors='replace')
        rows = []
        for line in data.splitlines():
            try: row = json.loads(line)
            except ValueError: continue
            if isinstance(row, dict): rows.append(row)
        return rows[-2000:]
    def close(self): self.server.shutdown(); self.server.server_close()

def main():
    parser = argparse.ArgumentParser(); parser.add_argument('--home', required=True); parser.add_argument('--once', action='store_true')
    args = parser.parse_args(); home = Path(args.home)
    try:
        with process_lock(home):
            worker = Worker(home); interval = max(60, min(600, int(worker.config.get('pollSeconds', 90))))
            audio_enabled=bool(worker.config.get('audioEnabled',True)) and Path(worker.config['projectRoot']).is_dir()
            feedback = FeedbackServer(worker.config['projectRoot']) if audio_enabled else None
            general=None
            if worker.config.get('privateRepository'):
                from universal_worker import UniversalWorker
                private=GitHub(worker.config['privateRepository'],worker.config.get('privateBranch','remote/pc-bridge'))
                general=UniversalWorker(home,private,GitHub())
                general.start()
            try:
                while not (home / 'state/stop.flag').exists():
                    try:
                        if audio_enabled: worker.tick()
                    except BridgeError as e: worker.ui('error', code=str(e))
                    except Exception: worker.ui('error', code='unexpected_local_error')
                    if args.once: break
                    for _ in range(interval):
                        if (home / 'state/stop.flag').exists(): break
                        time.sleep(1)
                worker.ui('stopped')
            finally:
                if general:
                    general.stop()
                    if general.thread: general.thread.join()
                if feedback: feedback.close()
    except BridgeError: pass

if __name__ == '__main__': main()
