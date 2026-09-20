# AI Installations / Work Root

기본 관리 루트: `C:\Program Files\_My\AI`

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
| jev-router | 0.2.0 | `C:\Users\user\AppData\Roaming\npm` | npm 전역 기본 위치 유지. `jev-codex.cmd`, `jev-claude.cmd` 설치 확인. API 키 미설정으로 비활성·실제 라우팅 미검증 |

## 인수인계 필수 항목
모든 인수인계에는 `AI 설치/작업 위치`, `관리 루트 준수/예외`, `실제 실행 진입점`, `이동 시 갱신해야 할 경로`를 적습니다.

## 현재 마이그레이션 메모
`C:\KJ\Tools\AI`는 새 구조와 동일한 폴더 체계로 임시 staging 정리되었습니다. `Scripts\Install\migrate-ai-root.ps1`가 준비되어 있으며, 관리자 승인을 받아 최종 루트로 복사·검증한 뒤 staging을 정리합니다.

## n8n local bridge
- Workflow: `AI Ops Hub - Local Task Bridge`
- Workflow ID: `aiOpsBridge001`
- Local endpoint: `POST http://127.0.0.1:5678/webhook/ai-ops-task`
- Helper: staging `C:\KJ\Tools\AI\Scripts\Runtime\Send-N8nTask.ps1`
- 검증: n8n 재시작 후 helper POST → `{ ok: true, source: local-n8n-bridge }` 응답 확인
- 보안: localhost 전용. 외부 tunnel/public webhook은 별도 승인·인증 설계 전에는 열지 않습니다.

| AI Control Tower | 0.1.0 | 소스: project-operations-hub\\ai-control-tower, publish: rtifacts\\win-x64\\AIControlTower.exe | .NET 9 WPF로 생성. 앱 설치 시 C:\\Program Files\\_My\\AI\\Applications\\AIControlTower 우선, 권한 부족 시 %LocalAppData%\\AIControlTower 사용 |
