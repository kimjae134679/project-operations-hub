import contextlib
import io
import json
from pathlib import Path, PureWindowsPath
import tempfile
import unittest
from unittest.mock import patch
from project_notice import main, owner, sha, comparable_path

class ProjectNoticeTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)
        (self.root/'받은공지').mkdir()
        self.body = '# 실제 공지\n작업 시작에 공지를 확인합니다.\n'
        self.notice = {'id':'N-0001','revision':1,'title':'공지','path':'N-0001.md','contentSha256':sha(self.body),'targets':['*'],'active':True}
        self.manifest = {'schemaVersion':1,'deliveredProjectId':'PhoneLOL','notices':[self.notice]}
        self.save()
    def tearDown(self): self.temp.cleanup()
    def save(self):
        (self.root/'받은공지/manifest.json').write_text(json.dumps(self.manifest),encoding='utf-8')
        (self.root/'받은공지/N-0001.md').write_text(self.body,encoding='utf-8')
    def run_cli(self,*args):
        with contextlib.redirect_stdout(io.StringIO()),contextlib.redirect_stderr(io.StringIO()):
            return main(['--root',str(self.root),*args])
    def ack(self,status='pending'):
        return self.run_cli('ack','--actor','나의 AI','--session','이번 작업','--notice','N-0001','--status',status,'--note','본인 적용 상태를 기록했다')
    def test_no_read_no_ack_and_actor_session_unicode_filename(self):
        self.assertEqual(1,self.ack())
        self.assertEqual(0,self.run_cli('check','--actor','나의 AI','--read'))
        self.assertEqual(0,self.ack())
        path=self.root/'확인기록/N-0001/r1'/f'{owner("나의 AI","이번 작업")}.json'
        record=json.loads(path.read_text(encoding='utf-8'))
        self.assertEqual('PhoneLOL',record['projectId'])
        self.assertEqual('pending',record['applicationStatus'])
        self.assertNotIn('appliedAt',record)
        self.assertEqual(0,self.ack('applied'))
        applied=json.loads(path.read_text(encoding='utf-8'))
        self.assertEqual(record['checkedAt'],applied['checkedAt'])
        self.assertIn('appliedAt',applied)
    def test_new_notice_hash_invalidates_old_read(self):
        self.run_cli('check','--actor','나의 AI','--read')
        self.body += '변경됨\n';self.notice['contentSha256']=sha(self.body);self.save()
        self.assertEqual(1,self.ack())
    def test_traversal_manifest_is_rejected(self):
        self.notice['path']='../../outside.md';self.save()
        self.assertEqual(1,self.run_cli('check','--actor','나의 AI','--read'))
    def test_long_windows_path_aliases_preserve_containment_only(self):
        ordinary=PureWindowsPath(r'D:\project\_통합소통')
        extended=PureWindowsPath(r'\\?\D:\project\_통합소통\확인기록\long.json')
        self.assertTrue(comparable_path(extended).is_relative_to(comparable_path(ordinary)))
        self.assertFalse(comparable_path(PureWindowsPath(r'\\?\D:\project\_통합소통-other\file.json')).is_relative_to(comparable_path(ordinary)))
        self.assertFalse(comparable_path(PureWindowsPath(r'\\?\GLOBALROOT\Device\HarddiskVolume1\file.json')).is_relative_to(comparable_path(ordinary)))

    def test_another_actor_cannot_use_my_read(self):
        self.run_cli('check','--actor','나의 AI','--read')
        self.assertEqual(1,self.run_cli('ack','--actor','다른 AI','--session','작업','--notice','N-0001','--status','applied','--note','대신 읽음이라고 주장'))

if __name__=='__main__': unittest.main()
