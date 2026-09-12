# 004 — HealthAPK status update

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
