"""Local recorder recovery tests; only owned synthetic D fixtures are written."""
import copy
import hashlib
import io
import json
import os
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

import project_notice as helper
from test_task_exchange import sample


class TaskRecoveryTests(unittest.TestCase):
    def setUp(self):
        base = Path(os.environ.get("TASK_RECORD_TEST_ROOT", str(Path(__file__).resolve().parents[2] / "checks" / "recorder-recovery-fixtures")))
        self.assertEqual("D:", base.drive.upper(), "Use the authorized D fixture root")
        base.mkdir(parents=True, exist_ok=True)
        self.temporary = tempfile.TemporaryDirectory(dir=base, prefix="owned-record-")
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name) / "_통합소통"
        notices = self.root / "받은공지"
        notices.mkdir(parents=True)
        (notices / "manifest.json").write_text(json.dumps({"schemaVersion": 1, "deliveredProjectId": "Control-Tower", "notices": []}), encoding="utf-8")

    def recovery_api(self):
        self.assertTrue(callable(getattr(helper, "record_task", None)), "Idempotent record result is missing")
        self.assertTrue(callable(getattr(helper, "latest_task", None)), "Explicit latest lookup is missing")

    def test_identical_retry_returns_already_stored_without_bytes_mtime_or_attributes_change(self):
        self.recovery_api()
        value = sample()
        first = helper.record_task(self.root, value)
        path = Path(first["path"])
        before = path.read_bytes(), path.stat().st_mtime_ns, getattr(path.stat(), "st_file_attributes", 0)
        retry = helper.record_task(self.root, copy.deepcopy(value))
        self.assertEqual("stored", first["status"])
        self.assertEqual("already_stored", retry["status"])
        self.assertEqual(before, (path.read_bytes(), path.stat().st_mtime_ns, getattr(path.stat(), "st_file_attributes", 0)))
        self.assertEqual(hashlib.sha256(before[0]).hexdigest(), retry["contentSha256"])
        self.assertEqual(1, len(list(path.parent.glob("*.json"))))

    def test_stale_identical_retry_preserves_newer_revision_and_does_not_repeat_work(self):
        self.recovery_api()
        old = sample()
        original = helper.record_task(self.root, old)
        new = copy.deepcopy(old)
        new["revision"] = 2
        new["response"]["summary"] = "Already delivered synthetic answer"
        helper.record_task(self.root, new)
        retry = helper.record_task(self.root, old)
        self.assertEqual("already_stored", retry["status"])
        self.assertEqual(original["contentSha256"], retry["contentSha256"])
        latest = helper.latest_task(self.root, project="Control-Tower", actor="/root", record_id="REQ-1")
        self.assertEqual(2, latest["revision"])
        self.assertEqual(2, len(list((self.root / "보낼자료").glob("*.json"))))

    def test_same_revision_different_payload_is_rejected_without_overwrite(self):
        self.recovery_api()
        value = sample()
        first = helper.record_task(self.root, value)
        path = Path(first["path"])
        before = path.read_bytes()
        value["response"]["summary"] = "Unconfirmed replacement"
        with self.assertRaises(ValueError):
            helper.record_task(self.root, value)
        self.assertEqual(before, path.read_bytes())

    def test_latest_has_explicit_owner_selectors_and_exact_raw_file_hash(self):
        self.recovery_api()
        mine = sample()
        first = helper.record_task(self.root, mine)
        other = copy.deepcopy(mine)
        other["actorId"] = "another-AI"
        helper.record_task(self.root, other)
        result = helper.latest_task(self.root, project="Control-Tower", actor="/root", record_id="REQ-1")
        self.assertEqual("found", result["status"])
        self.assertEqual("/root", result["actorId"])
        self.assertEqual(first["contentSha256"], result["contentSha256"])
        self.assertEqual("in_progress", result["taskStatus"])
        self.assertEqual(1, result["revision"])

    def test_latest_cli_requires_every_selector_and_emits_only_lookup_json(self):
        self.recovery_api()
        helper.record_task(self.root, sample())
        with patch("sys.stdout", new=io.StringIO()) as out:
            self.assertEqual(0, helper.main(["--root", str(self.root), "latest", "--project", "Control-Tower", "--actor", "/root", "--record-id", "REQ-1"]))
        result = json.loads(out.getvalue())
        self.assertEqual("found", result["status"])
        self.assertNotIn("response", result)
        for missing in ["--project", "--actor", "--record-id"]:
            arguments = ["latest", "--project", "Control-Tower", "--actor", "/root", "--record-id", "REQ-1"]
            i = arguments.index(missing)
            del arguments[i:i+2]
            with patch("sys.stderr", new=io.StringIO()), self.assertRaises(SystemExit):
                helper.main(arguments)

    def test_actor_or_project_body_collision_under_existing_filename_fails_closed(self):
        self.recovery_api()
        for field, replacement in [("actorId", "another-AI"), ("projectId", "Threads")]:
            value = sample()
            first = helper.record_task(self.root, value)
            path = Path(first["path"])
            before = path.read_bytes()
            invalid = {**value, field: replacement}
            path.write_text(json.dumps(invalid), encoding="utf-8")
            try:
                with self.assertRaises(ValueError):
                    helper.latest_task(self.root, project="Control-Tower", actor="/root", record_id="REQ-1")
                with self.assertRaises(ValueError):
                    helper.record_task(self.root, value)
            finally:
                path.write_bytes(before)

    def test_duplicate_same_revision_with_different_body_is_not_silently_selected(self):
        self.recovery_api()
        value = sample()
        helper.record_task(self.root, value)
        value["response"]["summary"] = "Conflicting duplicate"
        collision = self.root / "보낼자료" / ("task-" + "0" * 64 + "-r1.json")
        collision.write_text(json.dumps(value), encoding="utf-8")
        with self.assertRaises(ValueError):
            helper.latest_task(self.root, project="Control-Tower", actor="/root", record_id="REQ-1")

    def test_latest_not_found_does_not_create_outbox_or_infer_an_actor(self):
        self.recovery_api()
        result = helper.latest_task(self.root, project="Control-Tower", actor="/root", record_id="missing")
        self.assertEqual("not_found", result["status"])
        self.assertFalse((self.root / "보낼자료").exists())
        with self.assertRaises(ValueError):
            helper.latest_task(self.root, project="Control-Tower", actor="", record_id="REQ-1")
        with self.assertRaises(ValueError):
            helper.latest_task(self.root, project="Threads", actor="/root", record_id="REQ-1")

    def test_latest_rejects_inconsistent_received_command_history(self):
        self.recovery_api()
        value = sample()
        helper.record_task(self.root, value)
        newer = copy.deepcopy(value)
        newer["revision"] = 2
        result = helper.record_task(self.root, newer)
        newer["request"]["summary"] = "Rewritten command"
        Path(result["path"]).write_text(json.dumps(newer), encoding="utf-8")
        with self.assertRaises(ValueError):
            helper.latest_task(self.root, project="Control-Tower", actor="/root", record_id="REQ-1")

    def test_duplicate_json_field_is_not_a_verified_record(self):
        self.recovery_api()
        value = sample()
        result = helper.record_task(self.root, value)
        path = Path(result["path"])
        text = path.read_text(encoding="utf-8")
        path.write_text(text.replace('"revision": 1', '"revision": 99, "revision": 1'), encoding="utf-8")
        with self.assertRaises(ValueError):
            helper.latest_task(self.root, project="Control-Tower", actor="/root", record_id="REQ-1")


    def test_latest_conflict_cli_returns_held_json_not_success_or_guessed_state(self):
        self.recovery_api()
        value = sample()
        result = helper.record_task(self.root, value)
        value["actorId"] = "mismatched-actor"
        Path(result["path"]).write_text(json.dumps(value), encoding="utf-8")
        with patch("sys.stdout", new=io.StringIO()) as out, patch("sys.stderr", new=io.StringIO()):
            self.assertEqual(1, helper.main(["--root", str(self.root), "latest", "--project", "Control-Tower", "--actor", "/root", "--record-id", "REQ-1"]))
        self.assertTrue(out.getvalue().strip(), "Lookup conflicts must emit machine-readable held JSON")
        public = json.loads(out.getvalue())
        self.assertEqual("held", public["status"])
        self.assertNotIn("revision", public)
        self.assertNotIn("contentSha256", public)

    def test_unrelated_legacy_actor_does_not_block_exact_owned_lookup_or_retry(self):
        self.recovery_api()
        mine = sample()
        saved = helper.record_task(self.root, mine)
        legacy = copy.deepcopy(mine)
        legacy.update(actorId="codex-continuity-20261007", recordId="continuous-orchestration-20261007")
        path = self.root / "보낼자료" / "task-continuous-orchestration-20261007-r1.json"
        path.write_text(json.dumps(legacy, ensure_ascii=False, indent=2), encoding="utf-8")
        def snapshot(p):
            info = p.stat()
            return p.read_bytes(), info.st_mtime_ns, getattr(info, "st_file_attributes", 0)
        mine_path = Path(saved["path"])
        before = snapshot(mine_path), snapshot(path)
        try:
            latest = helper.latest_task(self.root, project="Control-Tower", actor="/root", record_id="REQ-1")
            retry = helper.record_task(self.root, mine)
        except ValueError:
            self.fail("Unrelated valid legacy filename must not block exact owned record recovery")
        self.assertEqual("found", latest["status"])
        self.assertEqual(saved["contentSha256"], latest["contentSha256"])
        self.assertEqual("already_stored", retry["status"])
        self.assertEqual(before, (snapshot(mine_path), snapshot(path)))

    def test_requested_key_under_legacy_wrong_filename_is_held_not_adopted(self):
        self.recovery_api()
        folder = self.root / "보낼자료"
        folder.mkdir()
        path = folder / "task-REQ-1-r1.json"
        path.write_text(json.dumps(sample()), encoding="utf-8")
        before = path.read_bytes()
        with self.assertRaises(ValueError):
            helper.latest_task(self.root, project="Control-Tower", actor="/root", record_id="REQ-1")
        with self.assertRaises(ValueError):
            helper.record_task(self.root, sample())
        self.assertEqual(before, path.read_bytes())
        self.assertEqual(1, len(list(folder.glob("*.json"))))

if __name__ == "__main__":
    unittest.main()



