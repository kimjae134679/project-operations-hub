#!/usr/bin/env python3
"""File-backed notice board. Caller identity is declared, not authenticated by this CLI."""
import argparse
import hashlib
import json
import os
from pathlib import Path, PurePosixPath, PureWindowsPath
import re
import sys
import tempfile
from datetime import datetime, timedelta, timezone

STATUSES = {"pending", "applied", "not_applicable", "blocked"}


def now():
    return datetime.now(timezone.utc).isoformat()


def content_hash(data):
    text = data.decode("utf-8-sig").replace("\r\n", "\n").replace("\r", "\n")
    return hashlib.sha256(text.encode("utf-8")).hexdigest()


def safe_id(value):
    if not isinstance(value, str) or not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9_.-]{0,127}", value):
        raise ValueError("잘못된 ID: " + repr(value))
    return value


def identity(value):
    if not isinstance(value, str) or not value.strip() or len(value) > 256 or any(ord(c) < 32 for c in value):
        raise ValueError("작성자/session ID는 비어 있지 않은 256자 이하 문자열이어야 합니다.")
    return value


def digest(value):
    if not isinstance(value, str) or not re.fullmatch(r"[0-9a-f]{64}", value):
        raise ValueError("contentSha256는 소문자 SHA-256이어야 합니다.")
    return value


def comparable_path(path):
    r"""Compare resolved Windows drive/UNC paths independently of a long-path prefix.

    Windows resolve() may retain \\?\ on a not-yet-existing long destination while
    returning the ordinary spelling for its existing root. Keep the original
    resolved path for I/O; normalize only these equivalent anchors for comparison.
    Other device namespaces are deliberately not accepted as aliases.
    """
    if isinstance(path, PureWindowsPath):
        value = str(path)
        if value[:8].lower() == "\\\\?\\unc\\":
            value = "\\\\" + value[8:]
        elif (value.startswith("\\\\?\\") and len(value) >= 7
              and value[4].isascii() and value[4].isalpha() and value[5:7] == ":\\"):
            value = value[4:]
        return PureWindowsPath(value)
    return path


def inside(root, relative):
    parts = PurePosixPath(relative)
    if (not relative or "\\" in relative or ":" in relative or parts.is_absolute()
            or any(p in {"..", "."} for p in relative.split("/"))):
        raise ValueError("공지 경로는 내부 상대 경로여야 합니다.")
    target = (root / relative).resolve()
    if not comparable_path(target).is_relative_to(comparable_path(root.resolve())):
        raise ValueError("공지 경로가 공지방 밖을 가리킵니다.")
    return target


def atomic_json(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    fd, temp = tempfile.mkstemp(prefix=".notice-", suffix=".tmp", dir=path.parent)
    try:
        with os.fdopen(fd, "w", encoding="utf-8", newline="\n") as stream:
            json.dump(value, stream, ensure_ascii=False, indent=2)
            stream.write("\n")
            stream.flush()
            os.fsync(stream.fileno())
        os.replace(temp, path)
    finally:
        if os.path.exists(temp):
            os.unlink(temp)


def identity_hash(actor, session=None):
    return hashlib.sha256(json.dumps([actor, session], ensure_ascii=False).encode("utf-8")).hexdigest()


def timestamp(value):
    if not isinstance(value, str):
        raise ValueError("확인 시각이 없습니다.")
    parsed = datetime.fromisoformat(value.replace("Z", "+00:00"))
    if parsed.tzinfo is None:
        raise ValueError("확인 시각에 시간대가 필요합니다.")
    return parsed


def escaped(value):
    return str(value).replace("|", "\\|").replace("\n", " ").replace("\r", " ")


class Board:
    def __init__(self, root):
        self.root = Path(root).resolve() / "04_COMMUNICATION" / "announcements"
        self.manifest = json.loads((self.root / "manifest.json").read_text(encoding="utf-8-sig"))
        if self.manifest.get("schemaVersion") != 1:
            raise ValueError("지원하지 않는 manifest schemaVersion")
        self.projects = self.index("projects")
        self.participants = self.index("participants")
        self.notices = self.index("notices")
        for p in self.projects.values():
            if not isinstance(p.get("required"), bool):
                raise ValueError("project.required가 필요합니다.")
        for n in self.notices.values():
            if type(n.get("revision")) is not int or n["revision"] < 1:
                raise ValueError("공지 revision은 양의 정수여야 합니다.")
            if any(not isinstance(n.get(k), bool) for k in ("required", "active")):
                raise ValueError("공지 required/active가 필요합니다.")
            targets = n.get("targets")
            if not isinstance(targets, list) or not targets or any(not isinstance(t, str) for t in targets) or len(set(targets)) != len(targets):
                raise ValueError("공지 targets를 확인하세요.")
            if any(t != "*" and t not in self.projects for t in targets):
                raise ValueError("등록되지 않은 공지 대상입니다.")
            path = n.get("path")
            if not isinstance(path, str) or not path.startswith("notices/"):
                raise ValueError("공지 본문은 notices/ 안에 있어야 합니다.")
            file = inside(self.root, path)
            if not file.is_file() or content_hash(file.read_bytes()) != digest(n.get("contentSha256")):
                raise ValueError("공지 본문 hash가 manifest와 다릅니다: " + n["id"])

    def index(self, key):
        records = self.manifest.get(key)
        if not isinstance(records, list):
            raise ValueError(key + " 목록이 필요합니다.")
        result = {}
        for record in records:
            if not isinstance(record, dict):
                raise ValueError(key + " 항목이 객체가 아닙니다.")
            ident = identity(record.get("id")) if key == "participants" else safe_id(record.get("id"))
            if ident in result:
                raise ValueError("중복 " + key + " ID: " + ident)
            result[ident] = record
        return result

    def applicable(self, notice, project):
        return notice["active"] and ("*" in notice["targets"] or project in notice["targets"])

    def project(self, project):
        safe_id(project)
        if project not in self.projects:
            raise ValueError("등록되지 않은 프로젝트입니다: " + project)

    def read_path(self, notice, project, actor):
        return inside(self.root, f"reads/{notice['id']}/r{notice['revision']}/{project}/{identity_hash(actor)}.json")

    def receipt_path(self, notice, project, actor, session):
        return inside(self.root, f"receipts/{notice['id']}/r{notice['revision']}/{project}/{identity_hash(actor, session)}.json")

    def receipt_valid(self, record):
        if not isinstance(record, dict) or record.get("schemaVersion") != 1:
            raise ValueError("잘못된 receipt schemaVersion")
        for field in ("noticeId", "projectId"):
            safe_id(record.get(field))
        identity(record.get("actorId"))
        session = record.get("sessionId")
        identity(session)
        if type(record.get("revision")) is not int or record["revision"] < 1:
            raise ValueError("receipt revision이 잘못됐습니다.")
        digest(record.get("contentSha256"))
        checked = timestamp(record.get("checkedAt"))
        status = record.get("applicationStatus")
        if status not in STATUSES or not isinstance(record.get("note"), str) or not record["note"].strip():
            raise ValueError("receipt 적용 상태와 설명이 필요합니다.")
        if not isinstance(record.get("evidence"), list) or any(not isinstance(x, str) for x in record["evidence"]):
            raise ValueError("evidence는 문자열 목록이어야 합니다.")
        if status == "applied":
            if timestamp(record.get("appliedAt")) < checked:
                raise ValueError("appliedAt가 확인 시각보다 빠릅니다.")
        elif record.get("appliedAt") is not None:
            raise ValueError("적용 외 상태에는 appliedAt를 기록하지 않습니다.")
        return record

    def receipts(self):
        records = []
        for path in sorted((self.root / "receipts").rglob("*.json")):
            inside(self.root, path.relative_to(self.root).as_posix())
            record = self.receipt_valid(json.loads(path.read_text(encoding="utf-8-sig")))
            notice = {"id": record["noticeId"], "revision": record["revision"]}
            expected = self.receipt_path(notice, record["projectId"], record["actorId"], record["sessionId"])
            if comparable_path(path.resolve()) != comparable_path(expected):
                raise ValueError("receipt 경로와 기록의 소유자가 다릅니다: " + str(path))
            if record["projectId"] not in self.projects or record["noticeId"] not in self.notices:
                raise ValueError("receipt의 프로젝트/공지 ID가 등록되지 않았습니다.")
            records.append(record)
        return records

    def current(self, record):
        notice = self.notices[record["noticeId"]]
        return (record["revision"] == notice["revision"] and record["contentSha256"] == notice["contentSha256"]
                and self.applicable(notice, record["projectId"]))

    def check(self, project, actor, read=False):
        self.project(project)
        identity(actor)
        records = [r for r in self.receipts() if self.current(r) and r["projectId"] == project and r["actorId"] == actor]
        output = []
        applicable_count = 0
        for notice in self.notices.values():
            if not self.applicable(notice, project):
                continue
            applicable_count += 1
            matching = [r for r in records if r["noticeId"] == notice["id"]]
            latest = max(matching, key=lambda r: timestamp(r["checkedAt"]), default=None)
            if latest and latest["applicationStatus"] in {"applied", "not_applicable"}:
                continue
            output.append(f"{notice['id']} r{notice['revision']} {notice['title']} · " + (latest["applicationStatus"] if latest else "미확인"))
            output.append("SHA256 " + notice["contentSha256"])
            if read:
                data = inside(self.root, notice["path"]).read_bytes()
                if content_hash(data) != notice["contentSha256"]:
                    raise ValueError("본문 조회 중 공지가 변경됐습니다. 다시 확인하세요.")
                body = data.decode("utf-8-sig").replace("\r\n", "\n").replace("\r", "\n")
                output.append(body)
                atomic_json(self.read_path(notice, project, actor), {"noticeId": notice["id"], "revision": notice["revision"],
                    "contentSha256": notice["contentSha256"], "projectId": project, "actorId": actor, "checkedAt": now()})
        if not applicable_count:
            return "이 프로젝트에 적용되는 현재 활성 공지가 없습니다."
        return "\n\n".join(output) if output else "현재 대상 공지에 대한 본인 적용/해당없음 기록이 있습니다. 다른 AI 확인을 뜻하지 않습니다."

    def ack(self, project, actor, session, notice_id, revision, sha256, status, note, evidence=None):
        self.project(project)
        identity(actor)
        notice = self.notices.get(safe_id(notice_id))
        if notice is None or not self.applicable(notice, project):
            raise ValueError("현재 해당 프로젝트 대상인 활성 공지가 아닙니다.")
        if revision != notice["revision"] or sha256 != notice["contentSha256"]:
            raise ValueError("공지 revision/hash가 변경됐습니다. 최신 본문을 다시 읽으세요.")
        fresh = Board(self.root.parents[1])
        if fresh.notices.get(notice_id) != notice:
            raise ValueError("확인 기록 작성 중 공지가 변경됐습니다. 다시 읽으세요.")
        path = self.read_path(notice, project, actor)
        if not path.is_file():
            raise ValueError("본인 check --read 기록이 없습니다. 먼저 본문을 읽으세요.")
        reading = json.loads(path.read_text(encoding="utf-8-sig"))
        for field, value in (("noticeId", notice_id), ("revision", revision), ("contentSha256", sha256), ("projectId", project), ("actorId", actor)):
            if reading.get(field) != value:
                raise ValueError("본인 읽기 기록이 현재 공지와 다릅니다. 다시 읽으세요.")
        record = {"schemaVersion": 1, "noticeId": notice_id, "revision": revision, "contentSha256": sha256,
                  "projectId": project, "actorId": actor, "sessionId": session, "checkedAt": reading["checkedAt"],
                  "applicationStatus": status, "note": note, "evidence": evidence or []}
        if status == "applied":
            record["appliedAt"] = now()
        target = self.receipt_path(notice, project, actor, session)
        if target.is_file():
            existing = self.receipt_valid(json.loads(target.read_text(encoding="utf-8-sig")))
            if any(existing[k] != record[k] for k in ("noticeId", "revision", "projectId", "actorId", "sessionId")):
                raise ValueError("기존 확인 파일은 다른 작성자/세션의 기록입니다.")
            if existing["contentSha256"] == sha256:
                record["checkedAt"] = existing["checkedAt"]
        self.receipt_valid(record)
        atomic_json(target, record)
        return target

    def write_status(self, text):
        target = inside(self.root, "STATUS.md")
        fd, temp = tempfile.mkstemp(prefix=".status-", suffix=".tmp", dir=self.root)
        try:
            with os.fdopen(fd, "w", encoding="utf-8", newline="\n") as stream:
                stream.write(text)
                stream.flush()
                os.fsync(stream.fileno())
            os.replace(temp, target)
        finally:
            if os.path.exists(temp):
                os.unlink(temp)

    def status(self):
        records = [r for r in self.receipts() if self.current(r)]
        instant = datetime.now(timezone.utc)
        generated = instant.astimezone(timezone(timedelta(hours=9))).strftime("%Y-%m-%d %H:%M:%S KST") + " / " + instant.strftime("%Y-%m-%d %H:%M:%S UTC")
        lines = ["# 공지 확인 현황", "", "생성 시각: " + generated, "", "프로젝트 확인은 누군가 그 범위의 기록을 남겼다는 뜻입니다. 모든 AI의 확인 또는 적용 완료가 아닙니다.",
                 "작성자 신원은 호출자가 명시합니다. 이 CLI는 계정 인증이나 다른 채팅의 열람을 증명하지 않습니다.", "", "| 공지 | 현재 버전 | 확인 프로젝트 / 대상 | 적용 기록 | 미적용·막힘 기록 |", "|---|---|---|---|---|"]
        for n in self.notices.values():
            if not n["active"]:
                continue
            targets = [p for p in self.projects if self.projects[p]["required"] and self.applicable(n, p)]
            relevant = [r for r in records if r["noticeId"] == n["id"] and r["projectId"] in targets]
            count = len({r["projectId"] for r in relevant})
            lines.append(f"| {escaped(n['id'] + ' ' + n['title'])} | r{n['revision']} | {count} / {len(targets)} | {sum(r['applicationStatus']=='applied' for r in relevant)} | {sum(r['applicationStatus'] in {'pending','blocked'} for r in relevant)} |")
        lines.extend(["", "공지별 분모·분자는 필수 프로젝트 기준입니다. 선택 프로젝트는 아래 표에 별도로 표시합니다.", "", "| 프로젝트 | 분류·구분 | 현재 대상 공지 중 확인 기록 | 작성자 | 적용 / 해당없음 / 미적용 / 막힘 |", "|---|---|---|---|---|"])
        for p in self.projects.values():
            relevant = [r for r in records if r["projectId"] == p["id"]]
            target = sum(self.applicable(n, p["id"]) for n in self.notices.values())
            count = len({r["noticeId"] for r in relevant})
            authors = ", ".join(sorted({self.participants.get(r["actorId"], {}).get("name", r["actorId"]) for r in relevant})) or "미확인"
            totals = " / ".join(str(sum(r["applicationStatus"] == s for r in relevant)) for s in ("applied", "not_applicable", "pending", "blocked"))
            category = str(p.get("category", "")) + " · " + ("필수" if p["required"] else "선택")
            lines.append(f"| {escaped(p['name'])} | {escaped(category)} | {count} / {target} | {escaped(authors)} | {totals} |")
        lines.extend(["", "| 등록 담당 AI | 현재 확인 프로젝트·공지 쌍 | 상태 |", "|---|---|---|"])
        for actor in self.participants.values():
            count = len({(r["projectId"], r["noticeId"]) for r in records if r["actorId"] == actor["id"]})
            lines.append(f"| {escaped(actor['name'])} | {count} | {'확인 기록 있음' if count else '미확인'} |")
        lines.extend(["", "pending은 읽음/확인만 기록한 상태이며 적용 완료가 아닙니다. 옛 revision·다른 hash·대상 밖 기록은 집계에서 제외합니다.", ""])
        return "\n".join(lines)


def main(argv=None):
    parser = argparse.ArgumentParser(description="공지 본인 확인 및 적용 기록")
    parser.add_argument("--root", default=str(Path(__file__).resolve().parents[1]), help="저장소 루트")
    sub = parser.add_subparsers(dest="command", required=True)
    check = sub.add_parser("check")
    check.add_argument("--project", required=True)
    check.add_argument("--actor", required=True)
    check.add_argument("--read", action="store_true")
    ack = sub.add_parser("ack")
    for flag in ("project", "actor", "session", "notice", "sha256", "note"):
        ack.add_argument("--" + flag, required=True)
    ack.add_argument("--revision", type=int, required=True)
    ack.add_argument("--status", choices=sorted(STATUSES), required=True)
    ack.add_argument("--evidence", action="append", default=[])
    status = sub.add_parser("status")
    status.add_argument("--write", action="store_true")
    sub.add_parser("validate")
    args = parser.parse_args(argv)
    try:
        board = Board(args.root)
        if args.command == "check":
            print(board.check(args.project, args.actor, args.read))
        elif args.command == "ack":
            print(board.ack(args.project, args.actor, args.session, args.notice, args.revision, args.sha256, args.status, args.note, args.evidence))
        elif args.command == "status":
            text = board.status()
            if args.write:
                board.write_status(text)
            print(text)
        else:
            board.receipts()
            print("공지 manifest·본문 hash·확인 기록 구조 검증 통과. 실사용 적용 완료를 뜻하지 않습니다.")
        return 0
    except (ValueError, OSError, UnicodeError, KeyError, TypeError) as exc:
        print("공지 처리 실패: " + str(exc), file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
