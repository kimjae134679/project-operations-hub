"""Contract checks; optional real Framework compilation exercises only pure helpers.
These checks never capture a screen, focus a window or inject user input.
"""
import os
import json
import shutil,sys,time,urllib.request,ctypes
from pathlib import Path
import subprocess
import tempfile
import unittest

SOURCE = Path(__file__).resolve().parents[1] / 'DesktopAutomation.cs'


class DesktopHelperContract(unittest.TestCase):
    @unittest.skipUnless(os.name=='nt','Windows installer file-access preflight')
    def test_installer_existing_config_preflight_is_nondestructive_and_fails_before_mutation(self):
        import base64
        installer=SOURCE.with_name('install.ps1');text=installer.read_text(encoding='utf-8-sig')
        self.assertIn('function Assert-ExistingConfigWritable(',text)
        self.assertGreaterEqual(text.count(' Assert-ExistingConfigWritable $configPath'),2)
        first=text.index(' Assert-ExistingConfigWritable $configPath')
        last=text.rindex(' Assert-ExistingConfigWritable $configPath')
        self.assertLess(first,text.index(' $base=$null')+len(' $base=$null')+150)
        self.assertLess(last,text.index(' New-Item -ItemType Directory -Force -Path $target,$statePath,$runtimePath'))
        self.assertLess(last,text.index(" [IO.File]::WriteAllText((Join-Path $statePath 'stop.flag'),'installation pause')"))
        with tempfile.TemporaryDirectory() as temp:
            script=r'''
$ErrorActionPreference='Stop';$errors=$null;$tokens=$null
$ast=[System.Management.Automation.Language.Parser]::ParseFile('INSTALL_PATH',[ref]$tokens,[ref]$errors)
if($errors){throw 'Installer parse failed'}
$function=$ast.Find({param($node) $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Assert-ExistingConfigWritable'},$true)
if(!$function){throw 'Config preflight missing'}
Invoke-Expression $function.Extent.Text
$root='ROOT_PATH';$config=Join-Path $root 'config.json';$flag=Join-Path $root 'stop.flag'
[IO.File]::WriteAllText($config,'{"deviceId":"fixture","keep":"unchanged"}')
[IO.File]::WriteAllText($flag,'existing user choice')
$before=[IO.File]::ReadAllBytes($config);$stamp=[IO.File]::GetLastWriteTimeUtc($config)
Assert-ExistingConfigWritable $config
if([IO.File]::GetLastWriteTimeUtc($config) -ne $stamp){throw 'Writable probe changed write time'}
$hold=[IO.File]::Open($config,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
try {
 $blocked=$false
 try{Assert-ExistingConfigWritable $config}catch{if($_.Exception.Message -notmatch 'config_not_writable_before_install'){throw};$blocked=$true}
 if(!$blocked){throw 'Read-only sharing did not fail closed'}
}finally{$hold.Dispose()}
$after=[IO.File]::ReadAllBytes($config)
if([Convert]::ToBase64String($before) -ne [Convert]::ToBase64String($after)){throw 'Probe modified original config'}
if([IO.File]::ReadAllText($flag) -ne 'existing user choice'){throw 'Probe changed existing stop choice'}
$missing=Join-Path $root 'missing.json';Assert-ExistingConfigWritable $missing
if(Test-Path -LiteralPath $missing){throw 'Probe created config file'}
$bad=Join-Path $root 'directory.json';[IO.Directory]::CreateDirectory($bad)|Out-Null
$blocked=$false;try{Assert-ExistingConfigWritable $bad}catch{$blocked=$true}
if(!$blocked){throw 'Directory config was accepted'}
'''.replace('INSTALL_PATH',str(installer).replace("'","''")).replace('ROOT_PATH',temp.replace("'","''"))
            command=base64.b64encode(script.encode('utf-16le')).decode('ascii')
            result=subprocess.run(['powershell.exe','-NoProfile','-NonInteractive','-EncodedCommand',command],capture_output=True,text=True,encoding='utf-8',errors='replace',timeout=20,creationflags=subprocess.CREATE_NO_WINDOW)
            self.assertEqual(result.returncode,0,result.stdout+result.stderr)
    @unittest.skipUnless(os.name=='nt','Windows staged installer regression')
    def test_installer_function_contract_runs_from_isolated_payload_stage(self):
        import base64
        release=SOURCE.parent
        with tempfile.TemporaryDirectory() as temp:
            stage=Path(temp)/'stage';stage.mkdir()
            manifest=json.loads((release/'release_manifest.json').read_text(encoding='utf-8'))
            for row in manifest['files']:
                target=stage/row['path'];target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(release/row['path'],target)
            # Exercise the actual staging function before running the leaf test
            # from its payload directory, just as the real installer does.
            script=r'''
$ErrorActionPreference='Stop';$errors=$null;$tokens=$null
Import-Module (Join-Path $env:WINDIR 'System32/WindowsPowerShell/v1.0/Modules/Microsoft.PowerShell.Utility/Microsoft.PowerShell.Utility.psd1') -Force
$ast=[System.Management.Automation.Language.Parser]::ParseFile('INSTALL_PATH',[ref]$tokens,[ref]$errors)
if($errors){throw 'Installer parse failed'}
$function=$ast.Find({param($node) $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Stage-InstallerSource'},$true)
Invoke-Expression $function.Extent.Text
Stage-InstallerSource 'RELEASE_PATH' 'INSTALL_PATH' '' 'DESTINATION_PATH'
$bad='BAD_PATH';[IO.File]::WriteAllText($bad,'different installer')
$rejected=$false
try{Stage-InstallerSource 'RELEASE_PATH' $bad '' 'DESTINATION_PATH'}catch{if($_.Exception.Message -eq 'Installer source binding mismatch'){$rejected=$true}else{throw}}
if(!$rejected){throw 'Unbound installer source accepted'}
'''
            for key,value in [('INSTALL_PATH',release/'install.ps1'),('RELEASE_PATH',release),('DESTINATION_PATH',stage/'install.ps1'),('BAD_PATH',Path(temp)/'different.ps1')]:script=script.replace(key,str(value).replace("'","''"))
            command=base64.b64encode(script.encode('utf-16le')).decode('ascii')
            result=subprocess.run(['powershell.exe','-NoProfile','-NonInteractive','-EncodedCommand',command],capture_output=True,text=True,encoding='utf-8',errors='replace',timeout=20,creationflags=subprocess.CREATE_NO_WINDOW)
            self.assertEqual(result.returncode,0,result.stdout+result.stderr)
            self.assertEqual((stage/'install.ps1').read_bytes(),(release/'install.ps1').read_bytes())
            code="import sys,unittest;sys.path.insert(0,'tests');import test_desktop_helper_source as t;s=unittest.defaultTestLoader.loadTestsFromName('test_installer_starts_registered_task_after_resume_flags_and_hidden_fallback',t.DesktopHelperContract);r=unittest.TextTestRunner().run(s);sys.exit(not r.wasSuccessful())"
            tested=subprocess.run([sys.executable,'-c',code],cwd=stage,capture_output=True,text=True,encoding='utf-8',errors='replace',timeout=30,creationflags=subprocess.CREATE_NO_WINDOW)
            self.assertEqual(tested.returncode,0,tested.stdout+tested.stderr)
    @unittest.skipUnless(os.name=='nt','Windows installer task supervision')
    def test_installer_starts_registered_task_after_resume_flags_and_hidden_fallback(self):
        import base64
        with tempfile.TemporaryDirectory() as temp:
            script=r'''
$ErrorActionPreference='Stop'
$errors=$null;$tokens=$null
$ast=[System.Management.Automation.Language.Parser]::ParseFile('INSTALL_PATH',[ref]$tokens,[ref]$errors)
if($errors){throw 'Installer parse failed'}
$function=$ast.Find({param($node) $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Start-InstalledBridge'},$true)
if(!$function){throw 'Startup function missing'}
Invoke-Expression $function.Extent.Text
$statePath='STATE_PATH';New-Item -ItemType Directory -Path $statePath|Out-Null
$target='fixture';$exePath='fixture.exe';$taskName='fixture'
$script:taskCalls=0;$script:nativeCalls=0;$script:failTask=$false
function Start-ScheduledTask($TaskName,$ErrorAction){
 foreach($name in @('stop.flag','disconnected.flag','stopped_logon.txt')){if(Test-Path (Join-Path $statePath $name)){throw 'Stop flag remained before task launch'}}
 $script:taskCalls++
 if($script:failTask){throw 'Simulated task start unavailable'}
}
function Start-Process($FilePath,$ArgumentList,$WorkingDirectory,$WindowStyle){
 if($WindowStyle -ne 'Hidden' -or $ArgumentList -ne '--resume'){throw 'Visible fallback'}
 $script:nativeCalls++
}
foreach($name in @('stop.flag','disconnected.flag','stopped_logon.txt')){[IO.File]::WriteAllText((Join-Path $statePath $name),'stopped')}
Start-InstalledBridge $true
if($script:taskCalls -ne 1 -or $script:nativeCalls -ne 0){throw 'Task did not own initial launcher'}
$script:failTask=$true
Start-InstalledBridge $true
if($script:taskCalls -ne 2 -or $script:nativeCalls -ne 1){throw 'Task failure did not use hidden native resume'}
Start-InstalledBridge $false
if($script:taskCalls -ne 2 -or $script:nativeCalls -ne 2){throw 'No-task startup failed'}
'''.replace('INSTALL_PATH',str(SOURCE.with_name('install.ps1')).replace("'","''")).replace('STATE_PATH',str(Path(temp)/'state').replace("'","''"))
            command=base64.b64encode(script.encode('utf-16le')).decode('ascii')
            result=subprocess.run(['powershell.exe','-NoProfile','-NonInteractive','-EncodedCommand',command],capture_output=True,text=True,encoding='utf-8',errors='replace',timeout=20,creationflags=subprocess.CREATE_NO_WINDOW)
            self.assertEqual(result.returncode,0,result.stdout+result.stderr)
    def test_background_launcher_never_shows_window_and_accepts_shutdown_query(self):
        if os.name!='nt':self.skipTest('Windows native launcher required')
        compiler=Path(os.environ.get('WINDIR',r'C:\Windows'))/'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
        if not compiler.exists():self.skipTest('Framework compiler unavailable')
        with tempfile.TemporaryDirectory() as temp:
            home=Path(temp);exe=home/'ProjectBridge-QA.exe'
            source=SOURCE.with_name('BridgeLauncher.cs')
            compiled=subprocess.run([str(compiler),'/nologo','/target:winexe','/out:'+str(exe),'/r:System.Windows.Forms.dll','/r:System.Drawing.dll','/r:System.Web.Extensions.dll',str(source)],capture_output=True,text=True,timeout=40,creationflags=subprocess.CREATE_NO_WINDOW)
            self.assertEqual(compiled.returncode,0,compiled.stdout+compiled.stderr)
            runtime=home/'Runtime';runtime.mkdir()
            for name in ('bridge_worker.py','universal_worker.py','universal_actions.py','process_runner.py','local_api.py'):shutil.copy2(SOURCE.with_name(name),runtime/name)
            (home/'config.json').write_text(json.dumps({'deviceId':'test-device','hubRepository':'kimjae134679/project-operations-hub','python':sys.executable,'projectRoot':str(home/'missing-audio'),'audioEnabled':False,'maxWorkers':4}),encoding='utf-8')
            process=subprocess.Popen([str(exe),'--background'],creationflags=subprocess.CREATE_NO_WINDOW)
            user=ctypes.WinDLL('user32');kernel=ctypes.WinDLL('kernel32');windows=[];visible=[]
            callback_type=ctypes.WINFUNCTYPE(ctypes.c_int,ctypes.c_void_p,ctypes.c_void_p)
            user.EnumWindows.argtypes=[callback_type,ctypes.c_void_p];user.GetWindowThreadProcessId.argtypes=[ctypes.c_void_p,ctypes.POINTER(ctypes.c_uint32)];user.IsWindowVisible.argtypes=[ctypes.c_void_p]
            def each(hwnd,lparam):
                pid=ctypes.c_uint32();user.GetWindowThreadProcessId(hwnd,ctypes.byref(pid))
                if pid.value==process.pid:
                    windows.append(hwnd)
                    if user.IsWindowVisible(hwnd):visible.append(hwnd)
                return 1
            callback=callback_type(each);endpoint=None;duplicate_started=False
            try:
                deadline=time.monotonic()+3
                while time.monotonic()<deadline:
                    user.EnumWindows(callback,None)
                    path=home/'state/local_endpoint.json'
                    if path.exists():
                        try:endpoint=json.loads(path.read_text(encoding='utf-8'))
                        except ValueError:pass
                    if endpoint is not None and not duplicate_started:
                        duplicate_started=True
                        for flag in ('--background','--resume'):
                            subprocess.run([str(exe),flag],timeout=3,creationflags=subprocess.CREATE_NO_WINDOW)
                    time.sleep(.02)
                self.assertEqual(visible,[],'Background launcher displayed a window')
                self.assertIsNotNone(endpoint,'Hidden launcher did not start local endpoint')
                self.assertTrue(duplicate_started,'Duplicate hidden invocation was not exercised')
                headers={'X-ProjectBridge-Token':endpoint['token']}
                with urllib.request.urlopen(urllib.request.Request(endpoint['baseUrl']+'/v1/status',headers=headers),timeout=3) as response:status=json.load(response)
                self.assertTrue(status['localReady']);self.assertEqual(status['deviceId'],'test-device')
                user.GetWindowTextW.argtypes=[ctypes.c_void_p,ctypes.c_wchar_p,ctypes.c_int]
                form=None
                for hwnd in set(windows):
                    title=ctypes.create_unicode_buffer(512);user.GetWindowTextW(hwnd,title,512)
                    if title.value=='공용 PC 연결 · ProjectBridge 3.0':form=hwnd;break
                self.assertIsNotNone(form)
                user.SendMessageTimeoutW.argtypes=[ctypes.c_void_p,ctypes.c_uint32,ctypes.c_size_t,ctypes.c_ssize_t,ctypes.c_uint32,ctypes.c_uint32,ctypes.POINTER(ctypes.c_size_t)]
                result=ctypes.c_size_t();accepted=user.SendMessageTimeoutW(form,0x11,0,0,2,1000,ctypes.byref(result))
                self.assertTrue(accepted);self.assertEqual(result.value,1)
                self.assertFalse((home/'state/stop.flag').exists(),'Query alone incorrectly stopped a live session')
            finally:
                subprocess.run([str(exe),'--stop'],timeout=3,creationflags=subprocess.CREATE_NO_WINDOW)
                ticket=home/'state/launcher_child.json'
                if ticket.exists():
                    child=json.loads(ticket.read_text(encoding='utf-8'))
                    kernel.OpenProcess.argtypes=[ctypes.c_uint32,ctypes.c_int,ctypes.c_uint32];kernel.OpenProcess.restype=ctypes.c_void_p
                    kernel.WaitForSingleObject.argtypes=[ctypes.c_void_p,ctypes.c_uint32];kernel.WaitForSingleObject.restype=ctypes.c_uint32
                    kernel.CloseHandle.argtypes=[ctypes.c_void_p]
                    handle=kernel.OpenProcess(0x100000,False,int(child['pid']))
                    if handle:
                        try:self.assertEqual(kernel.WaitForSingleObject(handle,10000),0,'Owned test worker did not release its lock')
                        finally:kernel.CloseHandle(handle)
                deadline=time.monotonic()+3
                while (home/'state/local_endpoint.json').exists() and time.monotonic()<deadline:time.sleep(.05)
                process.terminate();process.wait(timeout=3)

    def test_no_privilege_escalation_desktop_switch_or_persistence(self):
        source = SOURCE.read_text(encoding='utf-8')
        # The capture result stays in memory; helper has no networking or startup persistence.
        for forbidden in ['SwitchDesktop(', 'SetThreadDesktop(', 'AdjustTokenPrivileges(',
                          'AttachThreadInput(', 'SetWindowsHookEx(', 'Clipboard.',
                          'File.Write', 'File.Create', 'System.Net', 'Process.Start(']:
            self.assertNotIn(forbidden, source)
        self.assertIn('finally { CloseDesktop(desktop); }', source)

    def test_framework_compile_native_abi_and_pure_request_validation(self):
        if os.name != 'nt':
            self.skipTest('Windows .NET Framework compiler/runtime required')
        windows = Path(os.environ.get('WINDIR', r'C:\Windows'))
        compilers = [windows/'Microsoft.NET/Framework64/v4.0.30319/csc.exe',
                     windows/'Microsoft.NET/Framework/v4.0.30319/csc.exe']
        compiler = next((p for p in compilers if p.is_file()), None)
        if compiler is None:
            self.skipTest('.NET Framework C# compiler unavailable')
        harness = r'''
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
class Check {
 static Type helper;
 static object Invoke(string name, params object[] args) {
  return helper.GetMethod(name, BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,args);
 }
 static void Reject(string name, params object[] args) {
  try { Invoke(name,args); throw new Exception("Input was incorrectly accepted: "+name); }
  catch(TargetInvocationException e) {
   if(e.InnerException.GetType().Name!="RequestFailure")throw;
  }
 }
 static int Main(string[] args) {
  helper=Assembly.LoadFile(args[0]).GetType("DesktopAutomation");
  Type input=helper.GetNestedType("INPUT",BindingFlags.NonPublic|BindingFlags.Public);
  if(Marshal.SizeOf(input)!=(IntPtr.Size==8?40:28))throw new Exception("Wrong INPUT ABI size");
  int left=(int)Invoke("AbsolutePixel",-1919,-1920,3840);
  if((int)Math.Floor(left*3840.0/65536.0)!=1)throw new Exception("Wrong pixel normalization on negative monitor");
  int right=(int)Invoke("AbsolutePixel",1919,-1920,3840);
  if((int)Math.Floor(right*3840.0/65536.0)!=3839)throw new Exception("Wrong right-edge normalization");
  var d=new Dictionary<string,object>();d["x"]=-1920;
  if((int)Invoke("Integer",d,"x",0,-30000,30000)!=-1920)throw new Exception("Negative monitor coordinate lost");
  d["x"]=1.5;Reject("Integer",d,"x",0,-30000,30000);
  d["x"]=50000;Reject("Integer",d,"x",0,-30000,30000);
  d["x"]="1";Reject("Integer",d,"x",0,-30000,30000);
  d["text"]=new string('x',4001);Reject("Text",d,"text","",4000);
  if((ushort)Invoke("VirtualKey","ENTER")!=0x0D)throw new Exception("Wrong ENTER");
  if((ushort)Invoke("VirtualKey","F24")!=0x87)throw new Exception("Wrong F24");
  if((ushort)Invoke("VirtualKey","CTRL")!=0x11)throw new Exception("Wrong CTRL");
  Reject("VirtualKey","F25");Reject("VirtualKey","arbitrary_key");
  if((uint)Invoke("KeyFlags",(ushort)0x25)!=1)throw new Exception("Navigation must be extended");
  Console.WriteLine("ABI and pure argument guards passed "+(IntPtr.Size*8));return 0;
 }
}
'''
        with tempfile.TemporaryDirectory() as temp:
            temp = Path(temp); harness_path = temp/'Check.cs'; harness_path.write_text(harness,encoding='utf-8')
            for architecture in ['x86', 'x64']:
                helper = temp/('DesktopAutomation-'+architecture+'.exe')
                check = temp/('Check-'+architecture+'.exe')
                common = [str(compiler),'/nologo','/platform:'+architecture]
                compiled = subprocess.run(common+['/target:winexe','/out:'+str(helper),
                    '/r:System.Windows.Forms.dll','/r:System.Drawing.dll','/r:System.Web.Extensions.dll',str(SOURCE)],
                    capture_output=True,text=True,timeout=40,creationflags=subprocess.CREATE_NO_WINDOW)
                self.assertEqual(compiled.returncode,0,compiled.stdout+compiled.stderr)
                compiled = subprocess.run(common+['/target:exe','/out:'+str(check),str(harness_path)],
                    capture_output=True,text=True,timeout=40,creationflags=subprocess.CREATE_NO_WINDOW)
                self.assertEqual(compiled.returncode,0,compiled.stdout+compiled.stderr)
                result = subprocess.run([str(check),str(helper)],capture_output=True,text=True,timeout=15,creationflags=subprocess.CREATE_NO_WINDOW)
                self.assertEqual(result.returncode,0,result.stdout+result.stderr)
                self.assertIn('ABI and pure argument guards passed',result.stdout)
                # Exercise WinExe + CREATE_NO_WINDOW + redirected stdio. This
                # lists windows only; it never captures or injects input.
                result=subprocess.run([str(helper)],input=json.dumps({'action':'window_list','args':{}}),capture_output=True,text=True,encoding='utf-8',timeout=15,creationflags=subprocess.CREATE_NO_WINDOW)
                response=json.loads(result.stdout)
                self.assertIs(type(response.get('ok')),bool)
                if not response['ok']:
                    self.assertNotIn('IOException',response.get('error',{}).get('message',''))


if __name__ == '__main__': unittest.main()
