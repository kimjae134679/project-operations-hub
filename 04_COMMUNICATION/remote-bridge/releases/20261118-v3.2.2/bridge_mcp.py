from version import VERSION
"""MCP stdio adapter for Codex/Jev/local clients; stdout contains JSON-RPC only."""
import argparse,datetime as dt,hashlib,json,sys,urllib.request,urllib.error,urllib.parse,uuid
from pathlib import Path

TOOLS=[{'name':'pc_status','description':'공용 PC 연결·작업 상태 확인','inputSchema':{'type':'object','properties':{},'additionalProperties':False}},
 {'name':'pc_submit','description':'파일·빌드·설치·장기 작업·화면 조작 제출. 작업 ID로 결과 조회.','inputSchema':{'type':'object','required':['action','args'],'properties':{'action':{'type':'string','enum':['capabilities','read_file','write_file','list_dir','run_command','start_process','process_status','stop_process','ui_control']},'args':{'type':'object'},'id':{'type':'string'},'projectId':{'type':'string'},'toolId':{'type':'string'},'dependsOn':{'type':'array','items':{'type':'string'}},'resourceKeys':{'type':'array','items':{'type':'string'}}},'additionalProperties':False}},
 {'name':'pc_result','description':'등록 작업 실제 결과 조회','inputSchema':{'type':'object','required':['id'],'properties':{'id':{'type':'string','pattern':'^[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}$'}},'additionalProperties':False}}]
TOOLS[1]['description']='Submit an authorized PC operation and poll pc_result(id). read_file/list_dir/make_dir: args.path absolute. write_file: path, expectedSha256 (null only for new file), contentUtf8 or contentBase64. move_file: path, destination, expectedSha256. delete_file: path, expectedSha256; returns recoverable trashId. restore_file: trashId, expectedSha256 in the same projectId/toolId scope. Commands: argv or python or powershell, optional cwd and timeoutSeconds. Use start_process for long work; process_status/stop_process require owned processId. Never treat accepted as completed. Large files: file_info(path) once, then read_file with encoding=base64, includeSha256=false, expectedFileVersion, offsetBytes/maxBytes; verify chunkSha256 and final sha256 locally. Upload: upload_begin(path, expectedSha256, totalBytes, sha256), upload_chunk(uploadId, offsetBytes, contentBase64, chunkSha256), upload_status, upload_commit or upload_abort. Keep the same projectId/toolId scope. ui_control takes action/args: screen_capture, image_preview(path,maxWidth,maxHeight,quality), window_list, mouse_move(x,y), mouse_drag(x,y,toX,toY,durationMs,button), mouse_click, mouse_scroll, type_text, send_keys, focus_window. Use expectedHwnd on input operations to refuse a changed foreground window. pc_result returns previews as MCP images. A focus failure is scoped to window activation; refresh once and continue independent authorized work, without repeated manual-click requests or bypassing a host policy denial.'
for tool in TOOLS:
 tool['annotations']={'readOnlyHint':tool['name']!='pc_submit','destructiveHint':tool['name']=='pc_submit','idempotentHint':tool['name']!='pc_submit','openWorldHint':False}
TOOLS[1]['inputSchema']['properties']['action']['enum'].extend(['make_dir','move_file','delete_file','restore_file']+['file_info', 'upload_begin', 'upload_chunk', 'upload_status', 'upload_commit', 'upload_abort'])

def tool_result(data):
 import base64
 value=json.loads(json.dumps(data));images=[]
 outer=value.get('result') if isinstance(value,dict) else None
 if isinstance(outer,dict) and outer.get('outcome')=='completed':
  candidate=outer.get('data',{})
  if isinstance(candidate,dict) and candidate.get('ok') is True:
   image=candidate.get('result',{})
   if isinstance(image,dict) and image.get('mime_type')=='image/jpeg' and isinstance(image.get('jpeg_base64'),str):
    encoded=image.pop('jpeg_base64')
    if len(encoded)>768*1024:raise ValueError('image_too_large')
    raw=base64.b64decode(encoded,validate=True)
    if not raw.startswith(bytes([255,216,255])):raise ValueError('invalid_image')
    images=[{'type':'image','mimeType':'image/jpeg','data':encoded}]
 failed=isinstance(outer,dict) and outer.get('outcome')=='failed'
 return {'isError':failed,'structuredContent':value,'content':[{'type':'text','text':json.dumps(value,ensure_ascii=False)}]+images}

from tool_catalog import additions,TYPED,MANAGEMENT,META,check,validate_operation,obj
TOOLS.extend(additions())
TOOLS[1]['inputSchema']['properties'].update({k:v for k,v in META.items() if k in ('title','tags')})
TOOLS[1]['inputSchema']['properties']['action']['enum'].append('find_files')
TOOLS[1]['annotations']['openWorldHint']=True

class NoRedirect(urllib.request.HTTPRedirectHandler):
 def redirect_request(self,*args,**kwargs):raise ValueError('local_endpoint_redirect_refused')

class Adapter:
 def __init__(self,home,tool_id=None):self.home=Path(home);self.tool_id=tool_id or 'mcp-'+uuid.uuid4().hex
 def request(self,path,data=None,timeout=10):
  endpoint=json.loads((self.home/'state/local_endpoint.json').read_text(encoding='utf-8'))
  # Local file configuration cannot redirect a secret token to a remote host.
  base=endpoint['baseUrl'];url=urllib.parse.urlparse(base)
  if url.scheme!='http' or url.hostname!='127.0.0.1' or not url.port or url.path:raise ValueError('invalid_local_endpoint')
  raw=None if data is None else json.dumps(data).encode()
  headers={'X-ProjectBridge-Token':endpoint['token'],'Content-Type':'application/json'}
  opener=urllib.request.build_opener(urllib.request.ProxyHandler({}),NoRedirect())
  try:
   with opener.open(urllib.request.Request(base+path,data=raw,headers=headers),timeout=timeout) as response:return json.load(response)
  except urllib.error.HTTPError as exc:
   try:code=json.loads(exc.read(16384)).get('error','http_error')
   except ValueError:code='http_error'
   import re
   raise ValueError(code if isinstance(code,str) and re.fullmatch(r'[A-Za-z0-9_]{1,100}',code) else 'http_error')
 def call(self,name,args):
  if name in MANAGEMENT:
   check(args,MANAGEMENT[name]);value=json.loads(json.dumps(args));value.setdefault('projectId','Control-Tower');value.setdefault('toolId',self.tool_id)
   return self.request('/v1/manage',{'tool':name,'arguments':value},timeout=value.get('waitSeconds',20)+5 if name=='pc_job_wait' else 10)
  if name in TYPED:
   tool=next(t for t in TOOLS if t['name']==name);check(args,tool['inputSchema'])
   action,ui=TYPED[name];meta={k:v for k,v in args.items() if k in META};operation={k:v for k,v in args.items() if k not in META}
   operation={'action':ui,'args':operation} if ui else operation
   validate_operation(action,operation);return self.call('pc_submit',dict(meta,action=action,args=operation))
  if name=='pc_status':return dict(self.request('/v1/status'),callerToolId=self.tool_id)
  if name=='pc_result':
   import re
   if set(args)!={'id'} or not re.fullmatch(r'[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}',args['id']):raise ValueError('invalid_job_id')
   return self.request('/v1/jobs/'+args['id'])
  if name=='pc_submit':
   status=self.request('/v1/status');at=dt.datetime.now(dt.timezone.utc)
   job={'id':args.get('id','local-'+uuid.uuid4().hex),'target':'PC','deviceId':status['device'],'action':args['action'],'args':json.loads(json.dumps(args['args'])),
    'createdAt':at.isoformat(),'expiresAt':(at+dt.timedelta(days=1)).isoformat(),'projectId':args.get('projectId','Control-Tower'),'toolId':args.get('toolId',self.tool_id)}
   for key in ('dependsOn','resourceKeys','title','tags'):
    if key in args:job[key]=args[key]
   import re
   if not re.fullmatch(r'[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}',job['id']):raise ValueError('invalid_job_id')
   chunk_payload=None
   if job['action']=='upload_chunk' and isinstance(job['args'].get('contentBase64'),str):
    chunk_payload=job['args']['contentBase64'];job['args']['contentBase64']='sha256:'+hashlib.sha256(chunk_payload.encode()).hexdigest()
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
   if chunk_payload is not None:job['args']['contentBase64']=chunk_payload
   return self.request('/v1/jobs',job)
  raise ValueError('unknown_tool')
 def handle(self,message):
  if not isinstance(message,dict) or not isinstance(message.get('method'),str) or not isinstance(message.get('params',{}),dict):return {'jsonrpc':'2.0','id':None,'error':{'code':-32600,'message':'Invalid Request'}}
  method=message.get('method');rid=message.get('id');params=message.get('params',{})
  if rid is None:return None
  try:
   if method=='initialize':result={'protocolVersion':params.get('protocolVersion','2024-11-05'),'capabilities':{'tools':{}},'serverInfo':{'name':'ProjectBridge','version':VERSION}}
   elif method=='ping':result={}
   elif method=='tools/list':result={'tools':TOOLS}
   elif method=='tools/call':
    try:
     data=self.call(params['name'],params.get('arguments',{}));result=tool_result(data)
    except Exception as exc:
     code=str(exc) if isinstance(exc,ValueError) else type(exc).__name__
     import re
     if not re.fullmatch(r'[A-Za-z0-9_]{1,100}',code):code='request_failed'
     result={'isError':True,'content':[{'type':'text','text':'ProjectBridge: '+code}]}
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
