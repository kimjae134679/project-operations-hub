import json
import os
from pathlib import Path
import sys
import tempfile
import unittest
from unittest.mock import patch
sys.path.insert(0,str(Path(__file__).parents[1]))
import universal_actions as u

class FileCrudTests(unittest.TestCase):
    def setUp(self):
        self.tmp=tempfile.TemporaryDirectory();self.addCleanup(self.tmp.cleanup)
        self.root=Path(self.tmp.name);self.state=self.root/'state'
        self.source=self.root/'source.txt';self.source.write_bytes(b'original')
        self.digest=u.file_sha(self.source);self.config={}
    def call(self,action,**args):return u.perform({'action':action,'args':args},self.config,self.state)
    def test_delete_restore_no_overwrite(self):
        deleted=self.call('delete_file',path=str(self.source),expectedSha256=self.digest)
        self.assertFalse(self.source.exists())
        self.source.write_bytes(b'new')
        with self.assertRaises(u.ActionError):self.call('restore_file',trashId=deleted['trashId'],expectedSha256=self.digest)
        self.assertEqual(self.source.read_bytes(),b'new');self.source.unlink()
        self.call('restore_file',trashId=deleted['trashId'],expectedSha256=self.digest)
        self.assertEqual(self.source.read_bytes(),b'original')
        with self.assertRaises(u.ActionError):self.call('restore_file',trashId=deleted['trashId'],expectedSha256=self.digest)
    def test_move_preconditions_and_destination(self):
        target=self.root/'target.txt';target.write_bytes(b'existing')
        with self.assertRaises(u.ActionError):self.call('move_file',path=str(self.source),destination=str(target),expectedSha256=self.digest)
        self.assertEqual(target.read_bytes(),b'existing');target.unlink()
        with self.assertRaises(u.ActionError):self.call('move_file',path=str(self.source),destination=str(target),expectedSha256='0'*64)
        self.call('move_file',path=str(self.source),destination=str(target),expectedSha256=self.digest)
        self.assertFalse(self.source.exists());self.assertEqual(target.read_bytes(),b'original')
    def test_protection_and_nonrecursive_directory(self):
        self.config={'protectedRoots':[str(self.source)]}
        with self.assertRaises(u.ActionError):self.call('delete_file',path=str(self.source),expectedSha256=self.digest)
        self.config={};directory=self.root/'folder'
        self.call('make_dir',path=str(directory));(directory/'child').write_text('safe')
        with self.assertRaises(u.ActionError):self.call('delete_file',path=str(directory),expectedSha256=self.digest)
        self.assertTrue((directory/'child').exists())
    def test_invalid_and_foreign_ticket(self):
        with self.assertRaises(u.ActionError):self.call('restore_file',trashId='../escape',expectedSha256=self.digest)
        result=self.call('delete_file',path=str(self.source),expectedSha256=self.digest)
        metadata=self.state/'file_trash'/result['trashId']/'metadata.json'
        record=json.loads(metadata.read_text());record['ownerNonce']='foreign';metadata.write_text(json.dumps(record))
        with self.assertRaises(u.ActionError):self.call('restore_file',trashId=result['trashId'],expectedSha256=self.digest)
    def test_reparse_ancestor_rejected_without_resolving(self):
        original=Path.lstat
        def fake(path,*args,**kwargs):
            info=original(path,*args,**kwargs)
            if path==self.root:
                class Reparse:
                    st_mode=info.st_mode
                    st_file_attributes=0x400
                return Reparse()
            return info
        with patch.object(Path,'lstat',fake):
            with self.assertRaisesRegex(u.ActionError,'reparse'):u.mutation_path(str(self.source),{})
    def test_failed_copy_keeps_source_and_cleans_destination(self):
        target=self.root/'target'
        with patch.object(u.shutil,'copyfileobj',side_effect=OSError('disk error')):
            with self.assertRaises(OSError):self.call('move_file',path=str(self.source),destination=str(target),expectedSha256=self.digest)
        self.assertTrue(self.source.exists());self.assertFalse(target.exists())
    def test_prepared_ticket_after_crash_is_restorable(self):
        result=self.call('delete_file',path=str(self.source),expectedSha256=self.digest)
        metadata=self.state/'file_trash'/result['trashId']/'metadata.json'
        record=json.loads(metadata.read_text());record['stage']='prepared';metadata.write_text(json.dumps(record))
        self.call('restore_file',trashId=result['trashId'],expectedSha256=self.digest)
        self.assertEqual(self.source.read_bytes(),b'original')

if __name__=='__main__':unittest.main()
