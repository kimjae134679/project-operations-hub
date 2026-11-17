import base64, hashlib, json, os, subprocess, sys, tempfile, time, unittest
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parents[1]))
import universal_actions as u
import universal_worker as worker
from bridge_mcp import Adapter, tool_result
from local_api import LocalServer

class TransferTests(unittest.TestCase):
    def setUp(self):
        self.tmp=tempfile.TemporaryDirectory();self.addCleanup(self.tmp.cleanup)
        self.root=Path(self.tmp.name); self.state=self.root/'state';self.config={'protectedRoots':[]}
        self.path=self.root/'dest.bin';self.data=os.urandom(2*1024*1024+37)
    def call(self,action,**args): return u.perform({'action':action,'args':args},self.config,self.state)
    def begin(self,data=None,expected=None):
        data=self.data if data is None else data
        return self.call('upload_begin',path=str(self.path),expectedSha256=expected,totalBytes=len(data),sha256=u.sha(data))['uploadId']
    def chunk(self,uid,offset,data):
        return self.call('upload_chunk',uploadId=uid,offsetBytes=offset,contentBase64=base64.b64encode(data).decode(),chunkSha256=u.sha(data))
    def send(self,uid,data):
        for off in range(0,len(data),u.MAX_BYTES):self.chunk(uid,off,data[off:off+u.MAX_BYTES])
    def test_binary_roundtrip_resume_replay_and_commit(self):
        uid=self.begin();chunk=self.data[:u.MAX_BYTES]
        self.chunk(uid,0,chunk)
        self.assertTrue(self.chunk(uid,0,chunk)['replayed'])
        self.assertEqual(self.call('upload_status',uploadId=uid)['nextOffsetBytes'],len(chunk))
        for off in range(len(chunk),len(self.data),u.MAX_BYTES):self.chunk(uid,off,self.data[off:off+u.MAX_BYTES])
        result=self.call('upload_commit',uploadId=uid);self.assertEqual(result['stage'],'completed')
        self.assertEqual(self.path.read_bytes(),self.data);self.assertEqual(self.call('upload_commit',uploadId=uid)['stage'],'completed')
        info=self.call('file_info',path=str(self.path))
        output=bytearray()
        for off in range(0,len(self.data),u.MAX_BYTES):
            read=self.call('read_file',path=str(self.path),offsetBytes=off,maxBytes=u.MAX_BYTES,encoding='base64',includeSha256=False,expectedFileVersion=info['fileVersion'])
            decoded=base64.b64decode(read['contentBase64']);self.assertEqual(u.sha(decoded),read['chunkSha256']);output.extend(decoded)
            self.assertIsNone(read['sha256'])
        self.assertEqual(u.sha(output),info['sha256'])
    def test_reject_corrupt_chunk_and_wrong_offset(self):
        uid=self.begin()
        with self.assertRaisesRegex(u.ActionError,'chunk_hash_mismatch'):self.call('upload_chunk',uploadId=uid,offsetBytes=0,contentBase64='YQ==',chunkSha256='0'*64)
        with self.assertRaisesRegex(u.ActionError,'chunk_offset_conflict'):self.chunk(uid,1,b'a')
        with self.assertRaisesRegex(u.ActionError,'upload_incomplete'):self.call('upload_commit',uploadId=uid)
    def test_final_hash_and_changed_destination_preserve_original(self):
        self.path.write_bytes(b'original');uid=self.begin(expected=u.sha(b'original'))
        self.send(uid,self.data);self.path.write_bytes(b'external edit')
        with self.assertRaisesRegex(u.ActionError,'file_sha_conflict'):self.call('upload_commit',uploadId=uid)
        self.assertEqual(self.path.read_bytes(),b'external edit')
        self.call('upload_abort',uploadId=uid)
        uid=self.begin(expected=u.sha(b'external edit'));self.send(uid,b'X'*len(self.data))
        with self.assertRaisesRegex(u.ActionError,'upload_hash_mismatch'):self.call('upload_commit',uploadId=uid)
        self.assertEqual(self.path.read_bytes(),b'external edit')
    def test_overwrite_backup_empty_file_and_abort(self):
        self.path.write_bytes(b'old');uid=self.begin(expected=u.sha(b'old'));self.send(uid,self.data)
        result=self.call('upload_commit',uploadId=uid);self.assertEqual(Path(result['backup']).read_bytes(),b'old')
        uid=self.begin(data=b'',expected=u.sha(self.data));self.call('upload_commit',uploadId=uid);self.assertEqual(self.path.read_bytes(),b'')
        uid=self.begin(expected=u.sha(b''));self.chunk(uid,0,b'abc');self.call('upload_abort',uploadId=uid)
        self.assertEqual(self.path.read_bytes(),b'');self.assertFalse(list(self.root.glob('*.part')))
    def test_scope_protection_and_quota(self):
        uid=self.begin()
        with self.assertRaisesRegex(u.ActionError,'upload_not_found_in_scope'):
            u.perform({'action':'upload_status','args':{'uploadId':uid}},self.config,self.root/'other')
        self.config['protectedRoots']=[str(self.root)]
        with self.assertRaisesRegex(u.ActionError,'protected_write_path'):self.call('upload_commit',uploadId=uid)
        self.config={'protectedRoots':[],'maxUploadBytes':1}
        with self.assertRaisesRegex(u.ActionError,'upload_too_large'):self.begin()
    def test_binary_read_exact_and_version_conflict(self):
        data=b'\xef\xbb\xbfsecret-prefix\x00';self.path.write_bytes(data);info=self.call('file_info',path=str(self.path))
        result=self.call('read_file',path=str(self.path),encoding='base64')
        self.assertEqual(base64.b64decode(result['contentBase64']),data)
        self.path.write_bytes(b'changed')
        with self.assertRaisesRegex(u.ActionError,'file_version_conflict'):self.call('read_file',path=str(self.path),expectedFileVersion=info['fileVersion'])
    def test_mcp_image_blocks_and_failed_job(self):
        jpeg=base64.b64encode(b'\xff\xd8\xfftest').decode()
        value={'result':{'outcome':'completed','data':{'ok':True,'result':{'mime_type':'image/jpeg','jpeg_base64':jpeg,'image_width':12}}}}
        result=tool_result(value);self.assertEqual(result['content'][1]['type'],'image')
        self.assertNotIn(jpeg,result['content'][0]['text']);self.assertIn('jpeg_base64',value['result']['data']['result'])
        self.assertTrue(tool_result({'result':{'outcome':'failed','data':{'code':'problem'}}})['isError'])
        value['result']['data']['result']['jpeg_base64']='YWJj'
        with self.assertRaisesRegex(ValueError,'invalid_image'):tool_result(value)
    def test_commit_crash_recovery(self):
        uid=self.begin();self.send(uid,self.data)
        meta=self.state/'uploads'/(uid+'.json');row=json.loads(meta.read_text())
        row['stage']='committing';meta.write_text(json.dumps(row),encoding='utf-8')
        os.replace(row['part'],self.path)
        result=self.call('upload_commit',uploadId=uid)
        self.assertEqual(result['stage'],'completed');self.assertEqual(self.path.read_bytes(),self.data)
    def test_quota_and_replay_conflict(self):
        self.config['maxUploadBytes']=len(self.data)
        uid=self.begin()
        with self.assertRaisesRegex(u.ActionError,'upload_quota_exceeded'):self.begin()
        self.chunk(uid,0,b'abc')
        with self.assertRaisesRegex(u.ActionError,'chunk_retry_conflict'):self.chunk(uid,0,b'xyz')
        self.call('upload_abort',uploadId=uid);self.begin()
    def test_upload_id_traversal_rejected(self):
        for value in ['../x','a'*31,'z'*32]:
            with self.assertRaisesRegex(u.ActionError,'invalid_upload_id'):self.call('upload_status',uploadId=value)

class McpTransferIntegration(unittest.TestCase):
    def test_real_loopback_upload_and_read(self):
        with tempfile.TemporaryDirectory() as temp:
            home=Path(temp);worker.atomic(home/'config.json',{'deviceId':'transfer-fixture','python':sys.executable,'protectedRoots':[]})
            w=worker.UniversalWorker(home);server=LocalServer(w);adapter=Adapter(home,'transfer-test');counter=0
            def call(action,args):
                nonlocal counter
                counter+=1;jid='transfer-'+str(counter);adapter.call('pc_submit',{'id':jid,'action':action,'args':args})
                deadline=time.monotonic()+20
                while time.monotonic()<deadline:
                    w.pump();row=adapter.call('pc_result',{'id':jid})
                    if row.get('result'):
                        self.assertEqual(row['result']['outcome'],'completed',row);return row['result']['data']
                    time.sleep(.02)
                self.fail('timeout')
            try:
                path=home/'binary.bin';data=os.urandom(600000)
                uid=call('upload_begin',{'path':str(path),'expectedSha256':None,'totalBytes':len(data),'sha256':u.sha(data)})['uploadId']
                for off in range(0,len(data),u.MAX_BYTES):
                    chunk=data[off:off+u.MAX_BYTES]
                    call('upload_chunk',{'uploadId':uid,'offsetBytes':off,'contentBase64':base64.b64encode(chunk).decode(),'chunkSha256':u.sha(chunk)})
                # Re-submit the identical chunk job after journal payload compaction.
                off=u.MAX_BYTES;chunk=data[off:]
                jid='transfer-'+str(counter)
                replay=adapter.call('pc_submit',{'id':jid,'action':'upload_chunk','args':{'uploadId':uid,'offsetBytes':off,'contentBase64':base64.b64encode(chunk).decode(),'chunkSha256':u.sha(chunk)}})
                self.assertTrue(replay['duplicate'])
                for record in w.journal.values():
                    if record['action']=='upload_chunk':self.assertNotIn('contentBase64',record['job']['args'])
                call('upload_commit',{'uploadId':uid});info=call('file_info',{'path':str(path)})
                self.assertEqual(info['sha256'],u.sha(data));self.assertEqual(path.read_bytes(),data)
            finally: server.close();w.stop();w.pool.shutdown(wait=True,cancel_futures=True)

class NativePreviewTests(unittest.TestCase):
    @unittest.skipUnless(os.name=='nt','Windows native helper')
    def test_native_preview_and_foreground_guard_without_input(self):
        runtime=Path(__file__).parents[1];csc=Path(os.environ['WINDIR'])/'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
        with tempfile.TemporaryDirectory() as temp:
            exe=Path(temp)/'helper.exe';image=Path(temp)/'fixture.bmp'
            compile=subprocess.run([str(csc),'/nologo','/target:winexe','/out:'+str(exe),'/r:System.Windows.Forms.dll','/r:System.Drawing.dll','/r:System.Web.Extensions.dll',str(runtime/'DesktopAutomation.cs')],capture_output=True,text=True,creationflags=subprocess.CREATE_NO_WINDOW)
            self.assertEqual(compile.returncode,0,compile.stdout+compile.stderr)
            # Generate known test pixels, never capture the user's desktop.
            import struct
            width,height=3000,2000;pixels=bytes([140,80,30])*width*height
            header=b'BM'+struct.pack('<IHHI',54+len(pixels),0,0,54)+struct.pack('<IiiHHIIiiII',40,width,height,1,24,0,len(pixels),0,0,0,0)
            image.write_bytes(header+pixels)
            def request(action,args):
                p=subprocess.run([str(exe)],input=json.dumps({'action':action,'args':args}),capture_output=True,text=True,encoding='utf-8',timeout=15,creationflags=subprocess.CREATE_NO_WINDOW)
                return json.loads(p.stdout)
            result=request('image_preview',{'path':str(image),'maxWidth':800,'maxHeight':600})
            self.assertTrue(result['ok'],result);self.assertEqual(result['result']['image_width'],800)
            self.assertLessEqual(len(result['result']['jpeg_base64']),768*1024)
            # A nonexistent expected window is rejected before any SendInput.
            result=request('mouse_move',{'x':0,'y':0,'expectedHwnd':'9223372036854775807'})
            self.assertFalse(result['ok']);self.assertIn(result['error']['code'],('foreground_window_changed','ui_permission_or_desktop_unavailable'))
if __name__=='__main__':unittest.main()
