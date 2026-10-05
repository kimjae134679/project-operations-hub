#!/usr/bin/env python3
"""프로젝트 폴더의 본인 공지 확인. 네트워크·Git·AI 호출은 하지 않습니다."""
import argparse
import hashlib
import json
import os
from pathlib import Path, PureWindowsPath
import re
import sys
import tempfile
from datetime import datetime, timezone

STATES = {"pending", "applied", "not_applicable", "blocked"}

def normalized(data):
    return data.decode("utf-8-sig").replace("\r\n", "\n").replace("\r", "\n")

def sha(text):
    return hashlib.sha256(text.encode("utf-8")).hexdigest()

def owner(actor, session=None):
    value = [actor, session] if session is not None else actor
    return sha(json.dumps(value, ensure_ascii=False))

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


def safe(root, relative):
    raw = root / relative
    for item in [raw, *raw.parents]:
        if item.exists() and (item.is_symlink() or getattr(item.stat(), "st_file_attributes", 0) & 0x400):
            raise ValueError("연결 파일·폴더에는 기록하지 않습니다.")
        if item == root:
            break
    resolved = raw.resolve()
    if not comparable_path(resolved).is_relative_to(comparable_path(root.resolve())):
        raise ValueError("소통 폴더 밖 경로입니다.")
    return resolved

def read_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))

def atomic(root, relative, value):
    path = safe(root, relative)
    path.parent.mkdir(parents=True, exist_ok=True)
    safe(root, relative)
    handle, temporary = tempfile.mkstemp(prefix=".writing-", suffix=".tmp", dir=path.parent)
    try:
        with os.fdopen(handle, "w", encoding="utf-8", newline="\n") as stream:
            json.dump(value, stream, ensure_ascii=False, indent=2)
            stream.write("\n")
            stream.flush()
            os.fsync(stream.fileno())
        os.replace(temporary, path)
    finally:
        if os.path.exists(temporary):
            os.unlink(temporary)
    return path

def board(root):
    received = safe(root, "받은공지")
    manifest = read_json(safe(received, "manifest.json"))
    project = manifest.get("deliveredProjectId", "")
    if manifest.get("schemaVersion") != 1 or not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9_-]*", project):
        raise ValueError("컨트롤타워가 전달한 프로젝트 공지 목록이 필요합니다.")
    notices = {}
    for item in manifest["notices"]:
        if not item.get("active", True) or not ("*" in item["targets"] or project in item["targets"]):
            continue
        nid = item["id"]
        if not re.fullmatch(r"N-[0-9]+", nid) or nid in notices or type(item["revision"]) is not int or item["revision"] < 1:
            raise ValueError("공지 ID·버전 오류입니다.")
        body = normalized(safe(received, item["path"]).read_bytes())
        if sha(body) != item["contentSha256"]:
            raise ValueError("공지가 변경 중이거나 해시가 다릅니다. 다음 전달 뒤 다시 읽으세요.")
        notices[nid] = (item, body)
    return project, notices

def current(record, project, actor, notice):
    return all(record.get(k) == v for k, v in {"noticeId": notice["id"], "revision": notice["revision"], "contentSha256": notice["contentSha256"], "projectId": project, "actorId": actor}.items())

def check(root, actor, should_read):
    project, notices = board(root)
    records = [read_json(p) for p in safe(root, "확인기록").rglob("*.json")] if safe(root, "확인기록").exists() else []
    shown = 0
    for notice, body in notices.values():
        own = [r for r in records if current(r, project, actor, notice)]
        if own and any(r.get("applicationStatus") in {"applied", "not_applicable"} for r in own):
            continue
        print(f"\n{notice['id']} · 버전 {notice['revision']} · {notice['title']}\n{body}")
        shown += 1
        if should_read:
            atomic(root, f".읽기기록/{notice['id']}/{owner(actor)}.json", {"noticeId": notice["id"], "revision": notice["revision"], "contentSha256": notice["contentSha256"], "projectId": project, "actorId": actor, "checkedAt": datetime.now(timezone.utc).isoformat()})
    if shown == 0:
        print("현재 공지에 대한 본인 적용·해당없음 기록이 있습니다. 다른 AI의 확인을 뜻하지 않습니다.")

def ack(root, args):
    project, notices = board(root)
    notice, _ = notices[args.notice]
    reading = read_json(safe(root, f".읽기기록/{args.notice}/{owner(args.actor)}.json"))
    if not current(reading, project, args.actor, notice):
        raise ValueError("최신 공지를 먼저 check --read로 읽으세요.")
    checked = datetime.fromisoformat(reading["checkedAt"])
    if checked.tzinfo is None:
        raise ValueError("확인 시각의 시간대가 필요합니다.")
    if len(args.note.strip()) < 3:
        raise ValueError("본인이 확인한 내용·적용 상태를 적으세요.")
    record = {"schemaVersion": 1, "noticeId": notice["id"], "revision": notice["revision"], "contentSha256": notice["contentSha256"], "projectId": project, "actorId": args.actor, "sessionId": args.session, "checkedAt": reading["checkedAt"], "applicationStatus": args.status, "note": args.note, "evidence": args.evidence}
    relative = f"확인기록/{notice['id']}/r{notice['revision']}/{owner(args.actor,args.session)}.json"
    path = safe(root, relative)
    if path.exists():
        old = read_json(path)
        if current(old, project, args.actor, notice) and old.get("sessionId") == args.session:
            record["checkedAt"] = old["checkedAt"]
    if args.status == "applied":
        record["appliedAt"] = datetime.now(timezone.utc).isoformat()
    # Refuse a delivery change between the read and the write.
    if board(root)[1][args.notice][0] != notice:
        raise ValueError("공지가 바뀌었습니다. 다시 읽으세요.")
    print(atomic(root, relative, record))
    print("본인 기록을 남겼습니다. 컨트롤타워가 실행 중이면 수집·중앙 동기화를 진행합니다.")

def main(argv=None):
    parser = argparse.ArgumentParser(description="프로젝트 공지 본인 확인")
    parser.add_argument("--root", default=str(Path(__file__).resolve().parent), help="_통합소통 폴더")
    commands = parser.add_subparsers(dest="command", required=True)
    read = commands.add_parser("check")
    read.add_argument("--actor", required=True)
    read.add_argument("--read", action="store_true")
    write = commands.add_parser("ack")
    for name in ("actor", "session", "notice", "note"):
        write.add_argument("--" + name, required=True)
    write.add_argument("--status", choices=sorted(STATES), required=True)
    write.add_argument("--evidence", action="append", default=[])
    args = parser.parse_args(argv)
    try:
        if not args.actor.strip() or len(args.actor) > 200:
            raise ValueError("본인 AI 식별자가 필요합니다.")
        root = Path(args.root).resolve()
        if args.command == "check":
            check(root, args.actor, args.read)
        else:
            if not args.session.strip() or len(args.session) > 200:
                raise ValueError("작업 세션 이름이 필요합니다.")
            ack(root, args)
        return 0
    except (ValueError, OSError, UnicodeError, KeyError, TypeError) as error:
        print("공지 기록 대기: " + str(error), file=sys.stderr)
        return 1

if __name__ == "__main__":
    raise SystemExit(main())
