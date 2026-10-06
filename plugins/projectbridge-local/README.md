# ProjectBridge PC Control

현재 상태: 개발·격리 검증 완료. 실제 D: 설치와 ChatGPT 플러그인 연결은 아직 실행되지 않았습니다.

지원: 파일 읽기·새 파일 생성·기존 파일 수정·목록·폴더 생성·파일 이동·복구 가능한 삭제·복구·명령 실행·독립 프로세스 시작 및 상태 조회. 수정/이동/삭제는 읽은 파일 SHA256을 전달합니다. 삭제 결과의 trashId와 SHA256으로 같은 프로젝트/호출자에서 복구합니다. 기존 파일 덮어쓰기, 재귀 영구 삭제, 다른 호출자 작업 강제 종료는 제공하지 않습니다.

배포 설치 스크립트 `scripts/install-projectbridge-plugin.ps1`에 배포 커밋과 release_manifest SHA256을 전달하면 원격 수정본을 사전 검증해 설치하고 로그인 시작을 등록한 뒤, 로컬 MCP 플러그인을 Codex 마켓플레이스에 설치합니다. 기존 Windows 계정 권한으로 실행합니다. 관리자 권한, ACL, ChatGPT 승인 정책, 샌드박스는 변경하지 않습니다. 중간 실패 시 오류와 설치 단계가 남고 수정된 원격을 제거하지 않습니다.

설치 후 새 대화에서 `pc_status`, `pc_submit`, `pc_result`가 실제 제공되는지 확인해야 합니다. Codex의 로컬 플러그인과 ChatGPT 웹의 custom MCP 연결은 별도입니다. 웹은 공식 Secure MCP Tunnel 연결이 추가로 필요하며, 터널이나 복구용 두 번째 원격은 이 패키지에 설치됐다고 주장하지 않습니다.

세 도구: pc_status는 연결 상태, pc_submit은 action/args/projectId/toolId 작업 제출, pc_result는 작업 ID 결과 조회입니다. 완료 판단은 result.outcome이며, 명령은 data.succeeded와 stdout까지 확인합니다. 온라인 상태만으로 작동 판정을 하지 않습니다.

제약: 이동/삭제/복구는 검증된 복사 후 원본 제거로 처리해 다른 디스크를 지원하지만 원자적 이동은 아닙니다. 외부 프로그램의 동시 파일 변경은 충돌로 실패할 수 있습니다. 영구 기록은 로컬 state 및 승인된 비공개 릴레이에 저장하며 파일 내용·토큰은 공개 저장소에 올리지 않습니다. Desktop, 보호 설정 및 sources 참조 자료는 변경 보호합니다.
