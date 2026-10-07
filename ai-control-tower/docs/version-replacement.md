# 정확한 버전으로 안전하게 전환하기

`scripts/switch_control_version.ps1`은 승인된 **0.9.6 종료를 기다린 뒤**, 정확히 검증한 별도 폴더의 **0.9.7 manual-control**을 한 번 실행하는 코드 전용 도우미입니다. 기존 프로세스를 자동 종료하는 도구가 아닙니다.

## 기존 창의 종료 방식

- 인수 없는 일반 실행(`-CurrentMode application`, 기본값): 실행 중인 작업이 모두 끝난 것을 확인하고 기존 창의 **X**로 정상 종료합니다.
- `--manual-control` 실행(`-CurrentMode manual-control`): X는 창을 트레이로 보냅니다. 작업이 끝난 뒤 트레이의 **종료**를 선택합니다.
- 실행 중인 0.9.6에는 비화면 종료 IPC가 없습니다. 도우미는 창 조회, 입력, 닫기 메시지, 종료 신호, 강제 종료를 보내지 않습니다. 일반 창 종료는 소유 작업을 중지할 수 있으므로 작업 중에는 종료하지 않습니다.

## 입력과 검증 경계

필수 입력은 현재 EXE의 정확한 경로와 승인된 SHA-256, 대상 `VersionRoot`와 버전·소스 커밋·SHA-256, 명시적으로 준비한 상태 JSON 경로입니다.

- 현재 EXE: `D:\A_KJ\AI\Applications\AIControlTower\versions\0.9.6-*\AIControlTower.exe`; 승인된 `0.9.6+84840a130d8a3fb6911b50fe528ab3709e1c76e1`과 SHA-256만 허용합니다.
- 대상: 같은 `versions`의 직접 하위 `0.9.7-*` 폴더입니다. 기존 폴더와 같을 수 없습니다. 같은 버전의 다른 소스 커밋은 정확한 요청·패키지 검증을 거쳐 선택할 수 있습니다.
- 실행기는 대상 폴더의 `start_manual_control.ps1`만 사용합니다. 호출 때마다 reparse 경계와 승인된 실행기 SHA-256을 확인합니다. 다른 스크립트나 실행 파일, 검사 fixture를 CLI로 지정할 수 없습니다.
- 실행기 승인 SHA-256: `a608ac3553310edf211f2ee311a14dca84d4e5a416db02f3f503a04464652730`.
- 상태 파일은 `D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks` 또는 `D:\A_KJ\AI\ControlTowerData\version-replacement` 하위의 JSON만 허용합니다. 부모 폴더는 호출자가 미리 준비합니다. 경로 전체의 reparse를 거부합니다.
- `-WaitSeconds`는 1~1800초, 기본 1800초입니다. `-PollSeconds`는 1~30초, 기본 2초입니다. 별도 waiter mutex가 중복 대기를 막습니다.
- `-CheckOnly`는 패키지를 검증하고 상태를 기록할 뿐, 실행 프로세스를 조회하거나 새 앱을 시작하지 않습니다.

대기 시작 전 대상 패키지를 검증합니다. 현재 프로세스는 CIM의 정확한 EXE 경로·인수·세션·PID·생성 시각으로 묶습니다. PID 재사용, 다른 인스턴스, 미확인 실행 형태는 전환을 보류합니다. 기존 프로세스가 사라진 뒤 대상 패키지를 다시 검증하고, 기존 실행기의 프로세스·manual mutex 검사와 `--no-activate-existing` 자식 계약을 거쳐 실행합니다. 해시 검사는 .NET SHA-256 스트림을 사용하므로 PowerShell `Get-FileHash` cmdlet 유무에 의존하지 않습니다.

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

`operatingConfirmed`는 항상 false입니다. 실행 수락 뒤 상태 파일 저장만 실패해도 이미 관찰한 `started`를 거짓 `held`로 바꾸지 않습니다. 실행기 완료 대기 제한은 20초이며 그 후 결과가 불명확하면 임의로 프로세스를 종료하지 않습니다. 정상 대기 시간 제한이 끝나면 도우미는 보류로 종료하며, 나중에 사용자가 창을 닫아도 자동 실행하지 않습니다.

## 격리 검사

소스 루트에서 `python -m unittest discover -s scripts -p test_switch_control_version.py -v`를 실행합니다. 검사는 도우미의 실제 검증·대기 로직을 불러오고, OS 프로세스 조회·실행 경계를 검사 harness에서만 대체합니다. 실제 앱·창·원격 연결은 사용하지 않습니다. 실제 .NET 파일 해시 경계도 소유한 D 드라이브 fixture로 검사합니다.
