"""Regression checks without activating a user window or injecting input."""
import json, os, subprocess, sys, tempfile, unittest
from pathlib import Path
from unittest.mock import Mock, patch
sys.path.insert(0,str(Path(__file__).parents[1]))
import universal_worker as u
import tool_catalog
from version import VERSION

class FocusDiagnostics(unittest.TestCase):
 def test_catalog_limits_focus_failure_and_prevents_stale_input(self):
  catalog={x['name']:x for x in tool_catalog.additions()}
  self.assertIn('not a global permission denial',catalog['pc_focus_window']['description'])
  self.assertIn('Do not ask for repeated manual clicks',catalog['pc_focus_window']['description'])
  for action in ('mouse_click','type_text','send_keys'):
   self.assertIn('expectedHwnd',catalog['pc_'+action]['description'])
   self.assertIn('no input was sent',catalog['pc_'+action]['description'])
 def test_versions_have_one_source(self):
  from bridge_worker import VERSION as a
  from local_api import VERSION as b
  from bridge_mcp import VERSION as c
  self.assertEqual({VERSION,u.VERSION,a,b,c},{'3.2.2'})
 def test_legacy_queue_not_polled_or_published_by_default_loop(self):
  from bridge_worker import legacy_queue_status
  worker=Mock();legacy_queue_status(worker)
  worker.tick.assert_not_called();worker.publish_pending.assert_not_called()
  worker.ui.assert_called_once_with('connected',code='legacy_public_queue_retired')
 def test_atomic_metadata_does_not_append_uuid_to_long_basename(self):
  import process_runner
  with tempfile.TemporaryDirectory() as temp:
   folder=Path(temp);path=folder/('a'*180+'.json')
   process_runner.atomic(path,{'saved':True})
   self.assertEqual(json.loads(path.read_text()),{'saved':True})
   self.assertEqual(list(folder.iterdir()),[path])
 def test_recovery_checkpoints_large_journal_once_per_batch(self):
  with tempfile.TemporaryDirectory() as temp:
   home=Path(temp);u.atomic(home/'config.json',{'deviceId':'fixture-device'})
   records={}
   for n in range(20):
    jid='batch-'+str(n);job={'id':jid,'action':'start_process','args':{}}
    accepted={'outcome':'completed','data':{'processId':format(n,'032x'),'stage':'running','done':False}}
    records[jid]={'state':'finished','action':'start_process','job':job,'jobHash':'a'*64,'result':accepted}
   u.atomic(home/'state/universal_journal.json',records)
   terminal={'stage':'completed','done':True,'running':False,'succeeded':True}
   with patch.object(u.UniversalWorker,'live_process',return_value=terminal),patch.object(u.UniversalWorker,'checkpoint',autospec=True) as checkpoint:
    worker=u.UniversalWorker(home)
    try:
     self.assertLessEqual(checkpoint.call_count,2)
     self.assertTrue(all(r['processFinished'] for r in worker.journal.values()))
    finally:worker.stop();worker.pool.shutdown(wait=True,cancel_futures=True)
 def test_private_delivery_never_automatically_posts_to_public_hub(self):
  with tempfile.TemporaryDirectory() as temp:
   home=Path(temp);u.atomic(home/'config.json',{'deviceId':'fixture-device'})
   private=Mock();public=Mock();private.request.return_value={'private':True}
   worker=u.UniversalWorker(home,private,public)
   try:
    row={'source':'github','state':'finished','action':'capabilities','result':{'outcome':'completed'}}
    self.assertTrue(worker.publish('fixture-job',row))
    private.publish.assert_called_once();public.publish.assert_not_called()
   finally:worker.stop();worker.pool.shutdown(wait=True,cancel_futures=True)
 def test_history_compaction_preserves_full_result_and_duplicate_identity(self):
  with tempfile.TemporaryDirectory() as temp:
   home=Path(temp);u.atomic(home/'config.json',{'deviceId':'fixture-device'})
   worker=u.UniversalWorker(home)
   try:
    job={'id':'old-large','action':'run_command','args':{'python':'print(1)'},'projectId':'A','toolId':'B'}
    row={'source':'local','state':'finished','action':'run_command','job':job,'jobHash':u.hashed(job),'result':{'outcome':'completed','data':{'stdout':'preserved-value'*10000,'succeeded':True}}}
    worker.journal['old-large']=row
    worker.save_record('old-large',row)
    for n in range(100):worker.journal['recent-'+str(n)]={'state':'finished','source':'local'}
    with patch.object(worker,'save_record',wraps=worker.save_record) as save:
     worker.checkpoint();save.assert_not_called()
    self.assertIn('historyArchive',worker.journal['old-large'])
    self.assertLess(worker.journal_path.stat().st_size,20000)
    self.assertEqual(worker.get_job('old-large')['result'],row['result'])
    self.assertTrue(worker.submit(job)['duplicate'])
    worker.save_record('old-large',worker.journal['old-large'])
    self.assertEqual(worker.get_job('old-large')['result'],row['result'])
    archive=worker.scope(job)[0]/'journal/old-large.json';archive.write_bytes(b'{}')
    with self.assertRaisesRegex(u.UniversalError,'history_archive_hash_mismatch'):worker.get_job('old-large')
   finally:worker.stop();worker.pool.shutdown(wait=True,cancel_futures=True)
 @unittest.skipUnless(os.name=='nt','Windows compiler')
 def test_native_diagnostic_contract_and_version_without_foreground_action(self):
  runtime=Path(__file__).parents[1]
  compiler=Path(os.environ['WINDIR'])/'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
  source='''using System;using System.Reflection;using System.Web.Script.Serialization;
class Contract {static void Main(string[] a) {
var type=Assembly.LoadFrom(a[0]).GetType("DesktopAutomation");
var method=type.GetMethod("FocusFailureDetails",BindingFlags.NonPublic|BindingFlags.Static);
Console.Write(new JavaScriptSerializer().Serialize(method.Invoke(null,new object[]{"123","456"})));
}}'''
  with tempfile.TemporaryDirectory() as temp:
   folder=Path(temp);helper=folder/'helper.exe';harness=folder/'contract.exe';cs=folder/'contract.cs';cs.write_text(source)
   flags=['/nologo','/r:System.Windows.Forms.dll','/r:System.Drawing.dll','/r:System.Web.Extensions.dll']
   for target,src,kind in [(helper,runtime/'DesktopAutomation.cs','winexe'),(harness,cs,'exe')]:
    result=subprocess.run([str(compiler),*flags,'/target:'+kind,'/out:'+str(target),str(src)],capture_output=True,text=True,creationflags=subprocess.CREATE_NO_WINDOW,timeout=20)
    self.assertEqual(result.returncode,0,result.stdout+result.stderr)
   result=subprocess.run([str(harness),str(helper)],capture_output=True,text=True,creationflags=subprocess.CREATE_NO_WINDOW,timeout=10)
   self.assertEqual(result.returncode,0,result.stderr);details=json.loads(result.stdout)
   self.assertEqual(details['scope'],'window_activation');self.assertFalse(details['policy_denied'])
   self.assertFalse(details['automatic_retry']);self.assertTrue(details['other_pc_operations_available'])
   self.assertEqual((details['expected_hwnd'],details['observed_hwnd']),('123','456'))
   result=subprocess.run([str(helper)],input=json.dumps({'action':'focus_window','args':{'hwnd':'0'}}),capture_output=True,text=True,encoding='utf-8',creationflags=subprocess.CREATE_NO_WINDOW,timeout=10)
   body=json.loads(result.stdout);self.assertFalse(body['ok']);self.assertEqual(body['error']['code'],'invalid_arguments')

if __name__=='__main__':unittest.main()
