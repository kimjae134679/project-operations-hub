# AI Control Tower 설계

## 목표

Windows에서 Desktop Commander Remote, Jev, Codex 및 관련 운영 도구의 실제 상태를 한 화면에서 확인하고 Jev 작업을 실행·취소할 수 있는 .NET 9 WPF 데스크톱 앱을 제공한다. 배포물은 `win-x64` 단일 실행 파일이며 설치·제거·복구를 지원한다.

## 범위와 원칙

- 상태를 확인할 수 없으면 `Unknown`으로 표시하며 추측한 성공 상태를 만들지 않는다.
- 비밀번호, API 키, 토큰, 환경 변수 값은 UI·앱 로그·인수인계 문서에 저장하거나 노출하지 않는다.
- Desktop Commander 및 상태 확인용 PowerShell/CMD는 숨김으로 실행한다. Jev 작업 콘솔만 사용자가 볼 수 있다.
- 각 도구는 현재 PC의 실제 파일·프로세스·명령 결과만 사용해 상태를 판단한다.
- 기존 파일은 필요한 범위로만 수정한다. 시작 파일을 바꿀 때에는 먼저 백업을 만든다.

## 구조

`AIControlTower` WPF 앱을 단일 프로젝트로 만들고, MVVM 형태로 화면과 운영 로직을 분리한다.

| 구성 | 책임 |
| --- | --- |
| `MainWindow` / XAML | 대시보드, 작업 입력, 실행·취소, 설치·제거·복구 UI |
| `MainViewModel` | 5초 타이머, 명령 상태, 화면 상태 집계 |
| `IStatusProvider` | 개별 도구의 상태 조회 계약 |
| 상태 공급자 | Desktop Commander, Jev, Codex, GitHub CLI, n8n, AI Ops Runner, 전달 체인, 현재 Git 작업 조회 |
| `ProcessRunner` | 창을 숨길 프로세스와 Jev의 표시 콘솔을 안전하게 분리 실행 |
| `InstallationService` | 설치 경로 선택, 시작 프로그램 등록, 제거·복구, 원본 시작 파일 백업 |
| `ActivityStore` | 실행 중인 Jev 작업의 프로젝트·내용·브랜치·단계·시작 시간을 메모리에 보관 |

## 상태 판단과 데이터 흐름

앱 시작 후 즉시 모든 상태 공급자를 병렬 조회하고, 이후 5초마다 다시 조회한다. 공급자는 프로세스 존재, 명령 유무, 구성 파일 존재, 안전한 명령 결과를 조합해 `Ready`, `Running`, `NotConfigured`, `NotInstalled`, `Unknown`, `Error` 등의 상태를 반환한다.

Desktop Commander는 설정 값 자체가 아닌 설정 파일/환경 변수의 **존재 여부**만 확인한다. Jev도 API 키의 값 대신 설정 여부만 표시한다. Codex 및 GitHub CLI 로그인 확인은 해당 CLI의 비대화형 상태 명령을 사용하며, 예상하지 못한 출력이나 오류는 `Unknown` 또는 `Error`로 바꾼다.

GPT → Desktop Commander → Jev → Codex 및 GPT → Jev 전달 상태는 실제 프로세스·전달 증적을 확인할 수 있는 구간만 표시한다. GPT가 외부 ChatGPT 세션인 구간처럼 로컬에서 입증할 수 없는 항목은 `Unknown`으로 남긴다.

## Jev 작업 실행

사용자는 작업 텍스트와 프로젝트 경로를 입력한다. 실행 시 앱은 Jev 런처가 설치됐고 API 키가 설정됐는지 먼저 확인한다. 충족되지 않으면 명확한 오류 상태를 보여 주고 작업을 시작하지 않는다. 충족되면 Jev 콘솔을 표시해 실행하고, `ActivityStore`가 시작 시간과 현재 Git 브랜치를 추적한다. 취소는 실행한 프로세스 트리만 대상으로 하며 Desktop Commander 서비스나 사용자가 열어 둔 다른 터미널은 종료하지 않는다.

## 설치·시작·복구

설치는 우선 `C:\Program Files\_My\AI\Applications\AIControlTower`에 시도하고 권한 오류인 경우 `%LocalAppData%\AIControlTower`로 대체한다. 설치 프로그램은 포터블 EXE와 유지보수 스크립트를 복사하고 Windows 시작 레지스트리에 관제탑 자동 실행을 등록한다.

DesktopCommanderRemote의 표시형 기존 시작 파일은 발견 시 날짜가 붙은 백업을 만들고, 원본 실행을 끄되 숨김 방식의 백그라운드 시작으로 대체할 수 있도록 복구 정보를 유지한다. 제거는 관제탑의 시작 등록과 설치본만 제거한다. 복구는 백업한 시작 파일과 이전 자동 실행 구성을 되돌린다.

## 검증

- .NET 9 Release 빌드와 `win-x64` 단일 파일 publish
- 정적 코드 검사 및 빌드 성공
- 포터블 EXE의 실제 시작 확인
- 설치, LocalAppData 대체 로직, 제거, 복구의 안전한 경로 검증
- 상태 조회 실패가 `Unknown`으로 표시되는지 확인
- 비밀값이 로그 및 화면 텍스트에 포함되지 않는지 확인

## GitHub 인수인계

완료 시 README와 운영 인수인계 문서를 추가하거나 갱신해 설치 위치, 생성한 EXE, 검증 근거, 알려진 한계, 복구 절차를 기록한다. 최신 사용자 지시에 따라 관련 변경을 커밋하고 원격 GitHub 저장소에 push한다.
