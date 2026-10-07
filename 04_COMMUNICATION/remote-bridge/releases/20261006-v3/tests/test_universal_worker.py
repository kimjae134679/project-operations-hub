import datetime as dt,json,sys,tempfile,unittest
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parents[1]))
import universal_worker as u
from unittest.mock import Mock,patch
import base64,copy,types,urllib.parse
import bridge_worker as b

class Client:
 def __init__(self,jobs=None):self.jobs=jobs or [];self.private=True;self.values={};self.writes=[];self.fail=False
 def request(self,*args,**kwargs):return {'private':self.private}
 def read_bytes(self,path):return json.dumps({'schemaVersion':1,'jobs':self.jobs}).encode()
 def publish(self,path,value):
  if self.fail:raise RuntimeError('github_network_error')
  self.values[path]=value;self.writes.append((path,value))
 def upsert(self,path,value):self.values[path]=value

class ImmutableClient(Client):
 # Exercise the real append-only publisher; only the HTTP boundary is fake.
 def __init__(self,jobs=None):super().__init__(jobs);self.queue_reads=0;self.error=None
 def default_branch(self):return 'remote/pc-bridge'
 def read_bytes(self,path):
  if self.error:raise b.BridgeError(self.error)
  if path=='_pc_bridge/queue.json':self.queue_reads+=1;return super().read_bytes(path)
  if path not in self.values:raise b.BridgeError('github_http_404')
  return u.serialized(self.values[path])
 def publish(self,path,value):return b.GitHub._publish(self,path,value)
 def request(self,method,path,value=None,**kwargs):
  if self.error:raise b.BridgeError(self.error)
  if method=='PUT':
   key=urllib.parse.unquote(path[len('contents/'):]);result=json.loads(base64.b64decode(value['content']))
   self.values[key]=result;self.writes.append((key,result));return {}
  return {'private':self.private}

class LegacyPublicationConflicts(unittest.TestCase):
 def setUp(self):GeneralWorker.setUp(self);self.workers=[]
 def tearDown(self):
  for worker in getattr(self,'workers',[]):worker.stop();worker.pool.shutdown(wait=True,cancel_futures=True)
 def fixture(self):
  for worker in self.workers:worker.stop();worker.pool.shutdown(wait=True,cancel_futures=True)
  self.workers=[];value=job('legacy',action='start_process');key=u.hashed(value)
  accepted={'schemaVersion':1,'device':'device1','jobId':'legacy','jobSha256':key,'finishedAt':'2026-10-06T00:00:00+00:00','outcome':'completed','data':{'processId':'a'*32,'stage':'starting','running':True,'done':False,'succeeded':False}}
  old={'state':'published','source':'github','jobHash':key,'action':'start_process','job':value,'createdAt':value['createdAt'],'result':accepted}
  u.atomic(self.home/'state/universal_journal.json',{'legacy':old})
  self.private=ImmutableClient([job('fresh')]);self.private.values['_pc_bridge/results/device1/legacy.json']=copy.deepcopy(accepted)
  self.accepted=copy.deepcopy(accepted);self.public=Client()
  terminal={'processId':'a'*32,'stage':'completed','running':False,'done':True,'succeeded':True,'returnCode':0,'finishedAt':1791244800}
  with patch.object(u.UniversalWorker,'live_process',return_value=terminal):worker=self.restart()
  return worker
 def restart(self):
  worker=u.UniversalWorker(self.home,self.private,self.public,self.execute);self.workers.append(worker);return worker
 def test_known_legacy_conflict_does_not_starve_fresh_queue_or_replay(self):
  worker=self.fixture();worker.tick();worker.tick()
  self.assertEqual(self.calls.count('read_file'),1);self.assertNotIn('start_process',self.calls)
  self.assertTrue(worker.relay_available);self.assertGreaterEqual(self.private.queue_reads,2)
  self.assertEqual(self.private.values['_pc_bridge/results/device1/legacy.json'],self.accepted)
  self.assertEqual(worker.journal['legacy']['state'],'finished')
  self.assertEqual(worker.journal['legacy']['result']['outcome'],'completed')
  self.assertEqual(worker.journal['fresh']['state'],'published')
  self.assertFalse(any(path.endswith('/legacy.json') for path,value in self.private.writes))
  self.assertFalse(any(value.get('recordId')=='pc-bridge-legacy' for path,value in self.public.writes))
 def test_quarantine_persists_across_restart_without_claiming_publication(self):
  worker=self.fixture();worker.tick();before=copy.deepcopy(worker.journal['legacy'])
  worker.stop();worker.pool.shutdown(wait=True);worker=self.restart();worker.tick()
  self.assertEqual(worker.journal['legacy'],before);self.assertEqual(self.calls.count('read_file'),1)
  self.assertEqual(worker.status()['publicationConflictCount'],1)
  exposed=worker.status()['publicationConflicts'];self.assertEqual(exposed[0]['id'],'legacy')
  self.assertNotIn('PRIVATE_COMMAND',json.dumps(exposed));self.assertNotIn('args',json.dumps(exposed))
 def test_other_finished_result_publishes_after_known_conflict(self):
  worker=self.fixture();worker.submit(job('other'));worker.journal['other']['source']='github'
  worker.pump()
  while worker.active:worker.pump()
  worker.publish_pending();self.assertEqual(worker.journal['other']['state'],'published')
  self.assertEqual(worker.journal['legacy']['state'],'finished')
 def test_network_auth_and_unrecognized_conflicts_stay_fail_closed(self):
  for code in ('github_network_error','github_authentication_failed','github_permission_or_rate_limit'):
   with self.subTest(code=code):
    worker=self.fixture();self.private.error=code
    with self.assertRaisesRegex(b.BridgeError,code):worker.relay_tick()
    self.assertEqual(self.private.queue_reads,0);self.assertNotIn('publicationConflict',worker.journal['legacy'])
  worker=self.fixture();self.private.values['_pc_bridge/results/device1/legacy.json']['jobSha256']='b'*64
  with self.assertRaisesRegex(b.BridgeError,'remote_result_conflict'):worker.relay_tick()
  self.assertEqual(self.private.queue_reads,0);self.assertNotIn('publicationConflict',worker.journal['legacy'])
 def test_changed_local_result_invalidates_quarantine(self):
  worker=self.fixture();worker.tick();worker.journal['legacy']['result']['data']['returnCode']=9
  with self.assertRaisesRegex(u.UniversalError,'invalid_publication_conflict'):worker.relay_tick()
 def test_tampered_quarantine_digest_cannot_hide_publication(self):
  worker=self.fixture();worker.tick();worker.journal['legacy']['publicationConflict']['remoteResultSha256']='b'*64
  with self.assertRaisesRegex(u.UniversalError,'invalid_publication_conflict'):worker.relay_tick()
 def test_changed_relay_context_invalidates_quarantine(self):
  worker=self.fixture();worker.tick();worker.config['privateBranch']='another-branch'
  with self.assertRaisesRegex(u.UniversalError,'invalid_publication_conflict'):worker.relay_tick()
 def test_quarantine_skip_does_not_read_old_result_again(self):
  worker=self.fixture();worker.tick();worker.stop();worker.pool.shutdown(wait=True);worker=self.restart()
  original=self.private.read_bytes
  def read(path):
   if path.endswith('/legacy.json'):self.fail('verified quarantine re-read the historical result')
   return original(path)
  with patch.object(self.private,'read_bytes',side_effect=read):worker.tick()
  self.assertEqual(worker.journal['legacy']['state'],'finished')
 def test_job_detail_reports_quarantined_not_delivered(self):
  worker=self.fixture();worker.tick();detail=worker.get_job('legacy')
  self.assertEqual(detail['publication']['state'],'quarantined')
  self.assertEqual(detail['publication']['code'],'legacy_acceptance_result_conflict')
  self.assertEqual(detail['state'],'completed');self.assertEqual(worker.journal['legacy']['state'],'finished')
 def test_quarantine_status_is_bounded_but_total_count_is_exact(self):
  worker=self.fixture();worker.tick();template=copy.deepcopy(worker.journal['legacy'])
  for n in range(105):
   jid='old'+str(n);row=copy.deepcopy(template)
   for field in ('result','acceptedResult'):row[field]['jobId']=jid
   row['publicationConflict']['localResultSha256']=u.digest(u.serialized(row['result']))
   accepted=u.digest(u.serialized(row['acceptedResult']))
   row['publicationConflict']['acceptedResultSha256']=accepted;row['publicationConflict']['remoteResultSha256']=accepted
   worker.journal[jid]=row
  status=worker.status();self.assertEqual(status['publicationConflictCount'],106)
  self.assertEqual(len(status['publicationConflicts']),100);self.assertEqual(status['pendingPublicationCount'],0)
 def test_unrecognized_acceptance_identity_is_never_quarantined(self):
  for field in ('device','jobId','jobSha256'):
   with self.subTest(field=field):
    worker=self.fixture();worker.journal['legacy']['acceptedResult'][field]='changed'
    with self.assertRaisesRegex(b.BridgeError,'remote_result_conflict'):worker.relay_tick()
    self.assertEqual(self.private.queue_reads,0);self.assertNotIn('publicationConflict',worker.journal['legacy'])
 def test_public_conflict_is_never_swallowed_as_legacy_migration(self):
  worker=self.fixture();self.private.values['_pc_bridge/results/device1/legacy.json']=copy.deepcopy(worker.journal['legacy']['result'])
  with patch.object(self.public,'publish',side_effect=b.BridgeError('remote_result_conflict')):
   with self.assertRaisesRegex(b.BridgeError,'remote_result_conflict'):worker.relay_tick()
  self.assertNotIn('publicationConflict',worker.journal['legacy'])
 def test_matching_error_text_from_non_bridge_exception_stays_fail_closed(self):
  worker=self.fixture()
  with patch.object(self.private,'publish',side_effect=RuntimeError('remote_result_conflict')):
   with self.assertRaisesRegex(RuntimeError,'remote_result_conflict'):worker.relay_tick()
  self.assertNotIn('publicationConflict',worker.journal['legacy']);self.assertEqual(self.private.queue_reads,0)
 def test_actual_script_module_bridge_error_identity_is_supported(self):
  worker=self.fixture();module=types.ModuleType('script_bridge_fixture')
  module.BridgeError=type('BridgeError',(Exception,),{'__module__':module.__name__})
  relay_type=type('GitHub',(ImmutableClient,),{'__module__':module.__name__})
  worker.private.__class__=relay_type
  with patch.dict(sys.modules,{module.__name__:module}),patch.object(self.private,'publish',side_effect=module.BridgeError('remote_result_conflict')):
   worker.publish_pending()
  self.assertEqual(worker.get_job('legacy')['publication']['state'],'quarantined')
 def test_verified_quarantine_does_not_bypass_current_auth_check(self):
  worker=self.fixture();worker.tick();self.private.error='github_authentication_failed';before=self.private.queue_reads
  with self.assertRaisesRegex(b.BridgeError,'github_authentication_failed'):worker.relay_tick()
  self.assertEqual(self.private.queue_reads,before)
 def test_publication_batch_is_bounded_and_remaining_results_continue(self):
  worker=self.fixture();worker.tick()
  for n in range(101):
   jid='finished'+str(n);result={'schemaVersion':1,'device':'device1','jobId':jid,'jobSha256':'a'*64,'outcome':'completed','finishedAt':'2026-10-07T00:00:00+00:00','data':{}}
   worker.journal[jid]={'state':'finished','source':'github','action':'capabilities','result':result}
  worker.publish_pending()
  self.assertEqual(sum(worker.journal['finished'+str(n)]['state']=='published' for n in range(101)),100)
  self.assertEqual(worker.status()['pendingPublicationCount'],1)
  worker.publish_pending();self.assertEqual(worker.journal['finished100']['state'],'published')

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
