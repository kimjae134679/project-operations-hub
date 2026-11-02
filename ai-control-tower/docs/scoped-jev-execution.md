# Jev 독립 작업 폴더 실행

## 기본 동작과 명시적 선택

- 기존 프로그램 명령과 기본 Jev 실행은 **프로젝트당 소유 작업 1개**를 유지합니다. 서로 다른 폴더라는 이유만으로 일반 명령의 프로젝트 잠금을 해제하지 않습니다.
- AI 작업 전달 창의 **독립 작업 폴더** 체크박스는 기본 꺼짐이며 설정 파일에 저장하지 않습니다. 선택 프로그램이 바뀌면 다시 꺼집니다.
- 사용자가 이 옵션을 켠 Jev 작업만 선택한 등록 프로그램의 `WorkingDirectory`에서 독립 실행합니다. 창에는 선택 프로그램 이름과 실제 실행 폴더를 표시합니다.
- 선택 프로그램은 해당 프로젝트 목록의 실제 객체여야 하며, 등록 명령이 있어야 합니다(`CanLaunch`). 발견한 파일·명령 미등록 폴더나 임의로 입력한 프로젝트 경로를 독립 작업으로 추정하지 않습니다.
- 독립 폴더는 등록 프로젝트 루트 안에 이미 있는 엄격한 하위 폴더여야 합니다. 프로젝트 루트 자체, 외부·없는 폴더, reparse/link 경로는 거절합니다. 실행 슬롯을 기다린 뒤에도 시작 직전에 다시 검사합니다. 작업 폴더·Git worktree·실제 프로젝트 manifest를 자동 생성하거나 변경하지 않습니다.

## 동시 실행과 소유권

- 한 `JobManager` 인스턴스의 실제 실행 슬롯은 기존과 같은 **최대 3개**입니다. 이 한도는 모든 프로젝트의 로컬 소유 작업을 합친 값이며 PC 전체 외부 프로세스 한도가 아닙니다.
- 같은 프로젝트의 명시적 독립 Jev 작업은 서로 다른 실행 ID와 겹치지 않는 작업 폴더일 때 함께 실행할 수 있습니다. 프로젝트 독점 작업이 있으면 독립 작업도 거절하며, 반대 방향도 같습니다.
- 폴더 동일성·상위/하위 관계를 대소문자와 끝 구분자를 정규화해 비교합니다. 다른 프로젝트라고 해도 겹치는 작업 폴더는 거절합니다. `a`와 `ab` 같은 단순 문자열 접두어는 충돌로 오인하지 않습니다.
- 실행 ID 중복과 폴더 충돌 검사·예약은 하나의 잠금 안에서 처리합니다. 슬롯 대기 중인 작업도 예약을 유지합니다. `BusyProjectCount`는 작업 폴더 수가 아닌 고유 프로젝트 수입니다.
- 독립 Jev ID는 프로젝트 ID·등록 프로그램 ID·정규화한 작업 폴더의 해시에서 생성합니다. 같은 선택의 실행·중지는 같은 resolver를 사용하고 정확한 소유 ID만 중지합니다. 별도 run ID와 기존 런처의 PID별 runtime은 호출마다 분리됩니다.
- 예약은 자식 프로세스 수명 관리와 AI 의미적 완료 검사 및 결과/receipt 저장이 끝난 뒤 해제합니다. `exit 0`만으로 AI 업무 완료로 바꾸지 않습니다.

**이 옵션은 작업 범위 선언이지 파일시스템 sandbox가 아닙니다.** 서로 다른 cwd의 프로그램도 절대 경로나 상위 경로로 같은 파일을 수정할 수 있습니다. 그런 공유 파일·Git 상태·외부 자원은 별도로 작업 담당자를 조정해야 하며 이 변경이 자동으로 감지하거나 격리한다고 주장하지 않습니다. 공유 프로젝트 루트 도구는 읽기 전용처럼 보여도 별도 권한/자원 선언이 없으므로 기본 독점 실행을 유지합니다. 새 actor/session orchestration 시스템도 아닙니다.

## 실제 검증과 운영 경계 · 2026-10-07

- 신규 `ScopedJevExecutionTests` **16개 통과**. D 전용 fixture에서 두 개의 실제 소유 PowerShell 프로세스가 동시에 살아 있음을 확인하고, 하나만 중지한 뒤 다른 프로세스가 계속 살아 있는 것을 확인했습니다. 모델 호출·Jev proxy 호출 없이 소유 실행기의 병렬 동작을 검증한 것입니다.
- 기본 공유 프로젝트 거절, 같은 실행 ID·동일/하위/case 변형 폴더·프로젝트 간 충돌, 3슬롯 및 대기 예약, AI 실패 receipt 저장, 등록 workspace resolver, 알려지지 않은 scoped ID 분류 차단, 기본 꺼짐/XAML 바인딩을 검사했습니다. drive/UNC 루트 및 경계 접두어 비교도 포함합니다.
- 주 세션의 fresh 전체 .NET 코드 전용 검사: **602개 통과 / 실패 0, exit 0**. API 호환 검사도 기존 인수 이름·형식·순서·기본값을 유지하고 마지막 선택 인수 `independentWorkspaceRoot = null`만 추가했는지 확인합니다.
- 이 결과는 실제 사용자 창의 조작·복수 실제 Jev 모델 작업·복수 AI의 공동 업무 결과를 검증한 것이 아닙니다. 기존 단일 Jev 응답 확인도 복수 세션 실사용의 증거로 확대하지 않습니다.
- 기존 ProjectBridge 연결과 설치를 이 변경 때문에 중지하거나 업그레이드하지 않았습니다. Bridge 업그레이드/배포 검증은 보류이며, 최신 Bridge 소스의 장기 작업 lifecycle 수정과 실제 설치본은 별도로 대조해야 합니다. 이 로컬 변경은 설치된 Bridge의 acceptance-as-completion 문제를 고쳤다는 뜻이 아닙니다.

## 관련 소스와 검사

- `src/AIControlTower/Services/JobManager.cs` — 기본 독점 및 명시 workspace 예약, 3슬롯, 소유 중지/receipt 후 해제.
- `src/AIControlTower/Services/JevExecutionWorkspace.cs` — 등록 선택 검증·경로 충돌·안정 ID·실행/중지 resolver.
- `src/AIControlTower/ViewModels/MainViewModel.cs`, `src/AIControlTower/JevControlWindow.xaml` — 세션 내 명시 선택 및 같은 scope의 실행/중지.
- `src/AIControlTower/Services/WorkDashboardService.cs` — 정확한 등록 scope에 대한 Jev 분류. 모델명이나 프로세스명만으로 추정하지 않음.
- `tests/AIControlTower.Tests/ScopedJevExecutionTests.cs`, `tests/AIControlTower.Tests/AiCompletionContractTests.cs` — 병렬 소유 프로세스와 안전/호환 회귀 검사.

신규 검사만 다시 실행:

```powershell
dotnet test ai-control-tower/tests/AIControlTower.Tests/AIControlTower.Tests.csproj --filter FullyQualifiedName~ScopedJevExecutionTests
```

명령은 저장소 루트에서 실행하며, 실제 프로젝트·설치 설정이 아닌 D 전용 fixture를 사용합니다. 실제 복수 Jev 실사용은 별도 명시적 검증 범위입니다.

카탈로그 자동 갱신은 같은 프로젝트·작업 ID·정규 작업 폴더의 명시 선택과 중지 대상을 유지합니다. 실제 프로젝트/폴더/등록 실행 변경은 명시 선택을 해제합니다. DTO 객체가 교체되는 것만으로 실행 범위를 바꾸지 않습니다.

최종 602개 검사는 xUnit collection 병렬만 끈 코드 기반 실행으로 32초에 통과했습니다. 개별 검사 안의 두 프로세스 병렬 시험은 그대로 수행했습니다. 이전 기본 병렬 전체 실행의 격리 Git fixture push timeout은 로그에 보존하며 원인을 확정하지 않았습니다. 검사 deadline을 늘리거나 production Git을 바꾸지 않았습니다.
