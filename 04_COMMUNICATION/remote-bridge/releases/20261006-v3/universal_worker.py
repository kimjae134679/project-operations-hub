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
ACTIONS=frozenset(('capabilities','read_file','write_file','list_dir','run_command','start_process','process_status','stop_process','ui_control'))
NAMES={'capabilities':'PC 기능 확인','read_file':'파일 읽기','write_file':'파일 수정','list_dir':'폴더 확인','run_command':'명령 실행','start_process':'장기 작업 시작','process_status':'작업 상태 확인','stop_process':'등록 작업 중지','ui_control':'화면 확인·조작'}
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
 temp=path.with_name(path.name+'.'+uuid.uuid4().hex+'.tmp')
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
 if not isinstance(job,dict) or not required<=set(job) or set(job)-required-{'dependsOn','projectId','resourceKeys','toolId'}:raise UniversalError('invalid_general_job_schema')
 if not ID.fullmatch(str(job['id'])) or job['target']!='PC' or job['deviceId']!=device:raise UniversalError('invalid_general_job_target')
 if job['action'] not in ACTIONS or not isinstance(job['args'],dict):raise UniversalError('invalid_general_job_action')
 if not PROJECT.fullmatch(str(job.get('projectId','Control-Tower'))):raise UniversalError('invalid_project_id')
 if not PROJECT.fullmatch(str(job.get('toolId','ProjectBridge'))):raise UniversalError('invalid_tool_id')
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
 if action in ('read_file','write_file') and isinstance(args.get('path'),str):keys.add('file:'+os.path.normcase(str(Path(args['path']).resolve())))
 if action in ('run_command','start_process') and isinstance(args.get('cwd'),str):keys.add('cwd:'+os.path.normcase(str(Path(args['cwd']).resolve())))
 if action in ('process_status','stop_process'):keys.add('process:'+str(args.get('processId')))
 return keys

class UniversalWorker:
 def __init__(self,home,private=None,public=None,executor=None):
  self.home=Path(home);self.state=self.home/'state';self.config=read(self.home/'config.json',{})
  self.device=self.config.get('deviceId','');self.private=private;self.public=public
  if not ID.fullmatch(self.device):raise UniversalError('invalid_device_id')
  self.journal_path=self.state/'universal_journal.json';self.journal=read(self.journal_path,{})
  self.lock=threading.RLock();self.active={};self.held=set();self.stop_event=threading.Event();self.thread=None
  self.last_hello=0;self.relay_code=None;self.relay_available=False
  self.worker_count=max(1,min(16,int(self.config.get('maxWorkers',4))))
  self.pool=futures.ThreadPoolExecutor(max_workers=self.worker_count,thread_name_prefix='project-bridge')
  if executor is None:
   from universal_actions import perform
   executor=perform
  self.executor=executor
  # Classify abandoned starts once, never while an active job is still running.
  for jid,record in self.journal.items():
   if record.get('state')=='started':
    record['result']=self.make_result(jid,record['jobHash'],'interrupted',{'code':'interrupted_review_before_new_job_id'})
    record['state']='finished'
  self.checkpoint()
  for jid,record in self.journal.items():self.save_record(jid,record)
 def checkpoint(self):
  with self.lock:atomic(self.journal_path,self.journal)
 def scope(self,job):
  project=digest(job.get('projectId','Control-Tower').encode())[:16]
  issuer=digest(job.get('toolId','ProjectBridge').encode())[:16]
  return self.state/'general/actors'/project/issuer,self.state/'workspaces'/project/issuer
 def scoped_job(self,job,create_workspace=False):
  value=json.loads(json.dumps(job));state,workspace=self.scope(value)
  if value['action'] in ('run_command','start_process') and not value['args'].get('cwd'):
   if create_workspace:workspace.mkdir(parents=True,exist_ok=True)
   value['args']['cwd']=str(workspace.resolve())
  if value['action'] in ('process_status','stop_process') and value.get('projectId','Control-Tower')=='Control-Tower' and value.get('toolId','ProjectBridge')=='ProjectBridge':
   pid=value['args'].get('processId','')
   if isinstance(pid,str) and re.fullmatch(r'[a-f0-9]{32}',pid) and not (state/'processes'/pid/'ticket.json').exists() and (self.state/'general/processes'/pid/'ticket.json').exists():
    # v2 tickets cannot be moved: a detached runner still writes to its old
    # directory. Only the legacy default issuer may access these owned tickets.
    state=self.state/'general'
  return value,state
 def save_record(self,jid,record):
  job=record.get('job')
  if job:atomic(self.scope(job)[0]/'journal'/(jid+'.json'),record)
 def status(self):
  with self.lock:
   paused=(self.state/'disconnected.flag').exists()
   return {'bridgeVersion':'3.0.0','version':'3.0.0','stage':'stopped' if self.stop_event.is_set() else 'disconnected' if paused else 'running' if self.active else 'connected',
    'deviceId':self.device,'localReady':not self.stop_event.is_set(),'relayConnected':self.relay_available,'parallelLimit':self.worker_count,'activeJobCount':len(self.active),
    'device':self.device,'workerPid':os.getpid(),'updatedAt':now(),'localChannelAvailable':not self.stop_event.is_set(),
    'privateChannelAvailable':self.relay_available,'relayCode':self.relay_code,'maxWorkers':self.worker_count,
    'runningJobs':len(self.active),'pendingJobs':sum(r.get('state')=='queued' for r in self.journal.values()),
    'jobs':[{'id':jid,'title':NAMES.get(r.get('action'),'PC 작업'),'projectId':r.get('projectId','Control-Tower'),'toolId':r.get('job',{}).get('toolId','ProjectBridge'),'action':r.get('action'),'state':r.get('result',{}).get('outcome',r.get('state')),'startedAt':r.get('startedAt'),'outcome':r.get('result',{}).get('outcome')} for jid,r in list(self.journal.items())[-100:]]}
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
   if sum(r.get('state') in ('queued','started') for r in self.journal.values())>=250:raise UniversalError('local_queue_full')
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
   return json.loads(json.dumps({'id':jid,'state':row['state'],'result':row.get('result')}))
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
     atomic(self.state/'local_results'/(jid+'.json'),record['result']);self.save_record(jid,record);del self.active[jid];self.held.difference_update(keys);self.checkpoint()
   if self.stop_event.is_set() or (self.state/'stop.flag').exists() or (self.state/'disconnected.flag').exists():self.ui();return
   for jid,record in list(self.journal.items()):
    if len(self.active)>=self.worker_count:break
    if record.get('state')!='queued':continue
    if stamp(record['job']['expiresAt'])<=dt.datetime.now(dt.timezone.utc):
     record['result']=self.make_result(jid,record['jobHash'],'failed',{'code':'general_job_expired_or_invalid_time'});record['state']='finished'
     atomic(self.state/'local_results'/(jid+'.json'),record['result']);self.save_record(jid,record);self.checkpoint();continue
    deps=[self.journal.get(x) for x in record['job'].get('dependsOn',[])]
    if any(x is None or x.get('state') in ('queued','started') for x in deps):continue
    scoped,_=self.scoped_job(record['job']);keys=resource_keys(scoped)
    if keys&self.held:continue
    record['state']='started';record['startedAt']=now();self.save_record(jid,record);self.checkpoint()
    failed=any(d.get('result',{}).get('outcome')!='completed' for d in deps)
    self.held.update(keys);self.active[jid]=(self.pool.submit(self.execute,jid,record,failed),keys)
   self.ui()
 def publish(self,jid,record):
  self.verify_private();self.private.publish(self.result_path(jid),record['result'])
  if self.public is None:return
  result=record['result'];ok=result['outcome']=='completed';title=NAMES[record.get('action','capabilities')]
  finished=result['finishedAt'];rid='pc-bridge-'+jid;project=record.get('projectId','Control-Tower')
  item={'schemaVersion':1,'recordType':'task_exchange','recordId':rid,'revision':1,'title':'PC 작업 · '+title,'projectId':project,'actorId':'ProjectBridge/'+self.device,'sessionId':self.device,'receivedAt':record.get('createdAt',finished),'updatedAt':finished,
   'request':{'summary':title,'details':'사용자가 요청한 공용 PC 작업. 세부 내용은 비공개 경로에 보관.','source':'private_pc_bridge_queue'},
   'response':{'summary':title+(' 완료' if ok else ' 확인 필요'),'details':'실제 결과는 비공개 PC 작업 경로에서 확인합니다. 파일·명령·로그·화면은 공개하지 않습니다.','source':'project_bridge_operational_result'},
   'status':'completed' if ok else 'blocked','workDone':[title] if ok else [],'verification':[{'name':'실제 작업 결과','result':'pass' if ok else 'fail','evidence':['private_pc_bridge_result:'+self.device+'/'+jid]}],
   'nextActions':[] if ok else ['비공개 실제 결과 확인'],'blockers':[] if ok else ['private_operation_requires_review'],'supersedes':[]}
  h=digest(serialized(item));base='04_COMMUNICATION/project-inbox/'+project+'/'+h;self.public.publish(base+'/content.json',item)
  source_hash=digest(json.dumps([project,item['actorId'],rid],ensure_ascii=False).encode())
  self.public.publish(base+'/item.json',{'projectId':project,'contentSha256':h,'sourceName':'task-'+source_hash+'-r1.json','centralPath':base+'/content.json','collectedAt':finished})
 def publish_pending(self):
  with self.lock:pending=[(jid,json.loads(json.dumps(r))) for jid,r in self.journal.items() if r.get('state')=='finished' and r.get('source','github')=='github']
  for jid,record in pending:
   self.publish(jid,record)
   with self.lock:self.journal[jid]['state']='published';self.save_record(jid,self.journal[jid]);self.checkpoint()
 def relay_tick(self):
  self.verify_private();self.relay_available=True;self.relay_code=None;self.publish_pending()
  if time.monotonic()-self.last_hello>60:
   capability=self.executor({'id':'device-capabilities','action':'capabilities','args':{}},self.config,self.state/'general')
   self.private.upsert('_pc_bridge/devices/'+self.device+'.json',{'schemaVersion':1,'bridgeVersion':'3.0.0','deviceId':self.device,'lastSeenAt':now(),'capabilities':capability,'maxWorkers':self.worker_count,'localChannelAvailable':True,'queuePath':QUEUE,'privateRepository':self.config.get('privateRepository'),'privateBranch':self.config.get('privateBranch','remote/pc-bridge')})
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
