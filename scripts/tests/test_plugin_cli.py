"""Test actual installed Codex registration/install using a disposable home.

Only child-process configuration is isolated; user config/permissions are untouched.
"""
import json,os,subprocess,tempfile,zipfile
from pathlib import Path
root=Path(__file__).parent.resolve()
cli=Path(r'C:\Users\user\AppData\Roaming\npm\node_modules\@openai\codex\node_modules\@openai\codex-win32-x64\vendor\x86_64-pc-windows-msvc\bin\codex.exe')
with tempfile.TemporaryDirectory(prefix='ProjectBridgePluginCli_') as fixture_home:
 env={k:v for k,v in os.environ.items() if k.upper() not in ('CODEX_HOME','APPDATA')};env['CODEX_HOME']=fixture_home
 env['APPDATA']=str(Path(r'C:\Users\user\AppData\Roaming'))
 with zipfile.ZipFile(root/'ProjectBridge_PC_Control_3.0.1.zip') as z:z.extractall(Path(fixture_home)/'archive')
 package=Path(fixture_home)/'archive/ProjectBridge_PC_Control'
 def run(*args):
  p=subprocess.run([str(cli),*args],env=env,cwd=str(root),capture_output=True,text=True,encoding='utf-8',timeout=30,creationflags=0x08000000)
  print(json.dumps({'args':args,'code':p.returncode,'stdout':p.stdout,'stderr':p.stderr},ensure_ascii=True),flush=True)
  if p.returncode:raise SystemExit(p.returncode)
  return p
 run('--version')
 run('plugin','marketplace','add',str(package),'--json')
 run('plugin','add','projectbridge-local@projectbridge-personal','--json')
 result=run('plugin','list','--json')
 data=json.loads(result.stdout);assert any(p['pluginId']=='projectbridge-local@projectbridge-personal' and p['installed'] and p['enabled'] for p in data['installed'])
 install=subprocess.run([os.environ.get('COMSPEC',r'C:\Windows\System32\cmd.exe'),'/d','/c',str(package/'INSTALL.cmd')],env=env,input='\n',capture_output=True,text=True,encoding='utf-8',errors='replace',timeout=30,creationflags=0x08000000)
 print(json.dumps({'actualPackagedInstallerCode':install.returncode,'stdout':install.stdout,'stderr':install.stderr},ensure_ascii=True),flush=True)
 assert install.returncode==0,install.stderr
 print('PASS: actual installed Codex registered marketplace, installed plugin and listed installed entry',flush=True)
