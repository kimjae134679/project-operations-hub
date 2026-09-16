# AI Tool Usage & Assistance Matrix

AI/Plugin/Skill/MCP/CLI/Agent 후보를 단순히 `좋다/나쁘다`나 `설치됨/안 됨`으로만 관리하지 않고, **누가 실제로 쓰는지, AI가 혼자 끝까지 다룰 수 있는지, 사용자가 얼마나 개입해야 하는지, 추가 비용이 필요한지**를 함께 기록하는 내부 기준입니다.

사용자용 요약은 `000_사용자용/06_AI_도구_플러그인_사용구분.md`에 동기화합니다.

## 분리해서 보는 축

- `ACCESS` — `READY / CONNECT / INSTALL / REFERENCE`
- `OPERATOR` — `AI / USER / BOTH`
- `AUTONOMY` — `FULL / AFTER_SETUP / PARTIAL / USER_DRIVEN / UNKNOWN`
- `ASSISTANCE` — `NONE / ONCE / FREQUENT`
- `BLOCKER` — `LOGIN / OAUTH / UAC / API_KEY / 2FA / PAYMENT / CAPTCHA / GUI_CHECK / GPU`
- `COST` — `FREE_OSS / FREE_TIER / INCLUDED / TRIAL / PAID / UNKNOWN`
- `INSTALLED` — `YES / NO / UNKNOWN / SERVICE`
- `ADOPTION` — `ACTIVE / PROJECT / CANDIDATE / REFERENCE / RETIRED`
- `BURDEN` — `⚪ NONE / 🟢 LIGHT / 🟡 MEDIUM / 🟠 HEAVY / 🔴 VERY_HEAVY`

`ACCESS`, `AUTONOMY`, `INSTALLED`, `ADOPTION`, `COST`는 서로 다른 의미입니다.

## 사용자 비용 선호 정책

`FREE_OSS → FREE_TIER → INCLUDED → TRIAL → PAID`

- `PAID`는 사용자가 명시적으로 요청한 경우가 아니면 기본 후보/추천/설치에서 제외합니다.
- `TRIAL`도 기본적으로 피하고 무료 대안을 우선합니다.
- 카드 등록, 체험 종료 후 자동결제, 크레딧 소진 후 과금 가능성이 있으면 무료로 취급하지 않습니다.
- 가격이 확인되지 않으면 `UNKNOWN`으로 둡니다.
- 오픈소스 자체가 무료여도 외부 API/모델/클라우드 비용은 별도로 적습니다.

## 현재 세션에서 바로 활용 가능한 연결 도구

| Tool | ACCESS | OPERATOR | AUTONOMY | ASSISTANCE | COST | BURDEN | 메모 |
|---|---|---|---|---|---|---|---|
| GitHub | READY | BOTH | FULL~AFTER_SETUP | NONE~ONCE | INCLUDED | ⚪ | 노출된 repo/PR/Issue/Actions 작업은 AI가 직접 처리 가능 |
| Gmail | READY | BOTH | AFTER_SETUP | ONCE when auth needed | INCLUDED | ⚪ | 연결 후 검색/읽기/작성 가능 |
| Google Calendar | READY | BOTH | AFTER_SETUP | ONCE when auth needed | INCLUDED | ⚪ | 연결 후 일정 조회/변경 가능 |
| Google Contacts | READY | AI | AFTER_SETUP | ONCE when auth needed | INCLUDED | ⚪ | 수신자/참석자 확인 |
| Google Drive | READY | BOTH | AFTER_SETUP | ONCE when auth needed | INCLUDED | ⚪ | 연결 후 Drive/Docs/Sheets/Slides 작업 가능 |
| Notion | READY | BOTH | AFTER_SETUP | ONCE when auth needed | INCLUDED/UNKNOWN external plan | ⚪ | 현재 연결 범위 내 작업 가능 |
| Supabase | READY | AI | AFTER_SETUP | ONCE when permission needed | INCLUDED/UNKNOWN external plan | ⚪ | 현재 연결 범위 내 DB/프로젝트 작업 가능 |
| Remote Desktop Commander | READY when PC online | AI | PARTIAL~AFTER_SETUP | ONCE/FREQUENT for blockers | 기존 설치 | 🟢 | 일반 파일/명령은 AI 처리 가능. UAC/CAPTCHA/특정 GUI는 사용자 필요 가능 |
| ChatGPT Files / Library | READY | BOTH | FULL | NONE | INCLUDED | ⚪ | 파일/라이브러리 검색·읽기 |

## 무료 우선으로 확인된 개발 도구

| Tool | ACCESS | AUTONOMY | COST | BURDEN | 주의 |
|---|---|---|---|---|---|
| ripgrep (`rg`) | INSTALL | FULL | FREE_OSS | 🟢 | 로컬 문자열 검색은 AI가 독립 운용하기 좋음 |
| fd | INSTALL | FULL | FREE_OSS | 🟢 | MIT/Apache-2.0. 파일·폴더 경로 검색용. rg와 보완 관계 |
| jq | INSTALL | FULL | FREE_OSS | 🟢 | MIT. JSON을 CLI에서 slice/filter/map/transform. 공식 prebuilt standalone binary 제공 |
| Gitleaks CLI | INSTALL | FULL | FREE_OSS | 🟢 | 로컬 CLI 기준. `gitleaks-action` 조건과 분리 |
| uv | INSTALL | FULL | FREE_OSS | 🟢 | MIT OR Apache-2.0. Python 환경/패키지/도구 관리. 제3자 패키지/API 비용 별도 |
| just | INSTALL | FULL | FREE_OSS | 🟢 | CC0-1.0 command runner. Windows에서 PowerShell/cmd shell 지정 가능. 처음 보는 justfile recipe는 실행 전 검토 |
| mise | INSTALL | FULL | FREE_OSS | 🟢~🟡 | 여러 언어/SDK 버전+env+task 재현. Windows winget 지원. 외부 `mise.toml`은 trust/실행 전 검토 |
| Context7 public docs | CONNECT/INSTALL | AFTER_SETUP | FREE_TIER | ⚪~🟢 | Free 월 1,000 API calls. 카드 없이 가입 가능. Private repo는 유료라 기본 제외 |

### jq

`jq`는 JSON 전용 CLI processor입니다. portable C로 작성되고 runtime dependency가 없으며 공식 release에서 플랫폼별 standalone executable을 제공합니다. 설정 파일, API 응답, package metadata처럼 JSON이 많은 자동화 작업에서 Python/PowerShell 임시 스크립트를 매번 만드는 대신 필요한 필드 추출·필터·변환을 한 줄 CLI로 처리하기 좋습니다. MIT 무료 오픈소스이며 로그인·API key·GUI가 필요 없어 AI 단독 운용성이 높습니다.

`jq`는 데이터를 읽고 변환하는 도구일 뿐 입력 JSON이나 그 안의 명령/URL이 안전하다는 보장은 하지 않습니다. 외부 데이터의 값을 후속 shell 명령에 넣을 때는 별도 검증/escaping을 유지합니다.

### mise

`mise`는 Node.js/Python/Go 등 개발 도구 버전, 환경변수, task를 `mise.toml`에 선언하고 같은 환경을 shell/editor/CI에서 재현하는 CLI입니다. Windows 공식 설치 경로에 `winget install jdx.mise`가 있고 shell activation 없이도 `mise exec`/`mise run`을 사용할 수 있어 AI 자동화와 잘 맞습니다.

프로젝트마다 Node/Python 버전이나 build/test 명령이 달라 환경 재현 문제가 반복될 때 후보 가치가 높습니다. 반대로 단순 Python 프로젝트에서는 이미 기록한 `uv`, 단순 command alias만 필요하면 `just`가 더 작은 선택일 수 있으므로 무조건 도입하지 않습니다.

`mise.toml`은 SDK 설치와 임의 task 실행을 유도할 수 있으므로 외부 저장소 설정은 내용을 확인한 뒤 trust/install/run 합니다. `mise` 자체의 무료 여부와 설치되는 SDK/API/패키지의 비용·라이선스는 분리합니다.

### fd + rg

`fd`는 파일/폴더 이름·경로 검색, `rg`는 파일 내용 검색을 맡게 하면 대형 repo 탐색을 대부분 CLI만으로 처리할 수 있습니다. 둘 다 GUI나 계정 연결이 필요하지 않아 AI 단독 운용성이 높습니다.

### just

`just`는 build system이 아니라 command runner입니다. 저장소의 반복 build/test/deploy 명령을 `justfile` recipe로 표준화하면 AI가 프로젝트별 명령을 매번 다시 추론하는 일을 줄일 수 있습니다. Windows에서는 PowerShell 또는 cmd.exe를 shell로 지정할 수 있습니다. 도구 자체는 CC0-1.0 무료이지만 recipe는 임의 명령을 실행하므로 외부 저장소의 justfile은 실행 전 내용을 확인합니다.

### uv 메모

`uv`는 Python package/project manager이며 standalone installer로 Python/Rust가 없는 환경에서도 설치할 수 있습니다. `pip`, `pip-tools`, `pipx`, `poetry`, `pyenv`, `virtualenv` 등이 나눠 맡던 상당 부분을 한 CLI에서 처리하고 Python 버전도 설치할 수 있어 AI가 여러 프로젝트의 Python 환경을 반복 재현하는 용도에 적합합니다. 프로그램 자체는 무료 오픈소스지만 `uv`가 설치/실행하는 제3자 패키지와 외부 API의 비용·라이선스는 별도입니다.

## Plugin Directory 연결 후보

공식 비용을 확인하지 않은 후보는 `UNKNOWN`이며 자동 설치/연결 우선순위에 올리지 않습니다.

| Tool | ACCESS | OPERATOR | AUTONOMY 예상 | COST | 부담 |
|---|---|---|---|---|---|
| Figma | CONNECT | BOTH | PARTIAL | UNKNOWN | ⚪ |
| Codex Security | CONNECT | AI | AFTER_SETUP | UNKNOWN | ⚪ |
| Superpowers | CONNECT | AI | AFTER_SETUP | UNKNOWN | ⚪ |
| OpenAI Library | CONNECT | AI | AFTER_SETUP | UNKNOWN/INCLUDED 가능 | ⚪ |
| Airtable | CONNECT | BOTH | AFTER_SETUP | UNKNOWN | ⚪ |
| Canva | CONNECT | BOTH | PARTIAL | UNKNOWN | ⚪ |
| HeyGen | CONNECT | BOTH | PARTIAL | UNKNOWN | ⚪ |
| HyperFrames by HeyGen | CONNECT | AI | AFTER_SETUP | UNKNOWN | ⚪ |
| Remotion | CONNECT | AI | AFTER_SETUP | UNKNOWN | ⚪~🟢 |
| Longbridge | CONNECT | BOTH | AFTER_SETUP | UNKNOWN | ⚪ |
| Quartr | CONNECT | BOTH | AFTER_SETUP | UNKNOWN | ⚪ |
| Financial Datasets | CONNECT | AI | AFTER_SETUP | UNKNOWN | ⚪ |
| Stocktwits | CONNECT | BOTH | AFTER_SETUP | UNKNOWN | ⚪ |
| Bigdata.com | CONNECT | AI | AFTER_SETUP | UNKNOWN | ⚪ |
| Consensus | CONNECT | BOTH | AFTER_SETUP | UNKNOWN | ⚪ |
| Sider Scholar | CONNECT | BOTH | AFTER_SETUP | UNKNOWN | ⚪ |

## 무거운 도구 별도 취급

- 🔴 `ComfyUI` — 모델/LoRA/VAE/PyTorch/CUDA/VRAM/디스크가 본체
- 🔴 `vLLM` — GPU/CUDA/대형 모델 serving
- 🔴 `RAGFlow self-host` — 높은 RAM/디스크 + Docker 다중 서비스
- 🔴 대형 로컬 LLM/DeepSeek — 모델별 재평가
- 🟠 `Ollama` — 앱보다 모델 파일 부담
- 🟠 `Dify self-host`, `Firecrawl self-host` — 직접 운영 시 서비스 계층 증가

## 갱신 규칙

`정체 확인 → ACCESS → OPERATOR → AUTONOMY → ASSISTANCE/BLOCKER → COST → INSTALLED → ADOPTION → BURDEN → Cloud/self-host → 권한·보안·라이선스 → 사용자용 한글 요약`

가격은 공식 pricing/README/license를 우선하며 SNS의 `무료`, `free`, `$0`만으로 확정하지 않습니다. 같은 도구가 여러 목록에 반복되면 기존 항목을 갱신합니다.

마지막 정리: **2026-09-17**
