# Candidate Catalog — 2026-09-15 조사본

사용자가 전달한 GitHub 프로젝트·웹사이트·SNS 링크를 **실제 원본 README/사이트 기준으로 다시 확인해 정리한 후보 카탈로그**입니다.

## 먼저 볼 것

1. [`MASTER_INDEX.md`](MASTER_INDEX.md) — 전체 후보의 **현재 상태 + 설치 여부 + 설치 부담 + 상세문서 위치**를 한 번에 보는 canonical index
2. [`INSTALLATION_BURDEN.md`](INSTALLATION_BURDEN.md) — `⚪/🟢/🟡/🟠/🔴` 설치·운영 무게 기준
3. 각 주제별 상세문서 — 실제 기능/설치/라이선스/우리 적용처 확인
4. [`SOURCE_BOOKMARKS.md`](SOURCE_BOOKMARKS.md) — 사용자가 준 원문 링크와 공식 원본

`후보 문서에 등장함 = 설치됨 = 채택됨`이 아닙니다.

- 실제 채택 상태 원본: `01_CONTROL/TOOLS.md`
- 실제 PC 설치 버전/경로 원본: `01_CONTROL/AI_INSTALLATIONS.md`
- 후보 조사/비교 원본: 이 `CANDIDATE_CATALOG/`

## 분류

| 문서 | 내용 |
|---|---|
| [MASTER_INDEX.md](MASTER_INDEX.md) | 모든 후보의 canonical 인덱스. ADOPTION / INSTALLED / BURDEN 분리 |
| [INSTALLATION_BURDEN.md](INSTALLATION_BURDEN.md) | 설치·운영 부담 기준 + 🔴 VERY_HEAVY 별도 분류 |
| [AI_AGENTS_AND_DEV.md](AI_AGENTS_AND_DEV.md) | OpenHands, Hermes Agent, CrewAI, Aider, LangGraph, browser-use, awesome-mcp-servers, Task Master, LibreChat, Anthropic Agent Skills |
| [AGENT_SKILLS_10_REPOS.md](AGENT_SKILLS_10_REPOS.md) | Vercel Skills, Superpowers, ECC, Hermes, Scientific Agent Skills, Agency Agents, Anthropic Skills, Awesome Agent Skills, OpenViking, Learn Claude Code |
| [50_USEFUL_REPOS.md](50_USEFUL_REPOS.md) | 사용자가 전달한 50-repo SNS 목록을 중복/이동/설치부담까지 다시 검증 |
| [LLM_STACK_7_REPOS.md](LLM_STACK_7_REPOS.md) | Ollama, Dify, Firecrawl, prompts.chat, AutoGPT, Hermes Agent, ECC — SNS 홍보문구와 실제 기능/비용/라이선스/우리 적용성 비교 |
| [AUTOMATION_AND_INTEGRATIONS.md](AUTOMATION_AND_INTEGRATIONS.md) | n8n, Agentic Inbox, Nango |
| [CONTENT_MEDIA.md](CONTENT_MEDIA.md) | MoneyPrinterTurbo, HyperFrames, VoxCPM |
| [FINANCE_AND_OSINT.md](FINANCE_AND_OSINT.md) | TradingAgents, Fincept Terminal, Flowsint |
| [UI_DESIGN_REFERENCES.md](UI_DESIGN_REFERENCES.md) | SceneAI, dashboard 디자인 참고자료, AGENTS/Skills 경량화 참고 |
| [SOURCE_BOOKMARKS.md](SOURCE_BOOKMARKS.md) | 사용자가 준 원문 링크와 확인 상태 |
| [AMBIGUITIES.md](AMBIGUITIES.md) | 정확한 원본을 아직 특정하지 못한 이름/링크와 처리 원칙 |

## 상태 표기

### 채택/적합성
- `ACTIVE` — 허브에서 실제 채택
- `PROJECT` — 특정 프로젝트에서 실제 채택
- `CANDIDATE` — 후보
- `HIGH-FIT` — 현재 프로젝트와 직접 연결점이 큼
- `PROJECT-FIT` — 특정 프로젝트에서만 가치가 큼
- `REFERENCE` — 설치 도구라기보다 참고자료/아이디어 원천
- `SOURCE-UNVERIFIED` — 원본 repo/본문을 현재 기준으로 확정하지 못함

### 설치 부담
- ⚪ `NONE / REFERENCE`
- 🟢 `LIGHT`
- 🟡 `MEDIUM`
- 🟠 `HEAVY`
- 🔴 `VERY_HEAVY`

Cloud/API는 가볍고 self-host는 무거운 경우 두 등급을 따로 씁니다.

## 이번 조사에서 바로잡은 홍보문구

- **50 useful repos** 목록은 실제로 50개의 서로 다른 repo가 아닙니다. 마지막 3개는 앞 항목 중복이며 일부 repo는 이름/조직이 이동했습니다.
- 게시물의 `ifixai-ai/iFix`는 2026-09-15 현재 공식 원본으로 확인되지 않아 `SOURCE-UNVERIFIED`로 분리했습니다.
- `MetaGPT`는 현재 `FoundationAgents/MetaGPT`, `Firecrawl`은 `firecrawl/firecrawl`, `ComfyUI`는 `Comfy-Org/ComfyUI`, `Lobe Chat`은 `lobehub/lobehub` 기준으로 봅니다.
- **Bumblebee**는 범용 AI security scanner가 아니라 read-only package/extension/MCP metadata inventory collector에 가깝습니다.
- **ComfyUI**는 앱 설치보다 모델/checkpoint/PyTorch/CUDA/VRAM/디스크가 실제 부담의 핵심이라 `VERY_HEAVY`로 따로 분리합니다.
- **RAGFlow self-host**는 공식 요구사항부터 CPU 4 cores+, RAM 16 GB+, Disk 50 GB+라 `VERY_HEAVY`입니다.
- **vLLM**은 설치 명령 자체보다 GPU/model serving 환경이 본체라 `VERY_HEAVY`로 봅니다.
- **7-repo LLM stack**은 하나의 무료 통합 스택이 아닙니다. Firecrawl/Dify/AutoGPT/Hermes/ECC는 역할이 겹치며 self-host여도 모델 API·GPU·서버·운영비가 별도일 수 있습니다.
- **Ollama**는 로컬 모델 runtime으로 유용하지만 지원 형식·VRAM·모델 라이선스 제약이 있습니다.
- **Dify**는 일반 Apache-2.0 그대로가 아니라 추가 조건이 있는 Dify Open Source License입니다.
- **Firecrawl** 코어는 AGPL-3.0이고 Cloud에는 추가 기능이 있으며 scraping 대상 사이트 정책과 robots/anti-bot 문제는 별도입니다.
- **AutoGPT**의 현재 주력은 Platform이며 `autogpt_platform/`은 Polyform Shield, Classic/기타는 MIT입니다.
- **Hermes Agent**의 self-improving은 모델 자체를 계속 재훈련한다는 뜻보다는 작업 경험을 Skill/기억으로 축적·재사용하는 구조에 가깝습니다.
- **ECC**의 `Agent Harness OS`는 포지셔닝 표현이며 실제로는 Codex/Claude Code 등에 붙는 Skill·Hook·Rule·MCP·검증 체계 묶음입니다.
- **Vercel Skills**는 Skill 품질/안전성을 보증하는 앱스토어가 아니라 여러 Agent에 Skill을 찾고 설치·업데이트하는 CLI입니다.
- **Superpowers**는 단순 Skill 모음보다 강한 개발 방법론에 가깝습니다. 전체 전역 적용 시 Hub의 action-first 방식과 절차 충돌 가능성이 있습니다.
- **OpenViking**은 일반 메모 앱이 아니라 Resource/Memory/Skill을 통합 관리하는 Agent용 context database입니다.
- **Learn Claude Code**는 완성형 코딩 Agent 제품이라기보다 Agent loop·tool·memory·skills·subagents·MCP 등을 단계별로 배우는 교육/참고 구현입니다.
- **n8n**은 흔히 “오픈소스 Zapier”라고 불리지만 공식 설명은 fair-code workflow automation입니다.
- **Fincept Terminal**은 “무료 Bloomberg”라기보다 오픈 에디션 금융 리서치 터미널로 보는 게 정확합니다.
- **HyperFrames**는 영상 생성 모델이 아니라 HTML/CSS/애니메이션을 결정론적으로 MP4로 렌더링하는 프레임워크입니다.
- **TradingAgents**는 자동수익 보장 시스템이 아니라 다중 LLM 기반 금융 분석/트레이딩 연구 프레임워크입니다.
- SNS 게시물의 Star 수, 출시 후 성장 속도, “완전 자동”, “무료” 같은 문구는 스냅샷/홍보 표현일 수 있어 **원본 저장소의 현재 기능·라이선스·설치 요구사항을 우선**합니다.

## 현재 우선 검토 순서

### 콘텐츠 자동화 쪽
1. `Firecrawl` — 우선 Cloud/SDK 경로. self-host는 HEAVY
2. `MoneyPrinterTurbo` — 이미 설치된 파이프라인 후보
3. `n8n` — 이미 local ACTIVE
4. `HyperFrames` — 이미 설치됨
5. `browser-use` / `Stagehand` — browser 자동화 비교
6. `ComfyUI` — 필요 시 별도 🔴 heavy 실험. 무심코 설치하지 않음

### 개발환경 쪽
1. `Vercel Skills`
2. `OpenViking`
3. `ECC` 일부 기능
4. `Learn Claude Code`
5. `Anthropic Agent Skills`
6. `OpenHands`
7. `Hermes Agent`
8. `Aider` — 이미 설치됨
9. `Superpowers` / `Agency Agents` — 필요한 것만 선별
10. `CrewAI` / `AutoGen` / `MetaGPT` / `LangGraph` — 구체적인 멀티 Agent 요구가 있을 때

### memory/RAG 쪽
1. `Mem0`
2. `Supermemory`
3. `OpenViking`
4. `LlamaIndex`
5. `RAGFlow` — 🔴 VERY_HEAVY, 필요성이 명확할 때만

### 로컬 AI 쪽
1. `Ollama` — 모델을 제한해서 시험
2. `Open WebUI` — 외부 API만 연결하면 중간급, 로컬 모델 포함 시 HEAVY
3. `Dify` / `Langflow` — visual workflow가 필요할 때
4. `llama.cpp` — 직접 로컬 inference를 세밀하게 제어할 때
5. `vLLM` — 🔴 server급 inference 목적일 때

### 주식/리서치 쪽
1. `TradingAgents`
2. `Fincept Terminal`
3. `Flowsint`

마지막 확인일: **2026-09-15**
