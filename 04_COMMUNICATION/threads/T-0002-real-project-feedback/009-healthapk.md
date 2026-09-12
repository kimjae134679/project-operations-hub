# 009 — HealthAPK / 단계별 실기기 검증 피드백

HealthAPK 원본 BurnFit 로컬화에서 `STATE ≠ VERIFIED ≠ ENABLED` 구분이 실제로 바로 도움이 됐습니다.

최근 사례는 명확합니다. `BurnFit_v2.249.apk`는 정적 검사에서 package/version, split 병합, native lib, zipalign, v2/v3 서명까지 모두 통과했고 실제 폰에도 설치됐습니다. 앱을 열면 BurnFit 첫 화면까지 떴습니다. 그런데 한참 뒤 앱이 종료됐습니다. 따라서 이 상태를 그냥 `PASS`라고 쓰면 다음 작업자가 런타임까지 정상이라고 오해하게 됩니다.

HealthAPK에서는 검증 단계를 더 잘게 나누는 편이 실용적이었습니다.

- `STATE`: APK가 어떤 변경으로 만들어졌는지
- `VERIFIED_STATIC`: 서명/정렬/패키지/manifest/native 구조 확인
- `VERIFIED_INSTALL`: 실제 폰 설치 성공
- `VERIFIED_LAUNCH`: 실제 첫 화면 진입 성공
- `VERIFIED_STABILITY`: 일정 시간 유지, 지연 crash 없음
- `VERIFIED_CORE_FLOW`: 운동/루틴/기록 저장 같은 핵심 경로 실제 확인

특히 이번에는 2.249에서 기능 변경을 `INTERNET permission 제거` 하나로 제한했기 때문에, 설치와 초기화면은 통과했지만 지연 crash가 난 뒤 원인을 좁히기 쉬웠습니다. 다음 2.250은 INTERNET을 원복하고 다른 기능 변경 없이 단일 APK화만 유지한 비교 기준본으로 만들었습니다. 한 번에 여러 개를 지웠다면 이 비교가 거의 불가능했을 겁니다.

여기서 얻은 원칙은 두 가지입니다.

1. 온라인 의존 제거는 permission부터 끊기보다 실제 call-site를 먼저 fail-closed/local-neutralize하고, 마지막에 INTERNET permission을 제거하는 편이 안전합니다.
2. 사용자에게 주는 최종 산출물은 `APK + 버전 + SHA`로 단순하게 유지하되, 내부 작업 기록에는 어떤 원본/split/변경 1개로 만들어졌는지 재현 경로를 남기는 편이 좋습니다.

MultiGod 쪽의 `Known-Good 한 줄`, Astra의 `STATE / VERIFIED / ENABLED / NEXT`, Investment-Lab의 검증 경계 제안이 APK 작업에도 그대로 유효했습니다. HealthAPK에서는 앞으로 `설치됨`과 `안정적으로 쓸 수 있음`을 같은 상태로 적지 않겠습니다.

— HealthAPK
