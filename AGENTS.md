# AGENTS.md

이 문서는 **AI/Codex가 실제 개발 작업에 사용하는 개발자용 실행 문서**입니다.

반드시 `README.md`를 먼저 읽고 이 문서를 읽습니다.

`README.md`는 사용자가 직접 읽고 수정하는 정책/의도 원본이고, 이 `AGENTS.md`는 그 내용을 실제 개발에 적용할 수 있도록 기술적으로 풀어쓴 실행 규칙입니다.

---

# 1. 최우선 문서 동기화 규칙

작업 시작 전 우선순위는 다음과 같습니다.

```text
1. 현재 채팅에서 사용자가 직접 내린 최신 지시
2. 해당 프로젝트 README의 사용자 규칙
3. Workbench README의 공통 사용자 규칙
4. 해당 프로젝트 AGENTS의 기술 규칙
5. Workbench AGENTS의 공통 기술 기본값
```

README와 AGENTS가 충돌하면 **README가 우선**입니다.

사용자가 README를 직접 수정한 흔적이 있거나 내용이 AGENTS와 어긋나면:

1. 변경된 사용자 의도를 먼저 파악합니다.
2. AGENTS의 관련 기술 규칙을 새 의도에 맞게 수정합니다.
3. 그 다음 실제 개발 작업을 시작합니다.

AGENTS는 README를 단순 복사하지 않습니다. 사용자 문장을 실제 실행 가능한 규칙으로 구체화합니다.

반대로 개발 중 새로운 기술 사실을 발견하면:

- 정확한 명령, 경로, SDK, 실패 원인, 권한, 검증법 → AGENTS에 기록
- 사용자가 알아야 하거나 앞으로 정책으로 컨트롤할 가치가 있는 내용 → README에도 짧게 승격

두 문서는 역할은 다르지만 서로 어긋난 채 방치하지 않습니다.

---

# 2. 두 문서의 역할

## README.md — 사용자 버전
사용자가 직접 읽고 수정하는 원본입니다.

포함 대상:
- 프로젝트 목적
- 현재 상태
- 실제로 되는 것
- 다음 할 일
- 사용자가 원하는 작업 방식
- UX/빌드/원격/정리/QA 정책
- 주요 도구와 준비 조건
- 다시 쓸 팁
- 주요 경로/링크

## AGENTS.md — 개발자 버전
AI/Codex가 실제로 작업을 수행하기 위한 세부 문서입니다.

포함 대상:
- 정확한 경로
- build/run/test/lint/package 명령
- 언어/SDK/framework/tool version
- native/package/signing 방식
- plugin/MCP/service/CI/CD
- OAuth/API/권한/env/certificate/signing 조건
- 기기/PC/runtime 정보
- 실패 원인과 우회법
- 검증 gate
- known-good baseline
- 실제 작업 중 발견된 재사용 가능한 기술 팁

별도 `PLAN.md`, `STATUS.md`, `NOTES.md`, `HANDOFF.md`, `TODO*.md`, 날짜별 인수인계 파일, note-only 폴더를 만들지 않습니다.

---

# 3. 기본 권한

사용자가 이미 요청한 작업을 수행하기 위해 필요한 다음 작업은 재확인 없이 진행할 수 있습니다.

- 프로젝트 파일 읽기
- 코드/설정 수정
- build / lint / typecheck / test
- 비파괴 디버깅
- 로그 확인
- local packaging
- 실행/검증
- 임시 build/test setup

다음은 사용자 확인이 필요합니다.

- 사용자/프로젝트 데이터의 파괴적 삭제
- 되돌리기 어려운 migration
- production 배포 또는 production data 변경
- 비용 발생
- 계정/저장소 권한 변경
- credential 노출/전송
- 사용자가 요청하지 않은 외부 메시지 전송

---

# 4. 작업 방식

- 요청된 일을 끝까지 수행합니다. 부분 구현은 완료가 아닙니다.
- 모든 요구사항을 만족하는 범위에서 가장 작은 완성형 구현을 선호합니다.
- 최소화는 코드/구조 복잡도에만 적용합니다. 요구사항, QA, 보안, migration safety, error handling을 줄이지 않습니다.
- 기존 정상 코드, platform-native 기능, 표준 라이브러리, 이미 설치된 dependency를 새 package보다 먼저 사용합니다.
- 이미 잘되는 동작은 보호합니다. 버그 수정/호환성 작업은 broad rewrite보다 small verified patch를 우선합니다.
- placeholder, fake data, fake success, swallowed error로 실패를 숨기지 않습니다.
- 되돌릴 수 있고 위험이 작은 애매함은 합리적인 기본값으로 진행합니다.
- 답에 따라 결과가 크게 달라지거나 위험이 생길 때만 질문합니다.

---

# 5. 상태 / 완료 판정

기본 상태 모델:

```text
ASSIGNED -> EXEC -> CLAIMED -> GATED -> ACCEPTED
```

AI가 `완료`라고 말하는 것은 `CLAIMED`일 뿐입니다.

가능한 경우 다음 증거를 분리해서 확인합니다.

- code complete
- static/CI PASS
- package/build PASS
- install PASS
- real user workflow PASS
- physical/manual device PASS
- final ACCEPTED

ACCEPTED 조건:

1. 사용자의 명시 요구사항이 모두 반영됨
2. 관련 static check 통과
3. focused automated test 또는 direct workflow check 통과
4. 실제 build/package 성공
5. 테스트한 artifact가 실제 최종 artifact와 동일
6. 핵심 사용자 흐름 end-to-end 성공
7. 필요한 경우 restart/re-entry 후 persistence 확인
8. 주변 정상 기능에 명백한 회귀 없음
9. fake result / known broken path / disposable implementation 없음
10. 외부 의존성 때문에 확인 못 한 것은 명확히 blocked/unverified 표시

변경된 HEAD에 과거 PASS를 그대로 상속하지 않습니다.

---

# 6. Remote Desktop Commander 운영 규칙

Remote Desktop Commander / Desktop Remote는 사용자의 **실제 PC 상태**를 다룰 때 사용합니다.

사용 범위:
- local file 확인/수정
- terminal command
- build/package
- 프로그램 실행
- 설치
- 실제 runtime 확인
- device-side check
- local verification

GitHub는 repository 상태, Remote Desktop은 실제 PC 상태를 담당합니다.

## 연결 유지
사용자가 한번 연결을 켜고 승인했고 device가 online이면:

- **작업 하나가 끝났다는 이유로 원격 연결 서비스를 끄지 않습니다.**
- 다음 작업을 바로 이어갈 수 있게 계속 작업 가능한 상태로 둡니다.
- 사용자가 끄라고 명시하기 전에는 bridge/agent/service를 종료하지 않습니다.
- remote shutdown을 임의로 호출하지 않습니다.
- connectivity process를 임의로 kill하지 않습니다.

예외:
- 사용자가 직접 disconnect/off 요청
- security 문제
- remote service 자체를 재시작해야 연결이 복구되는 maintenance 상황

재시작이 필요하면 이유를 밝히고 가능한 경우 연결을 다시 복구합니다.

build/test용 child process는 작업이 끝나면 종료해도 됩니다. **child process와 remote connection service를 혼동하지 않습니다.**

Device가 offline이면 실제 local 작업/검증을 했다고 주장하지 않습니다. 해당 단계는 `blocked / local verification not run`으로 표시합니다.

실제 PC 조작이 필요하지 않은데 Remote Desktop을 불필요하게 반복 호출하지 않습니다.

---

# 7. 원격 PC / 저장소 정리 규칙

작업 중에는 필요하면 temp/build/log/staging/test copy/helper를 만들 수 있습니다.

작업 종료 후 특히 원격 PC에서는 필요 없는 찌꺼기를 정리합니다.

삭제 후보:
- temporary build directory
- failed/intermediate artifact
- one-off test copy
- obsolete temp ZIP/APK/EXE
- disposable cache
- diagnosis가 끝난 debug log
- temporary staging secret/config
- 다시 쓰지 않을 one-off helper script

삭제 금지:
- 실제 source code
- 다음 수정에 필요한 project config/build script
- user data
- 승인된 private location의 credential/keystore
- final deliverable
- 아직 유용한 Known-Good rollback baseline
- 무엇을 빌드/검증했는지 증명하는 최소 verification record

목표 최종상태:

**작업 가능한 프로젝트 + 최종 산출물 + 다음 수정/재빌드/검증에 필요한 최소 지원파일**

Remote Desktop 연결 자체는 찌꺼기가 아니므로 계속 켜둡니다.

---

# 8. 공통 UX 규칙

프로젝트에 맞을 때 다음을 우선 검토합니다.

- 화면 순서: `현재 상태 -> 다음 행동 -> 결과 -> 상세`
- 화면당 strong primary CTA 하나
- 같은 목적의 duplicate control 최소화
- clickable control은 실제 동작 / fallback / 불가 이유 중 하나를 제공
- 모바일에서 중요한 기능을 삭제하지 말고 layout/scroll로 해결
- background 작업은 정상일 때 조용히, user-triggered 작업은 visible progress 제공
- 이미 선택된 설정을 다시 누르면 no-op 가능
- destructive action은 explicit selection 대상으로 제한
- raw diagnostic detail은 유지하되 기본 화면을 지배하지 않게 함

---

# 9. 공통 데이터 / persistence 규칙

- installed app file과 user data 분리
- deployed ID/key/storage name은 compatibility contract로 취급
- schema version + normalization/migration 사용
- old backup/data restore 확인
- `Unknown != Zero`
- product requirement가 허용하면 local-first onboarding + optional cloud/sync
- 같은 metric/concept는 Source of Truth 하나에서 계산

---

# 10. 공통 build / package 규칙

가능하면 다음 flow를 사용합니다.

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

운영 기본값:

1. normal-use entry point 하나를 명확히
2. predictable output directory
3. artifact filename에 version/build identity 포함
4. file/app/package version 일치 검증
5. 설치/서명/전송 artifact는 필요하면 SHA-256 기록
6. CI/static PASS와 physical/manual PASS 분리
7. data/UI-only 변경은 안전한 architecture라면 native rebuild를 강제하지 않음
8. secret value commit 금지, variable name/requirement만 문서화
9. 실제 성공한 command를 기록
10. release readiness는 source가 아니라 packaged artifact로 검증

---

# 11. Windows / 한글 규칙

- text/JSON/log는 UTF-8 기본
- PowerShell output encoding 명시
- 필요하면:

```cmd
@echo off
chcp 65001 >nul
set PYTHONUTF8=1
set PYTHONIOENCODING=utf-8
```

- Korean IME shortcut 실제 테스트
- space-containing path 테스트
- Korean path 테스트
- fragile legacy toolchain에는 ASCII-only temp path fallback 사용
- 내부 toolchain 문제 때문에 user-visible Korean name/UI를 제거하지 않음

---

# 12. 로그 / 관제 규칙

Human-facing status와 raw evidence를 분리합니다.

Human status가 답해야 하는 것:
- 지금 무엇을 하는가
- waiting / blocked / failed / passed 중 무엇인가
- 사용자가 다음에 무엇을 해야 하는가

Raw log에 들어갈 수 있는 것:
- timestamp
- run/session ID
- command / stack trace
- raw event/protocol
- exact reason/error

대형 로그는 가능하면 incremental/tail parsing을 사용합니다.

---

# 13. 업데이트 / rollback 규칙

Self-update 등에서는 다음 구조를 우선 검토합니다.

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

새 버전이 실제로 시작되고 user data가 정상임을 확인하기 전에 rollback backup을 삭제하지 않습니다.

---

# 14. 프로젝트에서 가져온 검증된 패턴

## Investment-Lab
- one-click Windows launcher
- safe idle / blocked / error 구분
- changed HEAD는 재검증
- UTF-8 launcher 기본화

## HealthAPK
- local-first
- full real user loop
- restart/re-entry persistence
- debug/Metro와 standalone/offline package 분리
- unknown metadata invent 금지

## ChungYack
- content/data layer와 native shell 분리
- persistent ID/storage key 보호
- migration 전 backup/export -> migrate/reinstall -> restore/import
- active/actionable info first

## PhoneLOL
- exact baseline SHA + minimal patch
- static PASS != physical-device PASS
- temp -> build/sign/verify -> final copy
- artifact SHA/signer/device/session evidence

## Market Radar
- desktop + phone viewport QA
- clipping/overflow/unreadable text/modal/back/runtime error 확인
- regenerated data와 hand-tuned UI 분리
- canonical live state + archive

## SideMemojang
- Delete/Backspace는 explicit selected object에만 적용
- low-frequency action은 context menu 고려
- 불필요한 toolbar whitespace 제거
- visually obvious success toast 절제
- contenteditable selection save/restore
- Korean IME shortcut에서 event.key + event.code 검토
- uninstall과 user-data deletion 분리

## FinanceOne
- major visual rewrite: `current -> mock/design -> feature preservation -> implementation -> regression QA`
- Source of Truth
- repeated-entry convenience value에 TTL 고려
- Unknown != Zero
- font scaling 시 container/row/button/input도 함께 조정
- stable auth/signing flow 이유 없이 재작성 금지
- missing signing/OAuth config는 wrong default로 성공시키지 말고 fail
- private build config는 approved staging으로 주입, secret value 출력 금지, build 후 staging 제거
- schema migration-first

## ASCII Aquarium
- human progress/status와 raw log 분리
- user-initiated update와 background update feedback 차등
- updater/helper READY handshake
- replacement validate 후 overwrite
- Korean/space path updater QA
- derived variant가 working desktop baseline을 훼손하지 않게 분리

---

# 15. 확인된 프로젝트 레지스트리

GitHub가 확인된 프로젝트:
- `운동앱` -> `kimjae134679/HealthAPK`
- `주식자동매매` -> `kimjae134679/Investment-Lab`
- `청약` -> hub `kimjae134679/ChungYack`; live shell `kimjae134679/stock/chungyack-apk/`
- `멀티의신` -> `kimjae134679/PhoneLOL`
- `주식 앱 / Market Radar` -> `kimjae134679/stock`

GitHub가 없어도 되는 프로젝트:
- `피규어만들기_01`
- `동물의숲 / Tiny Village`

비슷해 보인다는 이유로 repo를 추측해 연결하지 않습니다.

사용자가 명시하지 않았는데 registry를 채우기 위해 새 repo를 만들지 않습니다.

경험 소스로 사용하는 추가 프로젝트:
- `사이드메모장`
- `FinanceOne 리뉴얼`
- `사이버 아쿠아리움 / ASCII Aquarium`

---

# 16. 링크/도구 관리 규칙

친화적인 링크 목록은 Workbench `README.md`의 `🧰 꿀팁 링크함`에 둡니다.

- saved link는 candidate일 뿐 installed/trusted/adopted 증거가 아님
- canonical repo/site URL 기준 dedupe
- repost/screenshot보다 original project docs 우선
- 실제 사용 전 current version, install, compatibility, security, license, login, cost 재확인
- 실제 프로젝트에 채택되면 exact version/command/permission/config/path를 해당 프로젝트 AGENTS에 승격
- 별도 links/bookmarks/tips 파일 생성 금지

---

# 17. 각 프로젝트 인수인계 규칙

각 프로젝트도 가능하면 root에 두 파일만 유지합니다.

```text
README.md  = 사용자 버전 / 사용자가 직접 읽고 수정하는 정책·상태
AGENTS.md  = AI 개발자 버전 / README를 실행 가능한 기술 규칙으로 번역
```

다른 채팅에서 인수인계를 받을 때는 두 파일 전체를 각각 하나의 Markdown code block으로 받는 방식을 우선합니다.

프로젝트 README를 사용자가 수정하면 다음 AI는 먼저 그 변경을 파악하고 project AGENTS를 동기화한 뒤 작업해야 합니다.

---

# 18. 팁 승격 규칙

의미 있는 작업 후:

1. 다른 프로젝트에도 재사용 가능한 lesson 1~3개를 찾습니다.
2. project-specific operational detail은 project AGENTS에 기록합니다.
3. 사용자가 알아야 할 내용은 project README에 짧게 기록합니다.
4. 범용성이 입증되면 Workbench README/AGENTS에 승격합니다.
5. 별도 tips 파일은 만들지 않습니다.
6. 사용자 선호와 일반적인 engineering practice를 구분해서 표현합니다.

---

# 19. 최종 원칙

사용자가 모든 정책을 컨트롤할 수 있어야 합니다.

따라서:

- 사용자 의도는 README에서 사람이 읽을 수 있게 보존
- AI는 README를 먼저 읽음
- AI는 AGENTS를 실제 개발에 맞게 유지
- README 변경을 AGENTS에 기술적으로 반영
- AGENTS에서 새로 발견된 사용자 영향 정보를 README에 다시 올림
- 두 문서가 충돌하면 사용자 버전인 README가 우선

**사용자는 README만 수정해도 전체 개발 규칙을 컨트롤할 수 있고, AI는 그 변경을 개발자용 AGENTS에 정확히 번역해서 적용해야 합니다.**