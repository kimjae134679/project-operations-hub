# Agent Skills / Harness 10 Repos — 2026-09-15 검증본

원문 SNS: https://x.com/DivyanshT91162/status/2099439107122597950

SNS 요약은 후보 발견용으로만 보고, 아래 평가는 각 공식 GitHub 저장소와 현재 README 기준입니다. 전부 아직 `CANDIDATE`이며 설치·채택 상태가 아닙니다.

## 결론 먼저

| 후보 | 실제 정체 | 현재 판단 | Project Operations Hub 관점 |
|---|---|---|---|
| Vercel Skills | Agent Skill 검색·설치·업데이트 CLI | `HIGH-FIT / VERIFY-FIRST` | 가장 먼저 시험할 가치 큼 |
| Superpowers | Skill 기반 개발 방법론/워크플로 | `PROJECT-FIT / VERIFY-FIRST` | 일부 Skill만 골라 쓰는 편이 적합 |
| ECC | 대형 Agent harness: Skills/Agents/Hooks/Rules/MCP | `HIGH-FIT / VERIFY-FIRST` | 강력하지만 기존 규칙과 충돌 가능성 큼 |
| Hermes Agent | 기억·Skill 학습·도구·스케줄을 가진 독립 Agent | `HIGH-FIT` | 이미 별도 문서에서 상세 검토됨 |
| Scientific Agent Skills | 과학·연구용 대규모 Skill 모음 | `PROJECT-FIT / REFERENCE` | 과학 작업이 생길 때만 |
| Agency Agents | 전문 역할별 Agent persona 모음 | `REFERENCE / PROJECT-FIT` | 수백 개 전체보다 필요한 역할만 선별 |
| Anthropic Agent Skills | Agent Skills 예제·표준·템플릿 | `HIGH-FIT / REFERENCE` | Skill 설계 기준으로 매우 유용 |
| Awesome Agent Skills | Skill 큐레이션 인덱스 | `REFERENCE` | 새 Skill 찾는 검색 출발점 |
| OpenViking | Memory/Knowledge/Skill 통합 Context Database | `HIGH-FIT / VERIFY-FIRST` | Hub의 장기 기억 계층 후보 |
| Learn Claude Code | 실제 Agent 구조를 단계별 구현하는 학습 저장소 | `HIGH-FIT / REFERENCE` | 구조 아이디어를 가져오기에 매우 좋음 |

## 1. Vercel Skills
공식: https://github.com/vercel-labs/skills

### 실제 정체
`npx skills`로 여러 Agent의 `SKILL.md` 패키지를 찾고, 설치하고, 업데이트하고, 제거하는 CLI입니다. 현재 README는 OpenCode, Claude Code, Codex, Cursor를 포함한 다수 Agent를 지원한다고 명시합니다.

주요 흐름:
- `npx skills find ...` — 검색
- `npx skills add owner/repo` — 설치
- `npx skills add ... --list` — 설치 전 목록 확인
- `npx skills use ...` — 설치하지 않고 일회성 사용
- `npx skills list/update/remove/init` — 관리
- 프로젝트 범위와 global 범위를 구분

### 실제 평가
우리처럼 Codex/Claude/기타 Agent의 Skill을 함께 관리하려는 환경에 가장 직접적으로 맞습니다. 다만 CLI가 원격 Git 저장소의 Skill을 받아오기 때문에 **설치기 자체가 안전성을 대신 보장하지는 않습니다.** 출처·스크립트·권한은 별도 검토해야 합니다.

**판단: `HIGH-FIT / VERIFY-FIRST`**

---

## 2. Superpowers
공식: https://github.com/obra/superpowers

### 실제 정체
브레인스토밍, 계획, TDD, 체계적 디버깅, 코드 리뷰, subagent 작업 등으로 구성된 **Skill 기반 소프트웨어 개발 방법론**입니다.

### 실제 평가
좋은 Skill이 많지만 기본 철학은 꽤 강합니다. 일부 핵심 Skill은 “적용 가능성이 조금이라도 있으면 먼저 Skill을 호출”하는 식으로 절차 준수를 강하게 요구합니다.

우리의 `필요한 만큼만 읽고 바로 실제 작업` 원칙과 충돌할 가능성이 있으므로 전체를 전역 규칙처럼 넣는 것보다:
- systematic debugging
- TDD
- review
같은 필요한 Skill만 격리해서 시험하는 편이 맞습니다.

**판단: `PROJECT-FIT / VERIFY-FIRST`**

---

## 3. ECC
공식: https://github.com/affaan-m/ECC

### 실제 정체
Claude Code, Codex 등 위에 얹는 대형 **Agent harness**입니다. 현재 저장소는 Skills, Agents, Hooks, Rules, MCP, memory/security/verification 계층을 한꺼번에 제공합니다.

### 실제 평가
기능은 매우 풍부하지만 이미 Project Operations Hub가 맡고 있는 역할과 상당히 겹칩니다.

겹치는 영역:
- AGENTS / 규칙
- 검증 gate
- Skill 관리
- memory
- 역할 Agent
- 보안 검사
- workflow

따라서 전체 전역 설치는 context 증가와 규칙 충돌 위험이 큽니다. 먼저 테스트 저장소에서 필요한 Skill/기능만 선별해 기존 Workbench보다 실제 속도·품질이 좋아지는지 비교해야 합니다.

**판단: `HIGH-FIT / VERIFY-FIRST`**

상세 기존 조사: `LLM_STACK_7_REPOS.md`

---

## 4. Hermes Agent
공식: https://github.com/NousResearch/hermes-agent

### 실제 정체
작업 경험으로 Skill을 만들고 개선하며, persistent memory·과거 대화 검색·subagent·MCP·cron·messaging gateway 등을 가진 **독립형 개인 Agent runtime**입니다.

### 실제 평가
SNS의 “사용자와 함께 성장한다”는 표현은 방향은 맞지만 모델 자체를 계속 재훈련한다는 뜻보다는 **기억과 Skill을 축적·재사용하는 learning loop**에 가깝습니다.

우리 Hub의 장기 맥락/Skill 축적을 실제 실행 Agent가 대신할 수 있는지 볼 후보지만, 자동으로 쌓이는 기억과 Skill도 stale될 수 있으므로 현재 실행본을 깨끗하게 유지하는 정책이 필요합니다.

**판단: `HIGH-FIT`, 별도 격리 테스트 필요**

상세 기존 조사: `AI_AGENTS_AND_DEV.md`, `LLM_STACK_7_REPOS.md`

---

## 5. Scientific Agent Skills
공식: https://github.com/K-Dense-AI/scientific-agent-skills

### 실제 정체
논문, 생물학, 화학, 의약, 데이터베이스 등 과학 연구에 특화된 대규모 Agent Skill 모음입니다. 여러 coding agent에서 쓰는 Agent Skills 구조를 따릅니다.

### 실제 평가
도구 자체는 탄탄한 전문 Skill 카탈로그이지만 현재 주요 프로젝트에는 직접 연결점이 적습니다. 연구/논문/바이오·화학 작업이 생겼을 때 선택적으로 설치하는 편이 맞습니다. 저장소 전체 라이선스와 개별 Skill의 외부 라이선스는 따로 확인해야 합니다.

**판단: `PROJECT-FIT / REFERENCE`**

---

## 6. Agency Agents
공식: https://github.com/msitarzewski/agency-agents

### 실제 정체
개발, QA, 디자인, 마케팅, 보안 등 서로 다른 역할·성격·작업방식을 가진 **전문화 Agent persona/definition 모음**입니다. Codex 등 여러 Agent 환경용 변환/설치 흐름도 제공합니다.

### 실제 평가
수백 개 Agent를 전부 넣는 것은 우리 환경에는 과합니다. 대신 필요한 역할을 골라 참고하기 좋습니다.

예:
- Security Engineer
- QA / Reality Checker
- Researcher / Evidence Collector
- UI/UX specialist

Agent 정의는 주로 문서이지만 설치 스크립트나 executable이 포함되는 경우에는 별도 검토가 필요합니다.

**판단: `REFERENCE / PROJECT-FIT`**

---

## 7. Anthropic Agent Skills
공식: https://github.com/anthropics/skills
표준: https://agentskills.io/

### 실제 정체
`SKILL.md` 중심 Agent Skill 구조의 예제, specification, template을 제공하는 공식 참고 저장소입니다.

### 실제 평가
우리에게는 “전부 설치할 Skill 팩”보다 **Skill을 어떻게 작고 명확하게 설계할지 보는 기준서**에 가깝습니다.

권장 역할 분리:
- `AGENTS.md` = 현재 작업을 어디로 라우팅할지
- `Skill` = 그 작업이 필요할 때만 불러오는 전문 절차

이 방식이 Hub의 `AGENTS는 현재 실행본` 원칙과 잘 맞습니다.

**판단: `HIGH-FIT / REFERENCE`**

상세 기존 조사: `AI_AGENTS_AND_DEV.md`

---

## 8. Awesome Agent Skills
공식: https://github.com/VoltAgent/awesome-agent-skills

### 실제 정체
여러 제작자·팀의 Agent Skill을 모아 놓은 **큐레이션 인덱스**입니다. 실행 runtime이나 통합 설치 프로그램이 핵심은 아닙니다.

### 실제 평가
새 기능이 필요할 때 처음부터 Skill을 만들기 전에 후보를 찾는 검색 카탈로그로 유용합니다. 다만 목록에 있다는 이유로 안전성·유지보수·라이선스가 보장되는 것은 아니므로 실제 설치는 원본 저장소를 다시 확인해야 합니다.

**판단: `REFERENCE`**

---

## 9. OpenViking
공식: https://github.com/volcengine/OpenViking

### 실제 정체
Agent가 쓰는 **Resource + Memory + Skill을 하나의 context database로 관리**하는 시스템입니다. `viking://` 가상 파일시스템, 계층형 context loading, 검색, cross-session memory 등을 제공합니다.

### 우리한테 중요한 이유
현재 Project Operations Hub는 GitHub Markdown으로:
- 프로젝트 기억
- 교훈
- Skill 후보
- handoff
- 대화
을 보존합니다.

OpenViking은 이 중 일부를 실제 검색 가능한 context 계층으로 바꿀 가능성이 있습니다. 즉 이번 10개 중 장기적으로는 가장 흥미로운 후보 중 하나입니다.

### 주의
동기화하는 자료가 서비스/backend로 전달될 수 있으므로 private repo, 사용자 데이터, credential이 섞인 환경에서는 데이터 경계를 먼저 정해야 합니다. 기존 Hub를 바로 대체하지 말고 테스트 repo 하나로 memory/search 정확도와 삭제/갱신 정책부터 확인합니다.

**판단: `HIGH-FIT / VERIFY-FIRST`**

---

## 10. Learn Claude Code
공식: https://github.com/shareAI-lab/learn-claude-code

### 실제 정체
상용 Agent 제품이 아니라 **Agent harness가 내부에서 어떻게 작동하는지 단계별로 직접 구현해보는 교육/참고 저장소**입니다.

다루는 주제에는 tool loop, permission, hooks, task system, subagent, Skill loading, context compaction, memory, error recovery, background task, cron, agent team, worktree isolation, MCP 등이 포함됩니다.

### 실제 평가
직접 설치해 우리 일을 대신시키는 도구라기보다, Project Operations Hub를 앞으로 개선할 때 **어떤 구조를 가져오고 어떤 복잡성은 버릴지 배우는 참고자료**로 가치가 큽니다.

특히 참고 가치가 큰 부분:
- context compaction
- permission pipeline
- task/subagent 경계
- memory 구조
- worktree isolation
- agent-team protocol

**판단: `HIGH-FIT / REFERENCE`**

---

# 우리 기준 우선순위

## A. 바로 작은 실험을 해볼 가치
1. **Vercel Skills** — Skill 검색/설치/업데이트 계층
2. **OpenViking** — Hub용 memory/context 계층 후보
3. **ECC의 일부 기능** — 전체 설치가 아니라 필요한 Skill/검증 기능만

## B. 구조와 Skill을 가져올 가치
4. **Learn Claude Code** — Agent 구조 참고
5. **Anthropic Agent Skills** — Skill 표준/설계 기준
6. **Awesome Agent Skills** — Skill 후보 검색 인덱스

## C. 필요한 프로젝트에서만
7. **Superpowers** — 디버깅/TDD/review 등 일부 Skill 선별
8. **Agency Agents** — 역할 Agent 소수만 선별
9. **Hermes Agent** — 독립형 장기 Agent가 필요할 때 비교
10. **Scientific Agent Skills** — 과학/연구 프로젝트 전용

# 설치 전에 지킬 것

```text
후보 발견
→ 공식 원본 확인
→ 필요한 기능만 고르기
→ 테스트 repo/격리 환경
→ 기존 AGENTS/Skills와 충돌 확인
→ 실제 작업 시간·품질·context 사용량 비교
→ 이득이 확인될 때만 PROJECT/ACTIVE 승격
```

특히 `ECC + Superpowers + Agency Agents + 여러 Skill pack`을 한꺼번에 전역 설치하는 방식은 피합니다. 기능이 많아지는 만큼 규칙 충돌과 context noise가 늘 수 있습니다.
