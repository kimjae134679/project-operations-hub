import datetime as dt,json,sys,tempfile,unittest
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parents[1]))
import universal_worker as u
from unittest.mock import Mock

class Client:
 def __init__(self,jobs=None):self.jobs=jobs or [];self.private=True;self.values={};self.writes=[];self.fail=False
 def request(self,*args,**kwargs):return {'private':self.private}
 def read_bytes(self,path):return json.dumps({'schemaVersion':1,'jobs':self.jobs}).encode()
 def publish(self,path,value):
  if self.fail:raise RuntimeError('github_network_error')
  self.values[path]=value;self.writes.append((path,value))
 def upsert(self,path,value):self.values[path]=value

def job(identity='general1',device='device1',action='read_file'):
 at=dt.datetime.now(dt.timezone.utc)
 return {'id':identity,'target':'PC','deviceId':device,'action':action,
  'createdAt':at.isoformat(),'expiresAt':(at+dt.timedelta(days=1)).isoformat(),
  'args':{'path':'D:/AI/private.txt'}}

class GeneralWorker(unittest.TestCase):
 def setUp(self):
  self.tmp=tempfile.TemporaryDirectory();self.addCleanup(self.tmp.cleanup)
  self.home=Path(self.tmp.name);u.atomic(self.home/'config.json',{'deviceId':'device1','privateRepository':'kimjae134679/private','privateBranch':'remote/pc-bridge'})
  self.private=Client([job()]);self.public=Client();self.calls=[]
  def execute(job,config,state):
   self.calls.append(job['action'])
   return {'output':'NOVEL_SECRET','argv':['PRIVATE_COMMAND'],'jpeg_base64':'PRIVATE_IMAGE'}
  self.execute=execute
 def worker(self):return u.UniversalWorker(self.home,self.private,self.public,self.execute)
 def test_general_result_is_private_and_public_record_is_sanitized(self):
  worker=self.worker();worker.tick()
  private=json.dumps(self.private.values);public=json.dumps(self.public.values)
  for secret in ['NOVEL_SECRET','PRIVATE_COMMAND','PRIVATE_IMAGE']:
   self.assertIn(secret,private);self.assertNotIn(secret,public)
  self.assertIn('task_exchange',public);self.assertEqual(worker.journal['general1']['state'],'published')
 def test_v2_published_journal_migrates_without_reexecuting_completed_command(self):
  value=job(action='run_command');self.private.jobs=[value]
  old={'state':'published','jobHash':u.hashed(value),'action':'run_command','createdAt':value['createdAt'],'result':{'outcome':'completed','data':{'succeeded':True,'stdout':'PRIVATE_OLD'}}}
  u.atomic(self.home/'state/universal_journal.json',{'general1':old})
  worker=self.worker();worker.tick()
  self.assertEqual(self.calls,['capabilities']);self.assertEqual(worker.journal['general1'],old)
  self.assertEqual(worker.status()['jobs'][0]['state'],'completed')
 def test_private_channel_must_remain_private(self):
  self.private.private=False
  with self.assertRaises(u.UniversalError):self.worker().tick()
  self.assertEqual(self.calls,[]);self.assertEqual(self.private.writes,[])
 def test_wrong_device_never_executes(self):
  self.private.jobs=[job(device='device2')];self.worker().tick()
  self.assertEqual(self.calls,['capabilities'])
 def test_public_publish_failure_retries_only_result(self):
  worker=self.worker();self.public.fail=True
  with self.assertRaises(RuntimeError):worker.tick()
  self.public.fail=False;worker.tick()
  self.assertEqual(self.calls.count('read_file'),1);self.assertEqual(worker.journal['general1']['state'],'published')
 def test_ambiguous_crash_does_not_replay_write(self):
  value=job(action='write_file');self.private.jobs=[value]
  worker=self.worker();worker.journal[value['id']]={'state':'started','jobHash':u.hashed(value),'action':'write_file','createdAt':value['createdAt']};worker.checkpoint()
  worker.pool.shutdown(wait=True);worker=self.worker();worker.tick();self.assertNotIn('write_file',self.calls)
  self.assertEqual(worker.journal['general1']['result']['outcome'],'interrupted')
 def test_disconnected_does_not_fetch_or_execute(self):
  u.atomic(self.home/'state/disconnected.flag',{});self.worker().tick();self.assertEqual(self.calls,[])
 def test_queue_validation_and_reused_id(self):
  worker=self.worker();worker.tick();self.private.jobs[0]['args']['path']='D:/changed'
  with self.assertRaises(u.UniversalError):worker.tick()
  for row in [dict(job(),deviceId='*'),dict(job(),target='Mushoku-Audiobook'),dict(job(),action='unknown')]:
   with self.assertRaises(u.UniversalError):u.validate_job(row,'device1')
 def test_nonzero_command_keeps_private_output_and_blocks_dependents(self):
  value=job(action='run_command');after=job('later');after['dependsOn']=['general1'];self.private.jobs=[value,after]
  def execute(job,config,state):
   if job['action']=='capabilities':return {}
   return {'stage':'completed','returnCode':1,'succeeded':False,'stderr':'PRIVATE_FAILURE'}
  worker=u.UniversalWorker(self.home,self.private,self.public,execute);worker.tick()
  self.assertEqual(worker.journal['general1']['result']['outcome'],'failed')
  self.assertEqual(worker.journal['general1']['result']['data']['stderr'],'PRIVATE_FAILURE')
  self.assertNotIn('PRIVATE_FAILURE',json.dumps(self.public.values))
  self.assertEqual(worker.journal['later']['result']['data']['code'],'general_prerequisite_failed')
 def test_missing_and_failed_dependencies_do_not_execute(self):
  second=job('after');second['dependsOn']=['missing'];self.private.jobs=[second]
  worker=self.worker();worker.tick();self.assertEqual(self.calls,['capabilities'])
  worker.journal['missing']={'state':'published','jobHash':'a','result':{'outcome':'failed'}}
  worker.tick();self.assertEqual(worker.journal['after']['result']['data']['code'],'general_prerequisite_failed')
  self.assertEqual(self.calls,['capabilities'])
if __name__=='__main__':unittest.main()
