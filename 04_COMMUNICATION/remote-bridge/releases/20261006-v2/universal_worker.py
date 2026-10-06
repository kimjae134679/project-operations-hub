"""Private, device-scoped PC jobs, independent of audiobook operation latency."""
import datetime as dt
import hashlib,json,os,re,threading,time
from pathlib import Path

QUEUE='_pc_bridge/queue.json'
RESULTS='_pc_bridge/results'
ID=re.compile(r'^[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}$')
PROJECT=re.compile(r'^[a-zA-Z0-9][a-zA-Z0-9_-]{0,119}$')
ACTIONS=frozenset(('capabilities','read_file','write_file','list_dir','run_command',
 'start_process','process_status','stop_process','ui_control'))
NAMES={'capabilities':'PC 기능 확인','read_file':'파일 읽기','write_file':'파일 수정',
 'list_dir':'폴더 확인','run_command':'명령 실행','start_process':'장기 작업 시작',
 'process_status':'작업 진행 확인','stop_process':'지정 작업 중지','ui_control':'화면 확인·조작'}
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
 temp=path.with_name(path.name+'.tmp');temp.write_bytes(serialized(value));os.replace(temp,path)
def stamp(value):
 if not isinstance(value,str):raise UniversalError('invalid_job_time')
 try:v=dt.datetime.fromisoformat(value.replace('Z','+00:00'))
 except ValueError:raise UniversalError('invalid_job_time')
 if v.tzinfo is None:raise UniversalError('job_time_requires_timezone')
 return v

def validate_job(job,device,at=None):
 required={'id','target','deviceId','action','createdAt','expiresAt','args'}
 if not isinstance(job,dict) or not required<=set(job) or set(job)-required-{'dependsOn','projectId'}:
  raise UniversalError('invalid_general_job_schema')
 if not ID.fullmatch(str(job['id'])) or job['target']!='PC' or job['deviceId']!=device:
  raise UniversalError('invalid_general_job_target')
 if job['action'] not in ACTIONS or not isinstance(job['args'],dict):raise UniversalError('invalid_general_job_action')
 if not PROJECT.fullmatch(str(job.get('projectId','Control-Tower'))):raise UniversalError('invalid_project_id')
 dependencies=job.get('dependsOn',[])
 if not isinstance(dependencies,list) or len(dependencies)>30 or any(not isinstance(x,str) or not ID.fullmatch(x) or x==job['id'] for x in dependencies):
  raise UniversalError('invalid_general_dependencies')
 created=stamp(job['createdAt']);expires=stamp(job['expiresAt']);at=at or dt.datetime.now(dt.timezone.utc)
 if created>at+dt.timedelta(minutes=5) or expires<=at or expires<=created or expires-created>dt.timedelta(days=14):
  raise UniversalError('general_job_expired_or_invalid_time')
 if len(serialized(job))>850*1024:raise UniversalError('general_job_too_large')
 return job

class UniversalWorker:
 def __init__(self,home,private,public,executor=None):
  self.home=Path(home);self.state=self.home/'state';self.config=read(self.home/'config.json',{})
  self.device=self.config.get('deviceId','');self.private=private;self.public=public
  if not ID.fullmatch(self.device):raise UniversalError('invalid_device_id')
  self.journal_path=self.state/'universal_journal.json';self.journal=read(self.journal_path,{})
  self.stop_event=threading.Event();self.thread=None;self.last_hello=0;self.last_connection=0
  if executor is None:
   from universal_actions import perform
   executor=perform
  self.executor=executor
 def ui(self,stage,**data):
  atomic(self.state/'universal_status.json',{'stage':stage,'device':self.device,
   'workerPid':os.getpid(),'updatedAt':now(),**data})
 def checkpoint(self):atomic(self.journal_path,self.journal)
 def verify_private(self):
  metadata=self.private.request('GET','')
  if metadata.get('private') is not True:raise UniversalError('private_channel_required')
 def result_path(self,job):return RESULTS+'/'+self.device+'/'+job+'.json'
 def make_result(self,job_id,job_hash,outcome,data):
  return {'schemaVersion':1,'device':self.device,'jobId':job_id,'jobSha256':job_hash,
   'finishedAt':now(),'outcome':outcome,'data':data}
 def publish(self,job_id,record):
  # Raw output/image/filenames never go to the public client.
  self.verify_private();self.private.publish(self.result_path(job_id),record['result'])
  result=record['result'];ok=result['outcome']=='completed'
  action=record.get('action','capabilities');title=NAMES[action]
  finished=result['finishedAt'];rid='pc-bridge-'+job_id;project=record.get('projectId','Control-Tower')
  item={'schemaVersion':1,'recordType':'task_exchange','recordId':rid,'revision':1,
   'title':'PC 연결 · '+title,'projectId':project,'actorId':'ProjectBridge/'+self.device,
   'sessionId':self.device,'receivedAt':record.get('createdAt',finished),'updatedAt':finished,
   'request':{'summary':title,'details':'사용자가 요청한 비공개 PC 작업. 상세 명령은 비공개 통로에 보관.',
    'source':'private_pc_bridge_queue'},
   'response':{'summary':title+(' 완료' if ok else ' 확인 필요'),
    'details':'상세 결과는 비공개 PC 연결 통로에서 확인합니다. 원문·명령·로그·화면은 공개하지 않습니다.',
    'source':'project_bridge_operational_result'},
   'status':'completed' if ok else 'blocked','workDone':[title] if ok else [],
   'verification':[{'name':'로컬 작업 결과','result':'pass' if ok else 'fail',
    'evidence':['private_pc_bridge_result:'+self.device+'/'+job_id]}],
   'nextActions':[] if ok else ['비공개 상세 결과 확인'],
   'blockers':[] if ok else ['private_operation_requires_review'],'supersedes':[]}
  body=serialized(item);h=digest(body)
  base='04_COMMUNICATION/project-inbox/'+project+'/'+h
  self.public.publish(base+'/content.json',item)
  source_hash=digest(json.dumps([project,item['actorId'],rid],ensure_ascii=False).encode())
  self.public.publish(base+'/item.json',{'projectId':project,'contentSha256':h,
   'sourceName':'task-'+source_hash+'-r1.json','centralPath':base+'/content.json','collectedAt':finished})
 def publish_pending(self):
  for job_id,record in list(self.journal.items()):
   if record['state']=='started':
    record['result']=self.make_result(job_id,record['jobHash'],'interrupted',
     {'code':'interrupted_review_before_new_job_id'})
    record['state']='finished';self.checkpoint()
   if record['state']=='finished':
    self.publish(job_id,record);record['state']='published';self.checkpoint()
 def tick(self):
  if (self.state/'disconnected.flag').exists():self.ui('disconnected');return
  self.verify_private();self.publish_pending()
  if time.monotonic()-self.last_hello>300:
   capability=self.executor({'id':'device-capabilities','action':'capabilities','args':{}},self.config,self.state/'general')
   self.private.upsert('_pc_bridge/devices/'+self.device+'.json',{
    'schemaVersion':1,'deviceId':self.device,'lastSeenAt':now(),'capabilities':capability,
    'queuePath':QUEUE,'privateRepository':self.config['privateRepository'],
    'privateBranch':self.config.get('privateBranch','remote/pc-bridge')})
   self.last_hello=time.monotonic()
  queue=json.loads(self.private.read_bytes(QUEUE))
  if not isinstance(queue,dict) or set(queue)!={'schemaVersion','jobs'} or queue['schemaVersion']!=1 or not isinstance(queue['jobs'],list) or len(queue['jobs'])>250:
   raise UniversalError('invalid_general_queue')
  pending=[];seen=set()
  for row in queue['jobs']:
   if isinstance(row,dict) and row.get('deviceId')!=self.device:continue
   try:job=validate_job(row,self.device)
   except UniversalError:continue
   if job['id'] in seen:raise UniversalError('duplicate_general_job_id')
   seen.add(job['id']);key=hashed(job);old=self.journal.get(job['id'])
   if old:
    if old['jobHash']!=key:raise UniversalError('general_job_id_reused')
    continue
   pending.append(job)
  self.ui('connected',pendingJobs=len(pending),privateChannelAvailable=True)
  for job in pending:
   if self.stop_event.is_set() or (self.state/'stop.flag').exists() or (self.state/'disconnected.flag').exists():break
   dependencies=[self.journal.get(i) for i in job.get('dependsOn',[])]
   if any(x is None for x in dependencies):continue
   key=hashed(job);jid=job['id'];started=now()
   record={'jobHash':key,'state':'started','action':job['action'],
    'projectId':job.get('projectId','Control-Tower'),'createdAt':min(job['createdAt'],started),
    'startedAt':started}
   self.journal[jid]=record;self.checkpoint();self.ui('running',currentJob=jid,action=job['action'])
   try:
    if any(d.get('result',{}).get('outcome')!='completed' for d in dependencies):
     raise UniversalError('general_prerequisite_failed')
    self.verify_private()
    data=self.executor(job,self.config,self.state/'general');outcome='completed'
    if job['action']=='run_command' and data.get('succeeded') is not True:outcome='failed'
    if job['action']=='ui_control' and data.get('ok') is not True:outcome='failed'
    encoded=serialized(data)
    if len(encoded)>800*1024:
     private_log=self.state/'general'/'oversized_results'/(jid+'.json');atomic(private_log,data)
     raise UniversalError('private_result_too_large_use_smaller_read')
   except Exception as exc:
    code=str(exc)
    if not re.fullmatch(r'[a-zA-Z0-9_]{1,100}',code):code='general_operation_failed'
    data={'code':code};outcome='failed'
   record['result']=self.make_result(jid,key,outcome,data);record['state']='finished';self.checkpoint()
   self.publish(jid,record);record['state']='published';self.checkpoint()
  self.ui('connected',privateChannelAvailable=True)
 def run(self):
  delay=5
  while not self.stop_event.is_set() and not (self.state/'stop.flag').exists():
   try:self.tick();delay=5
   except Exception as exc:
    code=str(exc)
    if not re.fullmatch(r'[a-zA-Z0-9_]{1,100}',code):code='private_channel_connection_error'
    self.ui('error',code=code);delay=min(120,max(15,delay*2))
   self.stop_event.wait(delay)
  self.ui('stopped')
 def start(self):
  self.thread=threading.Thread(target=self.run,name='private-pc-bridge',daemon=True);self.thread.start()
 def stop(self):self.stop_event.set()
