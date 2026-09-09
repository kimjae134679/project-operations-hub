# Astra Codex Workbench

여러 프로젝트에서 실제로 겪은 **개발·UX·빌드·배포·QA 경험을 서로 공유하기 위한 최상단 허브**입니다.

핵심은 단순합니다.

- 각 프로젝트는 관리 문서를 **`README.md` + `AGENTS.md` 두 개만** 유지합니다.
- `README.md`는 **내가 보는 현재 상태/요약**입니다.
- `AGENTS.md`는 **다음 AI/Codex가 바로 작업하기 위한 작업 매뉴얼**입니다.
- 프로젝트마다 얻은 좋은 방법은 여기로 모으고, 다른 프로젝트에서 맞는 것만 골라 재사용합니다.
- GitHub가 필요 없는 프로젝트에는 저장소를 억지로 만들지 않습니다.
- 비밀번호·토큰·개인키 같은 비밀값은 문서에 기록하지 않습니다.

---

## 전체 구조

```text
Astra Codex Workbench
│
├─ README.md                 ← 내가 보는 최상단 요약
├─ AGENTS.md                 ← AI/Codex가 보는 공통 작업 규칙
│
├─ GitHub가 있는 실제 개발 프로젝트
│  ├─ 운동앱              → HealthAPK
│  ├─ 주식자동매매        → Investment-Lab
│  ├─ 청약                → ChungYack + stock/chungyack-apk
│  ├─ 멀티의신            → PhoneLOL
│  └─ 주식 앱 / Market Radar → stock
│
├─ GitHub가 없어도 되는 프로젝트
│  ├─ 피규어만들기_01
│  └─ 동물의숲 / Tiny Village
│
├─ 개발 경험을 추가로 회수한 프로젝트
│  ├─ 사이드메모장
│  ├─ FinanceOne 리뉴얼
│  └─ 사이버 아쿠아리움 / ASCII Aquarium
│
└─ 공통으로 축적하는 것
   ├─ UX / 편의성
   ├─ 코드 수정 방식
   ├─ 빌드 / 패키징
   ├─ 한글 / Windows
   ├─ 업데이트 / 마이그레이션
   ├─ 로그 / 관제
   ├─ 실제 기기 QA
   └─ 복구 / 롤백 / 작업 정리
```

---

## 현재 프로젝트 정리 현황

### GitHub가 확인된 프로젝트 — 정리 완료

- **운동앱** → `kimjae134679/HealthAPK`
- **주식자동매매** → `kimjae134679/Investment-Lab`
- **청약** → 허브 `kimjae134679/ChungYack`, 실제 live shell `kimjae134679/stock/chungyack-apk/`
- **멀티의신** → `kimjae134679/PhoneLOL`
- **주식 앱 / Market Radar** → `kimjae134679/stock`

위 프로젝트는 루트 `README.md` + `AGENTS.md` 체계로 정리했습니다.

### GitHub가 없어도 되는 프로젝트

**피규어만들기_01**
- 레퍼런스 이미지와 최대한 흡사한 Blender 피규어/3D 오브젝트 제작.
- 기존 초안은 보존 대상이 아니며 필요하면 다시 시작 가능.
- 기본 순서: `blockout → 비율/실루엣 → 디테일`.
- GitHub가 확인되지 않았고, **저장소가 반드시 필요한 프로젝트로 취급하지 않습니다.**

**동물의숲 / Tiny Village**
- 동물의숲 감성의 아늑한 데스크톱 설정 UI 콘셉트.
- 초록·베이지·목재 질감·둥근 패널 중심.
- 일반 / 주민 / 상호작용 / 화면 / 소리 / 기타 설정 구조가 확인됨.
- GitHub가 확인되지 않았고, **비슷한 저장소에 억지로 매핑하지 않습니다.**

둘 다 나중에 사용자가 저장소 연결/생성을 직접 요청할 때만 GitHub 작업을 합니다.

---

# 공통 개발 가이드

여러 프로젝트를 합쳐서 현재 가장 재사용 가치가 높다고 판단한 기본값입니다.

## 1. 프로그램을 쓰는 시작점은 하나

평소 사용자는 개발 구조를 몰라도 되게 합니다.

```text
좋음
└─ AA_Test.exe

또는
└─ RUN_START_HERE.cmd

피하고 싶은 상태
├─ run-dev.cmd
├─ run-real.cmd
├─ run-new.cmd
├─ start2.cmd
└─ 어떤 걸 눌러야 하는지 설명 20줄
```

고급 기능은 필요하면 별도 메뉴/옵션으로 둡니다.

## 2. 화면은 `현재 상태 → 지금 할 행동 → 결과 → 상세` 순서

기본 화면에서 가장 먼저 보여야 하는 것은 지금 필요한 정보입니다.

- 대표 행동 버튼은 가능하면 하나를 강하게 표시.
- 같은 목적의 버튼을 여러 곳에 중복하지 않음.
- 과거 로그·원자료·고급 설정은 숨기지 않되 상세 영역으로 내림.
- 버튼을 눌렀는데 아무 반응이 없는 상태는 만들지 않음.

## 3. 모바일이라고 기능을 삭제하지 않기

PC와 모바일은 **기능을 공유하고 배치만 다르게** 합니다.

```text
PC
└─ 사이드바 / 넓은 표 / 펼친 정보

Mobile
└─ 하단탭 / 카드 / 가로스크롤 / safe-area
```

작은 화면이라는 이유로 중요한 설정·백업·통계·정보를 없애지 않습니다.

## 4. 잘되는 기능은 통째로 다시 쓰지 않기

기존 정상 기능이 있다면 **검증된 정상판(Known-Good) + 최소 수정**을 기본으로 합니다.

- 문제 난 함수/영역만 작게 수정.
- 새 라이브러리보다 기존 코드·표준 기능을 먼저 검토.
- 인증·서명·네트워크처럼 이미 안정화된 부분은 이유 없이 재설계하지 않음.
- 새 버전이 실패하면 정상판으로 되돌릴 수 있게 유지.

## 5. 완료는 코드 작성이 아니라 실제 사용 흐름

```text
코드 작성
→ 정적 검사
→ 자동 테스트
→ 실제 빌드
→ 패키지 확인
→ 실제 설치/실행
→ 핵심 사용자 흐름
→ 재실행/데이터 유지
→ 회귀 확인
```

`CI PASS`, `빌드 PASS`, `실기기 PASS`는 서로 다른 증거입니다.

---

# 프로젝트끼리 서로 배운 것

## 🏋️ 운동앱 / HealthAPK

**특기: local-first + 실제 사용자 흐름 검증**

- 서버가 꼭 필요하지 않다면 로컬 우선으로 만들기.
- 로그인/결제/네트워크가 없어도 핵심 기능이 작동하게 하기.
- `운동 선택 → 세트 입력 → 휴식 → 완료 → 재실행 → 기록 확인`처럼 끝까지 테스트.
- 개발용 debug/Metro 앱과 실제 standalone APK는 따로 검증.
- 모르는 metadata를 임의의 값으로 채우지 않기.

## 📈 주식자동매매 / Investment-Lab

**특기: 실행 편의 + 안전 gate + Windows UTF-8**

- `RUN_START_HERE.cmd` 같은 더블클릭 진입점 하나.
- 과거 PASS를 새 HEAD의 PASS로 간주하지 않고 다시 검증.
- `권한 없음 / 조건 미충족 / 안전 대기`와 실제 오류를 분리.
- 실제 API 키는 로컬 secret에 두고 `.env.example`에는 이름만 기록.
- Windows 한글 출력 기본 후보:

```cmd
@echo off
chcp 65001 >nul
set PYTHONUTF8=1
set PYTHONIOENCODING=utf-8
```

## 🏠 청약 / ChungYack

**특기: 자주 바뀌는 콘텐츠와 APK shell 분리**

- HTML/UI/데이터 변경만으로 native APK를 매번 다시 만들지 않는 구조.
- persistent ID/localStorage key는 사용자 데이터 주소처럼 취급.
- 큰 migration 전 `백업 → 전환/재설치 → 복원` 준비.
- 기본 카드에는 판단에 필요한 정보만 두고 원자료는 상세로.
- 같은 기능 버튼 중복과 중첩 accordion을 피하기.

## 🎮 멀티의신 / PhoneLOL

**특기: 정확한 기준판 + 최소 patch + 실제 기기 증거**

- 정상 APK를 고칠 때 기준 SHA를 정확히 기록.
- 정적검사 PASS와 실제 폰 PASS를 구분.
- Windows self-hosted build는 쓰기 가능한 임시경로에서 `build → sign → verify` 후 최종 위치로 복사.
- 중요한 산출물은 필요하면 `APK + ZIP + SHA-256` 한 세트로 관리.
- 로그/관제에서는 현재 연결과 과거 세션, 실제 게임 채널과 진단 채널을 분리.

## 📊 주식 앱 / Market Radar

**특기: 모바일·PC UI QA + 자동화 경계**

- desktop + phone viewport를 둘 다 QA 대상으로 잡기.
- 가로 overflow, 잘림, 작은 글씨, modal/back, page error 검사.
- 모바일에 맞추려고 차트/정보를 읽을 수 없게 축소하지 않기.
- 자주 갱신되는 데이터와 사람이 다듬는 UI 코드를 분리.
- 시간마다 새 파일을 무한 생성하기보다 live 파일 + archive 구조 사용.

## 📝 사이드메모장

**특기: 작은 데스크톱 툴의 편집 UX**

- 좁은 화면에서는 저빈도 기능을 `...` 버튼으로 계속 늘리기보다 우클릭 메뉴 등으로 이동.
- 편집영역과 툴바 사이의 불필요한 여백을 줄여 실제 작업공간 확보.
- Delete/Backspace 같은 위험 행동은 **명시적으로 선택된 객체에만** 적용.
- 복사/붙여넣기처럼 결과가 눈앞에 바로 보이면 성공 토스트를 남발하지 않음.
- contenteditable 서식 버튼은 클릭 전에 selection range를 저장하고 다시 복원.
- 한글 IME 단축키는 `event.key` 하나만 믿지 말고 `event.code`도 검토.
- 프로그램 삭제와 사용자 데이터 삭제는 별개로 취급.

## 💰 FinanceOne 리뉴얼

**특기: 크로스플랫폼 데이터·반응형·마이그레이션·인증**

- 큰 UI 개편은 `현재 화면 → 개선 도안 → 기능 누락 비교 → 구현 → 회귀 QA` 순서.
- 같은 개념의 계산은 화면마다 따로 만들지 않고 하나의 Source of Truth 사용.
- 반복 입력 편의용 기억값에는 필요하면 TTL 적용.
- 데이터가 모르는 값이면 `0`으로 바꾸지 않고 `미확인/환산 불가/정산 대기`로 구분. **Unknown ≠ Zero.**
- 글씨 확대는 font-size뿐 아니라 카드/버튼/행/입력칸도 함께 커져야 함.
- Android system font scale과 앱 자체 zoom이 중복되지 않게 검증.
- OAuth/서명 설정이 잘못됐으면 틀린 키로 성공 처리하지 말고 빌드를 실패시키기.
- 비밀 설정은 빌드 시 임시 staging으로 주입하고 값은 로그에 출력하지 않기.
- schema 변경 전 normalize/migration을 먼저 만들고 과거 백업 복원을 테스트.

## 🐠 사이버 아쿠아리움 / ASCII Aquarium

**특기: Windows Portable/Setup + self-update + 빌드 UX**

- Setup과 Portable을 필요에 따라 둘 다 제공.
- 사람에게는 짧은 진행상태, 개발자에게는 별도 raw log를 제공.
- 사용자 직접 업데이트 확인은 진행상태를 보여주고, 백그라운드 자동 확인은 정상일 때 조용히.
- Portable self-update는 별도 updater/helper를 사용하고 **READY handshake 후에만 기존 앱 종료**.
- `프로세스 spawn 성공 ≠ 실제 updater 준비 완료`로 취급.
- 한글 경로와 공백 포함 경로를 실제 업데이트 QA 항목에 포함.
- 사용 가이드 이미지는 설명을 실제 버튼 가까이에 두고 1:1로 연결.
- Wallpaper Engine 같은 파생판은 데스크톱 원본을 훼손하지 않고 별도 버전으로 분리.

---

# 데이터·저장·마이그레이션 기본값

- 앱 설치파일과 사용자 데이터를 분리합니다.
- 앱 삭제/재설치가 사용자 데이터 삭제를 자동 의미하지 않게 합니다.
- 한번 배포된 DB key, record ID, localStorage key는 호환성 계약처럼 다룹니다.
- schema 변경은 normalize/migration 후 과거 데이터 복원 테스트.
- 모르는 값과 실제 0을 구분합니다.
- 동기화는 첫 실행부터 강제하기보다 local-first + 선택적 연결을 우선 검토합니다.

---

# 빌드·배포 기본값

가능한 경우 다음 흐름으로 통일합니다.

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

산출물 이름은 의미 있게 둡니다.

```text
MyApp-v1.4.2.apk
MyApp-Setup-v1.4.2.exe
MyApp-Portable-v1.4.2.exe
MyApp-v1.4.2.zip
SHA256SUMS.txt
```

파일명 버전 / 앱 내부 버전 / package 버전이 서로 어긋나지 않게 검증합니다.

---

# Windows / 한글 기본값

- 텍스트·JSON·로그는 UTF-8을 기본으로 명시.
- PowerShell 파일 출력도 UTF-8을 명시.
- Python은 필요하면 `PYTHONUTF8=1`, `PYTHONIOENCODING=utf-8`.
- 한글 IME 상태에서 단축키 실제 테스트.
- 오래된 Android/Java/Unity/CLI가 한글 경로에 약하면 **임시 빌드 경로만 ASCII**로 사용.
- 최종 프로그램 자체는 한글 경로와 공백 경로에서도 실행/업데이트되는지 가능하면 확인.

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

화면에 raw 로그를 수백 줄 쌓기보다 **사람용 상태판과 개발자용 상세로그를 분리**합니다.

대형 로그는 가능하면 전체를 계속 다시 읽지 않고 incremental/tail 방식으로 처리합니다.

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

# 작업 후 정리 규칙

특히 Desktop Remote/원격 PC에서 작업할 때는 **작업이 끝난 뒤 최종 사용에 필요 없는 찌꺼기를 남기지 않습니다.**

삭제 후보:
- 임시 build 폴더
- 테스트용 복사본
- 실패한 중간 산출물
- 임시 ZIP/EXE/APK
- 캐시
- 필요 없는 디버그 로그
- 일회성 스크립트

하지만 다음은 지우면 안 됩니다.
- 실제 프로젝트 소스
- 다음 수정에 필요한 설정/스크립트
- 사용자 데이터
- 정상 rollback에 필요한 기준판
- 최종 산출물
- 재현에 필요한 최소 문서/검증정보

즉 **“최종 작업 구조 + 다음 수정에 필요한 것”만 남기고 일회성 찌꺼기를 정리**합니다.

---

# 완료 상태 표시

완료 여부는 다음처럼 구분합니다.

```text
미착수
진행 중
막힘
코드 완료 / 미검증
자동검증 PASS
빌드 PASS
설치 PASS
실기기 PASS
최종 ACCEPTED
```

AI가 `완료`라고 말한 것만으로 완료로 보지 않습니다.

---

# 🧰 꿀팁 링크함

아래는 나중에 다시 쓸 수 있는 **참고 후보**입니다. 여기에 있다고 설치·채택된 것은 아닙니다.

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
