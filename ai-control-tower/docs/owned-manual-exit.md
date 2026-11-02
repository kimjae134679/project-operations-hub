# 수동 관제창의 소유 안전 종료

## 범위

수동 관제창(`--manual-control`)만 자기 프로세스를 안전하게 종료하는 통로다. 다른 프로세스/PID/창을 닫거나 원격 연결·Bridge·작업을 중지하는 기능이 아니다. X는 기존처럼 등록된 트레이로 숨기며, 트레이 등록 실패 시 창을 보존한다. 트레이의 명시적 종료도 같은 안전 확인을 거친다. 알림 영역 아이콘은 기존 기능이며 새로운 팝업 알림을 구현한 것은 아니다.

현재 실행 중인 이전 `0.9.7 / 475a0fc`에는 이 IPC가 없다. 새 소스 작성만으로 그 프로세스에 기능이 생기지 않는다. 최초 전환은 사용자의 기존 트레이 종료가 필요하며, 창/PID 강제 종료로 우회하지 않는다. 새 소스 버전 번호도 0.9.7로 유지한다. 배포·실제 자동 종료/재실행 여부는 별도 검증이다.

## 고정 headless 계약

소유 서버와 **동일한 기존 실행 파일**을 다음 한 인자로 실행한다. 새 빌드의 실행 파일로 이전 빌드를 제어하지 않는다.

```text
AIControlTower.exe --request-manual-exit
```

정상 Startup/설정 읽기/뮤텍스 활성화/MainWindow/MainVM/원격 초기화보다 먼저 처리한다. 임의 PID·경로·명령 인자는 받지 않는다. stdout은 UTF-8 JSON 한 줄, native 종료 코드는 JSON `exitCode`와 같다.

```json
{"schemaVersion":1,"status":"graceful_exit_accepted","reason":"graceful_exit_accepted","exitCode":0}
```

`reason`은 `status`와 동일한 리터럴이다.

| status | exitCode | 의미 / 재시도 |
|---|---:|---|
| `graceful_exit_accepted` | 0 | 유휴 확인 후 응답을 flush하고 자기 종료를 예약함. **실제 종료 완료가 아님**. 원래 PID/생성 시각/파일/빌드 생존을 확인한 뒤 교체한다. |
| `invalid_request` | 2 | 정확한 인자 계약 위반. 실행 입구 진입 없음. |
| `held_jobs` | 3 | 소유 실행/큐 예약 있음. 종료하지 않고 admission/자동 유지 재개. |
| `held_timeout` | 3 | 읽기/제어 작업이 제한 시간에 끝나지 않음. 종료하지 않고 재개. |
| `held_unknown` | 3 | 상태가 불명확함. 종료하지 않는다. 쓰기가 없다는 명시적 상태는 재개하며, 상태 읽기 자체 실패 시 freeze를 유지한다. |
| `pending_writes` | 3 | 실제 쓰기/소유 PC 변경 작업이 남음. **종료하지 않고 freeze 유지**. 이후 소유 상태 재확인 요청으로 안전 종료 가능. |
| `held_request_pending` | 3 | 다른 안전 종료 요청이 처리 중. 그 요청 결과를 확인하기 전 재호출하지 않는다. |
| `unsupported` | 4 | 요청 전 채널 없음/연결 실패. 기존 앱에 강제 종료로 대체하지 않는다. |
| `identity_rejected` | 5 | 소유/피어/프로토콜 일치 실패. 다른 앱을 제어하지 않는다. |
| `outcome_unknown` | 6 | 요청 일부 전송 이후 응답 불명확. **자동 재호출 금지**, 원래 소유 프로세스의 지연 종료/생존을 확인하고 보류한다. |

서비스 계약상 `held_jobs`, `held_timeout`, `held_unknown`, `pending_writes` 요청 자체는 종료를 예약하지 않으며 원래 소유 인스턴스를 다시 확인한 후 상태 재조회가 가능하다. 다만 교체 도우미는 더 보수적으로 **`held_jobs`, `held_timeout`, `pending_writes`만 자동 재조회**하고, `held_unknown`, `held_request_pending`, `outcome_unknown`은 자동 재호출하지 않고 보류한다. 별도 사용자 트레이 종료나 다른 소유 요청의 결과까지 보증하는 뜻은 아니다. 첫 readiness 조회가 예외이면 쓰기 상태를 알 수 없으므로 freeze를 유지하고, 마지막 관측에서 실제 쓰기가 있었다면 `pending_writes`로 유지한다.

## 소유 검증

고정 descriptor는 `D:\A_KJ\AI\ControlTowerData\manual-control\manual-exit-owner.json`이다. 원자적 파일 교체와 자기 인스턴스 일치 시에만 삭제한다. 테스트는 별도 D GUID fixture만 사용한다.

- `CurrentUserOnly` named pipe와 OS 피어 PID 조회를 사용한다. 같은 사용자라도 다른 세션/실행 경로/실행 파일 SHA-256/관리 빌드 MVID이면 거절한다.
- 운영 App의 채널 등록/클라이언트는 `D:\A_KJ\AI\Applications\AIControlTower\versions\<직접 하위 빌드 폴더>\AIControlTower.exe` 및 동일 `AppContext.BaseDirectory`만 허용한다. 경로는 정규형·reparse 없음이어야 한다. 공유 `dotnet.exe`/`testhost.exe`, source/bin 실행, fallback 설치 경로는 이 자동 종료 통로를 제공하지 않는다. self-contained 단일 배포물의 실제 SHA·capability는 교체 도우미의 배포 검증이 별도로 고정해야 한다. 테스트 서비스의 별도 D fixture 주입은 CLI root override가 아니다.
- 클라이언트는 서버 PID·생성 시각·세션·경로·빌드와 연결된 pipe 서버 PID를 모두 확인한다.
- 인스턴스 nonce 및 매 연결의 새 일회성 challenge를 결합한다. 동작은 `graceful_exit` 하나뿐이며 JSON은 중복/추가 필드·잘못된 nonce·4KiB 초과를 거절한다. 임의 명령/타깃 PID를 프로토콜로 보내지 않는다.
- 연결 3초, 전체 교환 12초, 실제 유휴 대기 5초로 제한한다. 전송 실패로 실제 작업을 취소하지 않는다.
- 같은 계정의 악성 코드가 사용자 소유 파일/실행 파일을 변경하는 공격을 OS 샌드박스로 차단하는 기능은 아니다.

## drain 대상과 보수적 제한

종료 확인은 dispatcher에서 새 등록 실행·수집·공개·PC 요청·조회 admission을 먼저 잠근다. 초기화 실제 Task, 진행 중 timer cycle, 발견/통신/새로고침/서버/접속자 semaphore, dashboard 상세 실제 조회, 문서 읽기 실제 개수, 소유 PC 호출을 확인한다.

특히 수집/공개 wrapper Task가 이미 끝나도 `_manualCollection.IsRunning`, `_registeredRecordPublishing.IsRunning`이 실제 쓰기 끝까지 유지되므로 이를 별도로 검사한다. 취소로 유휴인 척하거나 `StopAllOwned`로 작업을 없애지 않는다. PC의 외부 `ActiveJobCount`는 이 앱 소유 활동이 아니며 다른 프로젝트 원격 작업 종료를 기다리거나 취소하지 않는다.

현재 연결된 소유 활동만 관리한다. 이후 추가된 추적되지 않는 쓰기/부작용은 이 유휴 판정에 반드시 등록해야 한다. opaque/fault 상태에서는 fail-closed로 보류하며 전체 PC/임의 외부 작업이 안전하다는 보증을 하지 않는다.

## 검증 구분

`OwnedManualExitTests`, `OwnedManualReaderDrainTests`, `OwnedManualExitPipeTests`, `OwnedManualExitHostTests`의 22개 사례는 코드/순수 정책 및 새 D fixture의 무해한 IPC를 검사한다. 실제 same-account `powershell.exe` 피어는 drain 호출 전에 거절되는 사례도 포함한다. pipe 테스트의 accepted callback은 테스트 신호이며 실제 앱 종료가 아니다. 실제 운영 앱 재시작, 계정 변경, 다중 원격 AI, Bridge 업그레이드 성공을 이 검사로 주장하지 않는다. 22개 최종 GREEN/전체 검사 및 최신 로그 여부는 상위 실행자의 검증 보고가 원본이다.
