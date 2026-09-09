# AGENTS.md

이 문서는 **AI/Codex가 실제 개발 작업에 사용하는 현재 실행본**입니다.

반드시 `README.md`를 먼저 읽습니다. README는 사용자 정책 원본이고, 이 파일은 그 내용을 실제 작업에 적용하기 위한 기술 규칙입니다.

```text
README
├─ 0. Workbench 기반 원칙
├─ 1. 사용자 컨트롤 규칙
├─ 2. 도구·플러그인 상태
├─ 3. 프로젝트 현황
├─ 4. 재사용 팁
├─ 5. 후보 링크
└─ 6. PROJECT_CHANNEL

AGENTS
└─ 위 내용을 실제 실행 가능한 규칙으로 구체화
```

---

# 0. Workbench 기반 계약

## 0-1. 사용자 정책이 최상위

우선순위:

```text
1. 현재 채팅의 사용자 최신 지시
2. 해당 프로젝트 README
3. Workbench README
4. 해당 프로젝트 AGENTS
5. Workbench AGENTS
6. PROJECT_CHANNEL의 의견/제안
```

README와 AGENTS가 충돌하면 README가 우선입니다. `PROJECT_CHANNEL`은 제안/소통용이며 정책 원본이 아닙니다.

## 0-2. 기본 문서 체계

각 프로젝트의 기본 관리 문서는 가능하면:

```text
README.md  = 사용자 정책·상태 원본
AGENTS.md  = AI 실행 규칙
```

입니다. `PLAN.md`, `STATUS.md`, `NOTES.md`, `HANDOFF.md`, 날짜별 handoff를 습관적으로 늘리지 않습니다.

사용자가 명시적으로 만든 Workbench 공용 `PROJECT_CHANNEL/`은 예외이며, 정책 원본이 아닌 공유 작업공간입니다.

## 0-3. README ↔ AGENTS 동기화

작업 시작 전:

1. README 현재 내용/변경 확인
2. 기반 원칙 변경인지 일반 사용자 규칙 변경인지 구분
3. 기존 AGENTS와 충돌 확인
4. 사용자 의도 보존
5. 필요한 기술 규칙 갱신
6. 오래된 AGENTS 규칙 정리
7. 실제 작업 시작

개발 중 새 사실 발견 시:

- exact command/path/SDK/version/failure/permission/verification → AGENTS
- 사용자에게 중요한 policy/preparation/tool state/reusable tip → README
- 다른 프로젝트에도 유용한 질문/짧은 정보 → PROJECT_CHANNEL

## 0-4. 사실성

- repo/path/version/device state 추측 금지
- prior file == GitHub latest 가정 금지
- local/physical verification 미실행이면 명확히 표시
- changed HEAD에 old PASS 상속 금지
- `CLAIMED != ACCEPTED`

## 0-5. 비밀값

README/AGENTS/GitHub/PROJECT_CHANNEL에 secret value를 기록하지 않습니다.

기록 가능:
- account/service requirement
- permission requirement
- environment variable name
- certificate/signing requirement
- approved private storage location

기록 금지:
- password/token value/private key/OAuth secret/keystore password

## 0-6. 최소 구현

smallest complete implementation을 선호하되 requirement, QA, security, data preservation, migration safety, rollback, error handling, verification은 줄이지 않습니다.

## 0-7. AGENTS는 현재 실행본

이 파일은 영구 기록 보관소가 아닙니다.

다음은 제거/정리 대상입니다.
- 해결된 임시 우회법
- 중복 규칙
- README와 충돌하는 오래된 규칙
- 현재 없는 path/version/service
- 예전 model/tool에만 필요했던 지침
- 현재 실행에 필요 없는 장황한 역사 설명

다음은 현재도 유효하면 보존합니다.
- 실제 성공한 build/run/test command
- current path/SDK/tool/version 조건
- permission/OAuth/signing requirement
- Known-Good baseline
- 아직 재현되는 failure/workaround
- verification gate

AGENTS가 비대/충돌 상태가 되면:

```text
README
+ actual repository state
+ verified current facts
→ keep list 작성
→ stale/duplicate/temp rule 제거
→ 필요하면 AGENTS 전체 재작성
→ build/test/validation으로 확인
```

과거 문장을 보존하는 것보다 **현재 정확한 실행 규칙**이 우선입니다.

---

# 1. 사용자 규칙 해석 방식

README의 짧은 사용자 문장을 실행 가능한 조건으로 구체화하되, 의도를 좁히거나 과도하게 확대하지 않습니다.

예:

README:
> 원격 데스크톱은 한번 켜면 임의로 끄지 말고 계속 작업 가능한 상태로 둔다.

실행 규칙:
- bridge/agent/service 임의 stop 금지
- remote shutdown 금지
- connectivity process kill 금지
- build/test child process는 필요 시 종료 가능
- offline이면 local verification 미실행 표시
- 연결 복구에 restart가 필요하면 가능한 경우 다시 restore

---

# 2. 도구 상태 모델

```text
ACTIVE     = Workbench 전반에서 실제 사용
PROJECT    = 특정 프로젝트에서 실제 사용
CANDIDATE  = 후보. installed/adopted로 보지 않음
RETIRED    = 더 이상 사용하지 않음
```

- README의 상태를 그대로 존중
- CANDIDATE를 설치/신뢰/채택된 것으로 가정 금지
- 실제 채택 시 exact version/command/permission/config/path를 해당 project AGENTS에 기록
- Workbench 전체 반복 사용이 확인될 때만 Workbench AGENTS로 승격

---

# 3. 현재 ACTIVE 도구

## GitHub
역할: repository state.

사용 범위:
- source/doc read/write
- commit/change 확인
- PR/issue/Actions 확인
- README/AGENTS sync

GitHub evidence만으로 actual user PC의 local build/runtime 상태를 주장하지 않습니다.

## Remote Desktop Commander / Desktop Remote
역할: user's actual PC state.

사용 범위:
- local file 확인/수정
- terminal command
- install/build/package
- program launch/runtime check
- device-side/local verification

연결 유지:
- user가 연결/승인했고 device online이면 task 완료를 이유로 bridge/agent/service 종료 금지
- 다음 작업 가능한 상태 유지
- user explicit off 전 remote shutdown 금지
- connectivity process 임의 kill 금지
- build/test child process는 필요 없으면 종료 가능
- child process와 connection service를 혼동하지 않음

예외:
- user explicit off/disconnect
- security issue
- connection recovery에 필요한 service restart

Device offline이면 `blocked / local verification not run`으로 표시합니다.
Repo/file evidence로 충분하면 Remote Desktop을 불필요하게 반복 호출하지 않습니다.

## ChatGPT Files / Library
역할: current/prior conversation files and saved-file recovery.

- current attachment content
- prior upload/handoff recovery
- old version/planning/source-document retrieval

사용자가 prior file을 요구했는데 current conversation에 없으면 가능한 Files/Library lookup을 먼저 수행합니다.

---

# 4. PROJECT 도구/서비스

실제 사용이 확인된 경우에만 PROJECT로 취급합니다.

예:
- Supabase
- Google Drive
- OAuth
- hosting/DB
- Android SDK/signing
- Blender integration

현재 확인된 예:

## Supabase
- `chunkyack` 관련 작업에서 사용
- OAuth 연결 완료 상태에서 actual project 연결 단계로 이어가는 flow 존재
- exact project ID/URL/env/config는 해당 project AGENTS에 기록
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

- end-to-end로 처리; partial implementation은 completion이 아님
- broad rewrite보다 small verified patch 우선
- Known-Good baseline 보호
- existing working code/platform-native/standard library/installed dependency 우선 검토
- placeholder/fake data/fake success/swallowed error 금지
- reversible low-risk ambiguity는 sensible default로 진행
- 결과가 크게 달라지거나 risk가 생길 때만 질문

---

# 7. 상태 / 완료 판정

```text
ASSIGNED -> EXEC -> CLAIMED -> GATED -> ACCEPTED
```

증거 클래스:
- code complete
- static/CI PASS
- package/build PASS
- install PASS
- real user workflow PASS
- physical/manual device PASS
- final ACCEPTED

ACCEPTED 기준:
1. explicit requirement 반영
2. relevant static check 통과
3. focused automated/direct workflow check 통과
4. actual build/package 성공
5. tested artifact와 final artifact 동일
6. core user flow end-to-end 성공
7. 필요 시 restart/re-entry persistence 확인
8. obvious regression 없음
9. fake/broken/disposable implementation 없음
10. 외부 의존성 미검증은 blocked/unverified로 표시

---

# 8. 원격 PC / 저장소 정리

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
- real source
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

# 9. UX 실행 규칙

- `현재 상태 -> 다음 행동 -> 결과 -> 상세`
- strong primary CTA 하나 우선
- duplicate control 최소화
- clickable control은 action/fallback/reason 제공
- mobile에서 중요한 기능 삭제 금지; layout/scroll로 해결
- visually obvious success toast 절제
- background healthy work는 quiet, user-triggered work는 visible progress
- destructive action은 explicit selection 대상
- raw diagnostics는 유지하되 default UI를 지배하지 않게 함

---

# 10. 데이터 / persistence

- installed app file과 user data 분리
- deployed ID/key/storage name은 compatibility contract
- schema version + normalization/migration
- old backup/data restore 확인
- `Unknown != Zero`
- local-first + optional cloud/sync 우선 검토
- same metric/concept는 Source of Truth 하나
- stable auth/signing/network flow 이유 없이 rewrite 금지

---

# 11. build / package

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

# 12. Windows / 한글

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

# 13. 로그 / 관제

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

대형 로그는 incremental/tail 우선 검토합니다.

---

# 14. 업데이트 / rollback

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

이름이 비슷하다는 이유로 repo를 추측 연결하거나 registry를 채우기 위해 새 repo를 만들지 않습니다.

---

# 17. 후보 링크 / 도구 관리

Friendly candidate list는 Workbench README의 `5. 🧰 꿀팁 링크함`에 둡니다.

- candidate != installed/trusted/adopted
- canonical URL 기준 dedupe
- original project docs 우선
- actual use 전 current version/install/compatibility/security/license/login/cost 재확인
- 실제 채택 시 ACTIVE 또는 PROJECT로 승격
- exact version/command/permission/config/path는 project AGENTS에 기록
- RETIRED는 active dependency로 취급하지 않음

---

# 18. PROJECT_CHANNEL 협업 규칙

공용 경로:

```text
kimjae134679/astra-codex-workbench/PROJECT_CHANNEL/
├─ README.md
└─ CHANNEL.md
```

목적: 서로 다른 프로젝트 채팅의 AI들이 **필요한 순간에만** 정보를 주고받는 공유 게시판.

중요:
- 실시간 메신저/백그라운드 프로세스가 아님
- 매 작업마다 무조건 읽지 않음
- 다른 프로젝트 정보가 도움 될 때, targeted mention이 있을 때, 공통 lesson이 생겼을 때 확인
- raw log/secret/대형 코드 덤프 금지
- 다른 프로젝트에 명령을 강제하지 않음
- 정책 변경 필요 → 해당 README
- 영구 기술 규칙 → 해당 AGENTS
- 해결된 thread는 요지만 남기거나 정리 가능

권장 형식:

```md
## YYYY-MM-DD — [FROM: 프로젝트/AI] → [TO: 프로젝트/ALL]
TYPE: ASK | REPLY | FYI | REVIEW | DONE

핵심 내용 3~10줄
```

채널 내용은 README/AGENTS보다 우선하지 않습니다.

---

# 19. 각 프로젝트 인수인계

각 project도 가능하면:

```text
README.md  = user-readable policy/state source
AGENTS.md  = developer execution rules
```

Project README가 바뀌면 다음 AI는 변경을 파악하고 project AGENTS를 sync한 뒤 작업합니다.

---

# 20. 팁 승격

의미 있는 작업 후:
1. 다른 project에도 재사용 가능한 lesson 1~3개 식별
2. project-specific operational detail → project AGENTS
3. user-useful summary → project README
4. 범용성이 입증되면 Workbench README/AGENTS에 승격
5. cross-project 질문/짧은 공유 가치가 있으면 PROJECT_CHANNEL 사용
6. user preference와 general engineering practice 구분

---

# 21. 최종 실행 순서

```text
현재 사용자 지시
→ project README
→ Workbench README 0. 기반 원칙
→ Workbench README 1. 사용자 컨트롤 규칙
→ 필요한 도구/프로젝트/팁/PROJECT_CHANNEL
→ project AGENTS
→ Workbench AGENTS
→ stale AGENTS rule 정리
→ 실제 작업
```

사용자가 README만 수정해도 전체 개발 정책과 도구 상태를 컨트롤할 수 있어야 합니다.

AGENTS는 그 의도를 실행 가능하게 번역하되 **깨끗한 현재 실행본**으로 유지합니다.
