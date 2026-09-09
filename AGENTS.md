# AGENTS.md

이 문서는 **AI/Codex가 실제 개발 작업에 사용하는 개발자용 실행 문서**입니다.

반드시 `README.md`를 먼저 읽고 이 문서를 읽습니다.

`README.md`는 사용자가 직접 읽고 수정하는 정책/의도/도구 채택의 원본이고, 이 `AGENTS.md`는 그 내용을 실제 개발에 적용할 수 있도록 기술적으로 풀어쓴 실행 규칙입니다.

---

# 1. 최우선 문서 동기화 규칙

작업 시작 전 우선순위:

```text
1. 현재 채팅에서 사용자가 직접 내린 최신 지시
2. 해당 프로젝트 README
3. Workbench README
4. 해당 프로젝트 AGENTS
5. Workbench AGENTS
```

README와 AGENTS가 충돌하면 README가 우선입니다.

README가 바뀌었거나 AGENTS와 어긋나면:

1. 변경된 사용자 의도를 파악
2. 관련 AGENTS 기술 규칙 갱신
3. 실제 개발 시작

AGENTS는 README를 단순 복사하지 않고 실행 가능한 기술 규칙으로 구체화합니다.

개발 중 새 사실을 발견하면:

- exact command/path/SDK/version/failure/permission/verification → AGENTS
- 사용자가 알아야 할 정책/준비/도구 상태/재사용 팁 → README

두 문서를 어긋난 채 방치하지 않습니다.

---

# 2. 문서 역할

## README.md — 사용자 컨트롤 원본

포함 대상:
- 프로젝트 목적/상태/다음 작업
- 사용자 작업 방식
- UX/build/remote/cleanup/QA 정책
- 실제 사용 도구/플러그인 상태
- 프로젝트별 외부 서비스
- 후보 링크/팁
- 사용자가 직접 바꾸고 싶은 규칙

## AGENTS.md — 개발자 실행 문서

포함 대상:
- exact path
- build/run/test/lint/package command
- language/framework/SDK/tool version
- plugin/MCP/service/CI/CD
- OAuth/API/permission/env/certificate/signing requirement
- device/PC/runtime fact
- failure mode/workaround
- verification gate
- known-good baseline
- 실제 적용 규칙

별도 `PLAN.md`, `STATUS.md`, `NOTES.md`, `HANDOFF.md`, `TODO*.md`, 날짜별 인수인계 파일, note-only 폴더를 만들지 않습니다.

---

# 3. 도구 상태 모델

Workbench README의 도구 상태를 그대로 존중합니다.

```text
ACTIVE     = 실제 현재 작업에 사용
PROJECT    = 특정 프로젝트에서만 사용
CANDIDATE  = 후보. 설치/채택된 것으로 보지 않음
RETIRED    = 더 이상 사용하지 않음
```

규칙:
- CANDIDATE를 설치/신뢰/채택됐다고 가정하지 않음
- 사용자가 README에서 상태를 바꾸면 다음 작업 전에 AGENTS를 갱신
- 실제 채택 시 exact version/command/permission/config/path를 project AGENTS에 기록
- Workbench 전체 규칙으로 승격할 필요가 있을 때만 Workbench AGENTS에도 반영

---

# 4. 현재 공통 ACTIVE 도구

## GitHub
역할: repository state.

사용 범위:
- source/doc read/write
- commit/change 확인
- PR/issue/Actions 확인
- README/AGENTS 동기화

주의:
- GitHub evidence만으로 실제 사용자 PC의 local file/build/runtime 상태를 주장하지 않음

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

- 작업 하나 끝났다는 이유로 remote bridge/agent/service를 끄지 않음
- 다음 작업을 바로 이어갈 수 있게 계속 작업 가능한 상태 유지
- 사용자가 끄라고 하기 전 remote shutdown 금지
- connectivity process 임의 kill 금지
- build/test child process는 필요 없으면 종료 가능
- child process와 connection service를 혼동하지 않음

예외:
- 사용자 explicit off/disconnect
- security 문제
- 연결 복구를 위해 service restart가 필요한 maintenance

재시작이 필요하면 이유를 밝히고 가능한 경우 연결을 다시 복구합니다.

Device offline이면 local 작업/검증을 했다고 주장하지 않고 `blocked / local verification not run`으로 표시합니다.

Repo/file evidence로 충분하면 Remote Desktop을 불필요하게 반복 호출하지 않습니다.

## ChatGPT Files / Library
역할: current/prior conversation files and saved file recovery.

사용 범위:
- current attachment content
- prior upload/handoff recovery
- old version/planning/source-document retrieval

주의:
- prior file을 GitHub latest와 동일하다고 가정하지 않음
- 사용자가 과거 파일을 요구했는데 현재 대화에 없으면 가능한 Files/Library lookup을 먼저 시도하고 곧바로 재업로드 요구로 끝내지 않음

---

# 5. PROJECT 도구/서비스

특정 프로젝트에서 실제 사용이 확인된 도구만 PROJECT로 취급합니다.

예: Supabase, Google Drive, OAuth, hosting, DB, Android SDK, signing, Blender integration.

현재 확인된 공통 예:

## Supabase
- `chunkyack` 관련 작업에서 사용
- OAuth 연결 완료 상태에서 실제 프로젝트 연결 단계로 이어가는 흐름 존재
- exact project ID/URL/env/config는 해당 project AGENTS에서 관리
- secret value는 문서/GitHub에 기록하지 않음

다른 서비스는 실제 사용 증거가 있을 때만 PROJECT로 승격합니다.

---

# 6. 기본 권한

재확인 없이 진행 가능:
- project file read/edit
- build/lint/typecheck/test
- non-destructive debug/log inspection
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

# 7. 작업 방식

- 요청한 일을 end-to-end로 처리. 부분 구현은 완료가 아님.
- smallest complete implementation을 선호하되 requirement를 줄이지 않음.
- minimalism은 code/architecture complexity에만 적용. QA/security/migration/error handling은 줄이지 않음.
- existing working code, platform-native capability, standard library, installed dependency를 새 package보다 먼저 검토.
- broad rewrite보다 small verified patch 우선.
- placeholder/fake data/fake success/swallowed error 금지.
- reversible low-risk ambiguity는 sensible default로 진행.
- 결과가 크게 달라지거나 risk가 생길 때만 질문.

---

# 8. 상태 / 완료 판정

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
10. 외부 의존성 미검증은 blocked/unverified로 명시

Changed HEAD에 old PASS를 상속하지 않습니다.

---

# 9. 원격 PC / 저장소 정리 규칙

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

Remote Desktop connection 자체는 찌꺼기가 아니므로 계속 켜둡니다.

---

# 10. 공통 UX 규칙

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

# 11. 공통 데이터 / persistence 규칙

- installed app file과 user data 분리
- deployed ID/key/storage name은 compatibility contract
- schema version + normalization/migration
- old backup/data restore 확인
- `Unknown != Zero`
- local-first + optional cloud/sync 우선 검토
- same metric/concept는 Source of Truth 하나

---

# 12. build / package 규칙

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

# 13. Windows / 한글 규칙

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

# 14. 로그 / 관제 규칙

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

# 15. 업데이트 / rollback 규칙

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

# 16. 프로젝트에서 가져온 검증 패턴

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
- missing signing/OAuth config는 wrong default로 성공시키지 말고 fail
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

# 17. 프로젝트 레지스트리

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

이름이 비슷하다는 이유로 repo를 추측해 연결하지 않고, registry를 채우기 위해 사용자가 요청하지 않은 새 repo를 만들지 않습니다.

---

# 18. 링크/후보 관리

Friendly candidate list는 Workbench README `🧰 꿀팁 링크함`에서 관리합니다.

- candidate != installed/trusted/adopted
- canonical URL로 dedupe
- original project docs 우선
- 실제 사용 전 current version/install/compatibility/security/license/login/cost 확인
- 채택 시 README 상태를 ACTIVE/PROJECT로 변경
- exact version/command/permission/config/path는 project AGENTS에 기록
- 별도 tips/bookmarks 파일 생성 금지

---

# 19. 프로젝트 인수인계

각 프로젝트도 가능하면:

```text
README.md = 사용자 정책/상태 원본
AGENTS.md = AI 개발자 실행 문서
```

두 파일만 root management docs로 유지합니다.

다른 chat에서 handoff를 받을 때는 각 파일 전체를 각각 하나의 Markdown code block으로 받는 방식을 우선합니다.

Project README가 바뀌면 next AI는 변경을 읽고 project AGENTS를 sync한 뒤 작업합니다.

---

# 20. 팁 승격

Meaningful work 후:
1. 재사용 가능한 lesson 1~3개 식별
2. project-specific operational detail -> project AGENTS
3. user-relevant summary -> project README
4. broad reuse가 확인되면 Workbench README/AGENTS 승격
5. 별도 tips file 금지
6. user preference와 general engineering practice 구분

---

# 21. 최종 원칙

사용자가 모든 정책을 README에서 읽고 수정할 수 있어야 합니다.

AI는:
- README 먼저 읽기
- README 변경을 AGENTS에 기술적으로 반영
- AGENTS에서 발견한 사용자 영향 정보를 README로 승격
- README와 AGENTS가 충돌하면 README 우선
- CANDIDATE를 멋대로 ACTIVE로 취급하지 않기
- 실제 tool adoption/connection state를 README와 동기화

**사용자는 README만 수정해도 전체 개발 규칙과 도구 정책을 컨트롤하고, AI는 그 변경을 개발자용 AGENTS로 정확히 번역해 실행합니다.**
