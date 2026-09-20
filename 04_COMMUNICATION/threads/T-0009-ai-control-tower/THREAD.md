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

