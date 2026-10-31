# 50 Useful GitHub Repos — 2026-09-15 검증/분류본

원문 SNS: https://x.com/AISimplifyX/status/2099347537035726900

SNS 목록은 후보 발견용으로만 보고, 실제 정체·현재 repo 이동·설치 부담·우리 적용성을 다시 분류합니다.

부담 표기: ⚪ 설치 불필요/참고 · 🟢 가벼움 · 🟡 보통 · 🟠 무거움 · 🔴 매우 무거움

## 전체 50개

| # | 후보 | 현재 공식/대표 위치 | 실제 정체 | 부담 | 우리 기준 |
|---:|---|---|---|---|---|
| 1 | iFixAi | 게시물의 `ifixai-ai/iFix`는 현재 404 | AI misalignment testing 주장 | ⚪ | `SOURCE-UNVERIFIED`; 원본 재확인 전 채택 금지 |
| 2 | public-apis | `public-apis/public-apis` | 무료/공개 API 목록 | ⚪ | `REFERENCE`; API 후보 찾을 때 유용 |
| 3 | build-your-own-x | `codecrafters-io/build-your-own-x` | 직접 구현하며 배우는 튜토리얼 인덱스 | ⚪ | `REFERENCE` |
| 4 | developer-roadmap | `kamranahmedse/developer-roadmap` | 기술별 학습 로드맵 | ⚪ | `REFERENCE` |
| 5 | free-programming-books | `EbookFoundation/free-programming-books` | 무료 프로그래밍 자료 인덱스 | ⚪ | `REFERENCE` |
| 6 | system-design-primer | `donnemartin/system-design-primer` | 시스템 설계 학습 자료 | ⚪ | `REFERENCE` |
| 7 | coding-interview-university | `jwasham/coding-interview-university` | CS/면접 학습 커리큘럼 | ⚪ | `REFERENCE` |
| 8 | the-art-of-command-line | `jlevy/the-art-of-command-line` | Shell/CLI 팁 자료 | ⚪ | `REFERENCE` |
| 9 | project-based-learning | `practical-tutorials/project-based-learning` | 프로젝트 기반 학습 링크 모음 | ⚪ | `REFERENCE` |
| 10 | You-Dont-Know-JS | `getify/You-Dont-Know-JS` | JavaScript 책/학습자료 | ⚪ | `REFERENCE` |
| 11 | the-book-of-secret-knowledge | `trimstray/the-book-of-secret-knowledge` | 시스템/보안/CLI 지식 모음 | ⚪ | `REFERENCE`; 명령은 개별 검증 필요 |
| 12 | tech-interview-handbook | `yangshun/tech-interview-handbook` | 기술면접 가이드 | ⚪ | `REFERENCE` |
| 13 | awesome-selfhosted | `awesome-selfhosted/awesome-selfhosted` | self-host 소프트웨어 큐레이션 | ⚪ | `REFERENCE`; 실제 후보는 원본 재검증 |
| 14 | javascript-algorithms | `trekhleb/javascript-algorithms` | JS 알고리즘/자료구조 예제 | ⚪/🟢 | `REFERENCE` |
| 15 | 30-seconds-of-code | `Chalarangelo/30-seconds-of-code` | 짧은 코드/패턴 자료 | ⚪ | `REFERENCE` |
| 16 | gitignore | `github/gitignore` | 공식 `.gitignore` 템플릿 모음 | ⚪ | `HIGH-FIT / REFERENCE` |
| 17 | Ollama | `ollama/ollama` | 로컬 모델 runtime/server | 🟠 | `HIGH-FIT`; 설치는 쉽지만 모델이 무거움 |
| 18 | LangChain | `langchain-ai/langchain` | LLM app/agent framework | 🟢~🟡 | `PROJECT-FIT`; 필요 기능 있을 때만 |
| 19 | n8n | `n8n-io/n8n` | workflow automation | 🟡 | **이미 local ACTIVE**; 설치/브리지 상태는 `01_CONTROL` 참조 |
| 20 | OpenClaw | `openclaw/openclaw` | 로컬 Gateway 기반 개인/팀 AI assistant | 🟡 | `PROJECT-FIT / VERIFY-FIRST`; host tool 권한 큼 |
| 21 | Dify | `langgenius/dify` | visual LLM app/RAG/agent platform | 🟠 self-host | `PROJECT-FIT`; Cloud는 훨씬 가벼움 |
| 22 | Langflow | `langflow-ai/langflow` | visual AI workflow/agent builder + MCP/API | 🟡 | `PROJECT-FIT`; Desktop/Python/Docker 가능 |
| 23 | Mem0 | `mem0ai/mem0` | Agent memory layer | 🟢~🟡 | `HIGH-FIT / VERIFY-FIRST`; Hub memory 비교 후보 |
| 24 | browser-use | `browser-use/browser-use` | AI browser automation | 🟡 | `HIGH-FIT`; 브라우저/로그인/약관 검증 필요 |
| 25 | CrewAI | `crewAIInc/crewAI` | multi-agent orchestration | 🟢~🟡 | `PROJECT-FIT`; 단순 작업에는 과함 |
| 26 | MetaGPT | 현재 `FoundationAgents/MetaGPT` | software-company형 multi-agent framework | 🟡 | `PROJECT-FIT`; 게시물의 `geekan/MetaGPT`는 현재 이동됨 |
| 27 | AutoGen | `microsoft/autogen` | Microsoft multi-agent framework | 🟢~🟡 | `PROJECT-FIT`; orchestration 필요 시 비교 |
| 28 | Aider | `Aider-AI/aider` | terminal AI pair programmer | 🟢~🟡 | **이미 설치됨**; 실제 채택 상태는 `01_CONTROL` 참조 |
| 29 | MarkItDown | `microsoft/markitdown` | 문서/파일을 Markdown으로 변환 | 🟢 | `HIGH-FIT`; 자료 ingest에 실용적 |
| 30 | Open WebUI | `open-webui/open-webui` | self-host AI workspace/UI | 🟡, 로컬 모델 포함 시 🟠 | `PROJECT-FIT`; Ollama/CUDA 번들은 더 무거움 |
| 31 | Maigret | `soxoj/maigret` | username 기반 OSINT 수집 | 🟡 | `PROJECT-FIT`; 공개정보 조사에 한정 |
| 32 | TradingAgents | `TauricResearch/TradingAgents` | multi-LLM 금융 분석 연구 framework | 🟡 | `PROJECT-FIT`; Investment-Lab 참고 |
| 33 | Stagehand | `browserbase/stagehand` | AI browser automation SDK | 🟡 | `HIGH-FIT`; browser-use와 비교 대상 |
| 34 | Firecrawl | 현재 `firecrawl/firecrawl` | web search/scrape/crawl API | 🟢 Cloud / 🟠 self-host | `HIGH-FIT`; 게시물의 `mendableai` 경로는 현재 redirect |
| 35 | Transformers | `huggingface/transformers` | 대규모 ML/model library | 🟡 라이브러리 / 🟠 모델 | `PROJECT-FIT / REFERENCE` |
| 36 | vLLM | `vllm-project/vllm` | 고성능 LLM inference/serving | 🔴 | `PROJECT-FIT`; GPU/model serving 목적일 때만 |
| 37 | llama.cpp | `ggml-org/llama.cpp` | GGUF 중심 로컬 LLM inference | 🟠 | `PROJECT-FIT`; 모델 파일/빌드 옵션이 부담 |
| 38 | LlamaIndex | `run-llama/llama_index` | data/RAG/agent framework | 🟢~🟡 | `PROJECT-FIT` |
| 39 | nanoGPT | `karpathy/nanoGPT` | GPT 학습/구현 참고 | 🟡, 실제 학습은 🟠~🔴 | `REFERENCE / PROJECT-FIT` |
| 40 | RAGFlow | `infiniflow/ragflow` | 전체 RAG/Agent 플랫폼 | 🔴 | `VERIFY-FIRST`; 공식 self-host 최소 4 cores/16GB RAM/50GB disk |
| 41 | Supermemory | `supermemoryai/supermemory` | AI memory/context engine | 🟢 Cloud/MCP / 🟡 local | `HIGH-FIT / VERIFY-FIRST`; Hub memory 비교 후보 |
| 42 | awesome-claude-skills | `ComposioHQ/awesome-claude-skills` | Claude/Agent Skill 큐레이션 | ⚪ | `REFERENCE`; 원본 Skill별 검증 필요 |
| 43 | Bumblebee | `perplexityai/bumblebee` | 개발 endpoint의 패키지/extension/MCP metadata read-only inventory | 🟢 | `PROJECT-FIT`; **일반 보안툴이 아님**, 현재 macOS/Linux 중심 |
| 44 | ComfyUI | 현재 `Comfy-Org/ComfyUI` | image/video/audio/3D 생성 workflow engine | 🔴 | `PROJECT-FIT`; 앱보다 모델/checkpoint/GPU가 진짜 무거움 |
| 45 | DeepSeek | `deepseek-ai` 조직 | 여러 모델/repo의 공식 조직 | 🔴 로컬 대형모델 | `REFERENCE`; 하나의 설치 프로그램으로 취급 금지 |
| 46 | Lobe Chat | 현재 `lobehub/lobehub` | Agent team/workspace + self-host platform | 🟡~🟠 | `PROJECT-FIT`; 예전 `lobe-chat` 이름에서 확장/변경됨 |
| 47 | freeCodeCamp | `freeCodeCamp/freeCodeCamp` | 무료 학습 플랫폼/커리큘럼 | ⚪ 사용 / 🟠 전체 개발환경 | `REFERENCE` |
| 48 | coding-interview-university | #7과 동일 | 중복 | ⚪ | `DUPLICATE` |
| 49 | LangChain | #18과 동일 | 중복 | 🟢~🟡 | `DUPLICATE` |
| 50 | n8n | #19와 동일 | 중복 | 🟡 | `DUPLICATE` |

## 게시물에서 실제로 바로잡아야 할 점

1. **50개가 실제로는 50개의 서로 다른 repo가 아닙니다.** #48, #49, #50은 앞 항목과 중복입니다.
2. `iFixAi`의 게시물 경로 `ifixai-ai/iFix`는 2026-09-15 현재 확인되지 않았습니다. 비슷한 이름의 fork/복제 후보만 검색되어 원본 특정 전까지 `SOURCE-UNVERIFIED`로 둡니다.
3. `MetaGPT`는 현재 `FoundationAgents/MetaGPT`로 확인됩니다.
4. `Firecrawl`은 현재 `firecrawl/firecrawl`이 canonical repo입니다.
5. `ComfyUI`는 현재 `Comfy-Org/ComfyUI`로 이동되어 있습니다.
6. `Lobe Chat`은 현재 `lobehub/lobehub`으로 확장/이름이 바뀐 상태입니다.
7. `Bumblebee`는 막연한 “Perplexity 보안툴”이 아니라 **개발자 endpoint의 패키지/확장/MCP 구성 metadata를 읽는 supply-chain exposure inventory collector**입니다.
8. `DeepSeek`은 하나의 앱/repo가 아니라 여러 모델 저장소를 가진 조직이므로 모델별로 다시 평가해야 합니다.

## 🔴 진짜 무거운 것만 따로

### 1) ComfyUI
Desktop/Portable 설치 자체는 쉬워졌지만, 실제 모델/checkpoint와 PyTorch/CUDA가 무겁습니다. 이미지 몇 모델만 쓰는 경우와 비디오/3D/대형 workflow까지 쓰는 경우의 부담 차이가 큽니다.

### 2) vLLM
`uv pip install vllm` 자체보다 GPU serving stack과 모델이 본체입니다. 개인 PC에서 가볍게 쓰는 도구가 아니라 고성능 inference server 성격이 강합니다.

### 3) RAGFlow self-host
공식 요구사항부터 CPU 4 cores+, RAM 16 GB+, Disk 50 GB+, Docker/Compose입니다. 여러 backend 서비스를 함께 띄우는 플랫폼입니다.

### 4) DeepSeek 대형 모델 로컬 실행
모델별 크기 차이가 매우 커서 하나의 부담 등급으로 뭉뚱그릴 수 없지만, 대형 모델 로컬 실행은 서버급 자원을 요구할 수 있습니다.

## 🟠 무겁지만 상황 따라 쓸 만한 것

- Ollama — 모델을 제한하면 관리 가능
- Dify self-host — Cloud 이용 시 설치 부담 크게 감소
- Firecrawl self-host — Hosted API 사용 시 로컬 부담 거의 없음
- Open WebUI + Ollama/CUDA — 외부 API만 쓰면 훨씬 가벼움
- llama.cpp + 큰 GGUF — 작은 quantized 모델만 쓰면 부담 감소
- Transformers + 큰 PyTorch 모델 — 라이브러리보다 모델/torch 환경이 무거움
- LobeHub self-host — Docker/DB/agent 운영까지 가면 중간 이상

## 우리에게 당장 가치가 큰 신규 후보

### 바로 비교할 가치
- `MarkItDown` — PDF/Office/HTML 등 자료를 Markdown으로 넘기는 ingest 보조
- `Mem0` / `Supermemory` — Project Operations Hub의 장기 기억 계층과 비교
- `Stagehand` — `browser-use`와 브라우저 자동화 비교
- `OpenClaw` — 여러 채널/도구를 묶는 로컬 Gateway 구조 참고
- `gitignore` — 새 프로젝트 템플릿에서 바로 재사용 가능

### 구체적인 프로젝트가 생기면
- `Dify` / `Langflow` — 시각적 AI workflow
- `CrewAI` / `AutoGen` / `MetaGPT` — multi-agent orchestration
- `LlamaIndex` / `RAGFlow` — RAG/knowledge system
- `TradingAgents` — Investment-Lab 연구 참고
- `ComfyUI` — 이미지/영상 생성 파이프라인이 실제로 필요할 때

### 참고자료로만 두기
학습/면접/로드맵/awesome 계열은 설치 후보로 다루지 않고 `REFERENCE`로 유지합니다. AI가 매 작업마다 읽게 하지 않습니다.

상세 설치 부담 기준: `INSTALLATION_BURDEN.md`

마지막 확인: **2026-09-15**
