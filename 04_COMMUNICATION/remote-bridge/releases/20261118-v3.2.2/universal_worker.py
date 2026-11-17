from version import VERSION
"""Shared local-first scheduler. Private GitHub transport is an optional relay."""
import concurrent.futures as futures
import datetime as dt
import hashlib,json,os,re,threading,time,uuid
from pathlib import Path
from process_runner import replace_state_file

QUEUE='_pc_bridge/queue.json'
RESULTS='_pc_bridge/results'
ID=re.compile(r'^[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}$')
PROJECT=re.compile(r'^[a-zA-Z0-9][a-zA-Z0-9_-]{0,119}$')
ACTIONS=frozenset(('capabilities','read_file','write_file','list_dir','make_dir','move_file','delete_file','restore_file','run_command','start_process','process_status','stop_process','ui_control','file_info','upload_begin','upload_chunk','upload_status','upload_commit','upload_abort','find_files'))
NAMES={'capabilities':'PC 기능 확인','read_file':'파일 읽기','write_file':'파일 수정','list_dir':'폴더 확인','run_command':'명령 실행','start_process':'장기 작업 시작','process_status':'작업 상태 확인','stop_process':'등록 작업 중지','ui_control':'화면 확인·조작'}
NAMES.update({name:name.replace('_',' ') for name in ['find_files','file_info', 'upload_begin', 'upload_chunk', 'upload_status', 'upload_commit', 'upload_abort']})
class UniversalError(Exception):pass
def now():return dt.datetime.now(dt.timezone.utc).isoformat()
def digest(raw):return hashlib.sha256(raw).hexdigest()
def serialized(value):return json.dumps(value,ensure_ascii=False,sort_keys=True,indent=2).encode()
def hashed(value):return digest(json.dumps(value,ensure_ascii=False,sort_keys=True).encode())
def read(path,default=None):
 try:return json.loads(Path(path).read_text(encoding='utf-8-sig'))
 except (OSError,ValueError):return default
def atomic(path,value):
 path=Path(path);path.parent.mkdir(parents=True,exist_ok=True)
 temp=path.with_name('.pb-'+uuid.uuid4().hex+'.tmp')
 try:
  with temp.open('xb') as output:output.write(serialized(value));output.flush();os.fsync(output.fileno())
  replace_state_file(temp,path)
 finally:
  if temp.exists():temp.unlink()
def stamp(value):
 if not isinstance(value,str):raise UniversalError('invalid_job_time')
 try:v=dt.datetime.fromisoformat(value.replace('Z','+00:00'))
 except ValueError:raise UniversalError('invalid_job_time')
 if v.tzinfo is None:raise UniversalError('job_time_requires_timezone')
 return v
def validate_job(job,device,at=None):
 required={'id','target','deviceId','action','createdAt','expiresAt','args'}
 if not isinstance(job,dict) or not required<=set(job) or set(job)-required-{'dependsOn','projectId','resourceKeys','toolId','title','tags','batchId'}:raise UniversalError('invalid_general_job_schema')
 if not ID.fullmatch(str(job['id'])) or job['target']!='PC' or job['deviceId']!=device:raise UniversalError('invalid_general_job_target')
 if job['action'] not in ACTIONS or not isinstance(job['args'],dict):raise UniversalError('invalid_general_job_action')
 if not PROJECT.fullmatch(str(job.get('projectId','Control-Tower'))):raise UniversalError('invalid_project_id')
 if not PROJECT.fullmatch(str(job.get('toolId','ProjectBridge'))):raise UniversalError('invalid_tool_id')
 from tool_catalog import check,string,TAGS
 if 'title' in job:check(job['title'],string(200))
 if 'tags' in job:check(job['tags'],TAGS)
 if 'batchId' in job and not ID.fullmatch(job['batchId']):raise UniversalError('invalid_batch_id')
 deps=job.get('dependsOn',[])
 if not isinstance(deps,list) or len(deps)>30 or any(not isinstance(x,str) or not ID.fullmatch(x) or x==job['id'] for x in deps) or len(deps)!=len(set(deps)):raise UniversalError('invalid_general_dependencies')
 keys=job.get('resourceKeys',[])
 if not isinstance(keys,list) or len(keys)>30 or any(not isinstance(x,str) or not PROJECT.fullmatch(x) for x in keys):raise UniversalError('invalid_resource_keys')
 created=stamp(job['createdAt']);expires=stamp(job['expiresAt']);at=at or dt.datetime.now(dt.timezone.utc)
 if created>at+dt.timedelta(minutes=5) or expires<=at or expires<=created or expires-created>dt.timedelta(days=14):raise UniversalError('general_job_expired_or_invalid_time')
 if len(serialized(job))>850*1024:raise UniversalError('general_job_too_large')
 return job
def resource_keys(job):
 args=job['args'];action=job['action'];keys=set('explicit:'+x for x in job.get('resourceKeys',[]))
 if action=='ui_control':keys.add('desktop-input')
 if action.startswith('upload_'):keys.add('upload:'+str(args.get('uploadId',args.get('path','new'))))
 if action in ('read_file','file_info','upload_begin','write_file','make_dir','move_file','delete_file') and isinstance(args.get('path'),str):keys.add('file:'+os.path.normcase(str(Path(args['path']).resolve())))
 if action=='move_file' and isinstance(args.get('destination'),str):keys.add('file:'+os.path.normcase(str(Path(args['destination']).resolve())))
 if isinstance(job.get('_uploadPath'),str):keys.add('file:'+os.path.normcase(str(Path(job['_uploadPath']).resolve())))
 if action=='find_files' and isinstance(args.get('path'),str):keys.add('directory:'+os.path.normcase(str(Path(args['path']).resolve())))
 if action=='restore_file':keys.add('trash:'+str(args.get('trashId')))
 if action in ('run_command','start_process') and isinstance(args.get('cwd'),str):keys.add('cwd:'+os.path.normcase(str(Path(args['cwd']).resolve())))
 if action in ('process_status','stop_process'):keys.add('process:'+str(args.get('processId')))
 return keys

class UniversalWorker:
 def __init__(self,home,private=None,public=None,executor=None):
  self.home=Path(home);self.state=self.home/'state';self.config=read(self.home/'config.json',{})
  self.device=self.config.get('deviceId','');self.private=private;self.public=public
  if not ID.fullmatch(self.device):raise UniversalError('invalid_device_id')
  self.journal_path=self.state/'universal_journal.json';self.journal=read(self.journal_path,{})
  self.lock=threading.RLock();self.active={};self.monitoring={};self.held=set();self.stop_event=threading.Event();self.thread=None
  self.detached_states={};self.detached_cursor=0
  self.last_hello=0;self.relay_code=None;self.relay_available=False
  self.worker_count=max(1,min(16,int(self.config.get('maxWorkers',4))))
  self.pool=futures.ThreadPoolExecutor(max_workers=self.worker_count,thread_name_prefix='project-bridge')
  if executor is None:
   from universal_actions import perform
   executor=perform
  self.executor=executor
  # Classify abandoned starts once, never while an active job is still running.
  recovered=set()
  for jid,record in self.journal.items():
   if record.get('state')=='started':
    record['result']=self.make_result(jid,record['jobHash'],'interrupted',{'code':'interrupted_review_before_new_job_id'})
    record['state']='finished'
    recovered.add(jid)
   elif record.get('state')=='monitoring' or self.registered_process(record):
    # A ticket acceptance is not child completion. Recover its reservation,
    # including v3 journals that prematurely marked acceptance as published.
    if record.get('state')=='published':record['publicationRevision']=record.get('publicationRevision',1)+1
    record.setdefault('acceptedResult',json.loads(json.dumps(record.get('result',{}))))
    record['state']='monitoring'
    recovered.add(jid)
    job=record.get('job')
    self.monitoring[jid]=resource_keys(self.scoped_job(job)[0]) if job else set()
  self.refresh_processes()
  self.checkpoint()
  for jid in recovered:self.save_record(jid,self.journal[jid])
  from control_plane import Manager
  self.manager=Manager(self)
 def checkpoint(self):
  with self.lock:
   self.compact_history()
   atomic(self.journal_path,self.journal)
 def compact_history(self):
  # Keep full immutable terminal records in their actor journal, and only a
  # small index in the hot checkpoint. Old command/output payloads are retained.
  rows=list(self.journal.items())
  for jid,row in rows[:-20]:
   if row.get('historyArchive') or not row.get('job'):continue
   if not (row.get('source')=='local' and row.get('state')=='finished' or row.get('state')=='published'):continue
   if self.registered_process(row):continue
   raw=serialized(row)
   if len(raw)<=4096:continue
   archive=self.scope(row['job'])[0]/'journal'/(jid+'.json')
   # Actor journals already contain the full record. Reuse verified bytes;
   # thousands of redundant fsyncs can make a mechanical disk unusable here.
   if not archive.is_file() or archive.read_bytes()!=raw:self.save_record(jid,row)
   indexed=json.loads(json.dumps({k:v for k,v in row.items() if k not in ('job','result','acceptedResult')}))
   indexed['job']={k:v for k,v in row['job'].items() if k!='args'};indexed['job']['args']={}
   if isinstance(row.get('result'),dict):
    result=row['result'];indexed['result']={k:v for k,v in result.items() if k!='data'}
    data=result.get('data',{})
    indexed['result']['data']={k:data[k] for k in ('processId','stage','done','running','succeeded','returnCode','code','error') if k in data}
   indexed['historyArchive']={'schemaVersion':1,'sha256':digest(raw)}
   self.journal[jid]=indexed
 def full_record(self,jid,row):
  marker=row.get('historyArchive')
  if not marker:return row
  if not isinstance(marker,dict) or marker.get('schemaVersion')!=1:raise UniversalError('invalid_history_archive')
  try:
   raw=(self.scope(row['job'])[0]/'journal'/(jid+'.json')).read_bytes()
   if digest(raw)!=marker.get('sha256'):raise UniversalError('history_archive_hash_mismatch')
   full=json.loads(raw)
   if full.get('jobHash')!=row.get('jobHash') or full.get('job',{}).get('id')!=jid or self.scope(full['job'])!=self.scope(row['job']):raise UniversalError('history_archive_identity_mismatch')
   return full
  except (OSError,ValueError,KeyError):raise UniversalError('history_archive_unavailable') from None
 def scope(self,job):
  project=digest(job.get('projectId','Control-Tower').encode())[:16]
  issuer=digest(job.get('toolId','ProjectBridge').encode())[:16]
  return self.state/'general/actors'/project/issuer,self.state/'workspaces'/project/issuer
 def scoped_job(self,job,create_workspace=False):
  value=json.loads(json.dumps(job));state,workspace=self.scope(value)
  if hasattr(self,'manager'):
   value['args']=self.manager.resolve_args(value['args'],self.manager.scope(value))
  if value['action'] in ('run_command','start_process') and not value['args'].get('cwd'):
   if create_workspace:workspace.mkdir(parents=True,exist_ok=True)
   value['args']['cwd']=str(workspace.resolve())
  if value['action'] in ('process_status','stop_process') and value.get('projectId','Control-Tower')=='Control-Tower' and value.get('toolId','ProjectBridge')=='ProjectBridge':
   pid=value['args'].get('processId','')
   if isinstance(pid,str) and re.fullmatch(r'[a-f0-9]{32}',pid) and not (state/'processes'/pid/'ticket.json').exists() and (self.state/'general/processes'/pid/'ticket.json').exists():
    # v2 tickets cannot be moved: a detached runner still writes to its old
    # directory. Only the legacy default issuer may access these owned tickets.
    state=self.state/'general'
  if value['action'].startswith('upload_'):
   uid=value['args'].get('uploadId')
   if isinstance(uid,str) and re.fullmatch(r'[a-f0-9]{32}',uid):
    meta=state/'uploads'/(uid+'.json')
    try:
     upload=json.loads(meta.read_text(encoding='utf-8'));value['_uploadPath']=upload['path']
    except (OSError,ValueError,KeyError):pass
  return value,state
 def save_record(self,jid,record):
  if record.get('historyArchive'):return # Never overwrite the preserved full record with its index.
  job=record.get('job')
  if job:atomic(self.scope(job)[0]/'journal'/(jid+'.json'),record)
 def registered_process(self,record):
  accepted=record.get('acceptedResult',record.get('result',{}))
  pid=accepted.get('data',{}).get('processId')
  return record.get('action')=='start_process' and accepted.get('outcome')=='completed' and isinstance(pid,str) and re.fullmatch(r'[a-f0-9]{32}',pid) is not None and not record.get('processFinished')
 def reserve_resources(self):
  self.held=set().union(*self.monitoring.values(),*(keys for future,keys in self.active.values()))
 def refresh_processes(self):
  changed=False
  for jid in list(self.monitoring):
   record=self.journal[jid];process=self.live_process(record,maximum=65536)
   if process is None:process={'stage':'interrupted','running':False,'done':True,'succeeded':False,'error':'registered_process_missing'}
   state=self.effective_state(record,process);self.detached_states[jid]=state
   result=self.make_result(jid,record['jobHash'],state,process)
   result['acceptedAt']=record['acceptedResult'].get('finishedAt')
   if state in ('starting','running'):
    result.pop('finishedAt',None)
   else:
    if isinstance(process.get('finishedAt'),(int,float)):result['finishedAt']=dt.datetime.fromtimestamp(process['finishedAt'],dt.timezone.utc).isoformat()
    record['state']='finished';record['processFinished']=True;del self.monitoring[jid]
   if record.get('result')!=result or record.get('state')=='finished':
    record['result']=result;atomic(self.state/'local_results'/(jid+'.json'),result);self.save_record(jid,record);changed=True
  if changed:self.checkpoint()
  self.reserve_resources()
 def control_reservations(self,job):
  if job['action'] not in ('process_status','stop_process'):return set()
  _,state=self.scoped_job(job);pid=job['args'].get('processId');owned=set()
  for jid,keys in self.monitoring.items():
   row=self.journal[jid];accepted=row.get('acceptedResult',{})
   if accepted.get('data',{}).get('processId')!=pid:continue
   parent=row.get('job') or {'projectId':row.get('projectId','Control-Tower'),'toolId':'ProjectBridge'}
   _,parent_state=self.scoped_job({**parent,'action':'process_status','args':{'processId':pid}})
   if parent_state==state:owned.update(keys)
  # Never bypass another live owner's reservation, even if its key overlaps.
  for jid,keys in self.monitoring.items():
   row=self.journal[jid];parent=row.get('job') or {'projectId':row.get('projectId','Control-Tower'),'toolId':'ProjectBridge'}
   parent_pid=row.get('acceptedResult',{}).get('data',{}).get('processId')
   _,parent_state=self.scoped_job({**parent,'action':'process_status','args':{'processId':parent_pid}})
   if parent_pid!=pid or parent_state!=state:owned.difference_update(keys)
  for future,keys in self.active.values():owned.difference_update(keys)
  return owned
 def live_process(self,record,maximum=0):
  if record.get('processFinished'):return None
  accepted=record.get('acceptedResult',record.get('result',{}))
  if record.get('action')!='start_process' or accepted.get('outcome')!='completed':return None
  pid=accepted.get('data',{}).get('processId')
  if not isinstance(pid,str) or not re.fullmatch(r'[a-f0-9]{32}',pid):return None
  job=record.get('job') or {'projectId':record.get('projectId','Control-Tower'),'toolId':'ProjectBridge'}
  status_job={**job,'action':'process_status','args':{'processId':pid}}
  _,state=self.scoped_job(status_job)
  try:
   from universal_actions import process_status
   return process_status(pid,state,maximum=maximum)
  except Exception as exc:
   code=str(exc)
   if not re.fullmatch(r'[a-zA-Z0-9_]{1,100}',code):code='process_status_unavailable'
   return {'processId':pid,'stage':'interrupted','running':False,'done':True,'succeeded':False,'error':code}
 def effective_state(self,record,process=None):
  if process is None:return record.get('result',{}).get('outcome',record.get('state'))
  stage=process.get('stage','interrupted')
  if stage=='completed':return 'completed' if process.get('succeeded') is True else 'failed'
  return stage if stage in ('starting','running','failed','timed_out','stopped','interrupted') else 'failed'
 def status(self):
  with self.lock:
   paused=(self.state/'disconnected.flag').exists()
   rows=list(self.journal.items());candidates=[];active=set(self.active);jobs=[];conflicts=[];invalid_conflicts=0
   for jid,r in rows:
    if self.registered_process(r):
     if self.detached_states.get(jid) not in ('completed','failed','timed_out','stopped'):candidates.append((jid,r))
   pending_ids={jid for jid,r in candidates}
   if candidates:
    offset=self.detached_cursor%len(candidates);batch=(candidates[offset:]+candidates[:offset])[:100]
    self.detached_cursor=(offset+len(batch))%len(candidates)
    for jid,r in batch:self.detached_states[jid]=self.effective_state(r,self.live_process(r))
   for jid,r in rows:
    if 'publicationConflict' in r:
     if self.valid_publication_conflict(jid,r):conflicts.append({'id':jid,**r['publicationConflict']})
     else:invalid_conflicts+=1
    state=self.detached_states.get(jid,self.effective_state(r))
    if jid not in self.detached_states and jid in pending_ids:state='starting'
    if state in ('starting','running') and r.get('action')=='start_process':active.add(jid)
    jobs.append({'id':jid,'title':NAMES.get(r.get('action'),'PC 작업'),'projectId':r.get('projectId','Control-Tower'),'toolId':r.get('job',{}).get('toolId','ProjectBridge'),'action':r.get('action'),'state':state,'startedAt':r.get('startedAt'),'outcome':state if r.get('result') else None})
   # Keep an old long-running owner visible even after many newer requests.
   live_rows=[r for r in jobs if r['id'] in active][-100:]
   jobs=live_rows+([r for r in jobs if r['id'] not in active][-(100-len(live_rows)):] if len(live_rows)<100 else [])
   return {'bridgeVersion':VERSION,'version':VERSION,'stage':'stopped' if self.stop_event.is_set() else 'disconnected' if paused else 'running' if active else 'connected',
    'deviceId':self.device,'localReady':not self.stop_event.is_set(),'relayConnected':self.relay_available,'parallelLimit':self.worker_count,'activeJobCount':len(active),
    'device':self.device,'workerPid':os.getpid(),'updatedAt':now(),'localChannelAvailable':not self.stop_event.is_set(),
    'privateChannelAvailable':self.relay_available,'relayCode':self.relay_code,'maxWorkers':self.worker_count,
    'publicationConflictCount':len(conflicts),'publicationConflicts':conflicts[-100:],'invalidPublicationConflictCount':invalid_conflicts,
    'pendingPublicationCount':sum(r.get('state')=='finished' and r.get('source','github')=='github' and 'publicationConflict' not in r for jid,r in rows),
    'runningJobs':len(active),'pendingJobs':sum(r.get('state')=='queued' for r in self.journal.values()),'jobs':jobs}
 def ui(self,*unused,**ignored):atomic(self.state/'universal_status.json',self.status())
 def verify_private(self):
  if self.private is None:raise UniversalError('private_relay_not_configured')
  metadata=self.private.request('GET','',authenticated=True)
  if metadata.get('private') is not True:raise UniversalError('private_channel_required')
  # Explicitly distinguish missing branch/access from a missing queue file.
  import urllib.parse
  branch=self.config.get('privateBranch','remote/pc-bridge')
  self.private.request('GET','branches/'+urllib.parse.quote(branch,safe=''),authenticated=True)
 def result_path(self,jid):return RESULTS+'/'+self.device+'/'+jid+'.json'
 def make_result(self,jid,key,outcome,data):return {'schemaVersion':1,'device':self.device,'jobId':jid,'jobSha256':key,'finishedAt':now(),'outcome':outcome,'data':data}
 def submit(self,job,source='local'):
  if not isinstance(job,dict) or not isinstance(job.get('id'),str):raise UniversalError('invalid_general_job_schema')
  key=hashed(job);jid=job['id']
  with self.lock:
   old=self.journal.get(jid)
   if old:
    if old['jobHash']!=key:raise UniversalError('general_job_id_reused')
    return {'accepted':True,'id':jid,'duplicate':True,'state':old['state']}
   validate_job(job,self.device)
   if hasattr(self,'manager'):
    job=json.loads(json.dumps(job));job['args']=self.manager.resolve_args(job['args'],self.manager.scope(job))
   if sum(r.get('state') in ('queued','started','monitoring') for r in self.journal.values())>=250:raise UniversalError('local_queue_full')
   def reaches(current,seen):
    if current==jid:return True
    if current in seen:return False
    seen.add(current);row=self.journal.get(current,{}).get('job',{})
    return any(reaches(x,seen) for x in row.get('dependsOn',[]))
   if any(reaches(x,set()) for x in job.get('dependsOn',[])):raise UniversalError('dependency_cycle')
   self.journal[jid]={'jobHash':key,'state':'queued','job':json.loads(json.dumps(job)),'source':source,'action':job['action'],'projectId':job.get('projectId','Control-Tower'),'createdAt':job['createdAt']}
   self.save_record(jid,self.journal[jid]);self.checkpoint();self.ui();return {'accepted':True,'id':jid,'duplicate':False,'state':'queued'}
 def get_job(self,jid):
  with self.lock:
   row=self.journal.get(jid)
   if row is None:raise UniversalError('job_not_found')
   row=self.full_record(jid,row)
   output=json.loads(json.dumps({'id':jid,'state':row['state'],'result':row.get('result')}))
   if row.get('processFinished'):output['state']=self.effective_state(row);output['process']=json.loads(json.dumps(row['result']['data']))
   process=self.live_process(row,maximum=65536)
   if process is not None:
    state=self.effective_state(row,process);self.detached_states[jid]=state;output['state']=state;output['process']=process
    accepted=row.get('acceptedResult',row['result'])
    output['acceptedResult']=json.loads(json.dumps(accepted))
    output['result'].update(outcome=state,data=process,acceptedAt=accepted.get('finishedAt'))
    output['result'].pop('finishedAt',None)
    if isinstance(process.get('finishedAt'),(int,float)):
     output['result']['finishedAt']=dt.datetime.fromtimestamp(process['finishedAt'],dt.timezone.utc).isoformat()
   elif 'acceptedResult' in row:output['acceptedResult']=json.loads(json.dumps(row['acceptedResult']))
   if 'publicationConflict' in row:
    output['publication']={'state':'quarantined',**row['publicationConflict']} if self.valid_publication_conflict(jid,row) else {'state':'blocked','code':'invalid_publication_conflict'}
   return output
 def execute(self,jid,record,prerequisite_failed=False):
  job=record['job']
  try:
   job,scoped_state=self.scoped_job(job,create_workspace=True)
   if prerequisite_failed:raise UniversalError('general_prerequisite_failed')
   if stamp(job['expiresAt'])<=dt.datetime.now(dt.timezone.utc):raise UniversalError('general_job_expired_or_invalid_time')
   data=self.executor(job,self.config,scoped_state);outcome='completed'
   if job['action']=='run_command' and data.get('succeeded') is not True:outcome='failed'
   if job['action']=='ui_control' and data.get('ok') is not True:outcome='failed'
   if len(serialized(data))>8*1024*1024:raise UniversalError('private_result_too_large_use_smaller_read')
  except Exception as exc:
   code=str(exc)
   if not re.fullmatch(r'[a-zA-Z0-9_]{1,100}',code):code='general_operation_failed'
   data={'code':code};outcome='failed'
  return self.make_result(jid,record['jobHash'],outcome,data)
 def pump(self):
  with self.lock:
   for jid,(future,keys) in list(self.active.items()):
    if future.done():
     record=self.journal[jid];record['result']=future.result();record['state']='finished'
     # Completed upload payloads need only a digest for audit; avoid retaining
     # every base64 chunk in the in-memory and persisted job journal.
     if record['job']['action']=='upload_chunk':
      payload=record['job']['args'].pop('contentBase64',None)
      if payload is not None:record['job']['args']['contentBase64Sha256']=digest(payload.encode())
     if self.registered_process(record):
      record['acceptedResult']=json.loads(json.dumps(record['result']));record['state']='monitoring';self.monitoring[jid]=keys
      # Never expose an accepted launch as a completed canonical file, even
      # in the checkpoint window before the first owner-status refresh.
      accepted=record['acceptedResult'];stage=self.effective_state(record,accepted['data'])
      record['result']=self.make_result(jid,record['jobHash'],stage,accepted['data'])
      record['result']['acceptedAt']=accepted.get('finishedAt')
      if stage in ('starting','running'):record['result'].pop('finishedAt',None)
     atomic(self.state/'local_results'/(jid+'.json'),record['result']);self.save_record(jid,record);del self.active[jid];self.checkpoint()
   self.refresh_processes()
   if self.stop_event.is_set() or (self.state/'stop.flag').exists() or (self.state/'disconnected.flag').exists():self.ui();return
   for jid,record in list(self.journal.items()):
    if len(self.active)>=self.worker_count:break
    if record.get('state')!='queued':continue
    if stamp(record['job']['expiresAt'])<=dt.datetime.now(dt.timezone.utc):
     record['result']=self.make_result(jid,record['jobHash'],'failed',{'code':'general_job_expired_or_invalid_time'});record['state']='finished'
     atomic(self.state/'local_results'/(jid+'.json'),record['result']);self.save_record(jid,record);self.checkpoint();continue
    deps=[self.journal.get(x) for x in record['job'].get('dependsOn',[])]
    if any(x is None or x.get('state') in ('queued','started','monitoring') for x in deps):continue
    scoped,_=self.scoped_job(record['job']);keys=resource_keys(scoped)
    if keys&(self.held-self.control_reservations(record['job'])):continue
    record['state']='started';record['startedAt']=now();self.save_record(jid,record);self.checkpoint()
    failed=any(d.get('result',{}).get('outcome')!='completed' for d in deps)
    self.held.update(keys);self.active[jid]=(self.pool.submit(self.execute,jid,record,failed),keys)
   if hasattr(self,'manager'):self.manager.tick()
   self.ui()
 def relay_context_digest(self):
  return digest(serialized([self.config.get('privateRepository'),self.config.get('privateBranch','remote/pc-bridge')]))
 def legacy_publication_identity(self,jid,record):
  accepted=record.get('acceptedResult');result=record.get('result');revision=record.get('publicationRevision')
  if not isinstance(accepted,dict) or not isinstance(result,dict):return False
  key=record.get('jobHash')
  if not isinstance(key,str) or not re.fullmatch(r'[a-f0-9]{64}',key):return False
  if record.get('state')!='finished' or record.get('source','github')!='github' or record.get('action')!='start_process' or record.get('processFinished') is not True:return False
  if type(revision) is not int or revision<2:return False
  if any(v.get('device')!=self.device or v.get('jobId')!=jid or v.get('jobSha256')!=key for v in (accepted,result)):return False
  data=accepted.get('data')
  if not isinstance(data,dict) or accepted.get('outcome')!='completed' or data.get('stage') not in ('starting','running') or data.get('done') is not False:return False
  if not isinstance(data.get('processId'),str) or not re.fullmatch(r'[a-f0-9]{32}',data['processId']):return False
  return result.get('outcome') in ('completed','failed','timed_out','stopped','interrupted') and result!=accepted
 def valid_publication_conflict(self,jid,record):
  marker=record.get('publicationConflict')
  fields={'schemaVersion','code','observedAt','localResultSha256','acceptedResultSha256','remoteResultSha256','relayContextSha256'}
  if not isinstance(marker,dict) or set(marker)!=fields or type(marker.get('schemaVersion')) is not int or marker['schemaVersion']!=1 or marker.get('code')!='legacy_acceptance_result_conflict' or not self.legacy_publication_identity(jid,record):return False
  if not isinstance(marker.get('observedAt'),str) or len(marker['observedAt'])>64:return False
  try:stamp(marker['observedAt'])
  except UniversalError:return False
  accepted=digest(serialized(record['acceptedResult']))
  return marker['localResultSha256']==digest(serialized(record['result'])) and marker['acceptedResultSha256']==accepted and marker['remoteResultSha256']==accepted and marker['relayContextSha256']==self.relay_context_digest()
 def publish(self,jid,record):
  self.verify_private()
  try:self.private.publish(self.result_path(jid),record['result'])
  except Exception as exc:
   # Only the observed legacy acceptance collision is recoverable. The worker
   # can run as __main__, so its BridgeError class need not be the imported one.
   import sys
   from bridge_worker import BridgeError
   actual_error=getattr(sys.modules.get(type(self.private).__module__),'BridgeError',None)
   errors=(BridgeError,actual_error) if isinstance(actual_error,type) and issubclass(actual_error,Exception) else (BridgeError,)
   if not isinstance(exc,errors) or str(exc)!='remote_result_conflict' or not self.legacy_publication_identity(jid,record):raise
   try:remote=json.loads(self.private.read_bytes(self.result_path(jid)))
   except (ValueError,UnicodeError):raise UniversalError('unrecognized_remote_result_conflict') from None
   if remote!=record['acceptedResult']:raise
   marker={'schemaVersion':1,'code':'legacy_acceptance_result_conflict','observedAt':now(),
    'localResultSha256':digest(serialized(record['result'])),'acceptedResultSha256':digest(serialized(record['acceptedResult'])),
    'remoteResultSha256':digest(serialized(remote)),'relayContextSha256':self.relay_context_digest()}
   with self.lock:
    current=self.journal[jid]
    if current.get('result')!=record['result'] or current.get('acceptedResult')!=record['acceptedResult']:raise UniversalError('publication_record_changed')
    current['publicationConflict']=marker;self.save_record(jid,current);self.checkpoint()
   return False
  # Public summaries are exported by an explicitly invoked maintainer, never
  # by the repeating PC relay. Private result delivery is independent.
  return True
 def publish_pending(self):
  with self.lock:
   pending=[]
   for jid,r in self.journal.items():
    if r.get('state')!='finished' or r.get('source','github')!='github':continue
    if 'publicationConflict' in r:
     if not self.valid_publication_conflict(jid,r):raise UniversalError('invalid_publication_conflict')
     continue
    if len(pending)<100:pending.append((jid,json.loads(json.dumps(r))))
  for jid,record in pending:
   if not self.publish(jid,record):continue
   with self.lock:self.journal[jid]['state']='published';self.save_record(jid,self.journal[jid]);self.checkpoint()
 def relay_tick(self):
  self.verify_private();self.relay_available=True;self.relay_code=None;self.publish_pending()
  if time.monotonic()-self.last_hello>60:
   capability=self.executor({'id':'device-capabilities','action':'capabilities','args':{}},self.config,self.state/'general')
   self.private.upsert('_pc_bridge/devices/'+self.device+'.json',{'schemaVersion':1,'bridgeVersion':VERSION,'deviceId':self.device,'lastSeenAt':now(),'capabilities':capability,'maxWorkers':self.worker_count,'localChannelAvailable':True,'queuePath':QUEUE,'privateRepository':self.config.get('privateRepository'),'privateBranch':self.config.get('privateBranch','remote/pc-bridge')})
   self.last_hello=time.monotonic()
  queue=json.loads(self.private.read_bytes(QUEUE))
  if not isinstance(queue,dict) or set(queue)!={'schemaVersion','jobs'} or queue['schemaVersion']!=1 or not isinstance(queue['jobs'],list) or len(queue['jobs'])>250:raise UniversalError('invalid_general_queue')
  seen=set()
  for row in queue['jobs']:
   if isinstance(row,dict) and row.get('deviceId')!=self.device:continue
   try:validate_job(row,self.device)
   except UniversalError:continue
   if row['id'] in seen:raise UniversalError('duplicate_general_job_id')
   seen.add(row['id']);self.submit(row,'github')
 def tick(self,wait=True):
  if (self.state/'disconnected.flag').exists():self.ui();return
  self.relay_tick();self.pump()
  if wait:
   while self.active:
    futures.wait([f for f,k in list(self.active.values())],timeout=.05);self.pump()
   self.publish_pending()
  self.ui()
 def run(self):
  from local_api import LocalServer
  self.server=LocalServer(self);next_relay=0;delay=5
  relay_pool=futures.ThreadPoolExecutor(max_workers=1,thread_name_prefix='github-relay');relay=None
  try:
   while not self.stop_event.is_set() and not (self.state/'stop.flag').exists():
    self.pump()
    if relay is not None and relay.done():
     try:relay.result();delay=5
     except Exception as exc:
      code=str(exc);self.relay_code=code if re.fullmatch(r'[a-zA-Z0-9_]{1,100}',code) else 'private_channel_connection_error';self.relay_available=False;delay=min(120,max(15,delay*2))
     relay=None;next_relay=time.monotonic()+delay;self.ui()
    if self.private and relay is None and time.monotonic()>=next_relay and not (self.state/'disconnected.flag').exists():relay=relay_pool.submit(self.relay_tick)
    self.stop_event.wait(.1)
  finally:
   self.stop_event.set();self.server.close();self.pool.shutdown(wait=False,cancel_futures=True);relay_pool.shutdown(wait=False,cancel_futures=True);self.ui()
 def start(self):self.thread=threading.Thread(target=self.run,name='shared-pc-bridge',daemon=True);self.thread.start()
 def stop(self):self.stop_event.set()
