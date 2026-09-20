# T-0009 — AI Control Tower

## 2026-09-20 KST — 구현 및 인수인계

### 목적
- Windows 로컬 AI 도구의 실제 상태를 한 화면에서 확인하고 Jev 작업을 제어하는 .NET 9 WPF 앱을 구현한다.

### 구현
- 위치: `ai-control-tower/`
- 빌드: .NET SDK 9.0.306, `net9.0-windows`
- 배포: `ai-control-tower/artifacts/win-x64/AIControlTower.exe`
- 상태 확인: Desktop Commander, Jev, Codex, GitHub CLI, n8n, AI Ops Runner, GPT→Jev 전달 체인
- 보안: API 키·토큰·비밀번호 값은 저장·로그·UI에 표시하지 않는다.
- Desktop Commander: 기존 표시형 Startup CMD는 설치 시 백업한 뒤 비활성화하고 숨김 VBS 런처로 대체한다. 다른 시작 항목은 건드리지 않는다.

### 검증
- 상태 모델·명령 실패 안전 처리·작업 상태·시작 파일 백업 단위 테스트 6건 통과
- Release 빌드 경고 0개, 오류 0개
- 초기 publish EXE 실제 실행 프로세스 확인: `AIControlTower` PID 32936

### 실제 외부 도구 상태 관찰
- Desktop Commander: 자동 연결 구성은 있으나 전용 프로세스는 실행 중이 아님
- Jev: `jev-router@0.2.0` 및 진입점 존재, PowerShell 실행 정책으로 래퍼 검증 실패 가능
- Codex: 설치됨, 로그인 상태는 확인되지 않음
- GitHub CLI: 설치됨, 현재 토큰 유효성 확인 실패
- n8n: 설치/구성 흔적은 있으나 로컬 서비스 실행 증거 없음
- AI Ops Runner: Actions Runner 서비스 실행 관찰

### 다음 확인 사항
- 최종 publish 후 최신 EXE를 다시 실행해 UI 상태 갱신을 확인한다.
- 앱의 설치 버튼은 사용자가 명시적으로 누를 때만 시작 레지스트리와 Desktop Commander 시작 파일을 변경한다.

### 후속 개선
- 상태 표의 셀·행·헤더 색을 명시해 흰 바탕/흰 글씨 문제를 수정했다.
- %LocalAppData%\AIControlTower\queue\inbox의 작업당 TXT 파일을 processing으로 원자 이동해 Jev 작업으로 연결하는 큐를 추가했다.


### 실제 설치·시작 전환 검증 (2026-09-20)
- 사용자 설치 버튼 실행 결과: %LocalAppData%\AIControlTower 설치 확인.
- Startup의 AIControlTower-DesktopCommanderSilent.vbs 확인.
- 기존 DesktopCommanderRemote.cmd 백업 DesktopCommanderRemote.cmd.20260920114518.bak 확인.
- 설치 재실행 시 원본 CMD가 이미 없어 ‘기존 시작 파일을 찾지 못함’ 메시지가 나올 수 있으나, 이는 최초 전환이 이미 성공했다는 상태다.

### 디자인 개선 및 로컬 Jev 실행 차단 (2026-09-20)
- 바탕화면 `AA_01.png`, `AA_02.png`를 참고해 기능·이벤트 바인딩은 유지하면서 화이트·블루 대시보드, 요약 카드, 상태 배지, 다크 실시간 작업 콘솔로 화면을 개선했다.
- 사용자 OpenAI/ChatGPT 계정 사용 방침을 구현에 반영했다. `LocalExecutionPolicy`가 기본적으로 false이며, 화면의 Jev 실행 버튼·직접 실행 경로·TXT 큐 수신·Jev 상태 명령 실행을 모두 차단한다.
- TDD: 직접 실행 차단 테스트를 먼저 실패시킨 뒤 수정했고, 현재 9개 단위 테스트가 통과한다.
- 기존 `artifacts\\win-x64\\AIControlTower.exe`는 사용자 실행 프로세스가 점유 중이라 덮어쓰지 않았다. 새 디자인 미리보기는 `ai-control-tower\\artifacts\\win-x64-design-preview\\AIControlTower.exe`에 생성·실행했다.

