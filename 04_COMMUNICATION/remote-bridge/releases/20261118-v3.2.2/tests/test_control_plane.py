import copy,datetime as dt,json,os,sys,tempfile,threading,time,unittest
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parents[1]))
import universal_worker as u
import universal_actions as a
from bridge_mcp import Adapter,TOOLS
from control_plane import Manager,next_daily,next_after,validate_timing,UTC,utcnow
from tool_catalog import check,MANAGEMENT,validate_operation

class ControlTests(unittest.TestCase):
 def setUp(self):
  self.tmp=tempfile.TemporaryDirectory();self.addCleanup(self.tmp.cleanup);self.home=Path(self.tmp.name)
  u.atomic(self.home/'config.json',{'deviceId':'control-fixture','python':sys.executable,'protectedRoots':[]})
  self.w=u.UniversalWorker(self.home);self.addCleanup(self.cleanup);self.m=self.w.manager
  self.scope={'projectId':'Test','toolId':'client'}
 def cleanup(self):self.w.stop();self.w.pool.shutdown(wait=True,cancel_futures=True)
 def call(self,name,**args):return self.m.call(name,dict(self.scope,**args))
 def task(self,task_id='test-task',steps=None,**extra):
  steps=steps or [{'id':'one','action':'capabilities','args':{}}]
  return self.call('pc_task_put',taskId=task_id,title='fixture task',tags=['fixture'],steps=steps,expectedRevision=0,**extra)
 def drain(self):
  deadline=time.monotonic()+20
  while time.monotonic()<deadline:
   self.w.pump()
   if not self.w.active and not any(r['state']=='queued' for r in self.w.journal.values()):return
   time.sleep(.02)
  self.fail('worker timeout')
 def test_task_revision_scope_and_restart_persistence(self):
  self.task()
  with self.assertRaisesRegex(ValueError,'revision_conflict'):self.task()
  self.assertEqual(self.call('pc_tasks_list')['tasks'][0]['revision'],1)
  other=self.m.call('pc_tasks_list',{'projectId':'Other','toolId':'client'});self.assertEqual(other['tasks'],[])
  restored=Manager(self.w);self.assertEqual(restored.call('pc_task_get',dict(self.scope,taskId='test-task'))['revision'],1)
  self.call('pc_task_delete',taskId='test-task',expectedRevision=1);self.assertEqual(self.call('pc_tasks_list')['tasks'],[])
 def test_ordered_workflow_and_idempotent_replay(self):
  path=self.home/'out.txt'
  self.task(steps=[{'id':'write','action':'write_file','args':{'path':str(path),'expectedSha256':None,'contentUtf8':'done'}},{'id':'read','action':'read_file','args':{'path':str(path)},'dependsOn':['write']}])
  result=self.call('pc_task_run',taskId='test-task',runId='run-a');self.assertEqual(result['progress']['totalSteps'],2)
  self.drain();result=self.call('pc_run_status',runId='run-a');self.assertEqual(result['state'],'completed');self.assertEqual(result['progress']['percent'],100)
  self.assertEqual(self.call('pc_task_run',taskId='test-task',runId='run-a')['state'],'completed');self.assertEqual(len(self.w.journal),2)
  self.assertEqual(path.read_text(),'done')
 def test_bad_dependency_and_operation_rejected_before_save(self):
  with self.assertRaisesRegex(ValueError,'prior_steps'):self.task(steps=[{'id':'one','action':'capabilities','args':{},'dependsOn':['later']}])
  with self.assertRaisesRegex(ValueError,'required_argument_missing'):self.task(steps=[{'id':'one','action':'write_file','args':{'path':str(self.home/'x')}}])
  with self.assertRaisesRegex(ValueError,'unknown_argument'):self.call('pc_tasks_list',surprise=1)
 def test_failed_step_blocks_child(self):
  self.task(steps=[{'id':'bad','action':'read_file','args':{'path':str(self.home/'missing')}},{'id':'after','action':'write_file','args':{'path':str(self.home/'should-not-exist'),'expectedSha256':None,'contentUtf8':'wrong'},'dependsOn':['bad']}])
  self.call('pc_task_run',taskId='test-task',runId='failed');self.drain()
  self.assertEqual(self.call('pc_run_status',runId='failed')['state'],'failed');self.assertFalse((self.home/'should-not-exist').exists())
 def test_job_list_filter_stable_pagination_and_queued_cancel(self):
  self.task()
  for index in range(5):self.call('pc_task_run',taskId='test-task',runId='list-'+str(index))
  first=self.call('pc_jobs_list',limit=2,tag='fixture');self.assertEqual(len(first['jobs']),2)
  second=self.call('pc_jobs_list',limit=2,tag='fixture',cursor=first['nextCursor'])
  self.assertFalse({r['id'] for r in first['jobs']} & {r['id'] for r in second['jobs']})
  with self.assertRaisesRegex(ValueError,'filter_conflict'):self.call('pc_jobs_list',cursor=first['nextCursor'],query='different')
  jid=first['jobs'][0]['id'];self.assertEqual(self.call('pc_job_cancel',id=jid)['state'],'cancelled')
  self.assertTrue(self.call('pc_job_wait',id=jid,waitSeconds=0)['done'])
  self.assertEqual(len(self.call('pc_jobs_list',state='cancelled')['jobs']),1)
 def test_wait_deadline_and_foreign_scope_refusal(self):
  self.task();self.call('pc_task_run',taskId='test-task',runId='pending')
  start=time.monotonic();result=self.call('pc_job_wait',id='pending-one',waitSeconds=0);self.assertFalse(result['done']);self.assertLess(time.monotonic()-start,.5)
  with self.assertRaisesRegex(ValueError,'not_found_in_scope'):self.m.call('pc_job_wait',{'id':'pending-one','waitSeconds':0,'projectId':'Other','toolId':'client'})
 def test_alias_traversal_frozen_at_acceptance_and_update(self):
  folder=self.home/'folder';folder.mkdir();other=self.home/'other';other.mkdir()
  self.call('pc_alias_put',alias='output',path=str(folder),expectedRevision=0)
  self.task(steps=[{'id':'one','action':'write_file','args':{'path':'@output/a.txt','expectedSha256':None,'contentUtf8':'old location'}}])
  self.call('pc_task_run',taskId='test-task',runId='alias')
  self.call('pc_alias_put',alias='output',path=str(other),expectedRevision=1);self.drain()
  self.assertTrue((folder/'a.txt').exists());self.assertFalse((other/'a.txt').exists())
  with self.assertRaisesRegex(ValueError,'outside_root'):self.m.resolve_alias('@output/../../escape',self.m.scope(self.scope))
 def schedule(self,identity='schedule',**extra):
  return self.call('pc_schedule_put',scheduleId=identity,title='test schedule',taskId='test-task',expectedRevision=0,timing={'kind':'once','at':(utcnow()+dt.timedelta(seconds=10)).isoformat()},**extra)
 def test_once_schedule_snapshot_restart_and_no_duplicate(self):
  self.task();row=self.schedule()
  task=self.call('pc_task_get',taskId='test-task');self.call('pc_task_put',taskId='test-task',title='edited',steps=[{'id':'different','action':'capabilities','args':{}}],expectedRevision=1)
  self.w.manager=Manager(self.w);self.m=self.w.manager
  due=dt.datetime.fromisoformat(row['nextAt']);self.m.tick(at=due+dt.timedelta(seconds=1),force=True)
  scheduled=self.call('pc_schedules_list')['schedules'][0];self.assertEqual(scheduled['taskRevision'],1);self.assertIsNone(scheduled['nextAt'])
  run_id=scheduled['lastRunId'];self.drain();self.assertEqual(self.call('pc_run_status',runId=run_id)['taskRevision'],1)
  size=len(self.w.journal);self.m.tick(at=due+dt.timedelta(seconds=2),force=True);self.assertEqual(len(self.w.journal),size)
 def test_missed_skip_and_run_once(self):
  self.task();skip=self.schedule('skip',missedPolicy='skip',graceSeconds=0);catch=self.schedule('catch',missedPolicy='run_once',graceSeconds=0)
  self.m.tick(at=dt.datetime.fromisoformat(catch['nextAt'])+dt.timedelta(seconds=70),force=True)
  rows={r['scheduleId']:r for r in self.call('pc_schedules_list')['schedules']}
  self.assertIsNone(rows['skip']['lastRunId']);self.assertEqual(rows['skip']['lastDecision'],'missed_skipped');self.assertIsNotNone(rows['catch']['lastRunId'])
  self.drain()
 def test_pause_resume_and_revision_conflict(self):
  self.task();row=self.schedule(paused=True);due=dt.datetime.fromisoformat(row['nextAt'])
  self.m.tick(at=due+dt.timedelta(seconds=1),force=True);self.assertFalse(self.w.journal)
  self.call('pc_schedule_set',scheduleId='schedule',expectedRevision=1,paused=False)
  with self.assertRaisesRegex(ValueError,'revision_conflict'):self.call('pc_schedule_set',scheduleId='schedule',expectedRevision=1,paused=True)
  self.m.tick(at=due+dt.timedelta(seconds=2),force=True);self.assertTrue(self.w.journal);self.drain()
 def test_scheduled_desktop_actions_refused(self):
  self.task(steps=[{'id':'one','action':'ui_control','args':{'action':'mouse_click','args':{'x':1,'y':1}}}])
  with self.assertRaisesRegex(ValueError,'noninteractive'):self.schedule()
 def test_daily_weekday_korean_timezone_and_interval_math(self):
  timing={'kind':'daily','time':'09:00','timezone':'Asia/Seoul','daysOfWeek':[1]}
  after=dt.datetime(2026,10,11,23,59,tzinfo=UTC);self.assertEqual(next_daily(timing,after),dt.datetime(2026,10,12,0,0,tzinfo=UTC))
  due=utcnow();self.assertEqual(next_after({'kind':'interval','everySeconds':60},due,due+dt.timedelta(seconds=130)),due+dt.timedelta(seconds=180))
  with self.assertRaisesRegex(ValueError,'timezone|offset'):validate_timing({'kind':'once','at':'2026-10-13T12:00:00'},utcnow())
 def test_detached_dependency_waits_and_log_offsets(self):
  path=self.home/'after.txt'
  self.task(steps=[{'id':'start','action':'start_process','args':{'python':'import time; print("FIRST",flush=True); time.sleep(.5); print("SECOND",flush=True)','cwd':str(self.home),'timeoutSeconds':10}},{'id':'after','action':'write_file','args':{'path':str(path),'expectedSha256':None,'contentUtf8':'after'},'dependsOn':['start']}])
  self.call('pc_task_run',taskId='test-task',runId='logs');self.w.pump();time.sleep(.08);self.w.pump()
  self.assertFalse(path.exists())
  deadline=time.monotonic()+12
  while time.monotonic()<deadline:
   self.w.pump();result=self.call('pc_run_status',runId='logs')
   if result['done']:break
   time.sleep(.02)
  self.assertEqual(result['state'],'completed')
  first=self.call('pc_job_log',id='logs-start',maxBytes=5);second=self.call('pc_job_log',id='logs-start',offsetBytes=first['nextOffsetBytes'])
  self.assertIn('FIRST',first['contentUtf8']+second['contentUtf8']);self.assertIn('SECOND',second['contentUtf8'])
 def test_schedule_no_overlap_until_detached_child_ends(self):
  self.task(steps=[{'id':'one','action':'start_process','args':{'python':'import time; time.sleep(.6)','cwd':str(self.home),'timeoutSeconds':10}}])
  row=self.call('pc_schedule_put',scheduleId='interval',title='interval',taskId='test-task',expectedRevision=0,timing={'kind':'interval','everySeconds':60,'startAt':(utcnow()+dt.timedelta(seconds=10)).isoformat()})
  due=dt.datetime.fromisoformat(row['nextAt']);self.m.tick(at=due,force=True)
  self.w.pump();time.sleep(.06);self.w.pump()
  before=len(self.w.journal);self.m.tick(at=due+dt.timedelta(seconds=61),force=True);self.assertEqual(len(self.w.journal),before)
  deadline=time.monotonic()+12
  while time.monotonic()<deadline:
   self.w.pump()
   if self.call('pc_run_status',runId=self.call('pc_schedules_list')['schedules'][0]['lastRunId'])['done']:break
   time.sleep(.02)
 def test_directory_pages_search_filters_and_scope(self):
  folder=self.home/'files';folder.mkdir()
  for index in range(7):(folder/('asset'+str(index)+('.png' if index%2 else '.txt'))).write_text('fixture')
  state=self.home/'pages'
  first=a.perform({'action':'list_dir','args':{'path':str(folder),'limit':3}},self.w.config,state)
  second=a.perform({'action':'list_dir','args':{'path':str(folder),'limit':3,'cursor':first['nextCursor']}},self.w.config,state)
  self.assertFalse({r['name'] for r in first['entries']} & {r['name'] for r in second['entries']})
  with self.assertRaisesRegex(a.ActionError,'scope'):a.perform({'action':'list_dir','args':{'path':str(folder),'cursor':first['nextCursor']}},self.w.config,self.home/'otherpages')
  find=a.perform({'action':'find_files','args':{'path':str(folder),'pattern':'asset*','extensions':['.png'],'recursive':True}},self.w.config,state)
  self.assertEqual(len(find['entries']),3)
  (folder/'new.txt').write_text('change')
  with self.assertRaisesRegex(a.ActionError,'directory_changed'):a.perform({'action':'list_dir','args':{'path':str(folder),'cursor':first['nextCursor']}},self.w.config,state)

class RecoveryTests(unittest.TestCase):
 setUp=ControlTests.setUp
 cleanup=ControlTests.cleanup
 call=ControlTests.call
 task=ControlTests.task
 drain=ControlTests.drain
 schedule=ControlTests.schedule
 def test_partial_dispatch_restart_submits_each_step_once(self):
  self.task(steps=[{'id':'one','action':'capabilities','args':{}},{'id':'two','action':'capabilities','args':{},'dependsOn':['one']}])
  original=self.w.submit;calls=0
  def interrupted(job,*args,**kwargs):
   nonlocal calls
   calls+=1
   if calls==2:raise ValueError('fixture_dispatch_interrupted')
   return original(job,*args,**kwargs)
  self.w.submit=interrupted
  try:
   with self.assertRaisesRegex(ValueError,'fixture_dispatch_interrupted'):self.call('pc_task_run',taskId='test-task',runId='recover')
  finally:self.w.submit=original
  self.w.manager=Manager(self.w);self.m=self.w.manager;self.m.tick(force=True)
  self.drain();self.assertEqual(len(self.w.journal),2);self.assertEqual(self.call('pc_run_status',runId='recover')['state'],'completed')
 def test_workflow_history_and_cancel_queue(self):
  self.task()
  for index in range(3):self.call('pc_task_run',taskId='test-task',runId='history-'+str(index))
  page=self.call('pc_runs_list',limit=1);self.assertEqual(len(page['runs']),1)
  second=self.call('pc_runs_list',limit=1,cursor=page['nextCursor']);self.assertNotEqual(page['runs'][0]['runId'],second['runs'][0]['runId'])
  run=page['runs'][0]['runId'];result=self.call('pc_run_cancel',runId=run)
  self.assertEqual(len(result['cancelledQueuedIds']),1);self.assertFalse(result['runningJobsNotStopped'])
 def test_schedule_alias_snapshot_is_fixed(self):
  first=self.home/'first';first.mkdir();second=self.home/'second';second.mkdir()
  self.call('pc_alias_put',alias='out',path=str(first),expectedRevision=0)
  self.task(steps=[{'id':'one','action':'write_file','args':{'path':'@out/output.txt','expectedSha256':None,'contentUtf8':'fixed'}}])
  row=self.schedule();self.call('pc_alias_put',alias='out',path=str(second),expectedRevision=1)
  self.m.tick(at=dt.datetime.fromisoformat(row['nextAt']),force=True);self.drain()
  self.assertTrue((first/'output.txt').exists());self.assertFalse((second/'output.txt').exists())
 def test_cancel_partial_dispatch_never_enqueues_remaining_work(self):
  self.task(steps=[{'id':'one','action':'capabilities','args':{}},{'id':'two','action':'write_file','args':{'path':str(self.home/'never-write'),'expectedSha256':None,'contentUtf8':'bad'}}])
  original=self.w.submit;count=0
  def fail(job,*args,**kwargs):
   nonlocal count
   count+=1
   if count==2:raise ValueError('fixture_interrupted')
   return original(job,*args,**kwargs)
  self.w.submit=fail
  try:
   with self.assertRaisesRegex(ValueError,'fixture_interrupted'):self.call('pc_task_run',taskId='test-task',runId='cancel-partial')
  finally:self.w.submit=original
  self.call('pc_run_cancel',runId='cancel-partial');self.m.tick(force=True);self.drain()
  self.assertFalse((self.home/'never-write').exists());self.assertEqual(self.w.journal['cancel-partial-two']['result']['outcome'],'cancelled')
 def test_malformed_rpc_envelope_is_rejected_without_crash(self):
  adapter=Adapter(self.home,'client')
  for value in ([],None,{'method':'tools/call','params':[]}):self.assertEqual(adapter.handle(value)['error']['code'],-32600)

class ApiIntegration(unittest.TestCase):
 def test_live_authenticated_typed_tools_schedule_and_structured_output(self):
  with tempfile.TemporaryDirectory() as temp:
   home=Path(temp);u.atomic(home/'config.json',{'deviceId':'api-fixture','python':sys.executable,'protectedRoots':[]})
   w=u.UniversalWorker(home);w.start()
   try:
    deadline=time.monotonic()+5
    while not (home/'state/local_endpoint.json').exists() and time.monotonic()<deadline:time.sleep(.02)
    adapter=Adapter(home,'api-client')
    adapter.call('pc_task_put',{'taskId':'scheduled','title':'scheduled fixture','steps':[{'id':'write','action':'write_file','args':{'path':str(home/'scheduled.txt'),'expectedSha256':None,'contentUtf8':'scheduled success'}}],'expectedRevision':0})
    adapter.call('pc_schedule_put',{'scheduleId':'once','title':'once','taskId':'scheduled','expectedRevision':0,'timing':{'kind':'once','at':(utcnow()+dt.timedelta(seconds=1.2)).isoformat()}})
    deadline=time.monotonic()+10
    while not (home/'scheduled.txt').exists() and time.monotonic()<deadline:time.sleep(.03)
    self.assertEqual((home/'scheduled.txt').read_text(),'scheduled success')
    accepted=adapter.call('pc_file_info',{'path':str(home/'scheduled.txt'),'title':'file check','tags':['check']})
    row=adapter.call('pc_job_wait',{'id':accepted['id'],'waitSeconds':5});self.assertTrue(row['done'])
    listed=adapter.call('pc_jobs_list',{'query':'file check','tag':'check'});self.assertEqual(len(listed['jobs']),1)
    response=adapter.handle({'jsonrpc':'2.0','id':1,'method':'tools/call','params':{'name':'pc_schedules_list','arguments':{}}})
    self.assertIn('structuredContent',response['result'])
    with self.assertRaisesRegex(ValueError,'revision_conflict'):adapter.call('pc_task_put',{'taskId':'scheduled','title':'wrong','steps':[{'id':'a','action':'capabilities','args':{}}],'expectedRevision':0})
    with self.assertRaisesRegex(ValueError,'unknown_argument'):adapter.call('pc_file_info',{'path':str(home/'scheduled.txt'),'unknown':1})
   finally:w.stop();w.thread.join(timeout=5);w.pool.shutdown(wait=True,cancel_futures=True)
 def test_metadata_tools_have_strict_schemas_and_correct_hints(self):
  tools={row['name']:row for row in TOOLS}
  for name in ('pc_read_file','pc_job_wait','pc_task_put','pc_schedule_put','pc_mouse_drag'):
   self.assertFalse(tools[name]['inputSchema']['additionalProperties'])
  self.assertTrue(tools['pc_jobs_list']['annotations']['readOnlyHint'])
  self.assertFalse(tools['pc_schedule_put']['annotations']['readOnlyHint'])
  self.assertEqual(tools['pc_mouse_drag']['inputSchema']['properties']['durationMs']['maximum'],3000)

class NativeElementTests(unittest.TestCase):
 @unittest.skipUnless(os.name=='nt','Windows UI Automation')
 def test_real_offscreen_fixture_tree_and_mutation_foreground_guard(self):
  import subprocess
  runtime=Path(__file__).parents[1];compiler=Path(os.environ['WINDIR'])/'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
  source=r'''
using System;
using System.Drawing;
using System.Windows.Forms;
class Fixture : Form {
 protected override bool ShowWithoutActivation { get { return true; } }
 public Fixture() {
  Text="ProjectBridge controlled QA fixture";Name="BridgeFixture";ShowInTaskbar=false;
  StartPosition=FormStartPosition.Manual;Location=new Point(-30000,-30000);Size=new Size(400,200);
  var button=new Button();button.Text="Save fixture";button.Name="SaveButton";button.Location=new Point(10,10);Controls.Add(button);
  var text=new TextBox();text.Name="FixtureInput";text.AccessibleName="Fixture input";text.Location=new Point(10,50);text.Text="private fixture value";Controls.Add(text);
  var password=new TextBox();password.Name="FixturePassword";password.UseSystemPasswordChar=true;password.Location=new Point(10,90);password.Text="fixture password";Controls.Add(password);
  Shown+=delegate{Console.WriteLine(Handle.ToInt64());Console.Out.Flush();};
  var timer=new Timer();timer.Interval=20000;timer.Tick+=delegate{Close();};timer.Start();
 }
 [STAThread] static void Main(){Application.Run(new Fixture());}
}
'''
  with tempfile.TemporaryDirectory() as temp:
   folder=Path(temp);cs=folder/'Fixture.cs';cs.write_text(source);fixture=folder/'fixture.exe';helper=folder/'helper.exe'
   flags=['/nologo','/r:System.Windows.Forms.dll','/r:System.Drawing.dll','/r:System.Web.Extensions.dll']
   for inputs in [flags+['/target:exe','/out:'+str(fixture),str(cs)],flags+['/target:winexe','/out:'+str(helper),str(runtime/'DesktopAutomation.cs')]]:
    compiled=subprocess.run([str(compiler)]+inputs,capture_output=True,text=True,creationflags=subprocess.CREATE_NO_WINDOW,timeout=20)
    self.assertEqual(compiled.returncode,0,compiled.stdout+compiled.stderr)
   process=subprocess.Popen([str(fixture)],stdout=subprocess.PIPE,stderr=subprocess.PIPE,text=True,creationflags=subprocess.CREATE_NO_WINDOW)
   try:
    hwnd=process.stdout.readline().strip();self.assertTrue(hwnd.isdigit())
    def request(action,args):
     result=subprocess.run([str(helper)],input=json.dumps({'action':action,'args':args}),capture_output=True,text=True,encoding='utf-8',creationflags=subprocess.CREATE_NO_WINDOW,timeout=15)
     return json.loads(result.stdout)
    result=request('ui_elements',{'hwnd':hwnd,'limit':100,'depth':5})
    self.assertTrue(result['ok'],result)
    data=result['result'];rows=data['elements'];self.assertTrue(rows)
    button=next(row for row in rows if row['name']=='Save fixture')
    self.assertIn('ui_invoke',button['supportedActions'])
    self.assertLess(rows[0]['bounds']['x'],-10000,'QA fixture must remain offscreen')
    self.assertNotIn('private fixture value',json.dumps(data));self.assertNotIn('fixture password',json.dumps(data))
    self.assertTrue(any(row['password'] for row in rows))
    # This form was never focused, so actions must fail before touching any control.
    denied=request('ui_invoke',{'hwnd':hwnd,'elementId':button['elementId'],'expectedName':button['name'],'expectedProcessId':data['processId']})
    self.assertFalse(denied['ok']);self.assertEqual(denied['error']['code'],'foreground_window_changed')
   finally:process.terminate();process.wait(timeout=5);process.stdout.close();process.stderr.close()

if __name__=='__main__':unittest.main()
