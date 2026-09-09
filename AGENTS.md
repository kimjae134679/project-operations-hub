# AGENTS.md

이 문서는 **AI/Codex가 실제 개발 작업에 사용하는 개발자용 실행 문서**입니다.

반드시 `README.md`를 먼저 읽습니다.

README 안에서도 순서를 구분합니다.

```text
0. Workbench 기반 원칙   ← 시스템 헌법
1. 사용자 컨트롤 규칙   ← 사용자가 평소 수정
2. 도구·플러그인 상태
3. 프로젝트 현황
4. 팁 / 후보 링크
```

이 AGENTS는 위 내용을 실제 개발에 적용할 수 있도록 기술적으로 구체화한 실행 문서입니다.

---

# 0. Workbench 기반 계약 — AI가 임의로 바꾸면 안 되는 층

이 섹션은 README의 `0. Workbench 기반 원칙`을 실행 관점에서 해석합니다.

사용자가 바꾸면 동기화해야 하지만, AI가 작업 편의를 이유로 임의로 의미를 바꾸면 안 됩니다.

## 0-1. 두 파일 체계

가능하면 project root의 관리 문서는 다음 두 개만 유지합니다.

```text
README.md  = 사용자 정책·상태 원본
AGENTS.md  = AI/Codex 실행 문서
```

별도 `PLAN.md`, `STATUS.md`, `NOTES.md`, `HANDOFF.md`, `TODO*.md`, 날짜별 handoff, note-only folder를 계속 만들지 않습니다.

실제 source architecture에 필요한 folder/file은 유지합니다.

## 0-2. 우선순위

```text
1. 현재 채팅에서 사용자가 직접 내린 최신 지시
2. 해당 project README
3. Workbench README
4. 해당 project AGENTS
5. Workbench AGENTS
```

README와 AGENTS가 충돌하면 README가 우선입니다.

Workbench README 안에서는 `0. 기반 원칙`을 먼저 적용하고, 그 다음 `1. 사용자 컨트롤 규칙`을 적용합니다.

## 0-3. 동기화 의무

작업 시작 전:

1. README 변경 여부와 현재 내용 파악
2. 변경이 `기반 원칙`인지 `일반 사용자 규칙`인지 구분
3. 기존 AGENTS와 충돌 확인
4. 사용자 의도 보존
5. 필요한 AGENTS 기술 규칙 갱신
6. 실제 작업 시작

AGENTS는 README를 그대로 복사하지 않습니다. 실행 가능한 rule로 구체화합니다.

반대로 개발 중 새 사실을 발견하면:

- exact command/path/SDK/version/failure/permission/verification → AGENTS
- 사용자가 알아야 할 policy/preparation/tool state/reusable tip → README

두 문서를 어긋난 채 방치하지 않습니다.

## 0-4. 사실성

- repo/path/version/device state 추측 금지
- prior file == GitHub latest 가정 금지
- local/physical verification을 하지 않았으면 명확히 미검증 표시
- changed HEAD에 old PASS 상속 금지
- `CLAIMED != ACCEPTED`

## 0-5. 비밀값

README/AGENTS/GitHub에는 secret value를 기록하지 않습니다.

기록 가능:
- account/service requirement
- permission requirement
- environment variable name
- certificate/signing requirement
- secret이 보관되어야 하는 approved private location

기록 금지:
- password
- token value
- private key
- OAuth secret
- keystore password

## 0-6. 최소 구현 원칙

smallest complete implementation을 선호하되 아래는 줄이지 않습니다.

- explicit requirement
- QA
- security
- data preservation
- migration safety
- rollback
- error handling
- verification depth

---

# 1. 사용자 컨트롤 규칙을 개발용으로 해석하는 방법

README의 `1. 사용자 컨트롤 규칙`은 사용자가 평소 바꾸는 정책층입니다.

사용자가 한 문장으로 쓴 규칙도 실제 작업에서는 실행 가능한 세부조건으로 해석합니다.

예:

README:
> 원격 데스크톱은 한번 켜면 임의로 끄지 말고 계속 작업 가능한 상태로 둔다.

AGENTS 실행 규칙:
- bridge/agent/service 임의 stop 금지
- remote shutdown 금지
- connectivity process kill 금지
- build/test child process는 필요 시 종료 가능
- offline이면 local verification 미실행 표시
- restart가 필요하면 가능한 경우 connection restore

사용자 의도를 좁히거나 반대로 과도하게 확대하지 않습니다.

---

# 2. 도구 상태 모델

README의 tool state를 그대로 존중합니다.

```text
ACTIVE     = Workbench 전반에서 실제 현재 사용
PROJECT    = 특정 프로젝트에서 실제 사용
CANDIDATE  = 후보. 설치/채택된 것으로 보지 않음
RETIRED    = 더 이상 사용하지 않음
```

규칙:
- CANDIDATE를 installed/trusted/adopted로 가정 금지
- README에서 상태 변경 시 다음 작업 전 AGENTS 갱신
- 실제 채택 시 exact version/command/permission/config/path를 project AGENTS에 기록
- Workbench 전체에서 반복 적용될 때만 Workbench AGENTS에 승격

---

# 3. 현재 ACTIVE 도구

## GitHub
역할: repository state.

사용 범위:
- source/doc read/write
- commit/change 확인
- PR/issue/Actions 확인
- README/AGENTS sync

주의:
- GitHub evidence만으로 actual user PC local build/runtime 상태를 주장하지 않음

## Remote Desktop Commander / Desktop Remote
역할: user's actual PC state.

사용 범위:
- local file 확인/수정
- terminal command
- install/build/package
- program launch/runtime check
- device-side/local verification

### 연결 유지
사용자가 한번 연결을 켜고 승인했고 device가 online이면:

- task completion을 이유로 remote bridge/agent/service 종료 금지
- 다음 작업 가능한 상태 유지
- user explicit off 전 remote shutdown 금지
- connectivity process 임의 kill 금지
- build/test child process는 필요 없으면 종료 가능
- child process와 connection service를 혼동하지 않음

예외:
- user explicit off/disconnect
- security issue
- connection recovery에 필요한 service restart

restart가 필요하면 이유를 밝히고 가능한 경우 connection을 restore합니다.

Device offline이면 local 작업/검증을 했다고 주장하지 않고 `blocked / local verification not run`으로 표시합니다.

Repo/file evidence로 충분하면 Remote Desktop을 불필요하게 반복 호출하지 않습니다.

## ChatGPT Files / Library
역할: current/prior conversation file and saved-file recovery.

사용 범위:
- current attachment content
- prior upload/handoff recovery
- old version/planning/source-document retrieval

주의:
- prior file을 GitHub latest와 동일하다고 가정하지 않음
- user가 prior file을 요구했는데 current conversation에 없으면 가능한 Files/Library lookup을 먼저 수행

---

# 4. PROJECT 도구/서비스

특정 project에서 실제 사용이 확인된 tool/service만 PROJECT로 취급합니다.

예:
- Supabase
- Google Drive
- OAuth
- hosting
- DB
- Android SDK/signing
- Blender integration

현재 확인된 공통 예:

## Supabase
- `chunkyack` 관련 작업에서 사용
- OAuth 연결 완료 상태에서 actual project 연결 단계로 이어가는 flow 존재
- exact project ID/URL/env/config는 해당 project AGENTS에서 관리
- secret value는 문서/GitHub에 기록하지 않음

---

# 5. 기본 권한

재확인 없이 진행 가능:
- project file read/edit
- build/lint/typecheck/test
- non-destructive debugging/log inspection
- local packaging
- run/verify
- temporary build/test setup

확인 필요:
- destructive user/project data deletion
- irreversible migration
- production deployment/data modification
- spending money
- account/repository permission change
- credential exposure/transmission
- unsolicited external message

---

# 6. 작업 방식

- end-to-end로 처리. partial implementation은 completion이 아님.
- smallest complete implementation을 선호하되 requirement를 줄이지 않음.
- existing working code, platform-native capability, standard library, installed dependency를 새 package보다 먼저 검토.
- broad rewrite보다 small verified patch 우선.
- Known-Good baseline 보호.
- placeholder/fake data/fake success/swallowed error 금지.
- reversible low-risk ambiguity는 sensible default로 진행.
- 결과가 크게 달라지거나 risk가 생길 때만 질문.

---

# 7. 상태 / 완료 판정

```text
ASSIGNED -> EXEC -> CLAIMED -> GATED -> ACCEPTED
```

AI의 `완료` 발언은 CLAIMED일 뿐입니다.

증거 클래스:
- code complete
- static/CI PASS
- package/build PASS
- install PASS
- real user workflow PASS
- physical/manual device PASS
- final ACCEPTED

ACCEPTED 조건:
1. explicit requirement 모두 반영
2. relevant static check 통과
3. focused automated/direct workflow check 통과
4. actual build/package 성공
5. tested artifact와 final artifact 동일
6. core user flow end-to-end 성공
7. 필요 시 restart/re-entry persistence 확인
8. obvious regression 없음
9. fake/broken/disposable implementation 없음
10. 외부 의존성 미검증은 blocked/unverified로 표시

Changed HEAD에 old PASS를 상속하지 않습니다.

---

# 8. 원격 PC / 저장소 정리 규칙

작업 중 필요하면 temp/build/log/staging/test copy/helper를 만들 수 있습니다.

종료 후 삭제 후보:
- temporary build directory
- failed/intermediate artifact
- one-off test copy
- obsolete temp ZIP/APK/EXE
- disposable cache
- diagnosis 완료 debug log
- temporary staging secret/config
- future value 없는 one-off helper

삭제 금지:
- real source code
- future edit용 config/build script
- user data
- approved private credential/keystore
- final deliverable
- useful Known-Good rollback baseline
- minimal verification record

목표:
**working project + final deliverable + modify/rebuild/verify에 필요한 최소 support file**

Remote Desktop connection 자체는 cleanup 대상이 아닙니다.

---

# 9. 공통 UX 실행 규칙

- `현재 상태 -> 다음 행동 -> 결과 -> 상세`
- strong primary CTA 하나 우선
- duplicate control 최소화
- clickable control은 action/fallback/reason 제공
- mobile에서 중요한 기능 삭제 금지, layout/scroll로 해결
- visually obvious success toast 절제
- background healthy work는 quiet, user-triggered work는 visible progress
- destructive action은 explicit selection 대상
- raw diagnostics는 유지하되 default UI를 지배하지 않게 함

---

# 10. 공통 데이터 / persistence 규칙

- installed app file과 user data 분리
- deployed ID/key/storage name은 compatibility contract
- schema version + normalization/migration
- old backup/data restore 확인
- `Unknown != Zero`
- local-first + optional cloud/sync 우선 검토
- same metric/concept는 Source of Truth 하나
- stable auth/signing/network flow 이유 없이 rewrite 금지

---

# 11. build / package 규칙

```text
environment check
-> dependency check
-> static check
-> test
-> build
-> package
-> install/run verify
-> SHA-256
-> final output path
```

기본값:
1. obvious normal-use entry point
2. predictable output directory
3. version/build identity in artifact filename
4. file/app/package version consistency
5. important installed/signed/transferred artifact는 필요 시 SHA-256
6. CI/static PASS != physical/manual PASS
7. data/UI-only change는 안전하면 native rebuild 강제 금지
8. secret commit 금지
9. actual successful command 기록
10. release readiness는 packaged artifact 기준

---

# 12. Windows / 한글 규칙

- UTF-8 for text/JSON/log
- PowerShell output encoding explicit
- 필요 시:

```cmd
@echo off
chcp 65001 >nul
set PYTHONUTF8=1
set PYTHONIOENCODING=utf-8
```

- Korean IME shortcut test
- space path test
- Korean path test
- fragile legacy toolchain은 ASCII-only temp path fallback
- internal toolchain issue 때문에 user-visible Korean UI/name 제거 금지

---

# 13. 로그 / 관제 규칙

Human status:
- 현재 무엇을 하는가
- waiting/blocked/failed/passed
- next action

Raw log:
- timestamp
- run/session ID
- command/stack trace
- raw event/protocol
- exact reason/error

대형 로그는 incremental/tail 우선 검토.

---

# 14. 업데이트 / rollback 규칙

```text
discover
-> download
-> validate
-> backup
-> helper READY
-> close old app
-> replace
-> start new app
-> verify
-> cleanup backup
```

- process spawn != helper ready
- replacement validate 전 overwrite 금지
- new version start/user data 확인 전 rollback backup 삭제 금지

---

# 15. 프로젝트에서 가져온 검증 패턴

## Investment-Lab
- one-click Windows launcher
- safe idle/blocked/error 구분
- changed HEAD 재검증
- UTF-8 launcher

## HealthAPK
- local-first
- full real user loop
- restart/re-entry persistence
- debug/Metro vs standalone/offline 분리
- unknown metadata invent 금지

## ChungYack
- content/data vs native shell 분리
- persistent ID/storage key 보호
- backup/export -> migrate/reinstall -> restore/import
- active/actionable info first

## PhoneLOL
- exact baseline SHA + minimal patch
- static PASS != physical-device PASS
- temp -> build/sign/verify -> final copy
- artifact SHA/signer/device/session evidence

## Market Radar
- desktop + phone viewport QA
- clipping/overflow/unreadable text/modal/back/runtime error
- regenerated data vs hand-tuned UI 분리
- canonical live state + archive

## SideMemojang
- Delete/Backspace는 explicit selected object만
- low-frequency action은 context menu 고려
- toolbar whitespace 최소화
- visually obvious success toast 절제
- contenteditable selection save/restore
- Korean IME event.key + event.code 고려
- uninstall != user-data deletion

## FinanceOne
- current -> mock/design -> feature preservation -> implementation -> regression QA
- Source of Truth
- repeated-entry TTL 고려
- Unknown != Zero
- font scaling 시 container/row/button/input 함께 조정
- stable auth/signing 이유 없이 rewrite 금지
- private config는 approved staging, secret 출력 금지, build 후 staging 제거
- schema migration-first

## ASCII Aquarium
- human progress/status vs raw log 분리
- user-triggered vs background feedback 차등
- updater/helper READY handshake
- replacement validate 후 overwrite
- Korean/space path updater QA
- derived variant는 working baseline과 분리

---

# 16. 프로젝트 레지스트리

GitHub confirmed:
- `운동앱` -> `kimjae134679/HealthAPK`
- `주식자동매매` -> `kimjae134679/Investment-Lab`
- `청약` -> `kimjae134679/ChungYack` + `kimjae134679/stock/chungyack-apk/`
- `멀티의신` -> `kimjae134679/PhoneLOL`
- `주식 앱 / Market Radar` -> `kimjae134679/stock`

GitHub 없어도 되는 프로젝트:
- `피규어만들기_01`
- `동물의숲 / Tiny Village`

경험 소스:
- `사이드메모장`
- `FinanceOne 리뉴얼`
- `사이버 아쿠아리움 / ASCII Aquarium`

Repo는 이름이 비슷하다는 이유로 추측 연결하지 않습니다.

Registry를 채우기 위해 user request 없이 새 repo를 만들지 않습니다.

---

# 17. 후보 링크 / 도구 관리 규칙

Friendly candidate list는 Workbench README의 `🧰 꿀팁 링크함`에 둡니다.

- candidate != installed/trusted/adopted
- canonical URL 기준 dedupe
- original project docs 우선
- actual use 전 current version/install/compatibility/security/license/login/cost 재확인
- 실제 채택 시 ACTIVE 또는 PROJECT로 승격
- exact version/command/permission/config/path는 project AGENTS에 기록
- RETIRED는 active dependency로 취급하지 않음

---

# 18. 각 프로젝트 인수인계 규칙

각 project도 가능하면 두 파일만 유지합니다.

```text
README.md  = user-readable policy/state source
AGENTS.md  = developer execution rules
```

다른 chat에서 handoff를 받을 때는 두 파일 전체를 각각 하나의 Markdown code block으로 받는 방식을 우선합니다.

Project README가 바뀌면 다음 AI는 먼저 변경을 파악하고 project AGENTS를 sync한 뒤 작업합니다.

---

# 19. 팁 승격 규칙

의미 있는 작업 후:

1. 다른 project에도 재사용 가능한 lesson 1~3개 식별
2. project-specific operational detail → project AGENTS
3. user-useful summary → project README
4. 범용성이 입증되면 Workbench README/AGENTS에 승격
5. 별도 tips file 생성 금지
6. user preference와 general engineering practice 구분

---

# 20. 최종 실행 원칙

AI는 매 작업 시작 시 다음 순서로 읽습니다.

```text
현재 사용자 지시
→ project README
→ Workbench README의 0. 기반 원칙
→ Workbench README의 1. 사용자 컨트롤 규칙
→ 나머지 README 도구/프로젝트/팁
→ project AGENTS
→ Workbench AGENTS
```

사용자가 README만 수정해도 전체 개발 정책과 도구 상태를 컨트롤할 수 있어야 합니다.

AI는 그 변경을 AGENTS로 정확히 번역하고, 새로 발견한 사용자 영향 정보는 다시 README로 올립니다.
