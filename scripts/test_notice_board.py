import concurrent.futures
import json
from pathlib import Path, PureWindowsPath
import tempfile
import unittest
from unittest.mock import patch

from notice_board import Board, comparable_path, content_hash, inside


class NoticeBoardTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)
        self.base = self.root / "04_COMMUNICATION" / "announcements"
        (self.base / "notices").mkdir(parents=True)
        (self.base / "notices" / "N-0001.md").write_text("# 실제 공지\n내용\n", encoding="utf-8")
        self.manifest = {
            "schemaVersion": 1,
            "projects": [{"id": "A", "name": "프로젝트 A", "category": "개발", "required": True},
                         {"id": "B", "name": "프로젝트 B", "category": "개발", "required": True}],
            "participants": [{"id": "Sol", "name": "Sol", "mailbox": "../mailboxes/Sol.md"},
                             {"id": "Astra", "name": "Astra", "mailbox": "../mailboxes/Astra.md"},
                             {"id": "Nova", "name": "Nova", "mailbox": "../mailboxes/Account-B-Nova.md"}],
            "notices": [{"id": "N-0001", "revision": 1, "title": "작업 규칙", "path": "notices/N-0001.md",
                         "targets": ["A"], "required": True, "active": True,
                         "contentSha256": content_hash((self.base / "notices" / "N-0001.md").read_bytes())}]
        }
        self.write_manifest()

    def tearDown(self):
        self.temp.cleanup()

    def write_manifest(self):
        (self.base / "manifest.json").write_text(json.dumps(self.manifest, ensure_ascii=False), encoding="utf-8")

    def ack(self, board, actor="Sol", session="one", status="pending", revision=1, sha=None):
        return board.ack("A", actor, session, "N-0001", revision,
                         sha or self.manifest["notices"][0]["contentSha256"], status, "현재 작업 근거", ["관련 파일"])

    def test_unread_cannot_ack_and_pending_is_not_applied(self):
        board = Board(self.root)
        with self.assertRaisesRegex(ValueError, "check --read"):
            self.ack(board)
        self.assertIn("미확인", board.check("A", "Sol"))
        with self.assertRaises(ValueError):
            self.ack(board)  # Listing a hash is not reading the body.
        self.assertIn("실제 공지", board.check("A", "Sol", True))
        path = self.ack(board)
        record = json.loads(path.read_text(encoding="utf-8"))
        self.assertNotIn("appliedAt", record)
        self.assertEqual("pending", record["applicationStatus"])
        self.assertIn("pending", board.check("A", "Sol"))
        self.assertIn("Astra | 0 | 미확인", board.status())
        self.ack(board, status="applied")
        self.assertIn("appliedAt", json.loads(path.read_text(encoding="utf-8")))
        self.assertNotIn("실제 공지", board.check("A", "Sol", True))

    def test_content_change_same_revision_invalidates_read_and_receipt(self):
        board = Board(self.root)
        board.check("A", "Sol", True)
        self.ack(board, status="applied")
        old_hash = self.manifest["notices"][0]["contentSha256"]
        (self.base / "notices" / "N-0001.md").write_text("# 변경된 공지\n", encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "hash"):
            Board(self.root)
        self.manifest["notices"][0]["contentSha256"] = content_hash((self.base / "notices" / "N-0001.md").read_bytes())
        self.write_manifest()
        current = Board(self.root)
        self.assertEqual(1, len(current.receipts()))  # Old evidence is retained.
        self.assertIn("미확인", current.check("A", "Sol"))
        self.assertIn("| Sol | 0 | 미확인 |", current.status())
        with self.assertRaisesRegex(ValueError, "변경"):
            self.ack(current, sha=old_hash)
        with self.assertRaisesRegex(ValueError, "다릅니다"):
            self.ack(current)
        self.assertIn("변경된 공지", current.check("A", "Sol", True))
        self.ack(current)

    def test_revision_change_excludes_old_record(self):
        board = Board(self.root)
        board.check("A", "Sol", True)
        self.ack(board, status="applied")
        self.manifest["notices"][0]["revision"] = 2
        self.write_manifest()
        current = Board(self.root)
        self.assertEqual(1, len(current.receipts()))
        self.assertIn("미확인", current.check("A", "Sol"))
        with self.assertRaises(ValueError):
            self.ack(current)

    def test_wrong_target_cannot_ack_or_be_counted(self):
        board = Board(self.root)
        board.check("A", "Sol", True)
        self.ack(board, status="applied")
        with self.assertRaisesRegex(ValueError, "대상"):
            board.ack("B", "Sol", "one", "N-0001", 1, self.manifest["notices"][0]["contentSha256"], "applied", "틀린 대상")
        self.manifest["notices"][0]["targets"] = ["B"]
        self.write_manifest()
        self.assertIn("| Sol | 0 | 미확인 |", Board(self.root).status())

    def test_concurrent_writers_keep_distinct_owned_files(self):
        board = Board(self.root)
        board.check("A", "Sol", True)
        board.check("A", "별도 AI / 연구", True)
        tasks = [("Sol", "one"), ("Sol", "two"), ("별도 AI / 연구", "one")]
        tasks.extend(("Sol", "parallel-" + str(i)) for i in range(5))
        with concurrent.futures.ThreadPoolExecutor(max_workers=8) as pool:
            paths = list(pool.map(lambda pair: self.ack(board, *pair), tasks))
        self.assertEqual(8, len(set(paths)))
        self.assertEqual(8, len(board.receipts()))
        self.assertFalse(list(self.base.rglob("*.tmp")))

    def test_long_windows_resolved_destination_is_inside_ordinary_root(self):
        # Deterministic version of Python 3.13's observed Windows mkdir/resolve race:
        # missing destination returns \\?\D:\..., existing root returns D:\....
        for resolved_root, resolved_target in (
            (PureWindowsPath(r"D:\test\announcements"), PureWindowsPath(r"\\?\D:\test\announcements\receipts\record.json")),
            (PureWindowsPath(r"\\server\share\announcements"), PureWindowsPath(r"\\?\UNC\server\share\announcements\receipts\record.json")),
            (PureWindowsPath(r"\\?\D:\test\announcements"), PureWindowsPath(r"D:\test\announcements\receipts\record.json")),
        ):
            with self.subTest(root=resolved_root, target=resolved_target):
                with patch.object(Path, "resolve", side_effect=[resolved_target, resolved_root]):
                    self.assertEqual(resolved_target, inside(self.base, "receipts/record.json"))
                self.assertEqual(comparable_path(resolved_target), comparable_path(PureWindowsPath(str(resolved_root)) / "receipts/record.json"))

    def test_windows_anchor_normalization_does_not_allow_escape_or_device_paths(self):
        resolved_root = PureWindowsPath(r"D:\test\announcements")
        for target in (r"\\?\D:\test\announcements-other\record.json", r"\\?\E:\test\announcements\record.json",
                       r"\\?\GLOBALROOT\Device\HarddiskVolume1\record.json"):
            with self.subTest(target=target):
                with patch.object(Path, "resolve", side_effect=[PureWindowsPath(target), resolved_root]):
                    with self.assertRaisesRegex(ValueError, "공지방 밖"):
                        inside(self.base, "receipts/record.json")
        for relative in ("C:/outside.json", "notices/file.md:stream", "../outside.json"):
            with self.subTest(relative=relative), self.assertRaisesRegex(ValueError, "상대 경로"):
                inside(self.base, relative)

    def test_symlink_outside_root_remains_rejected(self):
        outside = self.root / "outside"
        outside.mkdir()
        link = self.base / "linked"
        try:
            link.symlink_to(outside, target_is_directory=True)
        except (OSError, NotImplementedError):
            self.skipTest("이 환경에는 디렉터리 symlink 생성 권한이 없습니다.")
        with self.assertRaisesRegex(ValueError, "공지방 밖"):
            inside(self.base, "linked/record.json")

    def test_line_endings_and_bom_do_not_change_hash(self):
        self.assertEqual(content_hash(b"a\nb\n"), content_hash(b"\xef\xbb\xbfa\r\nb\r\n"))
        self.assertEqual(content_hash(b"a\nb\n"), content_hash(b"a\rb\r"))

    def test_duplicates_and_path_traversal_fail_validation(self):
        self.manifest["notices"].append(dict(self.manifest["notices"][0]))
        self.write_manifest()
        with self.assertRaisesRegex(ValueError, "중복"):
            Board(self.root)
        self.manifest["notices"].pop()
        self.manifest["notices"][0]["path"] = "notices/../../outside.md"
        self.write_manifest()
        with self.assertRaisesRegex(ValueError, "상대 경로"):
            Board(self.root)
        self.manifest["notices"][0]["path"] = "notices/N-0001.md"
        self.write_manifest()
        with self.assertRaises(ValueError):
            Board(self.root).check("../A", "Sol")

    def test_receipt_path_cannot_impersonate_another_owner(self):
        board = Board(self.root)
        board.check("A", "Sol", True)
        path = self.ack(board)
        record = json.loads(path.read_text(encoding="utf-8"))
        record["actorId"] = "Astra"
        path.write_text(json.dumps(record), encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "소유자"):
            board.receipts()

    def test_named_participants_optional_denominator_and_preserved_read_time(self):
        self.manifest["participants"].append({"id": "/root/reader", "name": "문서 검토 담당", "mailbox": ""})
        self.manifest["projects"][1]["required"] = False
        self.manifest["notices"][0]["targets"] = ["*"]
        self.write_manifest()
        board = Board(self.root)
        board.check("A", "/root/reader", True)
        path = self.ack(board, actor="/root/reader")
        original = json.loads(path.read_text(encoding="utf-8"))["checkedAt"]
        board.check("A", "/root/reader", True)
        self.ack(board, actor="/root/reader", status="applied")
        self.assertEqual(original, json.loads(path.read_text(encoding="utf-8"))["checkedAt"])
        text = board.status()
        self.assertIn("| r1 | 1 / 1 |", text)
        self.assertIn("문서 검토 담당", text)
        self.assertIn("개발 · 선택", text)
        self.assertIn("KST", text)


if __name__ == "__main__":
    unittest.main()
