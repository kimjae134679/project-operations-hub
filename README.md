# Astra Codex Workbench

이 문서는 **내가 직접 읽고 수정하는 최상단 사용자 컨트롤 문서**입니다.

AI/Codex는 이 `README.md`를 먼저 읽고, 여기 적힌 의도·규칙·우선순위를 실제 개발용 `AGENTS.md`에 맞게 해석해서 동기화해야 합니다.

핵심 구조는 딱 두 층입니다.

```text
Astra Codex Workbench
│
├─ README.md
│  └─ ★ 내가 직접 읽고 수정하는 사용자 버전
│     ├─ 무엇을 만들지
│     ├─ 어떤 방식으로 일할지
│     ├─ UX/빌드/QA/원격도구 규칙
│     ├─ 프로젝트 현황
│     ├─ 내가 중요하게 보는 팁
│     └─ 앞으로 바꾸고 싶은 정책
│
└─ AGENTS.md
   └─ ★ AI/Codex가 실제 작업에 쓰는 개발자 버전
      ├─ README의 규칙을 기술적으로 해석한 실행 규칙
      ├─ 정확한 명령/경로/SDK/툴/권한
      ├─ 실패 조건/검증 gate
      ├─ 프로젝트별 실제 작업법
      └─ AI가 다음 세션에서 바로 이어가기 위한 세부사항
```

---

# 가장 중요한 컨트롤 원칙

## 1. 내가 수정하는 쪽이 원본

**사용자 의도·정책·선호·작업 방식의 원본은 `README.md`입니다.**

내가 여기 내용을 수정하면 AI는 다음 작업을 시작하기 전에 변경점을 파악하고, 필요한 내용을 `AGENTS.md`의 개발 규칙으로 다시 풀어 써야 합니다.

`AGENTS.md`가 더 기술적으로 자세하더라도 **내가 정한 정책과 충돌하면 README가 우선**입니다.

## 2. AGENTS는 README의 개발용 번역본

`AGENTS.md`는 단순 복사본이 아닙니다.

예를 들어 내가 README에:

> 원격 데스크톱은 한번 켜면 임의로 끄지 말고 계속 작업 가능한 상태로 둔다.

라고 적으면 AGENTS에는 실제 작업 규칙으로:

- Remote Desktop 연결 서비스 종료 금지
- build/test child process 종료는 가능
- device offline이면 local verification 미실행으로 표시
- remote shutdown은 사용자 지시 없이는 금지

처럼 구체화합니다.

## 3. AI가 새로 알아낸 기술 사실은 반대로 올려주기

AI가 개발 중에 새로운 빌드 명령, 실패 원인, SDK 조건, 인증 방식, 안전한 우회법 등을 알아내면:

- 세부 운영 정보 → `AGENTS.md`
- 내가 알아야 하거나 앞으로 정책을 바꿀 수 있는 내용 → 이 `README.md`에도 짧게 반영

합니다.

즉 두 문서는 **한쪽 방향 복사가 아니라 사용자 ↔ 개발자 양방향 동기화**입니다.

## 4. 충돌 시 우선순위

```text
1. 내가 현재 채팅에서 직접 내린 최신 지시
2. 해당 프로젝트 README의 사용자 규칙
3. 이 Workbench README의 공통 사용자 규칙
4. 해당 프로젝트 AGENTS의 기술 규칙
5. Workbench AGENTS의 공통 기술 기본값
```

충돌이 보이면 AI가 임의로 낮은 우선순위를 따르지 말고 위 순서로 정리하고, 필요한 AGENTS를 갱신합니다.

---

# 전체 프로젝트 구조

## GitHub가 확인된 프로젝트

- **운동앱** → `kimjae134679/HealthAPK`
- **주식자동매매** → `kimjae134679/Investment-Lab`
- **청약** → `kimjae134679/ChungYack` + `kimjae134679/stock/chungyack-apk/`
- **멀티의신** → `kimjae134679/PhoneLOL`
- **주식 앱 / Market Radar** → `kimjae134679/stock`

위 프로젝트는 각각 루트의 `README.md` + `AGENTS.md` 두 문서 체계를 사용합니다.

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

GitHub가 없는 프로젝트는 **불완전한 프로젝트가 아니라 그냥 GitHub가 필요 없는 프로젝트일 수 있습니다.**

## 개발 경험을 추가로 회수한 프로젝트

- 사이드메모장
- FinanceOne 리뉴얼
- 사이버 아쿠아리움 / ASCII Aquarium

이 프로젝트들은 GitHub 관리 대상이 아니어도, 실제로 잘 먹힌 개발 방법은 공통 규칙에 가져옵니다.

---

# 내가 컨트롤하는 공통 개발 규칙

## 사용 시작점
- 평소 사용하는 실행 파일/버튼/URL은 가능하면 **하나를 명확하게** 둡니다.
- 사용자가 `run-dev`, `run-real`, `start2` 중 뭘 골라야 하는 구조는 피합니다.
- 고급/관리 기능은 보조 메뉴나 옵션으로 둡니다.

## UI / UX
- 화면은 기본적으로 `현재 상태 → 지금 할 행동 → 결과 → 상세` 순서가 좋습니다.
- 화면마다 대표 행동 버튼은 가능하면 하나를 강하게 둡니다.
- 같은 목적의 버튼을 여러 군데 중복시키지 않습니다.
- 결과가 화면에서 즉시 보이는 단순 성공에 토스트를 남발하지 않습니다.
- 실패, 숨은 비동기 작업, 위험한 변경은 명확히 알려줍니다.
- 모바일이라고 중요한 기능·정보를 삭제하지 않고 레이아웃/스크롤로 해결합니다.
- 작은 업무 UI는 재배치/스크롤, 하나의 시각 장면은 필요하면 전체 비율 축소를 우선 검토합니다.
- 위험한 삭제/덮어쓰기 같은 행동은 명시적으로 선택된 대상에만 적용합니다.

## 코드 수정 방식
- 잘되는 기능은 통째로 다시 쓰지 않습니다.
- 검증된 정상판(Known-Good)을 기준으로 필요한 부분만 최소 수정합니다.
- 새 라이브러리보다 기존 코드, 플랫폼 기본 기능, 이미 설치된 의존성을 먼저 봅니다.
- 인증/서명/네트워크처럼 이미 안정화된 흐름은 이유 없이 재설계하지 않습니다.
- Unknown과 실제 0을 구분합니다. **Unknown ≠ Zero.**
- 같은 계산/지표는 화면마다 따로 만들지 않고 Source of Truth 하나를 사용합니다.

## 데이터 / 저장
- 앱 설치파일과 사용자 데이터를 분리합니다.
- 앱 삭제/재설치가 사용자 데이터 삭제를 자동 의미하지 않게 합니다.
- 한번 배포된 DB key, record ID, localStorage key는 호환성 계약처럼 다룹니다.
- schema 변경 전 normalize/migration을 준비하고 과거 데이터 복원을 시험합니다.
- cloud/login이 핵심 기능에 꼭 필요하지 않다면 local-first를 먼저 검토합니다.

---

# 빌드·배포 기본 규칙

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

산출물은 무슨 파일인지 이름만 보고 알 수 있게 합니다.

```text
MyApp-v1.4.2.apk
MyApp-Setup-v1.4.2.exe
MyApp-Portable-v1.4.2.exe
MyApp-v1.4.2.zip
SHA256SUMS.txt
```

- 파일명 버전 / 앱 내부 버전 / package 버전은 의도 없이 어긋나지 않게 합니다.
- CI PASS / build PASS / install PASS / physical-device PASS는 서로 다른 증거입니다.
- 데이터/UI만 바뀌었는데 native APK를 다시 만들 필요가 없는 구조라면 재빌드를 강요하지 않습니다.
- 비밀값은 저장소에 쓰지 않고 변수명/필요 조건만 기록합니다.

---

# Windows / 한글 기본 규칙

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
- 오래된 Android/Java/Unity/CLI가 한글 경로에 약하면 **임시 빌드 경로만 ASCII**로 사용합니다.
- 최종 프로그램은 가능하면 한글 경로와 공백 경로에서도 실행/업데이트를 시험합니다.

---

# Remote Desktop Commander / 실제 PC 작업 규칙

Remote Desktop Commander는 **내 실제 PC에서 파일 수정, 빌드, 실행, 설치, 테스트가 필요할 때 쓰는 도구**입니다.

GitHub는 저장소 상태를 다루고, Remote Desktop은 실제 PC 상태를 다룹니다.

내가 한번 Remote Desktop 연결을 켜고 승인해서 온라인 상태가 되면:

- **작업 하나 끝났다고 임의로 끄지 않습니다.**
- 다음 작업을 바로 이어갈 수 있게 연결 서비스를 계속 켜둡니다.
- 사용자가 직접 끄라고 하기 전에는 remote bridge/agent/service를 종료하지 않습니다.
- remote shutdown도 사용자가 요청하지 않으면 하지 않습니다.
- 다만 빌드/테스트용 일회성 child process는 필요 없으면 종료해도 됩니다.
- 연결 서비스와 테스트 프로세스를 혼동하지 않습니다.
- device가 offline이면 실제 PC 작업을 했다고 말하지 않고 `로컬 검증 미실행/막힘`으로 표시합니다.
- 실제 PC 조작이 필요하지 않은데 원격도구를 불필요하게 계속 호출하지 않습니다.

---

# 작업 후 정리 규칙

특히 원격 PC에서 작업했으면 **최종 사용/재빌드/검증에 필요 없는 찌꺼기를 정리**합니다.

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
- 승인된 위치의 keystore/credential
- 최종 산출물
- 아직 유용한 Known-Good rollback 기준판
- 무엇을 테스트했는지 증명하는 최소 검증 기록

목표는 **작업 가능한 프로젝트 + 최종 산출물 + 다음 수정에 필요한 최소 지원파일만 남기는 것**입니다.

Remote Desktop 연결 자체는 찌꺼기가 아니므로 계속 켜둡니다.

---

# 완료 판정

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

# 프로젝트끼리 서로 배운 핵심

## 운동앱 / HealthAPK
- local-first
- 실제 사용자 흐름 끝까지 테스트
- debug와 standalone/offline package 별도 검증
- 모르는 metadata를 임의로 만들지 않기

## 주식자동매매 / Investment-Lab
- 더블클릭 진입점 하나
- safe idle / blocked / error 구분
- 새 HEAD는 다시 검증
- Windows UTF-8 기본화

## 청약 / ChungYack
- 자주 바뀌는 HTML/UI/data와 native APK shell 분리
- persistent ID/storage key 보호
- migration 전 백업 → 전환 → 복원
- 원자료는 상세, 판단 정보는 기본 화면

## 멀티의신 / PhoneLOL
- exact SHA baseline + 최소 patch
- static PASS와 physical-device PASS 분리
- temp에서 build/sign/verify 후 final copy
- 필요하면 APK + ZIP + SHA-256 묶음

## 주식 앱 / Market Radar
- desktop + phone UI QA
- clipping/overflow/small text/modal/back/runtime error 확인
- 자동 생성 데이터와 hand-tuned UI 분리
- live file + archive 구조

## 사이드메모장
- 명시적 선택 객체만 Delete/Backspace
- 저빈도 기능은 context menu 활용
- 작업영역 확보
- 눈에 보이는 단순 성공 토스트 절제
- 한글 IME 단축키 실제 검증
- 앱 삭제와 사용자 데이터 삭제 분리

## FinanceOne 리뉴얼
- 큰 UI 변경은 `현재 → 도안 → 기능보존 확인 → 구현 → 회귀 QA`
- Source of Truth
- Unknown ≠ Zero
- 반복입력 기억값은 필요하면 TTL
- 글씨 확대 시 컨테이너도 함께 확대
- stable auth/signing을 이유 없이 갈아엎지 않기
- secret은 안전한 staging으로 주입 후 제거
- schema migration-first

## 사이버 아쿠아리움 / ASCII Aquarium
- 사람용 진행상태와 raw log 분리
- user-initiated 작업과 background 작업의 피드백 강도 구분
- Portable updater는 READY handshake 후 기존 앱 종료
- replacement 검증 후 교체
- 한글/공백 경로 QA
- 파생판은 working desktop baseline과 분리

---

# 로그·관제 기본값

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

대형 로그는 가능하면 incremental/tail 방식으로 처리합니다.

---

# 업데이트·복구 기본값

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

중간 실패 시 기존 정상판으로 돌아갈 수 있어야 합니다.

---

# 🧰 꿀팁 링크함

아래는 나중에 다시 쓸 수 있는 참고 후보입니다. 여기에 있다고 설치·채택된 것은 아닙니다.

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

### 링크 관리 원칙
- 같은 도구는 canonical URL 기준으로 한 번만 둡니다.
- SNS/블로그/스크린샷보다 원본 GitHub/공식 사이트를 우선합니다.
- 실제 사용 전 설치·로그인·비용·라이선스·보안·호환성을 다시 확인합니다.
- 실제 프로젝트에 채택되면 정확한 버전·설정·권한·명령은 그 프로젝트 `AGENTS.md`에 기록합니다.

---

# 문서 동기화 규칙

앞으로 AI는 작업 시작 시 다음 순서로 봅니다.

```text
현재 사용자 지시
→ 프로젝트 README
→ Workbench README
→ 프로젝트 AGENTS
→ Workbench AGENTS
```

README와 AGENTS가 충돌하면 **README의 사용자 의도를 기준으로 AGENTS를 수정한 뒤 작업**합니다.

내가 README만 고쳐도 되게 만드는 것이 목표입니다.

반대로 AI가 개발 중 중요한 규칙을 새로 발견했다면 AGENTS에 기록하고, 내가 알아야 하거나 앞으로 컨트롤할 가치가 있는 내용은 README에도 올립니다.

**사용자 버전과 개발자 버전은 분리하되, 서로 어긋난 채 방치하지 않습니다.**