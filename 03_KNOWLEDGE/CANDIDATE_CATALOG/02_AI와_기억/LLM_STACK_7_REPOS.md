# 7 GitHub Repos — 실제 검증 메모 (2026-09-15)

사용자가 전달한 X 게시물의 7개 저장소를 공식 GitHub/README 기준으로 다시 확인한 문서입니다.
SNS의 `무료`, `어떤 모델도`, `스스로 학습`, `전체 스택` 같은 표현은 그대로 믿지 않고 실제 설치·비용·라이선스·우리 적용처를 분리합니다.

## 결론 먼저

이 7개는 **서로 딱 맞물리는 하나의 무료 스택이 아닙니다.** 역할이 겹치는 도구도 많고, self-host가 가능해도 모델 API·GPU·서버·운영비가 따로 들 수 있습니다.

우리 기준 우선순위:
1. **Firecrawl — HIGH-FIT**: Threads/콘텐츠 수집·웹 조사 파이프라인에 직접 쓸모 큼.
2. **ECC — VERIFY-FIRST / HIGH-FIT**: Codex/Agent 작업 규율·Skill·검증에 강하지만 기존 AGENTS/Project Operations Hub와 충돌 가능성이 커 격리 테스트 필수.
3. **Ollama — PROJECT-FIT**: 로컬 모델 실행/비용 절감/오프라인 처리에 실용적. GPU VRAM에 따라 모델 선택 제한.
4. **Dify — PROJECT-FIT**: 시각적 Workflow/RAG/Agent 앱 제작에 좋지만 현재 작업에는 다소 무거울 수 있음.
5. **Hermes Agent — VERIFY-FIRST**: 지속 기억·Skill 생성·메신저·스케줄 자동화가 강한 독립 Agent. 권한 범위와 장기 실행 안전성 확인 필요.
6. **AutoGPT — WATCH / PROJECT-FIT**: 현재는 agent platform 쪽이 중심. Dify/Hermes/n8n과 겹쳐 우선순위는 낮음.
7. **prompts.chat — REFERENCE**: 프롬프트 자료/MCP/Plugin 참고용. 인프라 도구는 아님.

---

## 01. Ollama
- 공식: https://github.com/ollama/ollama
- 정체: 로컬에서 LLM을 내려받아 실행하고 API로 제공하는 런타임.
- 라이선스: MIT.
- 실제 장점: Windows/macOS/Linux에서 로컬 모델을 간단히 띄우고 다른 앱이 로컬 API로 연결 가능.
- 홍보문구 보정: `어떤 오픈 모델도`는 과장. Ollama가 지원하는 형식/모델과 내 하드웨어 한계가 있음.
- 비용: 소프트웨어 라이선스/API 요금은 없을 수 있지만 GPU/전력/저장공간 비용은 존재. 모델 자체 라이선스도 각각 다름.
- 우리 적용: 로컬 요약·분류·초안·민감자료 처리, API 비용 절감 실험. RTX 4070 Super 12GB에서는 중소형 양자화 모델 위주가 현실적.
- 판단: **PROJECT-FIT / 테스트 가치 높음**.

## 02. Dify
- 공식: https://github.com/langgenius/dify
- 정체: 시각적 LLM 앱/Workflow/RAG/Agent 제작 플랫폼.
- 실제 기능: visual workflow, 모델 연결, RAG, Agent, Prompt IDE, 로그/관측, API 제공. Docker self-host 지원.
- 라이선스: 일반 Apache-2.0 그대로가 아니라 **Dify Open Source License**. 멀티테넌트 운영·프론트 로고/저작권에 추가 조건이 있음.
- 비용: self-host 코드 자체는 사용할 수 있지만 외부 LLM/API, 서버, DB 등의 비용은 별도.
- 우리 적용: 콘텐츠 자동화나 사내형 도구를 시각적으로 조립할 때 유용. 단순 개발 보조 용도로는 과함.
- 판단: **PROJECT-FIT**.

## 03. Firecrawl
- 공식: https://github.com/firecrawl/firecrawl
- 정체: 웹페이지/사이트를 검색·스크랩·크롤링해 LLM용 Markdown/구조화 데이터로 만드는 도구.
- 실제 기능: JS 페이지 처리, scrape/crawl/search, self-host, CLI/MCP 계열 연동.
- 라이선스: 코어는 **AGPL-3.0**, SDK/일부 UI는 MIT. Cloud에는 추가 기능이 있음.
- 비용: self-host는 라이선스비가 없어도 서버/프록시/브라우저 자원 비용이 들 수 있고 Cloud API는 별도 서비스.
- 주의: robots.txt, 사이트 약관, 로그인/유료벽, anti-bot을 무시해도 된다는 뜻이 아님.
- 우리 적용: **Threads AI Lab의 공개 웹 수집/기사·블로그·커뮤니티 자료 정리 후보 1순위**. DC/Blind 등은 각 사이트 정책·로그인·차단 구조를 별도 검토.
- 판단: **HIGH-FIT**.

## 04. prompts.chat
- 공식: https://github.com/f/prompts.chat
- 정체: 대규모 오픈 프롬프트 라이브러리 + self-host 사이트 + CLI/MCP/Plugin 연동.
- 라이선스: 사이트 코드/책은 MIT, prompt dataset은 CC0로 공개된 부분이 있음.
- 실제 장점: 프롬프트 패턴 참고, MCP/Plugin으로 검색해 쓰기 좋음.
- 한계: 좋은 프롬프트를 모아둔 자료이지 Agent runtime이나 자동화 엔진은 아님.
- 우리 적용: Project Operations Hub의 prompt/skill 참고자료, 콘텐츠 초안 패턴 수집.
- 판단: **REFERENCE**.

## 05. AutoGPT
- 공식: https://github.com/Significant-Gravitas/AutoGPT
- 정체: 현재는 Agent를 build/deploy/run하는 플랫폼이 중심. 예전 `AutoGPT Classic`은 별도 구역으로 남아 있음.
- 실제 기능: 시각적 builder, trigger/schedule 실행, 외부 서비스 integration, self-host.
- 현재 중요한 점: **Classic은 과거 실험 계열이고 현재 주력은 Platform**.
- 라이선스: `autogpt_platform/`은 Polyform Shield, classic/기타는 MIT. 즉 저장소 전체가 단순 MIT는 아님.
- 비용: self-host 자체에 라이선스비가 없더라도 모델 API·인프라 비용은 사용자가 부담.
- 우리 적용: 장기 자동화 Agent 후보지만 Dify/Hermes/n8n과 역할이 많이 겹침.
- 판단: **WATCH / PROJECT-FIT**. 지금 당장 전부 설치할 필요 없음.

## 06. Hermes Agent
- 공식: https://github.com/NousResearch/hermes-agent
- 정체: 지속 기억, Skill 생성/개선, 도구 사용, 스케줄, 메신저 연결을 갖춘 독립 Agent.
- 라이선스: MIT.
- 실제 기능: 여러 모델 provider, MCP, persistent memory, Telegram/Discord/Slack/WhatsApp/Signal 등 gateway, cron, toolsets.
- `스스로를 가르친다`의 실제 의미: 모델 자체를 계속 재훈련한다기보다 **작업 경험을 Skill/기억으로 저장하고 다음 작업에 재사용하는 구조**에 가깝다.
- 우리 적용: 장기 실행 개인 Agent/서버형 자동화 실험에 적합.
- 위험: terminal/MCP/메신저/cron을 한 번에 주면 권한 범위가 넓어짐. 빈 테스트 환경에서 허용 도구와 승인 경계를 먼저 확인해야 함.
- 판단: **VERIFY-FIRST**.

## 07. ECC
- 공식: https://github.com/affaan-m/ECC
- 정체: Codex, Claude Code, Cursor, OpenCode 등용 **Agent harness / Skill·Hook·Rule·MCP·검증 체계 묶음**.
- 현재 확인: Codex plugin manifest가 있고 skills, MCP config, hooks, rules, quality gate, audit, autonomous loop 계열이 포함됨. 2026년 현재 릴리스도 계속 갱신 중.
- 라이선스: MIT.
- 실제 장점: TDD, security review, quality gate, agent 운영 패턴을 한꺼번에 제공.
- 홍보문구 보정: `운영체제`는 제품 포지셔닝 표현. 실제로는 기존 Agent 위에 붙는 harness/config/tooling 묶음에 가깝다.
- 우리 적용: Project Operations Hub와 매우 비슷한 영역을 건드림. 잘 쓰면 보강되지만 그대로 설치하면 AGENTS/Skill/Hook가 중복돼 오히려 AI가 느려지거나 충돌할 수 있음.
- 권장 테스트: 별도 빈 저장소 + minimal/selective 설치 → 생성 파일 diff 확인 → Codex 동작/품질 gate 확인 → 필요한 Skill만 선별 채택.
- 판단: **HIGH-FIT지만 VERIFY-FIRST**.

---

## 게시물의 `전체 스택` 문구에 대한 실제 평가

게시물은 `Ollama 실행 → Dify 구축 → Firecrawl 공급 → prompts.chat 안내 → AutoGPT 자동화 → Hermes 진화 → ECC 조율`처럼 설명하지만, 실제 제품들은 그렇게 일렬로 의존하는 구조가 아닙니다.

현실적인 조합 예시는 오히려 다음처럼 작게 고르는 편이 낫습니다.

```text
콘텐츠 수집 자동화
Firecrawl → 기존 LLM/ChatGPT 또는 Ollama → n8n/Dify 중 하나 → 게시 전 검수

로컬 AI
Ollama → 필요한 UI/Agent 하나

코딩 Agent 강화
Codex + Project Operations Hub
        + ECC에서 필요한 Skill/Quality Gate만 선택

장기 개인 Agent
Hermes Agent 단독 테스트
```

## 바로 설치하지 않는 이유

7개를 모두 설치하면 Docker, 모델 런타임, Agent memory, cron, MCP, hooks, rules가 겹쳐 관리 지점과 공격면이 급격히 늘어납니다.
따라서 `유명함/Star 수`가 아니라 **현재 프로젝트에 필요한 기능 하나씩** 검증해 채택합니다.

### 추천 다음 테스트
1. Firecrawl: 공개 페이지 5~10개를 Markdown으로 수집하고 품질/속도/차단 여부 확인.
2. ECC: 빈 Codex 테스트 저장소에 minimal 설치 후 생성 파일·hooks·AGENTS 충돌 확인.
3. Ollama: 12GB VRAM에서 실제 사용할 1~2개 모델로 속도/품질/메모리 측정.
4. 나머지는 위 3개 결과가 나온 뒤 필요할 때만 테스트.
