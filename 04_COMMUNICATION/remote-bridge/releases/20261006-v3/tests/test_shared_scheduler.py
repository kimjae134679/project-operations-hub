import datetime as dt,json,sys,tempfile,threading,time,unittest
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parents[1]))
import universal_worker as u

def job(identity,action='capabilities',args=None,**extra):
 at=dt.datetime.now(dt.timezone.utc)
 return dict(id=identity,target='PC',deviceId='test-device',action=action,args=args or {},createdAt=at.isoformat(),expiresAt=(at+dt.timedelta(days=1)).isoformat(),**dict({'toolId':'test-client'},**extra))

class SchedulerTests(unittest.TestCase):
 def setUp(self):
  self.tmp=tempfile.TemporaryDirectory();self.addCleanup(self.tmp.cleanup);self.home=Path(self.tmp.name)
  u.atomic(self.home/'config.json',{'deviceId':'test-device','maxWorkers':4});self.workers=[]
 def tearDown(self):
  for w in self.workers:w.stop();w.pool.shutdown(wait=True,cancel_futures=True)
 def worker(self,executor):
  w=u.UniversalWorker(self.home,executor=executor);self.workers.append(w);return w
 def drain(self,w,timeout=3):
  until=time.monotonic()+timeout
  while time.monotonic()<until:
   w.pump()
   if not w.active and not any(r['state']=='queued' for r in w.journal.values()):return
   time.sleep(.01)
  self.fail('scheduler did not drain')
 def test_independent_projects_execute_in_parallel_and_remain_bounded(self):
  gate=threading.Event();entered=threading.Event();lock=threading.Lock();current=0;peak=0
  def execute(*unused):
   nonlocal current,peak
   with lock:current+=1;peak=max(peak,current);entered.set() if current==4 else None
   gate.wait(2)
   with lock:current-=1
   return {}
  w=self.worker(execute)
  for n in range(8):w.submit(job('j'+str(n),projectId='Project'+str(n)))
  w.pump();self.assertTrue(entered.wait(1));self.assertEqual(len(w.active),4)
  gate.set();self.drain(w);self.assertEqual(peak,4);self.assertEqual(len(w.journal),8)
 def test_ui_and_same_file_are_serialized_but_other_resources_progress(self):
  first=threading.Event();release=threading.Event();calls=[]
  def execute(j,*unused):
   calls.append(j['id'])
   if j['id']=='ui1':first.set();release.wait(2)
   return {'ok':True}
  w=self.worker(execute);w.submit(job('ui1','ui_control'));w.submit(job('ui2','ui_control'));w.submit(job('other'))
  w.pump();self.assertTrue(first.wait(1));time.sleep(.04);self.assertIn('other',calls);self.assertNotIn('ui2',calls)
  release.set();self.drain(w);self.assertIn('ui2',calls)
  self.assertTrue(u.resource_keys(job('w','write_file',{'path':str(self.home/'x')}))&u.resource_keys(job('r','read_file',{'path':str(self.home/'x')})))
 def test_dependency_waits_for_completion_and_failed_parent_blocks_child(self):
  calls=[]
  def execute(j,*unused):calls.append(j['id']);return {'succeeded':False} if j['id']=='parent' else {}
  w=self.worker(execute);w.submit(job('child',dependsOn=['parent']));w.submit(job('parent','run_command'))
  self.drain(w);self.assertEqual(calls,['parent']);self.assertEqual(w.journal['child']['result']['data']['code'],'general_prerequisite_failed')
 def test_cycle_rejected_without_corrupting_existing_queue(self):
  w=self.worker(lambda *args:{});w.submit(job('a',dependsOn=['b']))
  with self.assertRaisesRegex(u.UniversalError,'dependency_cycle'):w.submit(job('b',dependsOn=['a']))
  self.assertEqual(list(w.journal),['a'])
 def test_idempotence_and_reused_id_refusal(self):
  calls=[];w=self.worker(lambda j,*args:calls.append(j['id']) or {});j=job('one')
  self.assertFalse(w.submit(j)['duplicate']);self.assertTrue(w.submit(j)['duplicate']);self.drain(w)
  self.assertTrue(w.submit(j)['duplicate']);self.assertEqual(calls,['one'])
  with self.assertRaisesRegex(u.UniversalError,'general_job_id_reused'):w.submit(dict(j,args={'changed':True}))
 def test_started_recovery_never_reexecutes_mutation_queued_survives(self):
  j=job('ambiguous','write_file');q=job('waiting')
  u.atomic(self.home/'state/universal_journal.json',{'ambiguous':{'state':'started','jobHash':u.hashed(j),'job':j,'action':'write_file','source':'local'},'waiting':{'state':'queued','jobHash':u.hashed(q),'job':q,'action':'capabilities','source':'local'}})
  calls=[];w=self.worker(lambda j,*args:calls.append(j['id']) or {});self.drain(w)
  self.assertEqual(calls,['waiting']);self.assertEqual(w.journal['ambiguous']['result']['outcome'],'interrupted')
 def test_concurrent_submit_persists_all_jobs_without_temp_collisions(self):
  w=self.worker(lambda *args:{})
  with ThreadPoolExecutor(max_workers=8) as pool:list(pool.map(lambda n:w.submit(job('concurrent'+str(n))),range(40)))
  self.assertEqual(len(u.read(w.journal_path)),40);self.assertEqual(len(w.journal),40)
 def test_pause_holds_new_jobs_and_stop_does_not_replay(self):
  calls=[];w=self.worker(lambda j,*args:calls.append(j['id']) or {});(w.state/'disconnected.flag').write_text('pause')
  w.submit(job('hold'));w.pump();self.assertEqual(calls,[]);self.assertEqual(w.status()['stage'],'disconnected')
  (w.state/'disconnected.flag').unlink();self.drain(w);self.assertEqual(calls,['hold'])
 def test_status_has_no_private_args_or_result_contents(self):
  w=self.worker(lambda *args:{'private':'SECRET_RESULT'});w.submit(job('safe','read_file',{'path':'PRIVATE_PATH'}));self.drain(w)
  text=json.dumps(w.status());self.assertNotIn('SECRET_RESULT',text);self.assertNotIn('PRIVATE_PATH',text)
  for key in ('version','deviceId','localReady','relayConnected','parallelLimit','activeJobCount'):self.assertIn(key,w.status())
 def test_expired_waiting_dependency_fails_without_running_and_duplicate_is_cached(self):
  calls=[];w=self.worker(lambda j,*args:calls.append(j['id']) or {});j=job('expires',dependsOn=['missing']);w.submit(j)
  w.journal['expires']['job']['expiresAt']='2000-01-01T00:00:00Z';w.pump()
  self.assertEqual(calls,[]);self.assertEqual(w.journal['expires']['result']['outcome'],'failed')
  # Replay the original request against its original hash, even after expiry.
  self.assertTrue(w.submit(j)['duplicate'])
 def test_project_and_issuer_workspaces_and_journals_are_separate(self):
  seen=[]
  def execute(j,config,state):seen.append((j,state));return {'succeeded':True}
  w=self.worker(execute)
  for project,issuer in [('One','Codex-a'),('One','Codex-b'),('Two','Codex-a')]:w.submit(job(project+'-'+issuer,'run_command',{'python':'print(1)'},projectId=project,toolId=issuer))
  self.drain(w);self.assertEqual(len({j['args']['cwd'] for j,state in seen}),3);self.assertEqual(len({state for j,state in seen}),3)
  for j,state in seen:self.assertTrue((state/'journal'/(j['id']+'.json')).exists())
 def test_legacy_tickets_remain_accessible_only_to_legacy_default_issuer(self):
  w=self.worker(lambda *args:{});pid='a'*32;ticket=w.state/'general/processes'/pid/'ticket.json';u.atomic(ticket,{'processId':pid})
  legacy=job('old-status','process_status',{'processId':pid},toolId='ProjectBridge',projectId='Control-Tower')
  self.assertEqual(w.scoped_job(legacy)[1],w.state/'general')
  other=dict(legacy,toolId='Codex-other');self.assertNotEqual(w.scoped_job(other)[1],w.state/'general')
  other=dict(legacy,projectId='Other');self.assertNotEqual(w.scoped_job(other)[1],w.state/'general')
if __name__=='__main__':unittest.main()
