# Candidate Master Index — canonical index

이 파일을 **후보 도구 전체의 1차 진입점**으로 사용합니다.

상세 설명은 각 주제 문서에 남기되, 현재 상태/설치 부담/어디를 봐야 하는지는 여기에서 먼저 확인합니다.

## 상태를 섞지 않기

서로 다른 축입니다.

- `ADOPTION` — 실제로 채택했는가: `ACTIVE / PROJECT / CANDIDATE / REFERENCE / RETIRED`
- `INSTALLED` — 실제 PC에 설치되어 있는가: `YES / NO / UNKNOWN`
- `ACCESS` — 현재 바로 사용 가능한가: `READY / CONNECT / INSTALL / REFERENCE`
- `OPERATOR` — 주 사용 주체: `AI / USER / BOTH`
- `ASSISTANCE` — 사용자 개입: `NONE / ONCE / FREQUENT`
- `AUTONOMY` — 핵심 기능을 AI가 끝까지 운용 가능한가: `AI 단독 가능 / 1회 준비 후 가능 / 부분 가능 / 사용자 중심 / 미확인`
- `BURDEN` — 설치·운영 부담: `⚪ NONE / 🟢 LIGHT / 🟡 MEDIUM / 🟠 HEAVY / 🔴 VERY_HEAVY`
- `COST` — `무료/오픈소스 / 기간 제한 없는 무료 플랜 / 기존 요금제 포함 / 무료체험 / 유료 / 미확인`

실제 채택 상태의 원본은 `01_CONTROL/TOOLS.md`입니다.
실제 설치 버전/위치의 원본은 `01_CONTROL/AI_INSTALLATIONS.md`입니다.
사용 가능 여부·누가 쓰는지·사용자 도움이 필요한지는 `USAGE_AND_ASSISTANCE.md`에서 관리합니다.
사용자 비용·도구 선택 정책의 원본은 `01_CONTROL/USER_POLICIES.md`입니다.
이 파일은 그 원본들과 **후보 조사 문서를 연결하는 인덱스**입니다.

---

## A. 현재 실제 설치/운영 확인된 것

| Tool | ADOPTION | INSTALLED | BURDEN | 메모 |
|---|---|---|---|---|
| GitHub | ACTIVE | service | ⚪ | 저장소/문서/commit/PR |
| Remote Desktop Commander | ACTIVE | connected when online | 🟢 | 실제 PC 작업/검증 |
| ChatGPT Files / Library | ACTIVE | service | ⚪ | 파일 회수 |
| n8n | ACTIVE — local | YES | 🟡 | local bridge 검증됨 |
| MoneyPrinterTurbo | 별도 프로젝트용 | YES — staging | 🟡 | 설치 위치는 AI_INSTALLATIONS 참조 |
| HyperFrames | 별도 프로젝트용 | YES | 🟢~🟡 | npm global |
| Aider | 설치됨, 채택 범위는 별도 | YES | 🟢~🟡 | installer-managed Python env |
| Jev Router | 미채택 / 비활성 | YES | 🟢 | API key 미설정·실제 라우팅 미검증. 추가 API 비용 때문에 사용자 승인 전 활성화 금지 |

> `설치됨 = ACTIVE`가 아닙니다. 실제 채택 상태는 반드시 `01_CONTROL/TOOLS.md` 기준으로 판단합니다.

---

## B. 지금 우선 검토 가치가 큰 후보

| Tool | ADOPTION | BURDEN | 상세 |
|---|---|---|---|
| Vercel Skills | CANDIDATE / HIGH-FIT | 🟢 | `AGENT_SKILLS_10_REPOS.md` |
| OpenViking | CANDIDATE / HIGH-FIT | 🟡 | `AGENT_SKILLS_10_REPOS.md` |
| ECC 일부 기능 | CANDIDATE / HIGH-FIT | 🟡 | `AGENT_SKILLS_10_REPOS.md`, `LLM_STACK_7_REPOS.md` |
| Anthropic Agent Skills | REFERENCE / HIGH-FIT | ⚪ | `AGENT_SKILLS_10_REPOS.md`, `AI_AGENTS_AND_DEV.md` |
| Learn Claude Code | REFERENCE / HIGH-FIT | ⚪ | `AGENT_SKILLS_10_REPOS.md` |
| Firecrawl Cloud/SDK | CANDIDATE / HIGH-FIT | 🟢 | `LLM_STACK_7_REPOS.md`, `50_USEFUL_REPOS.md` |
| browser-use | CANDIDATE / HIGH-FIT | 🟡 | `AI_AGENTS_AND_DEV.md`, `50_USEFUL_REPOS.md` |
| Stagehand | CANDIDATE / HIGH-FIT | 🟡 | `50_USEFUL_REPOS.md` |
| MarkItDown | CANDIDATE / HIGH-FIT | 🟢 | `50_USEFUL_REPOS.md` |
| Mem0 | CANDIDATE / HIGH-FIT | 🟢~🟡 | `50_USEFUL_REPOS.md` |
| Supermemory | CANDIDATE / HIGH-FIT | 🟢 Cloud/MCP / 🟡 local | `50_USEFUL_REPOS.md` |
| OpenHands | CANDIDATE / HIGH-FIT | 🟡~🟠 | `AI_AGENTS_AND_DEV.md` |
| Hermes Agent | CANDIDATE / HIGH-FIT | 🟡 | `AI_AGENTS_AND_DEV.md`, `AGENT_SKILLS_10_REPOS.md` |

### B-1. 가벼운 검증/개발 보조 후보

이 계열은 **전부 설치하는 목록이 아니라 저장소 파일 종류와 실제 문제에 맞춰 최소 세트만 고르는 후보군**입니다.

| Tool | 상태 | BURDEN | 역할 |
|---|---|---|---|
| Ruff / ty | CANDIDATE | 🟢 | Python lint·format / type check |
| ShellCheck / shfmt | CANDIDATE | 🟢 | Bash/sh 분석·포맷 |
| PSScriptAnalyzer | CANDIDATE | 🟢 | PowerShell 정적 분석 |
| actionlint / zizmor | CANDIDATE | 🟢 | GitHub Actions 정합성 / 보안 |
| yamllint | CANDIDATE | 🟢 | 일반 YAML 검사 |
| Tombi | CANDIDATE | 🟢 | TOML format/lint/schema |
| markdownlint-cli2 | CANDIDATE | 🟢 | Markdown 구조·스타일 |
| lychee / typos | CANDIDATE | 🟢 | 링크 / 오타 검사 |
| dprint | CANDIDATE | 🟢 | 여러 언어 formatter 통합 |
| OSV-Scanner | CANDIDATE | 🟢 | dependency/lockfile 취약점 검사 |
| Trivy | CANDIDATE / SELECTIVE | 🟡 | dependency/container/IaC/secret/SBOM 통합 검사 |
| pre-commit / reviewdog | CANDIDATE | 🟢 | 검사 gate / diff·PR 결과 통합 |
| mise | CANDIDATE | 🟢~🟡 | 다중 런타임·환경변수·반복 task 재현; 상세 `MISE.md` |

기본 운용은 `저장소 구조 확인 → 필요한 3~6개 정도 선택 → 검사 → 수정 → diff → 실제 build/test → 재검사` 순서를 우선합니다. 기존 후보와 역할이 사실상 겹치는 새 linter/formatter는 특별한 장점이 없으면 추가하지 않습니다.

---

## C. 콘텐츠 자동화 후보

| Tool | 상태 | BURDEN | 비고 |
|---|---|---|---|
| Firecrawl | CANDIDATE | 🟢 Cloud / 🟠 self-host | 웹 수집 |
| MoneyPrinterTurbo | 설치됨 | 🟡 | 주제→영상 파이프라인 |
| n8n | ACTIVE | 🟡 | workflow orchestration |
| HyperFrames | 설치됨 | 🟢~🟡 | HTML/CSS→영상 렌더 |
| VoxCPM | CANDIDATE | 🟠 model 포함 | 로컬 TTS/voice |
| browser-use | CANDIDATE | 🟡 | browser agent |
| Stagehand | CANDIDATE | 🟡 | browser automation SDK |
| ComfyUI | CANDIDATE | 🔴 | 이미지/영상/3D workflow. 모델/GPU 부담 큼 |

상세: `CONTENT_MEDIA.md`, `AUTOMATION_AND_INTEGRATIONS.md`, `50_USEFUL_REPOS.md`

---

## D. Agent / 개발환경 후보

| Tool | 상태 | BURDEN | 비고 |
|---|---|---|---|
| Vercel Skills | CANDIDATE | 🟢 | Skill package manager 성격 |
| Superpowers | CANDIDATE | 🟢 | 강한 개발 방법론. 일부 Skill 선별 권장 |
| ECC | CANDIDATE | 🟡 | 전체 harness는 규칙 충돌 주의 |
| Agency Agents | REFERENCE | ⚪ | 역할 Agent 정의 모음 |
| Scientific Agent Skills | REFERENCE / PROJECT-FIT | ⚪~🟢 | 과학 프로젝트 전용 |
| Anthropic Agent Skills | REFERENCE | ⚪ | Skill 설계 기준 |
| Awesome Agent Skills | REFERENCE | ⚪ | Skill 검색 인덱스 |
| OpenViking | CANDIDATE | 🟡 | memory/context DB |
| Learn Claude Code | REFERENCE | ⚪ | agent architecture 학습 |
| OpenHands | CANDIDATE | 🟡~🟠 | multi-agent control center |
| CrewAI | CANDIDATE / PROJECT-FIT | 🟢~🟡 | multi-agent framework |
| AutoGen | CANDIDATE / PROJECT-FIT | 🟢~🟡 | multi-agent framework |
| MetaGPT | CANDIDATE / PROJECT-FIT | 🟡 | software-company style agents |
| LangGraph | CANDIDATE / PROJECT-FIT | 🟢~🟡 | long-running/stateful workflow |
| Task Master | CANDIDATE / PROJECT-FIT | 🟢 | task/dependency graph |
| Aider | installed | 🟢~🟡 | terminal pair programmer |
| OpenClaw | CANDIDATE / PROJECT-FIT | 🟡 | local gateway + channels + host tools |
| Bumblebee | CANDIDATE / PROJECT-FIT | 🟢 | read-only dev endpoint inventory; macOS/Linux 중심 |

유료 API/추가 결제가 필요한 Agent 서비스는 기본 후보·자동 연결 대상에서 제외하고, 사용자가 명시적으로 요청한 경우에만 검토합니다.

---

## E. 로컬 AI / LLM platform 후보

| Tool | 상태 | BURDEN | 비고 |
|---|---|---|---|
| Ollama | CANDIDATE / HIGH-FIT | 🟠 | runtime은 단순, 모델이 무거움 |
| Dify | CANDIDATE / PROJECT-FIT | 🟠 self-host | visual workflow/RAG/agent |
| Langflow | CANDIDATE / PROJECT-FIT | 🟡 | Desktop/Python/Docker |
| Open WebUI | CANDIDATE / PROJECT-FIT | 🟡 / 로컬 모델 포함 🟠 | self-host AI workspace |
| LobeHub | CANDIDATE / PROJECT-FIT | 🟡~🟠 | agent team/workspace |
| llama.cpp | CANDIDATE / PROJECT-FIT | 🟠 | GGUF local inference |
| Transformers | REFERENCE / PROJECT-FIT | 🟡 library / 🟠 model | ML/model library |
| vLLM | CANDIDATE / PROJECT-FIT | 🔴 | high-throughput serving |
| DeepSeek model repos | REFERENCE / PROJECT-FIT | 🔴 가능 | 모델별 재평가 필요 |

---

## F. RAG / memory 후보

| Tool | 상태 | BURDEN | 비고 |
|---|---|---|---|
| Mem0 | CANDIDATE / HIGH-FIT | 🟢~🟡 | memory layer |
| Supermemory | CANDIDATE / HIGH-FIT | 🟢 Cloud/MCP / 🟡 local | memory/context engine |
| OpenViking | CANDIDATE / HIGH-FIT | 🟡 | Resource/Memory/Skill context DB |
| LlamaIndex | CANDIDATE / PROJECT-FIT | 🟢~🟡 | data/RAG framework |
| RAGFlow | CANDIDATE / VERIFY-FIRST | 🔴 | 4 cores/16GB RAM/50GB disk+ self-host |

---

## G. 금융 / 조사 후보

| Tool | 상태 | BURDEN | 비고 |
|---|---|---|---|
| TradingAgents | PROJECT-FIT | 🟡 | Investment-Lab 연구 참고 |
| Fincept Terminal | PROJECT-FIT | 🟡 | Market Radar UI/리서치 참고 |
| Flowsint | PROJECT-FIT | 🟡 | OSINT 관계 그래프/조사 UX |
| Maigret | PROJECT-FIT | 🟡 | 공개 username OSINT |

---

## H. 설치하지 않는 참고자료

다음 계열은 `설치 후보`가 아니라 **필요할 때 찾아보는 REFERENCE**입니다.

- public-apis
- build-your-own-x
- developer-roadmap
- free-programming-books
- system-design-primer
- coding-interview-university
- the-art-of-command-line
- project-based-learning
- You-Dont-Know-JS
- the-book-of-secret-knowledge
- tech-interview-handbook
- awesome-selfhosted
- javascript-algorithms
- 30-seconds-of-code
- github/gitignore
- freeCodeCamp
- prompts.chat
- awesome-mcp-servers
- Awesome Agent Skills
- awesome-claude-skills

이런 자료는 AI의 기본 컨텍스트에 항상 넣지 않습니다. 필요할 때만 참조합니다.

---

## 상세 문서 지도

- `USAGE_AND_ASSISTANCE.md` — 지금 바로 사용 가능 여부, AI/사용자 중 누가 쓰는지, 사용자 1회 도움, 연결/설치/GUI 구분
- `INSTALLATION_BURDEN.md` — 설치/운영 무게 기준
- `AGENT_SKILLS_10_REPOS.md` — Agent Skills/Harness 10개
- `50_USEFUL_REPOS.md` — 50개 GitHub 목록 검증본
- `LLM_STACK_7_REPOS.md` — Ollama/Dify/Firecrawl/AutoGPT/Hermes/ECC 등
- `AI_AGENTS_AND_DEV.md` — OpenHands/Hermes/CrewAI/Aider/LangGraph/browser-use 등
- `AUTOMATION_AND_INTEGRATIONS.md` — n8n/Nango/Agentic Inbox
- `CONTENT_MEDIA.md` — MoneyPrinterTurbo/HyperFrames/VoxCPM
- `FINANCE_AND_OSINT.md` — TradingAgents/Fincept/Flowsint
- `SOURCE_BOOKMARKS.md` — 사용자가 준 원문 링크와 공식 원본
- `AMBIGUITIES.md` — 원본 특정 실패/이름 충돌
- 개별 후보 문서 — `DPRINT.md`, `OSV_SCANNER.md`, `TOMBI.md`, `MARKDOWNLINT_CLI2.md`, `YAMLLINT.md`, `JEV_ROUTER.md` 등

사용자에게 보여줄 요약은 `000_사용자용/03_후보_도구_요약.md`와 `000_사용자용/06_AI_도구_플러그인_사용구분.md`에 동기화합니다.

## 정리 원칙

새 후보가 들어오면:

```text
원문 링크 보관
→ 공식 repo/Plugin 원본 확인
→ canonical 이름/redirect 확인
→ 실제 정체 확인
→ ACCESS / OPERATOR / ASSISTANCE / AUTONOMY 구분
→ ADOPTION과 INSTALLED 분리
→ BURDEN 표기
→ Cloud/self-host 차이 기록
→ COST와 외부 실행비용 분리
→ 비용·권한·보안·라이선스 확인
→ 상세 문서 기록
→ MASTER_INDEX 연결
→ 사용자에게 필요한 변화만 000_사용자용에 한글 동기화
```

같은 도구가 여러 SNS 목록에 다시 나오면 상세 설명을 복제하지 않고 기존 항목으로 연결합니다.

마지막 정리: **2026-09-24**