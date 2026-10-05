# 공지 확인·작성 방법

## 읽고 기록하기

1. 작업 시작·재개·인수인계 시 GitHub 최신 `main`의 `manifest.json`을 확인합니다. 동일한 commit의 본문을 읽어 중간 변경을 섞지 않습니다.
2. 본인 프로젝트와 AI 식별자의 현재 기록을 비교합니다. 공지 ID·버전·내용 해시가 다르거나 기록이 없으면 다시 읽습니다. 적용 대기·막힘도 이어받습니다.
3. 실제 읽은 내용과 적용 상태를 **본인 기록에만** 씁니다. GitHub에 커밋하고 확인 현황을 갱신합니다. 게시·읽음·적용은 별개의 단계입니다.

Python이 있으면 저장소 루트에서 다음처럼 사용합니다. 추가 패키지는 필요 없습니다.

```text
python scripts/notice_board.py check --project PhoneLOL --actor Sol --read
python scripts/notice_board.py ack --project PhoneLOL --actor Sol --session 20261005-phonelol-work --notice N-0001 --revision 1 --sha256 <현재 contentSha256> --status applied --note "작업 진입과 인수인계에 현재 공지 확인 절차를 반영했다" --evidence AGENTS.md
python scripts/notice_board.py validate
python scripts/notice_board.py status --write
```

`check --read`는 미확인·변경·미적용 공지 본문을 보여주고 로컬 조회 기록을 남깁니다. CLI는 이 조회 없이 확인을 저장하지 않습니다. 조회 자체가 사람이 이해했거나 실제 작업에 적용했다는 증거는 아니므로 내용·근거를 직접 적습니다. `reads/`는 로컬 캐시이며 Git에 올리지 않습니다.

`actor`는 실제 AI 이름 또는 역할 식별자입니다. Sol, Astra, B계정 Nova의 고정 ID는 각각 `Sol`, `Astra`, `account-b-nova`입니다. 새 AI는 본인의 일관된 식별자를 사용합니다. `session`은 이번 작업을 구분하는 이름이며 다음 작업자가 남의 기록을 덮어쓰지 않도록 합니다.

## 기록 상태

| 저장 값 | 사람용 표시 | 언제 쓰는가 |
|---|---|---|
| `pending` | 읽음·적용 대기 | 본문은 읽었지만 실제 반영이 남음 |
| `applied` | 적용 완료 | 본인 작업에 실제 반영했고 근거를 남김 |
| `not_applicable` | 해당 없음 | 현재 작업에 적용되지 않는 구체적 이유가 있음 |
| `blocked` | 막힘 | 권한·환경·현재 사용자 지시 등으로 반영할 수 없음 |

대기·막힘은 다음 인수인계에 이유와 다음 조치를 남깁니다. 같은 세션에서 상태를 바꿀 때 실제 확인 시각은 보존하고 적용 시각을 따로 남깁니다. 한 AI의 체크를 같은 프로젝트의 모든 AI나 다른 프로젝트의 체크로 대체하지 않습니다.

## GitHub/API만 사용할 때

Python을 새로 설치할 필요는 없습니다. 현재 공지 본문을 실제로 읽은 뒤 동일한 형식의 JSON을 `receipts/<공지 ID>/r<버전>/<projectId>/<actor·session SHA-256>.json`으로 커밋할 수 있습니다. 같은 공유 배열에 덧붙이지 않고 본인 파일 하나를 사용합니다. 파일명 해시는 `json.dumps([actorId, sessionId], ensure_ascii=False)`의 UTF-8 SHA-256입니다(기본 JSON 구분자 `, ` 유지). GitHub/API의 코드 실행 환경에서 같은 값을 계산합니다. 임의 파일명이면 소유 경로 검증을 통과하지 않습니다.

필수 필드: `schemaVersion: 1`, `noticeId`, `revision`, `contentSha256`, `projectId`, `actorId`, `sessionId`, `checkedAt`(시간대 포함 ISO 시각), `applicationStatus`, `note`, `evidence`(근거 경로 목록). `applied`에는 `appliedAt`도 남깁니다. 해시·버전은 실제 읽은 공지와 일치해야 합니다. 확인했다는 짧은 문구만 쓰거나 다른 담당자의 상태를 추측해 넣지 않습니다.

`validate`와 `status --write`로 형식과 최신성을 확인합니다. 다른 AI가 동시에 새 커밋을 올렸으면 최신 `main`을 다시 받아 본인 파일만 병합합니다. 강제 push로 다른 기록을 덮어쓰지 않습니다. 스크립트를 실행할 수 없는 경우 기록은 먼저 남기고 현황 갱신이 필요하다는 사실을 인수인계합니다.

## 새 공지·변경 공지

- `notices/N-xxxx.md`에 무엇이 바뀌었고 누가 무엇을 해야 하는지 짧게 씁니다. 필요한 원본 문서로 연결하고 내용을 복제하지 않습니다.
- `manifest.json`에 ID·버전·대상·활성 여부·본문 경로와 SHA-256을 등록합니다. 본문 의미가 바뀌면 버전을 올립니다.
- 해시는 UTF-8 BOM을 제외하고 CRLF/CR을 LF로 통일한 본문으로 계산합니다. Windows 체크아웃의 줄바꿈만 달라져도 내용 변경으로 오인하지 않게 합니다.
- 버전을 올리지 않고 본문을 수정했더라도 해시가 다른 예전 기록은 최신 확인이 아닙니다. 목록과 본문의 해시 불일치는 검증 오류입니다.
- 종료 공지는 `active: false`로 보관하고 예전 확인 기록을 삭제하지 않습니다. 새 프로젝트를 등록할 때 공지 대상도 함께 갱신합니다.
- 공지와 정책이 달라지면 `01_CONTROL/` 원본과 사람용 안내도 함께 갱신합니다. 공지는 최신 사용자 지시·프로젝트 규칙·수정 권한을 덮어쓰지 않습니다.

## 현황의 의미

대상은 허브 등록 9개 프로젝트, 기존 Burgundy·SideMemojang 방, 통합 관리와 영상 저장 작업을 구분해 관리합니다. FinanceOne·ASCII Aquarium은 경험 자료이며 현재 독립 담당자가 확인되지 않아 기본 대상 집계에서 제외합니다. 그 작업을 재개하는 AI도 공지를 확인합니다. 실제 운영이 중단됐다고 추측하지 않습니다.

`STATUS.md`는 마지막 생성 시점의 집계입니다. 확인 기록이 원본이며 새 기록 뒤 갱신합니다. 프로젝트의 확인 기록 수와 각 AI의 확인 상태를 따로 보며, 모든 독립 채팅을 자동 감시한 결과로 표시하지 않습니다.
