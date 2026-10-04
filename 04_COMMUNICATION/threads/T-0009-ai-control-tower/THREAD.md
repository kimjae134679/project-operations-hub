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
- GitHub `main` 반영 뒤 탐색 구현 커밋: `400e7d5` (`feat: add control tower navigation`). 고정 좌측 메뉴와 고정 헤더, 하나의 `DashboardScroll` 안의 요약·도구 상태·실시간 콘솔·설치/복구·보안 안내를 구성했다. 상태 표 최소 높이는 280px, 콘솔 최소 높이는 190px이다.
- Jev 입력·실행 편집기는 메인 대시보드에서 제거했고, 메뉴와 요약 카드가 단일 인스턴스의 별도 Jev 작업 제어 창을 연다. `LocalExecutionPolicy`의 기본 false 및 직접 실행·큐 수신·상태 명령 차단은 변경하지 않았다.
- 후속 소규모 정리: 더 이상 XAML에 연결되지 않는 `MainWindow` Jev 실행/취소 핸들러를 제거했고, 고정된 활성 메뉴 표시를 없애 이동 대상과 다른 활성 상태가 보이지 않게 했으며, `CONTROL/TOWER`, `LIVE STATUS` 레이블을 한국어로 바꿨다.
- 최종 검증: Release 단위 테스트 11개 통과, Release solution build 경고 0·오류 0, 단일 파일 publish 성공. `artifacts\\win-x64\\AIControlTower.exe`와 `%LocalAppData%\\AIControlTower\\AIControlTower.exe`의 SHA-256은 `89A4B961CA2F92EFBE53B1B872E18F843BB6478BDB418071BDA2A4B7DBCD4C3B`로 동일하며 설치본 실행 프로세스 PID `29428`를 확인했다.
- CUA에는 native app surface가 없어 자동 화면 캡처를 수행할 수 없었다. 따라서 좌측 메뉴·단일 전체 스크롤·별도 Jev 창·비활성 실행 버튼·금지된 GPT 스케줄러/예약/브라우저/권한 자동 클릭 UI 부재라는 화면 모양은 사용자 직접 확인이 남아 있다.

### 추가 AA_01 확인 및 최종 코드 검토 (2026-09-20)
- 바탕화면 `AA_01.png`의 생성 시각은 22:28:45이며, 당시 로컬에서 별도 Jev 창 커밋(22:45, GitHub 반영 뒤 `02ea6da`)과 좌측 탐색·전체 스크롤 커밋(23:01, GitHub 반영 뒤 `400e7d5`)보다 앞선다. 따라서 이 이미지는 최신 설치본이 아니라 수정 전 UI 기록이다.
- 설치본 `%LocalAppData%\AIControlTower\AIControlTower.exe`는 23:21에 교체되었고 PID `29428`은 23:23에 시작했다. 자동화 세션에서 기존 창 활성화를 시도했으나 다른 데스크톱 세션 때문에 활성화되지 않아 화면 모양을 대신 확인할 수는 없었다.
- 최종 독립 코드 리뷰에서 Critical 및 코드상 배포 차단 Important 이슈는 없었다. 후속 권장 사항은 키보드 포커스 시각화와 Jev 입력 레이블의 접근성 연결이다.
- 최종 재검증 결과 Release 단위 테스트 11/11 통과, Release solution build 경고 0·오류 0이다.
- 사용자 확인은 23:21 이후 설치본에서 좌측 메뉴, 하나의 전체 스크롤, 별도 Jev 창, 비활성 실행 버튼과 정책 안내를 보는 것으로 한다.


## 2026-10-05 KST — 통합 작업실과 도구 활용 기준 개편

작성자: Codex · 사용자 요청 범위: 통합소통방 관리, 용도별 도구 추천, 프로젝트/기능/프로그램 통합 제어, 공유 원격 연결 정리, 참고 사이트 기반 디자인과 실제 UI QA.

- 정책과 사용자 요약에 서브에이전트·Plugin·CLI·Skill·검색·검사기·화면 검증·미디어 배치·Jev 선택 기준을 반영했습니다. 과용 시 총 토큰·대기·재작업이 늘 수 있으며, 과거 후보는 현재 공식 문서와 더 나은 대안으로 재검토합니다.
- 사용자 최신 Jev 사용 허가와 D드라이브 지시를 반영했습니다. 실제 관리 루트는 `D:\A_KJ\AI`입니다. 설치·키 존재·로그인과 실제 라우팅 왕복을 구분합니다. 추가 결제는 승인되지 않았습니다.
- 폴더 발견과 `project.control.json`으로 프로젝트 → 기능 → 프로그램을 연결하고, 명령 선택·실행·중지·지속 결과 로그를 제공합니다. 기존 게임 저장소는 수정하거나 이동하지 않았습니다.
- Desktop Commander 시작 등록을 공유 중계 하나로 통합했습니다. 현재 연결 프로세스는 종료하지 않았고 도구 연결 Online을 확인했습니다. Node 자식 프로세스 여러 개를 중복 세션으로 오인하지 않습니다.
- 처음 배포 과정에서 WPF 네이티브 라이브러리 누락과 흰 배경/옅은 글자 결함이 실제로 발견됐습니다. 단일 EXE 추출 옵션과 명시적인 Window 테마 적용으로 수정했습니다. 작은 창에서의 버튼 잘림도 실제 캡처 뒤 수정했습니다.
- shadcn sidebar 예제, HyperUI 목록, daisyUI 상태, Uiverse 버튼을 확인해 탐색/선택/행동 배치를 다시 정리했습니다. 반복 제목·영문 라벨·안내를 줄이고 검색, 접는 실행 기록, 한국어 상태, 앱 아이콘을 추가했습니다.
- 빌드·실제 창 캡처·작은 창의 실행 버튼 경계·설치본 해시·검증 범위는 [최종 검증 기록](../../../ai-control-tower/docs/VERIFICATION-20261005.md)에 기록합니다. 실제 Jev 작업 왕복, 재로그인/재부팅 후 원격 재연결, 개별 프로젝트의 특수 실행 어댑터는 별도 미검증입니다.


## 2026-10-05 KST — 사람용 프로젝트 화면으로 전면 개편

최신 사용자 지시에 따라 개발자용 영문·폴더 나열 화면을 폐기했습니다. 실제 폴더와 안내 문서에 근거해 6개 프로젝트, 24개 프로그램·자료로 정리하고 현재·이전·실험·복구 자료를 구분했습니다. 기존 폴더는 이동·삭제하지 않았습니다.

- 기본 화면은 한국어 프로젝트 이름·하는 일·실제 행동이며, 경로·개발 점검·작업 기록은 필요할 때 펼칩니다. APK는 휴대폰 설치파일 위치 보기, 안내는 읽기, 제작 프로그램은 시작·열기로 구분합니다.
- 현재 기본 테마는 따뜻한 회백색·흰 내용 영역·남색 본문·파란 행동입니다. 앞선 어두운 작업실은 최종 디자인이 아닙니다. Notion·Linear·Things와 사용자 참고 라이브러리 비교는 디자인 문서에 기록했습니다.
- 후속 AI는 [프로젝트 표현 기준](../../../01_CONTROL/PROJECT_PRESENTATION_RULES.md), [폴더 안내](../../../000_사용자용/11_프로젝트와_폴더_안내.md), [도구 선택 기준](../../../03_KNOWLEDGE/CANDIDATE_CATALOG/TOOL_SELECTION_PLAYBOOK.md)을 먼저 읽습니다. 과거 조사만으로 도구·프로젝트 용도·실행 성공을 확정하지 않습니다.
- Windows Release 테스트 36개 통과. 실제 WPF 화면에서 검색과 좁은 창·실행 버튼을 확인했습니다. 설치본, 캡처와 미검증 범위는 [최종 검증 기록](../../../ai-control-tower/docs/VERIFICATION-20261005.md)에 있습니다.

최종 대조에서 Unity의 실제 비표준 설치 경로를 확인했습니다. 초기 표준 경로 조사만으로 편집기 연결을 미확인으로 둔 설명을 정정했습니다. 기존 OpenUnity.ps1로 게임 편집기를 열고 같은 프로젝트를 재사용하도록 연결했으며, 편집기 수명은 관제탑 종료와 분리했습니다. 실제 Unity 열기는 검증 중 실행하지 않았습니다.
