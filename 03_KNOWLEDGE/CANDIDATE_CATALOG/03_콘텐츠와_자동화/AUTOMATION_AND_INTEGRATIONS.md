# Automation & Integrations

확인 기준: 공식 GitHub README/조직 페이지, 2026-09-15.

---

## 1. n8n

- 공식: https://github.com/n8n-io/n8n
- 종류: **workflow automation platform**
- 상태: `HIGH-FIT / VERIFY-FIRST`
- 라이선스 성격: 공식 표현은 **fair-code**. 일반적인 의미의 완전한 오픈소스와 동일하게 적지 않음.

### 정체
노드 기반 workflow builder에 JavaScript/Python, AI Agent 기능, 각종 SaaS/API 연결을 섞을 수 있는 자동화 플랫폼입니다. 공식 조직 설명은 400+ integrations와 native AI capability를 강조합니다.

### 우리한테 가장 큰 의미
콘텐츠 자동화에서 실제 “접착제” 역할을 맡길 수 있습니다.

예:

```text
DC/Blind/웹 자료 수집
→ 중복 제거/필터
→ LLM 요약·재작성
→ 이미지/영상 생성 queue
→ 사용자 검수
→ Instagram/YouTube/Threads 게시
→ 결과 로그/실패 재시도
```

또 청약/주식/메일/리포트처럼 주기적 작업에도 재사용 가능합니다.

### 장점
- 시각적 workflow
- self-host 가능
- webhook/cron/event 기반
- custom code 삽입 가능
- AI node/agent workflow 지원
- 여러 API를 한 flow로 묶기 쉬움

### 주의
- self-host = 운영 책임도 직접 가짐
- credential 저장/백업/업데이트 관리 필요
- community node를 무조건 신뢰하지 않음
- 2026-09 초에도 보안 advisory가 계속 올라오고 있으므로 self-host 시 최신 stable 유지
- 외부 사이트 scraping/자동게시 약관은 n8n이 해결해주지 않음

### 추천 검증
테스트 flow 하나만 먼저:
`RSS/공개 URL 1개 → 요약 → Google Sheet/파일 저장`처럼 비용·권한이 낮은 것부터 검증 후 확대.

---

## 2. Agentic Inbox — Cloudflare

- 공식: https://github.com/cloudflare/agentic-inbox
- 종류: **self-hosted AI email client on Cloudflare Workers**
- 상태: `PROJECT-FIT`

### 정체
Cloudflare 계정 안에서 돌아가는 이메일 클라이언트 + AI Agent입니다.

공식 구조:
- Cloudflare Email Routing으로 수신
- mailbox마다 Durable Object
- SQLite 기반 mailbox storage
- attachment는 R2
- Cloudflare Workers에서 동작

### 사용자가 준 설명과의 차이
“데이터가 자기 쪽에 그대로 있다”는 말은 대체로 **자기 Cloudflare account/인프라에 배치한다**는 의미로 보는 게 정확합니다. 로컬 PC에만 저장되는 구조라고 적으면 안 됩니다.

### 우리한테 의미
- 특정 프로젝트 메일함 자동 분류
- 결과 메일 감지
- 초안 작성
- 프로젝트별 알림 routing

청약/서비스 가입/개발 알림처럼 메일이 중요한 프로젝트에서 별도 inbox를 만들 때 후보입니다.

### 주의
- Cloudflare 계정/도메인/Email Routing 구성 필요
- AI가 자동 발송까지 하게 만들 경우 외부 메시지 승인 규칙과 충돌하지 않도록 “초안 우선”으로 시작
- 개인 메일 전체를 바로 옮기지 말고 프로젝트 전용 주소로 테스트

---

## 3. Nango

- 공식: https://github.com/NangoHQ/nango
- 종류: **API integration / OAuth / sync / action infrastructure**
- 상태: `HIGH-FIT / PROJECT-FIT`
- 현재 공식 설명: 900+ APIs 지원
- 라이선스: 현재 repo는 Elastic License 계열. “무조건 완전 오픈소스”라고 단순 표기하지 않음.

### 정체
앱이나 AI Agent가 외부 서비스를 연결할 때 생기는 OAuth, token refresh, credential storage, proxy, sync/action 실행을 한 계층으로 묶는 기반입니다.

### 핵심 구성
1. **Auth** — OAuth/API key/token refresh
2. **Proxy** — 인증된 API request 전달
3. **Functions** — TypeScript integration logic 실행
4. sync/webhook/action/observability

### 우리한테 의미
프로젝트가 늘면서 각자 Google/Slack/Notion/GitHub/Instagram 계열 인증 코드를 따로 만들기 시작하면 Nango 같은 integration layer가 가치가 생깁니다.

### n8n과 차이

```text
n8n
= workflow 자체를 시각적으로 연결/실행

Nango
= 외부 API 인증·연동을 제품/Agent에 안정적으로 붙이는 기반
```

즉 둘은 경쟁이라기보다 역할이 다를 수 있습니다.

### 언제 검토할까
- 동일한 OAuth를 여러 프로젝트에서 반복 구현할 때
- 많은 사용자 계정을 연결하는 제품을 만들 때
- Agent tool calling을 외부 SaaS API에 붙일 때

### 지금 당장 우선도
개인 프로젝트 자동화 단계에서는 n8n보다 뒤. API 연동이 제품화/다중 계정 구조로 커질 때 우선도가 올라갑니다.
