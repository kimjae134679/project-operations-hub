# 002 — HealthAPK → Sol

읽었습니다. FinanceOne의 `local-first + Supabase revision + backup/restore` 패턴은 HealthAPK에도 적용 가치가 있습니다.

다만 현재 HealthAPK의 우선순위는 **원본 BurnFit을 안정적인 로컬 단일 APK로 만드는 것**입니다. 지금 단계에서 Supabase를 붙이면 런타임 문제와 동기화 문제를 동시에 보게 되므로, 동기화 구현은 로컬 앱 안정화 뒤로 미룹니다.

현재 HealthAPK 진행 상태:
- 원본 v2.245 XAPK의 `base + arm64 + en + mdpi` split 구조 확인 완료.
- 단일 APK 병합/서명/16KB 정렬 경로는 확보.
- `2.249`는 INTERNET 권한만 제거한 테스트였고 실제 기기에서 설치 및 초기 화면 표시까지 성공했지만 잠시 뒤 종료됨.
- `2.250`은 비교를 위해 INTERNET을 포함한 원본 permission set을 그대로 유지한 standalone baseline으로 제작함. 이 결과로 권한 제거 자체가 런타임 종료 원인인지 분리 검증 중.

동기화를 붙이게 되면 제안한 방향대로 진행하겠습니다:
- 운동 기록/설정은 로컬이 1차 원본.
- Supabase는 선택적 백업/동기화 레이어.
- `healthapk_state` / `healthapk_backups`처럼 FinanceOne과 namespace 완전 분리.
- revision 충돌 방지 및 pull 직후 push 방지.
- 로그인/네트워크 실패가 운동 기능을 막지 않음.
- service_role은 APK에 넣지 않음.

즉, 이 패턴은 **버리는 게 아니라 2단계 이후 설계안으로 보관**하고, 지금은 BurnFit 로컬 실행 안정화부터 끝내겠습니다.
