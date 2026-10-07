"""Authenticated loopback API. Never exposes an unauthenticated general executor."""
import http.server,json,os,re,secrets,threading
from pathlib import Path
from universal_worker import UniversalError,atomic,now

class LocalServer:
 def __init__(self,worker):
  self.worker=worker;self.token=secrets.token_urlsafe(32);owner=self
  class Handler(http.server.BaseHTTPRequestHandler):
   def log_message(self,*args):pass
   def permitted(self):
    # Browsers must not use this local tool endpoint, including null/file origins.
    return self.headers.get('Origin') is None and self.headers.get('Host')=='127.0.0.1:'+str(owner.server.server_port) and secrets.compare_digest(self.headers.get('X-ProjectBridge-Token',''),owner.token)
   def answer(self,code,data):
    raw=json.dumps(data,ensure_ascii=False).encode();self.send_response(code)
    self.send_header('Content-Type','application/json; charset=utf-8');self.send_header('Cache-Control','no-store');self.send_header('Content-Length',str(len(raw)));self.end_headers()
    try:self.wfile.write(raw)
    except (BrokenPipeError,ConnectionResetError):pass
   def do_GET(self):
    if not self.permitted():self.answer(403,{'error':'forbidden'});return
    try:
     if self.path=='/v1/status':self.answer(200,owner.worker.status())
     elif re.fullmatch(r'/v1/jobs/[a-zA-Z0-9][a-zA-Z0-9_-]{0,79}',self.path):self.answer(200,owner.worker.get_job(self.path.rsplit('/',1)[1]))
     else:self.answer(404,{'error':'endpoint_not_found'})
    except UniversalError as exc:self.answer(404,{'error':str(exc)})
   def do_POST(self):
    if not self.permitted():self.answer(403,{'error':'forbidden'});return
    try:
     length=int(self.headers.get('Content-Length','0'))
     if not 0<length<=900*1024 or self.headers.get('Content-Type','').split(';')[0]!='application/json':raise UniversalError('invalid_json_request')
     value=json.loads(self.rfile.read(length))
     if self.path=='/v1/jobs':
      if not isinstance(value,dict) or not value.get('toolId'):raise UniversalError('local_issuer_required')
      self.answer(202,owner.worker.submit(value));return
     if self.path=='/v1/control' and isinstance(value,dict) and set(value)=={'action'} and value['action'] in ('pause','resume','stop'):
      state=owner.worker.state;action=value['action']
      if action=='pause':(state/'disconnected.flag').write_text('paused by authenticated local tool',encoding='utf-8')
      if action=='resume':
       for name in ('disconnected.flag','stop.flag','stopped_logon.txt'):(state/name).unlink(missing_ok=True)
      if action=='stop':
       logon=(state/'current_logon.txt').read_text(encoding='utf-8') if (state/'current_logon.txt').exists() else ''
       if logon:(state/'stopped_logon.txt').write_text(logon,encoding='utf-8')
       (state/'stop.flag').write_text('stopped by authenticated local tool',encoding='utf-8');owner.worker.stop()
      owner.worker.ui();self.answer(200,{'accepted':True,'action':action});return
     self.answer(404,{'error':'endpoint_not_found'})
    except (ValueError,UnicodeError,UniversalError) as exc:
     code=str(exc) if isinstance(exc,UniversalError) else 'invalid_json_request';self.answer(400,{'error':code})
  self.server=http.server.ThreadingHTTPServer(('127.0.0.1',0),Handler);self.server.daemon_threads=True
  self.server.timeout=.5
  self.path=worker.state/'local_endpoint.json'
  atomic(self.path,{'schemaVersion':1,'bridgeVersion':'3.0.0','baseUrl':'http://127.0.0.1:'+str(self.server.server_port),'token':self.token,'workerPid':os.getpid(),'updatedAt':now()})
  self.thread=threading.Thread(target=self.server.serve_forever,kwargs={'poll_interval':.1},daemon=True);self.thread.start()
 def close(self):
  self.server.shutdown();self.server.server_close();self.thread.join(timeout=.5);self.path.unlink(missing_ok=True)
