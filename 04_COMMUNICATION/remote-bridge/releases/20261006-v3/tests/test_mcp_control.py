import hashlib,json,os,subprocess,sys,tempfile,time,unittest
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parents[1]))
import universal_worker as u
from bridge_mcp import Adapter,TOOLS
from local_api import LocalServer

class McpControlTests(unittest.TestCase):
 def setUp(self):
  self.tmp=tempfile.TemporaryDirectory();self.addCleanup(self.tmp.cleanup);self.home=Path(self.tmp.name)
  u.atomic(self.home/'config.json',{'deviceId':'mcp-fixture','python':sys.executable,'protectedRoots':[]})
  self.worker=u.UniversalWorker(self.home);self.server=LocalServer(self.worker)
  self.adapter=Adapter(self.home,'plugin-fixture');self.counter=0
 def tearDown(self):
  self.server.close();self.worker.stop();self.worker.pool.shutdown(wait=True,cancel_futures=True)
 def call(self,action,args,successful=True):
  self.counter+=1;jid='fixture-'+str(self.counter)
  accepted=self.adapter.call('pc_submit',{'id':jid,'action':action,'args':args})
  self.assertTrue(accepted['accepted']);deadline=time.monotonic()+25
  while time.monotonic()<deadline:
   self.worker.pump();row=self.adapter.call('pc_result',{'id':jid})
   if row.get('result'):break
   time.sleep(.03)
  else:self.fail('Job did not finish')
  self.assertEqual(row['result']['outcome'],'completed' if successful else 'failed',row)
  return row['result']['data']
 def test_actual_file_lifecycle_through_authenticated_mcp(self):
  folder=self.home/'documents';self.call('make_dir',{'path':str(folder)})
  src=folder/'source.txt';dst=folder/'moved.txt';content='verified Korean text: 파일'
  result=self.call('write_file',{'path':str(src),'expectedSha256':None,'contentUtf8':content})
  digest=result['sha256'];self.assertEqual(digest,hashlib.sha256(content.encode()).hexdigest())
  read=self.call('read_file',{'path':str(src)});self.assertEqual(read['contentUtf8'],content)
  self.call('move_file',{'path':str(src),'destination':str(dst),'expectedSha256':digest})
  self.assertFalse(src.exists());self.assertTrue(dst.exists())
  removed=self.call('delete_file',{'path':str(dst),'expectedSha256':digest});self.assertFalse(dst.exists())
  self.call('restore_file',{'trashId':removed['trashId'],'expectedSha256':digest})
  self.assertEqual(dst.read_text(encoding='utf-8'),content)
 def test_actual_command_execution_not_just_heartbeat(self):
  result=self.call('run_command',{'python':'print("PLUGIN_CONTROL_OK")','cwd':str(self.home),'timeoutSeconds':10})
  self.assertTrue(result['succeeded']);self.assertIn('PLUGIN_CONTROL_OK',result['stdout'])
 def test_stdio_initialization_tool_metadata_and_status(self):
  requests=[{'jsonrpc':'2.0','id':1,'method':'initialize','params':{'protocolVersion':'2025-03-26'}},
   {'jsonrpc':'2.0','id':2,'method':'tools/list'},
   {'jsonrpc':'2.0','id':3,'method':'tools/call','params':{'name':'pc_status','arguments':{}}}]
  script=Path(__file__).parents[1]/'bridge_mcp.py'
  proc=subprocess.run([sys.executable,str(script),'--home',str(self.home),'--issuer','plugin-fixture'],
   input='\n'.join(json.dumps(x) for x in requests)+'\n',text=True,encoding='utf-8',capture_output=True,timeout=10,
   creationflags=0x08000000 if os.name=='nt' else 0)
  self.assertEqual(proc.returncode,0,proc.stderr);rows=[json.loads(x) for x in proc.stdout.splitlines()]
  self.assertEqual(len(rows),3);self.assertTrue(json.loads(rows[2]['result']['content'][0]['text'])['localReady'])
  tools=rows[1]['result']['tools'];self.assertEqual(len(tools),3)
  self.assertTrue(tools[0]['annotations']['readOnlyHint']);self.assertTrue(tools[1]['annotations']['destructiveHint'])
  self.assertIn('restore_file',tools[1]['inputSchema']['properties']['action']['enum'])
 def test_endpoint_cannot_redirect_token_to_remote_host(self):
  u.atomic(self.home/'state/local_endpoint.json',{'baseUrl':'https://example.com','token':'not-a-secret'})
  with self.assertRaisesRegex(ValueError,'invalid_local_endpoint'):self.adapter.call('pc_status',{})
 def test_authenticated_loopback_redirect_is_refused(self):
  import http.server,threading
  class Redirect(http.server.BaseHTTPRequestHandler):
   def log_message(self,*args):pass
   def do_GET(self):
    self.send_response(302);self.send_header('Location','https://example.invalid/steal-token');self.end_headers()
  server=http.server.ThreadingHTTPServer(('127.0.0.1',0),Redirect)
  thread=threading.Thread(target=server.serve_forever,daemon=True);thread.start()
  try:
   u.atomic(self.home/'state/local_endpoint.json',{'baseUrl':'http://127.0.0.1:'+str(server.server_port),'token':'not-a-secret'})
   with self.assertRaisesRegex(ValueError,'local_endpoint_redirect_refused'):self.adapter.call('pc_status',{})
  finally:server.shutdown();server.server_close();thread.join(timeout=1)
 def test_loopback_request_disables_proxy_handler(self):
  import io,urllib.request
  from unittest.mock import patch,Mock
  response=io.BytesIO(b'{}');opener=Mock();opener.open.return_value=response
  with patch('bridge_mcp.urllib.request.build_opener',return_value=opener) as build:
   self.adapter.call('pc_status',{})
  proxy=build.call_args.args[0];self.assertIsInstance(proxy,urllib.request.ProxyHandler)
  self.assertEqual(proxy.proxies,{})

if __name__=='__main__':unittest.main()
