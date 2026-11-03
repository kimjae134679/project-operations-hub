# AI 설치·실제 확인 원본

## 최신 실제 테스트 · 2026-11-03

[실제 테스트 결과와 실패/차단 구분](../000_사용자용/02_프로그램/도구_실제테스트_결과_20261103.md)이 아래 설치단계의 “도움말만/실화면0/모델미실행” 기록보다 후속입니다. 전부PASS가 아닙니다.

- 유틸리티/Replica helper/등록된Apify문서MCP: **24 PASS/0 FAIL**, 그 안에 실제 helper18 fixture 포함. Actor는BLOCKED/AUTH_REQUIRED, Replica11모델 workflow는NOT_RUN.
- Git/rg/fd/uv/Gitleaks/Playwright 자체fixture6기능 PASS. Playwright초기 기본1243 누락 FAIL과 explicit1234PASS는 이력으로 남기고, 공식1243/headless1243의존성을 추가한 뒤 **기본경로override없는 한글fill/click/DOM PASS**를 확인했습니다. sandbox=true/network0, 기존cache611파일hash동일/1234보존. Python/Playwrightpackage업그레이드나 기본cache이동은 없음.
- agent-desktop1.0.2: 자체 격리창의 한글 입력/버튼/새앱결과 조회 **기능 PASS**. 다만 strict-headless에서도 is_focused false→true여서 **전경 비간섭 FAIL**, 정확한원인 미확정. 자체창 정상 종료/PID소멸PASS, 다른앱조사/강제 포커스 복구없음.
- Codex0.160.1: 기존 요금제 실제 응답·turn완료·exit0는 확인했지만 **한글JSON파일과제는policy거부로BLOCKED**. 성공한합계나 반환실제modelID증거는없으며 gpt-6.1-sol은요청값입니다.
- T3/OpenMaus기동의 이전거부는 재시도하지 않았고 GUI/provider/폰E2E는 여전히미검증입니다. 원문모델trace/privateconfig/비밀값은공개하지않습니다.



확인 **2026-11-03 KST** (PC 증거 생성 UTC 2026-11-02 16:37:01). 정책 루트 `D:/A_KJ/AI`를 유지하며 기존 안정 설치·사용자 자료는 이동/교체하지 않았습니다. 새 작은 CLI는 `Tools/<도구>/<버전>`, 사용자 입구는 `Launchers`로 분리했습니다. npm/공식 updater 환경은 기본 위치를 유지합니다.

## 최신 추가 도구 · T3 Code 0.1.6 (이전 사진7장과 별개)

새 이미지의 공식 `pingdotgg/t3code`를 검토하고 Windows CLI와 데스크톱 프로그램을 실제 설치했습니다. 고정source `2359643cffa6ba9ee84d6f2694b935de54a8013e`/tagv0.1.6, MIT. [공식 release](https://github.com/pingdotgg/t3code/releases/tag/v0.1.6) · [한글 실행/거부사유 안내](../000_사용자용/02_프로그램/T3_Code_설치와_사용.md).

- CLI: `D:/A_KJ/AI/Applications/T3Code/cli/0.1.6/t3-0.1.6-win32-x64/t3.exe`. 공식 WindowsZIP SHA256 `486604df780a72273fac94506e21edda6cab762cd4417393007c2953a44e609f`를 [SHA256SUMS](https://github.com/pingdotgg/t3code/releases/download/v0.1.6/SHA256SUMS)/assetdigest와 대조. EXE SHA256 `00144af15877882de94dc6eb431c63c40d6e6af54d543abd2ba40dbadbdd5010`, Authenticode **Valid/T3 Tools Inc**. 실제 hidden `--version`=`t3 v0.1.6`/`--help` exit0.
- GUI: `D:/A_KJ/AI/Applications/T3Code/desktop/0.1.6/T3 Code (Beta).exe`, FileVersion0.1.6, AuthenticodeValid. 공식 [Windows NSIS](https://github.com/pingdotgg/t3code/releases/download/v0.1.6/T3-Code-0.1.6-x64.exe) SHA256 `8cff5c90fdc976c0bdef86074a8cde9c3784af8de2091759514cd9f8f1457405`와 GitHubdigest/SHA512latest.yml을 대조했습니다. 전체 payload53개 파일/704710031 bytes를 기존7-Zip으로 추출했고 installer·GUI는 실행하지 않았습니다. 이름이 비슷한 macOS x64ZIP은 쓰지 않았습니다.
- 수동 입구: `D:/A_KJ/AI/Applications/T3Code/T3Code.lnk`/`T3Code.vbs`, `T3Code_CLI.lnk`/`T3Code_CLI.vbs`. 전용 D 데이터를 고르는 준비물이며 **미실행**입니다. 기존 거부를 우회하려 실행하지 않았습니다.
- staged 설정: `D:/A_KJ/AI/Applications/T3Code/data/userdata/settings.json`. 공식 초기 Full access 대신 **defaultRuntimeMode=approval-required(Supervised)**, 기본/텍스트모델 `gpt-6.1-sol`/codex, fallback=null, 기존 native Codex 경로/HOME을 준비했습니다. 기존 인증 복사/새 thread/model호출0. source-schema/정적 검사만이며 실제 T3 runtime readback은 없습니다. `status --json`의 CLI 설정 출력은 provider연결/서버권한 적용 확인이 아닙니다.
- Codex 호환: [공식 고정 manifest](https://github.com/pingdotgg/t3code/blob/v0.1.6/apps/server/src/provider/model-manifest.json)의 최소 0.156.0/권장 0.159.0이상에 기존 0.160.1이 맞습니다. **버전 조건 적합이지 T3의 실제provider인증/모델 응답 성공이 아닙니다.**
- 실행 제한: 일반 공식 `serve --host127.0.0.1 --port37731 --base-dir<전용D> --no-browser` 시작은 execution tool이 **CreateProcess 전에 거부**했습니다. PID/포트/서버 health/인증/GUI/폰 연결 없음, 다른 경로 재시도 없음. 다운로드만 하려던 install.ps1 검사도 거부돼 다른 방식으로 그 스크립트를 가져오지 않았습니다. 별도 사전선택한 공식 portable 설치와 구분합니다.

수동 래퍼는 process-only `T3CODE_TELEMETRY_ENABLED=false`, `OTEL_SDK_DISABLED=true`를 설정하도록 준비했고 source/정적·합성 검사 **3/3**을 통과했습니다. ClaudeAgent 기본 항목은 명시적으로 disabled하여 Codex-only 구성을 준비했습니다. wrapper/설정은 실제로 실행·readback하지 않았습니다. 최종 SHA256: GUI wrapper `f0543489f525d45a901794605b932f6c8ae987ffcfa49a0e6c167bc41ec008a8`, CLI wrapper `b7a5b1c8f5e110117e8844e9a22b226e3deb3ce5745c161a7549f1c0a34cd4ba`, 전용 settings `97c008314a18d5aaee8a9df01b53b6cbfba329cda56c67b09ad7aba6db92d719`.

읽기 전용 `t3 status --json --base-dir <전용D>` exit0에서 serviceInstalled=false/connectEnabled=false/lanEnabled=false를 확인했습니다. 이 명령은 DB/연결 검사를 하지 않는 CLI 설정 조회이며 **서버 health·Supervised/model 설정 적용·Codexprovider 감지/인증·모델 가용성의 증거가 아닙니다**. raw 출력/연결 정보는 비공개로 보존했습니다.

정확한 내부 거부 규칙은 미제공이며 malware/UAC/AV 판정은 관찰되지 않았습니다. 서명·hash확인은 손상/발행자 확인이지 전체 안전·동작보증이 아닙니다. 전역 CLI/Node/서비스/autostart/firewall/tunnel/보안 정책·키/계정/결제 변경과 model호출0. `t3 doctor`는 DB유지보수 명령으로 health검사에 쓰지 않았습니다.

## 이전 추가 설치 · 후속 “설치” 지시 반영

이 표가 초기 조사/1차 CLI 정리 때의 미도입 상태보다 우선합니다. 설치·Codex 인식·MCP 연결·정상 앱 시작·사용자 흐름 완료를 구분합니다. [사용자 실행 안내](../000_사용자용/02_프로그램/사진도구_설치와_실행.md).

| 도구 | 실제 설치/설정 | 검증 범위 |
|---|---|---|
| Replica Skill11개 | `C:/Users/user/.codex/skills/replica-*`; 공식 source `77c9436fb3d18c3d58169efb8caf4fe906b0dc51`, staging `D:/A_KJ/AI/Tools/replica-skill/77c9436fb3d18c3d58169efb8caf4fe906b0dc51` | 네이티브 skills/list11개 enabled/오류0. 로컬 helper 검사 18/18. 앱 복제/모델 동작 E2E 미실행 |
| agent-desktop Windows Skill | `C:/Users/user/.codex/skills/agent-desktop-windows`; tagv1.0.2 source `a47ed664e0db00d1d4df364eb3fe8a5599882f3c` | 기존binary1.0.2 재사용, 네이티브 enabled 인식·지침/버전 확인. 화면/클립보드/입력0회 |
| Apify MCP0.20.0 | `D:/A_KJ/AI/Tools/apify-mcp-server/0.20.0`; local lockfile+271packages; Codex MCP `apify-docs` enabled 등록 | 직접/실제PS launcher 초기화·정확히문서 도구 2개·공식 문서6,456자 조회. Actor/auth/데이터셋 수집 미실행 |
| OpenMausBot 0.1.138 | `D:/A_KJ/AI/Applications/OpenMausBot/0.1.138/OpenMausBot.exe`; portable payload970272192bytes | 배포hash/버전/필수 entry 파일 확인. 다음 수동 시작 wrapper정적·합성3/3. 정상 GUI/로그인/모델 응답 미검증 |

### Codex Skill 설치 범위

Replica 11개개와 Windows 1개개를 `skills/list(forceReload)`에서 **12개 enabled/오류0**으로 확인했으며 새 턴/세션 갱신 후 선택할 수 있습니다. 공식 Skill 설치 도구로 빈 대상에 설치해 기존 항목을 덮지 않았습니다. Replica Python6개/템플릿/지침과 Windows5개지침·참고를 확인했습니다. 실제 결제/환불을 기본 요구하는 지침·권한 상승/보안 우회 조언은 제한·제거했고, 단계 전환 권고가 자동 실행 권한이 아님을 추가했습니다. 자동 model 호출·global hook·AGENTS 덮어쓰기 없음.

### Apify: 문서 모드는 연결됨, Actor 모드는 인증 대기

공식 `@apify/actors-mcp-server@0.20.0`을 `--ignore-scripts --no-audit --no-fund --save-exact`로 설치했습니다. lifecycle/build script0, 게시된 dist로 실행했습니다. Node 24.16.0 재사용(요구>=22). tokenless **`search-apify-docs`, `fetch-apify-docs` 두 개만** 등록했습니다. root가 기존 MCP 11개와 동등한 구성을 보존하고 private backup 후 `apify-docs`를 추가했습니다. CLI 직렬화에서 default true/빈 args가 생략된 것 외 의미 차이가 없음을 확인했고 backup/비밀값은 공개 저장소에 넣지 않았습니다.

Docs child는 APIFY_TOKEN을 비우고 `NoAuthHome`을 사용해 기존 CLI auth파일 자동 fallback을 막으며 telemetry/Sentry는 false입니다. stdio initialize/tools-list는 outbound 차단 guard에서0시도/exit0, 실제 PS launcher도 성공했습니다. 공식 `https://docs.apify.com/actors/running` 조회6,456자, SHA256 `3eaf92e8c2eccb457438b69008a5e2b9fd4ce5903851e3eddb196c09101e738d`. 첫 구형URL의301은 테스트 guard가 막았으며 같은 공식 host의 canonical 경로를 확인해 성공했습니다. Actor/데이터셋/유료 model 호출0. Discovery는 세션APIFY_TOKEN 없으면 launcher exit3; default Actor server는 missing-auth exit1로 종료합니다. account/OAuth/결제 없이 임의 수집을 켜지 않았습니다.

수동 입구 `D:/A_KJ/AI/Tools/apify-mcp-server/0.20.0/Start-ApifyMcp.cmd`와 `Launch-ApifyMcp.ps1` 기본은 Docs stdio입니다. 인증 Discovery 예제는 disabled template으로 남겼고 call-actor를 노출하지 않습니다. registry tarball SHA512 metadata 대조/lockfile 고정은 했지만 독립 공급망 보증·취약점 전체 감사는 아닙니다.

### OpenMausBot: 프로그램 설치와 미실행 경계

공식 NSIS의 app-64.7z를 기존7-Zip으로 추출한 **실제 portable Electron 전체 파일 설치**입니다. installer 실행·uninstall 등록·로그인 시작은 없었습니다. 공식 installer SHA256 `40cf0100333d012a5b5766a6f35a24fcc637ff24320ff0e09d59b02bc9b7ed44`는 GitHub assetdigest, SHA512는 latest.yml과 대조했습니다. EXE SHA256 `4350b98e165e2269a01148b6b9baf6f4ad0152abb8dd66a872706e037b00cf67`; FileVersion0.1.138/ProductVersion0.1.138.0. unsigned이며 보안 설정을 낮추지 않았습니다.

주 입구 `D:/A_KJ/AI/Applications/OpenMausBot/OpenMausBot.lnk` → `OpenMausBot_D-data.vbs`는 문서화된 process-only `OMB_DATA_DIR=D:/A_KJ/AI/Applications/OpenMausBot/data`로 정상EXE를 다음 사용자 수동 시작 때 열도록 준비했습니다. staged config의 기존 `C:/Users/user/AppData/Roaming/npm/codex.cmd`/ChatGPT-plan/`gpt-6.1-sol`, automaticRecovery/cacheKeepWarm=false는 **미활성·미인증**입니다. credentials 복사 없음. 별도 직접실행 링크는 vendor 기본 데이터 경로를 쓰므로 구분합니다.

별도 동봉 CLI help는 상대 모듈 누락으로 exit1이었으며 정상 GUI/번들 backend 고장으로 확대하지 않습니다. 안전한 hidden backend 실행 시도는 도구 정책에서 **CreateProcess 전에 거부**되어 실행/포트개방/재시도 없었습니다. 그 거부를 우회하려 래퍼를 실행하지 않았습니다. 정상 GUI·CUA·화면·로그인·모델 응답은 모두 미실행. 정상 첫 실행은 CUA driver·Composio 익명 설치등록을 시작할 수 있어 별도 확인이 필요합니다. Boat/Composio OAuth·계정/결제 생성 없음.

공식 출처: [Replica 고정source](https://github.com/Jakeschincariol/replica-skill/tree/77c9436fb3d18c3d58169efb8caf4fe906b0dc51), [Windows Skill tag](https://github.com/vercel-labs/agent-desktop/tree/v1.0.2/skills/agent-desktop-windows), [Apify 0.20.0](https://github.com/apify/apify-mcp-server/releases/tag/v0.20.0), [OpenMaus0.1.138](https://github.com/milind-soni/OpenMausBot/releases/tag/v0.1.138). 같은 발행처 hash 일치는 독립 출처 보증이 아닙니다.

## 1차 설치와 제한된 검사 · 초기 CLI 정리

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

Codex 0.160.1, Git2.50.0.windows.1, gh2.100.0, Node 24.16.0/npm11.13.0, rg15.2.0, uv0.12.13, .NET SDK9.0.306, Python `py -3.13` (`C:/TempPy13/python.exe`), 기존 Python Playwright1.63.0/npm Playwright MCP0.0.80/Chromium cache1234를 확인·재사용했습니다. fd/Gitleaks도 기존 PATH에서 발견해 재설치하지 않았으며 이번 전체 기능 재검증은 아닙니다. Codex 업그레이드 없음.

## 1차 안전 경계와 후속 변경 구분

새 결제·구독·유료 모델 호출 **0**, 실화면 동작 **0**. global PATH/module/profile·ExecutionPolicy·repo trust·autostart 변경 없음. 후속 허용 변경은 사용자 Skill12개와 Codex apify-docs MCP 추가이며 기존 MCP는 보존했습니다. 기존 앱 교체·서비스 종료·재부팅 없음. 초기1차에는 optional Skill/MCP를 추가하지 않았으나 후속 지시로 위의 Skill12개와 Apify Docs MCP를 실제 추가했습니다.  현재 PC 연결은 ProjectBridge 경로이며 원격 연결을 임의 종료하지 않습니다. 옛 2분 예약/승인 생략 실행기·구형 shared-PC installer/updater를 활성화하지 않았습니다.

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
