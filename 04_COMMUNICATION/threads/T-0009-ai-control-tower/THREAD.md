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

### 탐색 화면 인수인계 (2026-09-20)
- 소스 기준 최신 커밋: `a89513b` (`feat: add control tower navigation`). 고정 좌측 메뉴와 고정 헤더, 하나의 `DashboardScroll` 안의 요약·도구 상태·실시간 콘솔·설치/복구·보안 안내를 구성했다. 상태 표 최소 높이는 280px, 콘솔 최소 높이는 190px이다.
- Jev 입력·실행 편집기는 메인 대시보드에서 제거했고, 메뉴와 요약 카드가 단일 인스턴스의 별도 Jev 작업 제어 창을 연다. `LocalExecutionPolicy`의 기본 false 및 직접 실행·큐 수신·상태 명령 차단은 변경하지 않았다.
- 후속 소규모 정리: 더 이상 XAML에 연결되지 않는 `MainWindow` Jev 실행/취소 핸들러를 제거했고, 고정된 활성 메뉴 표시를 없애 이동 대상과 다른 활성 상태가 보이지 않게 했으며, `CONTROL/TOWER`, `LIVE STATUS` 레이블을 한국어로 바꿨다.
- 소스 검증 기록: Release 단위 테스트 11개 통과, Release solution build 경고 0·오류 0, `git diff --check` 통과(Task 3). 이번 문서·정리 변경은 publish 전 소스 검증을 다시 수행해야 한다.
- **외부 권한 작업 잔여:** 최신 단일 파일 publish, `%LocalAppData%\\AIControlTower\\AIControlTower.exe` 설치본 교체, artifact/설치본 SHA-256 일치 대조, 설치본 수동 UI 확인, `git pull --rebase` 및 GitHub push. 이들이 끝나기 전 최신 소스가 설치본/원격 main에 반영됐다고 기록하지 않는다.

