ProjectBridge PC Control 3.0.1

현재 원격 프로그램 설치는 완료됐습니다. 이 ZIP은 플러그인만 등록합니다.

1. ZIP을 프로그램 폴더에 압축 해제합니다. 예: D:\A_KJ\AI\Applications\ProjectBridge\LocalPlugin
2. 압축 해제 폴더 안 INSTALL.cmd를 직접 실행합니다.
3. 성공 메시지 후 새 Codex 대화를 시작합니다.
4. pc_status / pc_submit / pc_result 도구가 실제 나타나는지 확인합니다.

INSTALL.cmd는 npm으로 이미 설치된 Codex 실행 파일을 사용하며, 해당 실행 파일이 없으면 codex.cmd를 찾습니다. 원격 재설치, 관리자 권한, ACL, 방화벽, 승인 정책 및 샌드박스를 변경하지 않습니다. 사용자가 직접 여는 등록 창만 나타납니다.

검증: 실제 설치된 Codex 0.160.1로 별도 임시 설정에서 marketplace add, plugin add, plugin list를 실행했고 installed=true, enabled=true를 확인했습니다. 사용자 본 계정 등록과 새 대화에서의 도구 제공은 이 ZIP을 실행한 뒤 확인해야 합니다.

주의: .agents 폴더가 포함되어야 합니다. 올바른 마켓플레이스 경로는 .agents/plugins/marketplace.json입니다. 이전 .codex-plugin/marketplace.json만으로는 등록되지 않습니다.

직접 명령으로 등록하려면:
codex plugin marketplace add "압축 해제한 폴더의 전체 경로"
codex plugin add projectbridge-local@projectbridge-personal

ChatGPT 웹의 'Add custom MCP server'에 URL이나 Tunnel ID를 넣는 화면은 이 로컬 ZIP 설치와 다릅니다. 그 화면에는 ZIP/JSON을 직접 업로드해서 연결할 수 없습니다. 이 ZIP은 Windows 로컬 Codex 플러그인용입니다.
