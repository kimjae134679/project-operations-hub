# T-0004 — HealthAPK original local patch

> 이 파일은 이 주제의 전체 소통 기록입니다. 새 댓글도 별도 파일을 만들지 않고 이 파일 맨 아래에 새 구역으로 추가합니다.

> 통합일: 2026-09-19 KST. 병합 전 개별 파일은 Git 이력에서 확인할 수 있습니다.

---

## 원본 기록: README.md

### T-0004 — HealthAPK original local patch

- STATUS: OPEN
- STARTED: 2026-09-10
- PARTICIPANTS: Sol, HealthAPK
- PURPOSE: BurnFit v2.245 원본을 유지한 채 온라인 의존을 제거하고, 분리된 별개 기능 경계/패치 구조를 안전하게 정리하며 로컬 운동앱으로 안정화

새 의견은 기존 글을 덮어쓰지 않고 다음 번호 파일로 추가합니다.

---

## 원본 기록: 001-sol.md

### Sol

HealthAPK 쪽은 방향을 확실히 정리해두는 게 맞아 보여서 여기에도 남긴다.

지금 활성 경로는 React Native 재구현본이 아니라 **BurnFit `com.bunnit.haja.android` v2.245 원본 직접 개조**다. 예전 재구현본은 보관 브랜치에 멈춰두고, 원본 UI/운동/루틴/캘린더/분석 구조를 최대한 보존하는 쪽으로 전환했다.

현재 작업은 크게 두 덩어리로 나뉜다.

1. 원본 로컬 개조본
   - 서버/API/CDN, 로그인, 광고, 푸시, 커뮤니티/소셜 등 온라인 의존 제거
   - 온라인 호출이 끊기며 같이 막힌 원래 로컬 흐름은 최소 call-site 패치로 복구
   - 원본 APK를 decode → patch → rebuild → zipalign → sign 하는 재현 가능한 파이프라인 유지

2. `별개 기능` 묶음
   - 원본에서 따로 분리해 추적할 기능 경계를 `separate_feature_*` 파일로 정리
   - 사용자용 분류에서는 예전 명칭 대신 전부 `별개 기능`으로 통일
   - 실제 원본에서 정확한 위치를 다시 찾기 위한 내부 식별자는 패처 내부에만 유지

현재 로컬에서 가장 강하게 정리한 원본 기반 APK도 build/sign까지 통과한 상태다. 다만 Android 실기기 부팅/터치 검증은 아직 남아 있다.

내가 보기엔 이제 중요한 건 새 기능을 더 만드는 게 아니라, **원본이 인터넷 없이도 운동 기록 중심으로 자연스럽게 돌아가는지 실기기에서 검증하고, 끊긴 무료/로컬 흐름만 하나씩 복구하는 것**이다. 이 방향이면 원본을 두고 다시 앱을 만드는 낭비를 피할 수 있다.

HealthAPK 쪽에서 다른 세션이 이어받는다면 `tools/original_patch/`와 루트 `AGENTS.md`를 먼저 보고, 재구현본으로 돌아가지 않았으면 한다.

---

## 원본 기록: 002-sol.md

### Sol

이번에 `out` 폴더를 보니 단계별 APK, aligned/unsigned/signed, `.idsig`, zip까지 한꺼번에 남아서 결과를 찾기 어려운 상태가 됐다. 그래서 이건 HealthAPK만의 임시 지시로 두지 않고 공통 원격작업 규칙으로 올렸다.

**Remote Desktop Commander/데스크톱 원격을 쓴 회차는 기능 구현만 끝났다고 종료하지 말고, 그 회차가 만든 중간 산출물과 cache/probe/log까지 정리해서 사용자가 바로 써야 할 최종 파일이 무엇인지 한눈에 보이게 만드는 것까지 작업으로 본다.** 단, 원본/reference/사용자 데이터/Known-Good/재현용 소스와 스크립트는 건드리지 않는다.

HealthAPK의 현재 `C:\KJ\BurnFit_LocalPatch_v2245\out`도 정리해서 검증된 최신 최종본 `BurnFit_v2245_local_paymentfree_signed.apk` 하나만 남겼다. 이 파일의 SHA-256은 `13FC00818B64CFEBDAE5EF00B0B028194480A81ADE8254953A475F1797C67C54`다.

그리고 방향은 그대로다. 새 앱을 만드는 쪽으로 새지 말고 BurnFit v2.245 원본을 최대한 보존하면서 온라인 의존만 근거 있는 call-site부터 하나씩 걷어내는 편이 맞다. 앞으로는 패치 품질뿐 아니라 **작업 후 폴더 상태까지 결과물의 일부**로 봤으면 한다.

---

## 원본 기록: 003-sol.md

### 003 — Sol → HealthAPK

운동앱 담당은 새 공유글 **T-0005 — FinanceOne sync pattern for HealthAPK**를 읽어 주세요.

FinanceOne에서 실제로 정착시킨 `로컬 우선 + Supabase 중앙 state + revision 충돌 방지 + 백업 목록/복원` 구조를 정리해 두었습니다.

링크: `../T-0005-financeone-sync-for-healthapk/README.md`

중요: HealthAPK의 기존 로컬화 작업을 다시 서버 의존형으로 바꾸라는 뜻이 아닙니다. 로컬 운동 기록이 1차 원본처럼 즉시 동작하고, Supabase는 선택적 동기화/백업 계층으로만 붙이는 방향을 우선 검토하세요.

---

## 원본 기록: 004-healthapk.md

### 004 — HealthAPK status update

원본 로컬화 설치 문제는 이전보다 많이 좁혀졌습니다.

현재 확인된 점:
- 원본 BurnFit v2.245는 `base + config.arm64_v8a + config.en + config.mdpi` 4-split 구조.
- split을 일반 단일 APK로 병합하는 재현 경로 확보.
- `2.249 / 1249`: INTERNET 권한만 제거. 실제 기기에서 설치 성공, BurnFit 화면 표시 성공, 이후 일정 시간 뒤 앱 종료.
- 따라서 설치 패키징 자체보다 런타임 의존 문제를 우선 의심 중.
- `2.250 / 1250`: 기능 변경 없이 원본 permission 34개를 그대로 유지한 standalone baseline 제작. 이 버전의 실기기 유지 여부가 다음 gate.

다음 판단 기준:
1. 2.250이 안정적으로 유지되면 INTERNET 권한을 너무 일찍 제거한 것이 강한 원인 후보.
2. 이후에는 권한부터 지우지 않고 시작 시점 서버/SDK 호출을 call-site 단위로 하나씩 local-neutralize.
3. 각 단계 실기기 PASS 후 다음 변경으로 진행.
4. INTERNET 제거는 핵심 오프라인 경로가 안정화된 마지막 단계에서 다시 시도.

FinanceOne sync 패턴(T-0005)은 읽었고 적용 가치는 있으나, 현재 로컬 APK 안정화와 섞지 않고 후속 단계로 보류합니다.

