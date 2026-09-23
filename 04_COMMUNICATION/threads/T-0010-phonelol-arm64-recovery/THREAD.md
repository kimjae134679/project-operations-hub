# T-0010 — PhoneLOL ARM64 복구와 인수인계

상태: OPEN — 복구 미완료, 인수인계 체크포인트 보존.
현재 상태 원본: [RECOVERY_STATUS.md](https://github.com/kimjae134679/PhoneLoL_02/blob/work/v1164-unity6-recovery/RECOVERY_STATUS.md).
후속 기록은 별도 댓글 파일을 만들지 말고 이 THREAD.md 아래에 이어 쓴다.

## 2026-09-23 — [B계정] Nova — [B계정] 전체 작업 인수인계

사용자 요청에 따라 추가 개발/빌드를 중단하고 다른 담당자가 이어받을 상태를 GitHub에 보존했다.

- 실제 개발 저장소: PhoneLoL_02, 브랜치 work/v1164-unity6-recovery. 이전 PhoneLOL은 참고 저장소다.
- [보존 커밋 7b3ee3a](https://github.com/kimjae134679/PhoneLoL_02/commit/7b3ee3a4b2005c701c64f69cdbeabd0067c29443) push 완료.
- [전체 인수인계 고정본](https://github.com/kimjae134679/PhoneLoL_02/blob/7b3ee3a4b2005c701c64f69cdbeabd0067c29443/RECOVERY_STATUS.md) 상단을 먼저 읽는다. 하단 버전별 기록은 과거 시점의 이력이다.
- 최신 APK/서버 기준은 1.16.7 / d97a5a0. 1.16.8 APK는 없으며 이번 WIP의 서버 배포도 하지 않았다.
- 사용자 피드백: 아이콘 확대/잘림, 1인 진행 불가, 방/게임 입장 이후 정상 플레이 불가, 전체 배경/맵 파손, 잘못된 랭킹.
- 보존 내용: 정적 배치 메시 932개, 두 scene 참조 수정, map shader 두 개의 원본 식 복구, 원본 shader index 76개 기록, 재현 스크립트/대응표.
- 확인한 추가 원인: 검사한 Actor prefab의 게임 직렬화 데이터 소실. 원본 DLL로 타입 트리 생성 성공, 실제 값 복원은 아직 미구현.
- 다음 순서: Actor/UI 데이터와 P5 hero-list 계약 → 맵 화면 확인 → 1인/다인 준비·시작·결과 → 실제 랭킹 저장 → adaptive icon 설정 → 최소 검사 및 1.16.8/194 빌드.
- P5 3바이트/UInt32 세 개 불일치, game 33/63, battle 21/22, 최소 2명 하드코딩도 상세 문서에 남겼다.
- WIP 전체 컴파일/렌더/APK/실기기 검증은 미실행이다. 기존 프로토콜 검사나 타이머 진행을 정상 전투 완료로 표현하지 않는다. 폰 검증은 사용자가 하며 과도한 반복 검사를 피한다.
- 사용자 별도 변경 UnityConnectSettings.asset은 커밋/복원하지 않고 남겼다.
- 1.15.11 원본 보호, ARM64, Unity 로고 없음, 알림 억제, 무료 무제한 닉네임, 친구 입력 보호, 교체 가능한 서버와 실시간 로그, 향후 iOS 요구를 유지한다.
- 현재 Windows 경로, APK hash, live DB/source backup, tunnel/이사 대응, Unity CLI 명령, 재개 순서를 프로젝트 문서에 통합했다.
- 기존 Python311에 TypeTreeGeneratorAPI 0.0.10을 설치했다. 실제 경로와 AI 기본 루트 외 설치 이유는 프로젝트 인수인계 참조. Jev 설치를 현재 운용 성공으로 간주하지 않는다.

재개 시 실제 저장소 tip/git status와 서버 상태를 확인한다. 사용자 DB, 인증정보, raw 개인정보 로그는 Git에 넣지 않았다.
