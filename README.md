# Astra Codex Workbench

이 문서는 **내가 직접 읽고 수정하는 최상단 사용자 컨트롤센터**입니다.

평소에는 이 `README.md`만 보면 됩니다. 다만 내용은 한 덩어리가 아니라 아래처럼 **기반 원칙 → 사용자 규칙 → 도구/프로젝트/팁** 순서로 나눕니다.

```text
Astra Codex Workbench
│
├─ README.md
│  ├─ 0. Workbench 기반 원칙      ← 이 시스템의 헌법
│  ├─ 1. 사용자 컨트롤 규칙      ← 내가 평소 수정
│  ├─ 2. 도구·플러그인 상태      ← ACTIVE / PROJECT / CANDIDATE / RETIRED
│  ├─ 3. 프로젝트 현황
│  └─ 4. 팁 / 후보 링크
│
└─ AGENTS.md
   ├─ 기반 원칙의 AI용 해석
   └─ 실제 개발자 실행 규칙
```

---

# 0. Workbench 기반 원칙 — 이 시스템의 헌법

이 부분은 **Astra Codex Workbench 자체를 어떻게 운영할지 정하는 기반**입니다.

사용자인 내가 언제든 바꿀 수 있지만, AI가 작업 편의를 이유로 임의로 의미를 바꾸면 안 됩니다.

## 0-1. 목적

이 Workbench의 목적은 여러 프로젝트의 현재 상태, 작업 규칙, 도구, 검증 기준, 재사용 가능한 개발 팁을 한 체계로 관리해서 **새 채팅/새 AI에서도 바로 이어서 일하게 하는 것**입니다.

## 0-2. 관리 문서는 두 개만

프로젝트 관리 문서는 가능하면 루트의 두 파일만 유지합니다.

```text
README.md  = 사용자가 읽고 수정하는 정책·상태 원본
AGENTS.md  = AI/Codex가 읽는 개발자용 실행 문서
```

별도 `PLAN.md`, `STATUS.md`, `NOTES.md`, `HANDOFF.md`, `TODO*.md`, 날짜별 인수인계 문서, note-only 폴더를 계속 늘리지 않습니다.

실제 프로그램 구조에 필요한 `src`, `assets`, `tests`, `android`, `scripts` 같은 폴더는 당연히 유지합니다.

## 0-3. 최종 컨트롤권은 사용자에게 있음

**사용자 의도·정책·선호·도구 채택 여부의 원본은 README입니다.**

AI는 README를 읽고 AGENTS를 실제 개발에 맞게 구체화합니다.

AGENTS가 더 기술적으로 자세해도 사용자 정책과 충돌하면 README가 우선입니다.

## 0-4. 문서 우선순위

```text
1. 현재 채팅에서 내가 직접 내린 최신 지시
2. 해당 프로젝트 README
3. 이 Workbench README
4. 해당 프로젝트 AGENTS
5. Workbench AGENTS
```

충돌이 있으면 낮은 우선순위 규칙을 억지로 유지하지 말고 위 순서대로 정리합니다.

## 0-5. README ↔ AGENTS 동기화

내가 README를 고치면 AI는 다음 작업 전에:

```text
README 변경 파악
→ 기존 AGENTS와 충돌 확인
→ 사용자 의도 보존
→ 개발용 규칙으로 구체화
→ AGENTS 갱신
→ 실제 작업
```

반대로 개발 중 AI가 새 사실을 알아내면:

```text
정확한 명령 / 경로 / SDK / 버전 / 실패원인 / 권한 / 검증법
→ AGENTS

내가 알아야 할 정책 / 준비조건 / 도구상태 / 재사용 팁
→ README
```

두 문서를 서로 어긋난 채 방치하지 않습니다.

## 0-6. 확인되지 않은 내용은 추측하지 않음

- repo/path/version/device 상태를 이름만 보고 추측하지 않습니다.
- 과거 파일을 GitHub 최신본과 동일하다고 가정하지 않습니다.
- `CLAIMED != ACCEPTED`입니다. AI가 완료라고 말한 것만으로 완료가 아닙니다.
- 실제 PC/실기기 검증을 하지 못했으면 그렇게 표시합니다.

## 0-7. 비밀값은 문서에 기록하지 않음

비밀번호, API token, private key, OAuth secret, keystore password 같은 **비밀값 자체는 README/AGENTS/GitHub에 적지 않습니다.**

필요한 계정, 권한, 환경변수 이름, 인증서/서명 조건만 기록합니다.

## 0-8. 구현은 최소화해도 요구사항은 최소화하지 않음

작은 코드, 단순한 구조, 기존 기능 재사용은 선호하지만 아래를 줄이는 근거로 쓰지 않습니다.

- 사용자 요구사항
- QA
- 보안
- 데이터 보존
- migration safety
- rollback
- error handling
- 실제 검증

---

# 1. 사용자 컨트롤 규칙 — 내가 평소 바꾸는 곳

이 아래부터는 내가 원하는 개발 방식과 운영 규칙입니다. 내가 수정하면 AI는 AGENTS에 기술적으로 반영합니다.

## 1-1. 기본 작업 방식

- 요청한 일은 가능하면 **끝까지** 처리합니다. 부분 구현을 완료로 부르지 않습니다.
- 잘되는 기능은 통째로 다시 쓰지 않습니다.
- Known-Good 기준판을 보호하고 broad rewrite보다 small verified patch를 우선합니다.
- 새 라이브러리보다 기존 코드, platform-native 기능, 표준 라이브러리, 이미 설치된 dependency를 먼저 검토합니다.
- placeholder / fake data / fake success로 실패를 숨기지 않습니다.
- 되돌릴 수 있고 위험이 작은 애매함은 합리적인 기본값으로 진행합니다.
- 결과가 크게 달라지거나 위험이 생길 때만 질문합니다.

## 1-2. 기본 권한

이미 요청한 작업에 필요한 다음 작업은 재확인 없이 진행할 수 있습니다.

- 프로젝트 파일 읽기/수정
- build / lint / typecheck / test
- 비파괴 디버깅
- 로그 확인
- local packaging
- 실행/검증
- 임시 build/test setup

다음은 확인이 필요합니다.

- 사용자/프로젝트 데이터의 파괴적 삭제
- 되돌리기 어려운 migration
- production 배포 또는 production data 변경
- 비용 발생
- 계정/저장소 권한 변경
- credential 노출/전송
- 요청하지 않은 외부 메시지 전송

## 1-3. UI / UX

- 기본 화면은 `현재 상태 → 지금 할 행동 → 결과 → 상세` 순서를 우선합니다.
- 화면마다 대표 행동 버튼은 가능하면 하나를 강하게 둡니다.
- 같은 목적의 버튼을 여러 군데 중복시키지 않습니다.
- 버튼을 눌렀는데 아무 반응 없는 상태를 만들지 않습니다. 실제 동작 / fallback / 불가 이유 중 하나를 보여줍니다.
- 결과가 화면에서 즉시 보이는 단순 성공에는 성공 토스트를 남발하지 않습니다.
- 실패, 숨은 비동기 작업, 위험한 변경은 명확히 알려줍니다.
- 모바일이라고 중요한 기능·정보를 삭제하지 않고 layout/scroll로 해결합니다.
- 작은 업무 UI는 재배치/스크롤, 하나의 시각 장면은 필요하면 전체 비율 축소를 우선 검토합니다.
- 위험한 삭제/덮어쓰기는 명시적으로 선택된 대상에만 적용합니다.
- raw 진단 정보는 유지하되 기본 화면을 지배하지 않게 합니다.

## 1-4. 코드 / 데이터 / 저장

- 같은 계산/지표는 화면마다 따로 만들지 않고 Source of Truth 하나를 사용합니다.
- **Unknown ≠ Zero.** 모르는 값과 실제 0을 구분합니다.
- 앱 설치파일과 사용자 데이터를 분리합니다.
- 앱 삭제/재설치가 사용자 데이터 삭제를 자동 의미하지 않게 합니다.
- 한번 배포된 DB key, record ID, localStorage key는 호환성 계약처럼 다룹니다.
- schema 변경 전 normalize/migration을 준비하고 과거 데이터 복원을 시험합니다.
- cloud/login이 핵심 기능에 꼭 필요하지 않다면 local-first를 먼저 검토합니다.
- 안정화된 auth/signing/network 흐름은 이유 없이 갈아엎지 않습니다.

## 1-5. 빌드·패키징·배포

가능하면 다음 흐름을 기준으로 합니다.

```text
대표 build/run 명령
→ 환경/버전 확인
→ 의존성 확인
→ lint/typecheck/static check
→ 자동 테스트
→ 실제 빌드
→ 패키징
→ 설치/실행 확인
→ SHA-256
→ 최종 산출물 위치 표시
```

- 평소 사용하는 진입점은 가능하면 하나를 명확히 둡니다.
- 산출물 폴더는 예측 가능하게 둡니다.
- 파일명에 의미 있는 version/build identity를 넣습니다.
- 파일명 버전 / 앱 내부 버전 / package 버전은 의도 없이 어긋나지 않게 합니다.
- 설치/서명/전송하는 중요한 산출물은 필요하면 SHA-256을 기록합니다.
- CI PASS / build PASS / install PASS / physical-device PASS는 서로 다른 증거입니다.
- data/UI-only 변경이 native rebuild를 필요로 하지 않는 구조라면 불필요한 APK 재빌드를 강요하지 않습니다.
- 실제 성공한 build 명령을 AGENTS에 남깁니다.

예시:

```text
MyApp-v1.4.2.apk
MyApp-Setup-v1.4.2.exe
MyApp-Portable-v1.4.2.exe
MyApp-v1.4.2.zip
SHA256SUMS.txt
```

## 1-6. Windows / 한글

- text/JSON/log는 UTF-8을 기본으로 합니다.
- PowerShell 파일 출력도 UTF-8을 명시합니다.
- 필요하면 CMD/Python에서:

```cmd
@echo off
chcp 65001 >nul
set PYTHONUTF8=1
set PYTHONIOENCODING=utf-8
```

- 한글 IME 상태에서 단축키를 실제 시험합니다.
- 공백 포함 경로를 시험합니다.
- 한글 경로를 시험합니다.
- 오래된 Android/Java/Unity/CLI가 한글 경로에 약하면 **임시 빌드 경로만 ASCII**로 사용합니다.
- 내부 toolchain 문제 때문에 사용자에게 보이는 한글 이름/UI를 없애지 않습니다.

## 1-7. Remote Desktop / 실제 PC

Remote Desktop Commander는 내 실제 PC에서 파일 수정, 빌드, 실행, 설치, 테스트가 필요할 때 쓰는 도구입니다.

내가 한번 연결을 켜고 승인해서 online이 되면:

- **작업 하나 끝났다고 임의로 끄지 않습니다.**
- 다음 작업을 바로 이어갈 수 있게 연결 서비스를 계속 켜둡니다.
- 내가 직접 끄라고 하기 전에는 bridge/agent/service를 종료하지 않습니다.
- remote shutdown을 임의로 하지 않습니다.
- connectivity process를 임의로 kill하지 않습니다.
- build/test용 일회성 child process는 필요 없으면 종료해도 됩니다.
- connection service와 child process를 혼동하지 않습니다.
- offline이면 실제 local 작업/검증을 했다고 주장하지 않습니다.
- repo/file evidence만으로 충분하면 원격도구를 불필요하게 반복 호출하지 않습니다.

예외는 내가 직접 끄라고 했거나, 보안 문제, 또는 연결 복구를 위해 service restart가 필요한 경우입니다. 재시작이 필요하면 가능한 경우 다시 작업 가능한 상태로 복구합니다.

## 1-8. 작업 후 정리

특히 원격 PC에서 작업했으면 최종 사용/재빌드/검증에 필요 없는 찌꺼기를 정리합니다.

삭제 후보:
- 임시 build 폴더
- 테스트용 복사본
- 실패한 중간 산출물
- 오래된 임시 ZIP/EXE/APK
- 일회성 캐시
- 진단이 끝난 불필요 로그
- 임시 staging secret/config
- 다시 쓰지 않을 일회성 helper script

지우면 안 되는 것:
- 실제 프로젝트 source
- 다음 수정에 필요한 config/build script
- 사용자 데이터
- 승인된 위치의 credential/keystore
- 최종 산출물
- 아직 유용한 Known-Good rollback 기준판
- 최소 verification record

목표는 **작업 가능한 프로젝트 + 최종 산출물 + 다음 수정/재빌드/검증에 필요한 최소 지원파일**만 남기는 것입니다.

Remote Desktop 연결 자체는 찌꺼기가 아니므로 계속 켜둡니다.

## 1-9. 로그 / 관제

```text
사용자 화면
├─ 현재 상태
├─ 현재 단계
├─ 진행률
└─ 필요한 오류 요약

상세 로그
├─ timestamp
├─ run/session ID
├─ 실제 명령/stack
├─ raw event
└─ 상세 실패 이유
```

- 사람용 상태판과 개발자용 raw log를 분리합니다.
- 대형 로그는 가능하면 incremental/tail 방식으로 처리합니다.
- background 작업이 정상일 때는 조용히, 사용자가 직접 시작한 작업에는 진행상태를 보여줍니다.

## 1-10. 업데이트 / 복구 / rollback

가능하면:

```text
새 버전 발견
→ 다운로드
→ 크기/hash/형식 검증
→ 기존판 백업
→ updater/helper READY 확인
→ 기존 앱 종료
→ 교체
→ 새 버전 실행 확인
→ 사용자 데이터 확인
→ 성공 후 백업 정리
```

- `process spawn 성공 = updater 준비 완료`로 보지 않습니다.
- 새 버전이 실제로 시작되고 사용자 데이터가 정상임을 확인하기 전에 rollback backup을 삭제하지 않습니다.
- 중간 실패 시 Known-Good로 돌아갈 수 있어야 합니다.

## 1-11. 완료 판정

AI가 `완료`라고 말한 것만으로 완료로 보지 않습니다.

```text
ASSIGNED
→ EXEC
→ CLAIMED
→ GATED
→ ACCEPTED
```

가능한 경우 증거를 따로 구분합니다.

- 코드 완료
- 정적/CI PASS
- package/build PASS
- install PASS
- 실제 사용자 흐름 PASS
- physical/manual device PASS
- 최종 ACCEPTED

새 코드가 들어갔으면 과거 버전 PASS를 그대로 물려받지 않습니다.

---

# 2. 도구·플러그인 컨트롤

도구 상태를 섞지 않습니다.

```text
ACTIVE     = Workbench 전반에서 실제 현재 사용
PROJECT    = 특정 프로젝트에서 실제 사용
CANDIDATE  = 저장해둔 후보. 아직 설치·채택된 것으로 보지 않음
RETIRED    = 더 이상 쓰지 않기로 한 것
```

후보 링크가 있다는 이유로 AI가 마음대로 설치하거나 현재 시스템의 일부라고 가정하면 안 됩니다.

## 🟢 ACTIVE — 실제 사용 중

### GitHub
역할: 저장소의 source/doc/commit/PR/issue/Actions 등 **repository state**를 다룹니다.

- 프로젝트 저장소 읽기/수정
- README/AGENTS 동기화
- commit/change 확인
- 필요 시 Actions/PR/issue 확인

GitHub evidence만으로 실제 사용자 PC의 local build/runtime 상태를 주장하지 않습니다.

### Remote Desktop Commander / Desktop Remote
역할: 사용자의 **actual PC state**를 다룹니다.

- local file 확인/수정
- terminal command
- install/build/package
- program launch/runtime check
- device-side/local verification

연결 유지 규칙은 위 `1-7`을 따릅니다.

### ChatGPT Files / Library
역할: 현재/과거 대화 파일과 저장된 자료 회수.

- current attachment 확인
- prior upload/handoff recovery
- old version/planning/source-document retrieval

과거 파일을 GitHub latest와 동일하다고 가정하지 않습니다. 사용자가 과거 파일을 요구했는데 현재 대화에 없으면 가능한 Files/Library lookup을 먼저 시도합니다.

## 🟡 PROJECT — 특정 프로젝트에서 실제 사용

### Supabase
- `chunkyack` 관련 작업에서 사용.
- OAuth 연결 완료 상태에서 actual project 연결 단계로 이어가는 흐름이 있음.
- exact project ID/URL/env/config는 해당 project AGENTS에서 관리.
- secret value는 README/AGENTS/GitHub에 기록하지 않음.

### 기타 프로젝트별 서비스
Google Drive, OAuth, hosting, DB, Android SDK, signing, Blender integration 등은 **해당 프로젝트에서 실제 사용이 확인된 경우에만 PROJECT**로 둡니다.

## ⚪ CANDIDATE

아래 `4. 꿀팁 링크함`에 저장합니다.

- 설치/채택됐다고 가정하지 않음
- 실제 사용 전 current version/install/login/cost/license/security/compatibility 재확인
- 실제 채택되면 `ACTIVE` 또는 `PROJECT`로 승격
- 채택 시 exact version/command/permission/config/path는 AGENTS에 기록

## ⚫ RETIRED

더 이상 쓰지 않기로 한 도구는 필요하면 여기에 기록합니다. 다시 쓰기로 결정하기 전에는 active dependency로 취급하지 않습니다.

---

# 3. 프로젝트 현황

## GitHub가 확인된 프로젝트

- **운동앱** → `kimjae134679/HealthAPK`
- **주식자동매매** → `kimjae134679/Investment-Lab`
- **청약** → `kimjae134679/ChungYack` + `kimjae134679/stock/chungyack-apk/`
- **멀티의신** → `kimjae134679/PhoneLOL`
- **주식 앱 / Market Radar** → `kimjae134679/stock`

각 프로젝트도 가능하면 `README.md + AGENTS.md` 구조를 사용합니다.

## GitHub가 없어도 되는 프로젝트

### 피규어만들기_01
- reference image와 최대한 흡사한 Blender 피규어/3D object 제작.
- 기존 초안은 보존 대상이 아니며 필요하면 다시 시작 가능.
- 기본 순서: `blockout → 비율/실루엣 → 디테일`.
- repo가 반드시 필요한 프로젝트로 취급하지 않습니다.

### 동물의숲 / Tiny Village
- 동물의숲 감성의 아늑한 desktop settings UI concept.
- 초록·베이지·목재 질감·둥근 panel 중심.
- 일반 / 주민 / 상호작용 / 화면 / 소리 / 기타 구조.
- 비슷한 repo에 근거 없이 연결하지 않습니다.

**GitHub가 없다고 불완전한 프로젝트는 아닙니다.** 필요 없으면 만들지 않습니다.

## 개발 경험을 추가로 회수한 프로젝트

- 사이드메모장
- FinanceOne 리뉴얼
- 사이버 아쿠아리움 / ASCII Aquarium

GitHub 관리 대상이 아니어도 실제로 잘 먹힌 개발 방법은 공통 규칙으로 가져옵니다.

---

# 4. 프로젝트에서 가져온 재사용 팁

## 운동앱 / HealthAPK
- local-first
- real user flow end-to-end test
- restart/re-entry persistence
- debug/Metro vs standalone/offline package 별도 검증
- unknown metadata invent 금지

## 주식자동매매 / Investment-Lab
- one-click/double-click entry point
- safe idle / blocked / error 구분
- changed HEAD 재검증
- Windows UTF-8 기본화

## 청약 / ChungYack
- frequent HTML/UI/data와 native APK shell 분리
- persistent ID/storage key 보호
- migration 전 `backup/export → migrate/reinstall → restore/import`
- 판단 정보는 기본 화면, 원자료는 상세

## 멀티의신 / PhoneLOL
- exact SHA baseline + minimal patch
- static PASS != physical-device PASS
- temp에서 build/sign/verify 후 final copy
- 필요하면 artifact SHA/signer/device/session evidence 묶기

## 주식 앱 / Market Radar
- desktop + phone viewport QA
- clipping/overflow/unreadable text/modal/back/runtime error 확인
- regenerated data와 hand-tuned UI 분리
- canonical live state + archive

## 사이드메모장
- explicit selected object만 Delete/Backspace
- low-frequency action은 context menu 고려
- toolbar whitespace 최소화
- visually obvious success toast 절제
- contenteditable selection save/restore
- Korean IME `event.key + event.code` 고려
- uninstall != user-data deletion

## FinanceOne 리뉴얼
- `current → mock/design → feature preservation → implementation → regression QA`
- Source of Truth
- repeated-entry convenience value는 필요하면 TTL
- Unknown ≠ Zero
- font scaling 시 container/row/button/input 함께 조정
- stable auth/signing 이유 없이 rewrite 금지
- private config는 approved staging, secret 출력 금지, build 후 staging 제거
- schema migration-first

## 사이버 아쿠아리움 / ASCII Aquarium
- human progress/status와 raw log 분리
- user-triggered vs background feedback 차등
- updater/helper READY handshake
- replacement validate 후 overwrite
- Korean/space path updater QA
- derived variant는 working baseline과 분리

---

# 5. 🧰 꿀팁 링크함 — 아직 후보

아래는 **CANDIDATE**입니다. 여기에 있다는 이유만으로 설치·채택·신뢰된 것으로 보지 않습니다.

## AI 개발 / 에이전트
- OpenAI Plugins — https://github.com/openai/plugins
- AGENTS.md — https://agents.md/
- Ponytail — https://github.com/DietrichGebert/ponytail
- FrontierAgent — https://github.com/ApodexAI/FrontierAgent
- Gentle-AI — https://github.com/Gentleman-Programming/gentle-ai
- sandbox-runtime — https://github.com/anthropics/sandbox-runtime
- Camofox Browser — https://github.com/jo-inc/camofox-browser

## 이미지 / 디자인 / Blender
- awesome-gpt-image-2 — https://github.com/YouMind-OpenLab/awesome-gpt-image-2
- Mimikyu — https://github.com/3x-haust/Mimikyu
- Blender MCP — https://github.com/emeryporter/blender-mcp

## 영상 / 콘텐츠
- JoyAI-Video-Edit — https://github.com/jd-opensource/JoyAI-Video-Edit
- Concat — https://github.com/jub0t/Concat
- HyperFrames — https://github.com/heygen-com/hyperframes
- Ddalkkak Threads Community — https://github.com/apache3563-bit/ddalkkak-threads-community/releases/tag/v1.11.7

## 브라우저 / 생산성
- TabZipsa — https://tabzipsa.com/

## AI 인프라 / 로컬 모델
- OpenLLM — https://github.com/bentoml/OpenLLM
- BentoML — https://github.com/bentoml/BentoML
- xFormers — https://github.com/facebookresearch/xformers
- cuML — https://github.com/NVIDIA/cuml
- Heretic — https://github.com/p-e-w/heretic

## 후보 링크 관리 규칙
- canonical URL 기준 dedupe
- SNS/블로그/스크린샷보다 original GitHub/official site 우선
- 실제 사용 전 current version/install/login/cost/license/security/compatibility 재확인
- 채택되면 `ACTIVE` 또는 `PROJECT`로 승격
- 채택 시 exact version/command/permission/config/path는 AGENTS에 기록
- 더 이상 필요 없으면 `RETIRED` 또는 제거

---

# 6. AI가 매번 지켜야 할 동기화 체크

작업 시작:

```text
현재 사용자 지시
→ 프로젝트 README
→ Workbench README
→ 프로젝트 AGENTS
→ Workbench AGENTS
```

Workbench README 안에서는 먼저 **0. 기반 원칙**, 그 다음 **1. 사용자 컨트롤 규칙**, 그 다음 도구/프로젝트/팁을 읽습니다.

README가 바뀌었으면:

```text
사용자 의도 파악
→ 기반 원칙 변경인지 일반 규칙 변경인지 구분
→ AGENTS 관련 규칙 갱신
→ 실제 개발
```

개발 중 새 사실을 알게 됐으면:

```text
기술 세부사항 → AGENTS
사용자가 알아야 할 정책/준비/팁 → README
```

**목표는 내가 README만 읽고 수정해도 전체 시스템을 컨트롤할 수 있으면서, 기반 원칙과 일상 규칙이 서로 섞이지 않게 하는 것**입니다.
