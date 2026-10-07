# AI Installations / Work Root

새 AI 작업 기본 관리 루트: `D:\A_KJ\AI` (2026-10-05 최신 사용자 지시/실제 경로 반영). 기존 안정 설치는 일괄 이동하지 않습니다.

## 폴더 구조
- `Applications/` — AI가 직접 관리하는 portable/독립 프로그램
- `Installers/` — 설치 원본, 압축파일, 검증한 패키지
- `Scripts/` — 설치·업데이트·복구 스크립트
- `Launchers/` — 사용자가 바로 실행하는 진입점
- `Projects/` — 새 AI 프로젝트 기본 작업 위치
- `Workspace/` — 임시 실험/검증 작업
- `External/` — 기본 위치를 유지하는 외부 설치의 포인터/설명

## 예외 원칙
패키지 매니저·공식 업데이터·런타임이 기본 위치를 전제로 하면 안정성을 우선합니다. 이 경우 억지로 이동하지 않고 실제 경로와 이유를 기록합니다.

## 현재 등록
| Tool | Version | Current / Target | 상태/이유 |
|---|---:|---|---|
| MoneyPrinterTurbo | 1.3.7 | 현재 staging `C:\KJ\Tools\AI\Applications\MoneyPrinterTurbo\1.3.7` → 목표 `C:\Program Files\_My\AI\Applications\MoneyPrinterTurbo\1.3.7` | 구조 정리 완료, Program Files 이동은 관리자 승인 필요 |
| n8n | 2.38.7 | `%APPDATA%\npm` + npm global modules | npm 기본 관리 위치 유지 |
| HyperFrames | 0.8.40 | `%APPDATA%\npm` + npm global modules | npm 기본 관리 위치 유지 |
| Aider | 0.86.2 | `%USERPROFILE%\.local\bin` + installer-managed Python env | updater/환경 관리 경로 유지 |
| jev-router | 0.2.0 | `C:\Users\user\AppData\Roaming\npm` | npm 기본 위치 유지. upstream gargpratyush/jev-router 확인. 사용자 환경의 키 존재와 Codex ChatGPT 로그인 확인. 사용 허용/선택 사용이며 인증·라우팅 왕복·업무 성공은 미검증 |
| AI Control Tower | 0.7.0 기존 배포 / 0.8.0 개편 | 실행 `D:\A_KJ\AI\Applications\AIControlTower\AIControlTower.exe`; 정식 저장소 `D:\A_KJ\AI\Projects\project-operations-hub`; 이번 소유 작업 트리 `D:\A_KJ\AI\Workspace\ControlTower\shared-pc-20261006\source` | 작업 트리 이동·대조와 최신 로컬 .NET175/175 검사 완료. 실제 PC 빌드·배포·화면·자동 시작은 진행 중. 검사·로그·배포 준비물은 같은 전용 Workspace 아래. 설정·기록·백업은 기존 위치 유지 |
| ProjectBridge / 프로젝트 연결 | 2.0 실제 설치 / 3.0 개편 | `D:\A_KJ\AI\Applications\ProjectBridge\프로젝트연결.exe` | 기기 KJW-80ea388278fd와 기존 연결 왕복 확인. 404 주소 수정. 이전 범위65 검사 통과, 최신 주체별 공간·기록·실행 단계 분리 보완은 재검증 중. 실제 새 설치·자동 시작·왕복은 별도 확인 |
| Unity iOS Build Support | 6000.3.14f1 | C:/Program Files_My/A_3D/Unity_Hub/Unity_Editor/6000.3.14f1/Editor/Data/PlaybackEngines/iOSSupport | 2026-09-25 공식 Unity CLI 설치 성공. 기존 Unity Editor 종속 모듈이므로 공식 설치 위치 유지. Windows에서 Xcode 프로젝트 내보내기용이며 Mac/Xcode는 별도 필요. 원격 에디터 실행은 PhoneLoL_02/Automation/OpenUnity.ps1 사용 |

| 무직전생 AI 오디오북 | VoxCPM2 원본참조 / 록시 CosyVoice3 선호 유지 | D:/AI/VoiceAudiobook; 환경 D:/AI/envs; 추가 모델 D:/A_KJ/AI/Models, 검토 환경 Workspace/VoiceBench | 현재 실행기 무직전생.exe / 오디오북제작.exe. 본편15~24 준비128회차, 완성 수는 실제 로컬 상태 확인 필요. 원본참조 새 판과 이전 판 분리. 공용 PC 연결 복구 확인, 이번 연결·관리 작업으로 오디오 제작 완료를 주장하지 않음. [공용 연결 현재 상태](../04_COMMUNICATION/remote-bridge/README.md) |

## 인수인계 필수 항목
모든 인수인계에는 `AI 설치/작업 위치`, `관리 루트 준수/예외`, `실제 실행 진입점`, `이동 시 갱신해야 할 경로`를 적습니다.

## 이전 마이그레이션 메모 (현재 실행 계획이 아님)
`C:\KJ\Tools\AI`는 새 구조와 동일한 폴더 체계로 임시 staging 정리되었습니다. `Scripts\Install\migrate-ai-root.ps1`가 준비되어 있으며, 이전 C루트 이관안이며 현재 정책은 D루트입니다. MoneyPrinterTurbo 등 다른 안정 설치는 이번 작업에서 이동하지 않았습니다.

## n8n local bridge
- Workflow: `AI Ops Hub - Local Task Bridge`
- Workflow ID: `aiOpsBridge001`
- Local endpoint: `POST http://127.0.0.1:5678/webhook/ai-ops-task`
- Helper: staging `C:\KJ\Tools\AI\Scripts\Runtime\Send-N8nTask.ps1`
- 검증: n8n 재시작 후 helper POST → `{ ok: true, source: local-n8n-bridge }` 응답 확인
- 보안: localhost 전용. 외부 tunnel/public webhook은 별도 승인·인증 설계 전에는 열지 않습니다.


## ProjectBridge — 공용 PC 실제 연결과 새 버전 적용

현재 D:/A_KJ/AI/Applications/ProjectBridge/프로젝트연결.exe의 2.0 설치와 실제 장치·파일·명령·창 목록 왕복을 확인했습니다. 기존 D:/AI/VoiceAudiobook과 실행 중인 제작 작업은 유지합니다. 3.0은 로컬 API·MCP·병렬 처리와 프로젝트·명령 주체별 공간·기록·실행 단계 분리를 보완·재검증 중입니다. 관리 앱 0.8.0 내부에서 설치·복구와 연결·작업 상태를 관리하고 실행용 별도 창을 없앱니다. 실제 새 설치·로그인 자동 시작·전체 왕복은 완료 기록에서 갱신합니다.

바탕화면 파일 없이 창 없는 worker·트레이로 시작하며, 기존 인증·기기 ID·자료·안정 설치와 백업을 보존합니다. 공개 기록에 비밀값·파일 원문·실제 명령·화면·상세 로그를 넣지 않습니다. [설치 및 검증](../04_COMMUNICATION/remote-bridge/README.md).

## 사용자 입구와 이번 작업 자료

사용자에게 보여주는 실행 입구는 관리 앱·프로젝트연결.exe와 프로젝트_사용안내.md입니다. 새 설치 소스는 내부 worker·MCP·helper·소스·검사·manifest를 숨김 Runtime에 모읍니다. 기존 루트 config.json·state는 경로 그대로 숨김 표시하며 실제 설정·백업·활성 작업을 보존합니다. MCP는 Runtime/bridge_mcp.py를 실행하고 --home은 기존 ProjectBridge 루트입니다. 실제 설치 검증은 진행 중입니다. 편집·미확인 파일을 임의 삭제하지 않습니다.

이번 소유 작업 트리는 `D:\A_KJ\AI\Workspace\ControlTower\shared-pc-20261006\source`로 실제 이동·대조했습니다. 검사·로그·빌드·배포 준비물은 `shared-pc-20261006` 아래에 모으고 완료 후 이번 작업에서 만든 불필요한 파일만 정리합니다. 보조 앱·CMD·PowerShell 창 없이 처리하며 바탕화면을 변경하지 않습니다. 정식 프로젝트·기존 오디오북과 실행 중 제작 작업은 보존합니다.
