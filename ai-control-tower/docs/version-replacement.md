# 정확한 버전으로 안전하게 전환하기

`scripts/switch_control_version.ps1`은 정확히 승인된 기존 **0.9.6/0.9.7**의 종료를 확인한 뒤, 별도 폴더의 **0.9.7 manual-control**을 한 번 실행하는 코드 전용 도우미입니다. 검증된 종료 IPC 기능이 없는 기존 빌드는 정상 사용자 종료를 기다립니다. 검증된 IPC 기능이 등록된 빌드만 정확한 기존 EXE의 headless 종료 요청을 사용하며 강제 종료는 하지 않습니다.

## 기존 창의 종료 방식

- 인수 없는 일반 실행(`-CurrentMode application`, 기본값): 실행 중인 작업이 모두 끝난 것을 확인하고 기존 창의 **X**로 정상 종료합니다.
- `--manual-control` 실행(`-CurrentMode manual-control`): X는 창을 트레이로 보냅니다. 작업이 끝난 뒤 트레이의 **종료**를 선택합니다.
- 기존 0.9.6 및 `475a0fc` 소스의 0.9.7에는 비화면 종료 IPC가 없습니다. **알 수 없는 CLI 인수로 기능을 탐색하지 않습니다.** 구버전은 잘못된 인수에서도 일반 창 초기화로 넘어갈 수 있습니다. 창 조회·입력·닫기 메시지·강제 종료는 사용하지 않습니다. 일반 창 종료는 소유 작업을 중지할 수 있으므로 작업 중에는 종료하지 않습니다.

## 입력과 검증 경계

필수 입력은 현재 EXE의 정확한 경로와 승인된 SHA-256, 대상 `VersionRoot`와 버전·소스 커밋·SHA-256, 명시적으로 준비한 상태 JSON 경로입니다.

- 현재 EXE: `D:\A_KJ\AI\Applications\AIControlTower\versions\0.9.6-*\AIControlTower.exe` 또는 `0.9.7-*\AIControlTower.exe`. `ExpectedCurrentVersion/ExpectedCurrentSourceCommit/ExpectedCurrentSha256`이 실제 메타데이터·바이트와 일치해야 합니다. 0.9.6 기본값은 기존 승인된 `0.9.6+84840a130d8a3fb6911b50fe528ab3709e1c76e1`과 SHA-256에 계속 고정됩니다. 현재 0.9.7은 같은 폴더의 고정 `DEPLOYMENT.json` 신원 필드까지 일치해야 합니다. 현재 실행본의 과거 실행기 descriptor 만료는 현재 신원 부정의 근거가 아니며, 대상 실행 descriptor 만료 검사는 유지합니다.
- 대상: 같은 `versions`의 직접 하위 `0.9.7-*` 폴더입니다. 기존 폴더와 같을 수 없습니다. 같은 버전의 다른 소스 커밋은 정확한 요청·패키지 검증을 거쳐 선택할 수 있습니다.
- 실행기는 대상 폴더의 `start_manual_control.ps1`만 사용합니다. 호출 때마다 reparse 경계와 승인된 실행기 SHA-256을 확인합니다. 다른 스크립트나 실행 파일, 검사 fixture를 CLI로 지정할 수 없습니다.
- 실행기 승인 SHA-256: `a608ac3553310edf211f2ee311a14dca84d4e5a416db02f3f503a04464652730`.
- 상태 파일은 `D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks` 또는 `D:\A_KJ\AI\ControlTowerData\version-replacement` 하위의 JSON만 허용합니다. 부모 폴더는 호출자가 미리 준비합니다. 경로 전체의 reparse를 거부합니다.
- `-WaitSeconds`는 1~1800초, 기본 1800초입니다. `-PollSeconds`는 1~30초, 기본 2초입니다. 별도 waiter mutex가 중복 대기를 막습니다.
- `-CheckOnly`는 패키지를 검증하고 상태를 기록할 뿐, 실행 프로세스를 조회하거나 새 앱을 시작하지 않습니다.

대기 시작 전 대상 패키지를 검증합니다. 현재 프로세스는 CIM의 정확한 EXE 경로·인수·세션·PID·생성 시각으로 묶습니다. PID 재사용, 다른 인스턴스, 미확인 실행 형태는 전환을 보류합니다. 기존 프로세스가 사라진 뒤 대상 패키지를 다시 검증하고, 기존 실행기의 프로세스·manual mutex 검사와 `--no-activate-existing` 자식 계약을 거쳐 실행합니다. 해시 검사는 .NET SHA-256 스트림을 사용하므로 PowerShell `Get-FileHash` cmdlet 유무에 의존하지 않습니다.

## 검증된 종료 IPC 빌드만 자동 요청

소스의 `Test-SwitchExitCapability`는 **버전·소스 커밋·SHA-256 고정 allowlist**만 검사합니다. 초기 목록은 비어 있으며 루트 담당자가 실제 IPC 빌드를 검증한 뒤 정확한 튜플을 등록합니다. 운영 CLI로 capability를 등록하거나 임의 bool로 지원을 주장할 수 없습니다. 목록에 없는 빌드는 기존 방식대로 종료를 기다립니다.

목록에 등록된 manual-control 빌드만 **기존 EXE 자체 + 단일 `--request-manual-exit` 인수**를 호출합니다. 새 대상 EXE를 기존 서버의 client로 사용하지 않습니다. 응답의 schemaVersion=1·status/동일 reason·exitCode·native 종료 코드가 일치해야 합니다. `graceful_exit_accepted/0`은 요청 수락일 뿐 실제 종료가 아닙니다. 이후 원래 PID/생성 시각/경로의 실제 종료를 확인하며 종료 요청을 반복하지 않습니다.

서버가 종료를 예약하지 않는다고 보장한 `held_jobs`, `held_timeout`, `pending_writes`만 같은 live 신원을 재검증한 뒤 전체 기한 내에서 재조회합니다. `held_unknown`, `held_request_pending`, 신원 거절·잘못된 요청은 보류하고 재시도하지 않습니다. `unsupported`는 더 이상 요청하지 않고 사용자 정상 종료를 기다립니다. `outcome_unknown/6`, 전송 오류, schema/종료 코드 불일치는 결과 미확인으로 보류하며 재요청하지 않습니다.

종료 요청·재조회·실제 종료 확인은 하나의 **monotonic 기한**을 공유합니다. 개별 client 응답 읽기 제한은 기존 20초를 유지하며 전체 남은 시간이 20초보다 짧으면 client를 시작하지 않습니다. 실제 앱 종료 확인 없이는 새 앱을 실행하지 않습니다.

## 보존 및 결과 의미

루트 EXE의 정확한 `--remote-supervisor`는 그대로 보존합니다. 다른 형태의 관제탑 프로세스는 종료하지 않고 보류합니다. ProjectBridge, 원격 연결, 다른 작업, 설정, 자동 시작, 예약, Git, 기존 EXE·대상 패키지는 수정하지 않습니다. 실패한 검사나 실행은 자동 재시도하지 않으며 예약 작업도 만들지 않습니다.

상태 JSON과 표준 출력에는 경로·명령 원문·비밀값 없이 다음 결과를 기록합니다.

| 상태 | 의미 |
|---|---|
| `verified` | CheckOnly 검증; 프로세스 준비 상태 또는 실제 운영 확인 아님 |
| `waiting` | 해당 실행 방식의 정상 종료가 필요함; `requiresUserExit=true` |
| `held` | 만료·미확인 인스턴스·검사 실패·대기 제한 등으로 보류; 자동 재시도 없음 |
| `started` | 검증 실행기가 시작 요청을 수락함; **실제 화면 사용·연결·업무 성공은 별도 확인** |
| `outcome_unknown` | 실행기 결과가 불명확함; `started=null`, 자동 재시도 없음. 실제 상태를 별도로 확인해야 함 |

`operatingConfirmed`는 항상 false입니다. 실행 수락 뒤 상태 파일 저장만 실패해도 이미 관찰한 `started`를 거짓 `held`로 바꾸지 않습니다. 실행기 응답은 **단일 newline JSON과 부모 실행기의 종료**로 확인합니다. 새 앱이 상속한 stdout/stderr 핸들이 닫힐 때까지 EOF를 기다리지 않습니다. stdout/stderr 검사는 각각 16,384자 이내이며 잘못된 JSON, 추가 본문·두 번째 JSON, stderr 오류는 정상 실행 확인으로 취급하지 않습니다.

실행기 응답 읽기와 부모 종료 확인은 **하나의 20초 전체 제한**을 공유합니다. 부모 종료 뒤 이미 전달된 추가 stdout/stderr는 짧은 bounded probe로 확인하며, 상속된 열린 핸들 자체는 오류가 아닙니다. 제한 내에 결과가 불명확하면 `outcome_unknown`으로 보고하고 임의로 프로세스를 종료하거나 재시도하지 않습니다. 정상 앱 종료 대기 시간 제한이 끝나면 도우미는 보류로 종료하며, 나중에 사용자가 창을 닫아도 자동 실행하지 않습니다.

## 격리 검사

소스 루트에서 `python -m unittest discover -s scripts -p test_switch_control_version.py -v`를 실행합니다. 검사는 도우미의 실제 검증·대기 로직을 불러오고, OS 프로세스 조회·실행 경계를 검사 harness에서만 대체합니다. 실제 앱·창·원격 연결은 사용하지 않습니다. 실제 .NET 파일 해시 경계와 **소유한 짧은 자식의 상속 pipe**도 D 드라이브 fixture로 검사합니다. 모든 새 검사 subprocess의 TEMP/TMP는 해당 D fixture에 지정하며 자식은 자연 종료합니다.
