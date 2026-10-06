# 공용 PC 작업 규약 3.0

기존 요청 형식은 유지하고 로컬 연결, MCP와 병렬 실행을 추가했습니다. 장치 ID는 설치된 config와 실제 장치 기록을 대조합니다. 기존 KJW ID를 새로 만들거나 임의 교체하지 않습니다.

## 연결

로컬 도구는 `state/local_endpoint.json`에서 `baseUrl`과 `token`을 읽습니다. 서버는 매 실행 시 새 토큰과 포트를 발급하며 `127.0.0.1`에만 바인딩합니다. 모든 요청은 `X-ProjectBridge-Token` 헤더가 필요합니다. 브라우저 Origin은 허용하지 않습니다. 이 파일을 채팅이나 공개 저장소에 복사하지 않습니다.

| API | 용도 |
| --- | --- |
| `GET /v1/status` | 안전한 작업 목록과 로컬/중계 상태 |
| `POST /v1/jobs` | 담당자 고유 `toolId`를 포함한 작업 제출, 응답은 202 + ID |
| `GET /v1/jobs/{id}` | 실제 결과와 상세 출력, 로컬 전용 |
| `POST /v1/control` | `{"action":"resume"}`, `pause`, `stop` |

관리 프로그램은 콘솔 없이 `프로젝트연결.exe --resume`, `--pause`, `--stop`을 실행할 수 있습니다. 연결이 꺼져 서버가 없으면 `--resume`으로 다시 시작합니다. 서버 heartbeat는 0.1초 주기로 갱신합니다. 파일 시간 또는 프로세스 존재만으로 연결 성공을 판단하지 않고 인증된 status 응답과 최근 heartbeat를 확인합니다.

MCP 설정은 다음 항목을 각 도구의 지원되는 설정에 등록합니다. Python과 경로는 설치된 config를 사용합니다. stdio 프로그램은 GitHub를 거치지 않고 같은 로컬 API에 접속합니다.

```json
{"mcpServers":{"projectbridge":{"command":"D:\\AI\\envs\\cosyvoice\\python.exe","args":["D:\\A_KJ\\AI\\Applications\\ProjectBridge\\bridge_mcp.py","--home","D:\\A_KJ\\AI\\Applications\\ProjectBridge","--issuer","Codex-audiobook"]}}}
```

도구는 `pc_status`, `pc_submit`, `pc_result`입니다. submit은 작업 ID를 즉시 돌려줍니다. 긴 작업은 `start_process`로 시작하고 실제 `processId`로 `process_status`를 확인합니다. 임의 프로세스 번호나 이름으로 종료하지 않고 `stop_process`로 등록된 작업만 중지합니다.

`--issuer`와 `--tool-id`는 같은 옵션입니다. 제품명 하나로 모든 GPT 담당자를 묶지 않고 `Codex-audiobook`, `Codex-manager`, `Jev-assets`처럼 담당자 인스턴스별 ID를 지정합니다. 생략하면 MCP 연결마다 `mcp-<uuid>`를 발급합니다. `pc_submit`의 `toolId`로 명시할 수도 있습니다. 직접 HTTP 제출은 toolId를 생략할 수 없습니다. 기존 v2 비공개 큐의 생략된 ID는 호환용 `ProjectBridge`로 취급합니다.

## 작업

```json
{"id":"project-task-001","target":"PC","deviceId":"설치된_기기_ID","projectId":"Control-Tower","toolId":"Codex","action":"capabilities","createdAt":"2026-10-06T14:00:00Z","expiresAt":"2026-10-07T14:00:00Z","args":{},"dependsOn":[],"resourceKeys":[]}
```

`id`는 최대 80자 영숫자·하이픈·밑줄입니다. `projectId`와 `toolId`는 최대 120자입니다. 같은 ID와 같은 전체 요청은 한 번만 실행합니다. 다른 내용으로 ID를 재사용하면 거절합니다. MCP는 ID별 원본 요청을 로컬에 보관하여 재시도의 시간 필드도 유지합니다.

`dependsOn`은 선행 작업이 완료된 뒤에만 실행합니다. 실패·중단된 작업의 후속 작업은 실행하지 않습니다. 순환 의존은 거절하며, 없는 선행 작업은 대기합니다. 대기 중 유효기간이 지나면 실제 명령을 실행하지 않습니다. 작업 수명은 최대 14일입니다.

최대 250개 작업을 대기·실행할 수 있고, 기본 worker는 4개입니다(config `maxWorkers`, 1~16). 프로젝트와 담당자마다 `state/workspaces/<project 해시>/<toolId 해시>`를 사용합니다. cwd를 생략한 명령은 이 전용 폴더에서 실행합니다. 프로세스·출력·파일 백업·개별 저널은 `state/general/actors/<project 해시>/<toolId 해시>`에 둡니다. 이는 작업 분리를 위한 규칙이며 서로 다른 Windows 보안 계정은 아닙니다.

독립 작업은 같은 프로젝트에서도 병렬로 실행합니다. 같은 파일 읽기/쓰기, 같은 명령 cwd, 같은 processId와 모든 화면 작업은 자원별로 직렬 실행합니다. 공유 빌드 폴더·설정 파일 등 추가 충돌이 예상되면 두 요청에 같은 `resourceKeys`를 지정합니다. 별도 detached 장기 작업이 수정하는 자원은 호출자가 의존 관계로 관리해야 합니다. v2의 `state/general/processes`에서 실행 중인 작업은 이동하지 않으며 호환용 Control-Tower/ProjectBridge 담당자만 기존 소유권 검사로 조회·중지할 수 있습니다.

지원 동작: `capabilities`, `read_file`, `write_file`, `list_dir`, `run_command`, `start_process`, `process_status`, `stop_process`, `ui_control`. 쓰기는 `expectedSha256`(새 파일은 null)을 요구하며 기존 파일을 백업합니다. Desktop 경로는 쓰기 대상으로 금지합니다. UI 동작은 `screen_capture`, `window_list`, `focus_window`, `mouse_click`, `mouse_scroll`, `type_text`, `send_keys`입니다. 이미지·입력 텍스트는 공개 기록에 남기지 않습니다.

프로세스 소유권은 launch ticket, nonce, PID와 커널 생성 시간으로 확인합니다. 연결 프로그램이 종료되어도 등록된 detached 장기 작업은 독립 소유자에게 남습니다. 재시작 시 실행 중이던 요청은 `interrupted`로 기록하고 자동 재실행하지 않습니다. 로컬의 영속 저널과 결과를 먼저 기록하며 GitHub 게시 실패는 실제 작업을 재실행하는 이유가 되지 않습니다.

Windows에서는 생성 시간이 남아 있는 종료된 프로세스와 실행 중인 프로세스를 구분하여 커널 대기 상태도 확인합니다. JSON 상태 파일을 읽는 도중 DELETE 공유가 거절되는 경우 교체를 잠깐 재시도합니다. 개별 기록을 덮어쓰는 임시 파일 이름은 고유하게 만듭니다.

## 원격 중계와 공개 기록

원격 GPT는 인증된 GitHub 도구가 비공개 저장소에 접근할 수 있을 때만 중계 요청을 보냅니다. 비공개 `remote/pc-bridge` 브랜치의 `_pc_bridge/queue.json`, `_pc_bridge/devices`, `_pc_bridge/results`를 유지합니다. 공개 저장소에 상세 요청을 올리지 않습니다.

PC의 로컬 작업은 중계 연결 실패에도 계속 작동합니다. GitHub 메타데이터 URL은 `/repos/owner/repository`이며 빈 경로 뒤에 `/`를 붙이지 않습니다. 인증 실패, 저장소 접근/존재 문제, 브랜치 문제와 queue 파일 부재를 구분합니다. 404만으로 인증 실패라고 확정하지 않습니다. gh/git의 기존 자격 증명을 사용하고 토큰을 출력하지 않습니다.

GitHub로 받은 작업만 공개 통합소통에 안전한 종류·성공 여부 요약을 게시합니다. 로컬 도구 결과는 로컬에 보관하며 해당 도구의 기존 프로젝트 작업 기록을 사용합니다. 원시 파일·명령·오디오·화면·로그는 공개 채널로 보내지 않습니다.

검증 순서: 장치 기록과 ID 대조 → capabilities → 파일 읽기 → 임시 파일 SHA 조건부 수정 → 짧은 명령 → 화면 캡처 → 테스트 앱의 입력. 자동 시작과 의도적 연결 끄기, 비정상 종료 복구도 실제 PC에서 별도로 검증하고 근거를 남깁니다.
