# AI 설치·실제 확인 원본

확인 **2026-11-03 KST** (PC 증거 생성 UTC 2026-11-02 16:37:01). 정책 루트 `D:/A_KJ/AI`를 유지하며 기존 안정 설치·사용자 자료는 이동/교체하지 않았습니다. 새 작은 CLI는 `Tools/<도구>/<버전>`, 사용자 입구는 `Launchers`로 분리했습니다. npm/공식 updater 환경은 기본 위치를 유지합니다.

## 이번 설치와 제한된 검사

| 도구 | 버전/실제 위치 | 확인 결과 |
|---|---|---|
| jq | 1.8.2 · `D:/A_KJ/AI/Tools/jq/1.8.2/jq.exe` | 한글 JSON 추출: exit0, 한글 정상 |
| ruff | 0.17.3 · `D:/A_KJ/AI/Tools/ruff/0.17.3/ruff.exe` | 미사용 import F401 검출: 예상/실제 exit1, 자동 fix 없음 |
| agent-desktop | 1.0.2 · `D:/A_KJ/AI/Tools/agent-desktop/1.0.2/agent-desktop.exe` | version/help exit0만 확인; 실화면 읽기/입력/기능 UI 검증 0회 |
| PSScriptAnalyzer | 1.25.0 · `D:/A_KJ/AI/Tools/PSScriptAnalyzer/Modules/PSScriptAnalyzer/1.25.0/PSScriptAnalyzer.psd1` | PSAvoidUsingWriteHost 경고 1건 검출, exit0 |

네 도구 모두 무료 로컬 도구입니다. 공식 배포 asset/checksum와 대조했습니다. **동일 발행처 checksum 일치는 별도 독립 공급망 출처 보증이 아닙니다.** 설치는 전체 프로젝트 검사나 실제 업무/화면 E2E 검증이 아닙니다.

## 출처와 무결성

- **jq 1.8.2**: [공식 asset](https://github.com/jqlang/jq/releases/download/jq-1.8.2/jq-windows-amd64.exe) · [검증 metadata/checksum](https://github.com/jqlang/jq/releases/download/jq-1.8.2/sha256sum.txt).
  - archive SHA256: `a6fc67fedaf9128a3309a1e2ebb8b986aeccf70122ee46d2cb4849e423f0c627`
  - executable SHA256: `a6fc67fedaf9128a3309a1e2ebb8b986aeccf70122ee46d2cb4849e423f0c627`
- **ruff 0.17.3**: [공식 asset](https://github.com/astral-sh/ruff/releases/download/0.17.3/ruff-x86_64-pc-windows-msvc.zip) · [검증 metadata/checksum](https://github.com/astral-sh/ruff/releases/download/0.17.3/ruff-x86_64-pc-windows-msvc.zip.sha256).
  - archive SHA256: `a84d9ac74af8ef5bac84a1888a1634df18e6e985e2f930d35c052beacd00e813`
  - executable SHA256: `1447a6a20b86e5d40bfebb0c2e56f8b929e2c30b0131f9ab1b5afbc826f5a11b`
- **agent-desktop 1.0.2**: [공식 asset](https://github.com/vercel-labs/agent-desktop/releases/download/v1.0.2/agent-desktop-v1.0.2-x86_64-pc-windows-msvc.tar.gz) · [검증 metadata/checksum](https://github.com/vercel-labs/agent-desktop/releases/download/v1.0.2/checksums.txt).
  - archive SHA256: `2186c76e7df197c0e67365e45d36d869f74714c56bb8a73817e2a66f3e18e222`
  - executable SHA256: `409e58cbdd6aed47e29771ffa60bc122a9843505a227af488cded85215943988`
- **PSScriptAnalyzer 1.25.0**: [공식 asset](https://www.powershellgallery.com/api/v2/package/PSScriptAnalyzer/1.25.0) · [검증 metadata/checksum](https://www.powershellgallery.com/api/v2/Packages(Id='PSScriptAnalyzer',Version='1.25.0')).
  - archive SHA256: `14e634c828eb98efb9f40b2918ba90f139ed5eccdf663a2a747736d996995d60`
  - module manifest SHA256: `2b219f688bcdd67101040f845e530b22907d39bbf49089b4b5b2afaba7996791`

## 사용자 실행 입구 (상시 실행 아님)

- `D:/A_KJ/AI/Launchers/개발도구_콘솔.cmd` — 원할 때 여는 PowerShell 개발 콘솔.
- `D:/A_KJ/AI/Launchers/개발도구_환경.ps1` — 해당 콘솔 세션에만 PATH/모듈 경로를 더함. profile·global PATH·global module·ExecutionPolicy 변경 없음.
- 기본 소스는 별도로 만든 clean clone `D:/A_KJ/AI/Projects/project-operations-hub-current`입니다. main 브랜치·clean 상태·GitHub origin을 확인했고, 변경하지 않은 콘솔 환경을 기본 Windows PowerShell 정책으로 로드해 이 D 경로 선택과 jq1.8.2/Ruff0.17.3/PSScriptAnalyzer1.25.0 해석을 확인했습니다. 기존 dirty D 저장소는 그대로 보존합니다. 미존재 시 fallback은 이번 Codex `work/hub-cleanup`입니다. 최종 문서의 원격 main 동기화는 별도 확인합니다.
- 콘솔 환경 합성 검사 **3/3**, 기본 Windows PowerShell 정책 로드 exit0. 실제 interactive 콘솔을 열거나 UI 작업은 하지 않았습니다.

## 기존 관리 앱 보존

설치 앱 `D:/A_KJ/AI/Applications/AIControlTower`의 **0.9.18.0**과 고정 source `a1bb5c4d376a05dc62b003a510a88012d5a466e6`을 보존했습니다. 기존 launcher `-CheckOnly` identity 검사 exit0/started:false만 확인했습니다. source 계열의 버전 표기와 설치 파일버전은 같다고 가정하지 않으며 낮은 main 빌드로 덮지 않습니다.

`D:/A_KJ/AI/Launchers/통합관제탑.lnk`는 기존 `D:/A_KJ/AI/Applications/AIControlTower/START_LATEST_VIEW.vbs`를 가리킵니다. 바로가기/VBS는 실행하지 않았습니다. 기존 VBS 내부의 ExecutionPolicy Bypass는 사전 구성으로 이번 편집/실행하지 않았으며 **새 개발 콘솔은 Bypass를 사용하지 않습니다**.

## 재사용한 환경

Codex0.160.1, Git2.50.0.windows.1, gh2.100.0, Node24.16.0/npm11.13.0, rg15.2.0, uv0.12.13, .NET SDK9.0.306, Python `py -3.13` (`C:/TempPy13/python.exe`), 기존 Python Playwright1.63.0/npm Playwright MCP0.0.80/Chromium cache1234를 확인·재사용했습니다. fd/Gitleaks도 기존 PATH에서 발견해 재설치하지 않았으며 이번 전체 기능 재검증은 아닙니다. Codex 업그레이드 없음.

## 변경하지 않은 것

새 결제·구독·유료 모델 호출 **0**, 실화면 동작 **0**. global PATH/module/profile·ExecutionPolicy·repo trust·autostart 변경 없음. 기존 앱 교체·서비스 종료·재부팅 없음. optional agent-desktop skills/integration 추가 없음. 현재 PC 연결은 ProjectBridge 경로이며 원격 연결을 임의 종료하지 않습니다. 옛 2분 예약/승인 생략 실행기·구형 shared-PC installer/updater를 활성화하지 않았습니다.

## 이전 프로젝트별 설치 관찰 (이번 재실행/현재 버전 검증 아님)
| Tool | Version | Current / Target | 상태/이유 |
|---|---:|---|---|
| MoneyPrinterTurbo | 1.3.7 | 현재 staging `C:\KJ\Tools\AI\Applications\MoneyPrinterTurbo\1.3.7` → 목표 `C:\Program Files\_My\AI\Applications\MoneyPrinterTurbo\1.3.7` | 구조 정리 완료, Program Files 이동은 관리자 승인 필요 |
| n8n | 2.38.7 | `%APPDATA%\npm` + npm global modules | npm 기본 관리 위치 유지 |
| HyperFrames | 0.8.40 | `%APPDATA%\npm` + npm global modules | npm 기본 관리 위치 유지 |
| Aider | 0.86.2 | `%USERPROFILE%\.local\bin` + installer-managed Python env | updater/환경 관리 경로 유지 |
| jev-router | 0.2.0 | `C:\Users\user\AppData\Roaming\npm` | npm 기본 위치 유지. upstream gargpratyush/jev-router 확인. 사용자 환경의 키 존재와 Codex ChatGPT 로그인 확인. 사용 허용/선택 사용이며 인증·라우팅 왕복·업무 성공은 미검증 |
| Unity iOS Build Support | 6000.3.14f1 | C:/Program Files_My/A_3D/Unity_Hub/Unity_Editor/6000.3.14f1/Editor/Data/PlaybackEngines/iOSSupport | 2026-09-25 공식 Unity CLI 설치 성공. 기존 Unity Editor 종속 모듈이므로 공식 설치 위치 유지. Windows에서 Xcode 프로젝트 내보내기용이며 Mac/Xcode는 별도 필요. 원격 에디터 실행은 PhoneLoL_02/Automation/OpenUnity.ps1 사용 |

| 무직전생 AI 오디오북 | VoxCPM2 원본참조 / 록시 CosyVoice3 선호 유지 | D:/AI/VoiceAudiobook; 환경 D:/AI/envs; 추가 모델 D:/A_KJ/AI/Models, 검토 환경 Workspace/VoiceBench | 현재 실행기 무직전생.exe / 오디오북제작.exe. 본편15~24 준비128회차, 완성 수는 실제 로컬 상태 확인 필요. 원본참조 새 판과 이전 판 분리. 공용 PC 연결 복구 확인, 이번 연결·관리 작업으로 오디오 제작 완료를 주장하지 않음. [공용 연결 현재 상태](../04_COMMUNICATION/remote-bridge/README.md) |


기존 n8n localhost bridge/ProjectBridge 왕복 등 이전 검증은 당시 기록이며 현재 연결/업무 성공이 아닙니다. Jev 설치·키 존재와 실제 인증/라우팅/비용 절감도 별개이며 새 유료 호출을 하지 않습니다.

[사람용 사용법](../000_사용자용/02_프로그램/설치_도구_현황.md) · [채택 기준](TOOLS.md).
