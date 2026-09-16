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
| ripgrep (`rg`) | INSTALL | FULL | FREE_OSS | 🟢 | MIT/UNLICENSE. 로컬 검색은 AI가 독립 운용하기 좋음 |
| Gitleaks CLI | INSTALL | FULL | FREE_OSS | 🟢 | 로컬 CLI 기준. `gitleaks-action`의 라이선스/runner 조건과 분리해서 봄 |
| uv | INSTALL | FULL | FREE_OSS | 🟢 | MIT OR Apache-2.0. Windows/macOS/Linux. Python 자체 설치, venv, lock/sync, Python CLI tool 실행까지 통합. 제3자 패키지/API 비용은 별도 |
| Context7 public docs | CONNECT/INSTALL | AFTER_SETUP | FREE_TIER | ⚪~🟢 | 공식 Free: 월 1,000 API calls. 한도 도달 시 월 reset 전까지 차단되며 일 20 bonus calls. 카드 없이 가입 가능. Private repo는 유료라 기본 제외 |

### uv 메모

`uv`는 Python package/project manager이며 standalone installer로 Python/Rust가 없는 환경에서도 설치할 수 있습니다. `pip`, `pip-tools`, `pipx`, `poetry`, `pyenv`, `virtualenv` 등이 나눠 맡던 상당 부분을 한 CLI에서 처리하고 Python 버전도 설치할 수 있어, AI가 여러 프로젝트의 Python 환경을 반복 재현하는 용도에 적합합니다. 프로그램 자체는 무료 오픈소스지만 `uv`가 설치/실행하는 제3자 패키지와 외부 API의 비용·라이선스는 별도입니다. Python package build 과정에서 임의 코드가 실행될 수 있으므로 신뢰되지 않은 dependency를 자동 설치하는 근거로 사용하지 않습니다.

### 2026-09-16 주의 — Gitleaks Action

GitHub-hosted runner의 Node 20 제거 일정 때문에 `gitleaks-action@v2`는 2026-09-16부터 동작하지 않는다고 공식 Marketplace/README가 안내합니다. Action을 채택할 경우 v3 및 runner 요구사항을 재검증합니다. 이는 무료 오픈소스인 로컬 Gitleaks CLI와 별개입니다.

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
