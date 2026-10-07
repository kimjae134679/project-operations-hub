"""명령·답변 기록의 수집 호환성과 이력 보존 검증."""
import copy
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import io
import os
import project_notice as helper

def sample():
    return {"schemaVersion": 1, "recordType": "task_exchange", "recordId": "REQ-1", "revision": 1,
            "title": "새로고침 개선", "projectId": "Control-Tower", "actorId": "/root",
            "sessionId": "test-session", "receivedAt": "2026-10-06T05:37:58+09:00",
            "updatedAt": "2026-10-06T05:40:00+09:00",
            "request": {"summary": "부드럽게 새로고침", "details": "한 바퀴 회전", "source": "current_chat"},
            "response": {"summary": "최종 답변 대기", "details": "", "source": "current_chat"},
            "status": "in_progress", "workDone": [],
            "verification": [{"name": "화면", "result": "not_run", "evidence": []}],
            "nextActions": ["화면 확인"], "blockers": [], "supersedes": []}

class TaskRecordTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix="communication-record-test-", dir=os.environ.get("TASK_RECORD_TEST_ROOT"))
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name) / "_통합소통"
        folder = self.root / "받은공지"
        folder.mkdir(parents=True)
        (folder / "manifest.json").write_text(json.dumps({"schemaVersion": 1, "deliveredProjectId": "Control-Tower", "notices": []}), encoding="utf-8")
    def invalid(self, change):
        value = sample()
        change(value)
        with self.assertRaises((ValueError, TypeError)):
            helper.validate_task(value)
    def test_valid_and_cli_readable_summary(self):
        source = self.root / "input.json"
        source.write_text(json.dumps(sample(), ensure_ascii=False), encoding="utf-8")
        with patch("sys.stdout", new=io.StringIO()) as out:
            self.assertEqual(helper.main(["render", "--input", str(source)]), 0)
        self.assertIn("받은 지시: 부드럽게 새로고침", out.getvalue())
        self.assertIn("AI 답변: 최종 답변 대기", out.getvalue())
        self.assertIn("진행 중", out.getvalue())
    def test_revision_keeps_original_and_rejects_collision(self):
        value = sample()
        first = helper.write_task(self.root, helper.validate_task(value))
        original = first.read_bytes()
        self.assertEqual(first, helper.write_task(self.root, value))
        conflicting = copy.deepcopy(value)
        conflicting["response"]["summary"] = "다른 내용"
        with self.assertRaises(ValueError):
            helper.write_task(self.root, conflicting)
        value = copy.deepcopy(value)
        value["revision"] = 2
        value["response"]["summary"] = "진행 답변을 전달함"
        value["updatedAt"] = "2026-10-06T06:00:00+09:00"
        second = helper.write_task(self.root, helper.validate_task(value))
        self.assertNotEqual(first, second)
        self.assertEqual(first.read_bytes(), original)
        self.assertEqual(helper.load_task(second)["revision"], 2)
    def test_revision_gap_and_command_rewrite_rejected(self):
        value = sample()
        helper.write_task(self.root, value)
        value["revision"] = 3
        with self.assertRaises(ValueError):
            helper.write_task(self.root, value)
        value["revision"] = 2
        value["request"]["summary"] = "다른 지시"
        with self.assertRaises(ValueError):
            helper.write_task(self.root, value)
    def test_actual_project_mismatch_rejected(self):
        value = sample()
        value["projectId"] = "Threads"
        with self.assertRaises(ValueError):
            helper.write_task(self.root, value)
        self.assertFalse((self.root / "보낼자료").exists())
    def test_path_id_and_bool_revision_rejected(self):
        self.invalid(lambda v: v.update(recordId="../escape"))
        self.invalid(lambda v: v.update(projectId="D:/elsewhere"))
        self.invalid(lambda v: v.update(revision=True))
        self.invalid(lambda v: v.update(schemaVersion=True))
    def test_timezone_and_order_required(self):
        self.invalid(lambda v: v.update(receivedAt="2026-10-06T05:37:58"))
        self.invalid(lambda v: v.update(updatedAt="2026-10-05T01:00:00+09:00"))
    def test_missing_and_unknown_fields_rejected(self):
        self.invalid(lambda v: v.pop("response"))
        self.invalid(lambda v: v.update(extra="unknown"))
        self.invalid(lambda v: v["request"].update(extra="unknown"))
        self.invalid(lambda v: v["response"].update(details=[]))
    def test_progress_next_step_and_blocker_required(self):
        self.invalid(lambda v: v.update(nextActions=[]))
        self.invalid(lambda v: v.update(status="blocked"))
    def test_completed_has_no_unfinished_work(self):
        self.invalid(lambda v: v.update(status="completed"))
        value = sample()
        value.update(status="completed", nextActions=[])
        helper.validate_task(value)
        value["blockers"] = ["막힘"]
        with self.assertRaises(ValueError):
            helper.validate_task(value)
    def test_executed_verification_requires_evidence(self):
        self.invalid(lambda v: v["verification"][0].update(result="pass"))
        value = sample()
        value["verification"][0].update(result="pass", evidence=["스크린샷 확인"])
        helper.validate_task(value)
    def test_over_collection_size_rejected_before_write(self):
        self.invalid(lambda v: v["request"].update(details="한" * 400000))
        path = self.root / "large.json"
        path.write_bytes(b" " * (helper.MAX_RECORD_BYTES + 1))
        with self.assertRaises(ValueError):
            helper.load_task(path)
    def test_helper_record_command_and_validation_are_distinct(self):
        path = self.root / "input.json"
        path.write_text(json.dumps(sample()), encoding="utf-8")
        with patch("sys.stdout", new=io.StringIO()):
            self.assertEqual(helper.main(["validate", "--input", str(path)]), 0)
            self.assertFalse((self.root / "보낼자료").exists())
            self.assertEqual(helper.main(["--root", str(self.root), "record", "--input", str(path)]), 0)
        self.assertEqual(len(list((self.root / "보낼자료").glob("*.json"))), 1)

if __name__ == "__main__":
    unittest.main()
