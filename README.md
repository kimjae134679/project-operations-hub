# Astra Codex Workbench

이 문서는 **내가 직접 읽고 수정하는 최상단 사용자 컨트롤센터**입니다.

평소에는 이 `README.md`만 보면 됩니다. 내가 여기서 규칙·도구·플러그인·프로젝트·팁·우선순위를 바꾸면, AI/Codex는 다음 작업을 시작할 때 그 변경을 읽고 개발자용 `AGENTS.md`를 실제 실행 규칙에 맞게 동기화해야 합니다.

```text
Astra Codex Workbench
│
├─ README.md
│  └─ ★ 사용자 컨트롤 원본
│     ├─ 프로젝트 현황
│     ├─ 작업 방식 / UX / 빌드 / QA 규칙
│     ├─ 실제 사용 중인 도구·플러그인
│     ├─ 프로젝트별 외부 서비스
│     ├─ 꿀팁 / 후보 링크
│     └─ 내가 바꾸고 싶은 정책
│
└─ AGENTS.md
   └─ ★ AI/Codex 실행 문서
      ├─ README를 기술적으로 해석
      ├─ 정확한 명령 / 경로 / SDK / 권한
      ├─ 실패 조건 / 검증 gate
      ├─ 프로젝트별 실제 작업법
      └─ 다음 AI가 바로 이어가기 위한 세부사항
```

---

# 1. 내가 컨트롤하는 방식

## README가 원본

**사용자 의도·정책·선호·도구 채택 여부의 원본은 이 `README.md`입니다.**

내가 여기 내용을 고치면 AI는 다음 작업 전에:

1. README 변경을 읽고
2. 기존 AGENTS와 충돌하는지 확인하고
3. 필요한 부분을 개발용 규칙으로 구체화해서 AGENTS를 수정하고
4. 그 다음 실제 작업을 시작합니다.

AGENTS가 더 기술적으로 자세해도 **사용자 정책과 충돌하면 README가 우선**입니다.

## AGENTS는 README의 개발용 번역본

AGENTS는 README를 그대로 복사하는 문서가 아닙니다.

예를 들어 README에:

> 원격 데스크톱은 한번 켜면 임의로 끄지 말고 계속 작업 가능한 상태로 둔다.

라고 적혀 있으면 AGENTS에서는:

- Remote Desktop bridge/agent/service 종료 금지
- remote shutdown 금지
- build/test child process는 필요 시 종료 가능
- device offline이면 local verification 미실행 표시
- 연결 재시작이 정말 필요하면 복구 후 작업 지속

처럼 실제 개발 규칙으로 풀어 씁니다.

## AI가 새로 알아낸 정보는 다시 README로 올리기

개발 중 AI가 새로운 빌드 방식, 실패 원인, 필요한 로그인, 위험한 제약, 유용한 팁을 발견하면:

- 정확한 기술 세부사항 → `AGENTS.md`
- 내가 알아야 하거나 앞으로 정책으로 컨트롤할 가치가 있는 내용 → 이 `README.md`

에 반영합니다.

## 충돌 우선순위

```text
1. 현재 채팅에서 내가 직접 내린 최신 지시
2. 해당 프로젝트 README
3. 이 Workbench README
4. 해당 프로젝트 AGENTS
5. Workbench AGENTS
```

---

# 2. 내가 README에서 직접 바꾸면 되는 것

예를 들어 내가 아래처럼 적으면 됩니다.

```text
"앞으로 Windows 앱은 가능하면 Portable도 같이 만들어."
"모바일이라고 기능 빼지 마."
"원격데탑 한번 켜면 임의로 끄지 마."
"성공 결과가 눈에 보이면 성공 토스트 남발하지 마."
"이 GitHub 도구는 실제 사용으로 승격해."
"이 링크는 후보에서 빼."
"이 프로젝트는 GitHub 필요 없어."
```

AI는 문장의 의도를 보존하면서 필요한 기술 규칙을 AGENTS에 맞게 변환합니다.

---

# 3. 도구·플러그인 컨트롤

도구는 상태를 섞지 않습니다.

```text
ACTIVE     = 실제 현재 작업에 쓰는 도구
PROJECT    = 특정 프로젝트에서만 쓰는 도구/서비스
CANDIDATE  = 저장해둔 후보. 아직 설치·채택된 것으로 보지 않음
RETIRED    = 더 이상 쓰지 않기로 한 것
```

후보 링크가 있다고 해서 AI가 마음대로 설치하거나 현재 시스템의 일부라고 가정하면 안 됩니다.

## 🟢 ACTIVE — 실제 사용 중

### GitHub
**역할:** 저장소의 소스·문서·커밋·PR·Actions 등 GitHub 쪽 상태를 다룹니다.

- 프로젝트 저장소 읽기/수정
- README/AGENTS 동기화
- 커밋/변경 확인
- 필요 시 Actions/PR/issue 확인
- 실제 사용자 PC의 로컬 상태를 대신하지는 않음

즉:

```text
GitHub = 저장소 상태
Remote Desktop = 실제 PC 상태
```

### Remote Desktop Commander / Desktop Remote
**역할:** 내 실제 PC에서 해야 하는 작업을 처리합니다.

- 로컬 파일 확인/수정
- 터미널 명령
- 프로그램 설치
- 빌드/패키징
- 프로그램 실행
- 실제 로컬 테스트
- 기기 연결/실행 상태 확인

중요 운영 규칙:

- 한번 내가 연결을 켜고 승인해서 online이 되면 **작업 하나 끝났다고 임의로 끄지 않습니다.**
- 다음 작업을 바로 이어갈 수 있게 계속 작업 가능한 상태로 둡니다.
- 내가 직접 끄라고 하기 전에는 bridge/agent/service를 종료하지 않습니다.
- remote shutdown도 임의로 하지 않습니다.
- build/test용 일회성 child process는 끝나면 종료해도 됩니다.
- 연결 서비스와 child process를 혼동하지 않습니다.
- device가 offline이면 실제 PC 작업을 했다고 말하지 않고 `로컬 검증 미실행 / 막힘`으로 표시합니다.
- 저장소/파일 정보만으로 충분하면 원격도구를 불필요하게 계속 호출하지 않습니다.
- 원격 작업이 끝난 뒤 필요 없는 찌꺼기는 정리하되 **원격 연결 자체는 켜둡니다.**

### ChatGPT Files / Library
**역할:** 이전 대화에서 올렸던 파일, 인수인계 자료, 과거 버전 같은 저장된 자료를 다시 찾을 때 사용합니다.

- 현재 첨부파일 내용 확인
- 이전 대화의 저장 파일 회수
- 과거 인수인계/기획서/원본 자료 찾기
- 파일을 찾았다고 해서 GitHub 최신본과 동일하다고 가정하지 않음

파일 내용이 필요한 요청인데 현재 대화에 없으면, 사용자가 다시 올리라고 하기 전에 가능한 범위에서 Files/Library를 먼저 찾습니다.

## 🟡 PROJECT — 특정 프로젝트에서 사용/연결

### Supabase
- `chunkyack` 앱 작업에서 사용.
- OAuth 연결을 완료한 상태에서 실제 프로젝트 연결 단계를 이어가는 흐름이 있음.
- 프로젝트 ID, URL, env 이름 등 실제 운영 세부사항은 해당 프로젝트 AGENTS에서 관리.
- 비밀값 자체는 README/AGENTS/GitHub에 적지 않음.

### 프로젝트별 서비스
Google Drive, OAuth, 호스팅, DB, 서명키, Android SDK, Blender 연동 등은 **해당 프로젝트에서 실제 사용이 확인된 경우에만 PROJECT로 취급**합니다.

Workbench 전체에서 쓰는 것처럼 자동 승격하지 않습니다.

## ⚪ CANDIDATE — 꿀팁/후보

아래 `🧰 꿀팁 링크함`에 있는 것들입니다.

- 아직 설치했다고 가정하지 않음
- 로그인/비용/보안/라이선스/호환성을 실제 사용 전에 다시 확인
- 실제 채택되면 `CANDIDATE → ACTIVE` 또는 `PROJECT`로 승격
- 승격할 때 정확한 버전·설정·권한·명령은 AGENTS에 기록

---

# 4. 전체 프로젝트 구조

## GitHub가 확인된 프로젝트

- **운동앱** → `kimjae134679/HealthAPK`
- **주식자동매매** → `kimjae134679/Investment-Lab`
- **청약** → `kimjae134679/ChungYack` + `kimjae134679/stock/chungyack-apk/`
- **멀티의신** → `kimjae134679/PhoneLOL`
- **주식 앱 / Market Radar** → `kimjae134679/stock`

각 프로젝트도 가능하면 루트의 `README.md` + `AGENTS.md` 두 문서 체계를 사용합니다.

## GitHub가 없어도 되는 프로젝트

### 피규어만들기_01
- 레퍼런스 이미지와 최대한 흡사한 Blender 피규어/3D 오브젝트 제작.
- 기존 초안은 보존 대상이 아니며 필요하면 다시 시작 가능.
- 기본 순서: `blockout → 비율/실루엣 → 디테일`.
- 저장소가 반드시 필요한 프로젝트로 취급하지 않습니다.

### 동물의숲 / Tiny Village
- 동물의숲 감성의 아늑한 데스크톱 설정 UI 콘셉트.
- 초록·베이지·목재 질감·둥근 패널 중심.
- 일반 / 주민 / 상호작용 / 화면 / 소리 / 기타 설정 구조.
- 비슷한 저장소에 근거 없이 연결하지 않습니다.

**GitHub가 없다고 불완전한 프로젝트는 아닙니다.** 필요 없으면 만들지 않습니다.

## 개발 경험을 추가로 회수한 프로젝트

- 사이드메모장
- FinanceOne 리뉴얼
- 사이버 아쿠아리움 / ASCII Aquarium

이 프로젝트들은 GitHub 관리 대상이 아니어도 실제로 잘 먹힌 개발 방법은 공통 규칙으로 가져옵니다.

---

# 5. 공통 작업 방식

- 요청한 일은 가능하면 **끝까지** 처리합니다. 부분 구현을 완료로 부르지 않습니다.
- 구현은 단순하게 하되 **요구사항을 줄이지 않습니다.**
- 기존 정상 기능을 보호하고 broad rewrite보다 small verified patch를 우선합니다.
- 새 라이브러리보다 기존 코드, platform-native 기능, 표준 라이브러리, 이미 설치된 dependency를 먼저 검토합니다.
- placeholder/fake data/fake success로 실패를 숨기지 않습니다.
- 되돌릴 수 있고 위험이 작은 애매함은 합리적인 기본값으로 진행합니다.
- 결과가 크게 달라지거나 위험이 생길 때만 질문합니다.

## 기본 권한

사용자가 이미 요청한 작업에 필요한 다음 작업은 재확인 없이 진행할 수 있습니다.

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
- production 배포/production data 변경
- 비용 발생
- 계정/저장소 권한 변경
- credential 노출/전송
- 사용자가 요청하지 않은 외부 메시지 전송

---

# 6. UI / UX 기본 규칙

- 화면은 `현재 상태 → 지금 할 행동 → 결과 → 상세` 순서를 우선 검토합니다.
- 화면마다 대표 행동 버튼은 가능하면 하나를 강하게 둡니다.
- 같은 목적의 버튼을 여러 군데 중복시키지 않습니다.
- 버튼을 눌렀는데 아무 반응 없는 상태를 만들지 않습니다. 실제 동작 / fallback / 불가 이유 중 하나를 보여줍니다.
- 결과가 화면에서 즉시 보이는 단순 성공에 토스트를 남발하지 않습니다.
- 실패, 숨은 비동기 작업, 위험한 변경은 명확히 알려줍니다.
- 모바일이라고 중요한 기능·정보를 삭제하지 않고 레이아웃/스크롤로 해결합니다.
- 작은 업무 UI는 재배치/스크롤, 하나의 시각 장면은 필요하면 전체 비율 축소를 우선 검토합니다.
- 위험한 삭제/덮어쓰기는 명시적으로 선택된 대상에만 적용합니다.
- raw 진단 정보는 숨기지 않되 기본 화면을 지배하지 않게 합니다.

---

# 7. 코드 / 데이터 / 저장 규칙

## 코드 수정
- 잘되는 기능은 통째로 다시 쓰지 않습니다.
- Known-Good 기준판을 유지하고 필요한 부분만 최소 수정합니다.
- 안정화된 인증/서명/네트워크 흐름은 이유 없이 재설계하지 않습니다.
- 같은 계산/지표는 화면마다 따로 만들지 않고 Source of Truth 하나를 사용합니다.
- **Unknown ≠ Zero.** 모르는 값과 실제 0을 구분합니다.

## 데이터 / 저장
- 앱 설치파일과 사용자 데이터를 분리합니다.
- 앱 삭제/재설치가 사용자 데이터 삭제를 자동 의미하지 않게 합니다.
- 한번 배포된 DB key, record ID, localStorage key는 호환성 계약처럼 다룹니다.
- schema 변경 전 normalize/migration을 준비하고 과거 데이터 복원을 시험합니다.
- cloud/login이 핵심 기능에 꼭 필요하지 않다면 local-first를 먼저 검토합니다.

---

# 8. 빌드·패키징·배포 규칙

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

- 평소 사용하는 실행점은 가능하면 하나를 명확히 둡니다.
- 산출물 폴더는 예측 가능하게 둡니다.
- 파일명에 의미 있는 version/build identity를 넣습니다.
- 파일명 버전 / 앱 내부 버전 / package 버전은 의도 없이 어긋나지 않게 합니다.
- 설치/서명/전송하는 중요한 산출물은 필요하면 SHA-256을 기록합니다.
- CI PASS / build PASS / install PASS / physical-device PASS는 서로 다른 증거입니다.
- 데이터/UI만 바뀌었는데 native APK를 다시 만들 필요가 없는 구조라면 재빌드를 강요하지 않습니다.
- 비밀값은 저장소에 쓰지 않고 변수명/필요 조건만 기록합니다.
- 실제 성공한 build 명령을 AGENTS에 남깁니다.

예시:

```text
MyApp-v1.4.2.apk
MyApp-Setup-v1.4.2.exe
MyApp-Portable-v1.4.2.exe
MyApp-v1.4.2.zip
SHA256SUMS.txt
```

---

# 9. Windows / 한글 규칙

- 텍스트·JSON·로그는 UTF-8을 기본으로 합니다.
- PowerShell 파일 출력도 UTF-8을 명시합니다.
- 필요하면 CMD/Python에서 다음 패턴을 사용합니다.

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

---

# 10. Remote Desktop / 실제 PC 규칙

Remote Desktop Commander는 내 실제 PC에서 파일 수정, 빌드, 실행, 설치, 테스트가 필요할 때 쓰는 도구입니다.

## 연결 유지
내가 한번 연결을 켜고 승인해서 online이 되면:

- **작업 하나 끝났다고 임의로 끄지 않습니다.**
- 다음 작업을 바로 이어갈 수 있게 연결 서비스를 계속 켜둡니다.
- 내가 직접 끄라고 하기 전에는 bridge/agent/service를 종료하지 않습니다.
- remote shutdown을 임의로 하지 않습니다.
- connectivity process를 임의로 kill하지 않습니다.
- build/test용 일회성 child process는 필요 없으면 종료해도 됩니다.
- 연결 서비스와 child process를 혼동하지 않습니다.
- offline이면 실제 local 작업/검증을 했다고 주장하지 않습니다.

예외는 내가 직접 끄라고 했거나, 보안 문제, 또는 연결 복구를 위해 서비스 재시작이 필요한 경우입니다. 재시작이 필요하면 가능한 경우 다시 작업 가능한 상태로 복구합니다.

---

# 11. 작업 후 정리 규칙

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
- 실제 프로젝트 소스
- 다음 수정에 필요한 설정/빌드 스크립트
- 사용자 데이터
- 승인된 위치의 credential/keystore
- 최종 산출물
- 아직 유용한 Known-Good rollback 기준판
- 최소 검증 기록

목표는 **작업 가능한 프로젝트 + 최종 산출물 + 다음 수정/재빌드/검증에 필요한 최소 지원파일**만 남기는 것입니다.

Remote Desktop 연결 자체는 찌꺼기가 아니므로 계속 켜둡니다.

---

# 12. 로그 / 관제 규칙

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

---

# 13. 업데이트 / 복구 / 롤백 규칙

가능하면 다음 구조를 검토합니다.

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

---

# 14. 완료 판정

AI가 `완료`라고 말한 것만으로 완료로 보지 않습니다.

```text
ASSIGNED
→ EXEC
→ CLAIMED
→ GATED
→ ACCEPTED
```

가능한 경우 아래 증거를 따로 구분합니다.

- 코드 완료
- 정적/CI PASS
- package/build PASS
- install PASS
- 실제 사용자 흐름 PASS
- physical/manual device PASS
- 최종 ACCEPTED

새 코드가 들어갔으면 과거 버전의 PASS를 그대로 물려받지 않습니다.

---

# 15. 프로젝트에서 가져온 재사용 팁

## 운동앱 / HealthAPK
- local-first
- 실제 사용자 흐름 끝까지 테스트
- restart/re-entry persistence 확인
- debug/Metro와 standalone/offline package 별도 검증
- 모르는 metadata 임의 생성 금지

## 주식자동매매 / Investment-Lab
- 더블클릭 진입점 하나
- safe idle / blocked / error 구분
- changed HEAD는 재검증
- Windows UTF-8 기본화

## 청약 / ChungYack
- 자주 바뀌는 HTML/UI/data와 native APK shell 분리
- persistent ID/storage key 보호
- migration 전 `backup/export → migrate/reinstall → restore/import`
- 판단 정보는 기본 화면, 원자료는 상세

## 멀티의신 / PhoneLOL
- exact SHA baseline + 최소 patch
- static PASS와 physical-device PASS 분리
- temp에서 build/sign/verify 후 final copy
- 필요하면 artifact SHA/signer/device/session 증거 묶기

## 주식 앱 / Market Radar
- desktop + phone viewport QA
- clipping/overflow/unreadable text/modal/back/runtime error 확인
- 자동 생성 데이터와 hand-tuned UI 분리
- canonical live state + archive 구조

## 사이드메모장
- 명시적으로 선택된 객체만 Delete/Backspace
- 저빈도 기능은 context menu 활용
- 불필요한 toolbar whitespace 제거
- 눈에 보이는 단순 성공 토스트 절제
- contenteditable selection save/restore
- 한글 IME 단축키에서 `event.key` + `event.code` 검토
- 프로그램 삭제와 user data 삭제 분리

## FinanceOne 리뉴얼
- 큰 UI 변경은 `현재 → 도안 → 기능보존 확인 → 구현 → 회귀 QA`
- Source of Truth
- 반복입력 기억값은 필요하면 TTL
- Unknown ≠ Zero
- 글씨 확대 시 container/row/button/input도 함께 확대
- 안정화된 auth/signing을 이유 없이 갈아엎지 않기
- secret은 안전한 staging으로 주입하고 로그에 출력하지 않은 뒤 제거
- schema migration-first

## 사이버 아쿠아리움 / ASCII Aquarium
- 사람용 진행상태와 raw log 분리
- user-triggered 작업과 background 작업의 피드백 강도 구분
- Portable updater는 READY handshake 후 기존 앱 종료
- replacement 검증 후 overwrite
- 한글/공백 경로 QA
- 파생판은 working desktop baseline과 분리

---

# 16. 프로젝트 인수인계 규칙

각 프로젝트도 가능하면 root에 두 파일만 유지합니다.

```text
README.md  = 사용자가 읽고 수정하는 정책·상태 원본
AGENTS.md  = AI가 읽는 개발자용 실행 문서
```

별도 `PLAN.md`, `STATUS.md`, `NOTES.md`, `HANDOFF.md`, `TODO*.md`, 날짜별 인수인계 문서를 계속 늘리지 않습니다.

다른 프로젝트 채팅에 정리를 부탁할 때는 두 파일 전체를 각각 하나의 Markdown code block으로 받는 방식을 우선합니다.

프로젝트 README를 내가 수정하면 다음 AI는 변경을 읽고 project AGENTS를 동기화한 뒤 작업합니다.

---

# 17. 🧰 꿀팁 링크함 — 아직 후보

아래는 나중에 다시 쓸 수 있는 **CANDIDATE**입니다. 여기에 있다는 이유만으로 설치·채택·신뢰된 것으로 보지 않습니다.

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
- 같은 도구는 canonical URL 기준으로 한 번만 둡니다.
- SNS/블로그/스크린샷보다 원본 GitHub/공식 사이트를 우선합니다.
- 실제 사용 전 현재 버전, 설치, 로그인, 비용, 라이선스, 보안, 호환성을 다시 확인합니다.
- 실제 채택되면 `ACTIVE` 또는 `PROJECT` 섹션으로 옮깁니다.
- 채택 시 exact version/command/permission/config/path는 AGENTS에 기록합니다.
- 더 이상 필요 없으면 `RETIRED`로 표시하거나 제거합니다.

---

# 18. AI가 매번 지켜야 할 동기화 체크

작업 시작:

```text
현재 사용자 지시
→ 프로젝트 README
→ Workbench README
→ 프로젝트 AGENTS
→ Workbench AGENTS
```

README가 바뀌었으면:

```text
사용자 의도 파악
→ AGENTS 관련 규칙 갱신
→ 실제 개발
```

개발 중 새 사실을 알게 됐으면:

```text
기술 세부사항 → AGENTS
사용자가 알아야 할 정책/준비/팁 → README
```

**목표는 내가 README만 읽고 수정해도 프로젝트와 AI 작업방식을 전부 컨트롤할 수 있게 하는 것**입니다.
