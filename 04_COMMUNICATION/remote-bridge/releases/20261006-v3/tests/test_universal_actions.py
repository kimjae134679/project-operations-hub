import base64
import hashlib
import importlib
import json
import os
from pathlib import Path
import sys
import tempfile
import time
import unittest
import ctypes,threading
from concurrent.futures import ThreadPoolExecutor
from unittest.mock import patch

sys.path.insert(0,str(Path(__file__).parents[1]))
import universal_actions as u
import process_runner as p

class UniversalTests(unittest.TestCase):
    def test_windows_exited_process_with_retained_handle_is_not_alive(self):
        if os.name!='nt':self.skipTest('Windows retained kernel handle behavior')
        process=u.subprocess.Popen([sys.executable,'-c','import time;time.sleep(.1)'],stdout=u.subprocess.DEVNULL,stderr=u.subprocess.DEVNULL,creationflags=u.subprocess.CREATE_NO_WINDOW)
        born=p.birth(process.pid);self.assertIsNotNone(born);self.assertTrue(p.alive(process.pid,born))
        process.wait(timeout=10)
        self.assertEqual(p.birth(process.pid),born,'Retained handle should preserve process identity')
        self.assertFalse(p.alive(process.pid,born),'Exited process must not remain running due to birth time')
    def test_windows_atomic_state_waits_for_reader_delete_sharing(self):
        if os.name!='nt':self.skipTest('Windows file sharing semantics')
        path=self.state/'shared-status.json';p.atomic(path,{'state':'old'})
        kernel=ctypes.WinDLL('kernel32',use_last_error=True)
        kernel.CreateFileW.argtypes=[ctypes.c_wchar_p,ctypes.c_uint32,ctypes.c_uint32,ctypes.c_void_p,ctypes.c_uint32,ctypes.c_uint32,ctypes.c_void_p];kernel.CreateFileW.restype=ctypes.c_void_p
        kernel.CloseHandle.argtypes=[ctypes.c_void_p]
        # Deliberately emulate a reader that permits read/write but not DELETE.
        handle=kernel.CreateFileW(str(path),0x80000000,3,None,3,0x80,None)
        self.assertNotEqual(handle,ctypes.c_void_p(-1).value)
        try:
            with ThreadPoolExecutor(max_workers=1) as pool:
                future=pool.submit(p.atomic,path,{'state':'new'});time.sleep(.08)
                self.assertFalse(future.done());self.assertEqual(p.load(path)['state'],'old')
                kernel.CloseHandle(handle);handle=None;future.result(timeout=3)
            self.assertEqual(p.load(path)['state'],'new')
        finally:
            if handle:kernel.CloseHandle(handle)
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory();self.addCleanup(self.temp.cleanup)
        self.root=Path(self.temp.name);self.state=self.root/'state';self.state.mkdir()
        self.work=self.root/'work';self.work.mkdir()
        self.desktop=self.root/'Desktop';self.desktop.mkdir()
        self.config={'python':sys.executable,'protectedRoots':[str(self.desktop)]}
        self.processes=[]
        self.addCleanup(self.cleanup_processes)
    def call(self,action,args):return u.perform({'id':'test','action':action,'args':args},self.config,self.state)
    def cleanup_processes(self):
        for pid in self.processes:
            try:u.stop_process(pid,self.state)
            except (u.ActionError,OSError):pass
            # Test runtime can mount host /proc with virtual Python PIDs.
            # This test owns the ticket; direct local flag cleanup does not
            # weaken the production API's kernel ownership requirement.
            folder=u.folder_for(self.state,pid)
            if folder.exists():(folder/'stop.flag').write_text('test cleanup')
            try:self.wait_done(pid)
            except Exception:pass
        for identity,child in list(u.OWNED_RUNNERS.items()):
            try:child.wait(timeout=5)
            except Exception:pass
            u.OWNED_RUNNERS.pop(identity,None)
    def wait_done(self,identity):
        # Windows process creation can wait for scanning under concurrent build
        # load. The runner's timeout begins after the owned child is launched.
        deadline=time.monotonic()+30
        while time.monotonic()<deadline:
            row=self.call('process_status',{'processId':identity})
            if row['done']:return row
            time.sleep(.05)
        self.fail('owned runner did not complete: '+str({k:v for k,v in row.items() if k not in ('stdout','stderr')}))
    def start(self,source,timeout=10):
        result=self.call('start_process',{'python':source,'cwd':str(self.work),'timeoutSeconds':timeout})
        self.processes.append(result['processId']);return result
    def test_write_requires_sha_precondition_and_retains_backup(self):
        path=self.work/'code.py';path.write_text('old')
        with self.assertRaises(u.ActionError):self.call('write_file',{'path':str(path),'contentUtf8':'new'})
        with self.assertRaises(u.ActionError):self.call('write_file',{'path':str(path),'expectedSha256':'a'*64,'contentUtf8':'new'})
        self.assertEqual(path.read_text(),'old')
        value=self.call('write_file',{'path':str(path),'expectedSha256':u.sha(b'old'),'contentUtf8':'new'})
        self.assertEqual(path.read_text(),'new');self.assertEqual(Path(value['backup']).read_text(),'old')
        with self.assertRaises(u.ActionError):self.call('write_file',{'path':str(path),'expectedSha256':u.sha(b'old'),'contentUtf8':'lost edit'})
        self.assertEqual(path.read_text(),'new')
    def test_new_file_null_hash_and_binary_round_trip(self):
        path=self.work/'image.bin';data=b'\xff\x00\x80'
        self.call('write_file',{'path':str(path),'expectedSha256':None,'contentBase64':base64.b64encode(data).decode()})
        row=self.call('read_file',{'path':str(path)})
        self.assertEqual(base64.b64decode(row['contentBase64']),data);self.assertEqual(row['sha256'],u.sha(data))
        with self.assertRaises(u.ActionError):self.call('write_file',{'path':str(path),'expectedSha256':None,'contentUtf8':'bad'})
    def test_protected_desktop_direct_and_symlink_write_rejected(self):
        for path in [self.desktop/'x.txt']:
            with self.assertRaises(u.ActionError):self.call('write_file',{'path':str(path),'expectedSha256':None,'contentUtf8':'x'})
        link=self.work/'alias'
        try:link.symlink_to(self.desktop,target_is_directory=True)
        except (OSError,NotImplementedError):return
        with self.assertRaises(u.ActionError):self.call('write_file',{'path':str(link/'x'),'expectedSha256':None,'contentUtf8':'x'})
    def test_read_large_file_bounded_offsets_and_whole_sha(self):
        data=b'a'*u.MAX_BYTES+b'b'*50;path=self.work/'big.log';path.write_bytes(data)
        first=self.call('read_file',{'path':str(path)})
        self.assertEqual(first['bytes'],512*1024);self.assertFalse(first['eof'])
        last=self.call('read_file',{'path':str(path),'offsetBytes':u.MAX_BYTES,'maxBytes':100})
        self.assertEqual(last['contentUtf8'],'b'*50);self.assertTrue(last['eof'])
        self.assertEqual(last['sha256'],u.sha(data));self.assertEqual(last['totalBytes'],len(data))
        for args in ({'maxBytes':u.MAX_BYTES+1},{'offsetBytes':-1},{'offsetBytes':len(data)+1}):
            with self.assertRaises(u.ActionError):self.call('read_file',{'path':str(path),**args})
    def test_relative_path_and_wide_process_kill_refused(self):
        with self.assertRaises(u.ActionError):self.call('read_file',{'path':'code.py'})
        with self.assertRaises(u.ActionError):self.start("print('taskkill /IM python.exe /F')")
        with self.assertRaises(u.ActionError):self.call('run_command',{'python':'print(1)','cwd':str(self.desktop)})
    def test_list_directory_bound_and_capabilities(self):
        for i in range(4):(self.work/str(i)).write_text(str(i))
        row=self.call('list_dir',{'path':str(self.work),'limit':2})
        self.assertEqual(len(row['entries']),2);self.assertTrue(row['truncated'])
        self.assertEqual(self.call('capabilities',{})['channelRequired'],'authenticated_private')
    def test_async_owner_survives_caller_and_returns_output(self):
        first=self.start("import time;time.sleep(.2);print('build complete')")
        self.assertIn(first['stage'],('starting','running','completed'))
        final=self.wait_done(first['processId'])
        self.assertEqual(final['stage'],'completed',str(final));self.assertEqual(final['returnCode'],0)
        self.assertIn('build complete',final['stdout']);self.assertTrue(final['done']);self.assertFalse(final['running'])
        self.assertTrue(Path(final['stdoutPath']).is_file())
    def test_sync_command_and_bounded_redacted_output(self):
        secret='ghp_'+'A'*32
        result=self.call('run_command',{'python':"print('"+secret+"');print('x'*80000)",
            'cwd':str(self.work),'timeoutSeconds':10})
        self.assertEqual(result.get('returnCode'),0,str({k:v for k,v in result.items() if k not in ('stdout','stderr')}));self.assertTrue(result['stdoutTruncated'])
        self.assertLessEqual(len(result['stdout']),65536)
        self.assertNotIn(secret,u.redact(secret))
    def test_stop_exact_owned_process_and_no_unrelated_kills(self):
        if p.birth(os.getpid()) is None:self.skipTest('runtime PID namespace does not match mounted /proc; Windows kernel identity requires device test')
        first=self.start('import time;time.sleep(20)',timeout=30)
        identity=first['processId'];folder=u.folder_for(self.state,identity)
        for _ in range(100):
            status=p.load(folder/'status.json',{})
            if status.get('stage')=='running':break
            time.sleep(.02)
        original=dict(status);status['runnerBirth']='wrong-birth';p.atomic(folder/'status.json',status)
        # An unowned PID/birth cannot receive a stop flag.
        with self.assertRaises(u.ActionError):self.call('stop_process',{'processId':identity})
        self.assertFalse((folder/'stop.flag').exists())
        p.atomic(folder/'status.json',original)
        self.assertTrue(self.call('stop_process',{'processId':identity})['stopRequested'])
        final=self.wait_done(identity);self.assertEqual(final['stage'],'stopped')
    def test_stop_birth_guard_refuses_forged_pid_without_touching_it(self):
        identity='a'*32;folder=u.folder_for(self.state,identity);folder.mkdir(parents=True)
        p.atomic(folder/'ticket.json',{'processId':identity,'ownerNonce':'own'})
        p.atomic(folder/'status.json',{'stage':'running','processId':identity,'ownerNonce':'own','runnerPid':1234,'runnerBirth':'original'})
        with patch.object(p,'alive',return_value=False):
            with self.assertRaises(u.ActionError):self.call('stop_process',{'processId':identity})
        self.assertFalse((folder/'stop.flag').exists())
    def test_stop_exact_birth_match_signals_only_owned_ticket(self):
        identity='b'*32;folder=u.folder_for(self.state,identity);folder.mkdir(parents=True)
        p.atomic(folder/'ticket.json',{'processId':identity,'ownerNonce':'own'})
        p.atomic(folder/'status.json',{'stage':'running','processId':identity,'ownerNonce':'own','runnerPid':1234,'runnerBirth':'original'})
        with patch.object(p,'alive',side_effect=lambda pid,born:pid==1234 and born=='original'):
            result=self.call('stop_process',{'processId':identity})
        self.assertTrue(result['stopRequested']);self.assertTrue((folder/'stop.flag').exists())
    def test_unknown_process_and_changed_nonce_refused(self):
        with self.assertRaises(u.ActionError):self.call('stop_process',{'processId':'a'*32})
        first=self.start('import time;time.sleep(.5)');folder=u.folder_for(self.state,first['processId'])
        status=p.load(folder/'status.json');original=dict(status);status['ownerNonce']='changed'
        p.atomic(folder/'status.json',status)
        with self.assertRaises(u.ActionError):self.call('process_status',{'processId':first['processId']})
        p.atomic(folder/'status.json',original)
    def test_timeout_terminates_only_owned_child(self):
        first=self.start('import time;time.sleep(20)',timeout=1)
        final=self.wait_done(first['processId']);self.assertEqual(final['stage'],'timed_out')
    def test_ui_helper_request_and_private_response(self):
        helper=self.work/'helper.py';helper.write_text('import json,sys;request=json.load(sys.stdin);print(json.dumps({"ok":True,"result":{"action":request["action"]}}))')
        self.config['uiHelper']=[sys.executable,str(helper)]
        row=self.call('ui_control',{'action':'window_list','args':{}})
        self.assertEqual(row['result']['action'],'window_list')
        with self.assertRaises(u.ActionError):self.call('ui_control',{'action':'shell','args':{}})
    def test_ui_exact_screen_capture_protocol_and_structured_failure(self):
        self.config['uiHelper']=sys.executable
        request={'action':'screen_capture','args':{'maxWidth':1280}}
        failure={'ok':False,'error':{'code':'desktop_locked','message':'Locked desktop'}}
        response=u.subprocess.CompletedProcess([],1,json.dumps(failure),'')
        with patch.object(u.subprocess,'run',return_value=response) as invoke:
            self.assertEqual(self.call('ui_control',request),failure)
        self.assertEqual(json.loads(invoke.call_args.kwargs['input']),request)
        self.assertEqual(invoke.call_args.kwargs['timeout'],30)
        for malformed in ({'action':'screen_capture'},{'action':'screenshot','args':{}},{'action':'screen_capture','args':{},'extra':1}):
            with self.assertRaises(u.ActionError):self.call('ui_control',malformed)
    def test_windows_job_clears_only_kill_on_close_on_success(self):
        import ctypes
        from unittest.mock import Mock
        class Basic(ctypes.Structure):_fields_=[('LimitFlags',ctypes.c_uint32)]
        class Info(ctypes.Structure):_fields_=[('BasicLimitInformation',Basic)]
        owned=p.WindowsJob.__new__(p.WindowsJob)
        owned.handle=123;owned.attached=True;owned.info=Info()
        owned.info.BasicLimitInformation.LimitFlags=0x2000|0x100
        owned.kernel=Mock();owned.kernel.SetInformationJobObject.return_value=1
        self.assertTrue(owned.preserve_descendants());self.assertEqual(owned.info.BasicLimitInformation.LimitFlags,0x100)
        args=owned.kernel.SetInformationJobObject.call_args.args
        self.assertEqual(args[:2],(123,9));self.assertEqual(args[3],ctypes.sizeof(Info))
        owned.terminate();owned.kernel.TerminateJobObject.assert_called_once_with(123,1)
    def test_windows_job_failed_flag_change_retains_crash_cleanup(self):
        import ctypes
        from unittest.mock import Mock
        class Basic(ctypes.Structure):_fields_=[('LimitFlags',ctypes.c_uint32)]
        class Info(ctypes.Structure):_fields_=[('BasicLimitInformation',Basic)]
        owned=p.WindowsJob.__new__(p.WindowsJob);owned.handle=123;owned.attached=True;owned.info=Info()
        owned.info.BasicLimitInformation.LimitFlags=0x2000;owned.kernel=Mock()
        owned.kernel.SetInformationJobObject.return_value=0
        self.assertFalse(owned.preserve_descendants());self.assertEqual(owned.info.BasicLimitInformation.LimitFlags,0x2000)

if __name__=='__main__':unittest.main()
