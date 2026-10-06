# 승인된 프로젝트 작업을 순서대로 이어 실행하기

`scripts/continuous_manager.py`는 **검토·승인한 고정 JSON 작업 목록**만 실행하는 로컬 CLI입니다. 기존 ProjectBridge·원격 연결을 대체하거나 다른 채팅을 감시하지 않습니다. 새로운 작업을 AI가 임의로 승인하는 플랫폼, 예약 서비스, GUI 통합, 중앙 공지 동기화, Jev 연결은 이 기능의 완료 범위가 아닙니다.

## 입력·상태·결과

- 입력: 대상 프로젝트의 전용 `work/` 아래에 둔 JSON 목록. 프로젝트 ID, 정확한 승인 루트, argv, 선행 단계, 단계/전체 시간·출력 한도를 포함합니다.
- 결과: **승인 루트 내부의 전용 상태 폴더**의 `state.json`, `events.private.jsonl`, `logs/*.stdout.private.log`, `*.stderr.private.log`. AI 입력은 `*.prompt.private.txt`에 따로 남습니다. 이 폴더는 공개 저장소·자동 소통 수집 대상에 넣지 말고 사용자 전용 위치/접근 권한을 사용합니다. `private`라는 파일명은 암호화나 ACL 설정을 뜻하지 않습니다.
- `state.json`은 원자적으로 갱신하고 시작·끝·실제 종료 코드를 기록합니다. Windows의 잠깐 열린 상태 읽기 핸들 때문에 이름 교체가 막히면 동일 교체만 최대 1초 재시도합니다. **명령 자체를 재시도하지 않습니다.**
- 이전 성공 단계는 다시 실행하지 않습니다. 실패·시간 초과·중단·결과 불명 단계는 자동 재시도하지 않습니다. `running` 중 강제 종료된 단계는 다음 실행에서 결과 불명 `blocked`가 됩니다.
- 선행 단계가 모두 실제 종료 코드 0으로 끝난 경우에만 다음 단계를 시작합니다. 이는 **프로세스 성공**이며 프로젝트 목표·모델 답변의 정확성 검증을 대신하지 않습니다. 필요한 검증 명령도 별도 단계로 포함합니다.
- OS 파일 잠금으로 같은 상태 폴더의 동시 실행을 막습니다. Windows에서는 자식을 정지 상태로 만들고 전용 JobObject에 할당한 뒤 실행하며, 정상 종료·중단·시간 초과·실행기 강제 종료 때 그 자식/일반 자손만 종료합니다. 다른 원격·콘텐츠 제작 프로세스를 찾거나 종료하지 않습니다. POSIX에서는 자식 프로세스 그룹을 사용하지만 **실행기 SIGKILL 시 자동 자식 정리는 보장하지 않습니다.**
- stdout/stderr는 단계별 공유 바이트 한도 내에서만 저장합니다. 한도를 넘으면 `output_limit`로 끝내며, 잘린 출력으로 성공을 추정하지 않습니다. JSONL의 `turn.completed.usage`가 실제 존재하면 기록하고, 없으면 토큰·비용을 추정하지 않습니다.

## 안전 경계 — 반드시 구분

1. **신뢰하고 검토한 명령만 승인합니다.** SHA-256은 목록 불변성 확인이지 명령의 안전성 판정이 아닙니다. AI가 새 목록을 만들었다고 자동으로 승인 해시를 발급하거나 검토 없이 `validate` 결과를 `run`에 연결하지 않습니다.
2. 선언된 승인 루트·cwd·상태/로그 경로의 이탈, 바탕화면 경로, 심볼릭 링크/Windows reparse point, 전체 드라이브 루트, 의존성 순환을 거부합니다. 경로를 실행 전에 재확인하지만 악의적 동시 파일 교체까지 막는 OS 보안 경계는 아닙니다.
3. **command argv는 OS 샌드박스가 아닙니다.** `shell=False`, 안전한 cwd 또는 경로 검사만으로 실행 파일·Python 코드가 다른 경로/네트워크를 건드리지 못하게 되지는 않습니다. `-c` 내용, 파일 경로, 호출 스크립트와 부작용을 승인자가 확인해야 합니다. 일괄 권한 승인·대량 원본 삭제·게시·결제·계정 권한 변경을 넣지 않습니다.
4. 다른 상태 폴더로 실행한 목록끼리의 자원 잠금은 이 CLI가 통합하지 않습니다. 같은 프로젝트/파일에 동시 쓰기 목록을 시작하지 않습니다. ProjectBridge 자원 잠금은 별도 기능입니다.
5. 중단 요청은 해당 목록 해시에 묶인 `stop.request.json`으로 남습니다. 같은 목록의 재시작도 중단 상태를 유지합니다. 실패나 중단을 해결하려면 실제 결과를 확인하고 새로운 ID/상태 폴더와 검토·승인한 목록을 만듭니다. 원본 상태를 지우거나 성공으로 조작하지 않습니다.

## 목록 형식 — 실제 무해한 두 단계 예

설치가 확인된 Python 경로를 사용한 예입니다. `approved_root`는 **실제로 허가된 대상 프로젝트의 기존 디렉터리**로 바꿉니다. 아래는 상태를 출력하는 두 Python 프로세스일 뿐 모델 호출·원격 변경·새 설치가 아닙니다.

```json
{
  "schema_version": 1,
  "id": "local-check-001",
  "project": "Control-Tower",
  "approved_root": "D:/A_KJ/AI/Workspace/ControlTower/continuous-20261007/source",
  "max_steps": 10,
  "max_run_seconds": 300,
  "max_output_bytes": 1048576,
  "steps": [
    {
      "id": "first", "type": "command", "cwd": ".",
      "depends_on": [], "timeout_seconds": 30,
      "argv": ["C:/Users/user/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe", "-c", "print('first complete')"]
    },
    {
      "id": "second", "type": "command", "cwd": ".",
      "depends_on": ["first"], "timeout_seconds": 30,
      "argv": ["C:/Users/user/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe", "-c", "print('second complete')"]
    }
  ]
}
```

필수 필드는 예와 같고 모르는 필드는 거부합니다. 최대 1000단계, 전체/단계 최대 86400초, 단계별 stdout+stderr 최대 64 MiB입니다. 숫자 한도를 올려도 새 작업/권한 승인으로 간주하지 않습니다. 실행 파일은 존재하는 절대 경로여야 하며 `.cmd/.bat/.ps1` 셸 래퍼는 지원하지 않습니다. PowerShell 작업은 확인한 `pwsh.exe`/`powershell.exe`를 정확한 argv로 명시합니다.

## 확인·실행·상태·중단 명령

예의 목록을 승인 루트 내부 `work/local-check-001.json`에 저장한 뒤 PowerShell에서:

```powershell
$python = 'C:\Users\user\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
$source = 'D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\source'
$runner = Join-Path $source 'scripts\continuous_manager.py'
$plan = Join-Path $source 'work\local-check-001.json'
$state = Join-Path $source 'work\continuous-private\local-check-001'
& $python $runner validate --plan $plan
# 목록의 실제 argv·범위·부작용을 검토하고 승인한 sha256을 직접 넣습니다.
$approved = '<검토하고 승인한 SHA-256>'
& $python $runner run --plan $plan --state-dir $state --approve-sha256 $approved
& $python $runner status --state-dir $state
& $python $runner stop --state-dir $state
```

종료 코드는 `0`=목록의 모든 단계가 프로세스 성공, `1`=실행 후 실패/막힘/중단, `2`=승인/입력/경로/잠금 등 오류입니다. 같은 해시·상태 폴더로 다시 `run`하면 완료 단계를 건너뜁니다. 다른 목록 해시는 거부합니다.

창 없는 실행은 승인된 목록에만 `Start-Process -WindowStyle Hidden`을 사용합니다. 작업 스케줄러·로그인 등록·전역 설정을 이 CLI가 임의로 만들지 않습니다. 명령 인수 경로에 공백이 있으면 각각 정확하게 인용해야 하며 상태 파일로 실제 시작/완료를 확인합니다.

## 선택 기능: 명시한 모델의 Codex 단계

`command` 대신 다음 단계를 넣을 수 있습니다. 이 경로의 **실제 모델 호출은 이 실행기 테스트에서 하지 않았습니다.** 2026-10-07 현재 이 PC의 상위 관리자 별도 읽기 전용 probe는 CLI 0.160.1 + ChatGPT 로그인 + `gpt-5.5` 성공을 확인했고, 옛 데스크톱 별칭 `gpt-6.1-sol`은 HTTP400 unsupported였습니다. 계정/버전별 지원은 별도로 확인하며 지원되지 않으면 실패 기록 후 멈춥니다. 자동 모델 교체·무한 재시도·API 신규 결제는 없습니다.

```json
{
  "id": "scoped-agent", "type": "codex", "cwd": ".",
  "depends_on": ["first"], "timeout_seconds": 600,
  "executable": "C:/Users/user/AppData/Roaming/npm/node_modules/@openai/codex/node_modules/@openai/codex-win32-x64/vendor/x86_64-pc-windows-msvc/bin/codex.exe",
  "model": "gpt-5.5",
  "prompt": "현재 프로젝트 안내·README/AGENTS·새 공지를 확인한다. 명시 승인된 해당 프로젝트 작업만 수행하고 실제 검증과 남은 막힘을 보고한다. 바탕화면이나 다른 프로젝트·기존 제작 작업은 건드리지 않는다."
}
```

실행은 `codex exec --model <명시값> --sandbox workspace-write -c approval_policy="never" --cd <승인 cwd> --color never --json -`입니다. 샌드박스 우회·`--ignore-rules`·자동 검토 플래그는 쓰지 않습니다. 모델명에 `astra`가 있으면 거부합니다. 입력 prompt는 최대 12000자에 고정 안전 지시만 추가하고 매번 전체 채팅/이력을 복사하지 않습니다. **전역 AGENTS/MCP/Skills가 상속되므로 총 모델 입력 토큰 상한을 보장하지 않습니다.** 전역 연결의 외부 API 권한을 이 샌드박스가 대신 통제한다고 주장하지 않습니다. 승인자는 실행 계정·기존 요금제·연결 도구·훅도 확인해야 합니다. 실제 모델이 필요한 단계만 선택하고 반복 상태 수집·검사는 deterministic command로 처리합니다.

Node 기반 CLI만 있는 환경은 `executable`에 검증한 절대 `node.exe`, 선택 `prefix_argv`에 검토한 **절대 로컬 CLI 스크립트 한 개**를 넣을 수 있습니다. 임의 추가 CLI 권한/우회 플래그는 지원하지 않습니다.

## 검증

```powershell
& $python -m unittest discover -s (Join-Path $source 'scripts') -p test_continuous_manager.py -v
```

무해한 실제 프로세스의 두 단계 연속·성공 재개·실패 차단·역순 의존성·정확한 해시·동시 잠금·중단·시간 제한·강제 종료 자식 정리·Windows 상태 읽기 공유 충돌·경로/reparse 차단·가짜 로컬 agent의 입력 막힘·출력 상한·실제 JSON 사용량 기록을 검증합니다. 모델 호출, GUI 사용자 흐름, 실제 프로젝트 구현, 중앙 공유, 실제 원격 재배포는 별도 검증 범위입니다.

Windows JobObject의 종료/상속 동작은 [Microsoft Job Objects](https://learn.microsoft.com/en-us/windows/win32/procthread/job-objects)의 `KILL_ON_JOB_CLOSE` 기준을 적용했습니다. Job containment는 프로세스 수명 관리이며 명령의 파일·네트워크 샌드박스가 아닙니다.
