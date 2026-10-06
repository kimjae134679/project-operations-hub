"""Contract checks; optional real Framework compilation exercises only pure helpers.
These checks never capture a screen, focus a window or inject user input.
"""
import os
from pathlib import Path
import subprocess
import tempfile
import unittest

SOURCE = Path(__file__).resolve().parents[1] / 'DesktopAutomation.cs'


class DesktopHelperContract(unittest.TestCase):
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
                    capture_output=True,text=True,timeout=40)
                self.assertEqual(compiled.returncode,0,compiled.stdout+compiled.stderr)
                compiled = subprocess.run(common+['/target:exe','/out:'+str(check),str(harness_path)],
                    capture_output=True,text=True,timeout=40)
                self.assertEqual(compiled.returncode,0,compiled.stdout+compiled.stderr)
                result = subprocess.run([str(check),str(helper)],capture_output=True,text=True,timeout=15)
                self.assertEqual(result.returncode,0,result.stdout+result.stderr)
                self.assertIn('ABI and pure argument guards passed',result.stdout)


if __name__ == '__main__': unittest.main()
