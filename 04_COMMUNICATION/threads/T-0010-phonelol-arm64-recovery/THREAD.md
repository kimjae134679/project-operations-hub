# T-0010 — PhoneLOL ARM64 복구와 인수인계

상태: OPEN — 1.16.8 후보 전달, 실기기 확인 대기.
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

## 2026-09-24 — [B계정] Nova — [B계정] 1.16.8 후보 빌드·서버 반영

사용자 요청에 따라 작업을 재개하고, 추가로 제보된 Unity Package Manager 시작 오류를 해결했다.

- 소스 push: [786409c](https://github.com/kimjae134679/PhoneLoL_02/commit/786409ccfc6f6e11b24b748de3120418703e4236).
- [현재 인수인계](https://github.com/kimjae134679/PhoneLoL_02/blob/work/v1164-unity6-recovery/RECOVERY_STATUS.md) 상단이 최신 상태다. 이전 1.16.8 미빌드 기록은 아래쪽 이력으로 보존했다.
- APK: PhoneLOL-v1.16.8-arm64-candidate.apk, versionCode 194, 117508955 bytes. SHA-256: 81881b91b5080f7e34949b323168a8676fb77f24afffc9cdc2690ef664fd17e5.
- 빌드 성공: 오류 0, 경고 760. 포함된 native library 6개 모두 arm64-v8a/ELF64. 설치 경로는 프로젝트 문서에 기록하고 사용자에게 먼저 전달했다.
- 원본 APK에서 Actor 193개를 프리팹/장면 167개에 복원. 932개 정적 배치 메시의 Unity 참조 확인. 두 map shader 구문 오류 수정.
- 챔피언 목록의 byte/UInt32 불일치와 bootstrap 순서, 준비 화면 null 상태, 진단 패킷 오응답을 수정했다.
- 관리형 1인/다인 시작, 실제 방장의 결과 저장·중복 정산 방지, 참가자의 결과 조회, DB 기반 랭킹을 구현했다.
- 원본 점수 산식은 확보되지 않았다. 복구 서버는 1000 시작/K32 Elo를 사용하며, 실제 양 팀이 있는 랭크 경기만 반영한다. 1인·일반·친선은 랭킹을 올리지 않는다. 경험치/코인 보상은 기존처럼 미지급이며 과거 기록을 만들지 않았다.
- 원본 PNG 유지, adaptive icon 확대 설정 제거. 실제 런처 표시와 플레이는 사용자 확인 대기.
- Package Manager 실행 오류: 원격 환경에서 빠진 Windows 기본 변수를 프로세스에 보완해 해결. Automation/OpenUnity.ps1로 재현 가능. 시스템 환경/백신 변경 없음.
- 실행 폴더의 일부 서버 파일이 이전 상태로 돌아가 있음을 발견했으나 원인은 확인하지 못했다. 실제 차이를 읽고 DB/소스 백업 후 현재 코드로 반영했다.
- backup: before-v1168-20260924-170631. 로컬·공개 endpoint 진단 업로드 HTTP 204 확인.
- 임시 DB에서 1인/2인 시작·relay·결과·랭킹·중복 방지·무료 닉네임을 확인했다. 실기기 전체 플레이, 시각 일치, reconnect, native cross-play, iOS 인증은 아니다.
- 생성한 probe/진단 임시 파일과 이 빌드의 중복 C++/managed 출력 폴더를 정리했다. APK, 실제 서버 로그/백업, 기준 원본과 사용자 UnityConnectSettings 변경은 보존했다.

다음은 사용자의 아이콘·챔피언 선택·1인/2인 진행·맵/HUD·종료/랭킹 피드백을 받고, 같은 진단 ID의 서버 로그로 좁혀 수정하는 단계다.
