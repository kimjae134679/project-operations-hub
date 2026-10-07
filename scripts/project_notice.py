#!/usr/bin/env python3
"""프로젝트 공지와 받은 명령·실제 답변 기록. 네트워크·Git·AI 호출은 하지 않습니다."""
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


TASK_STATES = {"pending", "in_progress", "completed", "blocked", "superseded"}
TASK_RESULTS = {"pass", "fail", "not_run", "partial"}
TASK_LABELS = {"pending": "대기", "in_progress": "진행 중", "completed": "완료", "blocked": "막힘", "superseded": "새 지시로 대체"}
MAX_RECORD_BYTES = 1024 * 1024

def task_text(value, name, required=True):
    if not isinstance(value, str) or (required and not value.strip()):
        raise ValueError(name + ": 비어 있지 않은 문자열이 필요합니다.")

def task_strings(value, name):
    if not isinstance(value, list) or any(not isinstance(item, str) or not item.strip() for item in value):
        raise ValueError(name + ": 문자열 배열이 필요합니다.")

def task_time(value, name):
    task_text(value, name)
    try:
        result = datetime.fromisoformat(value.replace("Z", "+00:00"))
    except ValueError:
        raise ValueError(name + ": ISO 8601 시각이 필요합니다.")
    if result.tzinfo is None:
        raise ValueError(name + ": 시간대가 필요합니다.")
    return result

def validate_task(value):
    required = {"schemaVersion", "recordType", "recordId", "revision", "title", "projectId", "actorId", "sessionId", "receivedAt", "updatedAt", "request", "response", "status", "workDone", "verification", "nextActions", "blockers", "supersedes"}
    if not isinstance(value, dict) or set(value) != required:
        raise ValueError("기록 필드가 부족하거나 알려지지 않은 필드가 있습니다.")
    if type(value["schemaVersion"]) is not int or value["schemaVersion"] != 1 or value["recordType"] != "task_exchange":
        raise ValueError("task_exchange 스키마 버전 1이 필요합니다.")
    for key in ("recordId", "projectId"):
        if not isinstance(value[key], str) or not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9_-]{0,119}", value[key]):
            raise ValueError(key + ": 영문·숫자·밑줄·하이픈 1~120자로 적으세요.")
    if type(value["revision"]) is not int or value["revision"] < 1:
        raise ValueError("revision: 1 이상의 정수가 필요합니다.")
    for key in ("actorId", "sessionId", "title"):
        task_text(value[key], key)
        if len(value[key]) > 200:
            raise ValueError(key + ": 200자를 넘을 수 없습니다.")
    received = task_time(value["receivedAt"], "receivedAt")
    updated = task_time(value["updatedAt"], "updatedAt")
    if updated < received:
        raise ValueError("updatedAt이 receivedAt보다 빠릅니다.")
    for key in ("request", "response"):
        section = value[key]
        if not isinstance(section, dict) or set(section) != {"summary", "details", "source"}:
            raise ValueError(key + ": summary/details/source가 필요합니다.")
        for field in ("summary", "source"):
            task_text(section[field], key + "." + field)
        task_text(section["details"], key + ".details", False)
    if value["status"] not in TASK_STATES:
        raise ValueError("알려지지 않은 작업 상태입니다.")
    for key in ("workDone", "nextActions", "blockers", "supersedes"):
        task_strings(value[key], key)
    for item in value["supersedes"]:
        if not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9_-]{0,119}", item):
            raise ValueError("supersedes: 안전한 recordId만 적으세요.")
    if not isinstance(value["verification"], list):
        raise ValueError("verification: 배열이 필요합니다.")
    for item in value["verification"]:
        if not isinstance(item, dict) or set(item) != {"name", "result", "evidence"}:
            raise ValueError("검증에는 name/result/evidence가 필요합니다.")
        task_text(item["name"], "verification.name")
        if item["result"] not in TASK_RESULTS:
            raise ValueError("verification.result: pass/fail/not_run/partial 중 하나가 필요합니다.")
        task_strings(item["evidence"], "verification.evidence")
        if item["result"] in {"pass", "fail", "partial"} and not item["evidence"]:
            raise ValueError("수행한 검증에는 근거가 필요합니다.")
    if value["status"] == "blocked" and not value["blockers"]:
        raise ValueError("막힌 작업에는 blockers 이유가 필요합니다.")
    if value["status"] in {"pending", "in_progress", "blocked"} and not value["nextActions"]:
        raise ValueError("진행·대기·막힘에는 다음 작업이 필요합니다.")
    if value["status"] == "completed" and (value["blockers"] or value["nextActions"]):
        raise ValueError("완료에는 남은 필수 작업·막힘을 남길 수 없습니다. 진행 중 또는 막힘으로 기록하세요.")
    encoded = (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8")
    if len(encoded) > MAX_RECORD_BYTES:
        raise ValueError("기록이 수집 한도 1MiB를 넘습니다.")
    return value

def _task_pairs(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError("기록 JSON의 중복 필드는 허용하지 않습니다.")
        result[key] = value
    return result


def _load_task_bytes(path):
    path = Path(path)
    for item in (path, *path.parents):
        if item.is_symlink() or (item.exists() and getattr(item.lstat(), "st_file_attributes", 0) & 0x400):
            raise ValueError("연결 파일·폴더의 기록은 조회하지 않습니다.")
    with path.open("rb") as stream:
        data = stream.read(MAX_RECORD_BYTES + 1)
    if len(data) > MAX_RECORD_BYTES:
        raise ValueError("기록 입력이 1MiB를 넘습니다.")
    def invalid_constant(_):
        raise ValueError("유효하지 않은 JSON 숫자입니다.")
    value = json.loads(data.decode("utf-8-sig"), object_pairs_hook=_task_pairs, parse_constant=invalid_constant)
    return validate_task(value), data


def load_task(path):
    return _load_task_bytes(path)[0]


def task_key(value):
    return (value["projectId"], value["actorId"], value["recordId"])


def _task_filename(value):
    return "task-" + sha(json.dumps(task_key(value), ensure_ascii=False)) + "-r" + str(value["revision"]) + ".json"


def _task_history(root, project, actor, record_id):
    task_text(actor, "actorId")
    if len(actor) > 200:
        raise ValueError("actorId: 200자를 넘을 수 없습니다.")
    for name, value in (("projectId", project), ("recordId", record_id)):
        if not isinstance(value, str) or not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9_-]{0,119}", value):
            raise ValueError(name + ": 정확한 본인 기록 식별자가 필요합니다.")
    actual_project, _ = board(root)
    if project != actual_project:
        raise ValueError("조회 projectId가 소통함의 실제 프로젝트 ID와 다릅니다.")
    folder = safe(root, "보낼자료")
    requested_key = (project, actor, record_id)
    requested_prefix = "task-" + sha(json.dumps(requested_key, ensure_ascii=False)) + "-r"
    previous = []
    if folder.exists():
        for path in folder.glob("task-*-r*.json"):
            path = safe(root, "보낼자료/" + path.name)
            old = load_task(path)
            # Preserve unrelated valid legacy filenames. A requested payload key OR its
            # canonical filename prefix remains in scope, so forged owners cannot hide.
            if task_key(old) != requested_key and not path.name.startswith(requested_prefix):
                continue
            if old["projectId"] != actual_project or task_key(old) != requested_key or path.name != _task_filename(old):
                raise ValueError("기록 파일명과 실제 작성자·프로젝트·버전이 일치하지 않습니다.")
            previous.append((path, old))
    previous.sort(key=lambda item: item[1]["revision"])
    for (_, older), (_, newer) in zip(previous, previous[1:]):
        if newer["revision"] != older["revision"] + 1:
            raise ValueError("기록 이력이 누락되거나 충돌했습니다.")
        if newer["receivedAt"] != older["receivedAt"] or newer["request"] != older["request"]:
            raise ValueError("기록 이력의 받은 명령·수신 시각이 변경됐습니다.")
        if task_time(newer["updatedAt"], "updatedAt") < task_time(older["updatedAt"], "updatedAt"):
            raise ValueError("기록 이력의 갱신 시각이 역전됐습니다.")
    return previous


def _task_result(path, expected, status):
    stored, data = _load_task_bytes(path)
    if stored != expected:
        raise ValueError("기록이 조회 중 변경됐습니다. 업무를 재실행하지 말고 이력을 확인하세요.")
    return {"status": status, "projectId": stored["projectId"], "actorId": stored["actorId"],
            "recordId": stored["recordId"], "revision": stored["revision"], "taskStatus": stored["status"],
            "updatedAt": stored["updatedAt"], "path": str(path),
            "contentSha256": hashlib.sha256(data).hexdigest(), "scope": "local_record"}


def latest_task(root, *, project, actor, record_id):
    previous = _task_history(root, project, actor, record_id)
    if not previous:
        return {"status": "not_found", "projectId": project, "actorId": actor,
                "recordId": record_id, "scope": "local_record"}
    path, value = previous[-1]
    result = _task_result(path, value, "found")
    if _task_history(root, project, actor, record_id) != previous:
        raise ValueError("최신 기록이 조회 중 변경됐습니다. 다시 조회하고 업무는 재실행하지 마세요.")
    return result


def record_task(root, value):
    validate_task(value)
    previous = _task_history(root, *task_key(value))
    for path, old in previous:
        if old["revision"] == value["revision"]:
            if old != value:
                raise ValueError("같은 버전의 내용이 다릅니다. 기존 기록을 덮어쓰지 않습니다.")
            return _task_result(path, value, "already_stored")
    if previous:
        latest = previous[-1][1]
        if value["revision"] != latest["revision"] + 1:
            raise ValueError("기존 최신 revision 다음 번호로 기록하세요. 원본은 덮어쓰지 않습니다.")
        if value["receivedAt"] != latest["receivedAt"] or value["request"] != latest["request"]:
            raise ValueError("받은 명령·수신 시각은 유지하세요. 새 명령은 새 recordId로 적습니다.")
        if task_time(value["updatedAt"], "updatedAt") < task_time(latest["updatedAt"], "updatedAt"):
            raise ValueError("기존 최신 기록보다 updatedAt이 빠릅니다.")
    relative = "보낼자료/" + _task_filename(value)
    destination = safe(root, relative)
    destination.parent.mkdir(parents=True, exist_ok=True)
    safe(root, relative)
    handle, temporary = tempfile.mkstemp(prefix=".writing-task-", suffix=".tmp", dir=destination.parent)
    status = "stored"
    try:
        with os.fdopen(handle, "w", encoding="utf-8", newline="\n") as stream:
            json.dump(value, stream, ensure_ascii=False, indent=2)
            stream.write("\n")
            stream.flush()
            os.fsync(stream.fileno())
        safe(root, relative)
        try:
            os.link(temporary, destination)
        except FileExistsError:
            # A concurrent identical publication is already saved, not permission to retry work.
            previous = _task_history(root, *task_key(value))
            if not any(path == destination and old == value for path, old in previous):
                raise ValueError("동시 기록 내용이 충돌했습니다. 업무를 재실행하지 않습니다.")
            status = "already_stored"
    finally:
        if os.path.exists(temporary):
            os.unlink(temporary)
    return _task_result(destination, value, status)


def write_task(root, value):
    # Keep the existing Path-returning Python API; CLI uses explicit storage status.
    return Path(record_task(root, value)["path"])


def render_task(value):
    print(value["title"] + " · " + TASK_LABELS[value["status"]])
    print("프로젝트: " + value["projectId"] + " / 작성: " + value["actorId"])
    print("받은 지시: " + value["request"]["summary"])
    print("AI 답변: " + value["response"]["summary"])
    for title, key in (("한 일", "workDone"), ("남은 일", "nextActions"), ("막힌 이유", "blockers")):
        if value[key]:
            print(title + ":")
            for item in value[key]:
                print("- " + item)
    if value["verification"]:
        print("검증:")
        for item in value["verification"]:
            label = {"pass": "통과", "fail": "실패", "not_run": "미실행", "partial": "일부 확인"}[item["result"]]
            print("- " + item["name"] + " · " + label)
            for evidence in item["evidence"]:
                print("  근거: " + evidence)
    print("기록: " + value["recordId"] + " / 버전 " + str(value["revision"]) + " / " + value["updatedAt"])
    print("지시 출처: " + value["request"]["source"])
    print("답변 출처: " + value["response"]["source"])


def main(argv=None):
    parser = argparse.ArgumentParser(description="프로젝트 공지·명령·답변 기록")
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
    for command in ("record", "validate", "render"):
        task = commands.add_parser(command)
        task.add_argument("--input", required=True, help="task_exchange JSON 경로")
    lookup = commands.add_parser("latest", help="본인 기록의 최신 로컬 버전·해시 조회 (쓰기 없음)")
    for name in ("project", "actor", "record-id"):
        lookup.add_argument("--" + name, required=True)
    args = parser.parse_args(argv)
    try:
        if args.command == "latest":
            result = latest_task(Path(args.root).resolve(), project=args.project, actor=args.actor, record_id=args.record_id)
            print(json.dumps(result, ensure_ascii=False))
            return 0 if result["status"] == "found" else 3
        if args.command in {"record", "validate", "render"}:
            value = load_task(Path(args.input))
            if args.command == "record":
                print(json.dumps(record_task(Path(args.root).resolve(), value), ensure_ascii=False))
            elif args.command == "render":
                render_task(value)
            else:
                print("기록 형식 검증 통과: " + value["recordId"] + " / 버전 " + str(value["revision"]))
            return 0
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
        if args.command == "latest":
            print(json.dumps({"status": "held", "scope": "local_record",
                              "error": "기록 검증 실패 또는 조회 중 변경. 업무를 재실행하지 않고 이력을 확인하세요."}, ensure_ascii=False))
        print("공지 기록 대기: " + str(error), file=sys.stderr)
        return 1

if __name__ == "__main__":
    raise SystemExit(main())