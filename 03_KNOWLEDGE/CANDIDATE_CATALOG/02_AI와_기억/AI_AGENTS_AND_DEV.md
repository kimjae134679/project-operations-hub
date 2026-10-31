# AI Agents & Developer Tools

확인 기준: 공식 GitHub README/조직 페이지, 2026-09-15.

---

## 1. OpenHands

- 공식: https://github.com/OpenHands/OpenHands
- 종류: **독립형 AI 개발 Agent 플랫폼 / Agent control center**
- 상태: `HIGH-FIT`, 설치 전 격리 테스트 권장

### 현재 정체
현재 OpenHands 저장소는 단순 “코딩 Agent 하나”보다 **Agent Canvas**라는 self-hosted 개발자 control center 성격을 강하게 내세웁니다. OpenHands 자체 Agent뿐 아니라 Claude Code, Codex, Gemini 및 ACP 호환 Agent를 local/remote/cloud backend로 연결할 수 있습니다.

### 주요 기능
- coding agent 대화/실행
- local, Docker, VM, cloud backend 선택
- schedule/webhook 기반 automation
- GitHub, Slack, Linear 등과 연결
- 여러 Agent backend를 한 UI에서 전환
- BYO model

### 설치/운영 포인트
- Windows에서는 Docker 또는 공식 Windows 가이드를 먼저 확인
- Agent가 접근할 `PROJECTS_PATH`를 명시적으로 제한
- source 실행은 Node.js/uv 등 개발환경을 요구
- 직접 host filesystem에 붙이는 방식은 권한이 매우 큼

### 우리한테 의미
`Project Operations Hub`와 궁합을 볼 가치가 큽니다. 특히 **여러 프로젝트·여러 Agent를 한 control center에서 돌리는 구조**는 현재 허브가 문서로 하던 일부 역할을 실제 실행 계층으로 확장할 수 있습니다.

### 위험/주의
- project root 전체를 mount하면 Agent 권한이 넓어짐
- 자동화 webhook/schedule을 활성화하면 “사용자 승인 없이 외부 변경”이 생기지 않도록 scope 제한 필요
- 먼저 테스트 repo 1개만 연결해 파일 읽기/수정/rollback을 확인

---

## 2. Hermes Agent — Nous Research

- 공식: https://github.com/NousResearch/hermes-agent
- 종류: **독립형 self-improving personal AI agent**
- 상태: `HIGH-FIT`, 별도 테스트 환경 권장

### 현재 정체
공식 README는 Hermes를 **built-in learning loop를 가진 self-improving AI agent**로 설명합니다. 작업 경험에서 Skill을 만들고, 사용 중 Skill을 개선하고, 과거 대화를 검색하고, 세션을 넘어 사용자 모델/기억을 유지하는 구조를 강조합니다.

### 주요 기능
- persistent memory
- task 경험에서 Skill 생성 및 개선
- 과거 대화 검색/요약
- terminal TUI
- Telegram/Discord/Slack/WhatsApp/Signal gateway
- cron 기반 scheduled automation
- subagent 병렬 위임
- local/Docker/SSH/Modal/Daytona/Vercel Sandbox 등 여러 backend
- MCP 연결
- OpenAI/OpenRouter/Nous Portal/자체 endpoint 등 모델 교체

### Windows
공식 README 기준 native Windows PowerShell 설치를 지원하며, WSL2도 선택할 수 있습니다.

### 우리한테 의미
현재 허브가 README/AGENTS/communication thread로 유지하는 **장기 기억·Skill 축적·프로젝트 간 맥락**을 실제 Agent runtime이 어느 정도 맡을 수 있는지 비교할 후보입니다.

### 주의
- “스스로 Skill을 만든다”는 기능은 강력하지만, 오래된/잘못된 행동을 학습해 누적할 위험도 있음
- 자동 memory/skill 생성 범위를 Workbench의 `현재 실행본` 원칙과 맞춰야 함
- shell, messaging gateway, cron을 한꺼번에 켜지 말고 단계별 권한 검증 필요

---

## 3. CrewAI

- 공식: https://github.com/crewAIInc/crewAI
- 종류: **멀티 Agent orchestration framework**
- 상태: `PROJECT-FIT`

### 정체
역할이 다른 autonomous agent들을 crew로 묶어 복합 작업을 나누고 협업시키는 프레임워크입니다.

### 적합한 경우
- 조사 담당 / 구현 담당 / 검증 담당처럼 역할을 명시적으로 분리
- 반복되는 복잡 workflow를 코드로 고정
- “Agent 여러 명이 서로 대화하도록” 직접 시스템을 만들고 싶을 때

### 우리한테 의미
현재 `04_COMMUNICATION`은 GitHub 파일을 통한 느슨한 비동기 소통입니다. CrewAI는 그보다 한 단계 아래 실행 계층에서 **동일 런 안의 역할 분담**을 만들 때 쓸 수 있습니다.

### 주의
- 단순 작업까지 crew로 만들면 token/cost/디버깅이 크게 늘어남
- “Agent 수가 많다 = 품질이 높다”가 아님
- 실제 필요가 있는 워크플로에만 도입

---

## 4. Aider

- 공식: https://github.com/Aider-AI/aider
- 종류: **터미널 AI pair programmer**
- 상태: `HIGH-FIT`

### 주요 기능
- 기존 codebase map을 만들어 큰 저장소 문맥 파악
- 거의 모든 주요 언어 지원
- Git diff/commit과 강하게 통합
- 수정 후 lint/test 수행 가능
- 이미지/웹페이지 컨텍스트 입력
- local/cloud LLM 사용 가능

### 우리한테 의미
Remote Desktop + GitHub로 기존 저장소를 고치는 작업에서 **작은 diff를 빠르게 만들고 Git 이력으로 되돌릴 수 있는 도구**라는 점이 맞습니다.

### 적용 후보
- 작은 버그 수정
- 반복 refactor
- 테스트와 함께 수정
- 기존 codebase에서 대규모 rewrite보다 patch 중심 작업

### 주의
자동 commit은 편하지만, 우리 규칙상 commit이 곧 ACCEPTED는 아닙니다. build/실행/실기기 검증은 별도로 유지해야 합니다.

---

## 5. LangGraph

- 공식: https://github.com/langchain-ai/langgraph
- 종류: **stateful/long-running Agent orchestration framework**
- 상태: `PROJECT-FIT`

### 핵심
LangGraph는 graph 구조로 Agent/workflow 상태를 연결하고 long-running 작업을 durable하게 실행하는 low-level orchestration layer입니다.

### 강점
- durable execution / failure 후 resume
- stateful workflow
- human-in-the-loop
- 장기 실행 Agent

### 우리한테 의미
Investment-Lab, 콘텐츠 자동화, 장기 리서치처럼 **중간상태를 잃지 않고 여러 단계가 이어지는 파이프라인**을 직접 코드로 만들 때 후보입니다.

### 주의
단순한 cron + 몇 단계 API 호출이면 n8n이나 간단한 코드가 더 싸고 관리하기 쉬울 수 있습니다.

---

## 6. browser-use

- 공식: https://github.com/browser-use/browser-use
- 종류: **AI browser automation library/platform**
- 상태: `HIGH-FIT / VERIFY-FIRST`

### 하는 일
Agent가 브라우저를 열어 페이지를 탐색하고, 클릭하고, 입력하고, 정보를 추출하는 일을 자동화합니다.

### 사용 후보
- 공개 웹페이지 조사
- 반복 폼 입력
- 사람이 하던 브라우저 절차 자동화
- 콘텐츠 파이프라인의 자료 수집/검수

### 우리한테 의미
Threads/Instagram/커뮤니티 자료 조사 흐름에서 브라우저 자동화 계층 후보입니다.

### 중요한 제한
- 로그인 세션, CAPTCHA, anti-bot, 사이트 약관은 별도 문제
- DC/Blind/Instagram 같은 서비스에서 자동 수집/게시가 허용되는지는 각 서비스 정책을 따로 확인
- 개인정보/비공개 글/유료벽 우회용으로 사용하지 않음
- 자동 클릭은 항상 테스트 계정/안전한 사이트에서 먼저 검증

---

## 7. awesome-mcp-servers

- 공식: https://github.com/punkpeye/awesome-mcp-servers
- 종류: **MCP 서버 큐레이션 목록**
- 상태: `REFERENCE`

### 정체
프로그램 하나가 아니라 GitHub, DB, 브라우저, SaaS 등과 Agent를 연결하는 **MCP 서버 후보 목록**입니다.

### 우리한테 의미
새 도구가 필요할 때 무작정 검색하기보다 여기서 후보를 찾고, 원본 repo로 이동해 보안/권한/유지보수 상태를 다시 검증하는 인덱스로 사용합니다.

### 주의
목록에 올라와 있다고 안전/공식/유지보수됨을 보장하지 않습니다. 각 MCP는 별도 검증이 필요합니다.

---

## 8. Task Master (`claude-task-master`)

- 공식: https://github.com/eyaltoledano/claude-task-master
- 종류: **AI 개발 task/dependency 관리 시스템 + CLI/MCP**
- 상태: `PROJECT-FIT`

### 정체
자연어 요구사항을 task/subtask/dependency 구조로 쪼개고, 여러 AI coding tool에서 작업 순서를 관리하는 시스템입니다. 이름은 Claude Task Master지만 Cursor, Windsurf, Roo 등 여러 환경을 대상으로 확장돼 있습니다.

### 우리한테 의미
프로젝트가 길어지면서 “뭘 먼저 고쳐야 하는지”가 뒤섞이는 경우에 사용 가치가 있습니다.

### 주의
우리 Workbench가 이미 프로젝트 상태/다음 작업을 관리하므로 중복 가능성이 큽니다. Task Master를 도입한다면 **execution task graph**만 맡기고 README/AGENTS의 정책 원본 역할까지 넘기지 않는 게 맞습니다.

---

## 9. LibreChat

- 공식: https://github.com/danny-avila/LibreChat
- 종류: **self-hosted multi-provider AI chat UI/platform**
- 상태: `PROJECT-FIT`

### 현재 기능 범위
- OpenAI, Anthropic, Gemini, Azure, OpenRouter 등 여러 provider
- 모델 전환
- Agents
- MCP
- Skills
- Code Interpreter
- Artifacts
- OpenAPI actions/functions
- multi-user auth

### 우리한테 의미
“ChatGPT·Claude·Gemini를 한 화면에서 모아서 쓴다”는 설명은 대체로 맞지만, 현재는 그보다 훨씬 큰 **self-hosted AI workspace**입니다.

### 적용 후보
Sol/Astra/다른 모델을 한 UI에서 비교할 필요가 커질 때 검토.

### 주의
직접 host하면 인증·provider key·conversation DB·업데이트를 사용자가 관리해야 하므로 단순 편의용으로는 운영비용이 큼.

---

## 10. Anthropic Agent Skills

- 공식: https://github.com/anthropics/skills
- 표준: https://agentskills.io/
- 종류: **Skill 예제 + specification + template**
- 상태: `HIGH-FIT / REFERENCE`

### 정체
Skill은 `SKILL.md`와 선택적인 script/resource를 묶어 Agent가 특정 작업을 반복적으로 잘하게 만드는 구조입니다. 공식 저장소에는 creative/design, technical, enterprise, document 계열 예제가 포함됩니다.

### 우리한테 중요한 점
SNS에서 말한 “모델이 바뀌면 AGENTS/Skills도 같이 진화시켜야 한다”는 방향과 연결됩니다. 다만 핵심은 파일을 무조건 줄이는 게 아니라:

- trigger를 너무 넓게 잡지 않기
- 관련 없는 문서를 강제로 읽히지 않기
- 중복 Skill을 합치기
- 오래된 확인 요청/충돌 규칙 제거
- 위험 작업의 안전 경계와 필요한 테스트는 유지

입니다.

### Workbench 적용
`AGENTS.md = 현재 실행 라우터`, `Skill = 특정 작업을 수행할 때만 불러오는 전문 절차`로 역할을 분리하면 가장 자연스럽습니다.

### 다음 실험
새 Skill을 바로 전역 설치하지 말고 테스트 프로젝트 하나에서:
1. trigger가 필요한 순간에만 발동하는지
2. 불필요한 파일을 읽지 않는지
3. 기존 AGENTS와 충돌하지 않는지
4. 작업 완료 시간이 실제 줄어드는지
확인합니다.
