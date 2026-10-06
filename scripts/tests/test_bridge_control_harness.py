"""Run the updater's PowerShell5 self-check against real local API/executor."""
import json, subprocess, sys, tempfile, threading, time
from pathlib import Path
repo=Path(__file__).resolve().parents[2]
helper_path=Path(sys.argv[1]).resolve() if len(sys.argv)>1 else repo/'scripts/update-codex.ps1'
runtime=Path(sys.argv[2]).resolve() if len(sys.argv)>2 else repo/'04_COMMUNICATION/remote-bridge/releases/20261006-v3'
sys.path.insert(0,str(runtime))
from universal_worker import UniversalWorker
from local_api import LocalServer

with tempfile.TemporaryDirectory() as tmp:
    home=Path(tmp); (home/'state').mkdir()
    (home/'config.json').write_text(json.dumps({'deviceId':'fixture-bridge','protectedRoots':[]}))
    worker=UniversalWorker(home)
    server=LocalServer(worker)
    done=threading.Event()
    def pump():
        while not done.is_set():
            worker.pump(); time.sleep(.05)
    thread=threading.Thread(target=pump);thread.start()
    try:
        helper=str(helper_path).replace("'","''")
        endpoint=str(home/'state/local_endpoint.json').replace("'","''")
        root=str(home).replace("'","''")
        script=f"""$ErrorActionPreference='Stop'
$tokens=$null;$errs=$null
$ast=[System.Management.Automation.Language.Parser]::ParseFile('{helper}',[ref]$tokens,[ref]$errs)
if(@($errs).Count){{throw 'Helper parse error'}}
$fn=$ast.Find({{param($a) $a -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $a.Name -eq 'Test-BridgeControl'}},$true)
$text=$fn.Extent.Text.Replace('D:\A_KJ\AI\Applications\ProjectBridge\state\local_endpoint.json','{endpoint}')
. ([scriptblock]::Create($text))
$root='{root}'
Test-BridgeControl|ConvertTo-Json -Compress
"""
        r=subprocess.run(['powershell.exe','-NoProfile','-NonInteractive','-Command',script],capture_output=True,text=True,encoding='cp949',errors='replace',creationflags=0x08000000,timeout=90)
        print(r.stdout);print(r.stderr)
        if r.returncode:raise SystemExit(r.returncode)
        result=json.loads(r.stdout.strip())
        assert result['commandRoundtrip']=='pass' and result['localReady'] is True
        actual=worker.get_job(result['jobId'])
        assert actual['state']=='finished' and actual['result']['outcome']=='completed'
        assert 'REMOTE_CONTROL_OK' in actual['result']['data']['stdout']
        worker.executor=lambda *a:{'succeeded':False,'stdout':'REMOTE_CONTROL_OK'}
        failed=subprocess.run(['powershell.exe','-NoProfile','-NonInteractive','-Command',script],capture_output=True,text=True,encoding='cp949',errors='replace',creationflags=0x08000000,timeout=90)
        assert failed.returncode!=0 and 'outcome failed' in failed.stderr,failed.stderr
        print('PASS: actual Windows PowerShell helper authenticated real local API and ran real hidden child; finished/completed shape accepted; unsuccessful result rejected.')
    finally:
        done.set();thread.join(timeout=3);server.close();worker.pool.shutdown(wait=True)
