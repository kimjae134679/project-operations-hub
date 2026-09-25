# T-0010 — PhoneLOL ARM64 복구와 인수인계

상태: OPEN — 1.16.9 로컬 후보·서버 반영 완료, 실기기 확인 및 GitHub 업로드 승인 대기.
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

## 2026-09-24 — [B계정] Nova / Codex Work — 1.16.9 구현·빌드·서버 완료, GitHub 업로드 보류

- 실제 로컬 소스: D:\A_KJ\AI\PhoneLoL_02, work/v1164-unity6-recovery, 구현 커밋 a64e535670e2bc2ca0517737c628a1b563172fae. 기존 원격 최신은 786409c이며 이번 소스는 아직 push하지 못했다.
- APK: D:\A_KJ\AI\PhoneLoL_02\PhoneLOL-02\Builds\PhoneLOL-v1.16.9-arm64-candidate.apk. 1.16.9/195, 136944006 bytes, SHA-256 8d8afbe292385af089a2fac69fbcb4241e2189e42e7ab2f9d9fbf17e3513788c. Build succeeded: 0 errors/760 warnings, native libraries 6개 모두 ARM64/ELF64.
- 전투: bounded Eve session으로 맵 오브젝트 번호 충돌과 Int32 overflow 방지. 양방향 팀 이동, 원본 walkable polygon 기반 두 맵 NavMesh 복구.
- 원본 조명: 두 맵 각각 2 lightmaps, 296/638 bindings, atlas UV2/ST 보정. 애니메이션 44개 복구 controller의 316 states/178 transitions/누락 motion 0. 숨겨진 main-controller flag도 수정했다.
- 일반대전은 모드대전(mode 10)으로 변경. 아이템 8칸 및 shop/HUD/use/result UI·전송, unique 장비 중복 허용, passive gold 2배, 팀당 미니언 8마리, 정글 리젠 시간 절반. 랭크·친선 규칙 유지. 모든 참가자는 1.16.9 설치 필요.
- 검증: Unity의 모드별 inventory round-trip·UI 배열·조명·spawn sample·애니메이션 참조, disposable server의 1인/2인·team move·direct/broadcast relay·8-item result·ranking/duplicate settlement. 실기기 전투·시각 일치·터치 사용성 검증은 사용자 담당이며 미확인.
- 기존 서버 소스와 SQLite를 before-v1169-20260924-212259에 백업하고 managed_battle_v1167.py / managed_results_v1168.py만 반영. 기존 launcher/DB/tunnel 유지. 29000 listen 및 로컬·공개 진단 HTTP 204 확인.
- 자동 승인 심사가 공개 저장소로 이번 소스·복구자료·운영기록을 게시하는 명시적 승인 부족으로 push를 거절했다. 동일 기존 승인 대상과 ADMIN 권한까지 확인 후에도 재거절됐다. 우회하지 않았고 이 허브 기록도 현재 로컬에만 저장한다.
- 다음: 사용자는 APK 테스트. 공개 업로드에 대한 명시적 승인 후 실제 저장소 및 이 스레드·사용자 요약을 push한다. 상세 원본은 로컬 RECOVERY_STATUS.md 최신 구역이며 원격 문서는 아직 1.16.8이다.
- 새 프로그램 설치 없음. 기존 사용자 지정 프로젝트/Unity/Python 경로 재사용. 이번 임시 probe와 중복 debug build 폴더는 제거했다. 사용자 UnityConnectSettings.asset 별도 변경은 보존·커밋 제외.
## 2026-09-25 — [B계정] Nova / Codex Work — 재부팅 후 서버 되돌림 수정

- 사용자 요청: 컴퓨터 재부팅 후 중단 위치 확인 및 이어서 완료.
- 1.16.9 / 195 APK와 구현 커밋은 보존됐고 Unity 오류 0 확인. APK 재빌드는 필요하지 않았다.
- 자동 시작 경로의 PHONELOL_TEST_V213.ps1이 PhoneLOL_v1155/APK의 예전 서버 소스와 manifest를 복사하는 것이 반복 되돌림 원인이었다. 관리형 전투 진입점과 account services가 구버전으로 바뀌어 있었다.
- 실행기를 현재 저장소의 관리형 5개 파일/manifest를 사용하도록 고치고, 구형 실행기 자기 덮어쓰기를 제거했다. 상속 의존성과 원본 PhoneLOL_v1155는 보존했다. 관리형 서비스 초기화도 시작 전 검사한다.
- 백업: before-v1169-startup-fix-20260925-001312. 서버 소스/manifest/실행기와 SQLite backup 포함. 기존 계정 DB 초기화/복원 없음.
- 실제 자동 시작 경로로 재시작 성공, 두 번째 실행 시 PID 29608 유지. 관리형 5개 파일과 manifest 해시 일치. 임시 DB의 1인/2인 전투 프로토콜·팀 이동·결과·랭킹·무료 닉네임 검사 통과. 로컬/공개 진단 HTTP 204.
- PC를 다시 재부팅하거나 폰에서 실제 전투를 검사한 것은 아니다. 사용자 폰 테스트 대기.
- 실제 저장소 로컬 커밋 f3ed64a73dabf9e51b8111bb2f5e6a26908f0da3. 상세 재개 원본 RECOVERY_STATUS.md 맨 위 및 Automation/Server/README.md.
- 공개 GitHub push는 이전 자동 승인 거절 상태로 유지하며 이번에도 시도하거나 우회하지 않았다. 허브 원격도 별도 변경이 있으므로 승인 후 fetch/통합하고 게시해야 한다.
- 사용자 UnityConnectSettings.asset 변경은 보존·커밋 제외. 이번 임시 probe/스크립트/검사 로그 정리. 기존 APK/실제 운영 로그/백업과 Remote Desktop 연결 유지.

## 2026-09-25 — [B계정] Nova / Codex Work — 인게임 버그 우선 수정본 1.16.10

- 사용자 우선순위: 인게임 버그 → 전투 기능 → 기타 UI/명칭. 사용자는 모드 전투, 자동 8원, 확장 아이템 슬롯의 실제 작동을 확인했다.
- 이번 단계는 인게임 버그 수정본이다. 라이트맵 double-LDR 해석, 원본 베이크 조명 51개의 중복 실시간 조명 방지, 지형 레이어, 캐릭터 조명 계산을 수정했다.
- 변환 중 누락된 파티클 머테리얼 목록 356개를 원본 식별자로 복원했다. 이펙트 크기 중복 배율과 남은 파티클 셰이더도 수정했다. 원본에 있던 예제용 null 머테리얼 29개는 별도로 확인하고 유지했다.
- 같은 이름의 다른 캐릭터 소리를 선택하던 문제를 수정했다. 원본 오디오 객체 518개를 정확한 바이트로 복구하고 Actor 193개/에셋 167개에 연결했다.
- 스킬 버튼 원본 위치와 70×70 판정 크기는 유지하고 UI 이동 후 물리 좌표를 갱신했다. 버튼 가장자리 포함 72지점 판정 통과.
- 두 맵의 원본 내비게이션 세부 삼각형 2357개와 높이를 사용했다. 0.3 이내 원본 중심점 누락 0, 양쪽 주 이동 경로 연결 확인.
- 에디터 검사: 오디오 518, 머테리얼 셰이더 630, 리소스 파티클 304개 머테리얼 배열, 이펙트 배율, 베이크 조명 상태 통과. 두 맵 렌더링에서 중복 밝기 제거 확인. 실기기 색감/터치감/전체 전투 검증을 의미하지 않는다.
- Android 빌드 중 에디터 전용 Light API 오류를 발견하여 직렬화된 원본 조명 참조로 수정했다. 최종 빌드 0 오류/760 경고, ARM64/IL2CPP, 1.16.10/196.
- APK: `D:\A_KJ\AI\PhoneLoL_02\PhoneLOL-02\Builds\PhoneLOL-v1.16.10-arm64-candidate.apk`. 기존 `00_PHONELOL_TEST_HERE`에도 같은 이름으로 복사하고 해시 일치를 확인했다. 143566248 bytes / SHA-256 `483fa84330eb119e1134a1bdd40f23c19a6df4b2aca476dcffbe46d35cba58e7`. native library 6개 전부 ARM64/ELF64.
- 구현 로컬 커밋 `0a3529d55fe6af4cf88497a9d317aadbd3b57e9e`. 상세 원본은 RECOVERY_STATUS.md 맨 위, Recovery/V11610*. 기존 서버/계정 DB는 변경하지 않았다.
- 아직 미포함: 넥서스 공격, 친선 모드 진입·5대5·모드3 규칙, 명칭, 전체 상태창 슬롯, 머리 위 글씨 제거, 선택 사항인 스폰 알리스타. 위 우선순위대로 후속 작업한다.
- 기존 공개 GitHub 업로드 승인 거절을 우회하지 않았다. 소스/허브는 로컬 저장. 사용자 UnityConnectSettings.asset 변경은 보존하고 커밋에서 제외했다.

## 2026-09-25 — [B계정] Nova / Codex Work — 1.7.0 Android 전달, iOS 차단 사유 기록

- 사용자가 버전 표기를 1.7.0 → 1.7.1 형식으로 정정했다. 이번 APK는 1.7.0/197이며 원본 저장소 로컬 구현 커밋은 7ad052c다.
- 이전 이펙트·소리·콜리전·라이트맵은 사용자가 정상으로 확인했다. 오른쪽 조작부 전체를 기존 오른쪽 기준점에서 6% 확대했다. 같은 폰 원본 스크린샷에 대한 정확한 복원이라고 주장하지 않는다.
- 넥서스에 기존 포탑 탄환과 공격 수치를 연결했다. 각 팀 스폰 뒤 2유닛에 같은 팀 알리스타를 생성하며 체력100000, 즉시 같은 위치 부활, 비참가자/상태창 제외, 공격·이동 동작 없음으로 구현했다.
- 모드1/모드2/모드3 명칭, 모드2·3 최대5대5, 모드3 기존 모드 전투 규칙, 10자리 방/상태/결과창, 확장8슬롯 표시, 캐릭터 위 지속 글씨 제거를 구현했다.
- 별도 테스트 DB에서 2/10/10명 입장·선택·시작·로딩·결과 전송과 5/8아이템 데이터를 확인했다. 서버의 별도4명 제한도 수정했다. 한글 닉네임 변경 후 재접속과 서버 재시작에도 프로필에 유지됐다. 실제 계정 이름/잔액을 테스트로 변경하지 않았다.
- Unity 실제 패킷 해석기로10명 데이터를 읽고 두 맵 UI 참조와 확대된 버튼 가장자리 판정을 확인했다. 알리스타 체력/같은 위치 즉시 부활3회는 분리된 상태 검사로 통과했다. 넥서스 실전 탄환 명중·알리스타 실제 맵 소환·폰 UI 체감은 아직 사용자 확인 대상이다.
- Android 빌드: 오류0/경고761. ARM64 ELF64 라이브러리6개, 16KB LOAD 정렬 및 APK zipalign 검사 통과. minSDK25/targetSDK36. 파일143232958바이트, SHA-256 b1a29fd0c96cd830fa545bf00615843e4a58452877c75cd86a7e751513456231.
- 전달: C:/Users/user/Documents/MultiGod/PhoneLOL_LocalRuntime/00_PHONELOL_TEST_HERE/PhoneLOL-v1.7.0-arm64-candidate.apk. 프로젝트 Builds에도 동일 파일을 보존했다.
- 서버는 기존 SQLite/소스를 before-v170-20260925-104451에 백업한 뒤 기존 시작 경로로 배포했다. 관리형 소스/manifest 일치, 로컬·공개 진단HTTP204 확인. 확장 방 참가자는 모두 새 클라이언트를 사용해야 한다.
- iOS는 미완료다. 공식 지원 모듈 설치가 Windows 관리자 승격 창 취소/시간 초과(ELEVATION_CANCELLED)로 실패했다. 연결된 Mac/Xcode/서명 환경도 없다. 컴파일되는 iOS 내보내기 명령은 준비했고, 실제 Xcode/IPA는 생성하지 않았다. 재개 절차는 Recovery/V170IOSStatus.md.
- 새 독립 도구 설치 없음. 기존 D:/A_KJ/AI/PhoneLoL_02, 사용자 Unity/Python/서버 경로 유지(기존 위치 예외). 이번 중복 빌드 폴더, 임시 probe/스크립트/로그, 실패한 iOS 설치 다운로드를 정리했다. APK·원본·운영 백업·Remote Desktop 연결은 보존했다.
- 사용자 UnityConnectSettings.asset 별도 변경을 보존하고 커밋에서 제외했다. 이전 자동 승인 심사가 거절한 공개 GitHub 업로드는 재시도하거나 우회하지 않았다. 소스와 허브 기록은 로컬 저장 상태다.
- 최신 원본: 실제 저장소 RECOVERY_STATUS.md 상단. 다음은 1.7.0 폰 피드백 및 iOS 모듈 승인/Mac 빌드 환경 확보다.

## 2026-09-25 — [B계정] Nova / Codex Work — iOS 내보내기 및 통합 ZIP

- iOS Build Support6000.3.14f1을 공식 설치로 완료했다. 기존 Unity 편집기 내부 설치로 기본 도구 루트의 예외를 설치 목록에 기록했다.
- 설치 직후 열린 편집기는 iOS 후처리 오류2개에도 성공으로 보고했고 실제 프로젝트는 없었다. 출력 파일과 오류 수를 확인하는 빌드 검증을 추가했다.
- 재실행 중 Package Manager Retry 창은 원격 실행 환경에서 누락된 Windows 환경변수 때문이었다. 기존 Automation/OpenUnity.ps1로 정상 실행했고 Editor 준비 완료를 확인했다. 보안/방화벽 설정은 변경하지 않았다.
- 실제 iOS Xcode 내보내기 성공: 오류0/경고35, 40500ms. Info.plist 및 Xcode 프로젝트, 1.7.0/197, ARM64/Metal/가로 화면 확인. 후속 C# 재컴파일 오류0.
- 사용자 요청에 따라 버전 뒤 문구를 제거했다. 기존 테스트 폴더의 PhoneLOL-1.7.0.zip에는 Android/PhoneLOL-1.7.0.apk와 iOS/PhoneLOL-1.7.0/ 프로젝트 및 한국어 안내가 들어 있다.
- ZIP648469575바이트, SHA-256 d0307a14902138020384cf92f72eaa0b7d7f9e111bdc0b116ecf9c3014750522. 압축 전체 CRC 검사와 내부 APK 원본 해시 일치 검증 통과. Mac 실행 스크립트 실행 권한 보존.
- Mac 연결이 없어 Xcode 컴파일/링크, Apple 서명, IPA 생성, 아이폰 실행은 미완료다. 현재 ZIP을 iOS 설치 앱으로 설명하지 않는다. Mac과 사용자 서명 환경이 다음 필요사항이다.
- 원본 저장소 로컬 커밋: 2136213a576b1b440ccb098ac8156300bc63b05c. 최신 근거: RECOVERY_STATUS.md, Recovery/V170IOSStatus.md, V170IOSExport.json/txt.
- 라이브 서버/계정 DB는 변경하지 않았다. 사용자 UnityConnectSettings.asset 변경은 보존·커밋 제외. 공개 업로드 자동 승인 거절을 우회하지 않았다.
- 이번 설치 캐시와 임시 진단 스크립트/로그는 정리한다. 새로 만든 별도 iOS ZIP은 통합 ZIP 검증 후 제거했고, 원본/운영 백업/최종 산출물과 Remote Desktop 연결은 유지한다.

## 2026-09-25 — Codex Work — 1.7.1 전달 후 알리스타 1.7.2 수정 및 빌드 요청

- 1.7.1 APK/ZIP은 이전 작업 중 바탕화면에 전달했고 사용자가 확인했다. 이후 요청으로 1.7.2는 바탕화면 복사 없이 빌드 명령만 내리고 종료한다.
- 알리스타가 같은 팀으로 필터링되고 호스트 피해 권한도 없던 문제를 수정했다. 중립 정글몹 판정/양 팀 공격 가능/scene-object 권한/재접속 생성 경로를 반영했다.
- 최신 수치: 체력20000, 처치한 챔피언에1000골드, 즉시 제자리 부활. 아래 스폰 오른쪽28, 위 스폰 왼쪽28로 대칭 배치; 직선맵은 중앙 길과도 분리했다.
- 실제 OnAttackDamage로20000에서19959.39로 감소, 양 팀 선택,3회 처치 각각 정확히1000골드와 체력20000 부활 검증 통과.
- 챔피언 옆 레벨은 예전 전체 머리 위 글씨 제거 요청으로 꺼져 있음을 확인했다. 이번 요청은 확인만이므로 복원하지 않았다.
- 1.7.2/199 빌드 명령을 최종 실행한다. 완료 여부는 Recovery/V172BuildJob.json 및 Builds/build-result.txt로 확인해야 하며 완료를 주장하지 않는다.
- 소스 로컬 커밋 8b6c6fb. 최신 원본 RECOVERY_STATUS.md. 서버/계정DB 변경과 공개 업로드 없음. 사용자 UnityConnectSettings.asset 수정 보존.
