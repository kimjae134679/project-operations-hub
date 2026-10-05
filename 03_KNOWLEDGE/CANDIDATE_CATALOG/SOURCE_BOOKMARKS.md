# Source Bookmarks — 2026-09-15

사용자가 직접 전달한 링크 + 조사 중 확인한 공식 원본을 한 곳에 보관합니다.

상태:
- `VERIFIED` — 현재 조사 환경에서 실제 페이지/README 확인
- `BOOKMARKED` — 링크는 보관
- `SOURCE-UNVERIFIED` — 링크 접근/본문 확인이 현재 조사 환경에서 실패. 내용은 추측하지 않음

---

## 사용자가 직접 준 웹/SNS 링크

| 상태 | 링크 | 정리 |
|---|---|---|
| VERIFIED | https://sceneai.art/ | UI/landing page/background용 AI prompt library. Premium 영역 존재 |
| SOURCE-UNVERIFIED | https://x.com/Aura_lixx/status/2099285949268279353 | X 원문은 현재 web cache에서 본문 조회 실패. 사용자가 준 링크 그대로 보관 |
| VERIFIED | https://kucharski.substack.com/p/ten-reasons-your-vibe-coded-dashboard | 2026-09-02 dashboard 사용자 여정/시각 위계 디자인 비평 |
| SOURCE-UNVERIFIED | https://x.com/shanyanggm/status/2099196413649490398 | X 원문은 현재 web cache에서 본문 조회 실패. 사용자가 준 링크 그대로 보관 |
| BOOKMARKED | https://x.com/ayush26291/status/2099345366005244288 | `7 GitHub Repos / LLM Stack`; 공식 GitHub 원본 기준으로 재검증 |
| BOOKMARKED | https://x.com/DivyanshT91162/status/2099439107122597950 | `10 Agent Skills / Harness repos`; `AGENT_SKILLS_10_REPOS.md`에 재검증 |
| BOOKMARKED | https://x.com/AISimplifyX/status/2099347537035726900 | `50 Useful GitHub Repos`; 중복/이동/설치 부담까지 `50_USEFUL_REPOS.md`에 재검증 |

### X 링크 처리 원칙
스크린샷/사용자 전달 요약은 후보 발견용입니다. 실제 채택 판단은 공식 저장소의 현재 README, 설치 요구사항, 라이선스, canonical repo를 우선합니다.

---

# 공식 GitHub — 50 Useful Repos 재검증

## 학습/참고
- public-apis — https://github.com/public-apis/public-apis
- build-your-own-x — https://github.com/codecrafters-io/build-your-own-x
- developer-roadmap — https://github.com/kamranahmedse/developer-roadmap
- free-programming-books — https://github.com/EbookFoundation/free-programming-books
- system-design-primer — https://github.com/donnemartin/system-design-primer
- coding-interview-university — https://github.com/jwasham/coding-interview-university
- the-art-of-command-line — https://github.com/jlevy/the-art-of-command-line
- project-based-learning — https://github.com/practical-tutorials/project-based-learning
- You-Dont-Know-JS — https://github.com/getify/You-Dont-Know-JS
- the-book-of-secret-knowledge — https://github.com/trimstray/the-book-of-secret-knowledge
- tech-interview-handbook — https://github.com/yangshun/tech-interview-handbook
- awesome-selfhosted — https://github.com/awesome-selfhosted/awesome-selfhosted
- javascript-algorithms — https://github.com/trekhleb/javascript-algorithms
- 30-seconds-of-code — https://github.com/Chalarangelo/30-seconds-of-code
- gitignore — https://github.com/github/gitignore
- freeCodeCamp — https://github.com/freeCodeCamp/freeCodeCamp

## AI / Agent / Automation
- Ollama — https://github.com/ollama/ollama
- LangChain — https://github.com/langchain-ai/langchain
- n8n — https://github.com/n8n-io/n8n
- OpenClaw — https://github.com/openclaw/openclaw
- Dify — https://github.com/langgenius/dify
- Langflow — https://github.com/langflow-ai/langflow
- Mem0 — https://github.com/mem0ai/mem0
- browser-use — https://github.com/browser-use/browser-use
- CrewAI — https://github.com/crewAIInc/crewAI
- MetaGPT — https://github.com/FoundationAgents/MetaGPT
- AutoGen — https://github.com/microsoft/autogen
- Aider — https://github.com/Aider-AI/aider
- MarkItDown — https://github.com/microsoft/markitdown
- Open WebUI — https://github.com/open-webui/open-webui
- Maigret — https://github.com/soxoj/maigret
- TradingAgents — https://github.com/TauricResearch/TradingAgents
- Stagehand — https://github.com/browserbase/stagehand
- Firecrawl — https://github.com/firecrawl/firecrawl
- Transformers — https://github.com/huggingface/transformers
- vLLM — https://github.com/vllm-project/vllm
- llama.cpp — https://github.com/ggml-org/llama.cpp
- LlamaIndex — https://github.com/run-llama/llama_index
- nanoGPT — https://github.com/karpathy/nanoGPT
- RAGFlow — https://github.com/infiniflow/ragflow
- Supermemory — https://github.com/supermemoryai/supermemory
- awesome-claude-skills — https://github.com/ComposioHQ/awesome-claude-skills
- Bumblebee — https://github.com/perplexityai/bumblebee
- ComfyUI — https://github.com/Comfy-Org/ComfyUI
- DeepSeek organization — https://github.com/deepseek-ai
- LobeHub — https://github.com/lobehub/lobehub

### 원본/이름 보정
- `ifixai-ai/iFix` — 2026-09-15 현재 404. 원본 특정 전 `SOURCE-UNVERIFIED`.
- `geekan/MetaGPT` → 현재 `FoundationAgents/MetaGPT`로 이동 확인.
- `mendableai/firecrawl` → 현재 `firecrawl/firecrawl`로 redirect/canonical 확인.
- `comfyanonymous/ComfyUI` → 현재 `Comfy-Org/ComfyUI`로 이동 확인.
- `lobehub/lobe-chat` → 현재 `lobehub/lobehub`으로 이동/확장 확인.
- 게시물 #48/#49/#50은 각각 coding-interview-university / LangChain / n8n의 중복.

상세 비교: `50_USEFUL_REPOS.md`

---

# 공식 GitHub — 7-repo LLM stack 재검증

- Ollama — https://github.com/ollama/ollama
- Dify — https://github.com/langgenius/dify
- Firecrawl — https://github.com/firecrawl/firecrawl
- prompts.chat — https://github.com/f/prompts.chat
- AutoGPT — https://github.com/Significant-Gravitas/AutoGPT
- Hermes Agent — https://github.com/NousResearch/hermes-agent
- ECC — https://github.com/affaan-m/ECC

상세 비교: `LLM_STACK_7_REPOS.md`

---

# 공식 GitHub — Agent Skills / Harness 10개 재검증

- Vercel Skills — https://github.com/vercel-labs/skills
- Superpowers — https://github.com/obra/superpowers
- ECC — https://github.com/affaan-m/ECC
- Hermes Agent — https://github.com/NousResearch/hermes-agent
- Scientific Agent Skills — https://github.com/K-Dense-AI/scientific-agent-skills
- Agency Agents — https://github.com/msitarzewski/agency-agents
- Anthropic Agent Skills — https://github.com/anthropics/skills
- Awesome Agent Skills — https://github.com/VoltAgent/awesome-agent-skills
- OpenViking — https://github.com/volcengine/OpenViking
- Learn Claude Code — https://github.com/shareAI-lab/learn-claude-code

상세 비교: `AGENT_SKILLS_10_REPOS.md`

---

# 공식 GitHub — AI Agent / 개발

- OpenHands — https://github.com/OpenHands/OpenHands
- Hermes Agent — https://github.com/NousResearch/hermes-agent
- CrewAI — https://github.com/crewAIInc/crewAI
- Aider — https://github.com/Aider-AI/aider
- LangGraph — https://github.com/langchain-ai/langgraph
- browser-use — https://github.com/browser-use/browser-use
- awesome-mcp-servers — https://github.com/punkpeye/awesome-mcp-servers
- Task Master / claude-task-master — https://github.com/eyaltoledano/claude-task-master
- LibreChat — https://github.com/danny-avila/LibreChat
- Anthropic Agent Skills — https://github.com/anthropics/skills
- Agent Skills specification — https://agentskills.io/

---

# 공식 GitHub — 자동화 / 외부 서비스 연동

- n8n — https://github.com/n8n-io/n8n
- Agentic Inbox — https://github.com/cloudflare/agentic-inbox
- Nango — https://github.com/NangoHQ/nango

---

# 공식 GitHub — 콘텐츠 / 영상 / 음성

- MoneyPrinterTurbo — https://github.com/harry0703/MoneyPrinterTurbo
- HyperFrames — https://github.com/heygen-com/hyperframes
- VoxCPM / VoxCPM2 — https://github.com/OpenBMB/VoxCPM

---

# 공식 GitHub — 금융 / 조사

- TradingAgents — https://github.com/TauricResearch/TradingAgents
- Fincept Terminal — https://github.com/Fincept-Corporation/FinceptTerminal
- Flowsint — https://github.com/reconurge/flowsint

---

# 후보 조사 흐름

```text
SNS/스크린샷에서 후보 발견
→ 원문 링크 보관
→ 공식 GitHub/사이트 확인
→ canonical repo/redirect 확인
→ 실제 기능 확인
→ 설치·OS·runtime 요구사항 확인
→ 설치 부담 등급 기록
→ 라이선스/비용 확인
→ ADOPTION / INSTALLED 상태 분리
→ 우리 프로젝트 적용처 구분
→ 실제 테스트 후에만 ACTIVE/PROJECT 승격
```

Star 수는 매우 빠르게 변하므로 핵심 판단 기준으로 쓰지 않습니다.
