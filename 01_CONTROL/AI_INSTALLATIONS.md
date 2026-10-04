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
| AI Control Tower | 0.2.0 | `D:\A_KJ\AI\Applications\AIControlTower\AIControlTower.exe`; 소스 `D:\A_KJ\AI\Projects\project-operations-hub\ai-control-tower` | 새 프로젝트 작업실·owned Windows job·공유 리모트 중계. 설정/작업 기록/백업은 `%LocalAppData%\AIControlTower` 유지. 옛 LocalAppData EXE는 복구용 보존. 실제 최종 검증/해시는 ai-control-tower/docs/VERIFICATION-20261005.md |
| Unity iOS Build Support | 6000.3.14f1 | C:/Program Files_My/A_3D/Unity_Hub/Unity_Editor/6000.3.14f1/Editor/Data/PlaybackEngines/iOSSupport | 2026-09-25 공식 Unity CLI 설치 성공. 기존 Unity Editor 종속 모듈이므로 공식 설치 위치 유지. Windows에서 Xcode 프로젝트 내보내기용이며 Mac/Xcode는 별도 필요. 원격 에디터 실행은 PhoneLoL_02/Automation/OpenUnity.ps1 사용 |

| 무직전생 AI 오디오북 | CosyVoice3 / Qwen3-TTS / Ollama 기존 설치 | D:\AI\VoiceAudiobook; 환경 D:\AI\envs; 문맥 모델 D:\AI\models\ollama | 사용자 명시 D드라이브 예외. 실행 RUN_Audiobook.bat. 기존 자료·환경 재사용, Ollama 12파일 SHA-256 및 실제 추론 검증. 상세 위치는 프로젝트 docs/RUNTIME_LOCATIONS.md |

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

