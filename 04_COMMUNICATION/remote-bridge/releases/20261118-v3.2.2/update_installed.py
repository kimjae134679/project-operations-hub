"""Backed-up, idle-only local update. --apply is required; never changes config/ACLs."""
import argparse, ctypes, datetime as dt, hashlib, json, os, shutil, subprocess, sys, time
import urllib.request, urllib.parse
from pathlib import Path

SOURCE=Path(__file__).resolve().parent
class NoRedirect(urllib.request.HTTPRedirectHandler):
 def redirect_request(self,*a,**k):raise RuntimeError('endpoint_redirect_refused')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def endpoint(home):
 value=json.loads((home/'state/local_endpoint.json').read_text(encoding='utf-8-sig'))
 url=urllib.parse.urlparse(value['baseUrl'])
 if url.scheme!='http' or url.hostname!='127.0.0.1' or not url.port or url.path:raise RuntimeError('invalid_local_endpoint')
 return value
def request(home,path,data=None):
 value=endpoint(home);raw=None if data is None else json.dumps(data).encode()
 req=urllib.request.Request(value['baseUrl']+path,data=raw,headers={'X-ProjectBridge-Token':value['token'],'Content-Type':'application/json'})
 with urllib.request.build_opener(urllib.request.ProxyHandler({}),NoRedirect()).open(req,timeout=15) as response:return json.load(response)
def inside(base,path):
 resolved=path.resolve()
 if resolved==base.resolve() or base.resolve() not in resolved.parents:raise RuntimeError('path_outside_installation')
 return resolved
def replace(source,dest):
 dest.parent.mkdir(parents=True,exist_ok=True);staged=dest.with_name(dest.name+'.update-322.tmp')
 shutil.copy2(source,staged);os.replace(staged,dest)
def start(home):
 result=subprocess.run([str(home/'프로젝트연결.exe'),'--resume','--background'],creationflags=subprocess.CREATE_NO_WINDOW,timeout=15)
 if result.returncode:raise RuntimeError('launcher_resume_failed')
def wait_ready(home,version,timeout=300):
 deadline=time.monotonic()+timeout
 while time.monotonic()<deadline:
  try:
   status=request(home,'/v1/status')
   if status.get('version')==version and status.get('localReady'):return status
  except (OSError,ValueError):pass
  time.sleep(.5)
 raise RuntimeError('updated_worker_not_ready')
def main():
 parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--home',type=Path,default=Path('D:/A_KJ/AI/Applications/ProjectBridge'));parser.add_argument('--apply',action='store_true');args=parser.parse_args()
 if os.name!='nt':raise RuntimeError('windows_required')
 home=args.home.resolve();runtime=inside(home,home/'Runtime')
 if home.is_symlink() or runtime.is_symlink():raise RuntimeError('reparse_installation_refused')
 manifest=json.loads((SOURCE/'release_manifest.json').read_text(encoding='utf-8'))
 for row in manifest['files']:
  path=inside(SOURCE,SOURCE/row['path'])
  if sha(path)!=row['sha256']:raise RuntimeError('release_hash_mismatch:'+row['path'])
 before=request(home,'/v1/status');config_hash=sha(home/'config.json')
 if before.get('activeJobCount')!=0 or before.get('runningJobs')!=0:raise RuntimeError('bridge_busy_no_update')
 if before.get('stage') in ('stopped','disconnected'):raise RuntimeError('bridge_paused_no_implicit_resume')
 audio_status=home/'state/ui_status.json'
 if audio_status.exists() and json.loads(audio_status.read_text(encoding='utf-8-sig')).get('stage')=='running':raise RuntimeError('legacy_operation_busy_no_update')
 plan={'version':manifest['version'],'previousVersion':before.get('version'),'files':len(manifest['files']),'configPreserved':True,'pendingJobsPreserved':before.get('pendingJobs')}
 print(json.dumps({'plan':plan}),flush=True)
 if not args.apply:return
 # Compile before touching the live worker. Existing launcher remains running.
 backup=inside(home,home/'state/install-backups'/('20261118-v3.2.2-'+dt.datetime.now().strftime('%H%M%S')))
 backup.mkdir(parents=True);helper=backup/'build/DesktopAutomation.exe';helper.parent.mkdir()
 compiler=Path(os.environ['WINDIR'])/'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
 compiled=subprocess.run([str(compiler),'/nologo','/target:winexe','/r:System.Windows.Forms.dll','/r:System.Drawing.dll','/r:System.Web.Extensions.dll','/out:'+str(helper),str(SOURCE/'DesktopAutomation.cs')],capture_output=True,text=True,creationflags=subprocess.CREATE_NO_WINDOW,timeout=30)
 if compiled.returncode:raise RuntimeError('helper_compile_failed:'+compiled.stdout+compiled.stderr)
 names=[r['path'] for r in manifest['files']]+['release_manifest.json','DesktopAutomation.exe']
 snapshots={name:sha(runtime/name) if (runtime/name).is_file() else None for name in names}
 (backup/'before.json').write_text(json.dumps({'plan':plan,'sha256':snapshots,'configSha256':config_hash},indent=2),encoding='utf-8')
 changed=[];stopped=False;replacement_started=False
 try:
  request(home,'/v1/control',{'action':'pause'})
  check=request(home,'/v1/status')
  if check.get('workerPid')!=before.get('workerPid') or check.get('activeJobCount')!=0 or check.get('runningJobs')!=0:raise RuntimeError('bridge_changed_or_busy')
  if audio_status.exists() and json.loads(audio_status.read_text(encoding='utf-8-sig')).get('stage')=='running':raise RuntimeError('legacy_operation_busy_no_update')
  kernel=ctypes.WinDLL('kernel32',use_last_error=True)
  kernel.OpenProcess.restype=ctypes.c_void_p;kernel.WaitForSingleObject.argtypes=[ctypes.c_void_p,ctypes.c_uint32];kernel.CloseHandle.argtypes=[ctypes.c_void_p]
  handle=kernel.OpenProcess(0x00100000,False,int(before['workerPid']))
  if not handle:raise RuntimeError('worker_identity_unavailable')
  try:
   try:request(home,'/v1/control',{'action':'stop'})
   except OSError:pass # An acknowledged shutdown may close its HTTP socket.
   stopped=True
   if kernel.WaitForSingleObject(handle,20000)!=0:raise RuntimeError('worker_did_not_stop_no_force')
  finally:kernel.CloseHandle(handle)
  for name in names:
   dest=inside(home,runtime/name);expected=snapshots[name]
   if (sha(dest) if dest.is_file() else None)!=expected:raise RuntimeError('installed_file_changed:'+name)
   if expected is not None:
    old=backup/'Runtime'/name;old.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(dest,old)
   replace(helper if name=='DesktopAutomation.exe' else SOURCE/name,dest);changed.append(name)
  if sha(home/'config.json')!=config_hash:raise RuntimeError('config_changed_during_update')
  start(home);replacement_started=True;after=wait_ready(home,manifest['version'])
  report={'installed':True,'version':after['version'],'workerPid':after['workerPid'],'localReady':after['localReady'],'configPreserved':sha(home/'config.json')==config_hash,'backup':str(backup),'helperSha256':sha(runtime/'DesktopAutomation.exe')}
  (backup/'result.json').write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(report),flush=True)
 except Exception:
  # Never roll bytes backward beneath a live replacement worker. If it started,
  # leave its intact backup and the diagnostic for a deliberate recovery.
  if stopped and not replacement_started:
   live=None
   try:live=request(home,'/v1/status')
   except (OSError,ValueError):pass
   if not live:
    for name in reversed(changed):
     dest=runtime/name;old=backup/'Runtime'/name
     if old.exists():replace(old,dest)
     elif snapshots[name] is None:dest.unlink(missing_ok=True)
    start(home)
  elif not stopped:
   request(home,'/v1/control',{'action':'resume'})
  raise
if __name__=='__main__':main()
