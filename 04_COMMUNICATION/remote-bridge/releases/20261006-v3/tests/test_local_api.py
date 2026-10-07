import json,sys,tempfile,time,unittest,urllib.request,urllib.error
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parents[1]))
import universal_worker as u
from local_api import LocalServer
from test_shared_scheduler import job
from bridge_mcp import Adapter
class LocalApiTests(unittest.TestCase):
 def setUp(self):
  self.tmp=tempfile.TemporaryDirectory();self.addCleanup(self.tmp.cleanup);self.home=Path(self.tmp.name)
  u.atomic(self.home/'config.json',{'deviceId':'test-device','maxWorkers':4})
  self.worker=u.UniversalWorker(self.home,executor=lambda *args:{'privateResult':'ONLY_LOCAL'})
  self.server=LocalServer(self.worker);self.endpoint=u.read(self.worker.state/'local_endpoint.json')
 def tearDown(self):self.server.close();self.worker.stop();self.worker.pool.shutdown(wait=True,cancel_futures=True)
 def request(self,path,data=None,auth=True,origin=None):
  headers={'Content-Type':'application/json'}
  if auth:headers['X-ProjectBridge-Token']=self.endpoint['token']
  if origin:headers['Origin']=origin
  raw=None if data is None else json.dumps(data).encode()
  with urllib.request.urlopen(urllib.request.Request(self.endpoint['baseUrl']+path,data=raw,headers=headers),timeout=2) as response:return json.load(response)
 def test_auth_required_and_all_browser_origins_refused(self):
  for auth,origin in ((False,None),(True,'null'),(True,'https://malicious.example')):
   with self.assertRaises(urllib.error.HTTPError) as error:self.request('/v1/status',auth=auth,origin=origin)
   self.assertEqual(error.exception.code,403)
 def test_local_job_roundtrip_without_github(self):
  j=job('offline');accepted=self.request('/v1/jobs',j);self.assertTrue(accepted['accepted'])
  self.worker.pump();time.sleep(.03);self.worker.pump()
  result=self.request('/v1/jobs/offline');self.assertEqual(result['result']['data']['privateResult'],'ONLY_LOCAL')
  status=self.request('/v1/status');self.assertTrue(status['localReady']);self.assertFalse(status['relayConnected']);self.assertNotIn('ONLY_LOCAL',json.dumps(status))
 def test_wrong_device_and_unknown_endpoint_rejected(self):
  j=job('wrong');j['deviceId']='other'
  with self.assertRaises(urllib.error.HTTPError) as error:self.request('/v1/jobs',j)
  self.assertEqual(error.exception.code,400)
  with self.assertRaises(urllib.error.HTTPError):self.request('/not-a-route')
 def test_pause_and_resume_require_authenticated_control(self):
  self.request('/v1/control',{'action':'pause'});self.assertTrue((self.worker.state/'disconnected.flag').exists())
  self.request('/v1/control',{'action':'resume'});self.assertFalse((self.worker.state/'disconnected.flag').exists())
 def test_mcp_initialize_tools_and_duplicate_job_keep_original_identity(self):
  adapter=Adapter(self.home);reply=adapter.handle({'jsonrpc':'2.0','id':1,'method':'initialize','params':{'protocolVersion':'2025-03-26'}})
  self.assertEqual(reply['result']['protocolVersion'],'2025-03-26')
  self.assertEqual(len(adapter.handle({'id':2,'method':'tools/list'})['result']['tools']),3)
  args={'id':'mcp-once','action':'capabilities','args':{}}
  self.assertFalse(adapter.call('pc_submit',args)['duplicate']);self.assertTrue(adapter.call('pc_submit',args)['duplicate'])
 def test_mcp_connections_receive_distinct_issuer_ids_unless_explicitly_set(self):
  one=Adapter(self.home);two=Adapter(self.home)
  self.assertNotEqual(one.tool_id,two.tool_id)
  self.assertEqual(Adapter(self.home,'Codex-audiobook').tool_id,'Codex-audiobook')
 def test_local_submission_without_issuer_is_refused(self):
  value=job('missing-issuer');del value['toolId']
  with self.assertRaises(urllib.error.HTTPError) as error:self.request('/v1/jobs',value)
  self.assertEqual(error.exception.code,400)
if __name__=='__main__':unittest.main()
