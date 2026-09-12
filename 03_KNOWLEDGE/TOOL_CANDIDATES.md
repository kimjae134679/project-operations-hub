# Tool Candidates — 도구 자체 설명판

마지막 정리: **2026-09-13**

이 문서는 “우리한테 쓸모 있나?”보다 먼저 **그 도구가 정확히 무엇이고, 원래 만든 사람들은 무엇을 하려고 만들었고, 실제로 어떤 구조로 동작하는지** 설명합니다.

각 항목은 가능하면 다음 순서로 적습니다.

1. **정체** — 프로그램인지, 플러그인인지, Skill인지, MCP인지
2. **원문 핵심** — 원래 README/게시물에서 강조한 내용
3. **동작 방식** — 실제로 무엇이 무엇과 연결되는지
4. **주요 기능** — 어떤 기능이 들어 있는지
5. **사용 흐름** — 대략 어떻게 설치·실행하는지
6. **알아둘 점** — 요구환경, 계정, 권한, 제한사항

SNS 게시물의 성능 수치나 홍보문구는 **제작자/게시물 주장**으로 따로 표시하고, 검증된 사실처럼 섞지 않습니다.

---

# 1. AI 작업 방식 / 플러그인 / 협업

## OpenAI Plugins
링크: https://github.com/openai/plugins

### 정체
OpenAI가 공개한 **Codex Plugin 예제 모음/마켓 구조 참고 저장소**입니다.

### 원문 핵심
README는 이 저장소를 **“curated collection of Codex plugin examples”**라고 설명합니다. 각 플러그인은 `plugins/<name>/` 아래에 있고, 필수로 `.codex-plugin/plugin.json` manifest를 가지며 필요에 따라 `skills/`, `.app.json`, `.mcp.json`, `agents/`, `commands/`, `hooks.json`, `assets/` 등을 같이 넣을 수 있습니다.

### 실제로 무슨 뜻인가
Codex Plugin은 기능 하나짜리 확장이라기보다 **AI에게 필요한 여러 부품을 하나의 패키지로 묶는 상자**에 가깝습니다.

예를 들어 Figma용 Plugin이라면:

```text
Plugin
├─ Skill      : Figma 작업 방법
├─ MCP        : Figma 데이터 연결
├─ Command    : 자주 쓰는 명령
├─ Agent      : 특정 역할의 하위 Agent
└─ Hook       : 특정 시점 자동 실행
```

처럼 구성할 수 있습니다.

### 원문에 나온 대표 예시
- `figma` — 디자인 시스템, Code to Canvas, Code Connect 계열
- `notion` — 계획, 조사, 회의, 지식 기록
- `build-ios-apps` — SwiftUI 구현/리팩터링/성능/디버깅
- `build-macos-apps` — macOS SwiftUI/AppKit 작업
- `build-web-apps` — 웹 배포/UI/결제/DB 작업
- `expo` — Expo/React Native, SDK upgrade, EAS
- `netlify`, `remotion`, `google-slides` 등

### 사용 흐름
필요한 Plugin을 Codex의 Plugin/Marketplace 흐름으로 설치합니다. Plugin 자체가 외부 App이나 MCP를 포함하면 그 서비스의 로그인/OAuth/API 설정이 추가될 수 있습니다.

### 설치 방법
1. Codex를 최신 상태로 준비하고 로그인합니다.
2. Codex의 Plugin/Marketplace 화면(또는 /plugins)에서 원하는 플러그인을 찾습니다.
3. 해당 플러그인을 설치합니다. 저장소 전체를 설치하는 게 아니라 필요한 플러그인만 고르는 방식입니다.
4. 플러그인이 Figma·Notion 같은 외부 App/MCP를 요구하면 설치 후 로그인/OAuth 연결을 진행합니다.
5. 새 세션에서 플러그인이 표시되고 관련 명령/Skill이 보이는지 확인합니다.

---

## AGENTS.md
링크: https://agents.md/

### 정체
AI 코딩 도구가 프로젝트에 들어왔을 때 읽는 **AI 전용 작업지침 파일 규약**입니다.

### 무슨 파일인가
사람용 README가 “이 프로젝트가 뭔지” 설명한다면 AGENTS.md는 주로 이런 내용을 적습니다.

- 빌드 명령
- 테스트 명령
- 코딩 규칙
- 수정하면 안 되는 영역
- 프로젝트 구조
- 보안 주의사항
- 완료 판단 기준

하위 폴더에도 AGENTS.md를 둘 수 있어서 큰 저장소에서는 위치에 따라 더 구체적인 지침을 줄 수 있습니다.

### 중요한 점
AGENTS.md 자체는 실행 프로그램이 아닙니다. AI가 읽는 **문서형 제어 장치**입니다. 따라서 오래된 명령이나 과거 우회법이 남아 있으면 AI가 그걸 현재 규칙으로 오해할 수 있습니다.

### 설치 방법
설치 프로그램은 없습니다.
1. 프로젝트 루트에 AGENTS.md 파일을 만듭니다.
2. 빌드/테스트 명령, 수정 금지 영역, 작업 규칙 등을 적습니다.
3. 하위 폴더만 다른 규칙이 필요하면 그 폴더 안에도 AGENTS.md를 둘 수 있습니다.
4. AI 코딩 도구를 새 세션으로 열어 파일을 읽는지 확인하면 끝입니다.

---

## Ponytail
링크: https://github.com/DietrichGebert/ponytail

### 정체
여러 AI 코딩 Agent에 붙일 수 있는 **Skill / Plugin / Ruleset 묶음**입니다.

### 원문 핵심
저장소는 Ponytail을 **“Lazy senior dev mode for AI agents”**라고 부르고, 패키지 설명에는 **“The best code is the code you never wrote.”**라는 문구가 있습니다.

Plugin 설명은 더 노골적입니다. “작동하는 가장 게으른 해결책을 강제한다”, `YAGNI`, 표준 라이브러리 우선, 큰 구현보다 짧은 구현을 선호한다는 방향입니다.

### 실제로 무엇을 시키나
대략 이런 사고순서를 Agent에 강하게 주입합니다.

```text
정말 필요한가?
↓
기존 코드로 가능한가?
↓
표준 라이브러리/플랫폼 기본기능으로 가능한가?
↓
이미 설치된 패키지로 가능한가?
↓
그래도 안 되면 새 코드/새 의존성 추가
```

즉 AI가 작은 변경을 요청받고 갑자기 새 프레임워크, 새 추상화 계층, 새 서비스까지 만드는 **과잉설계**를 줄이는 목적입니다.

### 들어 있는 기능
현재 plugin manifest에는 다음 계열이 명시되어 있습니다.

- `ponytail`
- `ponytail-review`
- `ponytail-audit`
- `ponytail-debt`
- `ponytail-gain`
- `ponytail-help`

또 lifecycle hook을 이용해 Agent 호출 전 현재 Ponytail 모드를 자동 주입하는 구성도 있습니다.

### 지원 형태
Claude Code, Codex 계열뿐 아니라 OpenCode, Gemini/Antigravity, Hermes, Devin, OpenClaw, Grok Build 등 여러 Agent 환경용 설치법/규칙파일이 들어 있습니다.

### 알아둘 점
Ponytail은 “요구사항을 줄이는 도구”가 아니라 **구현 방법을 덜 복잡하게 만드는 방향의 지침**입니다. 다만 강한 축약 모드를 쓰면 실제 요구사항까지 과하게 줄이는 결과가 날 수 있으므로 모드별 차이를 확인해야 합니다.

### 설치 방법
Codex 기준으로는 플러그인 마켓을 등록한 뒤 설치합니다.
```bash
codex plugin marketplace add DietrichGebert/ponytail
```
그다음 Codex를 다시 열고 /plugins에서 Ponytail을 선택해 설치합니다. lifecycle hook을 쓰는 구성은 Node.js가 PATH에 있어야 합니다. 설치 후 review/audit/debt 같은 제공 기능이 보이는지 확인합니다.

---

## FrontierAgent
링크: https://github.com/ApodexAI/FrontierAgent

### 정체
**독립형 Agent Runtime + 터미널 UI(TUI) + 평가 프레임워크**입니다. 단순 Plugin이 아니라 자체적으로 실행되는 Agent 프로그램 쪽에 가깝습니다.

### 원문 핵심
README는 FrontierAgent를 **장시간 연구(long-horizon research)와 파일 기반 작업(file-based work)을 위한 오픈소스 Agent runtime, terminal product, evaluation suite**로 설명합니다.

### 두 가지 핵심 작업 모드
#### 1. ReAct
하나의 상태 있는 Agent가 계속 이어서:

- 조사
- 파일 읽기
- 결과물 작성
- 명령 실행
- 결과 확인
- 다시 수정

을 반복합니다.

#### 2. Agent Team
Coordinator가 전체 일을 관리하고 여러 sub-agent에 독립 작업을 나눕니다.

```text
Coordinator
├─ Sub-agent A : 조사
├─ Sub-agent B : 코드/파일 분석
├─ Sub-agent C : 검증
└─ 보고서 회수 → 종합
```

원문은 coordinator가 task board를 유지하고, bounded assignment를 병렬 배포하고, 구조화된 보고서를 받아 최종 결과를 합치는 구조를 강조합니다.

### 별도로 들어 있는 것
같은 workflow engine을 benchmark/evaluation runner에도 사용합니다. 즉 “Agent 실행기”와 “그 Agent가 얼마나 잘했는지 평가하는 층”을 분리해서 재사용할 수 있게 만든 구조입니다.

### 사용 방식
별도 TUI 프로그램을 실행해 LLM endpoint를 연결하고 작업시킵니다. macOS/Linux의 간단한 설치 경로와 Docker/Compose 문서가 제공됩니다.

### 설치 방법
1. 공식 GitHub README의 최신 Quickstart를 먼저 확인합니다. 이 프로젝트는 버전에 따라 설치 스크립트와 모델 연결 방식이 바뀔 수 있습니다.
2. macOS/Linux라면 upstream one-command 설치 또는 clone 후 의존성 설치 흐름을 따릅니다.
3. Windows에서는 네이티브 지원 여부를 먼저 확인하고, 안 맞으면 WSL2 안에 별도 설치하는 편이 안전합니다.
4. 사용할 LLM endpoint/API key를 설정하고, 테스트용 빈 폴더에서 Agent가 파일/명령을 정상 처리하는지 확인합니다.

---

## Gentle-AI
링크: https://github.com/Gentleman-Programming/gentle-ai

### 정체
이미 설치해 둔 Claude Code, Cursor, OpenCode, Codex, Pi 같은 AI 코딩 Agent를 **하나의 개발환경처럼 구성해주는 설정 프레임워크**입니다.

### 원문 핵심
README의 표현은 “이미 쓰고 있는 AI coding agent를 configured engineering environment로 바꾼다”는 것입니다.

즉 새 AI 모델을 제공하는 게 아니라 기존 Agent에 다음 구성요소를 붙입니다.

- persistent memory
- Spec-Driven Development(SDD)
- Receipt-Driven Development(RDD)
- curated skills
- MCP servers
- personas
- bounded review

### Engram memory
Gentle-AI 문서에서 Engram은 **세션을 넘어 유지되는 기억 시스템**으로 설명됩니다. 결정사항, 발견, 버그 수정, 프로젝트 맥락 등을 자동 저장해서 다음 세션의 Agent가 이어받는 식입니다.

### SDD
Spec-Driven Development는 구현 전에 요구사항/설계를 명시적으로 만들고 그걸 기준으로 구현하는 흐름입니다.

### RDD
Receipt-Driven Development는 “했다”는 말만 믿는 게 아니라 작업 결과의 증거/receipt를 남기고 그것을 기준으로 확인하는 방향입니다.

### 설치 후 사용 개념
원문은 설치 후 사용자가 매번 SDD 단계나 내부 설정을 외울 필요 없이 평소처럼 Agent를 열어 작업하면 되게 만드는 것을 목표로 합니다. `gentle-ai doctor`로 설치 상태를 확인하는 흐름도 제공합니다.

### 설치 방법
Go가 설치되어 있어야 합니다. upstream 기준 설치 예시는 다음 계열입니다.
```bash
go install github.com/Gentleman-Programming/gentle-ai/v2/cmd/gentle-ai@latest
```
설치 후 gentle-ai를 실행해 사용할 Agent와 구성요소를 선택하고, 마지막에 `gentle-ai doctor`로 설정 상태를 점검합니다. 전역 규칙을 바로 덮어쓰기 전에 테스트 프로젝트에서 먼저 실행합니다.

---

## Anthropic Sandbox Runtime (`srt`)
링크: https://github.com/anthropics/sandbox-runtime

### 정체
컨테이너 없이도 임의 프로세스에 **파일시스템/네트워크 접근 제한을 거는 OS 레벨 sandbox 도구**입니다.

### 원문 핵심
Anthropic은 이를 “OS level에서 arbitrary process의 filesystem/network restriction을 강제하는 lightweight sandbox”로 설명하며, Claude Code용 더 안전한 Agent 실행을 연구하면서 공개한 **Beta Research Preview**라고 명시합니다.

### 무엇을 제한하나
- 어떤 파일/폴더를 읽을 수 있는지
- 어떤 파일/폴더에 쓸 수 있는지
- 어떤 인터넷 도메인에 접속할 수 있는지
- 어떤 Unix socket에 접근할 수 있는지

### 원문 예시
허용된 도메인에는 `curl`이 성공하지만 허용되지 않은 도메인은 차단하고, 현재 작업폴더의 README는 읽게 하면서 `~/.ssh/id_rsa` 같은 민감 파일은 OS 차원에서 접근을 거부하는 예시가 있습니다.

### 내부 구조
- macOS: `sandbox-exec`
- Linux: `bubblewrap`
- 네트워크: proxy 기반 filtering

따라서 단순히 “AI에게 읽지 말라고 말하는 것”보다 더 아래 계층에서 권한을 막는 도구입니다.

### 설치 방법
Node.js/npm이 필요합니다.
```bash
npm install -g @anthropic-ai/sandbox-runtime
```
설치 후 srt 명령으로 실행할 프로그램을 감싸고 파일/네트워크 허용 범위를 설정합니다. Windows 네이티브보다 macOS/Linux가 주 대상이므로 Windows에서는 WSL2 안에서 시험하는 편이 현실적입니다.

---

## Camofox Browser
링크: https://github.com/jo-inc/camofox-browser

### 정체
AI Agent용 **headless browser automation server**입니다. Camoufox/Firefox 계열을 사용해 일반 Playwright 브라우저보다 탐지 회피와 세션 분리를 강화한 쪽입니다.

### 원문/패키지 설명 핵심
패키지 설명은 “anti-detection, element refs, session isolation을 제공하는 AI Agent용 headless browser automation server”라고 되어 있습니다.

### 주요 특징
- headless browser 자동화
- 브라우저 fingerprint 위장/변형
- AI가 다시 찾기 쉬운 안정적인 element reference
- 여러 session 격리
- 접근성 기반 snapshot
- REST/MCP 계열 연결
- OpenClaw plugin

### 왜 일반 브라우저 자동화와 다른가
일반 Playwright/Selenium은 DOM selector가 쉽게 깨지거나 자동화 브라우저라는 사실이 사이트에 드러나는 문제가 있습니다. Camofox는 AI가 페이지를 반복 조작하는 데 필요한 안정적인 element 식별과 anti-detection 쪽을 함께 다룹니다.

### 요구환경
현재 npm package는 Node 22 이상을 요구합니다.

### 설치 방법
Node.js가 필요합니다. 패키지로 설치하려면:
```bash
npm install @askjo/camofox-browser
```
소스에서 돌리려면 저장소를 clone한 뒤 `npm install` → `npm start` 흐름을 사용합니다. 최근 버전은 Node 22+ 요구 여부를 README에서 확인합니다. 서버가 뜨면 REST/MCP 주소를 사용하는 Agent 쪽에 연결합니다.

---

## GPTaku Plugins — Claude Code
링크: https://github.com/fivetaku/gptaku_plugins

### 정체
Claude Code에 여러 전문 기능을 추가하는 **Plugin Marketplace**입니다.

### 원문 핵심
README 첫 설명은 “무엇을 원하는지 계속 설명하지 말고 Claude Code가 하게 만들자”는 방향이며, 현재 여러 플러그인을 묶어 제공합니다.

### 원문에 나온 주요 플러그인
#### `insane-search`
일반 검색에서 막히는 사이트(WAF/403/CAPTCHA 등)에 접근해 자료를 찾는 검색 플러그인으로 소개됩니다.

#### `insane-research`
여러 Agent가 조사하고 출처를 교차확인해서 citation 기반 결과를 만드는 deep research 플러그인입니다.

#### `insane-design`
웹사이트에서 디자인 시스템/스타일을 분석해 가져오는 기능입니다.

#### `docs-guide`
공식 문서 기반 답변에 집중하며 `llms.txt`와 여러 라이브러리 문서를 활용하는 플러그인입니다.

#### `insane-review`
로그인된 ChatGPT Plus/Pro 웹 세션을 이용해 코드리뷰를 보조합니다. API key 방식이 아니라 브라우저의 로그인 세션을 전제로 합니다.

#### `pumasi`
외부 Codex CLI를 사용해 작업을 나눠 맡기는 유형입니다.

#### 기타
PRD, Git 학습, workspace 도구, 병렬 workflow 등 여러 플러그인이 포함됩니다.

### 게시물에 붙어 있던 성능 설명
사용자가 보낸 게시물 이미지에서는 Astra 적용 후 제작자 측 측정으로:

- `insane-search` 로딩 시간 **59.6% 감소**
- `insane-research` 평가 계산 시간 **65.3% 감소**
- “Faster Start”, “Less Repeat” 같은 개선 포인트

를 강조하고 있었습니다. 이 수치는 **게시물/제작자 측 벤치마크**이며 환경에 따라 달라질 수 있습니다.

### 인증 요구가 다른 플러그인
원문은 대부분 credential이 필요 없다고 설명하지만 예외도 명시합니다.

- `nopal` → Google OAuth
- `insane-review` → 로그인된 ChatGPT 웹 세션
- `pumasi` → 호스트의 Codex CLI

### 설치 방법
1. Claude Code를 먼저 설치하고 정상 실행되는지 확인합니다.
2. GPTaku 저장소를 Claude Code의 Plugin Marketplace로 등록합니다.
3. 마켓에서 필요한 플러그인(insane-search, insane-research, docs-guide 등)만 골라 설치합니다.
4. 플러그인별 README에 추가 요구사항이 있으면 그때 설치합니다. 예: Google OAuth, Codex CLI, 브라우저 로그인 세션 등.
5. Claude Code를 다시 시작하고 해당 명령/기능이 보이는지 확인합니다.

---

## GPTaku Plugins — Codex
링크: https://github.com/fivetaku/gptaku-plugins-codex

### 정체
GPTaku 플러그인 아이디어를 **Codex Plugin 방식으로 옮긴 Marketplace**입니다.

### 구성
README 기준으로 Codex용 패키지들이 따로 제공되며, 예를 들어:

- `docs-guide-codex`
- `insane-search-codex`
- `insane-design-codex`

처럼 Codex Plugin으로 설치할 수 있게 구성됩니다.

### 사용 흐름
Marketplace 저장소를 Codex에 등록하고 `/plugins`에서 필요한 패키지를 선택하는 방식입니다.

```text
Codex
↓ marketplace 등록
GPTaku Codex Marketplace
↓ 필요한 기능 선택
Search / Docs / Design / ...
```

Plugin마다 필요한 외부 CLI/브라우저/런타임은 다를 수 있으므로 하위 README를 따로 봐야 합니다.

### 설치 방법
Codex에서 마켓을 등록합니다.
```bash
codex plugin marketplace add https://github.com/fivetaku/gptaku-plugins-codex.git
```
Codex를 재시작한 뒤 `/plugins`에서 docs-guide-codex, insane-search-codex, insane-design-codex 등 필요한 것만 설치합니다. 각 플러그인의 선택 의존성(git, gh, node, python3, tmux 등)은 해당 README를 보고 추가합니다. Windows에서는 upstream 권장대로 WSL2가 필요한 플러그인이 있는지 확인합니다.

---

## Tencent TeamAI
링크: https://github.com/Tencent/teamai-cli

### 정체
팀이 쓰는 **Skills, Rules, MCP, Knowledge를 여러 AI Agent에 동기화하는 CLI**입니다.

### 원문 핵심
Tencent README는 TeamAI가 Claude Code, Codex, CodeBuddy, WorkBuddy, OpenCode, Cursor 등 여러 AI Agent 사이에서 팀의 skills/rules/MCP/knowledge를 관리한다고 설명합니다.

### 사용자가 보낸 게시물의 설명
게시물에서는 다음 문제를 예로 들었습니다.

```text
회사에서
A는 Claude Code
B는 Codex
C는 Cursor
→ AI 설정이 전부 따로 놂
```

TeamAI는 한 사람이:

```text
Skill 수정
규칙 수정
MCP 수정
팀 지식 수정
```

하면 공유 저장소를 통해 팀원의 다른 AI 환경에도 동기화하고, 실수·수정 경험도 팀 지식으로 축적하는 방향이라고 소개됐습니다.

### 실제 구조
핵심은 **Git 저장소를 팀 경험의 원본**으로 삼는 것입니다.

```text
Shared Git Repo
├─ Skills
├─ Rules
├─ MCP config
└─ Knowledge
      ↓
TeamAI CLI
      ↓
Claude / Codex / Cursor / OpenCode / ...
```

### 주요 명령 개념
- `teamai init` — 공유 repo와 현재 환경 연결
- pull — 팀의 최신 경험 가져오기
- push/contribute — 새 규칙/지식 기여
- recall — 필요한 지식 찾기

### 범위
Project scope로 프로젝트 폴더 아래에 설치하거나 사용자 범위로 배치하는 흐름이 있습니다. GitHub뿐 아니라 GitLab 등 일반 Git 서비스도 원본 repo로 사용할 수 있게 설명되어 있습니다.

### 설치 방법
Node.js/npm이 필요합니다.
```bash
npm install -g teamai-cli
```
설치 후 빈 테스트 저장소에서 먼저 `teamai init ...` 흐름을 실행합니다. 공유할 TeamAI Git 저장소를 지정하고, Codex/Claude/Cursor 등 연결할 Agent를 선택합니다. 그다음 pull/push/recall/contribute 같은 명령으로 규칙·Skill·MCP·지식을 동기화합니다. 처음에는 실제 Hub가 아니라 테스트 repo에서 생성되는 .codex/.claude/hooks 파일을 확인합니다.

---

## Codex with ChatGPT
링크: https://github.com/XiaoDuoYa/codex-with-chatgpt

### 정체
ChatGPT 웹과 Codex를 연결하는 **Codex Skill + 읽기전용 MCP bridge + OAuth 연결 도구**입니다.

### 원문 슬로건
README의 핵심 문구는 **“ChatGPT thinks. Codex works.”**입니다.

### 해결하려는 문제
원문은 ChatGPT Plus/Pro 웹 사용량은 남는데 Codex/API 쪽 자원을 계획·리뷰에 많이 쓰는 상황을 문제로 듭니다. 그래서:

- ChatGPT 웹 → 계획, 추론, 리뷰
- Codex → 파일 수정, shell, 테스트, 실제 실행

으로 역할을 나눕니다.

### 데이터 흐름
```text
ChatGPT Web
  │  계획/리뷰
  │
  ├─ Computer Use : 짧은 제어 메시지
  │
  └─ MCP : 필요한 파일/상태 읽기
          ↓
      C2C Bridge
          ↓ read-only
   Local Workspace
          ↑
        Codex
   수정 / 명령 / 테스트
```

### ChatGPT가 읽을 수 있는 것
README는 read-only MCP 도구로 workspace 정보, 디렉터리 목록, 파일 읽기, 검색, git status/diff, test status, execution summary/output 등을 제공한다고 설명합니다.

### ChatGPT가 못 하는 것
V1 보안 문서에는 write/delete/shell/commit 도구 자체가 서버에 없어서 ChatGPT가 그 동작을 할 수 없다고 명시되어 있습니다.

### 보안 구조
- workspace 단위 token 격리
- OAuth 2.1
- PKCE
- one-time pairing code
- refresh token rotation
- `.env`, SSH key, credential 파일 deny
- symlink/`..` path escape 방지
- localhost bridge + Cloudflare tunnel

### 설치 흐름
Node 20+, Git, cloudflared를 준비하고 repo build → Skill 설치 → `c2c setup` → ChatGPT connector/pairing 순으로 구성합니다.

### 중요한 성격
OpenAI 공식 프로젝트가 아니라 **커뮤니티 프로젝트**입니다.

### 설치 방법
준비물은 Git, Node.js 20+, cloudflared입니다. upstream의 자동 설치 흐름은 대략 다음 순서입니다.
1. 저장소를 `~/codex-with-chatgpt`에 clone(이미 있으면 pull).
2. 폴더에서 `corepack pnpm install` 후 `corepack pnpm build`.
3. `skill/SKILL.md`를 `~/.codex/skills/codex-with-chatgpt/SKILL.md`로 복사하고 checkout 경로를 실제 경로로 수정.
4. `c2c setup` 실행.
5. 내장 브라우저에서 ChatGPT Connector를 만들고 pairing code를 입력.
6. 마지막에 파일 읽기 테스트가 PASS인지 확인합니다. ChatGPT/Cloudflare 로그인·2FA가 나오면 그 부분만 사람이 처리합니다.

---

# 2. 이미지 / 디자인 / Blender

## awesome-gpt-image-2
링크: https://github.com/YouMind-OpenLab/awesome-gpt-image-2

### 정체
GPT Image 계열로 만든 이미지 결과와 프롬프트를 모아둔 **예시/프롬프트 큐레이션 저장소**입니다.

### 무엇이 들어 있나
다양한 스타일, 제품 이미지, 캐릭터, 포스터, 그래픽 디자인 등 “이런 결과를 만들 때 어떤 프롬프트/구성이 쓰였는지” 참고할 수 있는 예시가 핵심입니다.

### 중요한 점
이건 Blender MCP처럼 무언가를 실행하는 도구가 아니라 **레퍼런스/아이디어 모음**에 가깝습니다. 설치 없이 GitHub에서 예시를 보는 것이 주 사용법입니다.

### 설치 방법
설치는 필요 없습니다. GitHub 저장소를 열어 카테고리별 프롬프트와 결과 예시를 찾아보면 됩니다. 저장소 자체의 생성/관리 스크립트를 수정하려는 경우에만 README에 적힌 Node/pnpm 개발환경을 준비하면 됩니다.

---

## Mimikyu
링크: https://github.com/3x-haust/Mimikyu

### 정체
Figma 디자인을 웹으로 구현할 때 “비슷해 보인다”가 아니라 **실제 숫자로 비교하며 맞추는 Skill + 검증 파이프라인**입니다.

### 원문 핵심
README는 “Figma design → web code with a measured verification loop”를 강조합니다.

기본 흐름은:

```text
Figma
→ figma-data.json
→ 웹 코드
→ screenshot
→ pixel diff
→ DOM/구조 검증
→ 수정
→ 반복
```

입니다.

### 왜 두 종류의 검증을 쓰나
#### Pixel diff
원본 디자인 이미지와 브라우저 screenshot을 픽셀 단위로 비교합니다. 어느 영역이 얼마나 다른지, heatmap과 영역별 점수를 만들 수 있습니다.

#### Structural verify
Playwright로 실제 DOM의:

- x/y 위치
- 색
- font size
- font weight
- line height
- letter spacing

등을 `figma-data.json`의 기대값과 비교합니다.

그래서 “대충 다르다”가 아니라 `expected 436.5 / actual 373.0` 같은 식으로 어떤 값이 틀렸는지 찾습니다.

### 원문 completion gate
README는 Agent가 멈추기 전에 다음 같은 gate를 통과하도록 설계합니다.

- figma-data 추출 완료
- 구현 map 작성
- overall match 99% 이상
- 각 영역 99% 이상
- structural mismatch 0
- critical mismatch 0
- 상호작용 검사
- 최종 screenshot/report 생성

### Responsive 문제도 검사
같은 크기 screenshot만 비교하면 reflow/overlap 문제를 놓칠 수 있어서 rendered width mismatch, horizontal overflow, text overlap 검사도 추가되어 있습니다.

### 설치 구성
Node 20+, pnpm, Python/Pillow, Playwright Chromium, Figma MCP 또는 PAT가 필요합니다. Claude Code, Codex, pi 등에 Skill 형태로 설치할 수 있게 되어 있습니다.

### 설치 방법
가장 간단한 설치는 upstream의 one-command installer입니다.
```bash
curl -fsSL https://raw.githubusercontent.com/3x-haust/Mimikyu/main/install.sh | bash
```
이 스크립트가 Claude Code, Codex, pi 환경을 감지해 Skill을 복사하고 `~/.mimikyu/scripts/`에 파이프라인 스크립트를 둡니다. Windows에서는 WSL/Git Bash 또는 수동 설치가 더 안전할 수 있습니다. 수동 설치 시 Codex는 `~/.codex/skills/mimikyu/`에 Skill을 복사합니다. Node 20+, pnpm, Python3+Pillow, Playwright Chromium, Figma MCP 또는 Figma token이 필요합니다.

---

## Blender MCP
링크: https://github.com/emeryporter/blender-mcp

### 정체
Blender 안에 서버를 띄우고 AI Client가 그 서버와 통신하게 하는 **Blender Add-on + MCP server**입니다.

### 실제 구조
```text
Claude / Codex / MCP Client
        ↓ HTTP/MCP
Blender MCP Add-on
        ↓
실제 Blender scene
```

### 설치
원문 권장 설치는 GitHub Release의 `blender_mcp.zip`을 Blender Add-ons에서 설치한 뒤 활성화하는 방식입니다.

Blender 3D Viewport의 `N` sidebar에 MCP 탭이 생기고 `Start Server`를 누르면 기본적으로 `http://localhost:9876`에서 서버가 뜹니다. Auto-Start 옵션도 있습니다.

### 무엇을 할 수 있나
MCP client가 Blender scene/오브젝트 정보를 읽고, add-on이 제공하는 command 범위 안에서 실제 scene 작업을 수행하게 만들 수 있습니다.

즉 사용법을 설명해주는 챗봇이 아니라 **Blender를 직접 조작하는 연결층**입니다.

### 설치 방법
1. GitHub Releases에서 `blender_mcp.zip`을 받습니다.
2. Blender → Edit → Preferences → Add-ons → Install from Disk에서 ZIP을 선택합니다.
3. 애드온을 활성화합니다.
4. Blender의 N 패널에서 MCP 탭을 열고 Server를 시작합니다.
5. Codex/Claude 등 사용하는 AI client에 이 MCP 서버를 등록합니다.
6. 간단한 장면 읽기/오브젝트 조회부터 시험한 뒤 수정 명령을 테스트합니다. 기본적으로 localhost 연결을 유지합니다.

---

# 3. 영상 / 콘텐츠

## JoyAI-Video-Edit
링크: https://github.com/jd-opensource/JoyAI-Video-Edit

### 정체
자연어 지시로 영상의 내용 자체를 바꾸는 **생성형 AI video editing 연구모델/파이프라인**입니다.

### 일반 영상편집기와 차이
Concat 같은 NLE가 컷을 자르고 자막을 붙이는 도구라면 JoyAI-Video-Edit는 diffusion/generative model을 이용해 영상 장면의 시각적 내용을 수정하는 쪽입니다.

### 실행 성격
Python/CUDA 환경, model checkpoint, inference server 구성이 필요한 연구/서버형 프로젝트입니다. 일반 데스크톱 영상편집 앱처럼 설치 후 클릭해서 쓰는 구조는 아닙니다.

### 하드웨어
공개 예시/테스트가 대형 NVIDIA GPU 환경을 중심으로 설명되어 있어 소비자 12GB GPU에서 바로 가볍게 돌리는 종류는 아닙니다.

### 설치 방법
공식 배포가 Linux/CUDA 중심입니다.
1. Python 3.10 계열 Conda 환경을 만듭니다.
2. 프로젝트를 clone하고 CUDA 버전에 맞는 PyTorch와 requirements를 설치합니다.
3. README에서 지정한 checkpoint/model 파일을 내려받아 위치를 맞춥니다.
4. `deploy/run_server.sh` 계열 스크립트로 서버를 실행합니다.
5. 예제 입력으로 추론이 되는지 확인합니다. Windows에서는 WSL/Linux GPU 환경을 별도로 잡는 편이 현실적입니다.

---

## Concat
링크: https://github.com/jub0t/Concat

### 정체
무료·오픈소스 **cross-platform video editor / CapCut 대체 앱**입니다.

### 원문 핵심
README는 “No watermarks. No paywalls. No subscriptions.”와 로컬 처리를 강조합니다. Rust native engine 기반으로 계정 없이 설치 후 편집하는 방향입니다.

### 주요 기능
- multi-track editing
- 프로젝트당 여러 timeline
- split / trim / merge
- transition
- speed control
- local auto captions
- local TTS
- voice filters
- title/styled text
- template
- offline processing

### 플랫폼
Windows, macOS, Linux를 지원한다고 명시하며 현재는 Beta/pre-release입니다.

### 출력/제한
README 비교표 기준 현재 export format, effect 수, keyframe 기능 등은 CapCut보다 제한적인 부분이 있고 안정성도 beta 단계라고 명시합니다.

### 설치 방법
가장 쉬운 방법은 GitHub Releases의 Windows 빌드/portable 패키지를 받는 것입니다.
1. 최신 Release에서 Windows용 압축파일 또는 설치파일을 받습니다.
2. Portable이면 원하는 폴더에 압축을 풀고 실행합니다. 설치형이면 일반 프로그램처럼 설치합니다.
3. 처음 실행 후 FFmpeg/모델 같은 추가 구성요소를 요구하면 안내에 따라 설치합니다.
4. 테스트 영상 하나를 넣어 재생·컷 편집·내보내기까지 확인합니다.

---

## HyperFrames
링크: https://github.com/heygen-com/hyperframes

### 정체
HTML/CSS/JS로 만든 장면과 애니메이션을 **결정적으로(deterministic) MP4로 렌더링하는 오픈소스 프레임워크**입니다.

### 원문 슬로건
**“Write HTML. Render video. Built for agents.”**

### 기본 아이디어
영상 timeline 전용 에디터를 만드는 대신 HTML을 composition으로 씁니다.

```text
HTML/CSS/Media
+ seek 가능한 animation
        ↓
Headless Chrome/Puppeteer
        ↓ frame capture
FFmpeg
        ↓
MP4
```

### Agent 친화적인 이유
AI coding agent는 HTML/CSS/JS를 잘 작성하므로 별도의 독특한 timeline 파일 포맷을 배우지 않고도 영상을 코드로 만들 수 있습니다. CLI도 비대화식 자동실행에 맞게 설계되어 있습니다.

### 지원 animation
- GSAP
- CSS animations
- Lottie
- Three.js
- Anime.js
- WAAPI
- custom runtime

### 주요 package
- `hyperframes` CLI
- `@hyperframes/core`
- `@hyperframes/engine`
- `@hyperframes/producer`
- `@hyperframes/studio`
- `@hyperframes/player`
- shader transition
- AWS Lambda renderer

### 렌더 엔진
engine은 headless Chrome에서 frame-by-frame으로 원하는 시간으로 seek하고 screenshot을 캡처한 뒤 FFmpeg로 encode합니다. 병렬 worker, audio mix, streaming encode 같은 구성도 있습니다.

### 설치 방법
Node.js와 FFmpeg 계열 환경이 필요합니다.
1. 저장소를 clone하거나 공식 CLI/package 설치 방법을 따릅니다.
2. repo 개발형이라면 Git LFS를 먼저 설치하고 clone하는 편이 안전합니다. Windows 예시는 `winget install GitHub.GitLFS`.
3. 패키지 의존성을 설치한 뒤 예제 composition을 실행합니다.
4. HTML/CSS/JS 장면을 만든 뒤 renderer/CLI로 MP4를 생성합니다.
5. 브라우저와 FFmpeg가 정상 인식되는지 샘플 렌더로 확인합니다.

---

## Ddalkkak Threads Community
링크: https://github.com/apache3563-bit/ddalkkak-threads-community

### 정체
Windows용 **Threads 콘텐츠 제작/자동화 도구 Community Edition**입니다.

### 저장소 설명
GitHub description 자체가 “Windows용 Threads 콘텐츠 제작 및 자동화 도구 Community Edition”입니다.

### 성격
SNS 게시물 작성과 반복작업을 보조하는 데스크톱 프로그램 계열입니다. Release에서 Windows 배포물을 받아 사용하는 형태이고, Threads/Meta 계정과 실제 플랫폼 동작에 영향을 받습니다.

### 라이선스
현재 저장소는 AGPL-3.0으로 표시됩니다.

### 설치 방법
1. GitHub의 최신 Releases 페이지로 갑니다.
2. Windows용 설치파일/배포물을 받습니다. 저장된 v1.11.7 링크보다 최신 버전이 있으면 최신 설명을 먼저 확인합니다.
3. 실행 후 Threads/Meta 로그인이나 별도 설정이 필요한지 프로그램 안내를 따릅니다.
4. 실제 계정 자동화 전에 테스트 계정 또는 수동 검토 모드로 게시물 생성까지만 먼저 확인합니다.

---

# 4. 로컬 AI / GPU / 서버

## OpenLLM
링크: https://github.com/bentoml/OpenLLM

### 정체
오픈소스 LLM을 쉽게 띄워 **OpenAI-compatible API endpoint**로 제공하는 self-hosting 도구입니다.

### 원문 핵심
README는 Llama, Qwen, Phi 등 open-source/custom model을 한 명령으로 OpenAI 호환 API로 실행하고 built-in chat UI, inference backend, Docker/Kubernetes/BentoCloud 배포 흐름을 제공한다고 설명합니다.

### 실제 구조
```text
Llama/Qwen/Custom Model
        ↓
OpenLLM runtime
        ↓
OpenAI-compatible API
        ↓
내 앱 / Agent / 웹서비스
```

즉 모델을 직접 앱에 박는 대신 “내가 운영하는 ChatGPT API 비슷한 서버”로 만드는 겁니다.

### 사용 시작
`pip install openllm` 후 `openllm hello`, 모델별 serve 명령으로 시작하는 흐름입니다.

### 설치 방법
Python 환경에서 설치합니다.
```bash
pip install openllm
openllm hello
```
그다음 원하는 모델에 맞춰 `openllm serve ...`를 실행해 OpenAI-compatible endpoint를 띄웁니다. gated model이면 Hugging Face token이 필요할 수 있습니다. GPU 모델은 CUDA/VRAM 요구량을 먼저 확인합니다.

---

## BentoML
링크: https://github.com/bentoml/BentoML

### 정체
AI/ML inference 코드를 **API 서비스와 배포 가능한 패키지**로 만드는 model serving framework입니다.

### OpenLLM과 차이
OpenLLM이 LLM을 빠르게 띄우는 완성형 진입점이라면 BentoML은 더 범용적인 **AI 서비스 제작 기반**입니다.

예:

```text
Python 모델 코드
+ preprocessing
+ batching
+ 여러 모델 pipeline
        ↓
BentoML Service
        ↓
REST/API
        ↓
Docker / Kubernetes / Cloud
```

### 주요 개념
- inference service 정의
- API endpoint
- batching
- multi-model pipeline
- containerize
- cloud deployment

LLM뿐 아니라 일반 ML/vision 모델도 대상으로 합니다.

### 설치 방법
Python 환경에서 설치합니다.
```bash
pip install -U bentoml
```
Python으로 service를 정의한 뒤 `bentoml serve`로 로컬 실행, 필요하면 `bentoml build`와 `bentoml containerize`로 패키징/컨테이너화합니다. GPU/Docker 조합은 WSL/Linux가 더 편할 수 있습니다.

---

## xFormers
링크: https://github.com/facebookresearch/xformers

### 정체
Transformer 모델을 만들고 실행할 때 쓰는 **고성능/저메모리 연산 부품 모음**입니다.

### 원문 핵심
README는 xFormers를:

- customizable building blocks
- research-first components
- speed/memory efficiency 중심

으로 설명합니다.

### 무엇이 빨라지나
대표적으로 memory-efficient attention, fused/custom CUDA kernel 등 Transformer 내부 연산을 더 효율적으로 수행합니다.

### 왜 앱처럼 보이지 않나
xFormers는 사용자가 직접 실행하는 프로그램이 아니라 PyTorch 프로젝트나 Stable Diffusion/Transformer stack이 내부에서 가져다 쓰는 **라이브러리**입니다.

### 설치 특징
PyTorch/CUDA 버전에 맞는 wheel을 사용해야 하며 Linux와 Windows용 CUDA wheel이 제공되는 버전이 있습니다.

### 설치 방법
xFormers는 현재 PyTorch/CUDA 버전과 맞춰 설치해야 합니다.
1. 프로젝트 전용 venv/conda 환경을 만듭니다.
2. 먼저 사용할 PyTorch와 CUDA 조합을 확정합니다.
3. xFormers README의 공식 wheel/index 명령에서 그 조합에 맞는 버전을 설치합니다.
4. 설치 후 Python에서 `import xformers`와 사용하는 모델의 attention 경로를 테스트합니다.
Windows wheel이 없는 조합이면 억지 빌드보다 지원되는 버전 조합으로 맞추는 편이 안전합니다.

---

## NVIDIA cuML
링크: https://github.com/NVIDIA/cuml

### 정체
scikit-learn 계열 머신러닝 알고리즘을 **NVIDIA GPU로 가속하는 RAPIDS 라이브러리**입니다.

### 무엇을 하는가
CPU에서 오래 걸릴 수 있는 데이터 분석/ML 알고리즘을 CUDA GPU에서 수행합니다. 분류, 회귀, clustering, dimensionality reduction 등 여러 알고리즘이 포함됩니다.

### 기본 개념
```text
CPU sklearn식 ML
        ↓ 비슷한 API
cuML
        ↓ CUDA
NVIDIA GPU 계산
```

### 사용환경
CUDA/Python 버전에 맞춰 RAPIDS 공식 설치 matrix를 확인해야 하며 일반적으로 Linux/WSL/컨테이너 환경과 강하게 연결되어 있습니다.

### 설치 방법
cuML은 RAPIDS 설치 matrix를 따라야 합니다.
1. NVIDIA driver/CUDA와 Python 버전을 확인합니다.
2. RAPIDS 공식 설치 페이지에서 버전에 맞는 conda/pip/container 명령을 생성합니다.
3. Windows라면 네이티브보다 WSL2/Linux 환경에 설치하는 것을 우선 검토합니다.
4. 설치 후 작은 sklearn 대응 예제로 CPU 결과와 값이 맞는지, GPU 가속이 실제 되는지 확인합니다.

---

## Heretic
링크: https://github.com/p-e-w/heretic

### 정체
Transformer LLM의 **refusal/safety alignment를 약화시키는 연구 도구**입니다.

### 실제 하는 일
모델 내부에서 거절 행동과 관련된 방향을 찾아 directional ablation/optimization을 적용해, 원래 모델이 거절하던 입력에 더 잘 답하게 만드는 “abliteration” 계열 연구입니다.

### 중요한 성격
일반 inference 최적화나 Agent 생산성 도구가 아닙니다. 모델의 안전 정렬 자체를 변경하는 연구 프로젝트입니다.

### 실행
Python/PyTorch와 실제 model weight를 받아 로컬에서 처리합니다. 결과 모델은 원 모델과 다른 안전행동을 보일 수 있습니다.

### 설치 방법
격리된 Python 환경에서 설치합니다.
```bash
pip install -U heretic-llm
```
그다음 지원되는 transformer 모델을 별도로 내려받고 README의 실행 예제를 따릅니다. 모델 크기에 따라 CUDA/PyTorch와 큰 VRAM이 필요할 수 있습니다. 일반 Agent 기본환경에 설치하지 말고 별도 venv/폴더에서만 시험합니다.

---

## Ling 3.0 Flash VL
공식 모델: https://huggingface.co/inclusionAI/Ling-3.0-flash-VL
OpenRouter: https://openrouter.ai/inclusionai/ling-3.0-flash-vl:free

### 정체
텍스트뿐 아니라 **이미지와 영상 입력도 처리하는 대형 멀티모달 Vision-Language 모델**입니다.

### 사용자가 보낸 게시물에서 강조한 내용
게시물에는 다음이 적혀 있었습니다.

- 텍스트 + 이미지 + 영상
- reasoning/코딩 가능
- `124B MoE`
- 약 `5.5B active`
- `262K context`
- OpenRouter `free` route

### 124B / 5.5B active가 무슨 뜻인가
MoE(Mixture-of-Experts) 구조라 전체 파라미터는 매우 크지만 한 token을 처리할 때 전체 expert를 다 쓰지 않고 일부만 활성화합니다. 그래서 “전체 모델 크기”와 “한 번 계산할 때 실제로 활성화되는 부분”이 다릅니다.

### 멀티모달
일반 텍스트 LLM과 달리 image/video 입력을 함께 받아 시각 내용에 대해 질문하거나 분석할 수 있는 모델입니다.

### OpenRouter route
OpenRouter에 올라온 `:free` route를 이용하면 로컬에 수백 GB weight를 직접 설치하지 않고 API 방식으로 호출할 수 있습니다. 무료 정책, rate limit, context 표시는 OpenRouter 정책에 따라 바뀔 수 있습니다.

### 설치 방법
로컬 설치보다 OpenRouter API로 쓰는 방법이 간단합니다.
1. OpenRouter 계정을 만들고 API key를 준비합니다.
2. 모델 ID로 `inclusionai/ling-3.0-flash-vl:free`를 선택합니다.
3. OpenAI-compatible API를 지원하는 앱/스크립트에 OpenRouter base URL과 key, model ID를 넣습니다.
4. 텍스트 → 이미지 → 영상 입력 순으로 작은 테스트부터 합니다.
로컬 weight를 직접 받는 방법도 있지만 모델 규모가 매우 커서 일반 12GB GPU PC용 설치법으로 보기는 어렵습니다.

---

# 5. 생산성 / 참고자료

## TabZipsa
링크: https://tabzipsa.com/

### 정체
Chrome 브라우저에서 열린 탭을 AI로 분류/정리하는 **탭 관리 확장프로그램**입니다.

### 동작 개념
여러 창에 흩어진 탭의 제목/사이트 정보를 모아서 비슷한 주제끼리 그룹화하고, 사용자가 많은 탭을 한 패널에서 관리하게 만드는 도구입니다.

예:

```text
열린 탭 50개
↓
GitHub / 개발문서 / 청약 / 주식 / 쇼핑
↓
주제별 그룹
```

### 확인할 점
브라우저 확장은 탭 URL/제목 같은 browsing metadata에 접근할 수 있으므로 실제 설치 전 개인정보 처리방침과 외부 전송 범위를 확인해야 합니다.

### 설치 방법
Chrome 확장프로그램이라 설치는 간단합니다.
1. tabzipsa.com 또는 Chrome Web Store에서 TabZipsa를 엽니다.
2. Chrome에 추가를 눌러 확장프로그램을 설치합니다.
3. 필요하면 확장 아이콘을 고정합니다.
4. 테스트용 탭 몇 개를 열고 그룹화 기능을 시험합니다.
업무용/로그인 탭이 많은 브라우저 프로필에 바로 넣기 전에 권한과 개인정보처리방침을 확인합니다.

---

# 참고: 종류가 서로 다른 이유

위 목록은 전부 “AI 플러그인”이 아닙니다.

```text
Plugin / Skill
→ AI에게 새로운 작업법이나 기능 묶음을 추가

MCP
→ AI와 실제 외부 프로그램/데이터 사이를 연결

Agent Runtime
→ 자체적으로 AI 작업을 실행하는 별도 프로그램

Framework / Library
→ 개발자가 다른 프로그램 안에서 사용하는 기반 부품

Model
→ 실제 추론을 하는 AI 모델 자체

Reference collection
→ 실행 기능 없이 예시/프롬프트/자료를 모아둔 저장소
```

그래서 GitHub 링크를 볼 때는 먼저 **“이게 설치해서 실행하는 프로그램인지, AI에 붙이는 Skill인지, 다른 프로그램을 연결하는 MCP인지, 그냥 라이브러리/자료인지”**를 구분해야 합니다.
