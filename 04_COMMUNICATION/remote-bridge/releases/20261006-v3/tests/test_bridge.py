import base64
import datetime as dt
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest
import urllib.request
import urllib.error
from unittest.mock import patch

spec=importlib.util.spec_from_file_location('bridge_worker',Path(__file__).parents[1]/'bridge_worker.py')
b=importlib.util.module_from_spec(spec); spec.loader.exec_module(b)

def job(action='status', identity='job001'):
    at=dt.datetime.now(dt.timezone.utc)
    return {'id':identity,'target':b.PROJECT,'action':action,'createdAt':at.isoformat(),
        'expiresAt':(at+dt.timedelta(days=1)).isoformat(),'args':{}}

class FakeGitHub:
    def __init__(self,jobs): self.jobs=jobs; self.published=[]; self.fail=False
    def read_bytes(self,path): return json.dumps({'schemaVersion':1,'jobs':self.jobs}).encode()
    def publish(self,path,result):
        if self.fail: raise b.BridgeError('github_network_error')
        self.published.append((path,result))

class BridgeTests(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory(); self.addCleanup(self.temp.cleanup)
        self.root=Path(self.temp.name); self.project=self.root/'project'; self.project.mkdir()
        self.home=self.root/'bridge'; self.home.mkdir()
        b.atomic(self.home/'config.json',{'deviceId':'test-device','hubRepository':b.HUB,
            'projectRoot':str(self.project),'python':'python','approvedBundleSha256':[]})
    def test_job_rejects_arbitrary_command_and_paths(self):
        for action in ('shell','powershell','download','self_update'):
            with self.assertRaises(b.BridgeError): b.validate_job(job(action))
        value=job();value['args']={'command':'Remove-Item'}
        with self.assertRaises(b.BridgeError): b.validate_job(value)
        for path in ('../private','C:/Windows/a','src/../../x','/tmp/x','src\\a','src//x'):
            with self.assertRaises(b.BridgeError): b.inside(self.project,path)
    def test_job_expiry_and_unknown_fields(self):
        value=job();value['expiresAt']='2000-01-01T00:00:00Z'
        with self.assertRaises(b.BridgeError): b.validate_job(value)
        value=job();value['token']='secret'
        with self.assertRaises(b.BridgeError): b.validate_job(value)
    def bundle(self, rows):
        return json.dumps({'schemaVersion':1,'project':b.PROJECT,'files':rows}).encode()
    def row(self,path,old,new):
        return {'path':path,'baseSha256':b.digest(old) if old is not None else None,
            'newSha256':b.digest(new),'contentBase64':base64.b64encode(new).decode()}
    def test_bundle_preflight_preserves_all_local_edits(self):
        path=self.project/'src/user_library.py';path.parent.mkdir();path.write_bytes(b'old')
        bad=self.project/'ui/listening_library.html';bad.parent.mkdir();bad.write_bytes(b'user changed')
        raw=self.bundle([self.row('src/user_library.py',b'old',b'new'),self.row('ui/listening_library.html',b'expected',b'new')])
        with self.assertRaises(b.BridgeError): b.apply_bundle(raw,[b.digest(raw)],self.project,self.home/'state')
        self.assertEqual(path.read_bytes(),b'old');self.assertEqual(bad.read_bytes(),b'user changed')
    def test_bundle_needs_local_hash_approval_and_disallows_secrets(self):
        raw=self.bundle([self.row('src/user_library.py',None,b'new')])
        with self.assertRaises(b.BridgeError): b.apply_bundle(raw,[],self.project,self.home/'state')
        raw=self.bundle([self.row('output/cast_lock.json',None,b'{}')])
        with self.assertRaises(b.BridgeError): b.apply_bundle(raw,[b.digest(raw)],self.project,self.home/'state')
    def test_bundle_replay_is_idempotent_and_retains_backup(self):
        path=self.project/'src/user_library.py';path.parent.mkdir();path.write_bytes(b'old')
        raw=self.bundle([self.row('src/user_library.py',b'old',b'new')]);sha=b.digest(raw)
        first=b.apply_bundle(raw,[sha],self.project,self.home/'state')
        self.assertEqual(first['changedFiles'],1)
        self.assertEqual((self.home/'state/backups'/sha/'src/user_library.py').read_bytes(),b'old')
        second=b.apply_bundle(raw,[sha],self.project,self.home/'state')
        self.assertEqual(second['changedFiles'],0)
    def test_network_failure_does_not_reexecute_operation(self):
        client=FakeGitHub([job()]);client.fail=True; worker=b.Worker(self.home,client)
        with patch.object(b,'perform',return_value={'safe':True}) as perform:
            with self.assertRaises(b.BridgeError): worker.tick()
            client.fail=False;worker.tick()
            self.assertEqual(perform.call_count,1)
        self.assertEqual(worker.journal['job001']['state'],'published')
    def test_crash_interrupted_job_not_replayed(self):
        value=job();client=FakeGitHub([value]); worker=b.Worker(self.home,client)
        worker.journal[value['id']]={'jobHash':b.job_hash(value),'state':'started'};worker.checkpoint()
        with patch.object(b,'perform') as perform: worker.tick();perform.assert_not_called()
        self.assertEqual(client.published[0][1]['outcome'],'interrupted')
    def test_result_is_also_canonical_communication_record(self):
        client=FakeGitHub([job()]);worker=b.Worker(self.home,client)
        with patch.object(b,'perform',return_value={'readyChapters':3}):worker.tick()
        records=[(p,r) for p,r in client.published if p.endswith('/content.json')]
        self.assertEqual(len(records),1);path,row=records[0]
        self.assertEqual(row['recordType'],'task_exchange');self.assertEqual(row['status'],'completed')
        self.assertEqual(row['response']['source'],'project_bridge_operational_result')
        content=json.dumps(row,ensure_ascii=False,sort_keys=True,indent=2).encode()
        self.assertIn(b.digest(content),path)
        before=len(client.published);worker.tick();self.assertEqual(len(client.published),before)
    def test_reused_id_with_different_job_refused(self):
        value=job();client=FakeGitHub([value]);worker=b.Worker(self.home,client);worker.tick()
        value['action']='locate_reference_inventory'
        with self.assertRaises(b.BridgeError):worker.tick()
    def test_public_status_cannot_leak_dialogue_tokens_or_audio(self):
        run=self.project/'output/production_15_24';run.mkdir(parents=True)
        b.atomic(run/'status.json',{'stage':'generating','current':144,'dialogue':'PRIVATE','token':'SECRET',
            'chapters':{'144':{'status':'ready','text':'RAW NOVEL'}}})
        value=b.status_snapshot(self.project);serialized=json.dumps(value)
        for word in ('PRIVATE','SECRET','RAW NOVEL'):self.assertNotIn(word,serialized)
        self.assertEqual(value['readyChapterNumbers'],[144])
    def test_expired_auth_is_reloaded_from_local_login(self):
        client=b.GitHub();client.token='expired';client.auth_checked=True
        error=urllib.error.HTTPError('https://api.github.com',401,'Unauthorized',{},None)
        with patch.object(b.urllib.request,'urlopen',side_effect=error):
            with self.assertRaises(b.BridgeError):client.request('GET','')
        self.assertIsNone(client.token);self.assertFalse(client.auth_checked)
    def test_feedback_auth_cors_and_dedup(self):
        server=b.FeedbackServer(self.project);self.addCleanup(server.close)
        url='http://127.0.0.1:'+str(server.server.server_port)+'/feedback'
        payload={'edition':'edition1','chapter':144,'speaker':'나나호시','score':2,'tags':['emotion'],'note':'Private voice feedback'}
        headers={'Content-Type':'application/json','Origin':'null','X-Bridge-Token':server.token}
        def post(h=headers):
            with urllib.request.urlopen(urllib.request.Request(url,data=json.dumps(payload).encode(),headers=h),timeout=5) as r:return json.loads(r.read())
        first=post();second=post();self.assertEqual(first['record']['id'],second['record']['id'])
        self.assertEqual(len(server.read_rows()),1)
        with self.assertRaises(urllib.error.HTTPError):post({'Content-Type':'application/json','Origin':'null'})
        with self.assertRaises(urllib.error.HTTPError):post({**headers,'Origin':'https://evil.example'})
    def test_feedback_strict_types_and_bounds(self):
        valid={'edition':'edition1','chapter':144,'speaker':'나나호시','score':2,'tags':['emotion'],'note':''}
        for key,bad in [('score',True),('score',11),('chapter',True),('note','a'*1001),('tags',['shell'])]:
            with self.assertRaises(b.BridgeError):b.validate_feedback({**valid,key:bad})
    def test_disconnect_does_not_fetch_queue(self):
        b.atomic(self.home/'state/disconnected.flag',{})
        client=FakeGitHub([job()]);worker=b.Worker(self.home,client)
        with patch.object(client,'read_bytes') as fetch:worker.tick();fetch.assert_not_called()
    def test_dependencies_do_not_run_after_failed_apply(self):
        first=job('apply_reviewed_bundle','apply1');first['args']={'bundlePath':'04_COMMUNICATION/remote-bridge/bundles/a.json','bundleSha256':'a'*64}
        second=job('organize_listening','arrange1');second['dependsOn']=['apply1']
        client=FakeGitHub([first,second]);worker=b.Worker(self.home,client)
        with patch.object(b,'perform',side_effect=b.BridgeError('bundle_not_locally_approved')) as perform:
            worker.tick();self.assertEqual(perform.call_count,1)
        self.assertEqual(worker.journal['arrange1']['result']['data']['code'],'prerequisite_failed')
    def test_missing_dependency_waits_without_marking_completed(self):
        value=job('organize_listening');value['dependsOn']=['missing']
        worker=b.Worker(self.home,FakeGitHub([value]))
        with patch.object(b,'perform') as perform:worker.tick();perform.assert_not_called()
        self.assertEqual(worker.journal,{})
        self.assertEqual(b.load(self.home/'state/ui_status.json')['pending'][0]['id'],'job001')
    def test_organize_rejects_partial_apply_and_busy(self):
        script=self.project/'src/user_library.py';script.parent.mkdir();script.write_text('# test')
        value=job('organize_listening')
        for payload in ({'busy':True},{'organization':{'applied':False},'shortcuts':{'applied':True}}):
            result=b.subprocess.CompletedProcess([],0,json.dumps(payload),'')
            with patch.object(b,'quiet_run',return_value=result):
                with self.assertRaises(b.BridgeError): b.perform(value,b.load(self.home/'config.json'),self.home/'state',FakeGitHub([]))
    def test_interlude_feedback_chapter_id_preserved(self):
        row={'edition':'edition1','chapter':900004,'speaker':'나ナホシ','score':8,'tags':['good'],'note':''}
        self.assertEqual(b.validate_feedback(row)['chapter'],900004)
        self.assertEqual(b.chapter_number(900004),900004)

if __name__=='__main__':unittest.main()

