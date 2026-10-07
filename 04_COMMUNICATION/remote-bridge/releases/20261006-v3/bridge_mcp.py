"""MCP stdio adapter for Codex/Jev/local clients; stdout contains JSON-RPC only."""
import argparse,datetime as dt,hashlib,json,sys,urllib.request,urllib.error,urllib.parse,uuid
from pathlib import Path

TOOLS=[{'name':'pc_status','description':'공용 PC 연결·작업 상태 확인','inputSchema':{'type':'object','properties':{},'additionalProperties':False}},
 {'name':'pc_submit','description':'파일·빌드·설치·장기 작업·화면 조작 제출. 작업 ID로 결과 조회.','inputSchema':{'type':'object','required':['action','args'],'properties':{'action':{'type':'string','enum':['capabilities','read_file','write_file','list_dir','run_command','start_process','process_status','stop_process','ui_control']},'args':{'type':'object'},'id':{'type':'string'},'projectId':{'type':'string'},'toolId':{'type':'string'},'dependsOn':{'type':'array','items':{'type':'string'}},'resourceKeys':{'type':'array','items':{'type':'string'}}},'additionalProperties':False}},
 {'name':'pc_result','description':'등록 작업 실제 결과 조회','inputSchema':{'type':'object','required':['id'],'properties':{'id':{'type':'string','pattern':'^[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}$'}},'additionalProperties':False}}]
TOOLS[1]['description']='Submit an authorized PC operation and poll pc_result(id). read_file/list_dir/make_dir: args.path absolute. write_file: path, expectedSha256 (null only for new file), contentUtf8 or contentBase64. move_file: path, destination, expectedSha256. delete_file: path, expectedSha256; returns recoverable trashId. restore_file: trashId, expectedSha256 in the same projectId/toolId scope. Commands: argv or python or powershell, optional cwd and timeoutSeconds. Use start_process for long work; process_status/stop_process require owned processId. Never treat accepted as completed.'
for tool in TOOLS:
 tool['annotations']={'readOnlyHint':tool['name']!='pc_submit','destructiveHint':tool['name']=='pc_submit','idempotentHint':tool['name']!='pc_submit','openWorldHint':False}
TOOLS[1]['inputSchema']['properties']['action']['enum'].extend(['make_dir','move_file','delete_file','restore_file'])

class NoRedirect(urllib.request.HTTPRedirectHandler):
 def redirect_request(self,*args,**kwargs):raise ValueError('local_endpoint_redirect_refused')

class Adapter:
 def __init__(self,home,tool_id=None):self.home=Path(home);self.tool_id=tool_id or 'mcp-'+uuid.uuid4().hex
 def request(self,path,data=None):
  endpoint=json.loads((self.home/'state/local_endpoint.json').read_text(encoding='utf-8'))
  # Local file configuration cannot redirect a secret token to a remote host.
  base=endpoint['baseUrl'];url=urllib.parse.urlparse(base)
  if url.scheme!='http' or url.hostname!='127.0.0.1' or not url.port or url.path:raise ValueError('invalid_local_endpoint')
  raw=None if data is None else json.dumps(data).encode()
  headers={'X-ProjectBridge-Token':endpoint['token'],'Content-Type':'application/json'}
  opener=urllib.request.build_opener(urllib.request.ProxyHandler({}),NoRedirect())
  with opener.open(urllib.request.Request(base+path,data=raw,headers=headers),timeout=10) as response:return json.load(response)
 def call(self,name,args):
  if name=='pc_status':return dict(self.request('/v1/status'),callerToolId=self.tool_id)
  if name=='pc_result':
   import re
   if set(args)!={'id'} or not re.fullmatch(r'[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}',args['id']):raise ValueError('invalid_job_id')
   return self.request('/v1/jobs/'+args['id'])
  if name=='pc_submit':
   status=self.request('/v1/status');at=dt.datetime.now(dt.timezone.utc)
   job={'id':args.get('id','local-'+uuid.uuid4().hex),'target':'PC','deviceId':status['device'],'action':args['action'],'args':args['args'],
    'createdAt':at.isoformat(),'expiresAt':(at+dt.timedelta(days=1)).isoformat(),'projectId':args.get('projectId','Control-Tower'),'toolId':args.get('toolId',self.tool_id)}
   for key in ('dependsOn','resourceKeys'):
    if key in args:job[key]=args[key]
   import re
   if not re.fullmatch(r'[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}',job['id']):raise ValueError('invalid_job_id')
   scope=hashlib.sha256(json.dumps([job['projectId'],job['toolId']]).encode()).hexdigest()[:24]
   spool=self.home/'state/mcp_requests'/scope/(job['id']+'.json');spool.parent.mkdir(parents=True,exist_ok=True)
   if spool.exists():
    prior=json.loads(spool.read_text(encoding='utf-8'))
    if {k:v for k,v in prior.items() if k not in ('createdAt','expiresAt')}!={k:v for k,v in job.items() if k not in ('createdAt','expiresAt')}:raise ValueError('job_id_reused')
    job=prior
   else:
    # Exclusive creation arbitrates multiple MCP processes sharing one ID.
    try:
     with spool.open('x',encoding='utf-8') as out:json.dump(job,out,ensure_ascii=False)
    except FileExistsError:
     prior=json.loads(spool.read_text(encoding='utf-8'))
     if {k:v for k,v in prior.items() if k not in ('createdAt','expiresAt')}!={k:v for k,v in job.items() if k not in ('createdAt','expiresAt')}:raise ValueError('job_id_reused')
     job=prior
   return self.request('/v1/jobs',job)
  raise ValueError('unknown_tool')
 def handle(self,message):
  method=message.get('method');rid=message.get('id');params=message.get('params',{})
  if rid is None:return None
  try:
   if method=='initialize':result={'protocolVersion':params.get('protocolVersion','2024-11-05'),'capabilities':{'tools':{}},'serverInfo':{'name':'ProjectBridge','version':'3.0.0'}}
   elif method=='ping':result={}
   elif method=='tools/list':result={'tools':TOOLS}
   elif method=='tools/call':
    try:data=self.call(params['name'],params.get('arguments',{}));result={'content':[{'type':'text','text':json.dumps(data,ensure_ascii=False)}]}
    except Exception:result={'isError':True,'content':[{'type':'text','text':'ProjectBridge 연결 또는 작업 요청을 확인하세요.'}]}
   else:return {'jsonrpc':'2.0','id':rid,'error':{'code':-32601,'message':'Method not found'}}
   return {'jsonrpc':'2.0','id':rid,'result':result}
  except Exception:return {'jsonrpc':'2.0','id':rid,'error':{'code':-32602,'message':'Invalid parameters'}}
def main():
 sys.stdin.reconfigure(encoding='utf-8');sys.stdout.reconfigure(encoding='utf-8')
 parser=argparse.ArgumentParser();parser.add_argument('--home',required=True);parser.add_argument('--issuer','--tool-id',dest='tool_id');args=parser.parse_args();adapter=Adapter(args.home,args.tool_id)
 for line in sys.stdin:
  try:
   if len(line)>1024*1024:raise ValueError()
   response=adapter.handle(json.loads(line))
  except ValueError:response={'jsonrpc':'2.0','id':None,'error':{'code':-32700,'message':'Parse error'}}
  if response is not None:print(json.dumps(response,ensure_ascii=False),flush=True)
if __name__=='__main__':main()
