# Candidate Master Index — canonical index

이 파일을 **후보 도구 전체의 1차 진입점**으로 사용합니다.

상세 설명은 각 주제 문서에 남기되, 현재 상태/설치 부담/어디를 봐야 하는지는 여기에서 먼저 확인합니다.

## 상태를 섞지 않기

서로 다른 축입니다.

- `ADOPTION` — 실제로 채택했는가: `ACTIVE / PROJECT / CANDIDATE / REFERENCE / RETIRED`
- `INSTALLED` — 실제 PC에 설치되어 있는가: `YES / NO / UNKNOWN`
- `BURDEN` — 설치·운영 부담: `⚪ NONE / 🟢 LIGHT / 🟡 MEDIUM / 🟠 HEAVY / 🔴 VERY_HEAVY`

실제 채택 상태의 원본은 `01_CONTROL/TOOLS.md`입니다.
실제 설치 버전/위치의 원본은 `01_CONTROL/AI_INSTALLATIONS.md`입니다.
이 파일은 그 둘을 **후보 조사 문서와 연결하는 인덱스**입니다.

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

- `AGENT_SKILLS_10_REPOS.md` — Agent Skills/Harness 10개
- `50_USEFUL_REPOS.md` — 이번 50개 게시물 검증본
- `LLM_STACK_7_REPOS.md` — Ollama/Dify/Firecrawl/AutoGPT/Hermes/ECC 등
- `AI_AGENTS_AND_DEV.md` — OpenHands/Hermes/CrewAI/Aider/LangGraph/browser-use 등
- `AUTOMATION_AND_INTEGRATIONS.md` — n8n/Nango/Agentic Inbox
- `CONTENT_MEDIA.md` — MoneyPrinterTurbo/HyperFrames/VoxCPM
- `FINANCE_AND_OSINT.md` — TradingAgents/Fincept/Flowsint
- `INSTALLATION_BURDEN.md` — 설치/운영 무게 기준
- `SOURCE_BOOKMARKS.md` — 사용자가 준 원문 링크와 공식 원본
- `AMBIGUITIES.md` — 원본 특정 실패/이름 충돌

## 정리 원칙

새 후보가 들어오면:

```text
원문 링크 보관
→ 공식 repo 확인
→ canonical 이름/redirect 확인
→ 실제 정체 확인
→ ADOPTION과 INSTALLED를 분리
→ BURDEN 표기
→ 상세 주제 문서에 기록
→ MASTER_INDEX에 한 줄 연결
```

같은 도구가 여러 SNS 목록에 다시 나오면 상세 설명을 복제하지 않고 기존 항목으로 연결합니다.

마지막 정리: **2026-09-15**
