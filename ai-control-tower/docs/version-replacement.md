# 정확한 버전으로 안전하게 전환하기

`scripts/switch_control_version.ps1`은 정확히 승인된 legacy **0.9.6** 또는 **0.9.7 이상** 현재본의 종료를 확인한 뒤, 별도 폴더의 검증된 manual-control을 한 번 실행하는 코드 전용 도우미입니다. 검증된 종료 IPC 기능이 없는 기존 빌드는 정상 사용자 종료를 기다립니다. 검증된 IPC 기능이 등록된 빌드만 정확한 기존 EXE의 headless 종료 요청을 사용하며 강제 종료는 하지 않습니다.

## 최신 운영·검증 · 2026-10-07

현재 **0.9.8 manual-control PID 25772**, 생성 17:39:00.861918 KST/endpoint ticks `639269591408619180`입니다. 루트 담당자가 fresh trayRegistered/pcConnected true·ownedJobsBusy false와 endpoint 신원을 확인했습니다. 패키지는 `D:\A_KJ\AI\Applications\AIControlTower\versions\0.9.8-20261007-managed`, 앱 소스 `c4f9669fcc287dcb47d74c97dd1f3661e621b488`, ProductVersion `0.9.8+c4f9669fcc287dcb47d74c97dd1f3661e621b488`, FileVersion `0.9.8.0`, 크기 135,902,087바이트, SHA-256 `484cdc3e25a9e3b71fa183898a65bf58165c97f97f3fcabb0d74e5a95667dd2b`입니다. helper/Python 소스는 `cbdefbfa09caed87439ad9e680e744a446ef20b3`; 앱·guard 바이트는 바꾸지 않았습니다. 정확한 0.9.8 capability 튜플을 등록했으며 exact EXE의 비운영 fixture exit0/WindowsCreated0/settingsfalse/MainVMfalse/processes0/syncfalse/stopped0도 확인했습니다.

helper PID 37016이 **한 번/종료 코드 0**으로 C1 PID 9552 실제 종료 → 다른 0.9.8 패키지 PID 376 실행(17:37:24.751947 KST, ticks `639269590447519475`)을 완료했습니다. `FULL_HANDOFF.json`: operatingHandoffConfirmed=true, **oldExitReplyDirectlyCaptured=false**입니다. 구버전 종료 수락 원문을 별도 직접 캡처했다고 주장하지 않습니다. 이어 exact 새 client PID 28344의 `graceful_exit_accepted/0`·빈 stderr → PID 376 실제 종료 → 고정 guard PID 23092 CheckOnly 0·명시 재시작 0 → 현재 PID 25772를 확인했습니다(`NEW_IDLE_ROUNDTRIP.json`). activated/automaticDifferentPackageHandoffVerified/idleCliRoundtripVerified=true, **busyDrainOperatingVerified=false**입니다. 별도 패키지 helper 전체 교체와 새 앱의 유휴 CLI 왕복은 확인했지만 운영 busy drain은 미검증입니다.

최종 **scripts 119 PASS/0 FAIL(119.066초, 종료 코드 0, 실행기 41개)**입니다. 앱 C# 추가 변경 없이 **.NET 634 PASS/0 FAIL(49초, Window 3개 제외)** 근거를 유지합니다. 직전 117개/118.331초의 teardown 1 ERROR 이력은 보존합니다. cleanup RED 2 FAIL(16.568초) → GREEN 5 PASS(15.034초); 기존 8초 안에서 모든 소유 파일의 exclusive read/share0 release를 확인한 뒤 한 번 삭제하며 지속 held는 증거 보존·명확 실패입니다. **이 검사 barrier는 프로세스 종료 증거가 아닙니다.** 기존 production 20초/외부 20·35초 제한은 변경하지 않았습니다. Python CLI RED 6 FAIL/4 PASS(15.112초) → GREEN 10 PASS(14.896초), DAG 차단·replay 1·privacy 0은 소스 CLI 검증이며 기존 Bridge 3.0 운영 설치 변경이 아닙니다.

root supervisor PID 5996은 그대로이며 Bridge PID 5140/v3 localReady·relayConnected true/parallel 4(08:39:30Z), 별도 Remote pong(08:39:27Z)을 확인했습니다. X는 트레이 유지, 강제 종료·창 제어는 없습니다. 보호 config·중앙 Git locks·실제 여러 Jev 모델 완료·모든 GPT 자동 연결은 미완료이며 PR #31 draft·미병합, review disabled/unmanaged, 예약 PAUSED입니다.

## 이전 C1 운영·0.9.8 준비 검증 · 2026-10-07

아래의 현재/미배포 표현은 활성화 전 당시 기록이며 위 최신 운영 확인이 우선합니다.

다음 **0.9.8은 소스 준비 단계이며 아직 배포·실행하지 않았습니다.** 버전은 ASCII 정규형 3부분(각 0~65535·선행 0/부호/공백/접미사 없음), 정확한 버전 폴더와 숫자 순서로 검증합니다. 대상은 0.9.7 이상이며 현재보다 낮을 수 없고, 모든 nonlegacy 현재본은 고정 descriptor 신원까지 확인합니다. 명시 AI 항목 오류는 후속 성공으로 지우지 않으며 일반 도구 nonzero는 회복 가능한 오류로 구분합니다. 버전 focused 4개(62행렬) PASS(1.850초), AI 오류 focused 10개 PASS(47ms); 최신 전체 **.NET 634 PASS/0 FAIL(49초, Window 3개 제외)**, **scripts 106 PASS/0 FAIL(103.428초, 종료 코드 0)**입니다. 근거는 `full-handoff-net-final.log/trx`, `full-handoff-scripts-final.log`입니다. 신규 패키지 신원·등록·전체 자동 버전 교체 운영 성공은 아직 확정하지 않습니다.

앞선 **scripts 102 PASS / 0 FAIL(89.035초, 실행기 34개 포함)**의 근거는 `automatic-exit-scripts-C2-final-102.log`입니다. 환경·외부 EOF focused 4 PASS(6.217초), 앱 .NET 624 PASS/0 FAIL도 이번 운영 전환 후 재실행한 검사가 아닙니다. C1 앱의 아래 등록 소스·SHA·바이트와 C2 helper 바이트/해시는 불변입니다.

사용자 트레이 종료 확인 후 legacy `475a0fc` PID 37456 소멸 → 검증 helper의 C1 PID 25136 실행(16:46:33.798088 KST)으로 첫 전환을 완료했습니다. 정확 C1 headless client PID 22216의 단일 `--request-manual-exit`가 `graceful_exit_accepted/0`·빈 stderr를 반환했고 PID 25136의 실제 종료를 확인했습니다. 고정 guard CheckOnly 0·명시 재시작 0으로 현재 C1 PID 9552가 실행됐습니다(16:48:14.613517 KST). 루트 담당자가 fresh trayRegistered/pcConnected true·ownedJobsBusy false와 endpoint PID 9552 신원을 확인했고 root supervisor PID 5996은 그대로입니다.

이번 증거는 **유휴 정상 CLI 종료 → 동일 C1 패키지 guard 재시작을 한 번 확인한 것**입니다. capable 기존본 → 다른 새 패키지의 helper 전체 자동 교체나 운영 busy drain 증거는 아닙니다. 강제 종료·창 제어·Bridge/Git/설정 변경은 하지 않았습니다. 보호 Bridge 설정·중앙 clone locks와 실제 여러 모델 동시 작업 증거는 여전히 막힘/미확인이며 PR #31은 draft·미병합, review는 disabled/unmanaged입니다.

종료 client는 검증된 **기존** D 패키지의 `bundle-extract`·`runtime-temp` 디렉터리를 reparse 검사한 뒤 `DOTNET_BUNDLE_EXTRACT_BASE_DIR`·`TEMP`·`TMP`를 자식 환경에 고정합니다. Python harness는 소유 D 출력 파일과 정확한 부모 프로세스 종료 대기로 상속 자식 EOF를 기다리지 않습니다. 기존 내부 20초·외부 35초 제한과 elapsed 검사는 유지합니다.

이전 전체 98개 검사는 두 번 외부 35초 timeout으로 각각 97 PASS/1 ERROR(95.932초·126.665초)였습니다. 변경 없는 deadline 단독 검사는 PASS(25.864초), private 추적은 reader 20,065ms·부모 종료 20.615초·capture EOF 25.900초를 구분했습니다. 상속 capture EOF 지연은 확인했지만 두 전체 실행의 추가 지연 원인이 모두 증명된 것은 아닙니다.

## 기존 창의 종료 방식

- 인수 없는 일반 실행(`-CurrentMode application`, 기본값): 실행 중인 작업이 모두 끝난 것을 확인하고 기존 창의 **X**로 정상 종료합니다.
- `--manual-control` 실행(`-CurrentMode manual-control`): X는 창을 트레이로 보냅니다. 작업이 끝난 뒤 트레이의 **종료**를 선택합니다.
- 기존 0.9.6 및 `475a0fc` 소스의 0.9.7에는 비화면 종료 IPC가 없습니다. **알 수 없는 CLI 인수로 기능을 탐색하지 않습니다.** 구버전은 잘못된 인수에서도 일반 창 초기화로 넘어갈 수 있습니다. 창 조회·입력·닫기 메시지·강제 종료는 사용하지 않습니다. 일반 창 종료는 소유 작업을 중지할 수 있으므로 작업 중에는 종료하지 않습니다.

## 입력과 검증 경계

필수 입력은 현재 EXE의 정확한 경로와 승인된 SHA-256, 대상 `VersionRoot`와 버전·소스 커밋·SHA-256, 명시적으로 준비한 상태 JSON 경로입니다.

- 현재 EXE: `D:\A_KJ\AI\Applications\AIControlTower\versions\<ExpectedCurrentVersion>-<tag>\AIControlTower.exe`의 정확한 직접 하위 패키지입니다. 정규형 버전은 고정 legacy 0.9.6 또는 0.9.7 이상이며 `ExpectedCurrentVersion/ExpectedCurrentSourceCommit/ExpectedCurrentSha256`이 실제 메타데이터·바이트와 일치해야 합니다. 0.9.6 기본값은 기존 승인된 `0.9.6+84840a130d8a3fb6911b50fe528ab3709e1c76e1`과 SHA-256에 계속 고정됩니다. 모든 nonlegacy 현재본은 같은 폴더의 고정 `DEPLOYMENT.json` 신원 필드까지 일치해야 합니다. 현재 실행본의 과거 실행기 descriptor 만료는 현재 신원 부정의 근거가 아니며, 대상 실행 descriptor 만료 검사는 유지합니다.
- 대상: 같은 `versions`의 직접 하위 `<ExpectedVersion>-<tag>` 폴더입니다(tag는 ASCII 영숫자·`_`·`-`). 정규형 0.9.7 이상이며 현재보다 낮은 버전·같은 폴더는 거부합니다. 같은 버전의 다른 소스 커밋은 정확한 요청·패키지 검증을 거쳐 선택할 수 있습니다.
- 실행기는 대상 폴더의 `start_manual_control.ps1`만 사용합니다. 호출 때마다 reparse 경계와 승인된 실행기 SHA-256을 확인합니다. 다른 스크립트나 실행 파일, 검사 fixture를 CLI로 지정할 수 없습니다.
- 실행기 승인 SHA-256: `a608ac3553310edf211f2ee311a14dca84d4e5a416db02f3f503a04464652730`.
- 상태 파일은 `D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks` 또는 `D:\A_KJ\AI\ControlTowerData\version-replacement` 하위의 JSON만 허용합니다. 부모 폴더는 호출자가 미리 준비합니다. 경로 전체의 reparse를 거부합니다.
- `-WaitSeconds`는 1~1800초, 기본 1800초입니다. `-PollSeconds`는 1~30초, 기본 2초입니다. 별도 waiter mutex가 중복 대기를 막습니다.
- `-CheckOnly`는 패키지를 검증하고 상태를 기록할 뿐, 실행 프로세스를 조회하거나 새 앱을 시작하지 않습니다.

대기 시작 전 대상 패키지를 검증합니다. 현재 프로세스는 CIM의 정확한 EXE 경로·인수·세션·PID·생성 시각으로 묶습니다. PID 재사용, 다른 인스턴스, 미확인 실행 형태는 전환을 보류합니다. 기존 프로세스가 사라진 뒤 대상 패키지를 다시 검증하고, 기존 실행기의 프로세스·manual mutex 검사와 `--no-activate-existing` 자식 계약을 거쳐 실행합니다. 해시 검사는 .NET SHA-256 스트림을 사용하므로 PowerShell `Get-FileHash` cmdlet 유무에 의존하지 않습니다.

## 검증된 종료 IPC 빌드만 자동 요청

소스의 `Test-SwitchExitCapability`는 **버전·소스 커밋·SHA-256 고정 allowlist**만 검사합니다. 루트 담당자가 실제 IPC 빌드를 검증한 뒤 아래 정확한 튜플을 등록했습니다. 운영 CLI로 capability를 등록하거나 임의 bool로 지원을 주장할 수 없습니다. 목록에 없는 빌드는 기존 방식대로 종료를 기다립니다.

등록된 C1 패키지: 버전 `0.9.7`, 소스 `5b0f3296d259ce03882e16eba1d8f93604af570f`, SHA-256 `2615d4a88f53410d963403cbb4dafabd7f6abf003c3a95737d4138b362cb244f`, ProductVersion `0.9.7+5b0f3296d259ce03882e16eba1d8f93604af570f`, FileVersion `0.9.7.0`, 크기 135,902,087바이트입니다. 루트의 `automatic-exit-C1-exact-proof.json`은 정확한 self-contained EXE의 pure fixture 성공(Windows·설정·MainViewModel·자식 프로세스·동기화·중지 없이), owner 없는 headless 요청 `unsupported/4`, 추가 인수 `invalid_request/2`, 단일 JSON stdout·빈 stderr를 확인했습니다. **이 비운영 증거 자체는 실제 운영 종료·교체 성공을 뜻하지 않습니다.** 목록 밖의 종전 `475a0fc` 첫 전환은 사용자 트레이 종료로 완료했으며 이번 유휴 운영 정상 종료 증거는 위 최신 구역과 구분합니다.

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
