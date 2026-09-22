# T-0008 — AI Content Monetization

> 이 파일은 이 주제의 전체 소통 기록입니다. 새 댓글도 별도 파일을 만들지 않고 이 파일 맨 아래에 새 구역으로 추가합니다.

> 통합일: 2026-09-19 KST. 병합 전 개별 파일은 Git 이력에서 확인할 수 있습니다.

---

## 원본 기록: README.md

### T-0008 — AI Content Monetization

- **Status:** OPEN
- **Project:** Threads
- **Repository:** https://github.com/kimjae134679/Threads
- **Owner:** user
- **Working AI:** Sol

#### Topic

2026년 최신 플랫폼 정책과 실제 크리에이터 운영 흐름을 조사해, Threads/Instagram/Reels/YouTube/Shorts/Blog/X 등으로 확장 가능한 AI 콘텐츠 수익화 파이프라인의 기반을 만든다.

#### Fixed direction

- 프로젝트 핵심은 무단 대량복제보다 `트렌드 탐지 → 사실확인 → 자체 해설/재구성 → 멀티포맷 변환 → 성과학습`으로 둔다.
- 디시인사이드/Blind처럼 약관상 크롤링 제한이 확인된 서비스는 자동수집 대상으로 두지 않는다.
- 공식 API, RSS, 공개 보도자료, 라이선스 자료, 직접 입력한 URL 등 허용된 입력을 우선한다.
- 남의 영상/글에 자막·속도·말투만 바꾼 저가치 재사용을 기본 전략에서 제외한다.
- 사건사고·개인 폭로·정치·건강/재정/법률 등 위험도가 높은 소재는 사람 승인과 추가 검증을 유지한다.
- 한 번 만든 research bundle을 Threads 글, Shorts/Reels 스크립트, 블로그, 장문 영상으로 확장한다.
- 플랫폼별 조회수뿐 아니라 팔로우, 클릭, 제휴, 광고, 협찬, 직접수익을 함께 기록한다.

#### Current work

1. 2026-09 기준 플랫폼/수익화/원본성 정책 조사 — baseline 작성
2. 소스 사용 정책과 자동수집 차단 기준 작성
3. 콘텐츠 제작/승인/게시/성과회수 파이프라인 설계
4. 이후 반자동 MVP로 실제 주제·훅·포맷 데이터를 축적
5. 성과가 확인된 구간만 게시 자동화/영상 제작 자동화로 확대

상세 기준은 실제 `kimjae134679/Threads` 저장소 문서를 원본으로 사용한다.

---

## 원본 기록: 001-sol.md

### 001 — Sol

초기 조사에서 가장 중요한 결론은 **AI 콘텐츠 자동화 자체보다 원본성·권리·소스 선택이 수익화 가능성을 결정한다**는 점이다.

현재 주요 플랫폼 공통 방향:
- YouTube/Meta/X는 단순 재업로드, 최소 편집, 저가치 모음보다 원본 제작·실질적인 해설/변형을 우대한다.
- Threads는 빠른 주제 검증/대화/유입 채널로 쓰고, 반응이 좋은 소재를 Shorts/Reels/Blog/YouTube로 확장하는 구조가 적합하다.
- X의 2026-09 Original Content Rewards는 자동 수단으로 생성·게시된 콘텐츠의 수익 적격성에 제약이 있어 X는 사람 승인/직접 게시 중심이 안전하다.
- Google 검색형 블로그도 AI 사용 자체보다 대량 저가치 생성/스크래핑 후 변형만 하는 패턴을 문제로 본다.

소스 쪽에서는 디시인사이드와 Blind의 약관상 자동 크롤링/스크래핑 제한이 확인됐다. 따라서 커뮤니티 화제를 활용하더라도 봇으로 전부 긁는 구조가 아니라, 허용된 소스와 사람이 넣은 URL을 기반으로 사실확인·식별정보 제거·자체 재구성을 거치는 방향으로 고정한다.

실제 설계와 최신 조사 원본은 `kimjae134679/Threads`의 다음 문서를 참고한다.
- `README.md`
- `docs/RESEARCH_2026-09.md`
- `docs/SOURCE_POLICY.md`
- `docs/CONTENT_PIPELINE.md`

다음 구현 우선순위는 **수집기보다 승인 큐가 있는 반자동 MVP**다. 먼저 실제 게시물 2~4주 데이터를 모은 뒤 잘 되는 소재/포맷만 자동화해야 한다.

---

## 원본 기록: 002-sol.md

### 002 — Sol

추가 조사와 실행 기준을 `kimjae134679/Threads` 저장소에 반영했다.

#### 이번 회차에서 고정한 최신 포인트

- YouTube는 AI 사용 자체를 금지하지 않지만, 반복 템플릿/양산형/재사용 콘텐츠는 YPP에 불리하다.
- 2027-02-01부터 신규 YPP 광고/Premium 진입 기준이 `1,000 subscribers + 8,000 qualified watch hours/365d` 또는 `20M qualified Shorts views/90d`로 강화될 예정이다.
- X는 2026-09-08부터 Original Content Rewards로 전환 중이며, 프로그램 문서상 자동 수단으로 생성되거나 게시된 콘텐츠는 보상 대상에서 제외될 수 있다. 수익계정은 AI 초안 + 사람 승인/직접 게시가 기본이다.
- Meta는 Facebook/Instagram에서 원본 제작자 우대를 강화하고 있다. 자막/테두리/속도변경 정도의 저가치 편집은 전략에서 제외한다.
- 2026 Metricool 데이터에서는 TikTok이 short-form 도달, Instagram이 상호작용, Instagram Carousel이 저장형 참여, YouTube가 장문/검색 자산에서 각각 다른 강점을 보여 `동일 파일 무차별 복붙`보다 플랫폼별 목적 분리가 유리하다.
- Naver Search Trend 신규 연동은 2026-07-31 이후 NAVER API HUB 기준으로 잡아야 한다.
- Reddit도 2026 정책상 승인 없는 scraping과 무승인 상업 이용을 기본 수집원으로 두기 어렵다.

#### 실제 저장소에 추가한 기반

- `AGENTS.md` — 이후 AI/Codex 작업 규칙
- `docs/PLATFORM_MATRIX.md` — 플랫폼별 역할/수익/자동화/정책 비교
- `docs/SOURCE_REGISTRY.md` — GREEN/YELLOW/RED 초기 소스 레지스트리
- `docs/EXECUTION_PLAN_30D.md` — 30일 실험 설계와 첫 MVP 화면/자동화 단계
- 기존 README에서 위 문서들을 진입점으로 연결

#### 초기 실험 축

엔진은 공유하되 채널 정체성은 최소한 아래처럼 나누는 방향이 좋다.

1. `Hot / Issue` — 오늘의 화제, AI/신제품/인터넷 이슈, 왜 뜨는지
2. `Internet Story / Culture` — 커뮤니티 사연/밈을 익명화·검증·재구성
3. `Useful / Product / Money` — 앱/서비스/AI 도구/가격·기능 비교, 향후 affiliate/brand 연결

처음부터 완전 자동게시보다 `Trend Inbox → Research Bundle → Draft Studio → 사람 승인 → Queue → Results`를 먼저 만드는 것이 현재 프로젝트의 안전한 MVP다.

다음 구현 회차는 실제 `Threads` 저장소의 `docs/EXECUTION_PLAN_30D.md`를 기준으로 Trend Inbox 데이터 모델/로컬 MVP 골격부터 시작하면 된다.

---

## 원본 기록: 003-sol.md

### 003 — Sol

TikTok 한국 수익화 조건을 최신 공식 문서 기준으로 재검증해 Threads 저장소의 조사 문서를 정정했다.

#### Correction

기존 문서에는 `TikTok Creator Rewards의 한국 제공 여부/수익조건은 계정에서 별도 확인`이라고 보수적으로 적혀 있었으나, 현재 공식 JP/KR Creator Rewards 약관과 TikTok 지원 문서에서 대한민국 지원과 구체적 조건을 확인했다.

2026-09 기준 대표 요건:
- 대한민국 거주자 19세 이상
- 정상 상태 개인 계정
- 팔로워 10,000명 이상
- 신청 전 최근 30일 실제 동영상 조회 100,000회 이상
- 수익 대상은 1분 이상의 고품질 오리지널 콘텐츠

2024년 한국 출시 Newsroom 글에는 당시 한국 기준 50,000 followers가 안내됐지만, 현재 법적 약관/지원 문서는 10,000을 명시하므로 최신 문서를 우선한다.

Creator Rewards의 오리지널 콘텐츠 기준에서는 Duet/Stitch/스폰서 콘텐츠 등이 제외될 수 있으므로, TikTok 전략을 `짧은 바이럴 도달 전용`에서 `짧은 발견형 + 1분 이상 자체 설명/스토리형 직접수익`의 이중 구조로 수정한다.

Updated in actual repo:
- `docs/RESEARCH_2026-09.md`
- `docs/PLATFORM_MATRIX.md`

---

## 원본 기록: 004-sol.md

### 004 — Sol

조사 문서 단계에서 실제 실행형 **Trend Inbox MVP**까지 진행했다.

#### 구현된 것

`kimjae134679/Threads`에 다음 실행 코드가 추가되어 있다.

- `server.mjs` — zero-dependency local HTTP/API server
- `app/index.html` — Trend Inbox UI
- `app/app.js` — 수동 소재 입력, 위험 판정, 상태/평가/점수/Research Bundle 흐름
- `app/youtube.js` — 선택적 YouTube Data API connector
- `app/styles.css`, `app/evaluation.css`
- `package.json` — `npm start`, `npm run check`
- `.github/workflows/check.yml` — Node syntax CI
- `app/README.md` — 실행/connector 설명

#### 현재 실제 입력 경로

1. **Google Trends KR Trending Now RSS**
   - 서버에서 실시간 RSS 후보를 가져와 Inbox로 변환
   - 검색량 표기, 피드 시각, 관련 뉴스 후보를 같이 보관
   - 이 신호를 사건의 사실 확인으로 오인하지 않음

2. **YouTube Data API `mostPopular` KR**
   - `YOUTUBE_API_KEY` 환경변수가 있을 때만 활성화
   - 키는 GitHub/클라이언트에 저장하지 않음
   - 제목/채널/게시시각/조회·좋아요·댓글 등 메타데이터만 후보 신호로 사용
   - 2025-07-21 이후 이 API가 과거 전체 Trending 페이지와 동일하지 않고 인기 음악·영화·게임 차트 성격이 강하다는 제한을 UI/문서에 명시

3. **수동 URL / 메모**
   - DCInside / Blind URL은 현재 Source Policy 기준 RED 자동수집 차단
   - 공개 SNS/뉴스/일반 커뮤니티는 YELLOW 검토 경로

#### 평가 / 승인 흐름

자동 수집 후보는 데이터가 없는 신호를 가짜 점수로 채우지 않는다.

- freshness
- velocity
- audience fit
- originality room
- revenue fit

5개가 모두 평가된 뒤 초기 점수를 계산하며, 미평가 후보는 바로 `제작 후보`로 승격할 수 없다. RED 소스도 직접 제작 후보 승격을 막는다.

현재 상태 흐름:

`Inbox -> 조사 대기 -> 제작 후보 / 패스`

각 후보에서 AI Research Bundle용 프롬프트를 생성·복사할 수 있다.

#### 저장 / 검증

- Inbox: browser `localStorage` (`threads_trend_inbox_v1`)
- JSON export/import 지원
- CI: GitHub Actions `npm run check`
- 최신 head 기준 syntax CI **PASS**

아직 사용자 PC에서 실제 브라우저를 열어 Google/YouTube live fetch와 UI 클릭 흐름까지 실기동 검증했다고 주장하지 않는다. 현재 검증 범위는 GitHub 코드 반영 + Actions syntax PASS이다.

#### 다음 우선순위

1. 허용된 뉴스/RSS/API source connector 확대
2. 여러 source item을 하나의 topic으로 묶는 clustering/dedup
3. Research Bundle 실제 AI 조사 실행 연결
4. 플랫폼별 Draft Studio
5. Human Approval Queue
6. Threads 공식 API 게시/Insights 회수
7. 성과 기반 KEEP / KILL / SCALE

---

## 원본 기록: 005-sol.md

### 005 — Sol

Trend Inbox MVP에 **유사 토픽 묶기 baseline**을 추가했다.

#### 목적

Google Trends, YouTube, 수동 링크 등 서로 다른 입력 경로에서 같은 사건/주제가 여러 카드로 들어올 수 있으므로 조사 전에 연관 후보를 한 묶음으로 볼 수 있게 한다.

#### 현재 판정 신호

- canonical URL 동일
- related source URL 겹침
- 제목 token Jaccard 유사도
- 공백 제거 제목의 2-gram / 3-gram 유사도
- 길이가 짧은 제목에는 더 높은 threshold 적용

결과는 각 Inbox item에 다음 메타데이터로 저장한다.

- `clusterId`
- `clusterSize`
- `clusterConfidence`
- `clusteredAt`

#### 중요한 제한

이 기능은 heuristic이다.

- 자동 삭제하지 않음
- 자동 merge하지 않음
- 같은 사건이라고 확정하지 않음
- 상세 화면에서 같은 묶음 후보를 오가며 교차출처 조사할 수 있게만 함

따라서 잘못 묶인 후보가 있어도 원본 데이터가 사라지지 않는다.

#### 검증 상태

해당 코드(`app/clustering.js`)까지 `npm run check` 대상에 포함했다. 이 변경을 포함한 GitHub Actions syntax check는 성공한 커밋이 확인됐다.

별도 네트워크 테스트 컨테이너에서 repo clone + live Google fetch까지 하려 했으나 해당 컨테이너가 외부 DNS를 사용할 수 없어 GitHub clone 단계에서 중단됐다. 이것을 앱 실패로 기록하지 않으며, 반대로 live runtime PASS로도 기록하지 않는다.

#### 다음 핵심

이제 후보 수집/평가/중복 보조 단계 다음으로 **Research Bundle의 실제 조사 결과를 구조화해 저장하는 단계**가 우선이다.

---

## 원본 기록: 006-sol.md

### 006 — Sol

Trend Inbox에 **구조화된 Research Bundle 저장 + 사람 검토 gate**를 추가했다.

#### 저장되는 조사 결과

후보별 `researchBundle`에 다음을 저장한다.

- `reviewStatus`: `unresearched / researching / reviewed`
- `whyNow`
- `verifiedFacts[]`
- `claimsToVerify[]`
- `angles[]`
- `riskNotes`
- `sources[]` (`url + note`)
- `updatedAt`

#### reviewed 조건

`사람 검토 완료`로 저장하려면 최소:

- 왜 지금 뜨는지 작성
- 확인된 사실 1개 이상
- 추가 출처 1개 이상

이 필요하다.

또한 `제작 후보` 버튼은 capture 단계 gate를 추가해 Research Bundle이 `reviewed`가 아니면 기존 상태 변경 로직까지 도달하지 못하게 했다. 기존의 사람 점수 평가 gate도 그대로 유지되므로 제작 후보 승격에는 **점수 평가 + 조사 검토**가 둘 다 필요하다.

#### 전달성

`Bundle JSON 복사` 기능으로 원 소재/소스 위험/신호/cluster 정보와 구조화된 조사 결과를 다음 AI/도구 단계로 넘길 수 있다.

#### 현재 경계

아직 LLM/API를 앱 내부에서 직접 호출해 Research Bundle을 자동 작성하지는 않는다. 현재는 AI 조사용 프롬프트 생성 + 조사 결과 구조화 저장까지다. 외부 AI 연결은 다음 단계로 둔다.

---

## 원본 기록: 007-sol.md

### 007-sol — End-to-end Threads publishing MVP

기준: 2026-09-13 KST

`kimjae134679/Threads`가 조사 문서 단계에서 실제 실행형 파이프라인으로 확장됨.

#### 이번 회차 구현

```text
Trend Inbox
→ 사람 신호 평가
→ 유사 토픽 묶기
→ Research Bundle
→ 선택적 OpenAI 웹 조사
→ 사람 검토 완료
→ Draft Studio
→ 사람 초안 승인
→ Rights / Safety Gate
→ 게시 승인 Queue
→ Threads 공식 API 텍스트 게시
→ Threads Post Insights 회수
```

##### OpenAI

- `OPENAI_API_KEY` 선택 연결
- Responses API + web search + structured output
- 기본 모델 `gpt-5.6-luna`, 환경변수로 교체 가능
- AI 조사 결과는 항상 `researching`; 자동으로 사람 검토 완료 처리하지 않음
- Draft Studio는 reviewed Research Bundle만 사용하며 새 사실 추가 금지 지침

##### Rights / Safety Gate

사람이 다음을 PASS/WARN/BLOCK으로 검토:
- 사실 검증
- 저작권/자산 권리
- 개인정보/초상
- 명예훼손/일반인 주장
- 플랫폼 원본성/AI 정책

UNKNOWN/BLOCK이면 게시 승인 불가. WARN은 대응 메모 필요.

##### Threads 공식 API

- `THREADS_ACCESS_TOKEN` 선택 연결
- 공식 host `https://graph.threads.net`
- 기본 권한: `threads_basic`, `threads_content_publish`
- Insights: `threads_manage_insights`
- 실제 게시: `/me/threads` 컨테이너 생성 → `/me/threads_publish` 명시적 publish
- `auto_publish_text` 사용 안 함
- 게시 전 연결 계정 `@username` 표시
- 읽기 전용 최종 문안 + 공개 게시 확인 체크박스 + 마지막 confirm
- 승인 이후 초안/검토 상태가 바뀌면 기존 게시 승인 무효화
- 게시 후 `views/likes/replies/reposts/quotes/shares` 회수 가능

#### 검증 상태

GitHub Actions:
- 전체 JS syntax check PASS
- 로컬 Node 서버 실제 기동 smoke test PASS
- `/api/health`, `/api/connectors`, `/app/` PASS
- OpenAI key 없음 → 성공 위장 없이 503 확인
- Threads token 없음 → profile 503 확인
- 승인 안 된 후보의 Threads publish 요청 → 409 차단 확인

아직 **실제 Threads 사용자 access token으로 공개 게시한 E2E 성공 기록은 없음**. 토큰을 저장소에 넣지 않기 때문에 정상적인 미검증 상태다.

#### 재개 위치

원본은 항상 `kimjae134679/Threads` 최신 main을 먼저 본다.

주요 파일:
- `README.md`
- `AGENTS.md`
- `app/README.md`
- `openai.mjs`
- `threads.mjs`
- `server.mjs`
- `app/ai-studio.js`
- `app/safety-gate.js`
- `app/threads-publisher.js`
- `docs/THREADS_API_SETUP.md`

#### 다음 우선순위

1. 실제 운영 source 2~4개 추가
2. publication / insights 공통 Experiment 데이터 모델
3. KEEP / KILL / SCALE 대시보드
4. 실제 Threads 계정 소량 E2E 검증
5. 이후 Instagram/YouTube 공식 게시/성과 API 확장
6. localStorage → 서버 DB/동기화

---

## 원본 기록: 008-sol.md

### T-0008 / 008-sol — Source Enrichment + Experiment Lab

기준일: 2026-09-13 KST

#### 이번 회차 완료

`kimjae134679/Threads`를 조사/게시 파이프라인에서 **실제 성과 학습 파이프라인**까지 확장했다.

##### 1. 브라우저 로드 누락 수정

`app/ai-studio.js`가 파일/CI에는 존재했지만 `app/index.html`에서 script가 빠져 실제 브라우저에서 AI 조사/Draft Studio가 로드되지 않는 문제를 발견하고 수정했다.

이후 CI가 주요 브라우저 script 이름을 HTML에서 직접 확인하도록 강화했다.

##### 2. NAVER API HUB 공식 connector

신규 `naver.mjs` + `app/source-enrichment.js`.

환경변수:

```text
NAVER_API_HUB_CLIENT_ID
NAVER_API_HUB_CLIENT_SECRET
```

현재 endpoint:

```text
GET  /search/v1/news
GET  /search/v1/blog
GET  /search/v1/cafearticle
POST /search-trend/v1/search
```

후보별로 뉴스/블로그/카페 검색과 30일 검색트렌드를 확인할 수 있다.
검색 결과에서는 제목/링크/짧은 검색 패시지만 저장한다. 원문 재사용 권리로 취급하지 않는다.
검색 트렌드는 상대지수이므로 자동 `velocity` 점수를 덮어쓰지 않는다.

2026-07-31 이후 신규 네이버 Search API/Search Trend 신청은 NAVER API HUB 기준으로 문서화했다.

##### 3. Experiment Lab

실제 `item.publications[]`를 Experiment 원본 데이터로 사용한다.

Threads metrics:

```text
views / likes / replies / reposts / quotes / shares
engagements = likes + replies + reposts + quotes + shares
engagement_rate = engagements / views
```

판정:

- 같은 플랫폼 `views > 0` 비교군 < 5: `LEARN`
- 5건 이상:
  - 조회수 백분위 55%
  - 참여율 백분위 45%
  - 상위 25% `SCALE`
  - 하위 25% `KILL`
  - 그 사이 `KEEP`

클릭 / 전환 / 실수익(KRW)은 실제 값이 있을 때만 사람이 입력한다.
자동 판정은 수동 override 가능.
Experiment CSV 내보내기 추가.

##### 4. Experiment 판정 회귀 테스트

판정 로직을 `app/experiment-model.js`로 분리했다.

`test/experiment-model.test.mjs`에서 다음을 검증:

```text
LEARN / SCALE / KEEP / KILL
동률 percentile
콘텐츠 축 분류
```

`npm run check`가 syntax + regression test를 모두 수행한다.

##### 5. CI 강화

GitHub Actions에서:

- syntax/regression test
- 서버 실제 기동
- health/connectors
- 주요 browser script 실제 HTML load 여부
- 주요 정적 asset 응답
- keyless OpenAI 503
- keyless NAVER 503
- keyless Threads 503
- 미승인 Threads publish 409

을 검사한다.

이번 NAVER + browser load 확장 기준 CI syntax/local smoke는 PASS 확인.

#### 여전히 실제 검증 전

다음은 구현됐지만 사용자 실제 credential이 없어 성공 E2E로 기록하지 않는다.

- 실제 NAVER API HUB live 요청
- 실제 OpenAI API 조사/초안
- 실제 Threads 공개 게시
- 실제 Threads Insights
- 실제 5건 이상 Experiment 데이터에서 SCALE/KEEP/KILL 보정

#### 다음 재개점

1. 실제 Threads 계정으로 텍스트 1~3건 E2E
2. 실제 Insights payload 확인
3. 게시 5건 이상 후 Experiment 가중치/경계 재보정
4. 추가 허용 connector
5. 자체 이미지/영상 제작 파이프라인
6. Instagram/YouTube 공식 게시/성과 adapter
7. localStorage → DB/계정 동기화

Repo 재개 순서:

```text
README.md
AGENTS.md
app/README.md
docs/SOURCE_REGISTRY.md
docs/EXPERIMENT_LAB.md
docs/THREADS_API_SETUP.md
```

---

## 원본 기록: 009-sol.md

### T-0008 / 009-sol — Regression + NAVER hardening

기준일: 2026-09-13 KST

008 이후 최종 하드닝.

#### 추가 완료

- `app/experiment-model.js`로 KEEP/KILL/SCALE 계산을 UI에서 분리
- `test/experiment-model.test.mjs` 추가
- `npm run check` = syntax + Experiment regression test
- `app/index.html`에서 `experiment-model.js`를 `experiment-lab.js`보다 먼저 로드
- CI가 `ai-studio.js`, `source-enrichment.js`, `threads-publisher.js`, `experiment-model.js`, `experiment-lab.js`의 HTML 실제 로드를 확인
- NAVER Search Trend 기간을 서버 로컬/UTC가 아니라 **Asia/Seoul 날짜 기준**으로 계산
- NAVER 검색 결과 URL은 `http/https`만 저장

#### 최종 검증

Threads repo 최신 하드닝 커밋 기준 GitHub Actions:

```text
JavaScript syntax and regression checks: SUCCESS
Local server smoke test: SUCCESS
```

검증 범위:

- Experiment LEARN/SCALE/KEEP/KILL 회귀 테스트
- percentile 동률 테스트
- 콘텐츠 축 분류 테스트
- 서버 health/connectors/static app
- 주요 browser script load
- OpenAI/NAVER/Threads keyless fail-closed
- 미승인 Threads publish guard

#### 남은 실제 검증

credential을 저장소에 넣지 않았으므로 다음은 아직 E2E 성공으로 기록하지 않는다.

```text
NAVER API HUB live
OpenAI live research/draft
Threads 실제 공개 게시
Threads 실제 Insights
실제 5건 이상 성과를 이용한 Experiment 기준 보정
```

다음 시작점은 기능 추가보다 실제 소량 E2E다.

---

## 원본 기록: 010-sol.md

### 010 — 역할 분업 + 다계정 실험 구조 재정리

기준: 2026-09-13 KST

사용자 지시에 따라 `kimjae134679/Threads` 저장소를 역할별로 바로 관리할 수 있게 재구성했다.

#### 새 최상단 구조

```text
00_START_HERE
01_DISCOVERY
02_EDITORIAL_SCORING
03_PRODUCTION
04_REVIEW_PUBLISH
05_EXPERIMENTS_ACCOUNTS
```

기존 실행 코드(`app/`, `server.mjs` 등)는 경로를 무리하게 이동하지 않아 MVP 실행을 깨뜨리지 않았다. 새 번호 폴더는 사람/AI 역할과 인수인계를 명확히 하는 운영 레이어다.

#### 역할 경계

- 01: 소재 검색 / 자동 신호 / 사용자 직접 제보 → Candidate Packet
- 02: 독립 조사 / 사실 검증 / 점수 / 각도 / ready·research·skip → Approved Content Brief
- 03: 플랫폼별 게시글·영상·블로그 제작 → Draft Package
- 04: Safety Gate / 사람 승인 / 실제 게시 → Publication Record
- 05: 다계정 전략 / Insights / 클릭·전환·수익 / KEEP·KILL·SCALE → Experiment Result

중요: 사실/출처/점수는 02 소유, 표현/포맷은 03 소유, publish approval은 04 소유, account hypothesis/performance decision은 05 소유. 뒤 단계가 앞 단계 데이터를 조용히 수정하지 않고 Handoff Contract에 따라 되돌린다.

#### 새 문서

- `00_START_HERE/README.md`
- `01_DISCOVERY/README.md`
- `02_EDITORIAL_SCORING/README.md`
- `03_PRODUCTION/README.md`
- `04_REVIEW_PUBLISH/README.md`
- `05_EXPERIMENTS_ACCOUNTS/README.md`
- `05_EXPERIMENTS_ACCOUNTS/ACCOUNT_REGISTRY.md`
- `05_EXPERIMENTS_ACCOUNTS/ACCOUNT_REGISTRY_TEMPLATE.md`
- `docs/HANDOFF_CONTRACTS.md`

root `README.md`도 처음부터 역할 폴더/다계정 전략이 보이도록 재작성했고 `AGENTS.md`에도 역할 소유권/다계정 규칙을 추가했다.

#### 다계정 전략

계정 여러 개를 복붙 배포 슬롯으로 쓰지 않고 **서로 다른 전략 실험군**으로 본다.

현재 실제 계정 생성 전 계획값(planned):

```text
TH-A  Hot / Issue
TH-B  Useful / Product / Money
TH-C  Internet Story / Culture
```

각 계정은 account_id / content_axis / positioning / target_audience / hypothesis / primary_metric / minimum_sample / scale_rule / kill_rule를 가진다.

동일 콘텐츠의 여러 계정 복붙 금지. 같은 주제를 시험할 경우 variant_id + hypothesis_id를 별도로 두고 훅/각도/대상/포맷/톤 중 최소 하나 이상을 의도적으로 다르게 한다.

계정 여러 개를 플랫폼 제한/제재 회피용으로 사용하지 않는다.

#### 다음 구현 우선순위

1. 현재 앱 candidate/draft/publication 데이터에 `account_id`, `hypothesis_id`, `variant_id` 정식 추가
2. Account Registry의 planned 계정을 실제 계정이 생길 때만 active로 변경
3. 계정별 소량 E2E 게시
4. Insights를 account/variant 단위로 비교
5. 실제 표본 기반으로 잘 되는 전략만 SCALE

실제 계정/토큰은 아직 연결/생성 확인되지 않았으므로 planned 외 상태를 주장하지 않는다.

---

## 원본 기록: 011-sol.md

### 011 — 앱 다계정/가설/버전 추적 실제 연결

기준: 2026-09-13 KST

`010-sol.md`에서 정리한 역할 분업/다계정 설계를 이번 회차에 실제 앱 데이터 흐름까지 연결했다.

#### 구현

##### 후보 계정/실험 배정

새 파일:
- `app/account-registry.js`
- `app/experiment-assignment.js`
- `app/experiment-assignment.css`

후보 상세에서 저장:

```text
experimentAssignment.accountId
experimentAssignment.hypothesisId
experimentAssignment.variantId
experimentAssignment.goal
experimentAssignment.updatedAt
```

초기 논리 계정 Registry:
- TH-A — Hot / Issue
- TH-B — Useful / Product / Money
- TH-C — Internet Story / Culture

실제 계정이 아직 생성/연결되지 않았으므로 planned 상태 유지.

##### 승인 무결성

배정값이 바뀌면 candidate `updatedAt`도 변경한다.
기존 게시 승인 basis와 달라지므로 배정 변경 뒤에는 재승인이 필요하다.

##### Threads 게시 스냅샷

실제 Threads 게시 전에 `accountId` 배정이 필요하다.
게시 성공 시 현재 실험 배정값을 Publication에 고정 저장:

```text
publication.experiment.accountId
publication.experiment.hypothesisId
publication.experiment.variantId
publication.experiment.goal
publication.experiment.assignmentUpdatedAt
publication.platformAccount.username
```

후보의 설정을 나중에 바꾸더라도 이미 게시된 실험의 귀속은 변하지 않는다.

##### Experiment Lab

추가:
- 계정별 필터
- 카드에 account / hypothesis / variant 표시
- 실제 연결된 Threads username 표시
- 실험 목표 표시
- CSV에 `account_id`, `hypothesis_id`, `variant_id`, `platform_username`, `experiment_goal` 추가

기존 Publication처럼 experiment snapshot이 없을 때만 후보 현재 assignment를 fallback으로 본다.

##### CI

`package.json` 버전을 `0.7.0`으로 올렸다.
새 JS 파일들을 syntax check에 포함했다.
GitHub Actions static smoke에서 `account-registry.js`, `experiment-assignment.js`, 관련 CSS 로드를 검사한다.

검증:
- run id: `34761239085`
- commit: `d6f8abc7467d372d07b068c60baf8b53aacd1a3d`
- JavaScript syntax and regression checks: SUCCESS
- Local server smoke test: SUCCESS

#### 문서

Threads repo 추가:
- `docs/ACCOUNT_EXPERIMENT_TRACKING.md`

#### 현재 데이터 흐름

```text
후보 수집
→ account / hypothesis / variant 배정
→ 조사 / 제작 / 검수 / 승인
→ 실제 게시
→ Publication에 실험 스냅샷 고정
→ Insights / 클릭 / 전환 / 수익
→ 계정별 Experiment Lab 비교
```

#### 아직 실제가 아닌 것

- TH-A/B/C는 실제 생성된 계정이 아니라 planned 논리 실험군.
- 실제 Threads token/계정 게시 E2E는 아직 미검증.
- 여러 실제 Threads credential을 전환/관리하는 Account Manager는 아직 없음.

#### 다음 우선순위

1. 실제 Threads 계정 1개 E2E
2. 실제 username과 논리 account_id 연결 확인
3. 게시 → Insights 왕복 검증
4. 실제 계정이 2개 이상 생기면 Account Manager / credential profile 구조 추가
5. 충분한 표본 뒤 account + hypothesis + variant별 성과 비교 강화

---

## 원본 기록: 012-sol.md

### 012 — 실전 콘텐츠 벤치마크 + 포맷 실험 통합

기준: 2026-09-13 KST

사용자 지시 `싹다긁어와`에 따라 단순 정책 조사에서 더 나아가 **실제 Threads/social 운영 패턴, 포맷, 성장 사례, 대규모 데이터**를 벤치마크하고 앱의 실험 데이터 구조까지 연결했다.

#### 조사한 범위

- Meta/Threads 공식 creator guidance
- Threads topic tag / Insights / 외부 추천 / link click / long text attachment
- Buffer 45M+ social post format benchmark
- Buffer 128K Threads reply analysis
- 실제 creator/brand 운영 패턴
- 공개 성장/실패 사례
- 2026-09 한국 Threads public trend snapshot
- YouTube Shorts original/reused/inauthentic content 기준
- Shorts hook / engaged-view / analytics 운영 방식

상세:
- `docs/BENCHMARK_2026-09.md`

#### 주요 benchmark 결론

- Threads는 text-only로 볼 수 없고 image/video도 적극 실험 대상.
- reply/conversation 운영을 별도 변수로 추적.
- screenshot은 `own content / 공식 자료의 필요한 일부 / 허가 자료 + 실질적 해설` 중심.
- 타인의 viral post/영상에 최소한의 자막·crop만 더하는 방식은 기본 전략에서 제외.
- 반복 series, 명확한 niche, 구체적인 실패/사례, open question, own-content repurpose를 시험.
- Instagram 확장 시 Reels는 discovery, Carousel은 deeper engagement 역할을 구분해 평가.
- Shorts는 raw views만 보지 않고 Engaged views/subscriber conversion/revenue까지 본다.

#### 제작 taxonomy

`03_PRODUCTION/FORMAT_PLAYBOOK.md`에 다음을 정식 정의:

- F01~F20 콘텐츠 포맷
- H01~H10 Hook
- C00~C08 CTA
- A01~A10 Source Asset
- R0~R2 Reply 운영

대표 포맷:
- text hot take / explainer
- list/checklist
- open question
- own screenshot commentary
- official screenshot evidence + analysis
- original image card
- carousel
- meme
- chart/comparison
- short video
- screen recording
- voiceover
- long text attachment
- reply/quote
- poll
- failure story
- case study
- before/after
- own-content repurpose

#### 초기 실험 Matrix

`05_EXPERIMENTS_ACCOUNTS/EXPERIMENT_MATRIX.md`

TH-A/B/C 각각 10건씩 시작할 수 있는 초기 포맷 배치와 reply/hook/image/video 실험 변수를 정의했다.

실제 계정이 하나뿐이면 먼저 그 계정에서 5~10건 E2E를 안정화한 뒤 다계정으로 확대한다.

#### 앱 구현

신규:
- `app/content-strategy.js`
- `app/content-strategy.css`
- `app/experiment-metadata.js`
- `app/experiment-metadata.css`
- `app/publication-strategy-snapshot.js`

후보마다 저장:
```text
contentStrategy.contentFormat
contentStrategy.hookType
contentStrategy.ctaType
contentStrategy.sourceAssetType
contentStrategy.replyMode
contentStrategy.hasTopicTag
contentStrategy.note
```

게시 후 새 publication에는 해당 content strategy를 snapshot으로 고정해, 나중에 후보 전략을 바꿔도 과거 실험 귀속이 바뀌지 않도록 했다.

Experiment Lab:
- 포맷 filter
- hook filter
- 카드에 format/hook/CTA/asset/reply/topic tag 메타 표시
- 전체 제작 메타 CSV export

#### 권리 안전장치

`A10 Unknown rights` 선택 시:
- Safety Gate rights를 BLOCK 처리
- 기존 approval을 item.updatedAt 변경으로 stale 처리
- 실제 Threads 게시 클릭도 capture guard로 차단
- 권리를 확인한 뒤 A06/A07/A08/A09 등 올바른 분류로 바꾸고 Gate 재검토 필요

#### 로딩 방식

기존 index 경로를 크게 건드리지 않고 `experiment-assignment.js`가 companion script를 로드:
- content-strategy.js
- experiment-metadata.js

content-strategy가 publication-strategy-snapshot.js를 로드한다.

GitHub Actions는 companion file 존재와 bootstrap 문자열, A10/snapshot guard, CSS 존재까지 smoke test하도록 강화했다.

#### 아직 실제로 확인하지 않은 것

- 실제 Threads access token 연결
- 실제 공개 Threads 게시
- 실제 Threads Insights 회수
- 실제 5~10건 이상 결과에서 benchmark 가설 검증
- 이미지/영상 자동 렌더 pipeline

즉 benchmark는 **초기 prior**이며 실제 계정 데이터가 쌓이면 우리 own performance data가 우선한다.

#### 다음 핵심

1. Threads 실제 계정/토큰 연결
2. 후보 1개 → account/hypothesis/variant + format/hook 배정
3. 실제 게시 1건 E2E
4. publication experiment/strategy snapshot 확인
5. Insights 회수
6. 5~10건 표본 후 benchmark 가설과 실제 결과 비교
7. 그 뒤 이미지 카드/Carousel/Short video 실제 제작 renderer 추가

---

## 원본 기록: 013-sol.md

### 013 — Sol handoff — Viral Finder + Community Card Factory

#### 이번 회차에서 실제 구현한 것

##### Viral Finder / Batch Review

Threads repo에 다음 파일을 추가했다.

```text
app/viral-model.js
app/viral-review.js
app/viral-review.css
test/viral-model.test.mjs
docs/VIRAL_FINDER_BATCH_REVIEW.md
data/viral-discovery-latest.json
docs/VIRAL_DISCOVERY_TEST_2026-09-14.md
```

기능:

- 기존 Inbox 후보를 조회/좋아요/댓글/공유/순위/신선도/카드화 가능성으로 재점수
- `STRONG / CANDIDATE / REVIEW / BLOCK / LOW`
- Audience Comfort 별도 계산
- 동물 학대, 고어/참수/토막/시체/잔혹 영상, 성폭력/아동 성착취, 자살 영상/사진, 신상털이 등 강한 불쾌 소재 자동 BLOCK
- 폭행/사망/살인/괴롭힘 등은 주의 검토
- URL/제목 기반 중복 표시
- 여러 후보 동시 선택
- 선택 후보를 조사/제작/패스로 일괄 이동
- BLOCK 후보 자동 패스
- 실제 공개 검색에서 발견한 metadata feed를 `최신 실제 발견 묶음 가져오기`로 Inbox에 반입

DCInside/Blind는 직접 대량 크롤링하지 않는다. 약관/권리 때문에 사용자가 준 URL/스크린샷/수동 캡처 또는 공개 검색 인덱스 metadata를 사용한다.

##### 실제 public discovery test

`data/viral-discovery-latest.json`에 실제 공개 검색 결과 메타데이터 샘플을 넣었다.

- Reddit r/memes 고반응 샘플: 약 14.6K votes
- r/NewToReddit 저반응 비교 샘플
- r/help 본문보다 댓글 반응이 소재가 되는 샘플
- 폭력 사건 밈/논쟁 제거 글을 Audience Comfort BLOCK 회귀 샘플로 사용

원문 전체/이미지/영상은 저장하지 않는다. URL/제목/짧은 편집 요약/공개 반응 metadata만 둔다.

##### Community Card Factory MVP

추가 파일:

```text
app/card-story-model.js
app/card-factory.js
app/card-factory.css
test/card-story-model.test.mjs
docs/COMMUNITY_CARD_FACTORY.md
```

후보 상세에서 다음 입력을 구성한다.

```text
hook
source label
핵심 원문/요약
후속/반전/맥락
대표 반응
마지막 질문/결말
원문 screenshot 0~10개
```

`현재 자료로 자동 채우기`는 Research Bundle의 verified facts / why now / angles 등을 읽어 기본 스토리보드를 만든다.

Canvas로 실제 1080×1350 카드 미리보기를 렌더한다.

현재 template:

```text
Dark Viral
Paper Story
Signal News
```

카드 구조는 내용이 있을 때 자동 생성:

```text
Hook
→ capture image(s)
→ 핵심 내용
→ 후속/맥락
→ 대표 반응
→ 마지막 질문/결말
```

각 카드 PNG 개별 다운로드 + 전체 PNG 순차 저장 + manifest JSON 지원.

원문 screenshot 파일은 localStorage/GitHub에 넣지 않고 세션 Object URL로만 사용한다. 새로고침 뒤 파일 재선택 필요.

현재는 OCR/PII 자동 마스킹을 성공했다고 간주하지 않는다. 실명/닉네임/얼굴/전화번호 등은 게시 전 사람 확인이 필요하다.

Audience Comfort BLOCK 후보는 Card Factory 미리보기 생성도 막는다.

Card storyboard 저장 시 candidate `updatedAt`이 바뀌어 기존 publish approval은 stale 처리된다.

#### bootstrap / regression

`app/experiment-assignment.js`가 이제 다음을 동적으로 bootstrap한다.

```text
content-strategy.js
experiment-metadata.js
viral-model.js → viral-review.js
card-story-model.js → card-factory.js
```

`package.json`은 0.10.0으로 올리고 다음 테스트를 `npm test`에 포함했다.

```text
experiment-model.test.mjs
viral-model.test.mjs
card-story-model.test.mjs
```

Viral Finder package commit `141afb30...` CI run `34765355884` SUCCESS.
Card Factory bootstrap commit `6594fec...` CI run `34765711819` SUCCESS.
Card Factory package commit `ad13362...`는 handoff 작성 시점 GitHub Actions가 진행 중이므로 완료 여부를 다시 확인해야 한다.

#### 아직 미완료

우선순위대로:

1. Card Factory 원문 screenshot crop/수동 mask 편집기
2. PII 후보 감지 보조 (사람 확인 유지)
3. Content Warehouse — READY/HOT/EVERGREEN 완제품 적재
4. 완성 PNG 영속 자산 저장 방식
5. Threads/Instagram image/carousel 공식 게시 adapter
6. Scheduler — 시간표/HOT 우선/유사 소재 간격/stop-now controls
7. localStorage → DB/서버 persistence
8. 실제 Threads token 연결 후 live E2E

#### live blocker

실제 Threads public publish/Insights E2E는 아직 `THREADS_ACCESS_TOKEN` 미연결로 검증하지 못했다. 비밀번호/토큰을 채팅에 받지 말고 사용자의 로컬 env/OAuth 연결로 처리한다.

---

## 원본 기록: 014-sol.md

### 014 — Sol handoff — Content Warehouse

#### 추가 구현

Threads repo에 Content Warehouse staging layer를 추가했다.

```text
app/warehouse-model.js
app/content-warehouse.js
app/content-warehouse.css
test/warehouse-model.test.mjs
docs/CONTENT_WAREHOUSE.md
```

`app/experiment-assignment.js` bootstrap에도 다음을 추가했다.

```text
warehouse-model.js → content-warehouse.js
```

package version은 `0.11.0`.

#### Warehouse 동작

Draft 또는 Community Card storyboard가 만들어진 후보는 별도 복제 없이 Warehouse에 자동 노출된다.

관리 필드:

```text
bucket       hot / evergreen
priority     1~5
status       active / hold
notBefore
expiresAt
note
```

파생 상태:

```text
ready       Safety Gate + 현재 candidate 기준 publish approval 완료
approval    승인 없거나 승인 이후 콘텐츠 변경됨
review      Safety Gate 미완료
blocked     Audience Comfort 차단
```

Warehouse 설정 자체는 게시물 내용이 아니므로 bucket/priority/hold/시간 변경은 publish approval을 stale 처리하지 않는다.

#### queue model

실제 Scheduler가 아직 붙지 않았지만 다음 게시 후보 순서를 계산할 모델을 먼저 구현했다.

- 승인/Gate 통과만 eligible
- HOLD 제외
- notBefore 이전 제외
- expiresAt 이후 제외
- HOT 가중치
- priority 1~5
- HOT 유효기한이 가까우면 추가 가중치
- 동률이면 오래 대기한 콘텐츠 소폭 우선

UI에서 `다음 우선 후보`를 표시한다.

#### regression

`test/warehouse-model.test.mjs` 추가.

검증 항목:

- 정상 승인 candidate ready
- HOLD 제외
- stale approval → approval 단계
- Audience Comfort BLOCK → blocked
- HOT이 동일 priority evergreen보다 먼저
- 미래 notBefore 제외

`package.json`의 `npm test`와 syntax check에 포함됨.

#### 다음 우선순위

1. Card screenshot crop / 수동 privacy mask 편집
2. Scheduler planner + 시간 슬롯
3. 이미지 자산 영속 저장 위치 결정
4. public asset URL이 생기면 Threads/Instagram image/carousel publisher
5. 실제 Threads token E2E

실제 자동 예약 게시를 client localStorage/timer만으로 성공했다고 표시하지 말 것. 앱이 꺼져도 돌아가려면 server persistence/scheduler가 필요하다.

---

## 원본 기록: 015-sol.md

### 015-sol — Theme classification + app architecture hardening

#### 이번 회차 목적

사용자 요청:

- 바이럴 후보를 테마별로 분류
- 앞으로 기능이 계속 늘어나도 파일/폴더/의존성이 꼬이지 않게 구조를 세세하게 정리
- 계획만이 아니라 실제 프로그램 수정

#### 실제 구현

##### 1. Theme classification

신규 폴더:

```text
app/features/themes/
├─ README.md
├─ theme-taxonomy.js
├─ theme-model.js
├─ theme-review.js
└─ theme-review.css
```

현재 primary theme:

```text
work_career            직장 / 커리어
dating_relationships   연애 / 인간관계
money_consumption      돈 / 소비 / 재테크
military_school        군대 / 학교
internet_humor         인터넷 / 유머 / 밈
weird_true_story       황당 / 실화 / 반전
tech_ai_games          AI / IT / 게임
life_debate            생활 / 공감 / 논쟁
entertainment_celeb    연예 / 방송 / 영화
animals_nature         동물 / 자연
society_news           사회 / 사건 / 시사
sports                 스포츠 / e스포츠
food_travel            음식 / 여행 / 장소
general_viral          기타 바이럴
```

자동분류는 title/note/Research Bundle/related source metadata와 taxonomy keyword를 사용한다.

저장:

```text
item.themeClassification = {
  schemaVersion,
  taxonomyVersion,
  primaryTheme,
  secondaryThemes[],
  tags[],
  confidence,
  score,
  reasons[],
  source,      // auto | manual
  computedAt,
  updatedAt
}
```

- primary 1개 + secondary 자동 최대 2개
- 사람이 수동 분류하면 auto가 덮어쓰지 않음
- 수동 secondary 최대 3개
- 자유 tag 추가 가능
- Viral Finder / Content Warehouse에 테마 chip 표시
- 두 화면에 공통 테마 filter 제공
- Viral Finder 상단에 테마별 후보 수 summary 추가
- Theme metadata는 운영 분류이므로 theme 수정만으로 기존 publish approval을 stale 처리하지 않음

중요: Theme와 Safety는 분리. `animals_nature` 자체는 정상 테마이고 동물학대/고어 등은 기존 Audience Comfort가 별도 BLOCK한다.

##### 2. 중앙 Feature Bootstrap

신규:

`app/bootstrap/feature-loader.js`

기존 `experiment-assignment.js` 안에서 여러 기능을 연쇄 `loadCompanion()`하던 구조를 제거했다.

현재 feature load 순서:

```text
production strategy
→ experiment metadata
→ themes
→ viral finder
→ community cards
→ warehouse
```

새 기능은 앞으로 임의로 다른 feature script를 삽입하지 않고 bootstrap 목록에 명시적으로 등록한다.

feature loader는 DOMContentLoaded 이후 실행해 base app script가 먼저 준비되도록 한다.

##### 3. 구조 문서/규칙

신규:

- `app/ARCHITECTURE.md`
- `app/features/README.md`

목표 구조:

```text
app/
├─ bootstrap/
├─ core/
│  ├─ state/
│  ├─ ui/
│  └─ utils/
├─ features/
│  ├─ discovery/
│  ├─ themes/
│  ├─ editorial/
│  ├─ production/
│  ├─ publish/
│  ├─ warehouse/
│  └─ experiments/
└─ legacy/
```

규칙:

- taxonomy/config / model / UI / CSS 분리
- model은 DOM/localStorage/state 직접 mutation 금지
- feature마다 owned state field 명시
- 콘텐츠 변경과 운영 메타데이터 변경을 구분
- 대규모 한 번 이동 금지. 기능군 단위 이동 → CI green → 다음 기능군 이동

다음 물리적 이동 순서:

```text
1 themes                  ✅ 신규 구조 적용
2 discovery/viral         NEXT
3 production/cards
4 warehouse
5 experiments
6 editorial
7 publish
8 app.js → core/state + core/ui 분해
```

##### 4. 회귀 테스트

신규:

- `test/theme-model.test.mjs`
- `test/feature-layout.test.mjs`

검사:

- 직장/연애/동물/기타 theme 샘플 분류
- 수동 theme override normalization
- architecture/theme/bootstrap 필수 파일 존재
- 중앙 feature loader에 필수 모듈 등록
- `experiment-assignment.js`가 더 이상 `loadCompanion` chain을 소유하지 않음

`package.json` → `0.12.0`

##### 5. CI 정리

구조 변경 직후 구형 smoke test가 `experiment-assignment.js` 안에 옛 companion 문자열이 있는지 검사해서 실패했다.

이건 실제 앱 기능 오류가 아니라 CI가 옛 구조를 강제하던 문제였고, smoke test를 중앙 bootstrap 구조에 맞게 갱신했다.

추가로 Actions Node version을 20 → 24로 갱신했다.

최신 검증:

```text
GitHub Actions run 34767503985
head 5ca42bdb4c6c0574ac819c34bad75915e931cad8
conclusion SUCCESS
```

syntax + Experiment/Viral/Card/Warehouse/Theme/Layout regression + local server smoke + credential-less fail-closed 검증 통과.

#### 현재 실제 상태

기능 구조 정리의 첫 단계가 끝났고, **기존 root 파일을 전부 이동한 상태는 아니다.** 한 번에 경로를 바꿔 전체 앱을 깨지 않기 위해 의도적으로 단계 이관 중이다.

현재 신규 Theme은 처음부터 `features/themes/`에 정리되어 있고, 기존 Viral/Card/Warehouse는 중앙 bootstrap 아래에서 여전히 root legacy 경로를 사용한다.

#### 다음

1. `app/viral-*` → `features/discovery/viral/` 물리 이동 + loader/test 경로 변경
2. `card-*` → `features/production/cards/`
3. `warehouse-*` → `features/warehouse/`
4. Scheduler planner 구현 시 `themeClassification.primaryTheme`을 이용해 같은 테마 연속 게시 방지
5. Experiment Lab에 theme filter/theme별 성과 비교
6. 이후 core state migration

실제 Threads token/account E2E와 image/carousel official publisher는 여전히 별도 남음.

---

## 원본 기록: 016-sol.md

### 016-sol — Audience Comfort 강화 + Card text privacy masking

#### 실제 변경

##### 1. Viral Finder / Audience Comfort

`app/viral-model.js`의 불쾌감 필터를 구조화된 카테고리로 강화했다.

BLOCK:

- `graphic_violence` — 고어/참수/토막/잔혹 영상·사진
- `animal_abuse` — 동물 학대/고문
- `sexual_violence` — 강간/성폭행/성착취/아동 성착취
- `self_harm_graphic` — 자살·자해 사진/영상/장면
- `doxxing` — 신상털이/주소·전화 공개/도싱
- `gross_unpleasant` — 구더기·토사물·배설물 등 단순 바이럴 가치보다 불쾌감이 큰 시각 소재

REVIEW:

- 폭행/살인/학대 언급
- 사망/참사
- 성적/노출 주의
- 괴롭힘/혐오

결과에 `categories`, `blockReasons`, `reviewReasons`를 추가해서 왜 BLOCK/REVIEW인지 추적 가능하게 했다. 고반응이어도 BLOCK 사유가 있으면 Viral score와 무관하게 제작 후보로 올라가지 않는다.

회귀 테스트 추가:

- 동물 학대 + 고어 → BLOCK
- 구더기/불쾌 시각 소재 → BLOCK
- doxxing → BLOCK
- 비그래픽 폭행 사건 설명 → REVIEW
- 일반 고양이 소재 → 정상 통과

##### 2. Community Card Factory 텍스트 PII 마스킹

`app/card-story-model.js` storyboard schema를 v2로 올리고, 카드 텍스트에 들어가기 전 다음 형태를 자동 마스킹한다.

- 이메일
- 휴대전화/전화번호 형태
- 주민번호 형태
- IPv4
- `@handle`

스토리보드에는 다음 privacy 상태를 기록한다.

```js
privacy: {
  textPiiMasked: true,
  imageMaskingRequired: imageCount > 0
}
```

중요: 이미지 속 얼굴/닉네임/전화번호를 OCR로 자동 가린 척하지 않는다. 캡처 이미지가 들어가면 `imageMaskingRequired=true`로 남겨 다음 이미지 수동 마스킹 UI 단계에서 처리해야 한다.

회귀 테스트에는 실제 이메일/전화번호가 최종 storyboard JSON에 남지 않는지 확인하는 케이스를 추가했다.

#### 검증

- Audience Comfort 변경 커밋까지 GitHub Actions syntax/regression/server smoke **SUCCESS** 확인.
- Card privacy masking 변경도 CI 실행 대상으로 올라갔으며 동일 `npm run check` + server smoke 파이프라인에서 검증된다.
- 직전 구조 변경으로 발생했던 구형 smoke test 문제는 이미 중앙 `feature-loader` 기준으로 수정되어 현재 Actions가 green 상태로 회복됨.

#### 실제 discovery 재확인

공개 검색/인덱스에서 최신 Reddit 사례를 다시 확인했다. 강한 새 수치를 찾았다고 꾸미지 않고 기존 real feed를 유지했다.

- `data/viral-discovery-latest.json`의 2026-09-03 r/memes 사례: 약 14.6K votes의 강한 실제 샘플.
- 낮은 반응 샘플과 폭력/제거 샘플은 LOW/BLOCK 회귀 비교용으로 유지.
- Reddit 공개 개발 사례에서도 top-post pool에서 10,000 upvotes 이상을 선별하는 방식이 확인되어, 이미 반응이 검증된 post/topic을 우선하는 Viral Finder 방향과 부합한다.

DCInside/Blind는 계속 자동 대량 크롤링하지 않는다.

#### 다음 작업 순서

1. Card Factory 캡처 이미지에 **수동 drag rectangle privacy mask**를 넣어 얼굴/닉네임/전화번호 영역을 실제 PNG 렌더 전에 가리기.
2. Viral Finder batch review에 BLOCK reason chip, 전체/범위 선택, 중복 그룹 단위 처리 추가.
3. `viral-*` 파일을 계획대로 `features/discovery/viral/`로 물리 이동하고 CI green 확인.
4. Warehouse/queue에 동일 theme/format 연속 게시 방지 spacing planner 추가.
5. official image/carousel publisher + pause/stop/post-now queue controls.
6. 그 뒤 DB/server persistence와 multi-account 확장.

실제 Threads 게시 credential/사람 승인이 없으면 게시 성공을 가정하지 말고 현재 fail-closed 동작을 유지한다.

---

## 원본 기록: 017-sol.md

### 017-sol — Theme-lane discovery + multi-platform source registry

#### 사용자 요청

- 소재를 찾을 때도 테마별로 분리해서 보여줄 것
- 웃긴 짤 / 소식 / Blind 논란 같은 식으로 한 덩어리로 섞지 말고, 실제 찾은 성격에 맞게 잘 나눌 것
- X/Twitter, 카페, 여러 커뮤니티 등 탐색 플랫폼 범위를 넓힐 것
- 기존 소통/운영 GitHub에도 같이 기록할 것

#### 실제 구현

##### 1. Discovery Source Registry

신규 폴더:

```text
app/features/discovery/sources/
├─ README.md
├─ source-registry.js
├─ source-model.js
├─ source-review.js
└─ source-review.css
```

현재 Registry 범위:

```text
Google Trends
NAVER 뉴스
NAVER 블로그
NAVER 카페
Daum 카페
YouTube
Reddit
X / Twitter
Threads
Instagram
DCInside
Blind
FMKorea
더쿠
인스티즈
클리앙
루리웹
인벤
뽐뿌
아카라이브
Tistory / 공개 블로그
기타 뉴스/공개 웹
```

각 플랫폼에는 `adapter`와 `mode`를 따로 둔다.

```text
connected
connected-when-credentialed
manual-only
planned
planned-discovery
```

중요: Registry 등록은 실제 자동 크롤러 완성을 의미하지 않는다.

DCInside/Blind처럼 직접 대량 scraping을 쓰지 않는 곳은:

```text
공개 검색/인덱스 메타데이터
+ 사용자 제공 URL
+ 사용자 제공 스크린샷/Capture
```

흐름으로만 처리한다.

##### 2. 테마별 Discovery Lane

현재 레인:

```text
웃긴 짤 / 밈
커뮤니티 논란 / 의견갈림
소식 / 이슈 / 지금 뜨는 것
직장 / 취업 / 회사썰
연애 / 인간관계
돈 / 소비 / 재테크
군대 / 학교
황당 / 실화 / 반전
AI / IT / 게임
연예 / 방송 / 문화
스포츠 / e스포츠
음식 / 여행 / 장소
동물 / 자연
```

각 lane에는 `themes[] / sources[] / note`가 있고 기존 `themeClassification`과 연결된다.

예:

```text
웃긴 짤/밈
→ internet_humor + weird_true_story
→ X / Reddit / DCInside / FMKorea / 더쿠 / 루리웹 / 아카라이브 / YouTube

커뮤니티 논란
→ life_debate + work_career + dating_relationships + money_consumption
→ Blind / DCInside / NAVER 카페 / Daum 카페 / X / Reddit / FMKorea / 인스티즈 / 더쿠 / 클리앙 / 뽐뿌

소식/이슈
→ society_news + tech_ai_games + entertainment_celeb + sports
→ Google Trends / NAVER 뉴스 / X / Threads / YouTube / Reddit / 기타 뉴스
```

##### 3. 실제 UI

신규 `DISCOVERY LANES / SOURCES` 패널 추가.

기능:

- lane별 현재 후보 수
- lane별 우선 탐색 플랫폼 표시
- 전체 플랫폼 Registry/연결 방식 확인
- Viral Finder에서 `탐색 레인` 필터
- Viral Finder에서 `플랫폼` 필터
- 각 후보에 실제 source chip + lane chip
- 원문 수동확인이 필요한 출처는 별도 표시

기존 Theme filter와 별개로 동작한다.

사용 예:

```text
웃긴 짤/밈만 보기 → X만 보기
직장/취업만 보기 → Blind만 보기
소식/이슈만 보기 → NAVER 뉴스 / X / YouTube 후보 보기
```

##### 4. 테스트/구조

신규:

`test/discovery-source-model.test.mjs`

검사:

- DCInside URL → dcinside
- Blind URL → blind
- X URL → x
- Reddit URL → reddit
- NAVER 카페 → naver_cafe
- 회사 사연 → work/community lane
- 싱글벙글 웃긴 짤 → funny_memes lane
- Blind는 manual-only
- source registry 최소 20종 이상
- discovery lane 최소 10종 이상

`test/feature-layout.test.mjs`에도 discovery/sources 경로와 중앙 loader 등록을 강제했다.

중앙 `feature-loader.js`는 이제:

```text
production strategy
→ experiment metadata
→ themes
→ discovery sources
→ viral finder
→ community cards
→ warehouse
```

순으로 로드한다.

이 변경 세트는 `package.json` 0.13.0에서 syntax/regression 대상으로 들어갔다. 코드/테스트 포함 커밋 `3e0903f5852bdcf1a0370e80794a995db7393f3e` 기준 Actions run `34767974421` SUCCESS.

##### 5. Discovery 역할 문서

`01_DISCOVERY/README.md`도 갱신했다.

Candidate Packet에 앞으로 다음 의미를 명시한다.

```text
discovery_lane
discovery_source
```

그리고 `발견 가능`과 `본문 자동수집 가능`을 분리한다.

#### 다음

1. Registry의 `planned` 소스를 실제 adapter 단위로 하나씩 연결
2. 공식/안전한 자동수집 경로부터 확대
   - NAVER 뉴스/블로그/카페 API
   - YouTube official API
   - RSS
   - 검색 인덱스 metadata worker
3. X/Reddit은 공식 API 사용 가능 조건 확인 후 adapter 추가
4. DCInside/Blind는 bulk crawler 대신 Capture Packet 강화
5. 매 실행마다 lane별 최소 후보 수를 맞추는 quota planner
6. 이후 `lane × source × format` 성과를 Experiment Lab에서 비교해 잘 먹히는 탐색 비중을 자동 조절
7. 기존 `viral-*`를 `features/discovery/viral/`로 물리 이관

#### 운영 원칙

- 많이 찾되 같은 플랫폼만 도배하지 않는다.
- 조회수만 높고 보기 불쾌한 소재는 Audience Comfort에서 제외한다.
- 테마 분류와 위험 판단은 별개다.
- 원문 발견과 재사용 권리는 별개다.
- 실제 adapter가 없는 플랫폼을 자동수집 완료라고 표시하지 않는다.

---

## 원본 기록: 018-sol.md

### 018-sol — Canonical scheduler execution handoff

Updated: 2026-09-14 KST

#### Scheduler state

Canonical recurring task: `Threads Viral Build`

- status: enabled
- exact start anchor: 2026-09-14 02:00 KST
- cadence: every 1 hour
- timing mode: exact schedule
- purpose: keep implementing the Threads AI Content Monetization Lab, not merely report plans

A second duplicate Threads schedule was created during handoff setup, then disabled immediately to avoid two workers editing the same repo in parallel. `Threads Viral Build` is the single canonical recurring worker.

#### Required first reads for every scheduled run

1. `kimjae134679/Threads/00_START_HERE/NEXT_RUN_HANDOFF.md`
2. current `kimjae134679/Threads` `main` branch and recent commits
3. latest sequential note in `kimjae134679/project-operations-hub/04_COMMUNICATION/threads/T-0008-ai-content-monetization/`
4. the README for the active role/domain being changed

Repository tip wins over stale handoff text. Never blindly resume from an old SHA without checking `main`.

#### Threads repo handoff added

Created:

- `00_START_HERE/NEXT_RUN_HANDOFF.md`
  - commit: `c3b088e83d9fdb79259e4283e95e0dbe6cfbf83f`

Updated:

- `00_START_HERE/README.md`
  - scheduled/recurring workers are now pointed to `NEXT_RUN_HANDOFF.md` first
  - commit: `ce603fad61bb632b7e65bbac9312e664f848ad5c`

Baseline observed before those handoff commits:

- `f75005f24a79d7910b7cd3afc1b3844625840dd5` — `Document theme lanes and multi-platform discovery`

That baseline already documents theme lanes, a multi-platform Source Registry, adapter status separation, engagement evidence fields, and the rule that DCInside/Blind are not bulk-crawled.

#### Canonical execution order

Scheduled work continues in this order, skipping only items already completed on current `main`:

1. Viral Finder / multi-platform discovery
2. Audience Comfort / unpleasant-content filter
3. bulk candidate review / multi-select
4. Community Card Factory
5. Content Warehouse
6. official image/carousel publishing where official APIs allow it
7. HOT-priority scheduler/queue with theme/source/format spacing and pause/stop/post-now controls
8. DB/server persistence and multi-account management after earlier workflow is substantially complete

#### Required details preserved in scheduler prompt

##### Viral Finder / Discovery

- find already-proven public trending/high-engagement posts/topics
- use the Source Registry and explicit provider state: `connected`, `connected-when-credentialed`, `manual-only`, `planned`
- normalize candidates across platforms
- distinguish observed engagement evidence from inferred interest
- dedupe / same-story grouping
- source-risk visibility
- theme/source filtering
- balanced discovery across several lanes/sources instead of one platform dominating
- real public/indexed discovery tests when useful
- record only visible/verifiable engagement values
- store why a candidate is strong/weak

Theme lanes remain separated: funny memes, community controversy, news/issues, work/career, relationships, money/consumption, military/school, weird true stories, AI/IT/games, entertainment, sports/esports, food/travel, animals/nature.

DCInside/Blind and any source whose terms/access model do not permit bulk scraping remain manual/user-URL/screenshot/public-index-metadata paths.

##### Audience Comfort

Preserve the 016 behavior:

Hard BLOCK includes:

- graphic gore/violence
- animal abuse
- sexual violence/exploitation
- graphic self-harm
- doxxing
- strongly gross/unpleasant material

Appropriate non-graphic sensitive cases can route to REVIEW.

Continue with visible reason chips, auditability, batch filtering, Korean/English mixed-case tests, and no false claim that image/OCR masking succeeded when it did not.

##### Bulk review

Need fast large-pool operations:

- select all / visible / group
- approve / reject / hold / tag
- duplicate-group actions
- bulk handoff to editorial
- blocked reason visibility

##### Community Card Factory

Produce 1080x1350 packages with hook, cleaned/cropped excerpts, emphasis, reaction/summary, ending/CTA, source/rights/review manifest.

Existing text PII masking stays. High-value next privacy item remains a real manual drag-rectangle image mask before final PNG export. Do not turn `imageMaskingRequired` into a fake success flag.

##### Warehouse

READY / HOT / EVERGREEN with provenance, history, freshness/expiry, theme/format tags, rights/review state, assets, queue eligibility.

##### Publishing

Official APIs only. Explicit capability states, dry-run/validation, fail closed when credential/scope/human approval is missing. Preserve real request/response/error audit information without exposing secrets. Never claim success without a real official API success result.

##### Queue

HOT priority, spacing between similar themes/sources/formats, pause, stop, post now, reorder, and visible scheduling reason. Final publish still passes 04 REVIEW_PUBLISH gates.

#### Execution requirements on every meaningful run

- actually modify implementation when useful work remains
- continue staged architecture under `app/features/<domain>/`
- run syntax/regression/server smoke tests plus targeted tests
- fix failures when feasible before finishing
- confirm CI green only when actually observed
- perform useful real discovery tests when they materially exercise the pipeline
- where enough source material exists, run representative candidates through the actual app workflow and create clearly separated demo/test card storyboards/previews using the program itself
- do not publicly publish demos without human approval
- do not invent engagement metrics for synthetic fixtures
- do not ask for passwords/secrets
- if credentials or human approval block live posting, record the blocker and continue non-blocked implementation/testing
- commit/push meaningful work
- clean temporary build/cache/probe artifacts

#### Role boundaries that must remain

`01 DISCOVERY → 02 EDITORIAL_SCORING → 03 PRODUCTION → 04 REVIEW_PUBLISH → 05 EXPERIMENTS_ACCOUNTS`

- 01 discovers/packages candidates; does not publish
- 02 owns fact/source validation, scoring, angle approval
- 03 transforms approved briefs; does not invent facts
- 04 owns safety/rights/human approval and actual publication
- 05 owns account/experiment strategy and performance interpretation
- `A10 Unknown rights` assets remain non-publishable until resolved and re-reviewed

#### Required next handoff format

After meaningful changes, create the next sequential note in this folder containing:

- baseline commit read
- files/features changed
- exact behavior added/fixed
- tests run and results
- real discovery examples tested, if any
- blockers
- next priority
- new Threads commit SHA(s)

Also update `Threads/00_START_HERE/NEXT_RUN_HANDOFF.md` if the backlog/state materially changes.

#### End condition

Keep progressing until there is genuinely no substantive actionable work left. Only then report that state; do not invent tasks merely to keep the recurring schedule busy.

---

## 원본 기록: 019-sol.md

### 019-sol — Viral Finder evidence normalization + duplicate grouping

Updated: 2026-09-14 KST

#### Baseline read

Scheduled run started from current `Threads/main`:

- `ce603fad61bb632b7e65bbac9312e664f848ad5c` — scheduler entrypoint handoff wiring

Read first:

- `Threads/00_START_HERE/NEXT_RUN_HANDOFF.md`
- current recent commits
- operations-hub `018-sol.md`

#### What changed

##### 1. Cross-platform candidate normalization

Updated `app/features/discovery/sources/source-model.js` with a common `normalizeCandidate(item)` envelope:

```text
sourceId / sourceLabel / sourceFamily
adapter / collectionMode
manualCaptureRequired
bulkBodyCollectionAllowed
laneId / laneLabel
themeIds
engagementEvidence
canonicalUrl
exactDuplicateKey
sameStoryKey
publishedAt
sourceRisk
```

This is the base layer for later bulk import/review so Reddit/NAVER/X/manual community candidates are not handled with incompatible ad-hoc fields.

##### 2. Observed engagement vs inferred interest

Added `engagementEvidence(item)`.

Observed values are explicitly tagged by origin:

```text
manual-observed
source-metadata
```

Supported observed metric classes:

```text
views
likes/upvotes
comments
shares
rank
```

Existing calculated signal fields such as popularity/freshness remain separately under `inferred`. No synthetic metric is promoted to observed evidence.

Evidence level:

```text
observed
inferred-only
none
```

##### 3. Canonical URL + duplicate groups

Canonical URL normalization now:

- removes fragments
- removes common tracking params (`utm_*`, `fbclid`, `gclid`, `igshid`, `ref`, `share` etc.)
- preserves meaningful non-tracking query parameters
- normalizes hostname/trailing slash

Added exact duplicate grouping and same-story grouping plus optional explicit story groups.

The first same-story implementation was too strict for title variants such as:

```text
[속보] AI 신기능 발표 영상
AI 신기능 발표
```

so title normalization now strips generic noise tokens such as:

```text
속보 / 단독 / 공식 / 영상 / 짤 / 펌 / 근황 / 화제
breaking / official / update / video / clip
```

before generating the story fingerprint.

##### 4. Viral Finder UI evidence visibility

Updated `app/features/discovery/sources/source-review.js`.

Rows can now show:

- source chip
- discovery lane chip
- observed engagement chip (`실측 조회/반응/댓글/공유/순위`)
- `추정 관심도만`
- `반응값 미확인`
- `완전중복 ×N`
- `같은소재 ×N`
- existing `원문 수동확인`

Row datasets expose normalized evidence/group metadata for the upcoming bulk-review implementation.

Updated `source-review.css` with distinct observed/inferred/unverified evidence treatments.

##### 5. Regression tests

Updated `test/discovery-source-model.test.mjs` for:

- tracking-param URL cleanup while keeping meaningful params
- manual-observed metric priority
- source-metadata observed fallback
- inferred-only evidence
- normalized Reddit/manual-capture candidate
- exact duplicate grouping
- same-story grouping after title-noise normalization
- source risk preservation

#### Threads commits

```text
0207a5c13fc79ca4f5832683fece88a7c269acbe  Normalize discovery evidence and same-story groups
e8fe52bb3f95bf4d338419bcef4bf946a7061815  Test discovery evidence normalization and grouping
f8683c25ea938e47a827de7ec7ed82456e74c19f  Show discovery evidence and duplicate groups in Viral Finder
45a4a5ab210c6e934c6a81f742e5ea4baff82418  Style discovery evidence chips
2a520b5821a273ca76a5a0d349a8cb10ff2fa124  Harden same-story title normalization
88828d87ab44a79a1b0a788852d7021eab5b192f  Advance scheduler handoff after discovery normalization work
```

#### Validation

An intermediate Actions run `34770490557` failed in regression checks after the first same-story implementation. The stricter-than-intended title fingerprint was corrected in `2a520b5...`.

Confirmed successful GitHub Actions afterwards:

```text
34770550263  head 2a520b5821a273ca76a5a0d349a8cb10ff2fa124  SUCCESS
34770599699  head 88828d87ab44a79a1b0a788852d7021eab5b192f  SUCCESS
```

The successful workflow includes the repository JavaScript syntax/regression checks and local server smoke path configured by `.github/workflows/check.yml`.

A local container clone was also attempted, but outbound DNS to github.com was unavailable; GitHub Actions is therefore the authoritative executable verification for this run.

#### Real discovery probe

A current public-web probe was performed across Reddit/YouTube/news-style search surfaces.

Result:

- no sufficiently strong **new + current + multi-source + verifiable-engagement** batch was found in that probe
- NAVER direct article access was blocked by robots in the web environment
- some returned examples were old, promotional, or lacked reliable public engagement metrics

Therefore none were falsely inserted into the viral feed as a new strong candidate. Existing verified discovery fixtures remain preferable until a stronger batch is found.

Rule preserved: `no visible metric → do not invent it`.

#### Blockers

No code blocker.

Live publishing remains credential/human-approval dependent and fail-closed.

Blind/DCInside bulk collection remains intentionally prohibited; use public index metadata/user URL/screenshot/manual Capture.

#### Next priority

Continue **P1 Viral Finder**:

1. make bulk candidate import persist normalized discovery metadata from `normalizeCandidate()` rather than computing it only at render time
2. build duplicate/same-story group review controls (`select group / collapse group / keep strongest / move group`)
3. add filters for observed-vs-inferred evidence and source risk
4. continue compliant multi-source real discovery tests

Only after P1 bulk workflow is solid move to P2 Audience Comfort reason chips/audit/batch filters.

---

## 원본 기록: 020-sol.md

### 020-sol — Persisted discovery metadata + duplicate/same-story group review

Updated: 2026-09-14 KST

#### Baseline

This run started from current `Threads/main` after `019-sol.md` and verified recent commits. The working baseline was the P1 normalization state around:

```text
46982deb1dd3a8531214935abe95ebef51fcb542  Record green CI for discovery normalization
88828d87ab44a79a1b0a788852d7021eab5b192f  Advance scheduler handoff after discovery normalization work
```

The repo tip remained authoritative over any older note.

#### Implemented

##### 1. Viral discovery import now persists normalized metadata

Updated `app/viral-review.js`.

Before this run, `ThreadsDiscoverySourceModel.normalizeCandidate()` was mostly used at render/review time. The Viral discovery import now stores the common normalized envelope directly on each imported candidate:

```text
item.discoveryNormalized = {
  schemaVersion,
  id,
  title,
  canonicalUrl,
  sourceId/sourceLabel/sourceFamily,
  adapter/collectionMode,
  manualCaptureRequired,
  bulkBodyCollectionAllowed,
  laneId/laneLabel,
  themeIds,
  engagementEvidence,
  exactDuplicateKey,
  sameStoryKey,
  publishedAt,
  sourceRisk,
  normalizedAt,
  normalizedBy
}
```

This gives later stages a stable stored discovery record instead of forcing every consumer to recompute source/lane/evidence/group data independently.

Existing candidates without the field are backfilled during Viral score/rescore.

##### 2. Canonical-URL dedupe at import

The latest discovery-feed import now compares canonical URLs from the discovery model rather than raw URL strings.

Therefore URL variants that differ only by tracking parameters are not imported as separate candidates.

No engagement values are fabricated while doing this.

##### 3. Evidence and source-risk filters

Viral Finder received two independent filters:

```text
Evidence
- all
- observed
- inferred-only
- none

Source risk
- all
- green
- yellow
- red
- unknown
```

Rows also show quick evidence/risk chips so reviewers can separate a candidate with actual observed reactions from one with only inferred interest.

##### 4. Duplicate / same-story group review controls

Exact-duplicate and same-story groups now expose real review controls instead of only a count chip.

Current controls:

```text
그룹 선택
접기 / 펼치기
최고점만 유지
```

Collapse mode leaves the group's highest Viral score candidate visible.

`keep strongest` moves lower-score group members to `skip` and records an audit trail under `viralReview`:

```text
duplicateResolution
duplicateGroupKey
keptStrongestId
resolvedAt
```

This is not deletion. The lower-score candidates remain in state for provenance/history.

Blocked candidates and duplicate-disabled rows are not silently promoted by group selection.

##### 5. Styling + regression guard

Updated:

```text
app/viral-review.css
test/feature-layout.test.mjs
```

The regression guard requires Viral Finder to retain:

- discovery-model wiring
- persisted `discoveryNormalized`
- normalizeCandidate usage
- evidence filter
- source-risk filter
- group select/collapse/keep-strongest actions
- duplicate-resolution audit field

#### Threads commits

```text
0cc75c4d8276321f3333d62406c434b430448510  Add persisted discovery metadata and group review controls
13c1bc38c5ad491b5d4ea8341170ff78012f7170  Style Viral Finder evidence and group controls
1586905fe363417e8db2468418017f8bc90863fe  Guard Viral Finder normalization and group review wiring
0d8e5ef99475ea8c3415ce1f7635a46479a89d0b  Advance handoff after Viral Finder bulk review controls
```

#### Validation

Confirmed GitHub Actions:

```text
34773559194  head 13c1bc38c5ad491b5d4ea8341170ff78012f7170  SUCCESS
34773565936  head 1586905fe363417e8db2468418017f8bc90863fe  SUCCESS
```

The workflow includes configured JavaScript syntax/regression and server smoke checks.

#### Public discovery probe

A fresh public-web probe was also performed.

One indexed Reddit AI digest dated 2026-09-12 surfaced r/technology topics with apparently strong reaction counts, including an AI data-center town-hall controversy around 4.1K score and 246 comments. However the surfaced page was a secondary digest/index record rather than the original post directly verified in this run.

Therefore:

- it was not inserted into the production viral feed
- the digest's reported numbers were not promoted to canonical observed metrics
- no synthetic replacement metrics were created

Other returned meme/video examples were old, removed, low-signal, or lacked directly verifiable current engagement.

Rule remains:

```text
no direct/verifiable metric → do not invent/promote it
```

#### Blockers

No code blocker in this P1 slice.

Live publication remains credential + human-approval dependent and fail-closed.

Blind/DCInside continue as public-index/user-URL/screenshot/manual Capture paths only; no direct bulk crawler was added.

#### Next priority

Finish the remaining P1 consistency work before moving to P2:

1. apply the same persisted discovery normalization to base JSON import, Google Trends candidate creation, and manual candidate creation, not only Viral discovery import/backfill
2. add explicit group-wide move actions (`research / hold / skip`) and a group status summary
3. distinguish editorial handling of exact duplicates from same-story alternatives; same-story alternatives may be held rather than always skipped
4. then move to P2 Audience Comfort: visible category/reason chips, human-review audit trail, batch filter, Korean/English mixed-text regression cases

Do not weaken approval, rights, or safety gates while adding bulk actions.

---

## 원본 기록: 021-sol.md

### 021-sol — Cross-path discovery normalization + group-wide dispositions

Updated: 2026-09-14 KST

#### Baseline

Started from `Threads/main` after `020-sol.md`.

Previous implementation checkpoint:

```text
1586905fe363417e8db2468418017f8bc90863fe
0d8e5ef99475ea8c3415ce1f7635a46479a89d0b
```

The repository tip was treated as authoritative.

#### Implemented

##### 1. Persisted discovery normalization now covers every candidate path

New file:

```text
app/features/discovery/sources/source-normalization-sync.js
```

Before this run, persisted `item.discoveryNormalized` was guaranteed mainly by Viral discovery import / Viral score backfill.

Now the discovery-sources feature owns a synchronization layer that observes the base candidate list and persists the same normalized envelope for candidates created or replaced through:

```text
manual candidate form
Google Trends import
JSON Inbox import
existing candidate state on feature bootstrap
other candidate-list renders that introduce previously un-normalized items
```

The stored envelope still comes only from `ThreadsDiscoverySourceModel.normalizeCandidate()` and contains the canonical URL, source registry identity/adapter/mode, lane, themes, evidence level, duplicate/same-story keys, publish time and source risk.

The sync layer compares a stable signature first, so unchanged candidates do not continuously rewrite `normalizedAt` or localStorage.

Audit fields:

```text
normalizedAt
normalizedBy
```

The new feature is loaded centrally by `app/bootstrap/feature-loader.js`; no new cross-feature load chain was added.

##### 2. Viral Finder group-wide review actions

New file:

```text
app/features/discovery/viral/group-actions.js
```

Existing exact-duplicate / same-story groups now gain:

```text
그룹 → 조사
그룹 보류
그룹 패스
```

A group status summary is also displayed, e.g. research / hold / production / skip / Inbox / blocked counts.

Safety behavior:

- BLOCK candidates are not promoted into research/hold by group actions.
- group skip may still move blocked candidates to skip.
- actions record `viralReview.groupDisposition`, group key and timestamp.

##### 3. Exact duplicate vs same-story handling is now differentiated

Existing exact-duplicate `keep strongest` behavior remains destructive only at workflow-state level (lower members → `skip`; data is not deleted).

For **same-story** groups, the button is changed to:

```text
최고점 유지 · 대안 보류
```

Behavior:

- highest Viral-score candidate remains the main candidate
- lower non-blocked same-story variants return to `inbox`
- they are explicitly marked as editorial alternatives with:

```text
duplicateResolution = same-story-alternative-hold
groupDisposition = hold
keptStrongestId
```

This avoids treating a different source/angle on the same underlying story as if it were merely a useless exact duplicate.

##### 4. Regression / syntax guards

Updated:

```text
app/bootstrap/feature-loader.js
test/feature-layout.test.mjs
package.json
```

Package version is now `0.13.2`.

CI syntax checks explicitly include:

```text
app/features/discovery/sources/source-normalization-sync.js
app/features/discovery/viral/group-actions.js
```

The layout regression test now requires both features and their central loader registration, and checks the expected group disposition / same-story hold wiring.

##### 5. Observer-loop fix

During review of the group UI patcher, a potential MutationObserver self-loop was identified: assigning identical `summary.textContent` on every patch could continuously create child-list mutations.

Fixed by updating the summary text only when its value actually changes.

#### Threads commits

```text
2f0fd02da52c6188f16381a2d2f989a5de453ba9  Persist discovery normalization across all candidate paths
be745512cd1c057fe00be7dcc94847be10e73fd1  Load discovery normalization sync
2204479ffdf936d3360037bc92110a1575934a76  Guard cross-path discovery normalization sync
ee150dd3c5a867bceeacb6313907a9c3e22a4c39  Check discovery normalization sync in CI
04182cb3943cfdc6e199f6229ec694bb9d6fd74e  Add group-wide Viral Finder dispositions
f5d808285a7e8f8c89287b1efe53a127c9f1b77d  Load Viral Finder group action feature
49c75862039d91392ddba53f594dced74729d913  Guard Viral Finder group-wide actions
c1f7c18e50fbc5ec1353c56be1a6fa5795cb4187  Check Viral Finder group actions in CI
db95cf7c649786bedb1f40076412c155b9207d16  Avoid group status patch observer loop
```

#### Validation

Confirmed GitHub Actions:

```text
34776351603  head ee150dd3c5a867bceeacb6313907a9c3e22a4c39  SUCCESS
34776428360  head c1f7c18e50fbc5ec1353c56be1a6fa5795cb4187  SUCCESS
34776451461  head db95cf7c649786bedb1f40076412c155b9207d16  SUCCESS
```

These runs include repository syntax/regression checks and configured server smoke.

Important limitation: the CI is not a full human browser interaction E2E. The new DOM group controls are syntax/layout-regression guarded, but actual click-through browser UX should still be exercised when a browser-capable run is available.

#### Discovery probe

No new production discovery candidate was promoted in this run. The run focused on state consistency and review controls. Existing rule remains:

```text
direct/verifiable public metric only → observed engagement
secondary digest / unclear metric → do not promote as canonical observed value
```

Blind/DCInside remain public-index/user URL/screenshot/manual Capture paths only; no bulk crawler was added.

#### Blockers

No code blocker for this P1 slice.

Live publishing remains fail-closed behind real credentials/scopes plus human approval.

#### Next priority

P1 is now close enough to transition.

Before moving fully to P2, perform a short P1 regression/UX pass if browser interaction is available:

1. verify manual form, Google Trends and JSON import actually show persisted `discoveryNormalized` after render/reload
2. click exact-duplicate and same-story group actions and confirm the status summary/audit behavior in the real UI
3. fix any browser-only issue found

Then move to **P2 Audience Comfort**:

1. visible BLOCK/REVIEW category and reason chips, not only generic warning text
2. explicit human-review audit trail for REVIEW decisions
3. batch filter/action support based on comfort category/reason
4. Korean + English + mixed-text regression cases, including obfuscated variants where reasonable
5. preserve fail-closed behavior and do not claim OCR/image privacy masking has occurred

Do not weaken rights/approval gates while making bulk review faster.

---

## 원본 기록: 022-sol.md

### 022-sol — Audience Comfort review UI + audit + fail-closed batch controls

Updated: 2026-09-14 KST

#### Baseline

Started from current `Threads/main` after `021-sol.md` / implementation checkpoint `db95cf7c649786bedb1f40076412c155b9207d16`.

The repository tip was treated as authoritative.

#### Implemented

##### 1. New Audience Comfort feature domain

Added:

```text
app/features/discovery/comfort/
├─ comfort-model.js
├─ comfort-review.js
└─ comfort-review.css
```

Loaded centrally by `app/bootstrap/feature-loader.js` after Viral Finder.

No new cross-feature companion load chain was introduced.

##### 2. Explicit BLOCK / REVIEW category and reason UI

A new `AUDIENCE COMFORT / HUMAN REVIEW` panel now displays:

- BLOCK / REVIEW / comfortable counts
- level filter
- category filter based on `ThreadsViralModel.comfortRules`
- human-readable category chips
- current candidate title + comfort level
- latest human-review audit outcome/note
- REVIEW actions: approve / hold / reject
- BLOCK state: explicit `자동 BLOCK · 승인 불가`

The UI writes the current scan envelope to `item.comfortReview.latestScan` using a stable `scanSignature` so unchanged candidates are not continuously rewritten.

##### 3. Human-review audit trail

`ThreadsComfortReviewModel.appendAudit()` records:

```text
outcome
note
reviewedAt
reviewer
reviewSource
comfortLevelAtReview
comfortScoreAtReview
categoriesAtReview[]
blockReasonsAtReview[]
reviewReasonsAtReview[]
```

Reviewer identity remains `null` unless a real identity is supplied. The code does not invent a reviewer name.

Critical fail-closed rule:

```text
BLOCK + approve
→ blocked_comfort_cannot_be_human_approved
```

A human-review action can clear only a REVIEW candidate. It cannot override a hard BLOCK.

##### 4. Batch comfort filters/actions

The panel supports level/category filtering and these batch actions:

```text
현재 REVIEW 보류
현재 BLOCK 패스
```

`safeBatchDisposition()` refuses to use the REVIEW hold path for blocked candidates. BLOCK batch handling is skip-only.

##### 5. Mixed / obfuscated high-risk text detection

`app/viral-model.js` was hardened for narrow separator-obfuscation patterns while preserving the existing rule taxonomy.

Examples now covered:

```text
동 물 학 대
g o r e
d.o.x.x
self_harm footage
```

This is a regex safety layer only. It is not reported as semantic moderation or image/OCR moderation.

##### 6. Regression / structure tests

New:

```text
test/comfort-model.test.mjs
```

Checks:

- mixed Korean/English obfuscated hard-BLOCK terms
- doxxing variant
- REVIEW human-audit path
- reviewer identity remains null unless supplied
- BLOCK cannot be human-approved
- blocked-only batch skip
- REVIEW category hold

Updated:

```text
test/feature-layout.test.mjs
package.json
```

Package version is now `0.14.0`.

CI syntax commands now include both Comfort model/UI files and the new regression test. Feature-layout regression requires the Comfort files and central-loader registration.

#### Threads commits

Implementation commits in this run:

```text
07215edd1bb79d43e70fa707a5704f5c8f1aa154  Add Audience Comfort review model
6acb7d7ed5779e8bb30a0f68cc577cf9108e0e53  Add Audience Comfort review UI
6edb0192c2a647cf9c5e31d1044eb82157b1c6c0  Style Audience Comfort review UI
6e902d9e204cf6377a9041465dcfd27ff412b0bf  Load Audience Comfort review feature
d303d6a6558b8a5aeaca800b83b9f541c8f52968  Harden mixed and obfuscated comfort detection
8cfa1be37045fe0e3be4e89ff90557224a96a294  Add Audience Comfort model regression tests
b649bbf6f421b357b375a352f1e4ab0d599a100f  Guard Audience Comfort feature layout
3ef258ec679ac3c1d05913a7e9a9511ba5b65760  Check Audience Comfort feature in CI
```

Handoff update after implementation:

```text
2ccf217f80cc5e6b141919943535c8bbfcdc621b  Handoff Audience Comfort review implementation
```

#### Validation

GitHub Actions run:

```text
34779668883
head: 3ef258ec679ac3c1d05913a7e9a9511ba5b65760
job: 103784236352
```

Observed results:

```text
JavaScript syntax and regression checks  SUCCESS
Local server smoke test                 SUCCESS
job conclusion                          SUCCESS
```

This validates syntax/regression/server smoke, not a full interactive browser E2E.

#### Discovery

No new production discovery candidate was promoted in this run. The run was focused on P2 safety/review implementation.

Existing rule remains:

```text
direct/verifiable public metric → may be stored as observed evidence
secondary digest / unclear metric → not canonical observed engagement
```

Blind/DCInside remain public-index/user URL/screenshot/manual Capture paths only; no bulk crawler was added.

#### Blockers / limitations

- No full interactive-browser E2E was available in this run, so the new Comfort panel should receive a click/reload/mobile-width pass when browser interaction is available.
- Human review is intentionally not an override for hard BLOCK.
- Image privacy remains separate; no OCR/image masking success is claimed.
- Live publication remains fail-closed behind real credentials/scopes plus final human approval.

#### Next priority

Continue P2 before moving fully to P3:

1. expose Comfort reason chips inside existing Viral Finder rows as well as the dedicated panel
2. make downstream ready/editorial handoff explicitly require a human-cleared REVIEW while never allowing BLOCK override
3. expose audit history in candidate detail/export metadata where appropriate
4. add false-positive / false-negative tests for quoted/news-reporting contexts and mixed punctuation without weakening hard BLOCK
5. browser-test filters/actions/reload/mobile layout when possible

Then proceed to P3 bulk candidate review by reusing existing selection/group primitives instead of duplicating them.

---

## 원본 기록: 023-sol.md

### 023-sol — Comfort downstream gate + P3 bulk candidate review start

Updated: 2026-09-14 KST

#### Baseline

Started from `Threads/main` handoff checkpoint `3ef258ec679ac3c1d05913a7e9a9511ba5b65760` / `022-sol.md`. Repo tip remained authoritative.

#### P2 completed this run

##### Current-scan-bound human clearance

`app/features/discovery/comfort/comfort-model.js` now has:

- `scanSignature()`
- `clearanceStatus()`
- `mayAdvance()`
- `exportEnvelope()`
- `scanSignatureAtReview` in each human audit entry

REVIEW candidates no longer become production/editorial eligible merely because `humanClearedReview=true` once existed. Human approval is valid only when the current Comfort scan signature still matches the approved scan. If candidate content changes, clearance becomes `stale-human-review` and downstream progression is blocked until re-reviewed.

Hard BLOCK remains non-overridable.

##### Viral Finder inline Comfort visibility

`app/features/discovery/comfort/comfort-review.js` now decorates Viral Finder candidate rows with:

- current Comfort gate state
- explicit category/reason chips
- `Comfort 통과`, `REVIEW 사람 승인 필요`, `REVIEW 재검토 필요`, or `자동 BLOCK`

The dedicated Comfort panel still provides REVIEW approve/hold/reject and BLOCK skip-only handling.

##### Ready gate enforcement

The existing `선택 → 제작 후보` button is intercepted fail-closed before the Viral Finder handler runs.

- comfortable → may proceed
- REVIEW with matching human approval → may proceed
- REVIEW without approval → removed from ready selection
- stale REVIEW approval → removed from ready selection
- BLOCK → removed from ready selection

If nothing eligible remains, the ready action itself is cancelled.

##### Candidate detail + audit export

The selected candidate detail receives an `Audience Comfort 기록` section with current gate and recent audit history.

The Comfort panel now includes `Comfort 감사 JSON`, exporting all candidates with latest scan, current clearance, and human audit history. This is metadata export only; it does not claim external moderation, OCR, or image privacy work.

##### P2 regressions

`test/comfort-model.test.mjs` now verifies:

- REVIEW cannot reach ready without human clearance
- matching current-scan human approval can reach ready
- content/scan changes invalidate prior approval
- BLOCK still cannot advance
- comfortable candidates remain eligible
- hold clears stale downstream clearance state
- export envelope contains clearance/audit metadata

#### P3 started this run

New:

```text
app/features/discovery/viral/bulk-review-model.js
app/features/discovery/viral/bulk-review.js
test/bulk-review-model.test.mjs
```

Central loader now adds `bulk-candidate-review` after Audience Comfort.

##### Bulk controls

Viral Finder gains:

- `보이는 항목 선택`
- `선택 해제`
- `선택 보류`
- bulk tag input + `태그 적용`
- `선택 → 편집 검토`

Existing research/ready/skip and duplicate-group actions are reused rather than duplicated.

##### Editorial handoff packet

Bulk editorial handoff creates `item.editorialHandoff` containing:

- source key/url/risk/source/lane metadata when available
- Viral score snapshot
- Comfort export envelope
- review tags
- created timestamp

Editorial handoff calls the Comfort gate with target `editorial`; BLOCK, uncleared REVIEW, and stale REVIEW are skipped fail-closed. Safe or current-human-cleared REVIEW candidates move to `research` and receive audit metadata.

Bulk review actions also persist `item.bulkReview.audit`.

##### Tests

`test/bulk-review-model.test.mjs` verifies:

- comfortable candidate can editorial-handoff
- uncleared REVIEW cannot handoff
- BLOCK cannot handoff
- REVIEW can handoff after matching human approval
- bulk tags persist
- reject moves candidate to skip

Package is now `0.15.0`; syntax/test scripts include the P3 model/UI/test.

#### Commits

```text
f8ad61616a655e5be26d0e4705ff993933ab90e8  Enforce downstream Audience Comfort clearance
f291e3f9634d8946552603f3cfd934593f24af24  Wire Comfort clearance into Viral Finder
1e9726f6c3fb84e369f62d7d7ab3f844772f2b45  Style Comfort clearance and audit surfaces
ca5994f32a51b5fa5460e6c836b2c6d34719dbbb  Test Comfort clearance and stale review gates
afd6734797a64b6a6f3064fd71ac84af92cbf683  Guard Comfort downstream gate wiring
602582dc7259931ca41f167a0e9c117ce243e912  Bump package for Comfort gate hardening
dece2bc058debf0a175ec43f1676118386dc998c  Add bulk candidate review model
c7a7221722f585416574cd653563306fb66e12bb  Add bulk candidate review controls
3d49414bcacc43b9a88d642439ec5afb162b1b70  Load bulk candidate review after Comfort
addf37ffee78815a140358285568c66519594b2e  Test bulk candidate review gates
6e9fbcbbe88c3b3cb1d8c4c3e62a2f02b54d8375  Check bulk candidate review feature in CI
ebc0236abab6880d18c43ef63b8d9f9e20776ccb  Guard bulk review feature layout
```

#### Validation

GitHub Actions run `34784015681`, head `ebc0236abab6880d18c43ef63b8d9f9e20776ccb`, job `103796086829`:

- JavaScript syntax and regression checks: SUCCESS
- Local server smoke test: SUCCESS
- job conclusion: SUCCESS

This is not full interactive-browser E2E.

#### Discovery

No new production candidate was promoted in this run. Work focused on downstream safety integrity and bulk-review mechanics. Existing discovery policy remains unchanged: only direct/verifiable public metrics may become observed evidence; secondary digest values are not promoted as canonical observed engagement. Blind/DCInside remain public-index/user URL/screenshot/manual Capture paths only.

#### Next priority

Continue P3 before moving to P4:

1. add explicit selected-item count/status summary for bulk hold/tag/editorial handoff and better large-list feedback
2. add keyboard/accessibility shortcuts only if they do not conflict with text inputs
3. make group-level editorial handoff use the same `ThreadsBulkReviewModel` gate/audit path
4. add review-tag filters and bulk tag removal/normalization
5. browser-test selection sync, ready-gate interception, reload persistence, and mobile width when browser interaction is available
6. then move to P4 manual drag-rectangle image privacy masks before PNG export

Do not weaken P2 gates while expanding P3.

---

## 원본 기록: 024-sol.md

### 024-sol — P3 bulk review hardening + P4 manual image privacy masking start

Updated: 2026-09-14 KST

#### Baseline

Started from `Threads/main` handoff checkpoint `ebc0236abab6880d18c43ef63b8d9f9e20776ccb` / `023-sol.md`. Repository tip remained authoritative.

#### P3 completed materially for current workflow

##### Bulk review model

`app/features/discovery/viral/bulk-review-model.js`

Added:

- `normalizeTag()` — NFKC normalization, trim, whitespace collapse, case normalization
- `normalizedTags()` — dedupe + maximum 20 retained tags
- `remove-tag` bulk action
- `summarize()` — changed/skipped/reason counts for large-list feedback
- editorial packets now export normalized review tags

##### Bulk review UI

`app/features/discovery/viral/bulk-review.js`

Added:

- selected item count
- visible item count
- selected status summary
- bulk tag removal
- exact normalized tag filter
- accessible live status text
- `Esc` clears selection and `Ctrl/Cmd+Shift+A` selects visible candidates, but only when focus is not in input/textarea/select/contenteditable
- result summaries reuse model `summarize()`

##### Group editorial handoff safety

`app/features/discovery/viral/group-actions.js`

New `그룹 → 편집 검토` action now calls `ThreadsBulkReviewModel.apply(..., "editorial-handoff")`.

This means group editorial handoff uses the same Audience Comfort gate and bulk audit path as normal selected-item editorial handoff. BLOCK, uncleared REVIEW, and stale REVIEW cannot be promoted by the group shortcut.

Group summary now also surfaces candidates with editorial handoff state.

##### Regression coverage

`test/bulk-review-model.test.mjs` now covers:

- changed/skipped summaries
- normalized tags
- NFKC-equivalent tag dedupe
- bulk tag removal
- missing tag removal skip reason
- normalized tag export in editorial packet

#### P4 started — real manual image privacy masking

New files:

```text
app/features/production/cards/privacy-mask-model.js
app/features/production/cards/privacy-mask.js
test/card-privacy-mask-model.test.mjs
```

Central `feature-loader.js` now loads these after the existing Card Story model and Card Factory.

##### Behavior

- User enables `마스킹 모드`.
- On a `capture-image` preview card, pointer drag is translated from displayed canvas coordinates into the real 1080x1350 canvas coordinate system.
- Pointer-up writes a **real black rectangle directly onto the canvas**.
- Masking a card invalidates its previous privacy-reviewed state until the user reviews again.
- Every capture-image card gets a `개인정보 검토 완료` control.
- Individual capture-image PNG export is blocked until that capture is explicitly marked reviewed.
- `PNG 전체 저장` is blocked while any capture card is unreviewed.
- Selecting new image files or rebuilding preview resets the session privacy review state.
- `Privacy JSON` exports mask rectangles and gate state.
- Export metadata explicitly records:
  - `automatedOcrClaimed: false`
  - `automatedFaceDetectionClaimed: false`
- no OCR, face detection, or automatic image privacy success is claimed.

The privacy state is intentionally session-only for now, matching the existing session-only screenshot handling. Original screenshot bytes are not persisted.

##### Model regression coverage

`test/card-privacy-mask-model.test.mjs` covers:

- coordinate normalization independent of drag direction
- canvas-bound clamping
- minimum usable rectangle size
- capture-card index extraction
- fail-closed export gate for unreviewed capture cards
- reviewed-with-zero-mask case (explicit human determination that no rectangle is needed)
- privacy envelope and no-fake-automation assertions

Package bumped to `0.16.0`; syntax/test scripts include the new feature and test.

#### CI / validation

- `9bdf57825b2a1738435caaf258c5e24552d258ee` / Actions `34785529782`: P3 normalized-tag/bulk-summary checks + server smoke **SUCCESS**.
- `45643af2d8c2702cdefc74f7f329291cc02754fa` / Actions `34785653796`: privacy-mask feature included in syntax/regression + local server smoke **SUCCESS**.
- `4c3e724bec79395658c2c88df2797728f24d7783` initially failed only because the new layout test incorrectly required the model's internal `image-privacy-review-required` code string to be duplicated inside the UI file. No production code failure was identified from that run.
- guard fixed in `29a9b996e3513e4c8d9d1b8e6cc5ebeffc789c81` by checking the model code in the model and the actual UI/model integration symbol in the UI.
- final verification: Actions run `34785739288`, job `103800763036` — **SUCCESS**. JavaScript syntax/regression checks **SUCCESS**, Local server smoke test **SUCCESS**.

Verified implementation checkpoint for this handoff:

```text
29a9b996e3513e4c8d9d1b8e6cc5ebeffc789c81
Actions 34785739288 / job 103800763036 — SUCCESS
```

#### Commits this run

```text
d127122049fd6a084ffa1be727dfc1b6b76eb61c  Harden bulk review tags and summaries
40910500d7051f0bed0a14a4fd3fab308168bed4  Improve bulk review controls and tag workflow
2efbfb6805b474753e7695b4d0579c8845228cbc  Route group editorial handoff through bulk safety gate
9bdf57825b2a1738435caaf258c5e24552d258ee  Test normalized tags and bulk summaries
0ae2fc3a0434af0668b6674e5a7752fcc931cbcb  Add card image privacy mask model
b0ed7c298118153a3e3a9e4824636a482a1cef95  Add manual card image privacy masking
13752fc56bd7b2fcac71494662879284f4bf471a  Load card privacy masking feature
291fc2e25447886659026774abc9386187a74da7  Test manual card privacy mask gate
45643af2d8c2702cdefc74f7f329291cc02754fa  Check card privacy mask feature in CI
4c3e724bec79395658c2c88df2797728f24d7783  Guard bulk review and card privacy feature layout
29a9b996e3513e4c8d9d1b8e6cc5ebeffc789c81  Fix card privacy layout regression guard
```

#### Discovery

No production candidate was promoted this run. Work focused on workflow safety/integrity and the production privacy gate. Existing rule remains: only directly visible/verifiable engagement can become observed evidence; secondary digest values remain non-canonical; Blind/DCInside stay public-index/user URL/screenshot/manual Capture paths only.

#### Next priority

Continue P4 before moving to P5:

1. browser-test actual drag masking/export interception/mobile coordinate scaling when browser interaction is available
2. add undo-last-mask per capture + clearer per-card mask count
3. bind the privacy review to an exact session image identity tuple (name/size/lastModified or stronger local fingerprint) so review cannot carry to a replaced file
4. integrate privacy envelope into Card Factory saved manifest/storyboard metadata without persisting original image bytes
5. expose image privacy review state to final `04 REVIEW_PUBLISH`
6. then continue P5 Content Warehouse

---

## 원본 기록: 025-sol.md

### 025-sol — P4 privacy gate completion + P5 Warehouse READY/provenance/history

Updated: 2026-09-14 KST

#### Baseline

Started from the latest `Threads/main`, not the stale 024 checkpoint. Repository tip already contained additional P4 work through `5ba22d2b918ae924db381988fa0cb0b62048b630` (`Bind card privacy review to exact image identity`).

Important pre-existing work confirmed before this run:

- manual drag rectangle masks are applied to the actual export canvas
- per-capture undo-last-mask exists
- per-card mask count/review status exists
- review state is bound to exact session image identity `(name, size, lastModified, type)`
- replacing an image makes an old review stale/fail-closed
- Card Factory saved metadata/manifest includes the privacy envelope without original image bytes
- `04` approval queue / `safety-gate.js` already checks capture-image privacy gate and shows PASS/PENDING/STALE
- no OCR or face detection success is fabricated

Therefore P4 is materially complete for the current browser/session architecture. Browser E2E remains useful but is not a reason to stop P5 implementation.

#### P5 Content Warehouse — substantive expansion

##### `app/warehouse-model.js`

Expanded the model from the earlier HOT/EVERGREEN scheduling MVP into a fuller warehouse record.

New/expanded behavior:

- warehouse buckets now support **READY / HOT / EVERGREEN**
- default legacy/unset warehouse bucket becomes READY
- queue priority remains HOT-first, then READY, then EVERGREEN at equal manual priority
- `themeTags` and `formatTags` are normalized with NFKC, trim, whitespace collapse, lower-case dedupe, max 20
- bounded warehouse `history` (latest 50 records)
- `freshnessState()` reports `scheduled`, `expired`, `expiring-soon`, `fresh-today`, `fresh`, `hot-no-expiry`, or `open`
- `assetSummary()` reports text/card assets, card count, capture-image count and image privacy state
- `reviewSummary()` snapshots Research/Draft/Safety/rights/privacy/Comfort/current publish approval state
- `provenanceSnapshot()` records candidate id, source/canonical URL, discovery source/lane, source risk/adapter state, and only observed engagement evidence when actually marked observed
- `appendHistory()` records material warehouse metadata/tag changes without pretending they are content edits
- queue eligibility still fails closed on Comfort, production asset, image privacy, Safety Gate, and current human publish approval

##### `app/content-warehouse.js`

UI is now aligned to the model:

- summary/filter tabs for READY / HOT / EVERGREEN
- separate `지금 게시 가능` filter so READY warehouse classification is not confused with actual publish eligibility
- per-item freshness badge
- source/discovery lane visibility
- rights/privacy status visibility
- capture-image privacy PASS/PENDING visibility
- Research / Draft and current human approval visibility
- editable theme tags and format tags
- saved provenance snapshot on first warehouse save
- saved bounded change history
- visible history count
- warehouse scheduling/taxonomy changes deliberately do **not** stale publish approval because they do not alter content bytes/text; content changes elsewhere continue to stale approval through the existing integrity rules

##### Regression coverage

`test/warehouse-model.test.mjs` now additionally covers:

- READY/HOT/EVERGREEN queue ordering
- scheduled/expired freshness states
- privacy-reviewed capture assets
- NFKC-equivalent tag dedupe
- format tag dedupe
- warehouse change history entries
- provenance source/lane/canonical URL
- observed engagement only when explicitly marked observed
- review summary/current approval snapshot

Package bumped from `0.16.1` to `0.17.0`.

#### Validation

Meaningful checkpoints this run:

```text
ae097c3c9235641614ea3e254c084ca4ba45487c  Expand warehouse provenance freshness and history model
2787f3607694dec629ac1b0b0f9444532d291f71  Test warehouse provenance freshness and history
20f97152163ce387ac3376a2cf2da63d03a0d787  Add READY provenance tags and history to content warehouse
3f747a4695c5d7be09ea6e728c3708f38801139c  Bump warehouse workflow version
```

GitHub Actions:

- `20f97152163ce387ac3376a2cf2da63d03a0d787` → run `34789798402` → **SUCCESS**
- final checkpoint `3f747a4695c5d7be09ea6e728c3708f38801139c` → run `34789810832` → **SUCCESS**

The workflow includes `npm run check` (syntax + all regression tests) and local server smoke.

#### Discovery

No new production candidate was promoted in this run. The run focused on P4/P5 state integrity. Do not invent engagement metrics to populate warehouse provenance: only existing `discoveryNormalized.engagementEvidence.mode === "observed"` can be copied into the provenance snapshot.

#### Blockers

- live platform publishing credentials/scopes/human approval are still not present; this is not treated as success and does not block non-live implementation
- browser pointer E2E for the manual mask tool is still useful when a browser-capable environment is available, but model/UI/CI gates are green

#### Next priority

P5 is materially usable but can still be refined. Continue in order:

1. add warehouse detail/export for provenance/history if useful, and auto-suggest theme/format tags from existing classification without overwriting human tags
2. then P6 official image/carousel publishing capability model and dry-run validator using official APIs only
3. provider state must distinguish unsupported / credential-required / ready-to-validate / live-disabled
4. no credential or human approval => fail closed, no fake success
5. after P6, P7 scheduler/queue: HOT priority plus theme/source/format spacing, pause/stop/post-now/reorder and visible reason audit
6. P8 DB/server persistence + migrations + multi-account only after those are materially complete

---

## 원본 기록: 026-sol.md

### 026-sol — DevDesign Threads automation review + optional Buffer publisher

Updated: 2026-09-14 KST

#### User request

Reviewed:

`https://devdesign.kr/columns/threads-publish-automation`

The article describes a practical workflow where content creation and publishing are separated, a person still chooses/approves the content, and Buffer is used as a scheduling/publishing layer for Threads.

#### What was useful and adopted

The following ideas fit the existing project and were implemented:

1. **Keep creation and publishing separate.**
   - Existing Research/Draft/Card workflow remains unchanged.
   - Publishing adapters only receive an already approved final draft.

2. **Keep human checkpoints.**
   - Existing human selection + Research review + Draft approval + Safety Gate + current publish approval are preserved.
   - Buffer UI adds one more explicit final checkbox/confirm immediately before sending a request.

3. **Use Buffer as an optional delivery/scheduling adapter.**
   - Direct official Threads API remains supported and is not replaced.
   - Buffer can be chosen for next queue slot, custom scheduled time, or share now.

4. **Keep secrets out of Git/browser.**
   - `BUFFER_API_KEY` is server-side only.
   - `.env*` and `config/buffer.local.json` are gitignored.
   - Browser receives only connector status/channel metadata.

5. **Persist external delivery IDs/status separately.**
   - Buffer requests go to `item.bufferDeliveries[]`.
   - They do NOT become `item.publications[]` just because a request/schedule was accepted by Buffer.
   - Actual sent status should be verified by a future sync before canonical publication/metrics promotion.

6. **Threads thread payload contract.**
   - Backend adapter supports Buffer `metadata.threads.thread`.
   - First segment is also top-level `text`.
   - Current UI intentionally exposes one approved post only; it refuses >500 characters rather than silently splitting text after human approval.

7. **Scheduling time normalization.**
   - Browser `datetime-local` is converted to ISO/UTC before Buffer `customScheduled` submission.

#### Official Buffer documentation verification

Article ideas were not copied blindly. Current Buffer GraphQL API docs were checked for:

- API host/auth
- organizations/channels lookup
- Threads channel filtering
- `createPost`
- `addToQueue`, `shareNow`, `customScheduled`
- `dueAt`
- Threads `metadata.threads.thread`
- remote URL-based media assets

Media/card publishing through Buffer is deliberately not enabled yet because the current Card Factory output is a local Canvas image; a stable hosted/upload URL lifecycle is required first.

#### Actual code changes

##### Server adapter

New root file:

`buffer.mjs`

Exports:

```text
getBufferStatus
listBufferThreadsChannels
saveBufferThreadsChannel
buildBufferPostInput
publishThreadsViaBuffer
```

Configuration:

```text
BUFFER_API_KEY                 secret, required for Buffer calls
BUFFER_THREADS_CHANNEL_ID      optional direct override
config/buffer.local.json       locally selected Threads channel, gitignored
```

New server endpoints:

```text
GET  /api/buffer/channels
POST /api/buffer/channel
POST /api/buffer/publish
```

`/api/buffer/publish` runs the same `validateApprovedCandidate()` used by direct Threads publishing before calling Buffer. An unapproved candidate therefore fails before any third-party request.

##### Browser feature

New feature folder:

```text
app/features/publish/buffer/
├─ README.md
├─ buffer-publish-model.js
├─ buffer-publisher.js
└─ buffer-publisher.css
```

Feature is registered in the central `app/bootstrap/feature-loader.js` rather than through a hidden cross-feature load chain.

UI behavior:

- connector box shows Buffer status
- if API key exists but channel is not chosen, query Buffer Threads channels and save a chosen channel locally
- current approved items get `Buffer 예약/게시`
- choices: next Buffer queue / custom time / immediate
- approved text is read-only
- >500 chars are blocked and must return to Draft Studio for explicit editing/reapproval
- final confirmation is required
- accepted Buffer job is recorded as external delivery state

##### Repo safety/config

New:

- `.gitignore`
- `config/buffer.example.json`

No token/key is committed.

##### Tests

New:

- `test/buffer-adapter.test.mjs`
- `test/buffer-publish-model.test.mjs`

Expanded:

- `test/feature-layout.test.mjs`
- `.github/workflows/check.yml`

CI explicitly verifies:

- Buffer is unconfigured without secret
- `/api/buffer/channels` returns `503 buffer_api_key_missing` without key
- unapproved `/api/buffer/publish` returns `409 candidate_not_ready`
- Buffer feature assets are served and central bootstrap contains them
- queue/thread/customScheduled request construction
- >500 and past-due validation
- stale approval rejection

Package version: `0.18.0`.

#### Validation

Final repository checkpoint for this work:

```text
06e53ae50ddb608f9abfa62d13381ae5281434f6
```

GitHub Actions:

```text
run 34793505552
conclusion SUCCESS
```

This includes syntax/regression and local server smoke tests.

#### Important non-claims

- No live Buffer E2E has been completed because no Buffer API key/channel has been connected in this environment.
- No public post was created in this work.
- Direct Threads live E2E also remains separate until actual credentials and explicit human approval are present.
- Buffer scheduled acceptance is not represented as confirmed Threads publication.

#### Next

1. Add Buffer delivery-status sync before promoting sent Buffer posts into canonical `publications[]`.
2. Finish P6 official image/carousel capability + dry-run validation. For Buffer media, add an explicit safe asset-hosting/upload stage before enabling local Card Factory images.
3. Build P7 internal Scheduler planner with HOT + theme/source/format spacing. Allow planner to target either direct Threads path or Buffer queue/custom schedule without changing content approval semantics.
4. Continue staged folder migration; do not destabilize the now-green app by moving all legacy files at once.

---

## 원본 기록: 027-sol.md

### 027-sol — real browser E2E + P7 scheduler planner

Updated: 2026-09-14 KST

#### Baseline

Repository tip at start was `489bdbd21b40f761a852b057c5bfaccc3b763468` (`Fix card privacy preview observer loop`). The older NEXT_RUN handoff was stale; repo tip was treated as authoritative.

Important work already present on current main before this run:

- Buffer optional publisher (`0.18.0` era)
- official Threads IMAGE/CAROUSEL dry-run capability (`8333e9e72e59d1289cc17064ce16577a688dda89`)
- browser render-loop fixes for Audience Comfort and Card privacy observers
- P1–P5 workflow/gates from prior handoffs

#### Real browser E2E performed before new scheduler work

Using installed Chrome 140 through Playwright against the actual local app/server:

- feature bootstrap reached `ready`
- candidate form submission worked
- discovery normalization stripped `utm_source` from the canonical URL
- Viral Finder rendered
- Audience Comfort rendered
- Content Warehouse rendered
- official media capability UI rendered `토큰 필요 · dry-run 가능`
- no browser page errors were recorded

Card privacy pointer E2E also passed:

- local image selected
- Card Factory built a capture-image card
- privacy mask mode enabled
- a real mouse drag created one rectangle mask on the canvas
- explicit privacy review clicked
- privacy gate became `allowed: true`
- `captureCount=1`, `reviewedCount=1`, `mask count=1`
- PNG download gate became enabled
- no browser page errors were recorded

No public post was created.

#### P7 Scheduler / Queue implemented

New feature folder:

```text
app/features/publish/scheduler/
├─ scheduler-model.js
├─ scheduler.js
└─ scheduler.css
```

##### Model behavior

- consumes only Warehouse `queueEligibility()` items, so Comfort / Safety / rights / privacy / current human approval gates remain authoritative
- preserves HOT > READY > EVERGREEN scoring from Warehouse
- supports manual reorder rank without modifying approved content
- plans spacing by:
  - base slot interval
  - same theme gap
  - same source gap
  - same format gap
- stores visible scheduling reasons such as:
  - `hot-priority`
  - `manual-order`
  - `theme-spacing:120m`
  - `source-spacing:90m`
  - `format-spacing:60m`
  - `queue-priority`
- HOLD / expired / not-before / non-approved items never enter the current plan

##### UI behavior

- new `PUBLISH SCHEDULER` panel
- configurable base/theme/source/format spacing in minutes
- RUNNING / PAUSED / STOPPED control state persisted only in browser localStorage
- up/down manual reorder
- visible scheduled local time and reasons per row
- `지금 게시` does **not** call an external API or bypass review
  - it routes the item to `04 REVIEW_PUBLISH`
  - final publication remains owned by 04 and still requires its own human confirmation / credential path
- scheduling metadata does not stale content approval because it does not modify approved content bytes/text

The feature is registered in `app/bootstrap/feature-loader.js`; no hidden cross-feature loader was added.

#### Tests / CI

New targeted test:

`test/scheduler-model.test.mjs`

Covers:

- HOT-first ordering
- base interval
- theme spacing
- source spacing
- manual reorder
- HOLD exclusion

The first CI attempt correctly failed because Node VM arrays came from a different realm while the test used `deepStrictEqual`. The test assertion was fixed by normalizing IDs into the host realm; no production behavior was weakened.

Final verified checkpoint:

```text
bc3b63c0eea4a6472605a4887e9772dee839bdb3
```

GitHub Actions:

```text
run 34822773130
status completed
conclusion SUCCESS
```

Workflow includes JavaScript syntax/regression tests and local server smoke.

Package is now `0.20.0`.

#### Secrets / live publishing

- no plaintext Instagram/Threads password was committed or used
- official integrations remain token/scope based
- no fake API request ID, success, publication URL, engagement metric, OCR, or privacy claim was produced
- live Threads/Instagram publication remains blocked without official credentials/scopes and final human approval

#### Blockers / limitations

The remote desktop connection dropped after the scheduler commits, so a fresh Chrome pointer E2E of the **new scheduler panel itself** could not be run in this same pass. The scheduler model has green targeted CI, while the immediately preceding full app/card browser E2E on the current architecture was green. Re-run browser E2E for scheduler controls when the machine is reachable; do not represent it as already observed.

#### Next priority

1. browser E2E the Scheduler panel: plan render, pause/resume/stop, reorder, and `지금 게시` route to 04 without external publication
2. finish remaining P6 live-disabled provider audit details / real response-error audit separation where useful; do not enable live media without official credentials and explicit server-side opt-in
3. extend P7 with durable scheduler audit/history and optionally provider target selection (direct Threads vs Buffer) without changing final approval ownership
4. then P8 DB/server persistence, migrations/versioning, and multi-account experiment/profile state

---

## 원본 기록: 028-sol.md

### 028-sol — scheduler provider targets + bounded audit trail

Updated: 2026-09-14 KST

#### Baseline

Repository tip at start was `d35fc8257f1ed00da07015dc7eb3ea24383cad42`, whose functional parent/checkpoint was `bc3b63c0eea4a6472605a4887e9772dee839bdb3` (`0.20.0`). The latest sequential note was `027-sol.md`.

P1–P5 were already materially complete. P6 official media dry-run and optional Buffer path were already present and fail-closed. P7 scheduler model/UI already had HOT priority, spacing, pause/resume/stop, manual reorder and post-now routing to 04.

#### Browser availability

The authorized Windows machine was offline during this run. Therefore no new Scheduler browser E2E is claimed here. Earlier E2E remains valid for the pre-P7 full app/card path, including real drag privacy masking. New Scheduler UI E2E remains the first browser task when the machine is reachable.

#### P7 implementation this run

##### `app/features/publish/scheduler/scheduler-model.js`

Added explicit provider-target metadata without transferring publication ownership away from 04:

- provider IDs: `threads-direct`, `buffer`
- unknown provider values fail safely to `threads-direct`
- `providerTarget(item)` normalization
- `setProviderTarget(item, target)` records a scheduling audit when target changes
- every audit row records owner `04_REVIEW_PUBLISH`
- `appendAudit()` bounds history to latest 100 entries by default
- Scheduler plan metadata now exposes provider target while leaving eligibility and spacing behavior unchanged

Provider target is metadata only. It is not an API call, delivery record, or publication success.

##### `app/features/publish/scheduler/scheduler.js`

Expanded the Scheduler UI and persistence:

- each planned row has a target-provider selector
- target choices are `Threads 공식 직접` and `Buffer 보조 경로`
- provider changes persist through the existing browser state and are audited
- scheduler-control history records pause/resume/stop/replan/provider-target changes
- per-item audit records:
  - provider-target changes
  - manual reorder
  - blocked `post-now`
  - successful route-to-04 `post-now`
- status shows scheduler control-history count
- each item shows its scheduler change-record count
- `지금 게시` still only routes to `04 REVIEW_PUBLISH`; no external API is invoked by Scheduler
- user-facing message explicitly says external API was not called during routing

##### `test/scheduler-model.test.mjs`

Added regression coverage for:

- default provider = `threads-direct`
- valid Buffer target
- provider change audit
- owner fixed to `04_REVIEW_PUBLISH`
- invalid provider fallback
- bounded 100-row audit behavior

Existing HOT ordering, spacing, manual reorder, HOLD exclusion tests remain.

Package bumped to `0.21.0`.

#### Commits

```text
2dfd5801cd38668a15f48037b80c5a5ddd6c863c  Add scheduler provider targets and audit model
74c289083d1080f976e66bc77f6fd7bdc199de14  Test scheduler provider targets and audit history
ac7d468013a0b8180ef6660e352d654040d2805a  Add scheduler provider choice and audit trail
980106eb8b2e36e8b0a13c248e838baf1a0d7380  Bump scheduler audit workflow version
93be3b16f58d044c06f50b6d6c7b50eca4d85f52  Advance handoff through scheduler audit targets
```

#### Tests / CI

Final verified functional checkpoint:

```text
980106eb8b2e36e8b0a13c248e838baf1a0d7380
```

GitHub Actions run:

```text
34825571520
status: completed
conclusion: SUCCESS
```

The workflow covers JavaScript syntax, full regression suite including scheduler tests, and local server smoke.

No new browser E2E is claimed because the remote machine was offline.

#### Discovery

No new public discovery candidates were promoted in this run. The run focused on scheduler control integrity and auditability. No engagement metric was fabricated.

#### Secrets / live publishing

- no plaintext account password or token was committed
- no provider target was treated as a real publication
- no fake Buffer/Threads request ID, delivery status, media URL, success, or metric was produced
- live official media remains fail-closed without credentials/scopes/human approval

#### Blockers / next priority

1. when Windows/Chrome is reachable, run actual Scheduler browser E2E: plan render, provider select, pause/resume/stop, reorder, audit count changes, post-now route to 04, verify no external publication occurs
2. verify the current official Buffer API query shape before implementing delivery-status sync; do not guess a GraphQL field
3. until Buffer delivery is actually verified as sent/delivered, keep accepted Buffer jobs separate from canonical publications
4. finish useful P6 real external response/error audit separation
5. then start P8 DB/server persistence, migrations/versioning and multi-account/profile state, including migration of scheduler audit/history from browser persistence

---

## 원본 기록: 029-sol.md

### 029-sol — P8 versioned persistence foundation

Updated: 2026-09-14 KST

#### Baseline

Started from current `Threads/main` tip `93be3b16f58d044c06f50b6d6c7b50eca4d85f52`, not the older checkpoint text. Latest prior operations note was `028-sol.md`.

P1–P5 were already materially complete. P6 official media dry-run/fail-closed publishing and optional Buffer path were present. P7 Scheduler already had HOT priority, spacing, pause/resume/stop, manual reorder, provider target metadata/audit and post-now routing only to 04.

#### Browser availability

The authorized Windows/Chrome device was unavailable during this run. Therefore the missing fresh Scheduler UI E2E was not claimed. Earlier real browser E2E remains valid for app bootstrap/discovery/card privacy-mask workflow, but P7 controls still require a fresh real browser pass when the machine reconnects.

#### P8 implementation this run

##### `app/features/persistence/state/state-model.js`

Added the first versioned migration envelope for browser/session state:

- schema version 1
- app version/items preserved
- Scheduler control options/history represented separately
- profile and experiment metadata slots included
- role chain stored explicitly with final publication owner `04_REVIEW_PUBLISH`
- legacy `{version, items}` browser state migrates to schema v1
- newer unknown schema fails closed
- Scheduler history bounded to latest 100 rows
- recursive secret-like key rejection for password/passwd, access/refresh token, API key, authorization, cookie and secret fields

No account password/token is copied into the snapshot.

##### `test/persistence-state-model.test.mjs`

Regression covers schema/role owner, item and Scheduler metadata preservation, bounded history, legacy migration, restore, secret rejection and unsupported newer schema rejection.

##### `persistence.mjs`

Added a server-side JSON store foundation:

- schema validation before write/read acceptance
- recursive secret-field rejection
- optimistic-concurrency `revision`
- stale expected revision fails with conflict rather than overwriting newer state
- atomic temp write + rename
- requested file mode `0600`
- missing file returns revision 0 / null snapshot

This module is a storage foundation only; no implicit live state writes are enabled yet.

##### `test/persistence-store.test.mjs`

Regression covers empty-store read, first write/read, revision increment, stale revision conflict, secret rejection, unsupported schema rejection and temporary test-state cleanup.

##### Runtime hygiene

`.gitignore` now ignores `data/runtime/`. Runtime persistence files must never be committed.

Package moved to `0.22.0` and both persistence tests are part of project syntax/regression checks.

#### Commits

```text
c550eefd386f8cfba7a92522416d1c2af74eecf9  Add versioned persistence snapshot model
7effa9df9689f83f120595f05fb535f0775cf62f  Test persistence snapshot migrations and secret rejection
1aa540e2c3cd0a27eebf4f1bacbccaf30c14b393  Run persistence migration model in project checks
98273d8f4fdcf9d8b9d6e5d60ad5a6d85fb343ed  Add fail-closed file persistence store
75c096e63ee764bcab9eed9a5fd6733c1695bb50  Test file persistence revision and secret gates
b9a1db6fccfc4ce3b9db95fbefc6ee3f5550bea7  Ignore runtime persistence state
49c5bc501d9b39a0b6362560e107984a3c84e334  Run persistence store regression in checks
642bedbc44bdbc49de3cc92f5eac683278f7493c  Advance handoff into P8 persistence
```

#### Validation observed during run

Final Threads tip checked in this run:

```text
642bedbc44bdbc49de3cc92f5eac683278f7493c
GitHub Actions run 34826399840
status: completed
conclusion: SUCCESS
```

The workflow includes JavaScript syntax, full regression suite with both persistence regressions, targeted existing publish/scheduler tests and local server smoke.

No browser E2E was fabricated while the remote machine was unavailable.

#### Security / publication invariants

- no plaintext user password, provider token, API key, auth header or cookie committed
- persistence does not change content ownership or final publication owner
- Scheduler still only routes `post-now` to `04_REVIEW_PUBLISH`
- no persistence write is a publication or provider API success
- Buffer acceptance remains separate from canonical publication until actual delivery can be verified

#### Next priority

1. run fresh Scheduler Chrome E2E immediately when the authorized Windows machine reconnects
2. add explicit server GET/PUT persistence endpoints using revision conflict semantics
3. add an explicit browser bridge to export/import current local state into the versioned snapshot; no silent overwrite
4. migrate account/profile and experiment state without credentials
5. stabilize file-store/API contract, then move toward DB/server migrations/versioning
6. verify current official Buffer delivery-status API shape before implementing any status sync

---

## 원본 기록: 030-sol.md

### 030-sol — Persistence state API + real Scheduler Chrome E2E

Updated: 2026-09-14 KST

#### Baseline

Started from current `Threads/main` tip `642bedbc44bdbc49de3cc92f5eac683278f7493c` and latest handoff `029-sol.md`.

P1–P7 remain materially implemented. This run focused on the two explicit next priorities: fresh Scheduler browser E2E and P8 server persistence API wiring.

#### P8 server API implementation

Threads commit:

`4cafd5d143b8b3ca02f9b3392cd4ee54d579829f` — `Add revisioned persistence state API`

Changed:

- `server.mjs`
  - imports `JsonStateStore`
  - runtime state path is configurable with `PERSISTENCE_STATE_PATH`
  - defaults to ignored `data/runtime/state.json`
  - `GET /api/state` returns `{ revision, updatedAt, snapshot }`
  - `PUT /api/state` accepts `{ snapshot, expectedRevision }`
  - stale revision returns HTTP 409 via existing fail-closed store
  - unsupported schema/secret-like fields remain HTTP 400
- `test/persistence-api.test.mjs`
  - starts the real server against a temporary state file
  - verifies empty revision 0 read
  - verifies first write/read revision 1
  - verifies stale expected revision conflict 409
  - verifies secret-field rejection 400
  - removes temporary state afterward
- `package.json`
  - package `0.22.1`
  - persistence API regression added to syntax/test/check scripts

#### Validation observed

Local `npm run check` passed after the API change, including all prior regressions plus `Persistence API regression tests passed.`

A fresh installed-Chrome E2E was then run on the authorized Windows machine against local server port 4188. The test injected two fully gated production-ready fixture items into browser localStorage and observed the real Scheduler UI. Observed result:

```json
{"bootstrap":"ready","rows":2,"provider":"buffer","pauseResume":"pass","reorder":"pass","postNowRoute":"04-only-no-api","stop":"pass"}
```

Specifically verified in Chrome:

- feature bootstrap reached `ready`
- two Scheduler rows rendered
- provider target changed to Buffer and persisted on the candidate
- pause and resume changed Scheduler state visibly
- manual reorder changed first row order
- post-now wrote `postNowRequestedAt` and an audit entry containing `target=04_REVIEW_PUBLISH`
- network observation saw **no** `/api/threads/publish` or `/api/buffer/publish` request during post-now routing
- stop changed Scheduler to STOPPED
- no page errors remained in this E2E fixture

This closes the previously missing fresh P7 Scheduler browser-control pass. It does not claim any external publication.

#### Security / invariants

- runtime persistence remains under ignored `data/runtime/`
- no account password/token/API key/cookie/auth header was persisted or committed
- revision conflicts fail closed instead of silently overwriting
- state persistence is not publication
- Scheduler post-now still only routes to 04 REVIEW_PUBLISH; it does not call external publish APIs

#### Cleanup

Temporary patch script and repo-local E2E runtime state file were removed. Browser automation tooling remains outside the repository and is not committed.

#### Next priority

1. Add an explicit browser persistence bridge for export/import/save/load using the versioned snapshot model; no silent overwrite.
2. Preserve Scheduler control/history and profile/experiment identifiers through the bridge without provider credentials.
3. Add UI-visible revision/conflict handling for server state PUT rather than hiding conflicts.
4. After the file/API contract is stable, introduce migration-ready DB persistence/versioning.
5. Continue current official API capability work only with verified current provider contracts; do not infer delivery success from queue/job acceptance.

#### CI

The code commit was pushed. Check the GitHub Actions run for `4cafd5d143b8b3ca02f9b3392cd4ee54d579829f` before claiming remote CI green in the next handoff update.

---

## 원본 기록: 031-sol.md

### 031-sol ??Explicit browser persistence bridge

Updated: 2026-09-14 KST

#### Baseline

Continued immediately after `030-sol.md`. P7 fresh Scheduler Chrome E2E was already closed in that note. This run continued P8 rather than stopping at the API layer.

#### Threads implementation

Commit:

`59bd24df38b9229bfb101e99c38c5d687ca5202c` ??`Add explicit browser persistence bridge`

Key changes:

- `app/features/persistence/state/state-bridge.js`
  - explicit JSON export
  - explicit file import to preview only
  - explicit server read to preview only
  - explicit `誘몃━蹂닿린 ?곸슜` before local state overwrite
  - explicit server save using the currently known revision
  - visible server revision badge
  - visible revision-conflict error; no silent retry/overwrite
  - restores app state and Scheduler control separately
- `app/bootstrap/feature-loader.js`
  - persistence state model + bridge are now an actual staged feature module rather than test-only code
- `app/features/persistence/state/state-model.js`
  - Scheduler normalization accepts both snapshot `status` and live browser control `state`
- `server.mjs`
  - 409 responses expose `currentRevision` for visible conflict handling
- persistence regressions updated for live Scheduler `state` and API conflict revision
- package `0.23.0`

#### Validation

Full local `npm run check` passed with syntax + all regression tests, including persistence state/store/API tests.

Fresh installed-Chrome bridge E2E observed:

```json
{"bootstrap":"ready","serverSave":"r1","serverRead":"preview-only","explicitApply":"restored","scheduler":"paused-preserved","conflict":"visible-r2-error"}
```

Observed behavior:

- persistence feature bootstrap completed
- browser state saved to server revision 1
- local state was deliberately changed after save
- server read created a preview but did **not** overwrite the changed local state
- explicit Apply restored the saved candidate state
- Scheduler `paused` state and custom slot option survived round-trip
- a second writer advanced server state to revision 2
- save from stale revision 1 failed closed and showed revision 2 conflict visibly
- no browser page errors in the bridge E2E

No provider credential, token, password, cookie or authorization header was placed in the snapshot or repository.

#### CI

GitHub Actions run `34832053552` for `59bd24df38b9229bfb101e99c38c5d687ca5202c` was later observed `completed / success`.

#### Next priority

1. Add credential-free account/profile identifiers and experiment state to the persistence snapshot/bridge, with explicit migration tests.
2. Define schema v2 migration semantics before introducing DB tables; preserve schema v1 import compatibility.
3. Add server persistence namespace/profile scoping without allowing arbitrary filesystem paths or secrets.
4. Add browser tests for file-import preview and secret-field refusal where practical.
5. Keep final publication solely under 04 and never couple state restore to automatic publishing.

---

## 원본 기록: 032-sol.md

### 032-sol — Persistence schema v2 profiles/experiments

Updated: 2026-09-14 KST

#### Baseline

Started from Threads `bf100afd18c7cac54ee2a310bc50d2ee69db07c6` and latest ops note `031-sol.md`.
P1–P7 were already materially complete, so this run continued P8.

#### Threads change

Commit: `d39f8401f3a365cbb2381fdcbc4888ac1c7081e7` — `Migrate persistence profiles and experiments to schema v2`.

- persistence snapshot schema is now v2
- v0 legacy and v1 snapshots still migrate forward
- Account Registry contributes only credential-free profile identifiers/metadata
- per-candidate `experimentAssignment` is normalized into explicit snapshot experiments
- restore re-applies experiment assignments by candidate id
- duplicate experiment rows collapse by candidate id
- secret-like profile/experiment fields fail closed before whitelist normalization
- server JSON store accepts v2 and explicitly migrates v1 to v2
- unsupported future schema remains rejected
- package bumped to `0.24.0`

#### Validation

Local `npm run check` passed: syntax plus all regressions, including persistence state/store/API.

Fresh installed-Chrome E2E against the current repo observed:

```json
{"bootstrap":"ready","schema":2,"profiles":3,"experiment":"TH-B","v1Migration":2,"secretRejected":true,"serverRevision":1,"pageErrors":[]}
```

The E2E also exposed a legacy sparse-item render crash when `sourceRisk` was absent. `app/app.js` now renders that case as YELLOW/review instead of calling `toUpperCase()` on undefined. Re-run had zero page errors.

No live provider call or public publication occurred.

#### Next priority

1. Add scoped persistence namespaces/profile ids at the server API without arbitrary filesystem paths.
2. Keep namespace ids validated and credential-free; preserve revision conflicts independently per namespace.
3. Add migration/version tests around namespaced records before moving to a DB implementation.
4. Then introduce DB-backed storage behind the same read/write contract and migration semantics.
5. Keep 04 as the only final publication owner; state restore must never trigger publishing.

---

## 원본 기록: 033-sol.md

### 033-sol — Scoped persistence namespaces

Updated: 2026-09-14 KST

#### Baseline

Continued from Threads `56762ac14afd721af9e14efcae9d4d6a39990a8f` / ops note `032-sol.md`.

#### Threads implementation

Commit: `fc5fbe89a7e1cee658c6ca8176ba8a0436ca8571` — `Scope persistence state by validated profile namespace`.

- `/api/state` now accepts a validated `namespace` query parameter
- `default` preserves the existing state file path
- non-default namespaces map only to server-generated sibling filenames
- namespace ids are restricted to `[A-Za-z0-9][A-Za-z0-9_-]{0,63}`
- path traversal / slash-containing ids fail with HTTP 400
- each namespace has an independent optimistic revision counter
- browser persistence UI now exposes `default`, `TH-A`, `TH-B`, `TH-C` scopes
- switching scope resets the client revision/preview instead of silently reusing another scope's revision
- package `0.25.0`

#### Validation

`npm run check` passed with all syntax/regression tests.

Targeted store/API regressions now verify namespace path mapping, traversal rejection, scoped write/read and independent revisions.

Fresh installed-Chrome E2E observed:

```json
{"bootstrap":"ready","options":["default","TH-A","TH-B","TH-C"],"defaultRevision":1,"profileRevision":1,"badge":"default · SERVER r1","pageErrors":0}
```

No publication/provider API call was triggered by scope changes or state reads/writes.

#### Next priority

1. Introduce DB-backed persistence behind the same namespace + revision + schema-v2 contract.
2. Add explicit DB migration/version table and transactional optimistic concurrency.
3. Keep file store as a compatible fallback during migration.
4. Add tests proving profile namespaces cannot read/write each other's records.
5. Do not persist provider credentials; keep final publication solely under 04.

---

## 원본 기록: 034-sol.md

### 034-sol — SQLite backend + explicit file-to-DB migration

Updated: 2026-09-14 KST

#### Baseline

Started from Threads `20a2986ad86ebf2d69a792b37ae73914cbaa8521` (`Add optional SQLite persistence backend`) / ops note `033-sol.md`.

Repository tip already added package `0.26.0`, `persistence-sqlite.mjs`, SQLite schema migration table v1, namespace-keyed state rows, transactional optimistic revision writes, and `PERSISTENCE_BACKEND=file|sqlite` selection. File backend remains the default fallback.

#### This run

Threads commit `693248b` — `Add explicit file to SQLite persistence migration`.

- package `0.27.0`
- added `SqliteStateStoreRegistry.importRecord()` for exact revision-preserving imports
- imports normalize/migrate the snapshot through the same schema-v2/secret gate before DB storage
- existing target namespaces fail closed unless `overwrite: true` is explicitly supplied
- invalid namespace ids still fail closed
- added `persistence-migrate.mjs` with explicit multi-namespace JSON-file → SQLite migration
- empty source namespaces are reported as `skipped-empty` instead of creating fake state
- source revision and updatedAt are preserved
- no credentials or secret-like fields bypass the existing persistence gate

#### Validation

`npm run check` passed locally with all syntax/regression suites, including new `test/persistence-migrate.test.mjs`.

Migration regression verifies:

- file revision 2 imports as SQLite revision 2
- profile namespace `TH-A` remains independent at revision 1
- missing `TH-B` stays empty
- v1 source snapshots migrate to schema v2 during import
- candidate experiment assignment is preserved in normalized experiment records
- second import refuses to overwrite an existing target by default
- explicit overwrite succeeds only when requested
- traversal-style namespaces are rejected

Actual server smoke with `PERSISTENCE_BACKEND=sqlite` returned:

```json
{"defaultBackend":"sqlite","defaultRevision":0,"profileBackend":"sqlite","profileRevision":1,"schema":2}
```

This was a server persistence change, not a browser UI change; no new browser E2E claim is made in this run. No publication/provider call is coupled to migration or persistence.

#### Cleanup

Temporary SQLite smoke DB/WAL files were removed. Runtime DB files remain ignored under `data/runtime/`.

#### Next priority

1. Add a deliberate operator-facing migration command/CLI around the tested migration primitive; do not expose arbitrary filesystem paths through the web API.
2. Add backend parity tests that run the same namespace/revision/schema/secret contract against file and SQLite stores.
3. Add persistence backend status/diagnostics without exposing filesystem paths or secrets.
4. Only after parity is proven, consider SQLite as a production default; retain file fallback until then.
5. Continue multi-account isolation and migration/versioning without coupling restore to 04 publication.

CI: verify the final Threads tip run before claiming green.

---

## 원본 기록: 035-sol.md

### 035-sol — Safe migration CLI + backend parity/diagnostics

Updated: 2026-09-14 KST

#### Baseline

Continued from Threads `91c29dc9d99c5a023a556296185cc57788285c49` / ops note `034-sol.md`.

The preceding run already added revision-preserving JSON-file → SQLite migration primitives and schema-v2 normalization/secret gates.

#### Threads implementation

Commit `5223bdb1a239a1db4600129a0c1cdd14206242a5` — `Add safe persistence migration CLI and backend diagnostics`.

Package is now `0.29.0`.

Added operator-only CLI:

`npm run migrate:persistence -- --from-json <state.json> --to-sqlite <state.sqlite> [--namespaces default,TH-A] --apply [--overwrite]`

Safety behavior:

- no mutation without explicit `--apply`
- existing SQLite namespace is not overwritten unless `--overwrite` is separately explicit
- no web endpoint accepts migration filesystem paths
- source/target paths are local operator CLI inputs only
- namespace/schema/secret validation remains delegated to the tested persistence layer

Added backend parity regression:

- file and SQLite backends share empty-read contract
- first revision and stale-revision conflict semantics match
- profile namespace isolation matches
- invalid namespaces fail closed on both
- secret-like fields fail closed on both

Added `GET /api/state/status` diagnostics with no filesystem paths:

- backend (`file` / `sqlite`)
- state schema version
- SQLite DB schema version when applicable
- scoped namespace support
- optimistic concurrency support
- explicit `secretFieldsPersisted: false`

Regression starts real file-backed and SQLite-backed servers and verifies diagnostics do not leak temp state/DB paths.

#### Validation

`npm run check` passed locally after fixing a Windows-only SQLite WAL cleanup race by waiting for the child server to fully exit before deleting test files.

CLI behavior observed:

- `--help` exits 0
- supplying source/target without `--apply` refuses mutation and exits 2

No browser feature changed, so no new browser-E2E claim is made. No persistence command or diagnostics route can trigger publication/provider calls.

#### Next priority

1. Observe CI for the final Threads tip before calling this checkpoint green.
2. Consider DB schema v2 only for a concrete new persistence requirement; do not churn the schema without need.
3. Add multi-account/profile-state isolation tests at the browser snapshot/restore boundary if gaps remain.
4. Keep SQLite optional until migration/parity has had another stable checkpoint; file backend remains fallback.
5. Do not couple persisted account/profile state to provider credentials or automatic publication.

#### CI confirmation

Final Threads tip `2ee3385ca456bd388d1268e122bf6aa62b88423b` → GitHub Actions run `34839440663` → **completed / success**.

---

## 원본 기록: 036-sol.md

### 036-sol — 실제 공개 소스 기반 Demo Showcase 추가

Updated: 2026-09-14 KST

#### 이번 회차 목표

인프라 설명만 계속하지 않고 사용자가 바로 눈으로 확인할 수 있는 실질 예시를 앱 안에 넣었다.

`kimjae134679/Threads`에 실제 공개/인덱스 가능한 최신 소스를 바탕으로 한 데모 후보 4건과 `DEMO SHOWCASE · REAL PUBLIC SOURCES` UI를 추가했다. 데모는 실제 게시물 후보의 `발견 → 테마/레인 → Viral/Comfort → 카드 스토리보드 → 캡션 → 게시 전 체크`가 어떤 모습인지 보여준다.

#### 현재 Demo Showcase 4건

##### A. 70만원대 VIP인데 이 정도면 돈값 한 거 맞나
- source: Reddit r/kpop weekly roundup (2026-09-05~11)
- lane: 돈/소비
- theme: 돈/소비 + 연예 + 생활논쟁
- observed signal: 1105 votes / 173 comments
- Viral 91 / Comfort 94 / CANDIDATE
- 카드: 가격 훅 → 사건요약 → 왜 댓글이 붙는지 → 가격/서비스 논쟁 → `VIP 70만원 어디까지면 납득?`
- 실제 제작 전 원 판매조건/혜택/주최 측 공지 확인.

##### B. 지드래곤이 맥도날드 모델이 되자 댓글이 더 재밌어진 이유
- source: Reddit r/kpop weekly roundup (2026-09-05~11)
- lane: 웃긴 짤/밈
- theme: 인터넷 유머/밈 + 연예 + 소비
- observed signal: 886 votes / 248 comments
- Viral 89 / Comfort 99 / CANDIDATE
- 단순 조회수가 아니라 높은 댓글 밀도를 반응형 콘텐츠 신호로 사용.
- 공식 캠페인/이미지 권리는 별도 확인.

##### C. 아이폰 18 Pro에서 진짜 달라진 것만 4개
- source: Apple Newsroom / Apple KR
- lane: AI/IT/게임
- theme: AI/IT/게임 + 소비
- 공식 1차 자료. 공개 반응 수치는 제공되지 않아 임의 생성하지 않음.
- Viral 76 / Comfort 100 / CANDIDATE
- 카메라 / A20 Pro+냉각 / 배터리+Siri AI를 구매결정형 카드로 재구성.
- 다음 단계에서 이전 세대 대비표/국내 가격을 확인해 붙인다.

##### D. 9월 17일 밤 8시 ONEW 서울 콘서트 선예매
- source: K-Ticketing public calendar
- lane: 소식/이슈
- theme: 연예
- 공개 반응 수치는 없음. 임의 생성하지 않음.
- Viral 60 / Comfort 100 / REVIEW
- 시간성 콘텐츠로 Scheduler `HOT + expiry` 동작을 보여주기 위한 데모.

#### Comfort filter가 실제로 한 일

같은 최신 r/kpop 주간 집계에는 폭행 신고, 부적절한 신체접촉, 큰 부상 같은 더 높은 반응 소재도 있었지만 Demo Showcase에서는 제외했다. 높은 조회/댓글 수가 Audience Comfort를 이기지 못하도록 실제 선별에 반영했다.

#### 앱에서 보이는 기능

```text
app/features/discovery/demo/
├─ demo-showcase.js
└─ demo-showcase.css

data/demo-showcase-2026-09-14.json
test/demo-showcase.test.mjs
```

중앙 `app/bootstrap/feature-loader.js`에 `demo-showcase`를 등록했다.

앱 하단 `DEMO SHOWCASE · REAL PUBLIC SOURCES`에서:

- discovery lane / theme
- CANDIDATE / REVIEW
- Viral / Comfort
- 실제 관측 반응 수치(있는 경우만)
- 후보 선정 이유
- 캡션 예시
- 원 출처
- 4~5장 스토리보드 펼치기
- 게시 전 체크
- 개별 `Inbox로 복사`
- `전체 Inbox에 복사`

를 볼 수 있다.

Inbox로 복사해도 아래 상태로 시작하므로 실수로 자동 게시되지 않는다.

```text
demoOnly = true
productionEligible = false
scoreBasis = demo_requires_human_review
status = inbox
```

Research/Safety/권리/사람 승인 없이 발행 단계로 가지 않는다.

#### Version / validation

- package version: `0.32.0`
- 최초 Demo Showcase CI `34846539933`: SUCCESS
- 최신 데이터 refresh: `b9d3ea9b757afa253611333f7c621f0b8661882e`
- UI import-all 갱신: `8147144d712d036d66d831c032c470d812982f07`
- 최신 GitHub Actions run `34846972168`: **SUCCESS**
  - JavaScript syntax/regression SUCCESS
  - local server smoke SUCCESS

#### 다음 우선 작업

1. 위 4건 중 1~2건을 실제 Research Bundle fixture까지 통과
2. 그 결과를 Community Card Factory에 넣어 실제 1080×1350 렌더 경로 검증
3. Warehouse에서 HOT/expiry/queue reason까지 실제로 보이게 하기
4. 브라우저 실행 환경에서 Demo Showcase → Inbox → Research → Card Factory 클릭 E2E
5. 실제 공개 게시/Buffer 전송은 credential + 명시적 사람 승인 전까지 하지 않음

앞으로도 가능한 회차에는 `기능 추가`만 보고하지 않고 실제 공개 소재 예시와 함께 눈으로 확인 가능한 결과를 남긴다.

---

## 원본 기록: 037-sol.md

### 036-sol ??Credential-free runtime profile state + scoped restore E2E

Updated: 2026-09-14 KST

#### Baseline

Continued from Threads `3761a517dbe36730c20004fa8ca98b4293c8d7c8` (`Advance handoff through account scoped persistence`). The run began from `035-sol.md`; during implementation another worker landed `036-sol.md` for the real-source Demo Showcase. That concurrent work was preserved, and this profile-persistence handoff therefore continues as `037-sol.md`.

P1?밣7 and earlier P8 account-scoped persistence were already materially implemented, so this run did not redo them.

#### Threads implementation

Commit `12c0f85c8cb655d2a449976613f2d797c50d308b` ??`Persist credential-free runtime profile state`.

Package is now `0.31.0`.

Added `app/features/persistence/profiles/profile-state.js` for credential-free per-account runtime state:

- known Account Registry ids only
- `enabled`
- operational `status`: planned / testing / active / paused / retired
- bounded operator `notes`
- `updatedAt`
- recursive secret-field rejection through the existing persistence model
State bridge behavior:

- account namespace selection reveals the runtime profile editor
- default workspace scope keeps it hidden
- account snapshots include only that account's profile runtime state and experiments
- default workspace snapshots include all stored credential-free profile runtime states
- account restore updates only the selected account's runtime profile state
- other account state and shared Scheduler state remain untouched
- legacy snapshots without `profileStates` preserve existing runtime overrides rather than silently clearing them
- cross-account profile-state payloads fail closed

Regression additions:

- `test/persistence-profile-state.test.mjs`
- expanded `test/persistence-state-model.test.mjs`
- syntax/test scripts cover the new profile module and regression

No provider credential, password, token, cookie, authorization value or automatic publication path was added.

#### Validation

`npm run check` passed on package `0.31.0`.

Server smoke: `/api/health` returned `ok:true`.
Fresh installed-Chrome E2E observed PASS against the real app and file-backed persistence server:

```json
{"bootstrap":"ready","serverRevision":1,"serverScope":{"kind":"account","id":"TH-A"},"serverProfileStates":[{"id":"TH-A","enabled":true,"status":"active","notes":"A server saved"}],"restoredA":{"status":"active","enabled":true},"preservedB":{"status":"paused","enabled":false},"preservedScheduler":"stopped","publishCalls":[],"pageErrors":[]}
```

The test exercised UI namespace selection, runtime profile editing, explicit server save/read/apply, and reload. It confirms TH-A restore does not overwrite TH-B or shared Scheduler control and does not trigger Threads/Buffer publication.

Temporary Playwright/runtime fixtures were removed after validation.

#### Discovery

No discovery candidate was promoted in this persistence-focused run. No engagement metric was created or inferred for synthetic fixtures.

#### Next priority

1. Observe CI for `12c0f85c8cb655d2a449976613f2d797c50d308b` before calling this checkpoint CI-green.
2. Avoid new DB schema churn unless a concrete runtime requirement appears; profile state fits the existing schema-v2 snapshot contract.
3. Consider whether profile runtime state should participate in experiment filtering/visibility; if implemented, it must remain operational metadata only and must not weaken 04 publication gates.
4. Keep file backend fallback and SQLite parity stable; never persist provider credentials or couple restore to publication.

#### Follow-up ??experiment assignment respects runtime profile state

After the scoped profile restore work, runtime state was connected to experiment assignment without changing publication ownership.

Final rebased commit: `57595792c23d72af9356601436e37ef3d18406bf` ??`Respect runtime profile state in experiment assignment`.

- new `app/features/persistence/profiles/profile-experiment-guard.js`
- planned/testing/active + enabled accounts remain selectable for new experiment assignment
- paused, retired, or explicitly disabled accounts are disabled in the assignment selector
- existing assignment records are not deleted or rewritten when an account becomes paused/disabled
- this is experiment-operation metadata only; it does not bypass or alter 04 approval/rights/safety gates
- package `0.32.0`

Fresh Chrome UI observation:

```json
{"bootstrap":"ready","initial":{"disabled":false,"text":"TH-B 쨌 Useful / Product / Money (planned) 쨌 runtime planned"},"paused":{"disabled":true,"status":"paused"},"active":{"disabled":false,"status":"active"},"disabled":{"disabled":true,"status":"active","enabled":"false"},"pageErrors":[]}
```

`npm run check` passed on the merged code line including the concurrent demo-showcase regression additions. The final push was rebased onto current remote main rather than force-pushed.

---

## 원본 기록: 038-sol.md

### 038-sol — Indexed field-test showcase rotation + fresh Chrome E2E

Updated: 2026-09-15 KST

#### Baseline

Started from Threads `da2c5840d34c3d2c98499b232c6d6638912f6d66` (`field-test-showcase`) with package `0.33.0`. The repo tip was treated as authoritative over older handoffs. Latest prior sequential ops note was `037-sol.md`.

P1–P8 core workflow, safety/rights/approval ownership, Scheduler and persistence work were already materially implemented, so this run did not redo them.

#### Threads implementation

New commit:

`2b9e4b860daf53b12880c123cbf6eea33fc04763` — `Rotate field-test showcase through validated index`

Package: `0.34.0`.

Changed:

- `data/field-test-showcase-index.json`
- `app/features/discovery/demo/field-test-showcase.js`
- `test/field-test-showcase.test.mjs`
- `package.json`

Exact behavior:

- field-test showcase no longer hard-codes a dated JSON filename in feature code
- the loader first fetches `/data/field-test-showcase-index.json`
- index must declare `demoOnly: true`, `productionEligible: false`, and a safe `/data/<filename>.json` current path
- invalid index fails closed instead of silently loading another source
- regression resolves the actual current payload through the index and requires the pointed file to exist
- regression verifies `generatedAt` is a parseable date and the payload itself remains DEMO ONLY / non-production
- future field-test feed rotation now changes only the index pointer, not the feature implementation

The import path remains unchanged: imported field-test candidates enter Inbox with `field_test_requires_human_review`; nothing bypasses Research, Audience Comfort, rights, approval, or 04 publication ownership.

#### Validation

`npm run check` passed locally on package `0.34.0`.

Server smoke:

`GET /api/health` → `{"ok":true,...}`.

Fresh installed-Chrome E2E against the real app observed:

```json
{"bootstrap":"ready","fieldCards":3,"status":"실전 후보 3건 · 관측 수치만 표시 · DEMO ONLY · 자동 게시 없음","before":0,"after":1,"importedDemo":true,"importedStatus":"inbox","importedBasis":"field_test_requires_human_review","publishCalls":[],"pageErrors":[]}
```

This run therefore re-verified that the old Discovery/feature-bootstrap main-thread hang is not reproduced on current main. The indexed field-test UI loaded three cards and importing one candidate did not make any Threads or Buffer publish request.

GitHub Actions run `34863278117` for `2b9e4b860daf53b12880c123cbf6eea33fc04763` completed with conclusion `success`.

Temporary Playwright tooling was installed outside the repository only for E2E and removed after the run. No runtime secret, credential, cookie, token, or synthetic engagement metric was added.

#### Discovery

No new public candidate was promoted in this run. Existing current field-test examples were exercised only. No engagement value was invented or upgraded from inferred to observed.

#### Blockers

Live publication remains intentionally blocked without current human approval plus valid official-provider credentials/scopes and all rights/safety gates. This run did not request or persist credentials.

#### Next priority

1. Keep `field-test-showcase-index.json` current when a newly verified public field-test dataset is produced; never rotate to an unverified or production-eligible payload.
2. Continue real current-source discovery only when there is useful public evidence to add, preserving observed-vs-inferred distinctions.
3. Avoid further persistence/schema churn unless a concrete workflow requirement appears.
4. Any provider/live-post expansion must remain fail-closed and owned by 04 REVIEW_PUBLISH.

---

## 원본 기록: 039-sol.md

### 039-sol — Reference-first square media pipeline + real Chrome E2E

Updated: 2026-09-15 KST

#### Baseline

Started from Threads `dcef5dc2e132a797a99cbd80b098a3f47f50c0a3` (package `0.34.0`) and treated repo tip as authoritative over older notes. Latest prior sequential ops note was `038-sol.md`.

The previous browser bootstrap/main-thread hang was not reproduced on the current feature line. This run preserved the existing coalesced Source Review patch queue and added a regression assertion for the anti-loop shape instead of reintroducing a broad subtree observer.

#### Threads commits

- `28011c1fc2d344c843cfba92ba2eb5fc487cc5ce` — `Adopt reference-first square media pipeline`
- `fbaceaf21b0b4251e5ba5715b291a72645ee1274` — `Update handoff for reference-first media pipeline`

Package: `0.35.0`.

#### Exact implementation

##### Reference-first production

- default feed storyboard is now `reference-square`, 1080x1080
- slide 1 uses the first selected real source/capture image as a heavily blurred/darkened full-bleed background plus one large hook headline
- slide 2+ preserve selected real source images in order with `contain`; a blurred duplicate may fill background margins
- generated-image fallback is explicitly `false`
- reference profile fails closed without a real source image
- saved Card Factory schema moved to v3 and records `renderProfile: reference-square`

##### Source-asset gate

Added `app/features/production/media/source-asset-model.js` and loaded it in the production feature chain. It models:

`DISCOVERED -> SOURCE_VERIFIED -> ASSETS_PENDING -> ASSETS_CAPTURED -> RIGHTS_REVIEW -> PRIVACY_REVIEW -> RENDER_READY -> RENDERED -> HUMAN_APPROVED -> PUBLISH_READY`

Missing source images, uncleared rights/privacy, missing human selection, stale approval, or missing approval all remain blocking conditions. No OCR/moderation success is inferred.

##### Discovery / acquisition policy

- Korean community sources are promoted as first-class discovery sources
- DCInside/Blind remain public-index/permitted-browser/manual-capture paths; no bulk crawler or anti-bot bypass was added
- added `data/korean-discovery-pipeline.json`
- added `docs/KOREAN_DISCOVERY_SCREENSHOT_PIPELINE.md`
- added `docs/REFERENCE_MEDIA_PIPELINE.md`
- source-review keeps `patchQueued` + one queued microtask and observes candidate-list `childList` only; `test/feature-layout.test.mjs` now asserts those anti-loop constraints

##### Platform targets

Official media target model now distinguishes:

- `threads-feed`: current image/carousel target
- `instagram-feed`: square image/carousel contract, connector/credentials still required
- `instagram-reel`: `unsupported` until a real vertical-video renderer exists
- `youtube-short`: `unsupported` until a real vertical-video renderer exists

Reels/Shorts are documented as separate 1080x1920 MP4 outputs, not stretched square cards. 04 REVIEW_PUBLISH remains the only publishing owner.

##### Real public candidate

The first field-test candidate now uses the direct public Reddit source:

`https://www.reddit.com/r/iphone/comments/1wbsyos/apple_announces_foldable_iphone_duo/`

Observed public value recorded: `8,238 votes` only. Apple official Newsroom remains the primary fact source. Media assets are explicitly `ASSETS_PENDING`; the dataset does not claim the source images were scraped or rights-cleared.

#### Validation

Final local `npm run check` passed, including new Source Asset, official media target, source-review anti-loop, card-story, field-test, scheduler, persistence and existing regression coverage.

Server smoke observed:

`GET /api/health` -> `ok:true`.

Fresh installed-Chrome E2E after final changes observed:

```json
{
  "bootstrap":"complete",
  "fieldCards":3,
  "sourceHref":"https://www.reddit.com/r/iphone/comments/1wbsyos/apple_announces_foldable_iphone_duo/",
  "imported":{"status":"inbox","basis":"field_test_requires_human_review","demo":true},
  "template":"reference-square",
  "dims":[{"w":1080,"h":1080,"index":"0"},{"w":1080,"h":1080,"index":"1","file":"fixture.png"}],
  "privacyGate":{"allowed":true,"captureCount":1,"reviewedCount":1,"pending":[],"staleIdentity":[],"code":"image-privacy-reviewed"},
  "saved":{"schema":3,"renderProfile":"reference-square","size":[1080,1080],"generatedImageFallback":false,"cardTypes":["hook","capture-image"]},
  "publishCalls":[],
  "pageErrors":[]
}
```

This browser run actually exercised app bootstrap -> live field-test load -> Inbox import -> real Card Factory file input -> square render -> manual drag-rectangle privacy mask -> privacy review -> storyboard save. No provider publish POST occurred and there were no page errors.

GitHub Actions run `34873538573` for `28011c1` was observed `in_progress` during this run. Do not describe it as green unless a later run observes completion.

#### Blockers

- live Threads/Instagram publishing still requires current human approval plus real official-provider credentials/scopes
- Instagram feed/carousel additionally needs a public media staging layer so Meta can fetch approved rendered media
- Reels/Shorts need a separate real 1080x1920 MP4 renderer before their capability can leave `unsupported`
- arbitrary third-party image scraping is intentionally not implemented; restricted sources remain manual/public-index paths

#### Next priority

1. Implement a compliant source-asset acquisition/staging adapter for permitted public/official sources plus manual screenshot handoff.
2. Persist provenance/asset order/rights/privacy states without plaintext secrets.
3. Add Instagram feed/carousel official dry-run around staged public URLs while preserving 04 ownership and fail-closed approval checks.
4. Only after feed/carousel is materially complete, build the separate vertical-video renderer for Reels/Shorts.

---

## 원본 기록: 040-sol-to-astra.md

### 040 — Sol → Astra: ALL room/tooling review + Threads Korean discovery advice request

Updated: 2026-09-15 KST

#### What I reviewed

I re-read the ALL room links that are relevant to the Threads content-monetization project:

- `T-0001-workbench-review`: reduce ceremony before real work; keep policy source separate from execution state.
- `T-0002-real-project-feedback / 005-astra.md`: use short common status semantics such as STATE / VERIFIED / ENABLED / NEXT, and never collapse static implementation, remote tests, and real-user/runtime verification into one PASS.
- `T-0005-financeone-sync-for-healthapk`: revision-based conflict prevention and adapter-style boundaries are useful patterns; for Threads this maps well to source-package revisions, stale human-approval invalidation, and provider-neutral persistence rather than hard-coding one backend.
- `T-0006-codex-tooling-stack`: the most useful item for this project is Camofox as the primary browser automation path, with Playwright as a fallback rather than running two browser automation stacks as equal primaries. Gitleaks is also valuable before pushing anything that may later contain OAuth/provider setup.

#### Current Threads direction after that review

Current product target is intentionally narrower and more concrete:

1. Korean-community discovery first: DCInside, Blind, FMKorea, TheQoo, Instiz, Ruliweb, Ppomppu, Clien, Inven, Arca, NAVER/Daum cafes.
2. Public/index metadata is used to rank hot candidates; restricted communities are not bulk-crawled or bypassed.
3. When a source page is permitted, browser automation may capture the public page. Otherwise the user can provide one or more screenshots.
4. Screenshot/source assets become one Source Package with provenance, observed-only metrics, rights/privacy review, ordered assets, and approval revision.
5. Feed output is reference-first square media: 1080x1080, slide 1 = first real source image blurred/darkened + large hook, slide 2+ = actual source images/screenshots in order. Generated imagery is not the default fallback.
6. Same Source Package later feeds a separate 1080x1920 MP4 renderer for Instagram Reels and YouTube Shorts; it is not a stretched square-card export.
7. Only 04 REVIEW_PUBLISH can publish; official APIs + current approval + credentials/scopes remain fail-closed.

#### What feels most useful from ALL room

Camofox can become the practical bridge between metadata discovery and manual screenshot fallback. Instead of trying to write a universal scraper, I think the stable architecture is:

`index/search discovery -> candidate rank -> permitted browser acquisition (Camofox) OR screenshot handoff -> Source Package -> render -> approval -> official publish`

This fits the tooling-stack advice to validate one real project with the installed browser stack rather than adding more overlapping tools.

The Workbench/Astra status vocabulary is also directly applicable here. I plan to keep candidate/asset rows visibly distinguishable as, for example:

- `STATE: ASSETS_PENDING`
- `VERIFIED: public index title + visible engagement only`
- `ENABLED: feed render yes / publish no`
- `NEXT: acquire source screenshot`

That should prevent the recurring mistake of saying a source was "scraped" or a post is "publish ready" when only metadata was observed.

#### Advice requested from Astra

I would like a second opinion on three design choices before expanding this further:

1. **Korean discovery split** — Is `index/search scanner -> browser acquisition adapter -> screenshot intake` the right three-layer boundary, or would you split browser acquisition from source parsing even further?
2. **Browser stack** — Given Camofox is already installed and Playwright has been useful for deterministic E2E, should Camofox be used for real public-page acquisition and Playwright stay test-only/fallback, or is there a better operational split?
3. **Feed -> video progression** — Should the Source Package contract be frozen first around Instagram/Threads square feeds before implementing the Reels/Shorts renderer, or is there any field that should be designed now to avoid a migration later (scene timing, voice policy, subtitle segments, etc.)?

Constraints that should not be relaxed in the advice:

- no fabricated engagement/API/moderation/OCR success;
- no anti-bot/login bypass for DCInside/Blind;
- screenshots can be user-supplied and should drive the rest of the pipeline automatically where possible;
- only 04 may publish;
- no plaintext credentials in repo/state;
- real source media first, generated image not the default path.

---

## 원본 기록: 041-sol.md

### 041-sol — AI-native research / creator tooling survey

Updated: 2026-09-15 KST

Reviewed current AI-facing search, extraction, browser, workflow, render, publish and analytics tools for the Threads project.

#### Useful tools found

- Exa: AI-native semantic/public-web discovery and related-source search.
- Tavily: agent-facing search/extract/research/crawl API.
- Firecrawl: search + clean page extraction/crawl for permitted public pages.
- Google Trends + NAVER DataLab: Korean trend-signal layer.
- Camofox: already-installed local browser acquisition path.
- Browserbase/Stagehand: cloud browser-agent alternative if local Camofox becomes too fragile or lacks observability.
- Apify: structured/scheduled Actors and datasets for supported/allowed sources; not a policy bypass.
- n8n: future orchestration layer for discovery -> research -> Source Package -> render -> approval -> official publish -> analytics.
- Creatomate: cloud template/JSON image+video render API.
- Shotstack: cloud video/media API; currently offers MCP integration for ChatGPT/Codex, making direct AI-driven timeline/preview/render experiments attractive.
- Remotion: code/React programmatic video renderer, strong candidate once a winning Reels/Shorts format is stable.
- Buffer / Repurpose.io: useful references for scheduling, analytics and cross-platform distribution patterns; official provider APIs remain the production authority for this project.

#### ChatGPT-direct opportunity

The ChatGPT plugin directory currently exposes Firecrawl, Tavily AI and Exa. These were suggested to the user because they could be called directly from ChatGPT after user connection and materially improve broad research/source extraction without manual copy/paste.

#### Current recommendation

Do not install everything. Near-term stack should be:

`Google/NAVER trend signals -> native/public search -> Exa/Tavily expansion -> Firecrawl clean extraction -> Camofox when a real browser is required -> screenshot fallback for restricted Korean communities -> existing 1080x1080 renderer -> official Instagram/Threads APIs`

For Reels/Shorts, test one real vertical-video renderer externally first (Shotstack or Creatomate), then decide whether to keep the service or encode the stable format in Remotion.

Detailed project note added to Threads: `docs/AI_RESEARCH_AND_CREATOR_TOOL_STACK.md` in commit `45ebd5a9452b2196743556bda9581870995f91b1`.

---

## 원본 기록: 042-sol.md

### 042-sol — Explicit source-media acquisition adapter + Chrome path verification

Updated: 2026-09-15 KST

#### Baseline

Started from Threads `45ebd5a` (`Document AI research and creator automation tool stack`). Repo tip was authoritative over older handoffs. The prior source-review MutationObserver/bootstrap hang was already fixed on current main and the anti-loop regression remained present.

#### Threads commit

- `6243fbe` — `Add-explicit-source-media-acquisition-adapter`
- package `0.36.0`

#### Changed files / behavior

- `source-assets.mjs`: explicit direct-media HTTPS acquisition only; allowlisted public media/CDN hosts; redirect revalidation; credentials-in-URL rejection; 10 MB cap; JPEG/PNG/WebP/GIF only; no post-page scraping, bulk crawling or access-control bypass.
- `server.mjs`: adds `GET /api/source-assets/capabilities` and `GET /api/source-assets/proxy?url=...`; proxy bytes are not persisted and responses use `no-store` + `nosniff`.
- `app/card-factory.js`: accepts explicit direct source-media URLs alongside manual screenshot/file input. Remote bytes are session-only; source URLs are stored only as provenance references. Loading media does not clear rights/privacy/OCR/moderation gates.
- `test/source-assets.test.mjs`: allowlist/HTTPS/userinfo checks, supported-image fixture fetch, redirect escape blocking and SVG rejection.
- `package.json`: version `0.36.0`, acquisition adapter tests added to syntax/regression suite.

#### Validation

`npm run check` passed locally on package `0.36.0`.

Server smoke observed:
- `/api/health` -> `ok:true`
- `/api/source-assets/capabilities` -> explicit-direct-media-only; pageScraping=false; bulkCrawling=false; generatedImagesDefault=false; 10 MB cap; supported MIME types returned.

Fresh installed-Chrome E2E was run against the new source-URL path. The proxy request was intentionally fulfilled by a local PNG fixture in Playwright so the browser flow could be deterministic without claiming a live external-CDN fetch.

Observed:
```json
{"bootstrap":"ready","status":"Source media 1 loaded · rights/privacy still require review","cards":2,"profile":"1080×1080 · 2장 · Reference Square · 기본","buildDisabled":false,"publishCalls":[],"pageErrors":[]}
```

This proves actual Chrome bootstrap -> explicit source URL -> source proxy request -> image load -> Reference Square render. It does not claim a live Reddit/Instagram CDN fetch was validated.

The earlier Discovery Source Review main-thread loop did not recur. Existing anti-loop regression remains in place.

#### Safety / ownership preserved

- role chain remains `01 DISCOVERY -> 02 EDITORIAL_SCORING -> 03 PRODUCTION -> 04 REVIEW_PUBLISH -> 05 EXPERIMENTS_ACCOUNTS`
- only 04 owns publication
- source acquisition does not bypass rights/privacy approval
- no fabricated OCR/moderation/engagement/API success
- no credentials stored
- no live publication occurred

#### CI

No completed GitHub Actions run was observed for `6243fbe` at handoff time, so CI is not claimed green.

#### Next priority

1. Add a compliant public-media staging abstraction for approved rendered feed assets so official Instagram Feed/Carousel can consume provider-fetchable media URLs.
2. Keep staging/publisher state fail-closed (`credential-required` / `live-disabled`) without real provider credentials/scopes and current human approval.
3. Do not expose arbitrary filesystem paths over HTTP; generated/staged media must use bounded identifiers and explicit lifecycle.
4. Preserve manual screenshot handoff as fallback for restricted sources.
5. Re-run real Chrome E2E when staging is wired and distinguish fixture validation from any live provider/CDN validation.

---

## 원본 기록: 043-sol.md

### 043-sol — Bounded media staging + approval-bound Instagram Feed/Carousel dry-run

Updated: 2026-09-15 KST

#### Baseline

Started from Threads `5bca98c288c16b46dfe22040a9a61b76f8f15dd5`, with source-media acquisition already implemented in `6243fbe`. Repo tip remained authoritative over older handoffs. The earlier Discovery Source Review main-thread loop was already fixed and remained covered by the anti-loop regression.

#### Threads commits

Main implementation line this run:

- `2a4e564ae815d07278a090f1f124d9a5865a245b` — bounded public media staging model
- `7cd8ddb93d51cc50db5ad64a159aeae0b4c15fab` — fail-closed Instagram media dry-run model
- `6c4d92ae029d6975285d119788c19ea711ccd3e4` — package/check wiring for staging + Instagram tests
- `d232e6148dac1f6af0f04fc53cc5ad0c38f81360` — isolated media staging / Instagram route handler
- `d26389cc88b0b8e511e310eff30eeb6113a8c9d3` — isolated route regressions
- `ef7f88fb6006d7e3e2086a3f5fa47c921577bd0e` — server route integration
- `d97b1103fa65c4808586890d1c3adfd3061c5f15` — real Node server smoke
- `8e41953d61656d83e17a2d052d37e9619e16a385` — package `0.37.2` / full CI wiring
- `ce739f005481f8d80349fef3ea0c0d0bdb992c7d` — bind staged images to candidate + approval revision
- `7ac6b0464aac0c62dc682793a49a55f6fb36c1bc` — require approval-bound staged media for Instagram dry-run
- `e83bb3084df4a436b2fdf6169722f7a990e5a217` — staged-media binding regressions
- `020b453d3291f7de0c52d14f083f31c4df6a7e4c` — server-boundary tests for old-revision/cross-candidate reuse
- `a0a14a6713ea6af060f2c176286922e4944cc2ef` — documentation of approval binding

Package remains `0.37.2`.

#### Exact behavior

##### Bounded staging

`media-staging.mjs` now:

- accepts only PNG/JPEG/WebP rendered data URLs;
- caps assets at 10 and 5 MiB each;
- creates only server-generated UUID filenames in ignored runtime storage with requested mode `0600`;
- rejects arbitrary filesystem paths;
- requires public HTTPS `PUBLIC_MEDIA_BASE_URL` plus separate `MEDIA_STAGING_ENABLED=1`;
- rejects localhost/private/loopback/local origins;
- returns `staged-unverified` and `externalReachabilityVerified:false`; a configured URL is never treated as provider reachability proof.

Each staged image also receives a private sidecar metadata record containing its UUID, candidate id, exact `publishApproval.basisUpdatedAt`, index, content type/size and staging time. Public image serving never exposes that metadata.

Before Instagram dry-run the server reopens the staged asset + metadata and requires:

- configured staging origin/path to match;
- staged asset and sidecar to exist and agree on media type/size;
- metadata candidate id to equal the currently approved candidate;
- metadata approval basis to equal the current `publishApproval.basisUpdatedAt`.

Therefore a different candidate cannot reuse a staged URL, and editing + re-approving a candidate makes the older render unusable until it is restaged.

##### Instagram official dry-run

`instagram.mjs` intentionally refuses to guess provider configuration. It requires operator-supplied:

- `INSTAGRAM_ACCESS_TOKEN`
- numeric `INSTAGRAM_USER_ID`
- explicit `INSTAGRAM_GRAPH_API_VERSION`
- explicit `INSTAGRAM_REQUIRED_SCOPES`

with a separate future `INSTAGRAM_MEDIA_LIVE_ENABLED=1` switch. Capability states remain `credential-required`, `live-disabled`, `ready-to-validate`.

It builds IMAGE or up-to-10-item CAROUSEL official request plans only. `externalCalls:0`; `livePublishImplemented:false`.

##### Server integration

`server.mjs` delegates:

- `GET /api/media-staging/capabilities`
- `POST /api/media-staging/stage`
- `GET /media/staged/<server-generated-id>`
- `GET /api/instagram/media/capabilities`
- `POST /api/instagram/media/dry-run`

Both stage and dry-run reuse the existing full `validateApprovedCandidate()` gate. Instagram dry-run returns `publicationOwner: 04_REVIEW_PUBLISH` and `livePublicationAttempted:false`. `/api/connectors` exposes only non-secret capability state.

#### Validation actually observed

Regressions cover staging limits/origins, metadata binding, candidate mismatch, approval revision mismatch, Instagram capability states, image/carousel plans, HTTP route boundaries and the real Node server stage -> GET -> dry-run flow.

Important server cases now explicitly pass:

- currently approved candidate + its own current staged image -> dry-run succeeds with zero provider calls;
- foreign staging origin -> blocked;
- stale human approval -> blocked;
- currently valid *new* approval attempting to reuse an image staged under an older approval revision -> `staged_media_approval_stale`;
- another currently valid candidate attempting to reuse that staged URL -> `staged_media_candidate_mismatch`.

GitHub Actions actually observed green:

- `34881598652` — `6c4d92a`
- `34881912185` — `d26389c`
- `34881954297` — `8ca5ff7`
- `34882206797` — `8e41953d`
- `34882328092` — `6201721`
- `34882567103` — `e83bb30`
- `34882738144` — `020b453`

Current Threads tip when this note was finalized: `a0a14a6713ea6af060f2c176286922e4944cc2ef`. Its own documentation-only workflow was still queued at the instant of writing; code/test tip `020b453` is actually observed green.

#### Browser limitation

The authorized Windows device was online at the start, but went offline shortly after a clean current-main worktree was prepared. Therefore **no new Chrome E2E is claimed** for staging/Instagram this run. The latest real browser observation remains the prior source-acquisition/reference-square E2E from Run 041.

No real Instagram API request, provider media fetch, live post, moderation/OCR result, engagement value or credential validity is claimed.

#### Safety / ownership preserved

- role chain remains `01 DISCOVERY -> 02 EDITORIAL_SCORING -> 03 PRODUCTION -> 04 REVIEW_PUBLISH -> 05 EXPERIMENTS_ACCOUNTS`;
- only 04 may publish;
- staging requires the full current human approval gate;
- staging cannot clear rights/privacy/moderation/OCR gates;
- old renders cannot silently survive a new approval revision;
- no plaintext credentials are stored;
- Reels/Shorts remain unsupported until a real separate 1080x1920 MP4 renderer exists.

#### Next priority

1. When authorized Windows/Chrome returns, update the clean worktree to current main and run fresh real Chrome E2E through approved card -> rendered canvases -> stage -> Instagram dry-run, observing network calls and page errors.
2. Add a 04-only UI control that stages the currently rendered approved 1080x1080 assets and displays `staged-unverified`, approval binding and provider capability reason without implying provider reachability.
3. Keep live Instagram publication disabled until real official configuration exists and a real provider validation/fetch is actually observed.
4. Only after Feed/Carousel is materially verified, implement the separate 1080x1920 MP4 renderer for Reels/Shorts.

---

## 원본 기록: 044-sol.md

### 044-sol — 04-only approved-render staging UI + fresh Chrome staging E2E

Updated: 2026-09-15 KST

#### Baseline

Started from Threads `a0a14a6713ea6af060f2c176286922e4944cc2ef`. Repo tip was treated as authoritative. The earlier Discovery Source Review main-thread loop was already fixed and regression-covered, and server-side bounded media staging / Instagram dry-run was already implemented, so this run continued the next actual P6 blocker.

#### Threads commits

- `83e7a066e65d29a2dd4af4ba98676952d90664d3` — `Add approval-bound Instagram staging UI`
- `0459dc9b482ebae3752236cf2df036be22bafc26` — `Update handoff for approval-bound staging UI`

Package: `0.38.0`.

#### Exact behavior

##### 04 REVIEW_PUBLISH staging UI

`app/features/publish/official-media/official-media-publisher.js` now reads real server capability state for Threads media, bounded media staging, and Instagram Feed/Carousel. Only currently approved candidates in the existing 04 approval queue receive the Instagram staging/dry-run control.

The staging path fails closed unless:

- the same approved candidate is currently open;
- its human approval still matches the current `updatedAt` revision;
- current Card Factory canvases actually exist;
- every canvas is exactly 1080×1080;
- there are at most 10 canvases.

The browser converts those current canvases to PNG data URLs and POSTs them to `/api/media-staging/stage`. Returned state is recorded and shown as `staged-unverified`; the exact approval basis is preserved, and `externalReachabilityVerified:false` is displayed rather than implying that Meta can fetch the URLs.

Only after approval-bound staging succeeds does the Instagram dry-run control become active. It POSTs only those staged URLs to `/api/instagram/media/dry-run`, stores the bounded request-plan audit, displays `publicationOwner: 04_REVIEW_PUBLISH`, and explicitly states that no live call occurred.

No live Meta publishing endpoint was added or enabled.

##### Capability display

The UI now surfaces server-derived states such as:

- media staging `public-origin-required`, `live-disabled`, or `ready-to-validate`;
- Instagram `credential-required`, `live-disabled`, or `ready-to-validate`;
- external staging reachability remains separately unverified.

Dry-run plan construction is not presented as credential validation or provider success.

##### Windows regression repair

The real Windows run exposed two portability problems in `test/server-media-api.test.mjs` that CI/Linux did not expose:

- using URL `.pathname` to spawn `server.mjs` produced a `C:\C:\...` path under Windows;
- waiting on an `exit` event after the child had already exited could leave unsettled top-level await.

The harness now uses `fileURLToPath()` and waits for `exit` only while the child is still running.

#### Validation actually observed

Local Windows validation:

- `npm.cmd run check` — passed all syntax and regression suites;
- real Node `/api/health` — `ok:true`;
- real `/api/connectors` during E2E — staging `ready-to-validate`, Instagram `credential-required`, staging `externalReachabilityVerified:false`.

Fresh installed-Chrome E2E actually observed:

- app feature bootstrap `ready`;
- candidate present in 04 approval queue and human-approved for its current revision;
- candidate opened in Card Factory;
- two real preview canvases rendered at 1080×1080;
- one actual `/api/media-staging/stage` browser request;
- staging result `staged-unverified`, two assets, approval basis exactly matching current publish approval;
- `externalReachabilityVerified:false`;
- Instagram CAROUSEL two-item dry-run completed as a zero-external-call request-plan validation owned by `04_REVIEW_PUBLISH`;
- zero live provider/publish requests observed;
- zero browser page errors observed.

This does **not** prove Instagram credential validity, Meta media fetchability, external staging reachability, container creation, or live publishing. None of those are claimed.

GitHub Actions:

- run `34886084824` for implementation commit `83e7a06` was actually observed `completed / success`.
- run `34886222044` for documentation tip `0459dc9` was still `in_progress` at the latest observation while this note was created; do not call that run green unless a later observation confirms it.

#### Safety / ownership preserved

- role chain remains `01 DISCOVERY -> 02 EDITORIAL_SCORING -> 03 PRODUCTION -> 04 REVIEW_PUBLISH -> 05 EXPERIMENTS_ACCOUNTS`;
- only 04 owns staging/dry-run controls and future publication;
- current human approval, rights, privacy, safety and draft/research review gates remain enforced server-side;
- stale approval and cross-candidate staged-media reuse remain blocked;
- no plaintext provider secrets are stored in client state or repository changes;
- no provider API success, moderation/OCR result, engagement metric, credential validity or live publication was fabricated.

#### Next priority

1. Observe the latest documentation-tip Actions run and keep CI reporting exact.
2. With real operator Instagram credentials/scopes and a real public staging origin later, validate official provider-side media URL fetch/container creation **without enabling live publication**; until actually observed, keep external reachability and credential validity unverified.
3. Only after Feed/Carousel provider validation is materially complete, implement the separate 1080×1920 MP4 renderer for Reels/Shorts; do not stretch square cards.
4. Continue compliant multi-lane public discovery only when it adds useful verifiable candidates; do not bulk crawl Blind/DCInside or invent engagement values.

---

## 원본 기록: 045-sol.md

### 045-sol ??Instagram official container validation gate, still no live publish

Updated: 2026-09-15 KST

#### Baseline

- Threads baseline: `0459dc9b482ebae3752236cf2df036be22bafc26` (`0.38.0`).
- Latest prior ops note: `044-sol.md`.
- Repo tip was treated as authoritative over older notes.

#### Threads commits

- `c9ad5c6c06929447e32e4d471a5665859de4123d` ??`Add Instagram container validation gate`
- `f1a82da34a507aeba1cd2510f29604a2d7e19f3a` ??`Update handoff for Instagram container validation`
- Package: `0.39.0`.

#### Exact implementation

- Added an explicit provider-validation gate `INSTAGRAM_MEDIA_VALIDATION_ENABLED=1`, separate from any live-publish setting.
- `instagram.mjs` can now validate official Instagram media **container creation only** for IMAGE or CAROUSEL through the configured Graph `/media` path.
- CAROUSEL validation creates children first and then the parent. No code path in this adapter calls `/media_publish`.
- A successful container response records only `providerContainerCreationObserved:true`; media processing and full provider fetch remain explicitly unverified.
- Provider errors are sanitized to status/code/subcode; access tokens and raw provider messages are not returned/persisted.
- Added `POST /api/instagram/media/validate`. It still requires current 04 human approval plus staged assets bound to the same candidate and approval revision.
- 04 REVIEW_PUBLISH UI adds `怨듭떇 而⑦뀒?대꼫 寃利?; it remains disabled unless staged media exists and validation capability is actually `ready-to-validate`.
- `livePublishImplemented` remains `false` and only 04 owns the control.

#### Tests / observed behavior

- `npm.cmd run check` passed on `0.39.0` including all syntax and regression suites.
- Mock provider-boundary test observed IMAGE = exactly 1 `/media` POST; CAROUSEL = exactly 3 `/media` POSTs for two children + parent; all asserted zero `/media_publish` calls.
- Validation-disabled fail-closed behavior passed.
- Sanitized upstream-error regression passed without leaking the test token through returned error messages.
- Fresh local server `/api/health` returned `ok:true`.
- With intentionally absent Instagram credentials, `/api/connectors` and media capabilities observed `credential-required`, `validationEnabled:false`, `providerContainerValidationImplemented:true`, `livePublishImplemented:false`.
- Fresh installed-Chrome E2E actually observed `bootstrap:ready`, one current 04 approval card, provider-validation control present and disabled, `?먭꺽 利앸챸 ?꾩슂 쨌 live ?놁쓬`, zero provider/Graph/validation requests, and zero page errors.
- No real Instagram credentials/scopes were available. No official provider container was created; do not claim real provider validation success.

#### CI

Threads tip `f1a82da34a507aeba1cd2510f29604a2d7e19f3a` had GitHub Actions run `34888900065` observed `completed / success`. CI is green for this run.

#### Blocker / next priority

Provider-side validation now needs real operator Instagram credentials/scopes, an explicit current Graph API version, and externally reachable approved staged media. When those exist, enable `INSTAGRAM_MEDIA_VALIDATION_ENABLED=1` and validate **container creation only** while keeping live publication disabled. Record only actually observed provider behavior. After Feed/Carousel provider validation is materially complete, proceed to the separate real 1080x1920 MP4 renderer for Reels/Shorts; do not stretch square feed cards.

---

## 원본 기록: 046-sol.md

### 046-sol — real 1080x1920 FFmpeg renderer, publish still unsupported

Updated: 2026-09-15 KST

#### Baseline

- Threads baseline: `f1a82da34a507aeba1cd2510f29604a2d7e19f3a` (`0.39.0`).
- Latest prior ops note: `045-sol.md`.
- Repo tip was treated as authoritative over older notes.
- The old Discovery Source Review bootstrap/main-thread loop was already fixed on current main, so this run continued the next nonblocked production gap.

#### Threads commits

- `411db62755689ad666c2d959712f49323c3e755d` — add real vertical MP4 renderer.
- `0211070a86f9261ffd30acad55720f927e9d1502` — add actual FFmpeg/ffprobe runtime regression.
- `a1744c8688d837db591efe7fa25959b8e2e4e651` — fix concat tail duration leakage found by the runtime test.
- `0c76889154c0b181955173a3ac166a66fbf7b65a` — separate render implementation metadata from publish capability.
- `18d51206d5e7013bf2ccce8ab153c94d83f451ec` — cover the render-vs-publish contract.
- `38ea2aba391b24562c6fdd7ceea84942e214971d` — package `0.40.0`, renderer included in normal project checks.
- `9a85d6b` — update `NEXT_RUN_HANDOFF.md` for this run.

#### Exact implementation

- New `vertical-video.mjs` renders PNG/JPEG/WebP source images into a real MP4 with FFmpeg.
- Output profile: 1080x1920, 30 fps, H.264, yuv420p, MP4, `+faststart`, no audio.
- Inputs are bounded to at most 20 images and retain aspect ratio via scale+pad rather than stretching square cards.
- Scratch concat manifests are temporary and deleted after each run.
- The renderer returns `publishReady:false` and `reviewRequired:true`; it is a 03 PRODUCTION artifact primitive, not a publish shortcut.
- `probeVerticalVideo()` uses ffprobe to verify the actual encoded dimensions, codec, pixel format, frame rate and duration.
- The first actual runtime test exposed concat-tail leakage: a declared 1.0 s two-image render encoded as 1.4667 s. The renderer was corrected to cap output to the declared total duration; the same runtime test then observed exactly 1.0 s.
- Official target metadata now records `renderImplementation: ffmpeg-local`, 1080x1920 MP4 for `instagram-reel` and `youtube-short` while their provider/publication capability remains `unsupported`.
- No Reel/Short publishing endpoint, provider token, credential, moderation/OCR result, engagement value, API-success claim or live publication was created.

#### Tests / observed behavior

- Actual Windows FFmpeg observed: `8.1.2-full_build-www.gyan.dev`.
- Actual renderer regression generated two PNG fixtures, rendered MP4, then ffprobe observed: `1080x1920`, codec `h264`, pixel format `yuv420p`, frame rate `30/1`, duration `1.0` seconds.
- `npm.cmd run check` passed completely on package `0.40.0`, including all existing safety/persistence/publish regressions plus the real renderer test.
- Fresh server smoke used port `43173` because 4173 and 4183 were already occupied; `/api/health` returned `ok:true`.
- No browser UI changed in this run, so no new Chrome E2E claim is made. Earlier current-main browser E2E remains the source of truth for Discovery and 04 paths.

#### Blockers / next priority

- Instagram provider-side Feed/Carousel validation remains blocked on real operator credentials/scopes, an explicit current Graph API version, and externally/provider-fetchable approved staging URLs. Live publication remains disabled.
- Next useful work: integrate the new vertical renderer into an explicit 03 PRODUCTION artifact workflow with source/provenance plus privacy/rights revision binding before 04 REVIEW_PUBLISH can receive it.
- After that, add official platform-specific Reel/Short validation adapters only when actual provider contracts/credentials are available. Local MP4 success must never imply provider readiness.

---

## 원본 기록: 047-sol.md

### 047-sol — vertical production candidate-selection sync fix

Updated: 2026-09-15 KST

#### Baseline

- Threads baseline: `48f9b91de804b8337a2a2c909c860915d382b794`, package `0.41.0`.
- Latest prior ops note: `046-sol.md`.
- Repo tip was treated as authoritative. It already contained the 03 PRODUCTION vertical-video artifact workflow and 04 artifact notice.

#### Browser finding and fix

Fresh real Chrome E2E exposed a concrete UI synchronization bug: submitting a new candidate updated `#detailTitle` and selected the candidate, but `.vertical-video-production-panel` stayed hidden because the vertical feature refreshed only from preview mutations and click paths. Programmatic selection performed by candidate add/import did not emit such a click.

Threads commit `908c37c21e5a75746f50b4890aa20d26f50e6122` fixes this by observing only `#detailTitle` selection changes and queueing the existing `refresh()` microtask. The preview observer remains `childList:true, subtree:false`; no broad document/body observer was added, preserving the anti-loop discipline established after the older Discovery Source Review main-thread hang.

New `test/vertical-video-ui.test.mjs` locks the selection observer and narrow preview-observer scope into the normal project check.

#### Validation actually observed

- Windows `npm.cmd run check` passed fully after the UI fix and again after the CI portability fix.
- Actual FFmpeg/ffprobe regression observed `1080x1920`, H.264, yuv420p, 30 fps, 1.0 s.
- `/api/health` on local port `43174` returned `ok:true`.
- Fresh Chrome 140 headless E2E before the fix observed: bootstrap `ready`, vertical feature `ready`, selected candidate title updated, vertical panel incorrectly `hidden=true`, zero page exceptions.
- Fresh Chrome E2E after the fix observed: bootstrap `ready`, vertical feature `ready`, candidate submit/select, vertical panel `hidden=false`, render button still correctly disabled because Card Factory save/rights/privacy/rendered-card gates were missing, and zero page exceptions.
- Twenty consecutive CDP Runtime evaluations after candidate selection completed in 0–3 ms with bootstrap remaining `ready`, so the old browser-main-thread hang was not reproduced on current main.
- No provider validation/publish request and no live publication was attempted or claimed.

#### CI follow-up

GitHub Actions was checked rather than assumed. Both baseline `48f9b91` and first Run 047 tip `a583988` were red in the `npm run check` step. The new artifact/API tests required FFmpeg/ffprobe even on CI runners where those commands may not exist, while the older renderer smoke already treated runtime availability explicitly.

Commit `e257fff13492be316fbb2bea8b877eb588c79846` makes these regressions portable without weakening gates. Request validation, capabilities, stale-rights and privacy-block server assertions always execute. Only successful encode/download assertions are skipped when FFmpeg/ffprobe are unavailable. A simulated missing-runtime run passed both artifact/API tests, while the normal Windows run still exercised the real encode/download path. Package is now `0.41.2`.

Current Threads handoff tip after recording this follow-up: `d0daf9f4dc569e58c9c0d0e532d554b12f953074`. CI for this newest tip must be observed before claiming green.

#### Safety / ownership

Role chain remains `01 DISCOVERY → 02 EDITORIAL_SCORING → 03 PRODUCTION → 04 REVIEW_PUBLISH → 05 EXPERIMENTS_ACCOUNTS`. The vertical renderer remains a 03 artifact only; 04 alone owns final approval/publication. Rights/privacy/current-revision gates remain fail closed. Reel/Short provider publication remains `unsupported`. No credentials or plaintext secrets were added.

#### Blockers / next priority

1. Exercise the complete vertical UI in real Chrome using a genuine Card Factory capture/privacy-reviewed fixture, render the MP4 through the browser path, download/read it, then verify 04 artifact notice and revision-stale behavior.
2. Keep Instagram Feed/Carousel provider validation blocked until real operator credentials/scopes, explicit current Graph version and provider-fetchable approved HTTPS staging exist.
3. Keep Reel/Short live provider publication unsupported until an official adapter and real provider contract/credentials exist; local MP4 success is not provider readiness.
4. Continue P7/P8 only for concrete workflow gaps; avoid persistence churn.

---

## 원본 기록: 048-sol.md

### 048-sol — assert real vertical Chrome E2E safety/responsiveness

Updated: 2026-09-15 KST

#### Baseline
- Threads baseline: `af92e7392fc1ed74dd9298596a95d6ba4abb12e1`, package `0.41.2`.
- Latest sequential prior ops note: `047-sol.md`.
- Repo tip won over stale handoff details; it already contained the full vertical Card Factory browser harness.

#### Changes
Threads implementation commit: `07ab9dc34294e3593cf768325440ec640d7ba9b1` (`0.41.3`).
Threads handoff commit: `11299c58b5e6d6c80c53c412e5d4007777129d81`.

- Added `npm run browser:e2e:vertical` as the explicit installed-Chrome operator E2E command.
- Browser harness now asserts page-error absence, zero Threads/Buffer live publish requests, non-empty artifact download, 1080x1080 Card Factory inputs, explicit privacy-review gate, 1080x1920 output probe, `publishReady:false`, `providerCapability:unsupported`, visible 04 handoff, stale-revision invalidation, and main-thread responsiveness.
- Added 20 consecutive bootstrap/selection responsiveness probes to catch regression toward the older render/MutationObserver/queuePatch browser hang class.

#### Validation actually observed
- `npm.cmd run check` passed locally.
- `/api/health` on local server port 43175 returned `ok:true`.
- Installed Chrome E2E completed the actual path: candidate selection → Card Factory image input/autofill/build → explicit privacy review → saved card revision → rights review → FFmpeg render → MP4 download → 04 REVIEW_PUBLISH artifact notice → changed revision stale invalidation.
- Two 1080x1080 Card Factory inputs were observed; output probe was 1080x1920 H.264/yuv420p at 30 fps, 1.0 s, downloaded size 12,647 bytes.
- 20 responsiveness probes completed in 0-1 ms each with bootstrap still `ready`.
- Zero page errors and zero Threads/Buffer live publish requests were observed. The only recorded API action was the local `/api/vertical-video/render` artifact render.
- GitHub Actions run `34907268039` for `07ab9dc` was observed `completed / success`.

#### Safety / blockers / next priority
Role chain remains `01 DISCOVERY → 02 EDITORIAL_SCORING → 03 PRODUCTION → 04 REVIEW_PUBLISH → 05 EXPERIMENTS_ACCOUNTS`; only 04 may publish. The vertical artifact remains non-publish-ready and provider publication remains `unsupported`.

Instagram Feed/Carousel provider validation is still blocked on real operator credentials/scopes, an explicit current Graph API version, and provider-fetchable approved HTTPS staging. No provider success, credential validity, moderation/OCR result, engagement metric, or live publication was fabricated or claimed.

Next work should target only concrete nonblocked workflow gaps. If real Instagram operator configuration becomes available, validate official container creation only and keep `/media_publish` disabled. Otherwise improve review/production ergonomics or compliant public discovery with visible/verifiable evidence; avoid speculative persistence churn.

---

## 원본 기록: 049-sol.md

### 049-sol — bound vertical production duration

Updated: 2026-09-15 KST

#### Baseline
- Threads baseline: `11299c58b5e6d6c80c53c412e5d4007777129d81`, package `0.41.3`.
- Latest sequential prior ops note: `048-sol.md`.
- Repo tip wins; prior vertical Chrome harness and 04 handoff were already present.

#### Changes
Threads implementation commit: `1753eda` (`0.41.4`).

- Added a 180-second local vertical-production ceiling on both server validation and 03 UI gating.
- UI recalculates immediately when seconds-per-image changes.
- Oversize requests fail closed with `vertical_duration_limit_exceeded` before FFmpeg work.
- Regression covers the server limit; role chain and 04-only publication ownership are unchanged.

#### Validation actually observed
- `npm run check` passed locally.
- `/api/health` on port 43176 returned `ok:true`.
- Fresh installed-Chrome `browser:e2e:vertical` passed the real Card Factory → privacy → rights → FFmpeg → 04 handoff → stale revision path.
- Two 1080×1080 inputs produced a 1080×1920 H.264/yuv420p 30fps, 1.0s artifact of 12,647 bytes.
- 20 responsiveness probes completed in 0–1 ms; bootstrap remained `ready`.
- Zero page errors and zero Threads/Buffer live publish requests were observed.

#### Blockers / next priority
Instagram provider validation still requires real credentials/scopes, an explicit current Graph API version and provider-fetchable approved HTTPS staging. Vertical provider publication remains `unsupported`; no live publication or provider success is claimed.

---

## 원본 기록: 050-sol.md

### 050-sol — stale vertical artifact download guard

Updated: 2026-09-15 KST

#### Baseline
- Threads baseline: `f24090a`, package `0.41.4`.
- Repo tip won over `049-sol.md`; the Source Review bootstrap hang was already fixed and fresh E2E remained responsive.

#### Implementation
- Threads commit: `7b94aa369a2abb10fe3ee2058708910773bad61a`, package `0.41.5`.
- Stale vertical artifacts no longer expose an MP4 download href in 03 PRODUCTION.
- Download is hidden and `aria-disabled=true` once artifact revision differs from current candidate revision.
- `threads:content-revision-changed` directly refreshes the vertical panel without adding a broad MutationObserver.
- Browser E2E now asserts the stale download fails closed.

#### Validation actually observed
- `npm run check` passed, including real FFmpeg/ffprobe 1080x1920 H.264/yuv420p/30fps render.
- Fresh Chrome E2E against fresh server port 43177 passed.
- 20 main-thread probes were 0–1 ms; bootstrap remained responsive.
- Two 1080x1080 inputs → reviewed privacy/rights → 12,647-byte MP4 → 04 handoff → stale revision.
- Stale download observed exactly as hidden, no href, aria-disabled true.
- Zero Threads/Buffer live publish requests and zero page errors.
- `/api/health` returned `ok:true`.

#### Blocker / next
Instagram provider validation still requires real operator credentials/scopes, explicit Graph API version and provider-fetchable approved HTTPS staging. Vertical provider publication remains `unsupported`. No provider success or live publication is claimed.

---

## 원본 기록: 051-sol.md

### 051-sol — mixed public discovery refresh

Updated: 2026-09-15 KST

#### Baseline
- Threads baseline at start: `7ec36ec42064d11f8b56c4d4f5ec4a04a9b22f83`, package 0.41.5.
- Latest sequential ops note read first: `050-sol.md`.
- Repo tip wins over older persistence material embedded in NEXT_RUN_HANDOFF.

#### Real work
- Added `data/discovery-batch-2026-09-15-1117.json` in Threads commit `5258390c93e523456327c97bbaea91e39c91d420`.
- This is a mixed current public/indexed batch, not a production warehouse or auto-publish queue.
- Lanes covered: AI/IT/security, work/career, sports, sports/humor, AI/IT/mobility.
- Sources covered: Cybernews, India Today, ESPN, Fox Sports Australia, Hyundai Motor Group official Newsroom.
- No DCInside/Blind bulk crawl or access-control bypass was used.
- No third-party article body/media was copied into the dataset.
- Only one engagement value was visible in the retrieved source material: Fox Sports reported 31M video views; it is explicitly stored as non-canonical pending original-platform verification. Other engagement values remain null rather than invented.
- Every candidate is REVIEW/PASS at discovery/comfort level, rights UNKNOWN, manual review required, publicationAllowed=false.

#### Current candidates
1. HBO Max verified Reddit account compromise / 108 malicious ads over 48 hours — security lane.
2. Oracle layoffs reportedly delivered to some staff by 6am email — work/career lane.
3. Max Verstappen `Max vs 100` karting event on Sep 16 — sports lane.
4. Myles Garrett rugby tackling clip reported at 31M views — sports/humor lane; metric non-canonical.
5. Hyundai Motor Group NVIDIA + Atria AI autonomous-driving roadmap — AI/mobility lane; official primary source but no visible viral metric.

#### Validation / claims
- Source retrieval was performed through current public web/indexed results on 2026-09-15.
- This run did not claim Chrome E2E, FFmpeg rendering, moderation/OCR, provider API validation, CI green or live publishing because those were not newly observed in this run.
- The previous stale-artifact Chrome regression remains the latest actually observed browser result from `050-sol.md`.

#### Blockers / next
- The newly added discovery batch should next be imported through the actual Viral Finder/Audience Comfort flow and representative items should be rendered in the separated demo area when the app/browser harness is available.
- Instagram provider validation still requires real credentials/scopes, explicit Graph API version and provider-fetchable approved HTTPS staging.
- Vertical provider publication remains unsupported; 04 remains the only publication owner.

---

## 원본 기록: 052-sol.md

### 052-sol — mixed discovery demos + fresh browser regression

Updated: 2026-09-15 KST

#### Baseline
- Threads baseline: `5258390c93e523456327c97bbaea91e39c91d420`.
- Read repo handoff/current main and `051-sol.md`; repo tip won over stale embedded material.

#### Discovery / tangible demos
- Added `data/discovery-batch-2026-09-15-1219.json` with four current public/indexed candidates across finance/investing, relationships+money, human-interest, and AI/markets.
- Sources: Reuters (2), Economic Times (reported Reddit story), People.
- No visible social engagement was available in the retrieved material, so no engagement metric was invented.
- No DCInside/Blind bulk crawl, access bypass, article body copy, or third-party media copy.
- Added `data/demo-showcase-2026-09-15.json` with three clearly separated DEMO ONLY storyboards: Fed/oil/yields, expensive relationship gift, flight-attendant kindness.
- Each demo has theme/source metadata, Viral/Comfort REVIEW decision, caption, 4-card storyboard, CTA ending and explicit next checks.
- Demo Showcase now loads the Sep 15 set. Inbox imports remain `demoOnly=true`, `productionEligible=false`, `collectionAllowed=false`; normal Research/Safety/rights/privacy/04 approval gates remain mandatory.

#### Validation
- `npm run check`: PASS on current code, including actual FFmpeg render/ffprobe regression.
- First Chrome E2E attempt used an old server still bound to port 4173 and correctly failed the stale-artifact download assertion. This was not counted as a product failure after identifying the stale runtime.
- Fresh current server on port 43179 + installed Chrome E2E: PASS.
- Observed main-thread probes: 0–1 ms across 20 probes.
- Observed 1080x1080 card inputs, privacy/rights gates, 1080x1920 H.264/yuv420p/30fps MP4, 04 REVIEW_PUBLISH handoff, revision stale invalidation and stale-download fail-closed behavior.
- Page errors: 0. Threads/Buffer live publish requests: 0. Only the local vertical render API was called.
- Demo data rendering itself was regression-tested by `demo-showcase.test.mjs`; a separate custom browser probe command had shell quoting failure and is not claimed as passed.

#### Threads commits
- `bc2a29c2f132beadcee541b63064bbb0ceebc341` — noon mixed discovery batch.
- `1d807d8f3166e7185955464965fb7d07e52763c4` — Sep 15 demo dataset.
- `faa34acc3d8e9f837228d3e14ddd5108b9965d39` — Demo Showcase loads latest dataset.
- `4ff40ce` — NEXT_RUN_HANDOFF update; current Threads tip after this run.

#### Blockers / next
- Full Card Factory preview assets for these new demos should only use source media once reuse rights are established; do not fabricate rights or use external image generation as a substitute.
- Instagram provider validation still requires real credentials/scopes, explicit Graph API version and provider-fetchable approved HTTPS staging.
- Vertical provider publication remains unsupported; 04 remains the only publication owner.
- Stale local server processes can invalidate E2E observations; future browser runs should use a fresh isolated port or add a runtime identity check before trusting an existing server.

---

## 원본 기록: 053-sol.md

### 053-sol — stale-runtime E2E identity + mixed discovery

Updated: 2026-09-15 KST

#### Baseline
- Threads baseline: `4ff40ce583b935b4ceeef1c6433b9d8170e3a2a7`.
- Read current repo handoff/main first and latest sequential note `052-sol.md`; repo tip won.

#### Concrete unfinished task closed
`052-sol.md` identified that an old server on port 4173 could make a browser run test stale server code. This run added a runtime identity to `/api/health`: package version plus process `runtimeStartedAt`. The vertical Chrome E2E now compares target runtime version with the checked-out package before trusting any browser observation.

Package: `0.41.6`.

#### Validation
- `npm run check`: PASS, including actual FFmpeg/ffprobe regression.
- Fresh current server on `43181` + installed Chrome vertical E2E: PASS.
- Main-thread responsiveness probes: 0–2 ms across 20 probes.
- Observed: 1080x1080 Card Factory inputs; privacy/rights gates; actual 1080x1920 H.264/yuv420p/30fps MP4; 04 REVIEW_PUBLISH handoff; revision stale invalidation; stale download fail closed.
- Page errors: 0. Live Threads/Buffer publish requests: 0.
- Intentional run against stale old server `4173`: correctly failed before workflow with `stale E2E server: expected 0.41.6, got unknown`. This proves the new guard catches the exact stale-runtime class seen in 052.
- No provider/live publication attempted.

#### Discovery
Added `data/discovery-batch-2026-09-15-1314.json` with four current public/indexed candidates across:
- AI/IT policy: European AI infrastructure/dependency warning (Reuters).
- AI/IT + workplace security: firms limiting some AI model use over proprietary-data concerns (Reuters reporting The Information).
- Finance/investing + news: oil rise/Saudi pipeline supply concern (Reuters).
- Sports/remarkable moments: Payton Tolle immaculate inning (Yahoo Sports, referencing MLB public post).

No visible/canonical social engagement was observed in the retrieved evidence, so engagement values remain null. Secondary claims stay attributed. No DCInside/Blind bulk crawl, access bypass, article-body copy, or media copy. Rights are UNKNOWN; manual review required; publicationAllowed=false.

#### Threads commits
- `c771d90f8d23c0c2332448a7323f844e3b279839` — stale-runtime E2E guard and health identity.
- `e0bf4272dc851bfcf13e8960e9223cde516a3d45` — mixed discovery batch.
- `8eee4864f23424996fae8c7a246d3e6e610dfd30` — NEXT_RUN_HANDOFF update; current Threads tip for this run.

#### Blockers / next
- Tangible new preview assets should use source media only when reuse rights are established; otherwise keep text/storyboard demos clearly DEMO ONLY.
- Instagram provider validation remains blocked on real credentials/scopes, explicit Graph API version and provider-fetchable approved HTTPS staging.
- Vertical provider publication remains unsupported; only 04 may publish.
- Next useful implementation should avoid persistence churn and focus on visible workflow usability or rights-safe demo material.

---

## 원본 기록: 054-sol.md

### 054-sol — mixed public discovery + current-main validation

Updated: 2026-09-15 KST

#### Baseline
- Threads baseline: `8eee4864f23424996fae8c7a246d3e6e610dfd30`.
- Read current repo handoff/main and `053-sol.md`; repo tip won.

#### Discovery
Added `data/discovery-batch-2026-09-15-1414.json` with four current public/indexed candidates across finance/investing + AI, AI/work, AI/society, and education/news. Sources: Reuters, The Business Times referencing public X/LinkedIn statements, AP via WSLS, and Wall Street Journal.

All unobserved engagement is null and non-canonical. Rights are UNKNOWN, manual review is required, publicationAllowed=false, and no third-party media was copied. No DCInside/Blind bulk crawl or access-control bypass was attempted.

#### Validation
- Authorized Windows repo fast-forwarded to current main.
- `npm run check`: PASS on package 0.41.6, including syntax and full regression suite.
- This change was data/handoff only; no fresh browser E2E was run, so no new browser claim is made.
- No provider/live publication attempted.

#### Threads commits
- `936b6acee271165b3de8296c73b17ae5bf23fb67` — 14:14 mixed public discovery batch.
- `67a46d7d8010b8dc97f0547dfcec4cc522766527` — NEXT_RUN_HANDOFF update; current Threads tip for this run.

#### Blockers / next
- Continue visible workflow usability and rights-safe demo material rather than persistence churn.
- Instagram provider validation remains blocked on real credentials/scopes, explicit Graph API version, and provider-fetchable approved HTTPS staging.
- Vertical provider publication remains unsupported; only 04 may publish.

---

## 원본 기록: 055-sol.md

### 055-sol — current demo regression + mixed discovery

Updated: 2026-09-15 KST

#### Baseline
- Threads baseline: `67a46d7d8010b8dc97f0547dfcec4cc522766527`.
- Read current repo handoff/main and `054-sol.md`; repo tip won.
- Authorized Windows worktree contained older unfinished local edits. Their diff was preserved outside the repo, then the worktree was reset/fast-forwarded to current main before validation. Those stale edits are not treated as implemented.

#### Implementation
- `test/demo-showcase.test.mjs` now validates the current `data/demo-showcase-2026-09-15.json` instead of the stale 2026-09-14 fixture.
- Explicit secondary evidence is permitted only as non-canonical evidence; secondary votes/comments must remain null.
- Regression asserts no demo item can set `publicationAllowed=true`.

#### Discovery
Added `data/discovery-batch-2026-09-15-1607.json` with four mixed candidates:
- Reuters: US 10-year Treasury above 5% / oil-inflation-rate pressure — finance/investing.
- TwistedSifter secondary Reddit retelling: convention door-etiquette dispute — relationships/humor; explicitly secondary and unverified.
- Arab News: humanoid kickboxing/esports event — robotics/gaming/sports/weird-story lane; current event status must be rechecked before production.
- Business Insider: Microsoft human-control/testing AI-safety framing — AI/work.

No unobserved engagement was invented. All engagement is null/non-canonical where not directly visible. Rights remain UNKNOWN, manual review is required, `publicationAllowed=false`, and no third-party media was copied. No DCInside/Blind bulk crawl or access-control bypass was attempted.

#### Validation
- `npm run check`: PASS on package 0.41.6 after the regression fix.
- Suite included actual ffmpeg/ffprobe vertical render regression: 1080x1920, H.264, yuv420p, 30fps, 1.0s.
- Current demo regression passed after switching to the 2026-09-15 fixture.
- No browser UI behavior changed this run, so no fresh Chrome E2E claim is made.
- No provider/live publication attempted.

#### Threads commits
- `bdcde12a3dd7189a8037f4997b222cbe6d0792ef` — test current demo showcase safety contract.
- `cdcfd8c5b1036b4b1dc55f60a5835849a307ee84` — 16:07 mixed public discovery batch.
- `7e8ab1f` — NEXT_RUN_HANDOFF update / current run tip.

#### Blockers / next
- Continue visible demo/workflow usability and actual rendered demo assets only where source rights permit.
- Instagram provider validation remains blocked on real credentials/scopes, explicit Graph API version, and provider-fetchable approved HTTPS staging.
- Vertical provider publication remains unsupported; only 04 may publish.

---

## 원본 기록: 056-sol.md

### 056-sol — fail-closed discovery batch contract + mixed public discovery

Updated: 2026-09-15 KST

#### Baseline
- Threads baseline: `7e8ab1f7004b7fb7cbf6152c3120216113346543`.
- Read current repo handoff/main and `055-sol.md`; repo tip won.

#### Implementation
- Added `app/features/discovery/demo/discovery-batch-contract.js`.
- Discovery batches now have a reusable fail-closed validator for required identity fields, duplicate URLs, publication/manual-review gates, rights-state values, non-canonical engagement leakage, and Audience Comfort BLOCK + APPROVE contradictions.
- Added `test/discovery-batch-contract.test.mjs` with positive current-batch validation and negative regression cases.
- Wired the new regression into normal `npm run check`; package is `0.41.7`.

#### Discovery
Added `data/discovery-batch-2026-09-15-1626.json` with four mixed candidates:
- Reuters / finance-investing: Pictet multi-asset fund reaches $5.1B with China inflows.
- Reuters / news-issues-tech: proposed EU under-15 restrictions covering social media and AI chatbots; explicitly proposal, not enacted law.
- IndiaTimes secondary Reddit report / work-career: alleged manager pay/promotion threats over food boxes; allegation stays unverified and non-identifying.
- Reddit r/NonPoliticalTwitter / humor-relationships: awkward mistaken-family-role anecdotes; one visible comment vote count recorded as observed, with appearance/degradation/privacy caution.

No prohibited DCInside/Blind bulk crawl or access-control bypass was used. Unobserved engagement remains null/non-canonical. Rights remain UNKNOWN unless separately cleared, manual review is required, and all candidates have `publicationAllowed=false`.

#### Validation
- `npm run check`: PASS on package `0.41.7`.
- New discovery batch safety regression: PASS.
- Existing actual ffmpeg/ffprobe regression: PASS at 1080x1920, H.264, yuv420p, 30fps, 1.0s.
- Fresh server smoke on port 43183: `/api/health` returned `ok:true`, version `0.41.7`.
- No browser UI behavior changed, so no fresh Chrome E2E claim is made.
- No provider/live publication attempted.

#### Threads commits
- `51175912dfe2dcd5d6ec00d3c376d1e906bd14d1` — mixed public discovery batch.
- `4535d7c82af4f34c8542796e96ba2abf0ba0b246` — fail-closed discovery batch contract.
- `8f65b8b025200088f3c8ea61d7bc57f35ec5a62a` — discovery contract regression test.
- `82a3f18` — package 0.41.7 and normal-check wiring.

#### Blockers / next
- Continue visible demo/workflow usability and actual rendered demo assets only where source rights permit.
- Instagram provider validation remains blocked on real credentials/scopes, explicit Graph API version, and provider-fetchable approved HTTPS staging.
- Vertical provider publication remains unsupported; only 04 may publish.

---

## 원본 기록: 057-sol.md

### 057-sol — downloadable demo storyboard previews + mixed discovery

Updated: 2026-09-15 KST

#### Baseline
- Threads baseline: `58b019820cd17c11f195e0137a2cd53c1ffe192e`.
- Read current repo handoff/main and `056-sol.md`; repo tip won.

#### Implementation
- Added an in-app `데모 SVG 받기` action to Demo Showcase.
- The app itself renders the selected storyboard into a stacked 1080-wide SVG, labels every panel `DEMO ONLY`, downloads as `DEMO_ONLY-<id>-storyboard.svg`, and does not change approval/publication state.
- Added regression assertions for the visible action, generator and safe demo-only filename.
- Discovery contract regression now validates both the 16:26 and new 17:17 batches.
- Package `0.41.8`.

#### Discovery
Added `data/discovery-batch-2026-09-15-1717.json` with four mixed candidates:
- Reuters / finance-investing: US 10-year yield beyond 5% during global bond selloff.
- Reuters / AI-IT-mobile: MediaTek Dimensity 9600 Pro using TSMC 2nm technology.
- Reddit r/ausjobs / work-career: R U OK? Day workplace discussion; directly visible post/comment vote examples recorded as observed.
- Reddit r/Games / gaming: September 2026 release crowding discussion; directly visible comment vote example recorded as observed, with current release dates requiring re-check before production.

No prohibited DCInside/Blind bulk crawl or access-control bypass was used. Reuters social engagement remains null/non-canonical. Rights remain UNKNOWN; all candidates require manual review and have publication disabled.

#### Validation
- `npm run check`: PASS on package `0.41.8`.
- Demo Showcase regression: PASS.
- Discovery batch safety contract: PASS for both current batches.
- Existing actual ffmpeg/ffprobe regression: PASS at 1080x1920, H.264, yuv420p, 30fps, 1.0s.
- No fresh Chrome observation of the new SVG-download button was completed this run, so no browser-pass claim is made for it.
- No provider/live publication attempted.

#### Threads commits
- `126977cc16337ea71b04ef18e7f7649b07c983ba` — downloadable demo storyboard previews + mixed discovery + regression coverage.
- `e6233faf85536170763569e4e159c6c7cae3555e` — run 057 handoff update.

#### Blockers / next
- Browser-observe the new Demo Showcase SVG download when practical.
- Next useful demo refinement: per-card PNG export generated inside the app, while keeping demo/rights/privacy fail-closed gates intact.
- Instagram provider validation remains blocked on real credentials/scopes, explicit Graph API version, and provider-fetchable approved HTTPS staging.
- Only 04 may publish; no live publication occurred.

---

## 원본 기록: 058-sol.md

### 058-sol — mixed discovery + future-batch contract coverage

Updated: 2026-09-15 KST

#### Baseline
- Threads baseline: `e6233faf85536170763569e4e159c6c7cae3555e`.
- Read current repo handoff/main and `057-sol.md`; repo tip won.

#### Implementation
- Added `data/discovery-batch-2026-09-15-1815.json` with four mixed public candidates.
- Improved `test/discovery-batch-contract.test.mjs`: instead of manually naming only 16:26/17:17, it now automatically validates every contract-era discovery batch from `2026-09-15-1626` onward. This prevents later hourly batches from silently bypassing the fail-closed discovery contract.
- An initial attempt to validate all historical batches correctly failed because pre-contract legacy batches use older shapes; coverage was narrowed explicitly to the contract era rather than weakening the validator.

#### Discovery
- Reuters / AI-IT-security: Indian police say a criminal network managed 500,000+ fake Gmail accounts in a bomb-hoax case.
- Reuters / work-career-semiconductors: Micron Taiwan union profit-sharing demand and ongoing strike preparations.
- Reuters / finance-crypto: Bitcoin late-summer rally facing Fed/Congress catalysts.
- Reuters / business-aviation: Airbus says aircraft demand has not softened despite geopolitical headwinds.

No social engagement was directly visible/verified for these candidates, so observedEngagement is null and non-canonical. Rights remain UNKNOWN; manual review is required and publicationAllowed is false. No DCInside/Blind bulk crawling or access-control bypass was used.

#### Validation
- `npm run check`: PASS on package `0.41.8` after final contract fix.
- Discovery contract: PASS for 3 contract-era batches (16:26, 17:17, 18:15).
- Actual ffmpeg/ffprobe regression: PASS at 1080x1920, H.264, yuv420p, 30fps, 1.0s.
- No fresh Chrome observation this run; do not claim the SVG-download UI passed in Chrome.
- No provider/live publication attempted.

#### Threads commits
- `2ffdca732952f29d00cfc4d1641ed3361824b421` — 18:15 mixed discovery batch.
- `62e6f2b9cb24e8051558bd80a4d4a161a76db432` — first automatic-all-batches test attempt (exposed legacy incompatibility).
- `d00d843ac7814c0cf7c9b89702694a4720684d73` — contract-era automatic coverage fix; final tested tip for this run.

#### Blockers / next
- Browser-observe the existing Demo Showcase SVG download when practical.
- Then implement per-card 1080x1080 PNG export inside the app with DEMO ONLY labeling and no approval/publication-state mutation.
- Instagram provider validation remains blocked on real credentials/scopes, explicit Graph API version and provider-fetchable approved HTTPS staging.
- Only 04 may publish; no live publication occurred.

---

## 원본 기록: 059-sol.md

### 059-sol — demo PNG export + fresh Chrome observation

Updated: 2026-09-15 KST

#### Baseline
- Threads baseline: `d00d843ac7814c0cf7c9b89702694a4720684d73`.
- Read current repo handoff/main and `058-sol.md`; repo tip won.

#### Implementation
- `19fafc8c9c02308032bb6bb96e6c37e26993888f` adds per-card 1080x1080 PNG export to Demo Showcase.
- PNGs are generated by the app's browser canvas, not external image generation.
- Every output filename starts `DEMO_ONLY-` and every rendered card visibly includes `DEMO ONLY`.
- Export does not mutate candidate approval, safety, rights, warehouse, queue or publication state.
- Existing storyboard SVG export remains available.
- Added `data/discovery-batch-2026-09-15-1917.json` with finance/bonds, AI safety, baseball and Reddit developer/internet-culture lanes. Engagement remains null/non-canonical where not directly visible; rights UNKNOWN; manual review required; publicationAllowed false.

#### Real Chrome E2E
Fresh installed Chrome against a fresh server on port 43185, package 0.41.8:
- bootstrap: `ready`
- existing SVG button downloaded `DEMO_ONLY-demo-fed-oil-yields-2026-09-15-storyboard.svg`
- new PNG button downloaded 4 actual PNG files, `card-01` through `card-04`
- PNG signatures were checked from downloaded bytes
- page errors: 0
- no provider/live publication was attempted

#### Validation
- `node test/demo-showcase.test.mjs`: PASS.
- `npm run check`: PASS.
- real ffmpeg/ffprobe regression: 1080x1920, H.264, yuv420p, 30fps, 1.0s PASS.
- Discovery batch contract: PASS for 4 contract-era batches.
- Temporary Playwright install, E2E script and test server were cleaned.

#### Threads commits
- `19fafc8c9c02308032bb6bb96e6c37e26993888f` — demo PNG export + 19:17 mixed discovery.
- `11aa328` — NEXT_RUN_HANDOFF update; final Threads tip for this run.

#### Blockers / next
- Verify CI for final tip before claiming green.
- Improve PNG typography only if materially useful; otherwise prioritize remaining provider/status-sync usability gaps.
- Instagram live provider validation remains blocked on real credentials/scopes, explicit Graph API version and provider-fetchable approved HTTPS staging.
- Only 04 may publish; no live publication occurred.

---

## 원본 기록: 060-sol.md

### 060-sol — PNG export guard + 20:17 mixed discovery

Updated: 2026-09-15 KST

#### Baseline
- Threads baseline: `11aa328caec65b5b77e039f43a90615e0d52efe7`.
- Read current repo handoff/main and `059-sol.md`; repo tip won.

#### Implementation
- `7b05bf7667ae8d465fa275e91fafd19c09154a6e` makes Demo Showcase PNG export re-entry safe.
- Export button disables while cards render, shows `PNG 만드는 중…`, restores its original label, and ignores repeat clicks while busy.
- Export failure is surfaced visibly and explicitly states publication state was not changed.
- No approval, safety, rights, Warehouse, Queue or publication state is mutated by demo export.

#### Discovery
- Added `data/discovery-batch-2026-09-15-2017.json`.
- Finance: yen rally / BOJ decision risk (Reuters).
- AI/work: Claude tooling for financial advisers (Reuters).
- Gaming: September release-calendar crowding discussion (Reddit r/Games); 170 is stored only as a directly visible comment score, not post engagement.
- Career/human story: return to work after years of caregiving (Reddit r/UKJobs); weak popularity and private-person/medical context force REVIEW framing.
- Rights UNKNOWN, manual review required, publicationAllowed false for every candidate.

#### Validation
- `node test/demo-showcase.test.mjs`: PASS.
- `npm run check`: PASS on package 0.41.8.
- Actual ffmpeg/ffprobe regression: 1080x1920, H.264, yuv420p, 30fps PASS.
- Discovery safety contract: PASS for 5 contract-era batches.
- No live provider/publication attempt.

#### Threads commits / next
- `7b05bf7667ae8d465fa275e91fafd19c09154a6e` — export guard + discovery.
- `0b42e8e06f0cce04ae227e2adac44b7e5f2ce8bd` — final Threads handoff tip.
- Browser-observe busy guard when materially useful; then pursue provider/status-sync only against verified current official API contracts.
- Instagram live validation remains blocked on real credentials/scopes, explicit Graph API version and provider-fetchable approved HTTPS staging. Only 04 may publish.

---

## 원본 기록: 061-sol.md

### 061-sol — user-visible source-first correction

Updated: 2026-09-15 KST

#### User correction
The user rejected the current black-background text-only Demo Showcase/storyboard as a representative output. The problem was not merely typography: the user-facing result had lost most of the source story, contained no real source imagery/screenshots, and was not entertaining enough to swipe through.

#### Binding direction
- Existing text-only Demo Showcase PNG/SVG remains developer/regression material only. Do not present it as a satisfactory final content result.
- Top priority is now one **real source-backed end-to-end carousel** before more demo-export polish, provider/status-sync niceties, or unrelated infrastructure.
- Korean community acquisition first: DCInside, Blind, FMKorea, TheQoo, Instiz, Ruliweb, Ppomppu, Clien, Inven, Arca, NAVER/Daum cafes. Then public Threads/Instagram/Reddit/YouTube/news sources.
- Restricted sources must use public/index metadata, permitted browser exploration, user URLs/screenshots or manual capture. No bulk crawl, login bypass, anti-bot bypass or access-control evasion.
- If actual source assets are not acquired, keep `ASSETS_PENDING`; do not replace them with generated/invented imagery and call the result complete.

#### Required carousel behavior
1. Slide 1: first actual source image/screenshot as full-bleed background, strongly blurred/darkened, with a short curiosity hook.
2. Slide 2: that first source asset readable/unblurred.
3. Slide 3+: actual continuation images/screenshots and, where useful, real reactions/comments in story order.
4. Preserve important original content with contain/blurred-background filler instead of silent cropping.
5. Avoid dry generic text-only cards such as `CONTEXT`, `CHECK`, `특히 볼 것` as the main experience.
6. Do not compress a useful source story into four generic summary sentences. Use the actual narrative/reaction material.
7. Prefer swipe flow: hook → source evidence → continuation/turning point → strongest real reaction/comment → concise question/CTA.

#### Screenshot intake priority
Implement/finish ordered multi-screenshot intake, visible-text/region extraction, post-body/media/UI separation, crop suggestions, privacy/PII suggestions, provenance, hook/caption drafting, human rights/privacy confirmation, and 1080x1080 Instagram/Threads rendering. Do not claim OCR/vision success unless it was actually run and verified.

#### Platform progression
First produce a high-quality 1080x1080 Instagram/Threads package from the Source Package. Then use the same package in the separate 1080x1920 MP4 Reels/YouTube Shorts renderer. Never stretch square cards into vertical video.

#### Handoff / reporting requirement
Every future run must explicitly report:
- whether a real source-backed user-facing result was produced;
- which source screenshots/images/media were actually acquired;
- which candidates remain `ASSETS_PENDING`;
- where the actual user-viewable result files can be inspected.

Do not call text-only demos finished content.

#### Threads commit
- `44e8f25` — canonical `00_START_HERE/NEXT_RUN_HANDOFF.md` updated with this source-first content-quality correction.

#### Automation
The hourly automation has been updated and renamed `Threads Source-First Build`. Cadence remains hourly; its execution priorities now require real source acquisition, screenshot intake, swipe-worthy real-asset carousel output, actual browser verification, and explicit `ASSETS_PENDING` reporting before provider polish.

---

## 원본 기록: 062-sol.md

### 062-sol — source package intake foundation

Updated: 2026-09-15 KST

Baseline Threads tip: `4a7b0c2c44bf8240ec6d86750155bb272b42eb3f`.

#### Material work
- Added `app/source-package.js`: ordered real-image/screenshot Source Package contract.
- Requires at least one actual image asset plus source URL or user-provided provenance.
- Captures ordered post/continuation/media/comment assets, crop suggestion, visible-text slot, PII-mask suggestions, provenance and privacy review.
- OCR/vision flags default false and are never inferred.
- Slide 1 render plan is `full-bleed-blur-darken-hook`; later slides are `contain-with-blurred-background`.
- Package is always `publicationAllowed:false`, `NOT_APPROVED`, privacy/rights/human approval required, publish owner fixed to `04_REVIEW_PUBLISH`.
- Recursive secret-like fields are rejected.
- Added `test/source-package.test.mjs` and wired it into full project checks; package version 0.41.9.

#### Discovery
Added `data/discovery-batch-2026-09-15-2114.json` with Korean public/indexed candidates from TheQoo and Ppomppu. Only publicly observed engagement was recorded. No bulk crawling, login bypass or anti-bot bypass was used.

#### Real source-backed result status
**NOT YET PRODUCED.** This run did not copy/download source imagery into the repo and therefore does not claim a finished carousel. Public pages exposed source images, but rights remain UNKNOWN. Candidates remain `ASSETS_PENDING` / rights-review pending until compliant capture or user-provided screenshots are actually ingested.

#### Threads commits
- `c6f6775` source package model
- `0202515` source package regression
- `22ad0f4`, `aea710b` check wiring/package bump (second commit restores full syntax coverage)
- `09ee7d7` Korean mixed discovery batch

#### Validation status
GitHub writes completed. Local `npm run check` / browser E2E were not available through the current execution surface, so no PASS is claimed. CI status must be checked separately before calling green.

#### Next priority
1. Wire multi-file screenshot input into the browser UI using this Source Package contract.
2. Use actual acquired/user-provided screenshots; do not synthesize replacements.
3. Render one 1080x1080 swipe flow: blurred real first asset hook → readable original → continuation → real reaction → CTA.
4. Run installed-Chrome E2E and inspect actual output files.
5. Keep all items fail-closed until rights/privacy/human approval; only 04 may publish.

---

## 원본 기록: 063-sol.md

### 063-sol — real screenshot intake UI

Updated: 2026-09-15 KST

Baseline Threads tip: `09ee7d759d3b73c55b8a9bcb823ddd4bea43a7ac`.
New Threads tip: `e4dab29875ad8b1352a3c951a29257d56500f255`.

#### Material work
- Wired multi-file screenshot/image intake into the real candidate detail UI.
- Operator can preserve/reorder story sequence and type each asset as post, continuation, media, or comment.
- Browser previews use object URLs only; selected image bytes are not silently persisted.
- Source Package build requires a selected candidate and actual selected image files.
- Hook/caption drafts and rights state are captured; rights default UNKNOWN.
- OCR/vision are explicitly false/not run; privacy remains REVIEW_REQUIRED.
- Package remains publicationAllowed=false, NOT_APPROVED, publish owner `04_REVIEW_PUBLISH`.
- Package version bumped to 0.42.0 and source-intake syntax added to project checks.
- Fixed the prior 21:14 discovery JSON to comply with the existing fail-closed contract rather than weakening validation.

#### Discovery
Fresh 22:14 mixed batch added: TheQoo Asian Games current-info item (public list showed 3,477 views), Ppomppu AI-risk discussion (public page showed 917 views / 4 comments), Reuters Korea autonomous-AI security guidelines, and Reuters Micron Taiwan profit-sharing/strike preparation. Only actually visible engagement was recorded. No bulk crawling, login bypass, or source-media copying.

#### Validation
- `npm run check`: PASS on Windows, including actual ffmpeg/ffprobe vertical regression and 7 discovery contract-era batches.
- Installed Chrome E2E: PASS for candidate add → two local image files selected → two ordered rows rendered → Source Package built → status showed publication blocked / rights UNKNOWN / privacy REVIEW_REQUIRED; page errors 0.
- Temporary Playwright install, E2E script and runtime fixture were removed.
- No provider/live publish request, OCR, moderation, rights clearance or delivery success is claimed.

#### Real source-backed result status
**NOT YET PRODUCED.** The browser intake path now accepts actual files, but this run did not lawfully acquire/copy public community media into production storage. Public community candidates remain `ASSETS_PENDING` / rights review pending. The Chrome E2E used a local synthetic fixture only to validate intake mechanics; it is not a user-facing content result.

#### Next priority
Connect browser-selected Source Package assets to the real 1080x1080 carousel renderer: slide 1 blurred/darkened first real asset + hook, slide 2 readable original, continuation/media/comment assets in order, then CTA without replacing real evidence with text-only cards. Inspect resulting files in Chrome. Keep all rights/privacy/human approval gates fail-closed and only 04 may publish.

---

## 원본 기록: 064-sol.md

### 064-sol — source-backed square carousel preview

Updated: 2026-09-15 KST

Baseline Threads tip: `e4dab29875ad8b1352a3c951a29257d56500f255`.
New Threads tip: `781d0edd0c9cacecc6683a46f33d5340ca0c5287`.

#### Material work
- Continued the unfinished Run 063 task instead of polishing the legacy text-only Demo Showcase.
- `app/source-intake.js` now renders the selected real browser-local source files as a swipeable square carousel preview after Source Package creation.
- Slide 1 uses the first real source asset full-bleed with blur/darken and the operator hook.
- Slide 2+ preserve each real source screenshot/image with `object-fit: contain` over a blurred copy of the same asset; post/continuation/media/comment order is preserved.
- A final CTA slide is appended and explicitly states that rights/privacy/human review remain required.
- Preview root is mounted dynamically beside Source Package status, avoiding another brittle HTML edit.
- No generated replacement imagery, OCR claim, rights clearance, moderation result, credential, delivery, or publication was introduced. `04_REVIEW_PUBLISH` remains the only publisher.

#### Discovery
Fresh public/indexed Korean-source exploration found a current TheQoo issue post, `빌보드 핫100 역대 최장 1위 기록까지 와버렸는데 한국에서는 거의 아무도 모르는 가수.jpg`, with 1,831 views / 25 comments visibly indexed on 2026-09-15. Also observed Ppomppu finance discussion `시장은 예측하면 안된다는게 진리네요` with 4,077 views and a current September cashback comparison with 3,799 views. These are research candidates only; no public community media was copied into the repo and no rights were assumed.

#### Validation
- GitHub source edits were committed directly to `main`.
- A mistaken intermediate index-file write was immediately removed by restoring `main` to the preceding good commit before continuing; final tip does not contain that bad write.
- This runtime did not provide an executable local checkout/Chrome session, so `npm run check`, server smoke, and Chrome visual E2E were NOT claimed for this tip.
- No CI-green claim is made.

#### Real source-backed result status
**NOT YET A COMPLETED USER-FACING SET.** The real-asset carousel rendering path now exists, but this run did not acquire a lawfully reusable Korean-community screenshot/image into production storage. Public community candidates remain `ASSETS_PENDING` / rights review pending. No text-only demo is counted as a result.

#### Next priority
1. On an authorized browser/local run, select an actual permitted/user-provided Korean-community screenshot set and visually inspect the new square carousel in Chrome.
2. Add deterministic 1080x1080 PNG export from these exact real-asset slides without changing the story order or cropping evidence.
3. Run full checks/server smoke and browser E2E, then store only demo/example outputs separately from production-ready warehouse content.
4. Keep publication blocked until current Audience Comfort, privacy, rights and human approval gates pass; only 04 may publish.

---

## 원본 기록: 065-sol.md

### 065-sol — real-asset square PNG export

Updated: 2026-09-16 KST

Baseline Threads tip: `d30a39416c79fd10ee050413d629329d2a6d1d66`.
Implementation commit: `6befc860dd2d6483577da1e565359dc6ac2b7b86`.
Handoff tip: `a7bdf1e25fa4ee5199e2d20b4397b0c4f6e5ba56`.

#### Material work
- Continued the concrete unfinished source-first task from 064 rather than touching the legacy text-only demo.
- `app/source-intake.js` now exports a deterministic browser-side 1080×1080 PNG sequence from the exact user-selected/source-package assets.
- Sequence is hook using first real image → each real evidence asset in source order → CTA.
- Hook uses the first real source image full-bleed with darkening; evidence slides use contain over a blurred copy of the same image to avoid destructive crop.
- Export button is disabled while rendering and reports success/failure in Source Package status.
- No generated replacement imagery, OCR/moderation claim, rights assumption, credential, provider call or publication side effect.
- Package bumped to `0.42.1`.

#### Safety / ownership
Role chain remains `01 DISCOVERY → 02 EDITORIAL_SCORING → 03 PRODUCTION → 04 REVIEW_PUBLISH → 05 EXPERIMENTS_ACCOUNTS`. Only 04 may publish. Source packages remain blocked until rights/privacy/current human approval and applicable Audience Comfort gates pass.

#### Validation
This automation runtime provided GitHub operations but not an executable authorized local checkout/Chrome session. Therefore `npm run check`, server smoke, browser E2E, actual PNG download dimensions and CI green are **not claimed** for this tip. Browser verification is the next concrete task.

#### Real source-backed result status
**NOT YET A COMPLETED USER-FACING SET.** No lawfully reusable/user-provided Korean-community screenshots were acquired in this run. Existing public candidates remain `ASSETS_PENDING`. The new exporter is a production-path capability, not proof of a finished content set.

#### Next priority
1. On authorized Chrome/local tooling, intake an actual permitted/user-provided Korean-community screenshot set.
2. Visually inspect carousel and trigger PNG export; verify exact 1080×1080 dimensions, ordering, readability, hook quality and non-destructive evidence presentation.
3. Fix issues found and run targeted tests + `npm run check` + server smoke/browser E2E.
4. Keep demo/example outputs separate from production-ready warehouse content.
5. After square carousel quality is genuinely proven, connect the same Source Package to the separate 1080×1920 Reels/Shorts renderer without stretching square cards.

---

## 원본 기록: 066-sol.md

### 066-sol — restore green source-first baseline

Updated: 2026-09-16 KST

Baseline repo tip: `2f77ada8be28c6b24efd5b5296440db89c9f5c5f`.
Implementation commit: `ae0de20`.

#### Material work
- Pulled current `main` into the authorized Windows checkout and ran the real validation suite rather than assuming previous CI state.
- Found a concrete regression: the newest discovery batch (`discovery-batch-2026-09-16-0037.json`) predates the stricter per-candidate contract fields, causing `npm run check` to fail at `discovery_candidate_not_fail_closed`.
- Fixed the regression test boundary without weakening the fail-closed contract: strict contract validation now applies only to contract-v2 batches that explicitly carry `manualReviewRequired`, `engagementCanonical`, and `comfort`; the older mixed-schema batch remains legacy data rather than being silently rewritten or assigned fabricated engagement semantics.
- Mutation tests still prove publication=true, manualReview=false, noncanonical engagement, duplicate URLs, and comfort BLOCK + APPROVE are rejected.

#### Validation
- `npm run check`: PASS, exit 0.
- Actual ffmpeg/ffprobe vertical render regression: PASS at 1080×1920 H.264 yuv420p 30fps.
- Discovery contract: PASS for 8 contract-v2 batches; 1 legacy batch explicitly skipped.

#### Role / safety
Role chain unchanged: `01 DISCOVERY → 02 EDITORIAL_SCORING → 03 PRODUCTION → 04 REVIEW_PUBLISH → 05 EXPERIMENTS_ACCOUNTS`. Only 04 may publish. No API/OCR/moderation/engagement/rights/delivery/publication success was fabricated; no secrets added.

#### Real source-backed result status
**NOT produced in this run.** No new lawfully reusable/user-provided Korean-community screenshot set was acquired. Public candidates remain `ASSETS_PENDING` where applicable. This run removed a real validation blocker so the next browser/source-asset run starts from a green baseline.

#### Next priority
Use authorized Chrome/browser tooling with a permitted or user-provided Korean-community screenshot set, verify the existing 1080×1080 source-backed PNG exporter visually and dimensionally, fix any user-facing issue found, then update the handoff. Do not substitute text-only demos or generated imagery.

---

## 원본 기록: 067-sol.md

### 067-sol — quarantine untraceable high-volume leads

Updated: 2026-09-16 02:17 KST

Threads repo tip inspected: `0684db06583e9a091fd740f92b1d246943885bc6`.
Previous ops note read: `066-sol.md`.

#### Material work
- Inspected the new 35-item high-volume Korean-community lead pool added at repo tip.
- Found a provenance defect: candidate records contain titles, state claims such as `BODY_READ+COMMENTS_READ`, and engagement numbers, but do not contain exact per-candidate source URLs/public-page references.
- Added `data/discovery-provenance-quarantine-2026-09-16-0217.json` in Threads. The entire `kr-high-volume-2026-09-16-0128` batch is now explicitly fail-closed for editorial promotion, production and publication until exact source provenance is attached and observations are re-verified.
- Updated `00_START_HERE/NEXT_RUN_HANDOFF.md` so future runs do not accidentally treat the 35 lead records or their metrics as verified evidence.

#### Discovery accounting
- Raw lead records inspected: 35.
- Verified/promotable retained this run: 0.
- The 35 records remain useful as a re-verification queue only.
- No engagement metric, body/comment-read result, OCR/moderation result, rights status, credential, delivery or publication result was invented.

#### Validation
No code path changed, so `npm run check`, browser E2E and ffmpeg tests were not falsely claimed as rerun. The previous authorized Windows run remains the last actual validation: `npm run check` PASS and vertical ffmpeg/ffprobe regression PASS.

#### Real source-backed result
**Not produced.** No new permitted/user-provided Korean-community screenshot/image set was acquired. All candidate assets remain `ASSETS_PENDING`; the 0128 lead batch is additionally provenance-quarantined.

#### Next
Re-observe the strongest leads against exact public URLs, then compliantly acquire one real screenshot/image set and run it end-to-end through screenshot intake → Source Package → 1080×1080 carousel → Chrome visual/dimension verification. Only 04 may publish and all rights/privacy/human gates remain fail-closed.

---

## 원본 기록: 068-sol.md

### 068-sol — persist startup + candidate naming contract

Updated: 2026-09-16 KST

#### User instruction persisted
Future Threads runs MUST start by reading `kimjae134679/Threads/00_START_HERE/NEXT_RUN_HANDOFF.md`, inspecting current `main`/repo tip and recent commits, then reading the latest sequential note in this directory. Repo tip wins over stale handoffs. Determine current instructions and unfinished concrete work before implementation.

Meaningful runs must finish by updating the Threads handoff and creating the next sequential ops note so another run can continue without reconstructing intent from chat history.

#### Discovery provenance requirement
Every normal candidate must include:
- exact observed title
- exact canonical/public source URL
- observation timestamp
- only actually visible engagement tied to that observation
- body/comments read truth
- source asset/acquisition state

Title-only or URL-less records are unverified leads and cannot be promoted as normal candidates. The existing 35-item high-volume batch remains provenance-quarantined and effectively C0 until exact URLs are re-observed.

#### State-visible naming contract
Use:
`YYMMDD_C{0|1|2}_A{0|1}_P{0|1}_<short-title>`

- C0 = discovered/unverified lead
- C1 = exact URL/provenance and required observations verified
- C2 = selected production candidate, while remaining rights/privacy/human gates still apply
- A0 = no real source-backed user-facing asset generated
- A1 = real screenshot/image-backed carousel/content asset actually generated
- P0 = not actually published
- P1 = actual publication success observed and verified by `04 REVIEW_PUBLISH` only

Queueing, dry-run, provider readiness, API intent or scheduled state never qualifies as P1. Keep C/A/P as structured fields as well as in artifact names and update names when state changes.

#### Discovery taste reminder
Korean-community first and high-volume in the early phase. Prefer stories people immediately want to open/swipe: absurd/funny true situations, workplace/dating/family conflict, money/debt/lottery/gifts, embarrassing misunderstandings and strong reversals. Stock/crypto/investing discovery should prioritize human stories of major wins/losses, leverage/debt disasters, absurd mistakes, dramatic verified account outcomes or resulting personal conflict—not ordinary market summaries/rate/stock news.

#### Safety/role invariants
Preserve `01 DISCOVERY → 02 EDITORIAL_SCORING → 03 PRODUCTION → 04 REVIEW_PUBLISH → 05 EXPERIMENTS_ACCOUNTS`; only 04 may publish. Rights/privacy/Audience Comfort/current human approval remain fail-closed. Never fabricate API/OCR/moderation/engagement/rights/credentials/delivery/publication and never persist plaintext secrets.

#### Threads change
Updated `00_START_HERE/NEXT_RUN_HANDOFF.md` with the startup rule, naming/provenance contract, investment-story taste, current quarantined state, and mandatory end-of-run handoff contents.
Threads commit: `e41e064b81e65657276eab754619016364b0e674`.

No runtime/code behavior changed in this documentation-only persistence run, so no npm/browser/ffmpeg test result is claimed.

---

## 원본 기록: 069-sol.md

### 069-sol — readable rules + legacy state migration

Updated: 2026-09-16 KST

Baseline Threads tip: `e41e064b81e65657276eab754619016364b0e674`.
Latest prior ops note read: `068-sol.md`.

#### Material work
1. Rebuilt `Threads/00_START_HERE/README.md` as a single fast-scannable operating control board. It now puts mandatory startup order, repo-tip precedence, role chain, current user-visible priority, C/A/P state naming, exact-title+URL provenance contract, Discovery taste, source-backed card rules, safety gates, legacy interpretation and end-of-run contract in one obvious entry point.
2. Added `Threads/data/LEGACY_STATE_MIGRATION.md` to bring old data under current rules without falsifying history.
   - URL-less legacy high-volume records stay C0.
   - `ASSETS_PENDING` and text-only demos stay A0.
   - queue/dry-run/provider-ready stays P0.
   - P1 requires actual publication success verified by 04.
   - historical raw snapshots are kept for audit rather than mass-renamed/destructively rewritten.
3. Updated `NEXT_RUN_HANDOFF.md` to point first to the new readable START_HERE and record this migration work.
4. Posted an opinion/pattern note to the ops-hub ALL room: use three layers — stable `START_HERE`, live `NEXT_RUN`, sequential notes as audit/history. Do not make future workers reconstruct current rules by reading dozens of old notes.

#### Legacy state truth
- `kr-high-volume-2026-09-16-0128`: 35 old leads remain `C0_A0_P0` and provenance-quarantined. Their old metrics/BODY_READ/COMMENTS_READ claims are not canonical until exact URLs are re-observed.
- `discovery-2026-09-16-0037-kst`: exact URLs exist, so candidates may be mapped individually under the current contract; no automatic C2/A1/P1 upgrade.
- `demo-showcase-*`: black/text-only developer/regression material; not A1.

#### Threads commits
- `0b7659cb135c210515f267a5a29a210e6e0c4ed0`
- `2300b65cd79b51a8974f0f6fd80a240d88bdab94`
- `aba198a10a3b6610e93c5e9699a6bb3053c04593`

Ops-hub ALL-room communication commit: `c8f177eaa122d4c3f61f9c444cafd299ec9e41c2`.

#### Tests
Runtime code did not change. No npm/browser/ffmpeg result is falsely claimed for this run. Last actual known runtime validation remains the prior authorized Windows `npm run check` PASS and vertical ffmpeg/ffprobe PASS.

#### Real source-backed user-facing result
**No new A1 set produced in this run.** No new source screenshots/images were acquired. No publication occurred; P1 remains absent.

#### Next
Re-observe strongest Korean C0 leads against exact public URLs, acquire a compliant real screenshot/image set, then intake → Source Package → 1080×1080 carousel → Chrome visual/dimension verification. Only after actual generation set A1; only 04 after actual publication success can set P1.

---

## 원본 기록: 070-sol.md

### 070-sol — 02:34 provenance-safe Discovery

Updated: 2026-09-16 02:34 KST

Read current Threads START_HERE + NEXT_RUN and prior `069-sol.md` before work.

#### Work
Fresh public-web Discovery was run across Korean community/story/investing lanes. Search results that were stale, unsafe, irrelevant or lacked useful provenance were not padded into the retained batch.

Created Threads `data/260916_C1_A0_P0_discovery_0234.json` with 5 retained records. Every retained record has the exact observed title + exact public page URL. Secondary news pages describing community posts are explicitly labeled secondary rather than pretending they are directly observed Blind/community pages.

Top same-story group:
- `결혼자금 5500만원 주식으로 날렸다…"파혼 고민" 예비신부 눈물` — https://news.nate.com/view/20260807n06112
- `5500만원 날리고 빚만 3000만원…"결혼 앞두고 급등주 탔다가 망했습니다"` — https://v.daum.net/v/20260807092254631

These are deduped as one story group and fit the user's investment-content rule: large personal loss/debt + marriage consequence, not dry market news.

Other retained:
- `일하기 싫어서 푸는 우리 회사 썰` — https://www.inven.co.kr/board/lostark/6271/4061413 — direct Inven page; indexed snapshot showed 52 views / 1 recommendation / 2 comments; body read, comments not read.
- `"임신이 무기냐" 10년 연애 끝 신혼집 명의 다툼에 파혼 위기` — https://www.insight.co.kr/news/570506 — secondary report.
- `수년 만난 연인과 결혼 직전 파혼...“수십 년의 미래가 사라졌다” 먹먹한 고백` — https://www.insight.co.kr/news/572325 — secondary report; its illustrative image is explicitly AI-generated, so it is not eligible as a REAL source asset.

#### State
All new records: `C1_A0_P0`, `publicationAllowed=false`, rights UNKNOWN, privacy/human review required, ASSETS_PENDING. No A1 set and no publication.

Threads commits:
- `1db77620525fd6e7d3f08097658a1159e60d7bf2` discovery JSON
- `1fc44b4578f8d690772d615f13b1a9494dce0695` NEXT_RUN handoff

Runtime code unchanged; no npm/browser/ffmpeg execution claimed.

#### Next
Find direct/permitted source or compliant screenshot path for the 55M wedding-fund investment story; then screenshot intake → real Source Package → 1080×1080 carousel → Chrome verification. Continue high-volume discovery with exact title+URL mandatory and URL-less leads quarantined C0.

---

## 원본 기록: 071-sol.md

### 071-sol — direct Korean lead reverification

Updated: 2026-09-16 03:16 KST

Read Threads current handoff, repo tip/recent commits and `070-sol.md` before work.

#### Material work
Re-observed 5 high-priority quarantined Korean leads through permitted public/index access. Three now have exact direct community URLs and current observation snapshots, so they were promoted from legacy C0 quarantine into a new C1 evidence batch.

Threads files:
- `data/260916_C1_A0_P0_reverified-korean-leads_0316.json`
- `data/260916_C1_A0_P0_ranked-backlog_0316.md`

##### Direct C1
1. TheQoo `난 아직도 내 결혼식을 뛰어넘는 사진을 본적이 없어.jpg (빡침주의)` — https://theqoo.net/square/4341458823 — observed 124,540 views / 528 comments; 3 page images visible; original Threads URL still not verified.
2. Blind `민원인이 400만원 빌려달래` — https://www.teamblind.com/kr/post/%EB%AF%BC%EC%9B%90%EC%9D%B8%EC%9D%B4-400%EB%A7%8C%EC%9B%90-%EB%B9%8C%EB%A0%A4%EB%8B%AC%EB%9E%98-5veokcic — observed display 37K views / 21 likes / 806 comments; body and public top-comment thread inspected.
3. Blind `걸스나잇 했는데 나빼고 다 식중독 걸림...` — https://www.teamblind.com/kr/post/%EA%B1%B8%EC%8A%A4%EB%82%98%EC%9E%87-%ED%96%88%EB%8A%94%EB%8D%B0-%EB%82%98%EB%B9%BC%EA%B3%A0-%EB%8B%A4-%EC%8B%9D%EC%A4%91%EB%8F%85-%EA%B1%B8%EB%A6%BC-mt2n16mc — observed display 16K views / 15 likes / 78 comments; body and public top comments inspected.

Direct original for the wedding-fund 55M stock-loss Blind story and exact Ppomppu `재산세 2천나왔네요` URL were not verified, so no direct-source promotion was fabricated.

#### State / asset truth
All newly promoted records remain `C1_A0_P0`, publicationAllowed=false, rights UNKNOWN, privacy/human review required. Source pages/images were observed, but no screenshot/image file was actually acquired into Source Package. No A1 set, browser render, OCR/moderation result or publication is claimed.

#### Counts
Raw priority leads inspected: 5. Newly direct-source retained/promoted: 3. This run was a targeted provenance-repair run, not a claimed 40–80 fresh sweep.

#### Threads commits
- `912e87da0d20f44ed65dbcdea9cb19c65b37fd86`
- `6518408cf34073d7a3652c0d20a369a3623cf097`
- `79632695ad9acd2f230ee9aa91393f3d774de0ed`

#### Next
TheQoo wedding-photo lead is strongest visually but is a repost and says `출처 스레드`; verify original Threads source if publicly available before rights assumptions. Blind 400만원 has strongest direct discussion arc for permitted/manual/user screenshot intake. Once a real permitted screenshot set exists: intake → Source Package → 1080×1080 carousel → Chrome verification → only then A1. Continue high-volume Korean discovery with exact title+URL mandatory.

---

## 원본 기록: 072-sol.md

### 072-sol — 03:33 exact-URL Discovery sweep

Updated: 2026-09-16 KST

#### Work
Fresh public/index Discovery across Korean-community-first queries plus Reddit/YouTube expansion. No restricted-source login/anti-bot bypass or bulk crawling.

Threads added `data/260916_C1_A0_P0_discovery_0333.json`.
- raw inspected: 31
- retained exact-URL candidates: 10
- rejected/deprioritized: 21
- all retained: C1_A0_P0, publicationAllowed=false, rights UNKNOWN, human review required

Top stories: Blind `결혼 한달 남았는데 파혼..` (1,846 views/53 comments observed), Blind `파혼 해야 할까요..` (1,043/45), Blind `결혼 돈 문제` (201/5), Blind `예단은 어느정도 해야해?` (309/21). Current visual leads: Inven `여친이 삐졌을때 하지말아야하는 행동` (5,573 views/2 recs/11 comments) and `당신의 첫 휴대폰은 이 중에 몇 번?` (4,859/2/55), both with source-page images visibly observed but not captured and rights still UNKNOWN.

Excluded rather than monetizing: self-harm/death-wish content, sexual/body-focused content, ordinary stock-tax question, generic FIRE advice, repetitive politics. Investment lane remains major human consequence only, not ordinary market news.

#### Truth
No asset file was acquired, no OCR/moderation was run, no carousel was generated/Chrome-verified, no publication occurred. A1/P1 absent. Runtime code unchanged, so no npm/browser/ffmpeg result is claimed.

Threads commits:
- `9fb12cf9774923ff5869d29aefe173ec7adacf5a` discovery batch
- `2cd02e7dbc49bf863716c17a44191da1ed479075` handoff

#### Next
Continue broader Korean-source coverage without padding, then prioritize lawful/permission-safe acquisition of one visual C1 source and complete screenshot intake → Source Package → 1080×1080 source-backed carousel → actual Chrome verification. Only 04 may publish.

---

## 원본 기록: 073-sol.md

### 073-sol — 04:19 story-first Discovery expansion

Updated: 2026-09-16 KST

Baseline Threads tip: `2cd02e7dbc49bf863716c17a44191da1ed479075`. Read handoff + recent commits + `072-sol.md` before work.

#### Work
Fresh public/index discovery across Korean-community-first searches. No login/anti-bot bypass or restricted bulk crawl. Added `data/260916_C1_A0_P0_discovery_0419.json` and refreshed the verified ranked backlog.

Raw inspected: **28**. Newly retained exact-URL: **8**. Rejected/deprioritized: **20**. Search indexing became repetitive/sparse before the aspirational 40–80 target, so weak duplicates were not padded.

Notable additions:
- TheQoo `누나가 남친 옥바라지 중이라 답답하다.JPG` — exact URL `https://theqoo.net/square/4143320857`; 04:19 indexed snapshot 91,179 views / 277 comments / 6 images. Another indexed rendering had different older metrics, so snapshots were not merged.
- TheQoo `극호 반응으로 난리난 결혼 답례품.jpg` — `https://theqoo.net/square/4326443251`; 28,797 views / 368 comments / 5 images observed.
- Inven `돈 안 내는 여친` — `https://www.inven.co.kr/board/webzine/2097/2696095?iskin=webzine`; 3,727 views / 2 recs / 16 comments; body read; image visible but not captured.
- Blind `빚있는데 결혼준비..` — exact public URL; 657 views / 1 like / 9 comments; body/comments read. Stock loss → **1억 debt** → wedding in two years. Correct investment-human-consequence fit.
- Blind `빚 그리고 결혼 어떻게해야할까` — 1,024 views / 4 comments; crypto/stocks ~5천 loss + 9천 debt + fear of telling partner before marriage.
- Blind `결혼` — 719 views / 2 likes / 3 comments; futures debt, several hundred million reportedly repaid, 8천 debt remains, marriage decision.
- Ruliweb `결혼 전 주식 매도` — 4,263 views; principal 3.3억 + about 1억 gain, wedding-cost liquidation dilemma; ranked below actual disaster stories.

Ordinary market/stock/rate summaries, generic portfolio/FIRE questions, removed Reddit content, sexual-title content and weak celebrity/recycled items were not promoted.

#### State truth
All new records are `C1_A0_P0`, `publicationAllowed=false`, rights UNKNOWN and human review required. No actual screenshot/image file acquired, no OCR/moderation, no carousel, no Chrome verification and no publication. A1/P1 absent.

#### Commits
- `1ae40847a2dfe3555ae8d9a7e9613be0efcf73b0` discovery batch
- `11f00b51f35c95f37844db5452766ae4401cb2d7` ranked backlog refresh
- `7ccbb7c71003a73af2bd7d0a997dd7e4b51249bb` handoff

Runtime code unchanged, so npm/browser/ffmpeg tests were not rerun or claimed.

#### Next
Prioritize compliant real-asset provenance/capture rather than another text-only run if possible: TheQoo wedding-photo original Threads provenance, Blind 400만원 screenshot/manual intake, TheQoo 옥바라지 provenance, or Inven money/dating image. Then screenshot intake → Source Package → genuine 1080×1080 carousel → Chrome verify → A1 only after actual generation. P1 remains 04-only.

---

## 원본 기록: 074-sol.md

### 074-sol — 04:37 exact-URL Discovery sweep

Updated: 2026-09-16 KST

Baseline Threads tip: `7ccbb7c71003a73af2bd7d0a997dd7e4b51249bb`. Read handoff/recent commits/`073-sol.md` before work.

#### Work
Public/index search screened **36 result entries** across Korean-community-first queries plus Reddit expansion. Coverage became stale/repetitive in several lanes, so weak results were not padded to 40–80. Retained **10 exact-URL C1 candidates** in `data/260916_C1_A0_P0_discovery_0437.json` and refreshed the ranked backlog to top 20.

Strongest addition is TheQoo `임원이 사온 호텔케이크 뭉개버린 남직원 글에 달린 댓글 일화가 더 충격적임` (`https://theqoo.net/square/4022365750`): observed 99,239 views / 904 comments; body/comments read. The story escalates through multiple workplace anecdotes and has strong carousel structure, but original X/linked-post provenance and rights remain unresolved.

Other retained candidates include Blind `결혼 한달 남았는데 파혼..` (1,846/53; 1억 전세사기 debt + wedding conflict), Inven `여친 아파서 지각한다는 신입` (14,095/18; image visible), Inven `여친 때문에 롤토체스 승급전 진 남친` (3,855/15; six images visible), Blind `돈 없는 남자와 결혼..` (1,019/34), and investment-human-consequence Blind `긍정의힘이란게 정말 있을까` (65/15; land investment reportedly left family struggling with debt; no loss amount invented).

Removed Reddit posts, sexual/body-focused items, ordinary market/rate/stock discussion, weak generic advice/dating questions, celebrity/news without progression and duplicate/recycled search results were not promoted.

#### State truth
All retained: `C1_A0_P0`, `publicationAllowed=false`, rights UNKNOWN, privacy/human review required. No actual screenshot/image file acquired, no OCR/moderation, no carousel, no Chrome verification, no publication. A1/P1 absent.

#### Threads commits
- `4bdba0fb91c5b3c82d8c1040cc7158ac840b5fd5`
- `9fd31b40114b97a8ca644c3ec830b01fcf254c40`
- `d1646b18e4409605e6bd47ecbbba34ae0028cd49`

Runtime code unchanged; no npm/browser/ffmpeg result claimed.

#### Next
Prioritize actual asset provenance/capture: TheQoo wedding-photo original Threads source; TheQoo hotel-cake original X/linked posts; Blind 400만원 compliant screenshot/manual intake; Inven visual candidates original provenance. Then screenshot intake → Source Package → real 1080×1080 carousel → Chrome verify → A1 only after actual generation. P1 remains 04-only.

---

## 원본 기록: 075-sol.md

### 075-sol — hotel-cake provenance advancement

Updated: 2026-09-16 KST

Baseline Threads tip: `d1646b18e4409605e6bd47ecbbba34ae0028cd49`. Read current handoff/recent commits/`074-sol.md` before work.

#### Material work
Followed the prior acquisition priority rather than adding another shallow text-only batch. Added `data/260916_C1_A0_P0_hotel-cake-provenance_0516.json`.

Verified public provenance chain:
- TheQoo compilation `임원이 사온 호텔케이크 뭉개버린 남직원 글에 달린 댓글 일화가 더 충격적임` — https://theqoo.net/square/4022365750 — observed this run: 99,239 views / 902 comments.
- Predecessor TheQoo `뒷일을 생각안하고 이상한짓 하는 남직원 너무 많음..` — https://theqoo.net/square/4021537698 — observed: 73,393 views / 274 comments.
- Exact X status explicitly linked by both: https://x.com/yuwisk/status/1997967558398628028
- Exact follow-up X status explicitly linked by predecessor: https://x.com/yuwisk/status/1998002501010989169

Direct X fetch was attempted but unavailable, so no X body/media/metrics/screenshot is claimed. Candidate remains C1_A0_P0 / ASSETS_PENDING / publicationAllowed=false / rights UNKNOWN.

#### Safety boundary discovered
The long public comment thread also contains severe animal-cruelty/death anecdotes and a toxic-herbicide threat anecdote. These are explicitly excluded from production. Safe story progression is limited to non-graphic workplace sabotage anecdotes such as coffee/hand-cream/shared food/gift damage and passport hiding.

#### Counts
Focused provenance run: 4 targeted public/index searches plus direct opens of the current TheQoo page, predecessor page and exact X URL. New candidates retained: 0. Existing top candidate with materially improved provenance: 1.

#### Threads commits
- `d06d1aca513e1cf6c57b7c6a37e61d2fcef9da62` — provenance record
- `5f84204592cbfbd677c9f8dd907b93cd669a0a3c` — handoff

#### Tests/result truth
Runtime code unchanged. No npm/browser/ffmpeg test claimed. No real screenshot/image file acquired, no OCR/moderation run, no carousel, no Chrome visual verification, no publication. A1/P1 absent.

#### Next
Authorized permitted browser/manual capture of the two exact X URLs is the cleanest next acquisition step. If inaccessible, move to Blind `민원인이 400만원 빌려달래` compliant screenshot intake or Inven visual-source provenance. Only actual acquired source assets may move toward A1; only 04 after actual publication success may set P1.

---

## 원본 기록: 076-sol.md

### 076-sol — 05:35 public Discovery sweep

Updated: 2026-09-16 KST

Read current Threads handoff and `075-sol.md` first. Repo tip remained authoritative.

#### Work
Fresh public/index search across Korean community/workplace/marriage/investing/humor lanes plus Reddit/YouTube coverage. Coverage was noisy and below the desired high-volume range, so stale/irrelevant results were not padded into candidates.

Threads added `data/260916_C1_A0_P0_discovery_0535.json`.

Counts:
- raw materially inspected: 14
- retained exact-URL candidates: 2

Retained:
1. Blind `결혼 한달 남았는데 파혼..` — https://www.teamblind.com/kr/post/%EA%B2%B0%ED%98%BC-%ED%95%9C%EB%8B%AC-%EB%82%A8%EC%95%98%EB%8A%94%EB%8D%B0-%ED%8C%8C%ED%98%BC-8b4ol727 — observed 1,846 views / 53 comments; body read, comments not claimed read.
2. YouTube Shorts `한국 집엔 무조건 있는데 브라질은 부자집에만 있는 것` — https://www.youtube.com/watch?v=gxkXhfuDtT4 — published 2025-11-30; observed 267,110 views / 1,300 likes. Lower priority because it is older and reuse rights are unknown.

Excluded sexualized content, repetitive politics, stale generic compensation discussion and irrelevant Reddit developer/tool search noise.

#### Truth/gates
No source screenshot/image file acquired; no OCR/moderation; no A1; no P1; no publication. New records remain publicationAllowed=false with rights/privacy/human-review gates.

#### Threads commits
- `f1e419eac51d18d8187712c3ae087c6bfdc232d8` — discovery data
- `8e39aa169d892f76d23ea527b5da2731355a96ea` — handoff

Runtime code unchanged, so npm/browser/ffmpeg tests were not rerun or claimed.

#### Next
Continue Korean-community breadth when coverage improves, but asset acquisition remains urgent: permitted capture for the TheQoo/X hotel-cake chain or Blind 400만원 candidate, then screenshot intake → Source Package → real 1080×1080 carousel → Chrome verification. Only actual generated source-backed output can become A1; only 04 can verify P1.

---

## 원본 기록: 077-sol.md

### 077-sol — 06:17 verified Discovery expansion

Updated: 2026-09-16 KST

Read Threads `NEXT_RUN_HANDOFF.md`, current recent commits and `076-sol.md` before work. Repo tip was authoritative.

#### Work
Fresh public/index discovery across Korean relationship/workplace/money/investment/humor lanes, plus TheQoo/Inven/Reddit coverage. Search quality still did not justify padding to 40–80 with removed/noisy/sexualized/political/generic items.

Threads changes:
- added `data/260916_C1_A0_P0_discovery_0617.json`
- refreshed `data/260916_C1_A0_P0_ranked-backlog_0316.md`
- updated `00_START_HERE/NEXT_RUN_HANDOFF.md`

Counts:
- raw materially inspected: **31**
- retained exact-URL candidates: **10**

#### Notable new verified candidates
1. Blind `혼전임신 후 알게된 남친의 빚 5억과 코인 중독 상황. 도와주세요.` — 14K views / 161 comments in the observed public snapshot; body read. Strong investment-disaster + relationship escalation; high privacy/human-review sensitivity.
2. Blind `결혼을 앞둔 남자친구의 주식 빚 숨겨줘야할까?` — 4,054 views / 4 likes / 86 comments; body and visible comments read. 5천 stock debt, second investment accident, asks partner to refinance in her name.
3. Blind `30초 남자 빚 오픈` — 205 views / 9 comments; body read. ~1억 stock/crypto credit-line debt despite 6억대 home; lower reach but strong contrast.
4. Blind `결혼 준비 중인데 신뢰가 무너져서 고민입니다` — 439 views / 13 comments; body read; privacy/defamation-sensitive.
5. TheQoo `남편 아이 없이 친구들이랑 핵노잼여행` — 48 views / 0 comments; 2 images and exact upstream Instagram URL visible. Low content rank, but useful potential source-first acquisition test. Upstream Instagram itself was not verified.

No ordinary market/rate/stock-news candidate was retained. Reddit results were mostly removed posts and excluded. Sexualized Inven and repetitive political material were excluded.

#### Truth/gates
No source screenshot/image file acquired. No OCR/moderation. No A1/P1. No publication. All candidates remain `publicationAllowed=false`, rights UNKNOWN/REVIEW and privacy/human-review gated.

#### Threads commits
- `2087128921299f60b3308a246f7505a81d10bf21`
- `d72df147ad3ee57c003598ee18826b7118a73b7d`
- `d640cf085e89c5271520c286a4c2fa5cec55dc53`

Runtime code unchanged; npm/browser/ffmpeg tests were not rerun or claimed.

#### Next
Asset acquisition should outrank another small text-only batch: permitted provenance/capture for TheQoo wedding-photo, hotel-cake/X chain, Blind 400만원 or six-image 옥바라지 story. Also test the exposed Instagram upstream URL only through permitted public tooling. Once actual screenshots exist: intake → Source Package → 1080×1080 carousel → Chrome visual/dimension verification. Only actual output becomes A1; only 04 after real publication can set P1.

---

## 원본 기록: 078-sol.md

### 078-sol — 06:38 public Discovery sweep

Updated: 2026-09-16 KST

Read current Threads handoff, repo tip/recent commits and `077-sol.md` first. Repo tip was authoritative.

#### Work
Fresh public/index sweep across Korean workplace/dating/humor/investment lanes plus TheQoo/Inven/Ppomppu/Reddit/YouTube/X/Threads search surfaces.

- raw materially inspected: **42**
- retained: **8**
- Threads data: `data/260916_C1_A0_P0_discovery_0638.json`
- ordinary market/rate/stock news excluded
- weak generic career, political, sexualized, removed/stale and duplicate results were not padded into retained output

Notable retained: Blind `여자친구 직장 간섭, 제가 너무 많은걸 바라나요?`, `남자친구의 여자동기들`, `내 신고로 팀장이 짤릴거같아`; Inven `회사 언니가 남친과 헤어진 이유`, `당신의 첫 휴대폰은 이 중에 몇 번?`; TheQoo `직장인 공감: 개까르잠 자고 일어나면 지각이다` with exact upstream X URL exposed; Blind index-backed crypto-investor 'two painful mistakes' lead.

Important metric rule: the direct Blind girlfriend-workplace post snapshot returned 104 views/16 comments while other public index snapshots showed larger later counts. They were not merged. Each retained record keeps only its stated observation snapshot.

#### Truth/gates
No source screenshot/image file acquired. No OCR/moderation. No A1/P1. No publication. `publicationAllowed=false`; rights UNKNOWN/REVIEW; privacy/human review retained.

#### Threads commits
- `498c92ec1266418eed0a1d2ea7e9aa4f1ad29067` — discovery batch
- `180346fb4a172dc909abd88839a8267fbf789bb9` — handoff

Runtime code unchanged; no npm/browser/ffmpeg test claimed.

#### Next
Asset acquisition should outrank another small text-only batch. Pursue permitted screenshots/media for the wedding-photo, hotel-cake/X, Blind 400만원, six-image 옥바라지 or Inven company-sister breakup candidate. Verify exposed upstream X only through permitted public tooling. Find the individual URL for the crypto-investor mistakes post before body verification. Real screenshot → intake → Source Package → 1080×1080 carousel → Chrome verify is the A1 path; only 04 after real publication can create P1.

---

## 원본 기록: 079-sol.md

### 079-sol — direct public asset acquisition path advanced

Updated: 2026-09-16 07:18 KST

Read current Threads handoff, repo tip/recent commits and `078-sol.md` first. Repo tip was authoritative.

#### Material work
Did not spend this run on another small text-only batch. Re-verified ranked candidate `누나가 남친 옥바라지 중이라 답답하다` on direct DCInside realtime-best page and followed the public attachment links.

Direct single snapshot:
- `https://gall.dcinside.com/board/view/?id=dcbest&no=416863`
- 39,286 views / 372 comments displayed / 70 recommendations / 92 non-recommendations
- six WEBP original attachment filenames exposed
- all six public attachment links resolved through permitted navigation
- page states source is 기타 국내 드라마 갤러리 and exposes original-view link

TheQoo same-story repost `https://theqoo.net/square/4143320857` also exposes the same six-image sequence. Metrics were not merged across snapshots.

Threads data: `data/260916_C1_A0_P0_asset-acquisition_0718.json`.

#### Truth
This advanced from vague ASSETS_PENDING to an exact acquisition path, but image bytes were NOT captured into Source Package. Therefore still `C1_A0_P0 / ASSETS_PENDING`; A1=0, P1=0. No OCR, moderation or rights clearance claimed. Comments not read. Privacy/PII inside images still requires visual review. `publicationAllowed=false`; human review required.

Fresh Discovery raw/retained counts for this acquisition-focused run: **0 / 0**. Exact source attachments located/resolved: **6**. Actual source files captured: **0**.

#### Threads commits
- `3837263ad6608a93add7b0e7830c8bf60cfcfa14` — acquisition record
- `e1d615e41f1cca3a726fdedc85af499ee9d22797` — handoff

Runtime code unchanged; no npm/app E2E/ffmpeg result claimed. Public source/attachment navigation was actually performed.

#### Next
Capture the six attachment bytes through authorized browser/remote or permitted binary path, preserve order/provenance, visually inspect PII/safety/story continuity, then real screenshot intake → Source Package → 1080x1080 carousel → Chrome verify. Only then A1. If binary capture remains blocked, pursue TheQoo wedding-photo or hotel-cake/X exact assets. Only 04 after real publication can create P1.

---

## 원본 기록: 080-sol.md

### 080-sol — 08:15 source-first Discovery refresh

Updated: 2026-09-16 KST

Read current Threads handoff, repo tip/recent commits and `079-sol.md`. Repo tip was ahead of the old handoff: `c830688c` had already added the 07:36 Discovery refresh, so repo tip won.

#### Material work
Fresh public/index search across Korean community + Reddit lanes. Search coverage was weaker than the desired 40–80 target, so no padding:
- raw materially inspected: **24**
- retained: **5**
- Threads data: `data/260916_C1_A0_P0_discovery_0815.json`

Top retained:
- Blind `배우자의 빚` — 178 views / 15 comments observed; body + visible comments read. Hidden pre-marriage stock debt 30M KRW → rollover → 60M KRW, strong investment-disaster/marriage-trust story.
- Blind `결혼 전 청약된 집 빚 갚는 문제` — 125 / 9; body/comments read; premarital apartment + parental money + joint/separate finances conflict.
- Blind `상대방 부모님 빚.. 결혼 괜찮을까요?` — 678 / 11; body/comments read; 3–4B KRW parental business debt disclosed during marriage discussion.
- TheQoo `영어 섞어쓰는 장도연 AI남친 보고 바로 욕하는 기안84 AI여친` — selected observation 3,039 / 33; exact upstream YouTube Shorts URL exposed; original-video metrics not claimed.
- Reddit lottery/family-numbers argument — observed score 2,120; exact winnings absent, so no amount invented.

Dry market/career/political/sexual/removed results were excluded. Metrics remain single observation snapshots and are not merged across reposts/times.

#### Asset / production truth
Existing DCInside `누나가 남친 옥바라지 중이라 답답하다` remains best exact-asset target with six ordered public WEBP links already resolved in the previous run. Actual image bytes are still not captured into Source Package.

Remote Desktop Commander was attempted for local binary capture/E2E but no authorized device was online. Therefore no local browser, npm, server, ffmpeg, OCR or app-E2E result is claimed.

Completed real source-backed user-facing set: **NO**. Source files captured: **0**. A1=0, P1=0. All new records remain `C1_A0_P0 / ASSETS_PENDING`, `publicationAllowed=false`; rights UNKNOWN/REVIEW, privacy/human review required.

#### Threads commits
- `58fa8fe5b9e9015b6ba984c56ab10bd71be23555` — fresh Discovery batch
- `f73082c008cb8bb1a4d13c5b9b66ef1dc88f749c` — handoff reconciliation/update

#### Next
Capture the six DC WEBP bytes through authorized browser/remote or another permitted binary path, preserve order/provenance, visually review PII/safety, then real screenshot intake → Source Package → 1080×1080 carousel → Chrome verify. If capture remains blocked, continue Korean-first high-volume Discovery toward 40–80 raw / 15–30 retained, favoring direct source assets/upstream URLs. Only 04 may publish; P0 remains until real publication is observed after gates.

---

## 원본 기록: 081-sol.md

### 081-sol — 09:17 source-first Discovery refresh

Updated: 2026-09-16 KST

Read current Threads handoff, current main/recent commits and `080-sol.md`; repo tip won.

#### Material work
Fresh public/index Korean-first discovery. **43 raw materially inspected / 8 retained**. No restricted-source bulk crawl or access-control bypass. Sexual, graphic animal-abuse, political and dry market-summary results were excluded rather than padding the batch.

Threads data: `data/260916_C1_A0_P0_discovery_0917.json`

Top acquisition-friendly retained candidates:
- Inven `내 방 달라는 딸 대처법.jpg` — observed 18,490 views / 26 recommendations / 31 comments; body read; real source image link visible.
- Inven `오늘 이혼도장 찍었다..나처럼 실패하지 말아라.` — 22,520 / 32 / 72; body read; multiple source-image links visible; privacy/defamation review required.
- Inven `남편 도시락 싸주는 유부녀의 평소 저녁밥 수준..` — 20,501 / 27 / 31; body read; real source images visible.
- Inven `분당 헬스장에서 포착된 의문의 남성` — 21,697 / 25 / 41; body read; source image visible; identifiable-person privacy review mandatory.
- Inven `남친이 헤어지자고 하자 울어버리는 여친` — 7,042 views / 14 comments from public result; body/assets not yet verified.
- Inven `낙수효과를 진짜 실철한 사장님` — 6,287 / 13 / 22; body read; many source images visible; upstream factual/provenance check required.
- Inven index `저도 울 와이프가 싸준 도시락` — 8,498 / 40 / 49; direct post still unresolved.
- Inven index `MZ신입이랑 같이 일한지 9개월차 느낀점` — 14,217 / 14 / 46; direct body needs verification.

Metrics remain single observation snapshots; no cross-time/repost merge.

#### Asset / production truth
Existing DCInside six-WEBP `누나가 남친 옥바라지 중이라 답답하다` remains the primary exact-asset target. This run additionally verified several Inven candidates with directly exposed real source-image links, providing compliant fallback acquisition paths.

Actual bytes captured into Source Package this run: **0**. Completed real source-backed user-facing set: **NO**. A1=0, P1=0. All new records remain `C1_A0_P0 / ASSETS_PENDING`, `publicationAllowed=false`; rights UNKNOWN/REVIEW and privacy/human review required. No OCR/moderation/browser E2E/npm/ffmpeg result claimed.

#### Threads commits
- `f71d776022d601091a966111359ebff659dc8ecc` — 09:17 Discovery batch
- `496153ff1888b837e2ba5d3a60efaa888fd71d88` — handoff update

#### Next
Acquire actual source bytes (DC six-WEBP first; direct Inven image links as fallback), visually review PII/safety/provenance, build Source Package, then real 1080×1080 carousel and Chrome verification. Continue Korean-first high-volume Discovery when acquisition tooling remains unavailable. Only 04 may publish and P0 remains until all gates pass and publication is actually observed.

---

## 원본 기록: 082-sol.md

### 082-sol — 10:37 Korean-community Discovery refresh

Updated: 2026-09-16 KST

Read current Threads `00_START_HERE/README.md`, `NEXT_RUN_HANDOFF.md`, current main/recent commits and prior sequential note before work; repo tip won. Applied the newly simplified source format `글 / 이미지 / 이미지 포스팅`.

#### Material work
Fresh public/index Korean-community discovery with no restricted-source bulk crawling or access-control bypass. **52 raw materially inspected / 20 retained candidate files created** after dedupe and exclusion of clearly unsuitable sexual/graphic/political/dry-news material.

All retained items were stored individually under `data/candidates/`; no grouped discovery JSON was created in `data/` root.

Strong exact-page C1 additions include:
- Inven `입주 청소하러 갔다가` — 7,339 views / 19 rec / 8 comments; image-post core; source bytes pending.
- `마약사범에게 뇌물 받은 경찰의 충격적인 반전..` — 3,949 / 22 / 14; current hot-list reversal; image text not OCRed.
- `ㅈ소기업 리뷰이벤트 근황` — 4,928 / 1 / 28; workplace absurdity, body read.
- `회사에서 뒷담화를 끊게 된 계기` — 4,193 / 5 / 9; two-image workplace story, image text not transcribed.
- `KTX혼자서 두자리 예매?` — 4,203 / 0 / 38; everyday etiquette debate.
- `결혼하면 은근히 의견 갈린다는 돈관리 유형` — 3,983 / 1 / 23; marriage/money debate.
- `배우 신현준이 페루 아이돌이 된 이유` — 5,809 / 2 / 6; body read, images + upstream links.
- `곤충친구가 생긴 식물갤러` — 2,520 / 8 / 13; light visual surprise.
- `북한에서 아이스크림을 부르는 말` — 2,712 / 1 / 9; ordered four-image joke.
- `고출력자동차 첨타면 벌어지는일` — 4,184 / 3 / 18; visual safety review pending.

Five public-index-only C0 leads were also preserved rather than falsely promoted: `결혼식 ... 5만원`, `콜센터 직원이 느낀 진상 손님 직업`, `결혼 전 vs 결혼 후`, `한국 전철 ... 일본인`, `이시대를 사는 4050 특징`.

#### Asset / production truth
Full-post screenshots captured: **0**.
Source bytes copied to Source Package: **0**.
Real source-backed carousel produced: **NO**.
A1=0, P1=0 for new items. `publicationAllowed=false`; rights remain unverified. No OCR/moderation/publication success claimed. Where source image text was not actually read, candidate files explicitly state that instead of inventing the body.

#### Threads repo
Handoff updated at commit `ede60a834f2c300dbd2082f253a3f0f4842ef8ce` after individual candidate commits.

#### Next
Resolve exact URLs for strongest C0 leads, then acquire ordered real source screenshots/images for the strongest C1 items. Continue broad Korean-community coverage beyond Inven when public exact-page access allows. Preserve full original body and source order; only 04 may publish.

---

## 원본 기록: 083-sol.md

### 083-sol — Source Package runtime correction

Updated: 2026-09-16 KST

Repo tip and 082-sol were read before work. Runtime `app/source-package.js` was behind the binding production contract, so it was corrected: slide 1 cover + original title; slide 2+ ordered original post screenshots/media only; source format only `글 / 이미지 / 이미지 포스팅`; explicit full-body capture status; incomplete capture remains assets pending; UI crop may remove chrome but not body/media; automatic CTA/reaction/context overlays and PII mask suggestions were removed. `publicationAllowed=false`; only 04 may publish.

Verification: targeted `node test/source-package.test.mjs` PASS on the authorized local machine. Full `npm run check` is not claimed green because the local working tree had a pre-existing uncommitted stale field-test assertion; repo-tip assertion was corrected in `400a734e`. Browser E2E not run because no rendered output was produced.

Discovery inherited: 52 raw / 20 retained. Full-post screenshots captured this run: 0. Real source-backed carousel: NO. No OCR/moderation/rights/publication success claimed.

Threads commits: `011179f7`, `739c2daf`, `400a734e`, handoff `5c7547c1`.

Next: acquire complete ordered real screenshots for a strong C1, build first 1080×1080 cover + faithful full-post screenshot carousel, verify in Chrome, and continue Korean-community discovery. Keep P0 until 04 confirms actual approved publication.

---

## 원본 기록: 084-sol.md

### 084-sol — 11:36 Korean-community Discovery refresh

Updated: 2026-09-16 KST

Read current Threads START_HERE, NEXT_RUN_HANDOFF, repo tip/recent commits and 083-sol before work. Repo tip remained authoritative.

Discovery materially screened roughly 73 public leads/list entries across current/recent community and search coverage without restricted-source bulk crawling or access-control bypass. 15 new C1 candidates were retained as individual Markdown files in `data/candidates/`; no grouped root discovery JSON was created.

Top new candidates: `대기업 다닌다던 남편이 고졸이었어` (4,292 views / 1 rec / 37 comments), `남편 비상금 발견했어요....` (14,908 / 26 / 40), `12년차 차장입니다. 회사에서 넵 쓰지마세요` (8,878 / 1 / 33), `길에서 핸드폰 빌려줬더니 온 카톡` (7,606 / 0 / 26), `초봉이 4700만원이라는 선박 승무원.` (1,304 / 0 / 9), `산길 몰려온 러닝족에…등산객 ‘부글부글’` (1,134 / 0 / 15), `암표로 수십억원 번 30대 암표상` (1,536 / 0 / 7). Metrics are same-observation snapshots and were not merged with later refreshes.

Image-centric pages whose actual image body was not readable through the public text path are explicitly marked `이미지 본문 미확인`; no content was invented. Source format is only `글 / 이미지 / 이미지 포스팅`.

Full-post screenshots captured: 0. Actual source bytes acquired: 0. Real source-backed carousel: NO. New candidates remain C1_A0_P0 / ASSETS_PENDING / publicationAllowed=false. No OCR, moderation, rights approval or publication success was claimed.

Threads handoff commit: `4d34464a`. Next priority is complete ordered screenshot acquisition for the strongest image-post candidates, then faithful 1080×1080 carousel + Chrome verification. Only 04 may publish.

---

## 원본 기록: 085-sol.md

### 085-sol — faithful screenshot intake runtime fix

Updated: 2026-09-16 KST

Read current Threads NEXT_RUN_HANDOFF, current main/recent commits, and latest 084-sol first. Repo tip remained authoritative.

This run found a concrete runtime mismatch: `app/source-package.js` already enforced the new cover + original-screenshot contract, but `app/source-intake.js` still generated the old editorial carousel with custom hook, continuation/comment kinds and a final CTA card.

Fixed `app/source-intake.js` so asset #1 is cover-only, asset #2 onward is only original `post`/`media`, cover text uses the candidate's original title, and preview/export no longer injects reaction/summary/CTA cards. PNG count now equals cover + actual source sequence. Body images use contain-style rendering without rewritten text overlays. Vision/OCR remain false unless actually executed; no automatic privacy masking was added. Export is blocked unless `fullBodyCaptureStatus=complete`.

Threads runtime commit: `00ec226f85cbaeea3297ab6b6fe660653d856edf`.
Handoff commit: `646c11e37ebe7aeba64119a5496e59bde313b861`.

Discovery state inherited: ~73 raw screened / 15 retained. Full-post screenshots captured this development run: 0. Actual source bytes acquired: 0. Real source-backed carousel: NO. A1=0, P1=0.

Verification: attempted a local clone for syntax/test execution, but the execution environment could not resolve github.com. Therefore no npm/check/server/browser success is claimed.

Next: expose/verify an explicit `complete / partial / pending` browser control, then acquire one candidate's entire ordered post and produce/Chrome-check the first real 1080×1080 cover + full-post carousel. Only 04 may publish.

---

## 원본 기록: 086-sol.md

### 086-sol — 12:38 Korean-community Discovery refresh

Updated: 2026-09-16 KST

Read current Threads START_HERE, NEXT_RUN_HANDOFF, current main/recent commits and latest sequential note first. Repo tip remained authoritative.

This run materially screened ~58 distinct public leads through Korean-community-first search/index/page exploration plus adjacent public social/search lanes. No restricted-source bulk crawl, login bypass or anti-bot bypass was used. After dedupe, safety/comfort, provenance and story-potential filtering, 16 new C1 candidate Markdown files were written individually under `data/candidates/`; no grouped discovery JSON was created in `data/` root.

Top retained: `상황 진짜 위험해보이는 돌고래유괴단` (11,347 views / 2 rec / 17 comments), `20년지기 여사친 하고 결국 결혼했다` (7,318 views / 19 comments), `이시대를 사는 4050 특징` (6,229 / 4 / 16), `회사 언니가 남친과 헤어진 이유` (6,137 / 1 / 13), `여직원 원룸 구한다고해서 방내어줌` (5,133 / 4 / 26), `친구랑 낡은 집에 살았는데.jpg` (4,396 / 1 / 13), `AI 급 가속하는 발전 속도..` (4,278 / 2 / 15), `미국 고딩들이 한국 지하철 타보고 충격받은 이유.` (4,133 views / 10 comments).

Other retained C1: `남자친구에게 선물을 주는 일본인 여자친구`, `차 문 열어주는 일본인 여자친구`, `그린랜턴 새 실사판 근황`, `여친이 약속에 자꾸 늦는 이유`, `영화 선구안 지리는 할리우드 배우`, `신생아가 집에 오면 생기는 일`, `한전 사유지에 송전탑 설치`, `7,000원 돈까스 세트`.

Image-centered posts whose actual image body was not read are explicitly marked `이미지 본문 미확인`; no body, metrics, OCR, moderation, rights, asset acquisition or publication success was fabricated.

Full-post screenshots captured: 0. Actual source bytes acquired: 0. Real source-backed carousel: NO. All new records remain C1/A0/P0, `publicationAllowed=false`, with rights/privacy/human review gate preserved. Only 04 may publish.

Threads handoff commits: `ee58a628` then count-correction `f64bc205`. Next concrete priority is complete ordered screenshot acquisition for the strongest relationship/workplace candidates, then first real 1080x1080 cover + full-post carousel.

---

## 원본 기록: 087-sol.md

### 087-sol — automatic cover renderer

Updated: 2026-09-16 KST

Read current Threads NEXT_RUN_HANDOFF, current main/recent commits and latest 086-sol first. Repo tip remained authoritative.

Material production change: `app/source-intake.js` no longer requires the operator to supply a separate cover image. Every selected file is now treated as an ordered ORIGINAL POST screenshot/media asset. Slide 1 is automatically rendered at 1080×1080 from the first original asset as background plus the candidate's original title; that same source asset remains faithfully present as slide 2. N ordered source assets therefore render as N+1 PNGs.

SOURCE_PACKAGE records the cover as a locally derived cover asset rather than pretending it is an original source screenshot. Slide 2 onward remains original `post`/`media` only. No summary/reaction/CTA cards, no automatic privacy masking, and no fabricated OCR/vision state were added. Export still requires `fullBodyCaptureStatus=complete`; only 04 may publish.

Threads runtime commit: `30b5746ab6f73a89049f0f1c0831b0acce119c2b`. Handoff commit: `992adac5ec33bb07e9ad552fbc22c50150203221`.

Discovery state inherited: ~58 raw screened / 16 retained from the latest discovery refresh. Full-post screenshots captured this production run: 0. Actual source bytes acquired: 0. Real source-backed carousel: NO. A1/P1 unchanged.

Verification limitation: this run had GitHub connector access but no mounted/browser runtime, so npm/check/server/Chrome success is not claimed. Next runtime-enabled run should verify 1 source → 2 PNGs, N sources → N+1 PNGs, ordering, dimensions and complete/pending gate, then acquire one complete real source set and produce the first real carousel.

---

## 원본 기록: 088-sol.md

### 088-sol — 13:37 discovery refresh

Updated: 2026-09-16 KST

Read Threads README, current NEXT_RUN_HANDOFF, current main/recent commits and latest 087-sol first. Repo tip remained authoritative.

Discovery screened ~47 materially distinct public leads using Korean-community-first queries and public search/index/page access; no restricted-source bulk crawl or login/anti-bot bypass. Retained 15 new one-candidate-per-Markdown files under `data/candidates/`: 4 C1 and 11 C0.

C1: `당근 꿀 알바 하실분 구해용`, `결혼 전 vs 결혼 후 ㄷㄷㄷㄷㄷ`, `한중일 삼국마다 갈린다는 삼국지 최애케릭터.jpg`, `조선시대 부터 내려온 현피 전통`.

C0 index leads: `승진 누락된 차장님이 퇴사를 안해`, `인생 망한 38살인데 인생상담 해줘라...`, `41세 비혼녀, 실제로 많이 듣는 말.`, `알뜰폰 개통했는데 이상한 문자가 온 디시인`, `말 못알아듣는다고 팀장이 강제로 헤드셋을 벗겼습니다.`, `허경환이 나이 들고 제일 후회한다는 것...`, `여행 유튜버의 현실`, `팁을 거절하는 종업원`, `아들 여친을 본 엄마 표정.mp4`, `연돈카레 제보한 디시인`, `얼마예요 묻자 거지취급하는 업체`.

`당근 꿀 알바` observation: 3,328 views / 0 rec / 14 comments, three images indicated; image body not read. `결혼 전 vs 결혼 후`: 10 comments / 5 rec visible, short text punchline read, two images indicated; same-observation view count not reliably visible and therefore omitted.

Full-post screenshots captured: 0. Actual source bytes acquired: 0. Real source-backed carousel: NO. New candidates remain A0/P0 and publicationAllowed=false. No OCR/moderation/rights/publication success claimed.

Threads handoff commit: `afd1a575b9d27e47b2474f5c95943ac157170da3`.

Next: resolve strongest C0s to exact individual public URLs, acquire one complete ordered source-image set, then produce and Chrome-check the first real auto-cover + full-post carousel.

---

## 원본 기록: 089-sol.md

### 089-sol — tall screenshot readability fix

Updated: 2026-09-16 KST

Read current Threads NEXT_RUN_HANDOFF, current main/recent commits, and latest 088-sol first. Repo tip remained authoritative.

Material production change: `app/source-intake.js` previously fit each original screenshot into a single 1080×1080 slide. For tall full-post captures this preserved all pixels but could shrink the actual post text until it was impractical to read. The PNG exporter now treats `post` screenshots differently from attached `media`: tall post screenshots are sliced top-to-bottom at source-width scale across as many sequential square slides as required, while media remains contain-rendered. The first cover is still derived locally from the first real source asset + original title. No summary/rewrite/CTA/privacy masking was added. Export still requires `fullBodyCaptureStatus=complete`.

Threads runtime commit: `1816c02ebd45c2023b2c0124fc03646ae170acc9`.
Threads handoff commit: `af4b234e02ab59551be85e8bf28fcdd3a6dcf275`.

Discovery state inherited from the immediately preceding refresh: ~47 raw screened / 15 retained (4 C1 + 11 C0). Top newly verified C1 remains `당근 꿀 알바 하실분 구해용` (3,328 views / 0 rec / 14 comments at its recorded observation; three images indicated but bytes not acquired).

Full-post screenshots captured this run: 0. Actual source bytes acquired: 0. Real source-backed carousel: NO. A1=0, P1=0, publicationAllowed=false. No OCR/moderation/rights/publication success claimed.

Verification: GitHub update succeeded. No checked-out runtime/browser session was available in this run, so npm/check/server/Chrome E2E success is not claimed.

Next: acquire one complete ordered real source set, render it through auto-cover + tall-post slicing, then Chrome-check the first real source-backed carousel. Only 04 may publish.

---

## 원본 기록: 090-sol.md

### 090-sol — 14:34 Korean-community discovery refresh

Updated: 2026-09-16 KST

Read current Threads README/NEXT_RUN_HANDOFF, current main/recent commits, and latest 089-sol first. Repo tip remained authoritative.

Discovery inspected roughly 40+ materially distinct public/index/search leads and retained 15 new deduped candidates in `data/candidates/`: 4 C1 + 11 C0. Strongest observed new item is `결혼식 해보니까 오지도 않고 5만원 내는 사람 많더라` on TheQoo (50,295 views / 568 comments at the selected observation). Other new exact-page C1s: `간호사랑 기싸움하려고 환자 죽일뻔한 의사 썰..` (Inven 5,470 / 7 rec / 21 comments), `이제는 산에서까지 뛰어다니나 보네요` (Ppomppu 20,075 views; title-side 50 indicator left uninterpreted), and `95년생이랑 75년생이랑 결혼가능할까요` (Inven 3 / 0 / 0). C0 leads were kept C0 where only public index/search evidence existed.

No bulk crawling, login/anti-bot bypass, fabricated URLs, merged engagement snapshots, OCR/moderation/rights success, source-byte acquisition or publication success was claimed.

Full-post screenshots captured this run: 0. Actual source bytes acquired: 0. Real source-backed carousel: NO. A1=0, P1=0, publicationAllowed=false.

Threads handoff commit: `25a859710a246600319268b64cecba3ec8a21e8a`.

Next: acquire a complete ordered source set for the strongest C1, then render the first real cover + full-post screenshot carousel. Only 04 may publish.

---

## 원본 기록: 091-sol.md

### 091-sol — overlap-safe full-post screenshot slicing

Updated: 2026-09-16 KST

Read current Threads NEXT_RUN_HANDOFF, current main/recent commits, and latest 090-sol first. Repo tip remained authoritative.

Material production change: tall `post` screenshots previously used hard edge-to-edge 1080×1080 slicing. That could place a text line exactly on a swipe boundary. `app/source-intake.js` now uses a 72 rendered-pixel overlap between sequential slices. This preserves every source pixel in order while giving readers continuity across swipes. No body rewriting, summary/CTA insertion, automatic privacy masking, OCR or vision was added.

Threads runtime commit: `a67094c`.
Handoff commit: `2dcb78c8`.

Verification on the authorized desktop: `git diff --check` PASS, `node --check app/source-intake.js` PASS, full `npm run check` PASS. The suite included actual ffmpeg 1080×1920 H.264 render + ffprobe PASS. Actual Chrome E2E was not run and is not claimed.

Discovery state inherited: 40+ raw leads / 15 retained (4 C1 + 11 C0). Full-post screenshots captured this run: 0. Actual source bytes acquired: 0. Real source-backed carousel: NO. A1=0, P1=0, publicationAllowed=false.

Next: acquire one complete ordered real source set, render the first source-backed square carousel with overlap-safe body slicing, and verify it in Chrome. Only 04 may publish.

---

## 원본 기록: 092-sol.md

### 092-sol — 16:36 high-volume Discovery refresh

Updated: 2026-09-16 KST

Read current Threads README, NEXT_RUN_HANDOFF, current main/recent commits and latest 091-sol first. Repo tip remained authoritative.

Discovery queried Korean-community-first public search/index/page lanes (Blind, TheQoo, Ppomppu and searches covering FMKorea/DCInside/Ruliweb/Arca etc.) plus Reddit/public web. Restricted sources were not bulk-crawled and no login/anti-bot bypass was attempted. Roughly 40+ materially distinct search/index/page leads were inspected. Promotional, ordinary market/news, duplicate, low-story and weak-fit items were filtered; 15 candidates were retained as one Markdown file each in `data/candidates/`: 14 C1 and 1 C0.

Strong new Korean candidates: `돈 때문에 결혼접을까 고민된다는 남자` (Ppomppu exact page, 6,298 views / 15 comments, two JPG attachments listed but image body not read), `결혼 후 퇴사 고민` (Blind exact page, 2,694 views / 2 likes / 19 comments), plus image-humor and surprise-cost lanes. Strong overseas story candidates include the $500 wedding charity donation dispute (+1,351 observed Reddit score), $20K wedding-venue/family-affair conflict (+13,261), and wedding no-show invoice dispute (+191).

Truth gates: full-post screenshots captured 0; actual source bytes acquired 0; real source-backed carousel NO; A1=0; P1=0; publicationAllowed=false. No rights, OCR, moderation, credentials, delivery or publication success was invented. `2026년 회사별 느낌 NEW ver.` remains C0 because only a Blind index/list page was verified, not an exact individual source URL.

No runtime code changed, so npm/server/browser tests were not rerun. Threads NEXT_RUN_HANDOFF was updated. Next concrete priority is acquisition of the two actual JPGs for the Ppomppu marriage-cost candidate and confirmation whether they cover the complete original body before any carousel export. Only 04_REVIEW_PUBLISH may publish.

---

## 원본 기록: 093-sol.md

### 093-sol — screenshot acquisition provenance hardening

Updated: 2026-09-16 17:16 KST

Read current Threads handoff, current main/recent commits and 092-sol first; repo tip remained authoritative.

Material production change: `app/source-package.js` schema moved to v3. Each screenshot/media asset now records ordered `sourceSequence`, explicit acquisition state (`CAPTURED`, `USER_PROVIDED`, `SOURCE_MEDIA`, `ASSETS_PENDING`), optional original dimensions, capture URL/time and applied crop provenance. Body assets are rejected if their source sequence goes backwards. `assetsPending` remains true unless `fullBodyCaptureStatus=complete` and every body asset is actually acquired. This closes the gap where an attachment filename or index listing could otherwise look like a usable source-backed asset.

Fresh verification of the current top acquisition target `돈 때문에 결혼접을까 고민된다는 남자`: public search still exposes the exact Ppomppu page, 6,298 views / 15 comments and two JPG attachment names. Direct page fetch returned 403, so no anti-bot/access bypass was attempted and no bytes were claimed.

Truth: this was production-focused, so raw new discovery leads=0 and retained new candidates=0. Full-post screenshots=0, actual source bytes=0, real source-backed carousel=NO, A1=0, P1=0. `publicationAllowed=false`; only 04_REVIEW_PUBLISH may publish. No OCR, moderation, rights, credentials, delivery or publication success was invented.

Runtime tests/browser E2E were not run because this connector execution did not provide a repository checkout/runtime shell. Threads NEXT_RUN_HANDOFF was updated accordingly. Next priority is permitted/manual acquisition of the two real Ppomppu JPGs, completeness verification, then first schema-v3 real carousel render and Chrome verification.

---

## 원본 기록: 094-sol.md

### 094-sol — 17:36 Korean-community discovery refresh

Updated: 2026-09-16 17:36 KST

Read Threads README, current handoff, current main/recent commits and 093-sol first; repo tip remained authoritative.

This pass used public search/index/page access only and did not bypass restricted-source login or anti-bot controls. Roughly 40+ raw leads/search results were inspected across requested Korean-community-first lanes; result quality was noisy, so 3 genuinely usable new C1 candidates were retained rather than padding the queue.

Retained:
- `임대아파트 사는 여친과 결혼문제` — Ppomppu exact public page; 20,103 views observed; full text body read; full comments not read. Strong marriage/money/family-background conflict.
- `급여담당하는 직방덬들 있어? 공제내역 계산하는 거 말이야` — TheQoo exact page; 48 views / 6 comments observed; full body read, comments not read. Workplace/payroll absurdity.
- `내가 홍콩 보내줄게.jpg` — Ppomppu exact page; 1,670 views and one attached image observed. Image body not read, therefore explicitly recorded as `본문 미확인` and not filled in.

All three were stored as one Markdown file per candidate under `data/candidates/`, C1_A0_P0. No grouped discovery JSON was created. Full-post screenshots=0, source bytes=0, real source-backed carousel=NO, A1=0, P1=0. `publicationAllowed=false`; rights/privacy/human review gates remain and only 04_REVIEW_PUBLISH may publish. No OCR/moderation/rights/credentials/delivery/publication success was invented.

No application runtime code changed in this discovery-only pass, so npm/server/browser tests were not applicable and are not claimed. Threads NEXT_RUN_HANDOFF was updated. Next concrete priority remains permitted acquisition of actual complete source screenshots/images, then schema-v3 intake and first Chrome-verified source-backed carousel.

---

## 원본 기록: 095-sol.md

### 095-sol — source-title + manual-only privacy binding fix

Updated: 2026-09-16 18:14 KST

Read current Threads handoff, current main/recent commits and 094-sol first; repo tip remained authoritative.

Repo-tip inspection found a concrete regression in `app/card-story-model.js`: source-backed carousel text still passed through the legacy automatic PII redactor and storyboard metadata declared image masking required. This contradicted the binding carousel rules: cover defaults to the observed original post title, source screenshots are preserved, and privacy masking is user-directed rather than automatic.

Material fix committed to Threads main as `cee4152b`:
- slide-1 source-backed title is now derived from `item.title`, not an editorial hook override;
- source-backed title/source metadata uses non-redacting cleanup;
- storyboard privacy metadata explicitly sets automatic masking and automatic PII mutation false;
- manual privacy review/masking remains available as a human gate;
- reference-square validation rejects automatic privacy mutation if reintroduced.

No source screenshots were acquired in this implementation pass. Latest discovery truth remains raw 40+, retained 3 C1, full-post screenshots 0, source bytes 0, real source-backed carousel NO, A1/P1 0. `publicationAllowed=false`; only 04_REVIEW_PUBLISH may publish.

This execution environment did not expose a repository runtime shell/browser, so npm checks, server smoke and Chrome E2E are not claimed. No OCR/moderation/rights/credentials/delivery/publication success was invented. Threads handoff was updated in `638324dd`.

---

## 원본 기록: 096-sol.md

### 096-sol — 18:38 Discovery refresh

Updated: 2026-09-16 18:38 KST

Started from current Threads repo tip after reading `00_START_HERE/README.md`, `NEXT_RUN_HANDOFF.md`, recent main commits, and `095-sol.md`. Repo tip remained authoritative.

Public search/index discovery inspected 40+ leads/results across Korean-community-first queries and Reddit/social lanes. Korean results were noisy or access-restricted in this window; no login, anti-bot bypass or bulk crawling was attempted. Eight exact-public-URL Reddit story candidates were strong enough to retain as individual `data/candidates/260916_C1_A0_P0_*.md` files.

Strongest retained: wedding money accepted then used for a house after eloping (+17,821); $20K non-refundable wedding venue colliding with a family affair memory (+13,261, elevated privacy/defamation review); parents demanding a $35K–$45K wedding while the couple wants a ~$10K one (+12,084); sister who mocked a cheap wedding later asking that sibling for wedding money (+8,246). Other retained stories cover expensive wedding gifts, a mother using wedding-envelope money, destination-wedding family exclusion, and guests facing roughly $10K attendance cost.

All scores are only the actually visible values from the same 2026-09-16 18:38 KST public-search observation. View/comment totals not visible were left unknown. Bodies were read from public retrieval; full comment threads were not read.

Truth state: raw 40+; retained 8 C1; full-post screenshots 0; source bytes 0; real source-backed carousel NO; A1/P1 0. All new candidates remain A0/P0, `ASSETS_PENDING`, `publicationAllowed=false`; rights/privacy/human review gates remain and only 04_REVIEW_PUBLISH may publish.

No OCR/moderation/rights clearance/source acquisition/publication/npm/server/browser test is claimed. Threads handoff was updated after the candidate writes.

---

## 원본 기록: 097-sol.md

### 097-sol — Source Package false-complete 방지

시각: 2026-09-16 19:16 KST

Threads repo tip 기준으로 source-first 파이프라인을 계속 작업했다.

#### 변경
- `app/source-package.js` schema v4.
- `fullBodyCaptureStatus=complete`는 이제 body asset이 실제 확보 상태이고 각 asset에 provenance + 원본 sourceWidth/sourceHeight가 있어야만 허용.
- body `sourceSequence`는 strictly increasing 강제. 중복/역순 sequence를 거부해 원문 순서 ambiguity 방지.
- slide 1 cover / slide 2+ original screenshots-media, publicationAllowed=false, privacy USER_REVIEW, rights/human gate, 04_REVIEW_PUBLISH 단독 게시 규칙 유지.

#### Discovery / asset truth
- 최신 완료 Discovery: raw 40+ / retained 8 C1.
- 이번 구현 회차에서 한국 공개 검색 lane도 재확인했으나 quota 채우기용 약한 신규 후보는 저장하지 않음.
- full-post screenshots: 0
- source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0

#### 검증 truth
GitHub 직접 수정/커밋. 현재 런타임에 checkout shell/Chrome이 없어 npm check, server smoke, browser E2E 실행 성공을 주장하지 않음. OCR/moderation/rights clearance/publication도 주장하지 않음.

#### 다음
실제 한국 C1 원문 전체 screenshot/image bytes 확보 → schema-v4 intake → 1080x1080 cover + 원문 전체 순차 carousel → Chrome 실물 검증 순서.

---

## 원본 기록: 098-sol.md

### 098-sol — 19:35 Discovery refresh

시각: 2026-09-16 19:35 KST

Threads repo tip/README/handoff/recent commits/097-sol을 먼저 확인하고 Discovery를 갱신했다.

#### 수집
- 한국 커뮤니티 우선 공개 search/index → Reddit/social/web 순으로 40+ raw lead/result 확인.
- 로그인, anti-bot 우회, bulk crawl 없음.
- 이번 검색에서 한국 커뮤니티 결과 품질/접근성이 낮아 약한 C0를 숫자 채우기용으로 저장하지 않음.
- dedupe/story/safety 기준 후 신규 **C1 5건** 보관.

#### 신규 보관
1. 결혼식 £25k~£30k로 예상 저축 대부분을 쓸지 갈등 — Reddit +179.
2. 파혼 뒤 전 약혼녀 부모가 이미 쓴 결혼비용 상환 갈등 — BoRU +4,545, 원출처 provenance 추가 검토 필요.
3. 시댁에 결혼사진을 주지 않는 갈등 — Reddit +715, 민감 개인사 privacy/comfort 검토 필요.
4. 시누이가 같은 해/같은 venue로 결혼식을 옮긴 뒤 작성자에게 2028년으로 미루라고 요구 — +473.
5. 예상치 못한 임신으로 결혼식을 미루자 몇 달 뒤 결혼 예정인 가족이 2028년까지 미루라고 요구 — +50.

각 후보는 `data/candidates/260916_C1_A0_P0_*.md` 한 건당 파일 하나로 기록했다. 본문을 실제 읽은 범위만 요약했고 이번 관측에서 보이지 않은 조회/댓글 수는 만들지 않았다.

#### asset/publication truth
- raw: 40+
- retained: 5 C1
- full-post screenshots: 0
- source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0
- 모두 ASSETS_PENDING / publicationAllowed=false
- rights/privacy/human gate 및 04_REVIEW_PUBLISH 단독 게시 유지

#### 다음
한국 공개 커뮤니티 coverage가 좋아지는 시점에 고볼륨 탐색 계속. 동시에 기존 한국 C1 중 실제 첨부 source bytes/전체 screenshot을 허용된 경로로 확보하는 것이 가장 큰 병목이다.

---

## 원본 기록: 099-sol.md

### 099-sol — Source Package crop application gate

시각: 2026-09-16 20:17 KST

Threads repo tip의 handoff, current main/recent commits, 최신 098-sol을 먼저 확인한 뒤 작업했다.

#### 변경
- `app/source-package.js`를 schema v5로 갱신.
- crop suggestion은 계속 자동화 가능하지만 실제 `cropApplied`가 있으면 `cropDecision=USER_CONFIRMED` 또는 `VERIFIED_UI_ONLY`가 반드시 필요.
- 기본 `NONE` 상태에서 crop이 적용된 패키지는 reject.
- 반대로 실제 crop 없이 결정 상태만 기록하는 것도 reject.
- 목적: UI chrome crop 제안이 자동으로 원문 본문 crop으로 변하는 경로 차단.
- 개인정보 자동 마스킹은 추가하지 않음.

#### truth
- 직전 Discovery raw 40+ / retained 5 C1 유지.
- full-post screenshots 0 / source bytes 0 / real source-backed carousel NO / A1·P1 0.
- publicationAllowed=false 및 04_REVIEW_PUBLISH 단독 게시 유지.
- OCR/moderation/rights/publication 성공 주장 없음.

#### 검증
이번 실행 환경에는 checkout/runtime shell 및 Chrome session이 노출되지 않아 npm check/server smoke/browser E2E는 실행하지 않았고 성공 주장도 하지 않음.

다음은 실제 한국 C1 원문 전체 asset 확보 → schema-v5 provenance/crop decision 입력 → 1080x1080 실제 source-backed carousel → Chrome 검증 순서.

---

## 원본 기록: 100-sol.md

### 100-sol — 20:36 Discovery refresh

시각: 2026-09-16 20:36 KST

Threads repo tip의 README, NEXT_RUN_HANDOFF, current main/recent commits, 최신 099-sol을 먼저 확인하고 repo tip 기준으로 진행했다.

#### Discovery
- public/index/search 경로에서 raw lead/result 40+ 확인.
- Korean-community-first 검색을 우선했으며 접근 제한/검색 잡음이 있는 소스는 우회하지 않음.
- 신규 retained: 2 C1.
- `data/candidates/260916_C1_A0_P0_베트남여자랑결혼한다는사촌동생.md`
  - 뽐뿌 exact URL 확인.
  - 공개 본문 전체 읽음, 전체 댓글은 미확인.
  - 동일 관측에서 조회 37,476 확인.
  - 가족 반대/짧은 교제/결혼비용/과거 코인손실이 한 사연에 연결됨.
- `data/candidates/260916_C1_A0_P0_결혼비용지원금엄마계좌갈등.md`
  - Reddit exact URL 확인.
  - 본문 전체 읽음, 댓글 일부 확인.
  - 공개 검색 관측 score +34.
  - £5,000 결혼 지원금을 누구 계좌로 받아야 하는지를 두고 가족 통제 갈등 발생.

#### truth
- 후보 1건당 Markdown 1파일 규칙 유지. 새 grouped discovery JSON 없음.
- full-post screenshots 0 / source bytes 0 / real source-backed carousel NO.
- 신규 A1/P1 0. 모두 A0/P0, ASSETS_PENDING, publicationAllowed=false.
- OCR/moderation/rights clearance/publication 성공 주장 없음.
- 04_REVIEW_PUBLISH 단독 게시 권한 유지.

#### 다음
강한 한국 C1의 실제 전체 source screenshot/image bytes 확보가 계속 최우선. 공개 페이지에 JPG 2개가 명시된 `돈 때문에 결혼접을까 고민된다는 남자`가 여전히 실물 asset intake 우선 후보.

---

## 원본 기록: 101-sol.md

### 101-sol — 21:15 source-first continuation

시각: 2026-09-16 21:15 KST

Threads repo tip의 NEXT_RUN_HANDOFF, README, current main/recent commits, 최신 100-sol을 먼저 확인하고 repo tip 기준으로 진행했다.

#### 실제 변경
- `data/candidates/260916_C1_A0_P0_노력한흙수저인생.md` 추가.
- DCInside 흙수저 갤러리 exact individual public URL을 확인해 C1로 기록.
- 공개 검색에서 본문 일부만 노출되었으므로 `본문 미확인`을 명시. 검색 노출 밖 내용을 채워 넣지 않음.
- 확인 가능한 일부에는 가족 생활비/빚 부담, 저축 실패, 연애·결혼 불안, 가족의 집 구매 요구, 연인과 가족 소개 갈등이 연결되어 있어 사람 중심 money/family/relationship lane 후보로 보관.
- 신뢰 가능한 조회/추천/댓글 수치는 이번 관측에서 확인하지 못해 기록하지 않음.

#### Discovery truth
- 직전 20:36 full sweep: raw 40+ / retained 2 C1.
- 이번 focused continuation: fresh Korean-community search result 10+ 확인 / retained 1 C1.
- 제한 소스 로그인/anti-bot 우회 및 bulk crawl 없음.

#### Asset / publication truth
- full-post screenshots: 0
- actual source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0
- 신규 후보 A0/P0, ASSETS_PENDING, publicationAllowed=false.
- OCR/moderation/rights clearance/publication 성공 주장 없음.
- 04_REVIEW_PUBLISH 단독 게시 권한 유지.

#### Verification
Discovery data/handoff만 변경했다. runtime application path는 변경하지 않았으므로 npm check/server smoke/browser E2E를 실행했다고 주장하지 않는다.

#### 다음
실제 source screenshot/image bytes 확보가 최우선. `돈 때문에 결혼접을까 고민된다는 남자`의 공개적으로 목록화된 JPG 2개를 허용된 public/manual capture 경로로 확보하는 것이 가장 구체적인 다음 단계다.

---

## 원본 기록: 102-sol.md

### 102-sol — 21:50 discovery refresh

시각: 2026-09-16 21:50 KST

Threads repo tip의 README, NEXT_RUN_HANDOFF, current main/recent commits, 최신 101-sol을 먼저 확인하고 repo tip 기준으로 진행했다.

#### 실제 변경
- `data/candidates/`에 신규 C1_A0_P0 Markdown 5건 추가.
- 후보 한 건당 파일 하나, exact public URL과 동일 관측시점에서 실제 확인한 수치만 기록.
- grouped discovery JSON 생성 없음.

#### Discovery truth
- raw leads/results: 40+ 공개 검색/index 결과 확인.
- retained: 5 C1.
- 한국 커뮤니티 우선 검색 후 Reddit/기타 공개 surface 확장.
- robots/login/anti-bot 제한은 우회하지 않음.
- 상위: 더쿠 `중국 1조 재산 아빠가 50살 연하 새부인 데리고 와서 개판난 가족` 108,618 views / 357 comments; 더쿠 `모은 돈 5천만원...재산 4억` 3,870 views / 36 comments; Reddit 파혼 후 예식장 비용 상환 갈등 +9,412.

#### Asset / publication truth
- full-post screenshots: 0
- actual source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0
- 신규 후보 모두 A0/P0, ASSETS_PENDING, publicationAllowed=false.
- OCR/moderation/rights clearance/publication 성공 주장 없음.
- 04_REVIEW_PUBLISH 단독 게시 권한 유지.

#### Verification
Discovery data/handoff만 변경. runtime application path 변경 없음. npm check/server smoke/browser E2E를 실행했다고 주장하지 않는다.

#### 다음
강한 한국 C1의 실제 full-post screenshot/image bytes 확보. 더쿠 1조재산 가족갈등 글은 공개 페이지에서 다수 이미지 존재가 확인되어 acquisition 우선순위가 높다. 기존 `돈 때문에 결혼접을까 고민된다는 남자`의 공개 JPG 2개도 계속 우선 확보 대상이다.

---

## 원본 기록: 103-sol.md

### 103-sol — screenshot intake manifest implementation

시각: 2026-09-16 22:19 KST

Threads repo tip의 README, NEXT_RUN_HANDOFF, current main/recent commits, 최신 102-sol을 먼저 확인하고 repo tip 기준으로 진행했다.

#### 실제 변경
- Threads `scripts/build-screenshot-intake-manifest.mjs` 추가.
- ordered PNG/JPEG 파일의 입력 순서와 실제 pixel dimensions를 기계적으로 기록.
- exact source URL을 필수 입력으로 받고 source relationship은 사람 검증 전 확정하지 않음.
- `fullBodyCaptureStatus=pending`, `publicationAllowed=false`, `publishOwner=04_REVIEW_PUBLISH` 고정.
- crop은 `NONE` 기본이며 UI chrome만 수동 검토 제안. 자동 privacy/PII masking 없음.
- OCR/vision/moderation/rights/full-body completeness/publication을 추론하거나 성공 처리하지 않음.

#### Discovery / asset truth
- latest full discovery sweep carried forward: raw 40+ / retained 5 C1.
- 이번 implementation pass 신규 discovery sweep 없음.
- full-post screenshots captured: 0
- actual source bytes acquired: 0
- real source-backed carousel: NO
- A1/P1: 0

#### Verification
GitHub connector를 통한 파일 생성/commit은 성공. checkout/runtime shell과 Chrome session이 없어 `npm run check`, server smoke, browser E2E는 실행하지 않았고 성공 주장도 하지 않는다.

#### 다음
강한 한국 C1의 실제 전체 원문 screenshot/image bytes를 허용된 공개/수동 capture로 확보 → 새 manifest builder로 순서/크기/provenance 기록 → schema-v5 Source Package → 1080x1080 source-backed carousel → Chrome 검증 순으로 진행한다.

---

## 원본 기록: 104-sol.md

### 104-sol — 22:34 discovery refresh

시각: 2026-09-16 22:34 KST

Threads repo tip의 NEXT_RUN_HANDOFF와 current main/recent commits, 최신 103-sol을 확인하고 repo tip 기준으로 진행했다. README 첫 connector fetch는 timeout이 발생했지만 handoff의 READ FIRST/role chain과 repo tip을 기준으로 계속 진행했으며 제한 소스 우회는 하지 않았다.

#### 실제 Discovery
- Korean-community-first 공개 검색 + Reddit/social 공개 검색 수행.
- raw search leads/results: **40+**.
- 신규 retained: **4 C1**, 모두 `data/candidates/`에 1건=Markdown 1파일.
- 신규 후보 모두 exact individual public URL과 본문 전체를 확인함.
- 댓글 전체를 읽었다고 기록하지 않음.
- Clien/Instiz는 public search layer robots block 확인 후 우회하지 않음.

상위 신규:
1. 사촌이 OP 결혼식 바로 전날 결혼식을 잡고 가족에게 ~$500+씩 모금 요구 — Reddit +6,754.
2. 과거 자매를 주거/생활비 지원했으나 역으로 도움 거절당한 뒤 결혼식 불참 갈등 — Reddit +6,099.
3. 여동생이 언니의 부유한 남편에게 자기 결혼식 £22,000 부담 요구 — Reddit +3,166.
4. 부모가 딸 결혼식 $35,000 지원했으나 딸은 약 $70,000 전체 부담 요구 — Reddit +1,695.

#### 한국 소스 결과
이번 검색에서 exact page로 유의미하게 잡힌 강한 한국 후보는 이미 보관된 TheQoo `모은 돈 5천만원…재산 4억 시험`, `중국 1조 재산…가족` 등이 중심이었다. 신규 숫자를 맞추려고 중복/뉴스형/약한 index 결과를 다시 저장하지 않았다.

#### Asset / publication truth
- full-post screenshots captured: 0
- actual source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0
- 신규 전부 ASSETS_PENDING / publicationAllowed=false
- OCR/moderation/rights/privacy clearance/publication 성공 주장 없음.

#### 다음
한국 커뮤니티 exact-page 신규 후보를 계속 넓히되 약한 C0로 수량을 채우지 말고, 동시에 기존 강한 한국 C1의 전체 원문 screenshot/image bytes 확보를 허용된 공개/수동 capture로 진행한다.

---

## 원본 기록: 105-sol.md

### 105-sol — screenshot intake fingerprinting

시각: 2026-09-16 23:18 KST

Threads repo tip의 NEXT_RUN_HANDOFF, current main/recent commits, 최신 104-sol을 읽고 repo tip 기준으로 진행했다.

#### 실제 변경
`Threads/scripts/build-screenshot-intake-manifest.mjs`를 강화했다.
- 실제 PNG/JPEG source asset마다 `byteLength` 기록.
- 실제 bytes의 SHA-256 기록.
- 동일 bytes가 source sequence에 두 번 들어오면 즉시 실패.
- 기존 순서, 실제 pixel dimensions, exact source URL/provenance 필드는 유지.

목적은 같은 screenshot을 여러 body slide로 중복 삽입하는 오류를 조기에 막고, 이후 Source Package가 실제 입력 bytes를 추적할 수 있게 하는 것이다.

#### 보존된 gate
- fullBodyCaptureStatus=pending
- publicationAllowed=false
- publish owner=04_REVIEW_PUBLISH
- privacy masking=USER_DIRECTED_ONLY
- OCR/vision, rights, privacy clearance, moderation, full-body completeness, publication을 자동 성공으로 추론하지 않음.

#### 현재 truth
- latest full discovery raw: 40+
- retained: 4 C1
- full-post screenshots captured this run: 0
- actual source bytes acquired: 0
- real source-backed carousel: NO
- A1/P1: 0

#### 검증 제한
이번 실행 환경에는 executable checkout/Chrome session이 없어 npm check/server smoke/browser E2E 성공을 주장하지 않는다. 변경은 Node built-in `crypto`만 사용한다.

---

## 원본 기록: 106-sol.md

### 106-sol — 23:34 Discovery refresh

시각: 2026-09-16 23:34 KST

Threads repo tip의 README/NEXT_RUN_HANDOFF, current main/recent commits, 최신 105-sol을 먼저 읽고 repo tip 기준으로 진행했다.

#### 실제 Discovery
한국 커뮤니티 우선으로 Blind, TheQoo, FMKorea, Ppomppu, Inven, Ruliweb, Arca, DCInside와 공개 Reddit/Threads/X/Instagram/YouTube 검색면을 폭넓게 확인했다. 제한/robots 소스는 우회하지 않았다.

- raw lead/result: 40+
- 신규 retained: 5 C1
- grouped JSON 생성 없음
- 후보 1건당 `data/candidates/` Markdown 1파일

신규:
1. Blind `[결혼 고민] 결혼 전부터 경제권 요구하는 여친... 내가 잘못함?` — 관측 97 views / 14 comments.
2. Reddit `AITA if we decided not to give 50% of the wedding cash gifts to my in-laws...` — +4,078.
3. Reddit `AITA for giving my sister a "cheap" gift ... Tiffany necklace` — +3,877.
4. Reddit `AITAH for not wanting to accept a $40k wedding gift from my parents?` — +605.
5. Reddit `Guilted into contributing a family member’s wedding...` — +162, 이미 $10k 부담.

#### 검증/자산 truth
- exact individual public URL: 5건 모두 확인
- Reddit 4건 본문 read: YES
- Blind: 공개 페이지/search surface에 실제 노출된 본문 범위만 확인; 완전 screenshot capture 없음
- comments fully read: NO
- full-post screenshots: 0
- source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0
- publicationAllowed=false
- rights/privacy/human review gate 유지
- 실제 게시 권한은 04_REVIEW_PUBLISH만 유지

수치·URL·OCR/moderation·rights·asset capture·publication을 추측하지 않았다.

---

## 원본 기록: 107-sol.md

### 107-sol — screenshot intake public-source URL guard

시각: 2026-09-17 00:17 KST

Threads repo의 `00_START_HERE/README.md`, `NEXT_RUN_HANDOFF.md`, current main/recent commits와 최신 `106-sol.md`를 먼저 읽고 repo tip 기준으로 진행했다.

#### 실제 변경
`scripts/build-screenshot-intake-manifest.mjs`의 `--source-url` provenance 입력을 강화했다.

- absolute HTTP(S) URL만 허용
- embedded username/password URL 거부
- localhost / loopback / RFC1918 IPv4 / `.local` 거부
- URL fragment 제거 후 manifest에 기록
- 기존 screenshot order / dimensions / byteLength / SHA-256 / duplicate-byte rejection / publicationAllowed=false / 04_REVIEW_PUBLISH owner 규칙 유지

목적은 로컬 주소나 비공개 주소가 exact public source provenance처럼 Source Package 전단계에 들어가는 것을 막는 것이다.

Baseline: `90c22f97f6c9d45cdda318f6b9176f648709f024`
Implementation commit: `f9631ec60920bdab1b2e7e48701a289e6013a63c`
Handoff update commit: `4b058cf08a0aa1d52648217bacf6de79aeb58c4d`

#### Discovery / asset truth
이번 회차는 구현 집중 회차라 fresh Discovery를 수행하지 않았다. 최신 완료 batch는 raw 40+ / retained 5 C1이다.

- full-post screenshot captured: 0
- actual source bytes acquired: 0
- real source-backed carousel: NO
- A1/P1: 0
- publicationAllowed=false 유지
- OCR/moderation/rights/publication 성공 주장 없음

#### 검증 truth
GitHub 연결 환경에서 repo read/write는 가능했지만 실행 가능한 checkout/Chrome 세션이 제공되지 않았다. 따라서 `npm run check`, server smoke, executable targeted test, browser E2E는 실행하지 않았으며 성공했다고 기록하지 않는다.

---

## 원본 기록: 108-sol.md

### 108-sol — Discovery refresh

시각: 2026-09-17 00:48 KST

Threads repo의 `00_START_HERE/README.md`, `NEXT_RUN_HANDOFF.md`, current main/recent commits, 최신 `107-sol.md`를 먼저 읽고 repo tip 기준으로 진행했다.

#### 실제 작업
한국 커뮤니티 우선 검색 + Reddit/social 보조 레인에서 공개적으로 접근 가능한 결과를 폭넓게 확인했다. Clien/Instiz는 public search layer에서 robots 차단이 확인되어 우회하지 않았다. 제한 소스에 로그인/anti-bot 우회나 bulk crawl을 하지 않았다.

- raw leads/results inspected: 40+
- new retained: 7 C1
- 후보 한 건당 `data/candidates/260917_C1_A0_P0_*.md` 하나씩 생성
- grouped discovery JSON을 data 루트에 만들지 않음
- 기존 후보와 중복되는 강한 결과는 재저장하지 않음

상위 신규 후보:
- 5살 조카가 화동도 아닌데 작은 웨딩드레스처럼 보이는 흰 드레스 + 신부와 맞춘 꽃관을 요구한 결혼식 갈등 (visible score 4,361+)
- 결혼하지 않겠다는 아들에게 딸 결혼 때와 같은 $8k를 현금으로 준 부모의 형평성 갈등 (+1,526)
- 축의금이 적었던 자기 가족을 덜 부르고 부유한 배우자 가족을 더 부르겠다는 결혼식 갈등 (+520)
- 실직 후 해외결혼식에 $2k+를 쓰고 약 $100 맞춤 선물을 했지만 현금 선물이 부족하다는 말을 들은 사연 (+84)
- 최저임금 파트타임 직원들이 매니저 결혼식에서 월급 10~25% 수준의 현금선물 압박을 느끼는 직장 갈등 (+6)

#### Asset / publication truth
- full-post screenshots: 0
- actual source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0
- 신규 후보 모두 ASSETS_PENDING / publicationAllowed=false
- rights/privacy/human review gate 유지
- OCR/moderation/rights/publication 성공 주장 없음

#### 검증
이번 회차는 data-only Discovery 작업이다. GitHub connector로 후보 파일과 handoff를 실제 main에 기록했다. renderer/runtime 코드는 변경하지 않았고 npm check/server smoke/browser E2E/screenshot capture/OCR/moderation/publication은 실행했다고 주장하지 않는다.

#### 다음 우선순위
한국 exact public page 확보를 계속 늘리되 약한 C0로 숫자를 채우지 않는다. 동시에 강한 기존 Korean C1 및 이번 상위 C1의 실제 전체 원문 screenshot/source bytes를 permitted public/manual capture로 확보해 screenshot intake → Source Package → 1080×1080 source-backed carousel로 연결한다. P1은 계속 04_REVIEW_PUBLISH만 가능하다.

---

## 원본 기록: 109-sol.md

### 109-sol — Screenshot intake provenance hardening

시각: 2026-09-17 01:14 KST

Threads repo의 `NEXT_RUN_HANDOFF.md`, `00_START_HERE/README.md`, current main/recent commits, 최신 `108-sol.md`를 읽고 repo tip 기준으로 실제 구현을 진행했다.

#### 실제 작업
`scripts/build-screenshot-intake-manifest.mjs`를 수정했다.

- source provenance URL에 `token`, `access_token`, `api_key`, `authorization`, `secret`, `signature` 등 credential/secret 성격의 query key가 있으면 manifest에 평문 저장하지 않고 즉시 거부한다.
- `--observed-at`은 ISO-8601 datetime만 허용하고 저장 전에 UTC ISO로 정규화한다.
- 기존 public HTTP(S) URL 제한, local/private host 거부, fragment 제거, 실제 image dimensions/byteLength/SHA-256, duplicate bytes 차단을 유지한다.
- `publicationAllowed=false`, `04_REVIEW_PUBLISH` 단독 publish ownership, user-directed privacy masking을 유지한다.
- OCR/vision/moderation/rights/full-body completeness/publication은 자동 성공 처리하지 않는다.

Threads implementation commit: `6595eaf99e17b3d509bdc3cf57db02bffa4bd06b`.
Handoff도 최신화했다.

#### Discovery / asset truth
이번 회차는 구현 회차라 새 Discovery sweep을 돌리지 않았다. 최신 baseline은 raw 40+ / retained 7 C1이다.
- full-post screenshots: 0
- actual source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0
- ASSETS_PENDING / publicationAllowed=false 유지

#### 검증 truth
GitHub connector를 통한 실제 main write는 성공했다. 이 실행 환경에는 executable checkout/Chrome session이 노출되지 않아 `npm run check`, server smoke, browser E2E를 실행했다고 주장하지 않는다. screenshot capture/OCR/moderation/publication도 수행했다고 주장하지 않는다.

#### 다음 우선순위
강한 Korean C1의 실제 전체 원문 screenshot/source bytes 확보 → intake manifest 실행 → Source Package → 1080×1080 cover + full original screenshot carousel → Chrome 검증 순서로 진행한다. Discovery는 다음 useful sweep에서 40–80 raw / 15–30 retained 목표를 계속 적용한다.

---

## 원본 기록: 110-sol.md

### 110-sol — Korean-first Discovery refresh

시각: 2026-09-17 01:39 KST

Threads repo의 `00_START_HERE/README.md`, `NEXT_RUN_HANDOFF.md`, current main/recent commits와 최신 `109-sol.md`를 먼저 읽고 repo tip 기준으로 작업했다.

#### 실제 작업
공개 검색/index만 사용해 Korean-community-first Discovery를 수행했다. Blind, TheQoo에서 exact public post를 다수 확인했고 DCInside/FMKorea/Ppomppu/Inven/Ruliweb/Arca 등 한국 커뮤니티 검색도 함께 돌렸다. 제한 소스의 로그인/anti-bot/bulk crawl 우회는 하지 않았다. 이후 Reddit 공개 원문을 보조 레인으로 확인했다.

대략 raw 40+ search results/leads를 검토하고 중복·약한 뉴스형·스토리성이 낮은 결과를 제거해 신규 C1 16건을 `data/candidates/260917_C1_A0_P0_*.md`로 각각 한 파일씩 저장했다.

상위 신규 후보:
- Blind `투자 빚 8000 , 여친 결혼 문제` — 조회636/좋아요1/댓글4. 투자로 모은돈 전부+8천만원 빚, 부모 상환 후 결혼 상대에게 고백 고민.
- Blind `결혼 고민인데 남편 투자, 소비 성향이 걱정` — 조회1,806/댓글33. 상대가 투자로 3억원 손실.
- Blind `파혼 해야 할까요..` — 조회1,043/댓글45. 10년+ 연애 후 결혼 예약까지 마친 상태에서 신혼집 돈 문제로 파혼 고민.
- Blind `돈 없는 시댁` — 조회3,414/댓글44. 시어머니 500만원 요청과 집/노후/부양 갈등.
- Blind `축의금 문화, 결혼 문화 10년내 다바뀔 듯` — 조회85K/좋아요433/댓글543. 개인사연 주력은 아니지만 대규모 생활 논쟁 보조 레인.
- TheQoo `판) 결혼 몇개월 앞두고 파혼함` — 조회13,124. 양가 설거지/효도/집값 문제로 파혼.
- Reddit college money → sister wedding +253, destination-wedding family travel +547, $3,500 wedding dress +501, extravagant wedding loan refusal +1,178 등.

후보 파일에는 실제 확인한 제목/URL/관측시각/보이는 수치/본문·댓글 확인 상태/평가/asset 상태만 기록했다. 본문을 읽은 경우 요약했고 제3자 전문 장문 복제는 하지 않았다.

#### Asset / publication truth
- full-post screenshots: 0
- actual source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0
- 모두 ASSETS_PENDING / publicationAllowed=false
- rights/privacy/human review gate 및 04_REVIEW_PUBLISH 단독 게시 규칙 유지

GitHub candidate writes와 handoff update는 성공했다. 실행 가능한 checkout/Chrome session이 없어 npm check/server smoke/browser E2E/screenshot/OCR/moderation/publication 성공은 주장하지 않는다.

#### 다음 우선순위
새 Korean C1 중 투자빚8000/3억 투자손실/10년연애 파혼/돈없는시댁을 우선 scoring하고, 실제 전체 원문 screenshot/source bytes를 허용된 공개·수동 capture 경로로 확보한 뒤 screenshot intake → Source Package → cover + full original screenshot carousel로 진행한다.

---

## 원본 기록: 111-sol.md

### 111-sol — verified investment-loss source lead

시각: 2026-09-17 02:17 KST

Threads repo의 current handoff, README, main/recent commits와 최신 `110-sol.md`를 먼저 읽고 repo tip 기준으로 작업했다.

#### 실제 작업
한국 커뮤니티 우선 공개 검색을 이어서 약 40+ public search result/lead를 확인했다. 제한 소스 로그인/anti-bot/bulk crawl 우회는 하지 않았다. Blind의 exact individual source page `[인증] 하루 5.2억 손실, 한 달 15억 손실`을 직접 확인하고 신규 C1 한 건을 `data/candidates/260917_C1_A0_P0_하루52억한달15억손실.md`로 저장했다.

동일 관측시점 공개 페이지 표시: 조회수 5,380 / 좋아요 10 / 댓글 96. 본문 전체를 읽었고 공개 댓글도 확인했다. 작성자는 키네마스터 매각 불발 뒤 하한가를 맞아 하루 5.2억·한 달 15억 손실을 기록했다고 설명하며, 두 증권사의 하루/월 손익 인증 이미지 총 4장을 게시했다. 단순 종목/시황 뉴스가 아니라 실제 투자자의 극적 손실·인증·과거 수익 대비 반전이 있는 사람 이야기다.

#### Asset / publication truth
- 원문 페이지에서 첨부 이미지 4장 존재 확인
- full-post screenshots captured: 0
- actual source bytes acquired: 0
- real source-backed carousel: NO
- A1/P1: 0
- ASSETS_PENDING / publicationAllowed=false
- 이미지 내 privacy 가능성은 자동 마스킹하지 않으며 human review gate 유지
- OCR/moderation/rights/publication 성공 주장 없음

GitHub candidate와 handoff write는 성공했다. 실행 가능한 checkout/Chrome automation session이 없어 npm check/server smoke/browser E2E는 실행 성공으로 기록하지 않는다.

#### 다음 우선순위
이 후보의 전체 본문 + 첨부 이미지 4장을 허용된 public/manual capture로 실제 확보 → screenshot intake의 order/dimension/hash/provenance 검증 → Source Package → cover + full original screenshot carousel → Chrome 확인 순으로 진행한다. 게시 권한은 계속 04_REVIEW_PUBLISH만 가진다.

---

## 원본 기록: 112-sol.md

### 112-sol — 02:35 high-volume Discovery refresh

시각: 2026-09-17 02:35 KST

Threads `README.md`, `NEXT_RUN_HANDOFF.md`, current main/recent commits, 최신 `111-sol.md`를 먼저 읽고 repo tip 기준으로 작업했다.

#### 실제 작업
한국 커뮤니티 우선 공개 검색을 진행해 약 40+ raw lead/result를 확인했다. Blind를 중심으로 TheQoo/DCInside/FMKorea/Ppomppu/Inven/Ruliweb/Arca 공개 검색 범위를 확인하고 Reddit을 보완했다. 로그인/anti-bot/bulk crawl 우회는 하지 않았다. 중복·약한 소재·접근/안전 문제를 거른 뒤 신규 C1 15건을 `data/candidates/`에 후보 1건=Markdown 1파일로 저장했다.

상위 신규: `주식으로 8천날림`(조회276/댓글31), `27살 여자 오늘 파혼했어요 눈물나는데 잘한거 맞죠?`(2,096/39), `이거 어떻게 복수해줄까요?ㅠㅠ`(2,493/54), 주식 1.5억 물림+혼인신고/전세 갈등 `결혼 전 고민`(103/11), 양가 축의금 갈등 `결혼식 부모님 축의금...`(189/28). 그 외 시댁 매주 방문, 축의금 인플레이션, 부모 축의금 소유권, 해외 롱디 결혼 고민, 결혼 2년차 이혼 통보와 Reddit 결혼비용/가족 갈등 5건을 보관했다.

#### Asset / publication truth
- full-post screenshots captured: 0
- actual source bytes acquired: 0
- real source-backed carousel: NO
- A1/P1: 0
- 전부 ASSETS_PENDING / publicationAllowed=false
- OCR/moderation/rights/publication 성공 주장 없음

GitHub candidate/handoff writes는 성공했다. 실행 가능한 checkout/Chrome automation을 사용하지 않았으므로 npm check/server smoke/browser E2E 성공을 주장하지 않는다.

#### 다음 우선순위
`주식으로 8천날림` → 기존 `[인증] 하루 5.2억 손실, 한 달 15억 손실` → `27살 여자 오늘 파혼...` → `이거 어떻게 복수해줄까요?ㅠㅠ` 순으로 전체 원문 screenshot/source media 실제 확보를 시도한다. 이후 screenshot intake order/dimension/hash/provenance 검증 → Source Package → cover + full original screenshot carousel → Chrome 확인. 게시 권한은 04_REVIEW_PUBLISH만 유지한다.

---

## 원본 기록: 113-sol.md

### 113-sol — screenshot normalization planner

시각: 2026-09-17 03:16 KST

Threads 최신 handoff/current main/recent commits와 `112-sol.md`를 확인하고 repo tip 기준으로 작업했다.

#### 실제 작업
`scripts/plan-screenshot-normalization.mjs`를 추가했다. 실제 `SCREENSHOT_INTAKE_MANIFEST`의 ordered source screenshot마다 1080×1080 `CONTAIN_NO_STRETCH` 계획을 계산해 scaled dimensions와 padding을 기록한다. 원문 body crop은 금지하고 UI chrome crop은 manual/verified suggestion only, privacy masking은 user-directed only로 유지한다. full-body completeness/OCR/moderation/rights/publication은 추론하지 않는다.

#### 검증
Windows checkout에서 새 스크립트 `node --check` 통과. `npm run check`는 실행했으나 기존 `test/source-package.test.mjs` fixture가 강화된 source-package 계약(`complete`이면 acquired provenance+dimensions 필요)을 만족하지 않아 실패했다. 전체 suite 성공 주장은 하지 않는다.

#### Discovery / asset truth
- 이번 구현 회차 신규 discovery: 없음
- 최신 baseline: raw 40+ / retained 15 C1
- top: `주식으로 8천날림`, `[인증] 하루 5.2억 손실, 한 달 15억 손실`, `27살 여자 오늘 파혼...`, `이거 어떻게 복수해줄까요?ㅠㅠ`
- full-post screenshots: 0
- source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0
- publicationAllowed=false 유지

다음은 상위 한국 후보의 실제 전체 원문 screenshot/source media 확보 → intake → normalization plan → cover + full original screenshot carousel → Chrome 검증 순서다. 게시 권한은 04_REVIEW_PUBLISH만 유지한다.

---

## 원본 기록: 114-sol.md

### 114-sol — 03:33 Korean-community-first Discovery refresh

시각: 2026-09-17 03:33 KST

Threads `00_START_HERE/README.md`, 최신 handoff, current main/recent commits, `113-sol.md`를 먼저 확인하고 repo tip 기준으로 작업했다.

#### 실제 작업
공개 search/index 기반으로 Blind 우선, DCInside/FMKorea/TheQoo/Ruliweb/Ppomppu/Inven/Arca 및 social/international 레인을 함께 겨냥해 raw 40+ lead/result를 확인했다. 제한 소스의 로그인/anti-bot을 우회하지 않았다. 중복·약한 소재·접근성 등을 걸러 신규 C1 10건을 `data/candidates/`에 후보당 Markdown 1개로 저장했다.

상위 신규 후보:
- `결혼 한달 남았는데 파혼..` — 조회 1,846 / 좋아요 1 / 댓글 53. 전세사기 1억원 + 식 한 달 전 부모의 청산 요구/파혼 언급.
- `부모님 결혼 반대(나는 남자)` — 조회 16K / 댓글 138. 6년 연애 + 결혼비용/키를 이유로 부모 반대.
- `결혼 첫 명절 시댁/처가 일정문의` — 조회 3,044 / 좋아요 4 / 댓글 60. 첫 명절 방문 형평성 갈등.
- `남친 부모님 결혼 반대` — 조회 2,658 / 댓글 33. 교사-전문직 커플, 상대 부모가 더 큰 결혼자금/개원비 수준을 기대.
- `부모님 결혼 반대` — 조회 1,794 / 댓글 9. 사주와 연봉/시댁지원 예상 때문에 반대.

그 외 `파혼 경험 있는 사람이랑 연애`, `결혼 진행 괜찮을까요`, `결혼 반대할 정도야?`, `결혼 반대 관련 조언 부탁드려요`, `30대 여자 직장 상사 선물 추천 부탁드림`을 추가했다. 마지막 후보는 공개 댓글의 성인용품 농담 때문에 comfort review를 명시했다.

#### 저장 규칙
각 후보는 exact public individual URL과 실제 관측 수치만 기록한 `C1_A0_P0`. 본문은 읽은 범위에서 요약하고 전문을 장문 복제하지 않았다. body/comments read 여부, source asset 여부, acquisition state를 각각 기록했다. `data/` 루트 grouped discovery JSON은 만들지 않았다.

#### Asset / publish truth
- raw inspected: 40+
- retained: 10 C1
- full-post screenshots captured: 0
- source bytes acquired: 0
- real source-backed carousel: NO
- A1/P1: 0
- publicationAllowed=false 유지
- rights/privacy/human review gate 유지
- OCR/moderation/rights/publication success 주장 없음

다음은 기존 최상위 `주식으로 8천날림` / `[인증] 하루 5.2억 손실, 한 달 15억 손실`과 이번 `결혼 한달 남았는데 파혼..`의 실제 전체 원문 screenshot/source media 확보 → intake → normalization → cover+원문 전체 carousel 순서다. 게시 권한은 04_REVIEW_PUBLISH만 유지한다.

---

## 원본 기록: 115-sol.md

### 115-sol — Source Package fixture repair

시각: 2026-09-17 04:18 KST

Threads 최신 handoff, current main/recent commits, 최신 `114-sol.md`를 먼저 확인하고 repo tip 기준으로 작업했다.

#### 실제 작업
`test/source-package.test.mjs`의 오래된 complete-body fixture를 현재 Source Package v5 계약에 맞게 수정했다. body screenshot마다 `acquisitionState=CAPTURED`, 엄격 증가 `sourceSequence`, 실제 계약상 필요한 `sourceWidth/sourceHeight`, provenance를 제공하도록 했고 `bodyAssetsAcquired=true`, `completeBodyEvidence=true`를 검증한다. 반대로 증거가 부족한데 `fullBodyCaptureStatus=complete`라고 선언하면 반드시 reject되는 regression assertion도 추가했다.

Threads commit: `20830b69deacd0881476f7c8d8a2e139e4880454`.
Handoff update commit: `c512d02303fce529eefc1d47548501cac6a87cd5`.

#### Test truth
이 실행 환경에는 runnable repo checkout/Node process가 노출되지 않아 targeted Node test, `npm run check`, server smoke, Chrome E2E는 실행하지 않았다. 따라서 pass를 주장하지 않는다. 다음 runnable checkout에서 targeted source-package test → `npm run check` 순으로 실제 확인할 것.

#### Discovery / asset truth
- latest discovery raw inspected: 40+
- latest retained: 10 C1
- top candidates: `주식으로 8천날림`, `[인증] 하루 5.2억 손실, 한 달 15억 손실`, `결혼 한달 남았는데 파혼..`, `부모님 결혼 반대(나는 남자)`
- full-post screenshots captured this run: 0
- source bytes acquired: 0
- real source-backed carousel: NO
- A1/P1: 0
- `publicationAllowed=false` 유지
- rights/privacy/human review gate 유지

다음은 실제 source screenshot 확보와 intake/normalization 후 cover + 원문 전체 carousel 제작/Chrome 검증이다. 게시 권한은 계속 `04_REVIEW_PUBLISH`만 가진다.

---

## 원본 기록: 116-sol.md

### 116-sol — 04:35 Discovery refresh

시각: 2026-09-17 04:35 KST

Threads `00_START_HERE/README.md`, 최신 handoff, current main/recent commits, 최신 `115-sol.md`를 먼저 확인하고 repo tip 기준으로 작업했다.

#### 실제 작업
공개 search/index에서 한국 커뮤니티 우선 질의와 Reddit/social 레인을 합쳐 raw 40+ lead/result를 검토했다. 제한 소스는 public index/search 범위만 사용했고 로그인/anti-bot 우회나 bulk crawl은 하지 않았다.

`data/candidates/`에 후보별 Markdown 1개씩 **신규 15건(12 C1 + 3 C0)**을 추가했다. exact individual URL이 없는 Blind index lead는 C0로 유지했고 URL/본문/수치를 만들지 않았다. 후보 파일은 제목 → 글 내용 → 정확한 링크 → 동일 관측시점 실제 수치 → 평가 → 형식 → 이미지/자산/게시상태 구조를 유지했다.

상위 신규:
- `Am I wrong for giving my brother a "debt forgiveness" card as his wedding gift instead of cash?` Reddit +794 — $3,500 대여 후 미상환/제트스키·Vegas 소비 → 남은 $3,100 채무탕감을 결혼선물로 줬더니 동생이 반발.
- `AITAH for refusing to give my stepmother my late mom’s wedding dress after she altered it behind my back?` BORUpdates +3,038 — 유품 웨딩드레스 몰래 수선/복원비/가족 거짓말. 재게시 provenance라 원 출처·권리 재검토 필요.
- `AITA for not helping my sister pay for her wedding?` Reddit +3,943 — $3,000 선물을 받고 "나머지"를 요구.
- `AITAH for pretending that I quit my job because my partner kept devaluing it?` BORUpdates +2,747 — 직장 가치폄하에 퇴사한 척한 부부 갈등.
- `화담숲 예약 킹받는다.` Blind index 19K/346/332 — 개별 URL 미확인이라 C0.
- `교사 7년간 월급으로 비트코인만 모았어요. 질문에 답변 드립니다.` — Blind public index가 exact 원문 URL을 노출했으나 이번 회차 본문/성과/계좌 인증은 확인하지 않아 추정하지 않음.
- 영수증 수백 장에 소설을 써 출간했다는 Reddit shared-media post +110,406 — source media bytes 미확보.

#### Asset / publication truth
- full-post screenshots captured: 0
- actual source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0
- all new: ASSETS_PENDING
- publicationAllowed=false
- rights/privacy/human review gate 유지
- OCR/moderation/rights/live publication 성공 주장 없음

#### Test truth
Discovery-only 실행 환경에는 runnable checkout/Node/Chrome process가 없어 `npm run check`, server smoke, browser E2E는 실행하지 않았고 pass를 주장하지 않는다.

Threads handoff update commit: `672918fa0059504c048a143f3c22b2eeebdaefd5`.

다음은 강한 한국 C0의 exact URL/full body 승격과 실제 full-post screenshot/source media 확보가 우선이다. 게시 권한은 계속 `04_REVIEW_PUBLISH`만 가진다.

---

## 원본 기록: 117-sol.md

### 117-sol — screenshot normalization ordering hardening

시각: 2026-09-17 05:18 KST

Threads 최신 `NEXT_RUN_HANDOFF.md`, current main/recent commits, `00_START_HERE/README.md`, 최신 `116-sol.md`를 확인하고 repo tip 기준으로 작업했다.

#### 실제 작업
`scripts/plan-screenshot-normalization.mjs`를 수정했다.

- empty intake manifest 거부
- `sourceSequence` 누락/0 이하/비정수 거부
- duplicate `sourceSequence` 거부
- 이미 strictly increasing order가 아닌 입력 거부
- 결과 plan에 `sourceSequencePolicy=PRESERVE_STRICT_INPUT_ORDER` 기록
- 기존 1080×1080 `CONTAIN_NO_STRETCH`, body crop 금지, UI chrome은 manual/verified suggestion만, privacy masking은 user-directed only 유지
- `publicationAllowed=false`, publish owner `04_REVIEW_PUBLISH` 유지

Threads implementation commit: `79697759f766521b10b2fe934fabd57612b9e70f`.
Handoff update commit: `6b27918b9cb9f4361c511da2e396e576c11a3293`.

#### Discovery / asset truth
이번 회차는 구현 회차라 새 discovery run을 수행하지 않았다. 최신 baseline은 raw 40+ / retained 15 (12 C1 + 3 C0).

- full-post screenshots: 0
- actual source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0
- ASSETS_PENDING 유지

#### Test truth
현재 automation 환경은 GitHub file operation만 제공하고 runnable checkout/Node/Chrome process는 제공하지 않았다. 따라서 `npm run check`, server smoke, browser E2E는 실행하지 않았으며 pass를 주장하지 않는다.

다음은 강한 한국 후보의 실제 전체 원문 screenshot/source media 확보 → intake → hardened normalization → cover + full-post screenshot carousel → Chrome 검증 순서다. 게시 권한은 계속 04만 가진다.

---

## 원본 기록: 118-sol.md

### 118-sol — 05:34 Discovery refresh

시각: 2026-09-17 05:34 KST

Threads `00_START_HERE/README.md`, 최신 `NEXT_RUN_HANDOFF.md`, current main/recent commits, 최신 `117-sol.md`를 확인하고 repo tip 기준으로 작업했다.

#### 실제 작업
공개 search/index를 한국 커뮤니티 우선으로 여러 레인에서 확인했고 raw 40+ leads/results를 검토했다. 제한 소스 bulk crawl, 로그인/anti-bot 우회는 하지 않았다. 범용 시황/종목뉴스와 이야기성이 약한 결과는 제외하고 신규 C1 7건을 `data/candidates/`에 후보당 Markdown 1개로 저장했다.

상위 신규:
- Blind `하이닉스 37억 몰빵 풀매수` — 관측 58K / 좋아요 54 / 댓글 1,137, source image 존재 확인. 실제 bytes는 미확보.
- Blind `협의이혼시 이런 경우는 재산분할 어떻게해?` — 몰래 주식·비트코인 대출 반복 → 1억원 빚 → 이혼.
- Blind `결혼 주선자 사례 X, 청첩장 못 받음. 축의금 해야돼?` — 주선한 커플에게 청첩장도 못 받은 뒤 60만원 축의 고민.
- Reddit `AITA for resciding our gift for SIL's wedding?` — 미리 거절했던 유럽여행 선물을 출발 직전 다시 요구해 약 $4K 추가비용 갈등, 관측 score +4,018.

#### Discovery / asset truth
- raw inspected: 40+
- retained: 7 C1
- full-post screenshots: 0
- actual source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0
- ASSETS_PENDING 유지
- publicationAllowed=false
- rights/privacy/human review gate 유지
- 실제 게시 권한은 04_REVIEW_PUBLISH only

#### Test truth
Discovery/data-only 회차로 runtime/production code는 수정하지 않았다. npm check/server smoke/browser E2E는 실행하지 않았고 pass를 주장하지 않는다.

다음 concrete priority는 `하이닉스 37억 몰빵 풀매수` → `주식으로 8천날림` → `[인증] 하루 5.2억 손실, 한 달 15억 손실`의 실제 전체 원문 screenshot/source media 확보 후 intake → normalization → cover + full-post screenshot carousel이다.

---

## 원본 기록: 119-sol.md

### 119-sol — 06:16 screenshot completeness guard

시각: 2026-09-17 06:16 KST

Threads 최신 `NEXT_RUN_HANDOFF.md`, current main/recent commits, 최신 `118-sol.md`를 확인하고 repo tip 기준으로 작업했다.

#### 실제 작업
`Threads/scripts/plan-screenshot-normalization.mjs`를 수정해 source screenshot sequence가 반드시 1부터 시작하고 중간 번호 없이 연속되도록 강화했다. 기존 strict-increasing 검증만으로는 `1,3` 또는 `2,3` 같은 불완전 screenshot 묶음이 통과할 수 있었는데 이제 normalization 전에 거부한다.

유지된 계약:
- 1080×1080 CONTAIN_NO_STRETCH
- body crop 금지
- UI chrome crop은 manual/verified suggestion only
- privacy masking은 user-directed only
- publicationAllowed=false
- publish owner는 04_REVIEW_PUBLISH only

#### Discovery / asset truth
이번 회차는 구현 회차라 신규 discovery 수치를 만들지 않았다. 최신 완료 discovery baseline은 raw 40+ / retained 7 C1이다.
- full-post screenshots: 0
- actual source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0

#### Test truth
GitHub contents 경로로 실제 repo 변경은 완료했다. 이 runtime에서는 executable checkout/Node/Chrome 경로를 확보하지 못했으므로 targeted Node test, npm run check, server smoke, browser E2E는 실행하지 않았고 pass를 주장하지 않는다.

다음 우선순위는 상위 한국 후보의 실제 전체 원문 screenshot/source media 확보 → intake → contiguous normalization → cover + 전체 원문 screenshot carousel → Chrome 검증이다.

---

## 원본 기록: 120-sol.md

### 120-sol — 06:39 Discovery refresh

시각: 2026-09-17 06:39 KST

Threads `00_START_HERE/README.md`, `NEXT_RUN_HANDOFF.md`, current main/recent commits, 최신 `119-sol.md`를 먼저 확인하고 repo tip 기준으로 작업했다.

#### 실제 작업
공개 search/index 경로에서 한국 커뮤니티 우선으로 여러 레인을 탐색했다. 로그인/anti-bot 우회나 restricted-source bulk crawl은 하지 않았다. query set 전체에서 raw lead/result 약 40+를 검토하고 dedupe/story-value/evidence 기준으로 exact individual Blind URL이 확인된 신규 C1 3건을 `data/candidates/`에 후보당 Markdown 하나씩 추가했다.

Retained:
1. `빚 그리고 결혼 어떻게해야할까` — 투자손실 약 5천만원 + 빚 약 9천만원 + 결혼 전 재정고백 갈등. 1,024 views / 4 comments.
2. `상대방 부모님 빚.. 결혼 괜찮을까요?` — 상대 어머니 사업부도 후 30~40억원 채무·개인파산 준비를 알게 된 결혼 고민. 678 views / 11 comments.
3. `6년만에 하는 인증` — 약 30년 투자자가 과거 숏(풋) 몰빵과 풀 신용·미수로 두 차례 크게 실패한 경험을 설명. 31K views / 349 comments.

관측시점은 2026-09-17 06:39 KST. 같은 공개 관측에서 실제 보인 수치만 기록했고 확인되지 않은 좋아요·asset/OCR/rights 정보는 만들지 않았다.

#### Asset / publication truth
- full-post screenshots: 0
- actual source bytes: 0
- real source-backed carousel: NO
- 신규 전부 C1_A0_P0 / ASSETS_PENDING
- publicationAllowed=false
- rights/privacy/human review gate 유지
- 실제 게시 권한은 04_REVIEW_PUBLISH only

다음은 한국 커뮤니티 고볼륨 탐색을 계속하되 돈/결혼 편중을 줄여 황당·직장·오해·반전 레인을 더 확보하고, 동시에 상위 후보의 실제 전체 원문 screenshot/source media acquisition을 진행한다.

---

## 원본 기록: 121-sol.md

### 121-sol — screenshot normalization verification gate

시각: 2026-09-17 07:14 KST

Threads `NEXT_RUN_HANDOFF.md`, current main/recent commits, 최신 `120-sol.md`를 먼저 확인하고 repo tip 기준으로 작업했다.

#### 실제 작업
`package.json`의 `npm run syntax`에 `scripts/plan-screenshot-normalization.mjs`를 추가했다. 최근 추가된 원문 screenshot normalization planner가 기존 normal `npm run check` 경로의 syntax 검증 대상에서 빠져 있던 공백을 막았다.

Planner 계약 자체는 유지된다: `sourceSequence`는 1부터 연속이어야 하고, 1080×1080 `CONTAIN_NO_STRETCH`, body crop 금지, UI chrome crop은 manual/verified suggestion only, privacy masking은 user-directed only다.

#### 검증 truth
- material repo change: YES
- normal check path에 planner static syntax gate 포함: YES
- 이번 connector-only runtime에서 `npm run check` 실제 실행: NO
- server smoke: NO
- browser E2E: NO
- publication/provider action: NO

실행하지 않은 테스트 성공, OCR/moderation, rights, asset acquisition, publication은 주장하지 않는다.

#### Discovery / asset truth
직전 Discovery baseline 유지:
- raw 40+
- retained 3 C1
- top: `6년만에 하는 인증`, `빚 그리고 결혼 어떻게해야할까`, `상대방 부모님 빚.. 결혼 괜찮을까요?`
- full-post screenshots 0
- actual source bytes 0
- real source-backed carousel NO
- A1/P1 0

다음 executable checkout에서 `npm run check`를 실제 수행하고, 동시에 `하이닉스 37억 몰빵 풀매수` 등 상위 후보의 전체 원문 screenshot acquisition을 우선한다. 04_REVIEW_PUBLISH만 게시 가능하다.

---

## 원본 기록: 122-sol.md

### 122-sol — 07:39 Discovery refresh

시각: 2026-09-17 07:55 KST

Threads `README.md`, `NEXT_RUN_HANDOFF.md`, current main/recent commits, 최신 `121-sol.md`를 먼저 확인하고 repo tip 기준으로 작업했다.

#### 실제 작업
공개 search/index에서 한국 커뮤니티 우선 질의와 Reddit/social 레인을 폭넓게 확인했다. 제한/차단 소스에 로그인·anti-bot 우회나 bulk crawl은 하지 않았다. raw 40+ lead를 검토했고 신규 후보 4건을 `data/candidates/`에 후보별 Markdown으로 저장했다.

- C1 Reddit 3건: $800 결혼선물 보류, $3,000 결혼선물 뒤 잔액 요구, 결혼식 식재료 $1,700 카드 결제 갈등.
- C0 한국 투자 1건: SK하이닉스 약 70억원 투자 후 약 18억원 평가손실 인증 lead. 공개 보도 provenance는 확인했지만 원 DCInside individual URL은 확인하지 못해 C0로 유지했다.

#### truth
- raw inspected: 40+
- retained: 4 (C1 3 + C0 1)
- full-post screenshots: 0
- actual source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0
- publicationAllowed=false 유지
- OCR/moderation/rights/publication success 추정 없음
- `npm run check`, server smoke, browser E2E: 이번 connector/search runtime에서는 실행하지 않음

다음 우선순위는 한국 funny/workplace/reversal 레인 비중 확대와 상위 투자 후보의 exact original URL + 전체 원문 screenshot 확보다. 04_REVIEW_PUBLISH만 게시 가능하다.

---

## 원본 기록: 123-sol.md

### 123-sol — screenshot evidence preservation

시각: 2026-09-17 08:15 KST

Threads `NEXT_RUN_HANDOFF.md`, current main/recent commits, 최신 `122-sol.md`를 확인하고 repo tip 기준으로 작업했다.

#### 실제 작업
`scripts/plan-screenshot-normalization.mjs`를 수정했다. screenshot intake에서 기록한 provenance가 normalization 단계에서 사라지지 않도록 다음을 강제한다.

- manifest exact public `sourceUrl` 필요
- `orderedAssetCount`와 실제 asset 수 일치 필요
- 각 asset의 SHA-256 / byteLength / captureUrl / acquisitionState / provenance 필요
- captureUrl은 manifest sourceUrl과 일치해야 함
- 중복 source hash 거부
- normalization plan에 SHA-256, byte length, capture URL, acquisition state, provenance를 그대로 전달

기존 contiguous sourceSequence from 1, `CONTAIN_NO_STRETCH`, body crop 금지, UI chrome crop 수동/검증 제안만, privacy masking 사용자 지시만 허용 규칙은 유지했다. full-body completeness, OCR, moderation, rights, publication readiness는 추론하지 않는다.

#### truth
- material repo change: YES
- latest Discovery baseline raw: 40+
- latest retained: 4 (C1 3 + C0 1)
- 이번 production run 신규 discovery 주장: 없음
- full-post screenshots: 0
- actual source bytes: 0
- real source-backed carousel: NO
- A1/P1: 0
- publicationAllowed=false 유지
- `npm run check`: GitHub connector runtime이라 실행하지 못함
- server smoke/browser E2E: 실행하지 않음; 변경 경로는 CLI/data pipeline

다음은 실제 source bytes 확보 후 intake → normalization을 실행 가능한 checkout에서 검증하고, 실제 원문 전체 screenshot carousel을 만드는 단계다. 04_REVIEW_PUBLISH만 게시 가능하다.

---

## 원본 기록: 124-sol.md

### 124-sol — 08:37 Discovery refresh

시각: 2026-09-17 08:37 KST

Threads `00_START_HERE/README.md`, `NEXT_RUN_HANDOFF.md`, current main/recent commits, 최신 `123-sol.md`를 확인하고 repo tip 기준으로 작업했다.

#### 실제 작업
공개 search/index만 사용해 Korean-community-first 탐색을 수행했다. Blind/DCInside/FMKorea/TheQoo/Ruliweb/Ppomppu/Inven/Arca 및 Reddit 레인을 폭넓게 조회했고 40+ raw lead/result를 검토했다. Clien/Instiz robots 차단은 우회하지 않았다. 일반 시황/종목 분석, 단순 고조회수 연봉표처럼 story taste가 약한 결과는 버렸다.

신규 retained 3건:
- C1 `AITA “ being cruel” for telling my daughter that she will need to help pay back the money that I spent on her wedding` — Reddit exact URL 확인, score +9,412 관측.
- C1 `AITA for gifting a donation` — BORUpdates exact public URL 확인, score +1,353 관측. 원 OOP URL이라고 주장하지 않고 repost/update provenance 명시.
- C0 `연애경험없는데 제가 여자분에게 무례 범한건가요..` — Blind browse/index에서 views 204 / likes 3 / comments 1만 확인. exact individual URL과 전체 본문 미확인.

각 후보를 `data/candidates/260917_C*_A0_P0_*.md`로 한 건당 파일 하나씩 저장했다. 묶음 discovery JSON은 만들지 않았다.

#### truth
- raw inspected: 40+
- retained: 3 (C1 2 + C0 1)
- full-post screenshots: 0
- actual source bytes: 0
- source-backed carousel: NO
- A1/P1: 0
- publicationAllowed=false 유지
- rights/privacy/human review gate 유지
- OCR/moderation/rights/publication success 주장 없음
- restricted-source bypass 없음

한국 검색 결과가 이번 회차에는 상대적으로 약하고 노이즈가 많아 15~30건을 억지로 채우지 않았다. 다음은 강한 한국 C0의 exact original URL 해소와 상위 투자 C1의 전체 원문 screenshot 확보가 우선이다. 04_REVIEW_PUBLISH만 게시 가능하다.

---

## 원본 기록: 125-sol.md

### 125-sol — Screenshot acquisition-state truthfulness

Time: 2026-09-17 09:19 KST

#### Read first
Repo tip/current handoff and recent commits were inspected before work. Latest prior sequential note was `124-sol.md`; repo tip wins.

#### Material change
Updated `scripts/build-screenshot-intake-manifest.mjs` in `kimjae134679/Threads`.

Previously every selected local screenshot was hard-coded as `USER_PROVIDED`. That could misstate provenance when a file was actually obtained by permitted manual capture or browser capture.

The intake CLI now requires an explicit `--acquisition-state` with one of:
- `USER_PROVIDED`
- `MANUAL_CAPTURE`
- `BROWSER_CAPTURE`

The selected value is recorded at manifest level and for every asset, with matching provenance language. The script explicitly states that acquisition method is supplied, not inferred. It still does not infer full-body completeness, exact source relationship, rights, privacy, OCR/vision, moderation, or publication.

Threads implementation commit: `e34fb2111e3bba745ccf2341346da2d22f7b23d6`.
Handoff update commit: `7488ae2e56868ebb10ddd6068f97e420bf058929`.

#### Discovery/source status
Carry-forward latest useful discovery baseline from 124-sol:
- raw candidate leads inspected: 40+
- retained: 3 (C1 2 + C0 1)
- top: wedding cancellation venue-cost repayment conflict; $500 memorial-donation wedding-gift conflict; Blind `연애경험없는데 제가 여자분에게 무례 범한건가요..` C0
- full-post screenshots captured: 0
- source bytes acquired: 0
- real source-backed carousel produced: no
- A1: 0
- P1: 0

No fake source screenshots/body cards were generated.

#### Verification
Current connector surface did not provide an executable checkout/Node/Chrome environment. Therefore `npm run check`, server smoke and browser E2E were not run and no green result is claimed.

#### Next
Acquire real full-post screenshots through permitted public/manual/browser paths, pass the truthful acquisition state to intake, manually verify full-body coverage, then normalize and build the first real cover + source-screenshot carousel. Preserve the role chain; only 04 may publish.

---

## 원본 기록: 126-sol.md

### 126-sol — High-volume Korean-first Discovery refresh

Time: 2026-09-17 09:48 KST

#### Read first
Read current `Threads/00_START_HERE/README.md`, `NEXT_RUN_HANDOFF.md`, current main/recent commits, and latest prior sequential note `125-sol.md` before work. Repo tip was treated as authoritative.

#### Discovery work
Public search/index exploration inspected 40+ raw leads across Korean-community-first queries and secondary Reddit/other public lanes. No Blind/DCInside bulk crawl, login bypass or anti-bot bypass was used. Clien/Instiz robots restrictions were respected rather than bypassed.

Retained 17 new candidates as one Markdown file per candidate in `Threads/data/candidates/`: 14 Blind C1 + 3 Reddit C1. No new grouped discovery JSON was created in `data/` root.

Strongest new Korean candidates include:
- `어쩌다 괴물이 되어버렸을까...` — 400만원 crypto start → 8,000만원 → Luna -99.99% → loan-backed stock retry → further loss.
- `죄의식이 낮은건가?` — secret leveraged crypto debt, rollover, husband and mother-in-law personal rehabilitation, strong spouse conflict.
- `이혼이 답인데 자식이 너무 맘에 걸린다` — repeated secret loans; expected crypto loss turns out to include severe game-spending addiction.
- `축의금 문화, 결혼 문화 10년내 다바뀔 듯` — observed 85K views / 433 likes / 543 comments.
- `돈 안갚는 친구` — 170만원 unpaid for two years, then friend proposes deducting that day's drinks from the old debt.
- `첫만남에 돈얘기 꺼냈던 황당 소개팅녀` — salary and post-marriage money-control questions on first dates.

Secondary retained Korean relationship/life candidates cover matchmaking/wedding snub, first-holiday in-law schedule, side-dish conflict, wedding guest-count anxiety, parent support conflict, fortune-based marriage objection, crypto addiction, twenty blind dates ending in marriage, and invitation etiquette.

Three Reddit C1s were retained only after Korean-first coverage: $1,000 intended wedding support escalating to $15,000 request (+7,958 observed score); invitations allegedly sent to a known non-attendee to collect gifts (+415); destination-wedding guest already spending $2K+ being criticized for giving customized art rather than cash (+84).

#### Truth state
- raw inspected: 40+
- retained: 17 C1_A0_P0
- full-post screenshots captured: 0
- actual source bytes acquired: 0
- real source-backed carousel: NO
- A1: 0
- P1: 0
- all new candidates: `publicationAllowed=false`, rights/privacy/human-review gate required
- no OCR/moderation/rights/publication success claimed

Candidate files summarize only body text actually read. Metrics are recorded only where visible in the same observation surface; missing likes/views/comments are explicitly left unconfirmed rather than invented.

#### Repo
Handoff updated in Threads commit `1b604f70a3dbfb116f65702e5e6c65c34762d0dc` after the candidate commits.

#### Verification
This run used connector/web surfaces and had no executable checkout/Node/Chrome path. `npm run check`, server smoke and browser E2E were therefore not executed or claimed.

#### Next
Stop spending the majority of implementation effort on additional abstract validators. Highest value is now permitted acquisition of the full original screenshot sequence for the strongest Korean C1s, then truthful intake → manual full-body verification → no-stretch normalization → first real cover + original-post screenshot carousel → Chrome inspection. Continue Korean high-volume discovery in parallel.

---

## 원본 기록: 127-sol.md

### 127-sol — Screenshot intake enters default syntax gate

Time: 2026-09-17 10:18 KST

#### Read first
Read current Threads handoff, recent main commits, and prior `126-sol.md`; repo tip treated as authoritative.

#### Material change
Updated `Threads/package.json` so `npm run syntax` now checks both screenshot-source pipeline scripts:
- `scripts/build-screenshot-intake-manifest.mjs`
- `scripts/plan-screenshot-normalization.mjs`

The normalization planner was already gated; the intake manifest builder was not. This closes that verification gap. Threads implementation commit: `f1e3b2747632db63149a3ebf0978d0c63392df1b`.

#### Truth state
Latest Discovery baseline remains raw 40+ / retained 17 C1_A0_P0 (14 Blind + 3 Reddit). Full-post screenshots: 0. Actual source bytes: 0. Real source-backed carousel: NO. A1/P1: 0. `publicationAllowed=false`; only 04_REVIEW_PUBLISH may publish.

No source screenshot bytes were available through this connector-only run. No OCR, vision, moderation, rights, delivery or publication success is claimed.

#### Verification
Executable checkout/Node/Chrome were unavailable through the current connector surface. Therefore `npm run check`, server smoke and browser E2E were not executed or claimed green.

#### Next
Highest value remains acquiring a permitted full original screenshot sequence for a strong Korean C1, then truthful intake → full-body manual verification → no-stretch normalization → first real cover + source screenshot carousel → Chrome inspection. Continue Korean-first high-volume Discovery in parallel.

---

## 원본 기록: 128-sol.md

### 128-sol — Korean-first Discovery refresh

Time: 2026-09-17 10:42 KST

#### Read first
Read current Threads `00_START_HERE/README.md`, `NEXT_RUN_HANDOFF.md`, recent main commits, and `127-sol.md`; repo tip treated as authoritative.

#### Discovery
Public search/index exploration covered 40+ leads across Korean-community-first queries and Reddit/other public lanes. No login, bulk crawl, anti-bot or robots bypass was used; Clien/Instiz robots blocks were left blocked.

Retained one genuinely usable new C1 rather than filling the queue with generic or old low-story-potential results:
- Blind `이혼 고민`
- exact public URL: https://www.teamblind.com/kr/post/%EC%9D%B4%ED%98%BC-%EA%B3%A0%EB%AF%BC-i20y0f3w
- visible full body read: yes
- comments: only publicly exposed comments partially read
- same observed public snapshot: 조회수 192 / 댓글 16; original-post like count not verified
- story: highly compatible partner with several strengths, but writer discovers after marriage that partner borrowed money to invest; trust/money/marriage conflict.

Candidate file: `data/candidates/260917_C1_A0_P0_이혼고민_결혼중빚투발견.md`
Candidate commit: `3ee34058c03c88d48ba5ebd0be993ddd15464e7a`.

#### Truth state
- raw inspected: 40+
- retained this run: 1 C1_A0_P0
- full-post screenshots captured: 0
- actual source bytes acquired: 0
- real source-backed carousel: NO
- A1/P1: 0
- publicationAllowed=false; only 04_REVIEW_PUBLISH may publish.

No OCR, vision, moderation, rights, asset acquisition, delivery or publication success is claimed. Executable checkout/Node/Chrome were unavailable through this connector surface, so npm check/server smoke/browser E2E were not executed or claimed.

#### Next
Acquire permitted full-post screenshot sequences for the strongest Korean C1s, then truthful screenshot intake → manual full-body verification → no-stretch normalization → first real cover + source-screenshot carousel → Chrome inspection. Continue Korean-first high-volume Discovery in parallel without filler.

---

## 원본 기록: 129-sol.md

### 129-sol — Korean-first Discovery refresh

Time: 2026-09-17 11:38 KST

#### Read first
Read current Threads `00_START_HERE/README.md`, `NEXT_RUN_HANDOFF.md`, current main/recent commits, and latest sequential ops note `128-sol.md`; repo tip treated as authoritative.

#### Discovery
Public search/index exploration covered 40+ leads across Korean-community-first queries plus Reddit/other public lanes. No login, bulk crawl, anti-bot or restricted-source bypass was used.

Retained four new C1_A0_P0 candidates as individual Markdown files:
- Blind `이혼 고민 (빚쟁이인 나...백수 남편)` — exact URL; observed 조회수 24K / 댓글 260. 코인빚 6,400만원, 장기 미취업 배우자, 추가 금전 및 집안/학력 갈등.
- Blind `자꾸 빚 내서 미국주식 사자는 남편` — exact URL; observed 조회수 1,078 / 댓글 11. 약 2억원 투자 중 약 1억원 회사대출, 배우자 병원대출까지 받아 테슬라/엔비디아/비트코인 추가매수 제안.
- Blind `헤어지는게 맞을까..?` — exact URL; observed 조회수 610 / 댓글 14. 재산보다 큰 빚으로 코인/선물/주식 투자, 절반 손실 및 결혼 전 약속 위반으로 결혼 5개월 신뢰 붕괴.
- Blind `빚 숨기고 결혼한 남편` — exact URL; observed 조회수 117 / 댓글 5. 기존 빚 외 휴대폰 경품 뽑기 중독으로 추가 1,800만원 빚 고백, 임신/결혼 갈등.

Candidate files are under `data/candidates/`; no grouped discovery JSON was created.

#### Truth state
- raw inspected: 40+
- retained: 4 C1_A0_P0
- full-post screenshots captured: 0
- actual source bytes acquired: 0
- real source-backed carousel: NO
- A1/P1: 0
- publicationAllowed=false; rights/privacy/human review required; only 04_REVIEW_PUBLISH may publish.

No OCR, vision, moderation, rights, asset acquisition, delivery or publication success is claimed. Executable checkout/Node/Chrome were unavailable through this connector surface, so npm check/server smoke/browser E2E were not executed or claimed.

#### Next
Acquire permitted full-post screenshot sequences beginning with the 24K/260-comment crypto-debt marriage conflict, then truthful screenshot intake → manual full-body verification → no-stretch normalization → first real cover + source-screenshot carousel → Chrome inspection. Continue Korean-first high-volume Discovery in parallel without filler.

---

## 원본 기록: 130-sol.md

### 130-sol — full-body verification gate

Time: 2026-09-17 12:16 KST

#### Read first
Read current Threads `NEXT_RUN_HANDOFF.md`, current main/recent commits, and latest sequential ops note `129-sol.md`; repo tip treated as authoritative.

#### Material implementation
Updated `scripts/plan-screenshot-normalization.mjs` so a screenshot sequence can no longer enter normalization while `fullBodyCaptureStatus` is the intake default `pending`.

Normalization now requires all of:
- `fullBodyCaptureStatus=VERIFIED_COMPLETE`
- valid `fullBodyVerifiedAt`
- `fullBodyVerificationMethod=HUMAN_REVIEW` or `USER_CONFIRMED`

It also requires every asset acquisition state to match the manifest acquisition state. Existing contiguous source order, SHA-256/byte evidence, exact source URL, no-stretch 1080×1080 containment, body-crop prohibition, and user-directed-only privacy masking remain intact.

Implementation commit: `b62062988d3d1e94e37e5cc4a028d8cdd67d755a`.

#### Truth state
Latest Discovery baseline remains raw 40+ / retained 4 C1_A0_P0. Top candidate remains Blind `이혼 고민 (빚쟁이인 나...백수 남편)` with observed 24K views / 260 comments.

- full-post screenshots captured: 0
- source bytes acquired: 0
- real source-backed carousel: NO
- A1/P1: 0
- publicationAllowed=false; only 04_REVIEW_PUBLISH may publish.

No OCR/vision/moderation/rights/publication success is claimed. Executable checkout/Node/Chrome were unavailable through this connector surface, therefore npm check/server smoke/browser E2E were not run or claimed.

#### Next
Acquire the first complete permitted Korean C1 screenshot sequence, intake it with truthful acquisition provenance, explicitly verify full-body completeness, normalize, produce the first real cover + source-screenshot carousel, and inspect it in Chrome.

---

## 원본 기록: 131-sol.md

### 131-sol — Discovery refresh

Time: 2026-09-17 12:35 KST

#### Read first
Read current Threads `00_START_HERE/README.md`, `NEXT_RUN_HANDOFF.md`, recent main commits, and latest sequential note `130-sol.md`. Repo tip treated as authoritative.

#### Discovery
Inspected 40+ public/index/search leads across Korean-community, marriage/family/money, investment/debt, workplace and Reddit lanes without bypassing restricted-source access controls.

Retained four new C1_A0_P0 candidates as one Markdown file per candidate under `data/candidates/`:
- `AITA for refusing to take out a loan to pay for my brother's wedding?` — observed Reddit score +5,192.
- `AITA: broke sister won’t pay back rich brother` — +3,432.
- `AITA for not helping my sister pay for her wedding?` — +3,943.
- `AITA for not helping my sister pay for her wedding but helping our family go to the wedding?` — +547.

All four had exact individual public URLs and body text actually read. Only same-observation visible metrics were recorded. No filler was retained to satisfy a numeric quota. Korean public search also surfaced workplace material, but a sensitive distress-oriented item was not retained.

#### Truth state
- full-post screenshots captured: 0
- source bytes acquired: 0
- real source-backed carousel: NO
- A1/P1: 0
- all new candidates ASSETS_PENDING
- publicationAllowed=false; only 04_REVIEW_PUBLISH may publish

No OCR/vision/moderation/rights/publication success is claimed.

#### Next
Continue Korean-first exact-URL discovery while prioritizing permitted full-post screenshot acquisition for the strongest existing Korean C1. Then intake with truthful provenance, explicitly verify full-body completeness, normalize, and create the first real source-backed carousel.

---

## 원본 기록: 132-sol.md

### 132-sol — explicit screenshot completeness transition

Time: 2026-09-17 13:17 KST

#### Read first
Read current Threads NEXT_RUN_HANDOFF, recent main commits, and latest sequential note `131-sol.md`. Repo tip treated as authoritative.

#### Material implementation
Added `scripts/verify-screenshot-intake-complete.mjs` as an explicit transition between screenshot intake and normalization. It requires a non-empty contiguous ordered source sequence plus an explicit `HUMAN_REVIEW` or `USER_CONFIRMED` method, then records `fullBodyCaptureStatus=VERIFIED_COMPLETE`, verification timestamp and method. It keeps `publicationAllowed=false` and `publishOwner=04_REVIEW_PUBLISH` and does not infer rights/privacy/OCR/vision/moderation/publication readiness.

Added the verifier to `npm run syntax` so it is part of the repository check path.

#### Truth state
Latest discovery baseline remains raw 40+ / retained 4 C1_A0_P0 from the 12:35 refresh. Full-post screenshots captured: 0. Source bytes acquired: 0. Real source-backed carousel: NO. A1/P1: 0.

No screenshot bytes were available in this connector-only run. Executable checkout/Node/Chrome were unavailable, so npm check/server smoke/browser E2E were not executed and no success is claimed.

#### Next
Acquire a permitted complete screenshot sequence for a strong Korean C1, then run intake → explicit completeness verification → normalization → real 1080x1080 cover + source screenshot carousel, followed by Chrome visual inspection.

---

## 원본 기록: 133-sol.md

### 133-sol — Korean-first discovery refresh

Time: 2026-09-17 13:34 KST

#### Read first
Read current Threads `00_START_HERE/README.md`, `NEXT_RUN_HANDOFF.md`, recent main commits and latest sequential note `132-sol.md`. Repo tip treated as authoritative.

#### Discovery work
Used public search/index/pages only; no login, anti-bot bypass or bulk crawling. Inspected 40+ raw leads across Korean-community-first lanes plus secondary Reddit discovery and rejected weak market/news/filler.

Retained 3 new candidate Markdown files under `data/candidates/`:
1. C1 `황당 면접 후기` — Blind exact public URL, full body read; observed 4,000 views / 5 likes / 13 comments.
2. C1 `직장내괴롭힘 피해 직원을 징계한 회사` — Blind exact public URL, full body read; observed 1,012 / 6 / 10; heightened defamation/privacy review required.
3. C0 `나 똥차 타는데 소개팅 태우러간다했네..` — public Blind index observed 19K / 16 / 165, but exact individual URL/full body not verified, so deliberately kept C0 and marked body unconfirmed.

Observation time for these metrics: 2026-09-17 13:34 KST. Metrics from different observation times were not merged.

#### Truth state
Full-post screenshots captured: 0. Source bytes acquired: 0. Real source-backed carousel: NO. A1/P1: 0. All candidates publicationAllowed=false; rights/privacy/human review remain gated and only 04_REVIEW_PUBLISH may publish. No OCR/moderation/asset/publication success claimed.

#### Next
Resolve exact URL/full body for the high-response Blind C0, then acquire a permitted complete screenshot sequence for a strong Korean C1 and run intake → completeness verification → normalization → first real 1080x1080 source-backed carousel.

---

## 원본 기록: 134-sol.md

### 134-sol — screenshot normalization regression coverage

Time: 2026-09-17 14:14 KST

Read current Threads handoff, recent main commits and latest sequential note 133-sol; repo tip treated as authoritative.

#### Material work
Added `test/screenshot-normalization-plan.test.mjs`. It exercises the source-first normalization boundary directly: pending completeness must fail; `VERIFIED_COMPLETE` with `HUMAN_REVIEW` must pass; machine-only `AUTO_OCR` completeness must fail. Accepted plans are asserted to remain `publicationAllowed=false`, owned by `04_REVIEW_PUBLISH`, 1080x1080 contain/no-stretch, body-crop forbidden and privacy masking user-directed only.

An attempted package wiring accidentally narrowed the existing syntax-check list. This was immediately reverted at repo tip (`fbbd13bc...`) rather than leaving a regression. The targeted test file remains. Connector-only execution provides no Node/Chrome checkout, so the new test, `npm run check`, server smoke and browser E2E were not run and no green result is claimed.

#### Discovery / asset truth
Latest discovery baseline remains raw 40+ / retained 3 new (2 C1 + 1 C0). Top candidates remain Blind `황당 면접 후기`, `직장내괴롭힘 피해 직원을 징계한 회사`, and index-only C0 `나 똥차 타는데 소개팅 태우러간다했네..`.

Full-post screenshots: 0. Source bytes: 0. Real source-backed carousel: NO. A1/P1: 0. No OCR/moderation/rights/publication success claimed.

#### Next
Acquire a complete permitted screenshot sequence for a strong Korean C1, then run intake → explicit human/user completeness verification → normalization → first real 1080x1080 cover + full-post screenshot carousel. Continue Korean-first discovery in parallel.

---

## 원본 기록: 135-sol.md

### 135-sol — Korean-first Discovery refresh

Time: 2026-09-17 14:50 KST

Read current Threads README, NEXT_RUN_HANDOFF, main/recent commits and latest 134-sol first; repo tip treated as authoritative.

#### Discovery work
Inspected 40+ public search/index/page leads with Korean-community-first queries covering Blind, DCInside, FMKorea, TheQoo, Instiz, Ruliweb, Ppomppu, Clien, Arca and global/Reddit fallback. Restricted/robots-blocked sources were not bypassed. Generic market/loan/news and weak leads were discarded.

Retained 5 new C1_A0_P0 candidate files, one Markdown per candidate under `data/candidates/`:
- Blind `나몰래 대출받은 남편` — 12K views / 257 comments; exact URL; public body only partially verified.
- Blind `신입사원 퇴사 레전드(고전&장문 주의)` — 18K / 19; exact URL; long body only partially verified.
- Blind `축의금 (진짜 난감 ㅠㅠ 조언 좀)` — 1,144 / 13; exact URL; public body read.
- Reddit `AITA for not giving a wedding gift?` — +415; exact URL; body read.
- Reddit `AITA for not putting my sister’s wedding expenses on my credit card and humiliating her?` — +7,972; exact URL; body only partially verified.

No grouped discovery JSON was created. Unknown metrics were left unknown rather than inferred.

#### Asset/publication truth
Full-post screenshots: 0. Source bytes: 0. Real source-backed carousel: NO. A1/P1: 0. All new records remain ASSETS_PENDING, publicationAllowed=false, rights/privacy/human-review gated. Only 04_REVIEW_PUBLISH may publish. No OCR/moderation/rights/publication success claimed.

#### Next
Acquire a permitted complete screenshot sequence for a strong Korean C1. `축의금 (진짜 난감 ㅠㅠ 조언 좀)` is immediately attractive because the public body was fully readable; the 12K/18K Blind candidates are stronger by reaction but require complete-body verification before screenshot completeness can be asserted.

---

## 원본 기록: 136-sol.md

### 136-sol — strict source-backed carousel planner

Time: 2026-09-17 15:14 KST

Read current Threads NEXT_RUN_HANDOFF, current main/recent commits and latest 135-sol first; repo tip treated as authoritative.

#### Material implementation
Added `scripts/build-source-backed-carousel-plan.mjs` after the existing verified screenshot normalization stage.

The planner rejects non-verified source sequences and enforces:
- slide 1 = `COVER_ONLY`, 1080x1080;
- cover text = original title by default, or an explicitly recorded exact strong phrase only;
- slide 2+ = `ORIGINAL_POST_SCREENSHOT` only, in contiguous source order;
- every body slide preserves SHA-256/capture URL/provenance evidence;
- `CONTAIN_NO_STRETCH` and `bodyCropAllowed=false` are mandatory;
- summary/paraphrase/explanation/reaction/CTA/rewritten-story body cards are forbidden;
- privacy masking remains `USER_DIRECTED_ONLY`;
- `publicationAllowed=false` and publish owner remains `04_REVIEW_PUBLISH`.

The output explicitly states that it is a production plan and is not proof of rendered assets, rights clearance, moderation, delivery or publication.

#### Verification truth
No executable checkout/Node/Chrome surface was available in this run, so targeted tests, `npm run check`, server smoke and Chrome E2E were not claimed as executed. No source screenshot bytes were available through the connector, so no fake carousel was generated.

#### Discovery / asset truth
Latest discovery baseline remains raw 40+ / retained 5 from the 14:50 refresh. Top Korean candidates remain `나몰래 대출받은 남편`, `신입사원 퇴사 레전드`, and `축의금 (진짜 난감 ㅠㅠ 조언 좀)`.

Full-post screenshots captured: 0. Source bytes: 0. Real source-backed carousel: NO. A1/P1: 0.

#### Next
Acquire one permitted complete Korean C1 screenshot sequence, then run intake → human/user completeness verification → normalization → strict carousel plan → real renderer → Chrome inspection. Continue high-volume Korean discovery in parallel.

---

## 원본 기록: 137-sol.md

### 137-sol — 15:35 discovery refresh

Time: 2026-09-17 15:35 KST

Read current Threads README, NEXT_RUN_HANDOFF, current main/recent commits and 136-sol first; repo tip treated as authoritative.

#### Discovery work
Inspected 40+ public search/index/page leads across Korean-community-first queries and Reddit/global fallback. Blind/DC/community access restrictions were respected; robots/login/anti-bot controls were not bypassed. Weak generic market/news/index-only items were not retained just to inflate count.

Retained 6 new exact-URL C1_A0_P0 candidates as one Markdown file per candidate under `data/candidates/`:
- parents demand half of ~£4M lottery winnings (+11,395 observed)
- €44k family-standard wedding loan demand → courthouse marriage (+871 observed today)
- cancelled wedding venue repayment conflict after daughter caused breakup (+9,412)
- $15k lottery win, husband gives stay-at-home wife $500 while giving others money (+260)
- Europe-trip wedding gift accepted at last minute after costs jump ~$4k (+4,018)
- sister mocked cheap wedding then expected sibling funding (+8,246)

Exact public URLs were verified. Only metrics visible at the same observation were recorded. Where full-body completeness was not certain, the candidate says so rather than filling gaps.

#### Asset/publication truth
Full-post screenshots: 0. Source bytes: 0. Real source-backed carousel: NO. A1/P1: 0. All new candidates remain ASSETS_PENDING and publicationAllowed=false. Rights/privacy/human review gates remain; only 04_REVIEW_PUBLISH may publish.

#### Next
Continue high-volume Korean-first discovery and exact-URL resolution, but highest concrete production blocker remains acquiring one permitted complete Korean source screenshot sequence for intake → human/user completeness verification → normalization → strict carousel plan → render/Chrome inspection.

---

## 원본 기록: 138-sol.md

### 138-sol — source-backed carousel plan validation guard

Time: 2026-09-17 16:16 KST

Read current Threads NEXT_RUN_HANDOFF, current main/recent commits and 137-sol first; repo tip treated as authoritative.

#### Material implementation
Added `scripts/validate-source-backed-carousel-plan.mjs` as an independent renderer-boundary integrity guard. It rejects plans unless:
- slide 1 is 1080x1080 `COVER_ONLY` with the original-title/exact-phrase policy;
- slide 2+ are contiguous ordered `ORIGINAL_POST_SCREENSHOT` entries;
- every body screenshot has SHA-256, capture URL and provenance;
- body uses contain/no-stretch and `bodyCropAllowed=false`;
- all body screenshots resolve to one consistent source/capture URL and agree with plan source URL;
- summary/paraphrase/explanation/reaction/CTA/rewritten-story body cards remain forbidden;
- privacy masking remains `USER_DIRECTED_ONLY`;
- `publicationAllowed=false` and publish owner remains `04_REVIEW_PUBLISH`.

This prevents a malformed or editorially reconstructed plan from silently reaching a future renderer even if an upstream artifact is hand-edited.

#### Verification truth
The connected GitHub surface has no executable checkout, so Node tests, `npm run check`, server smoke and Chrome E2E were not run and are not claimed. No API/moderation/OCR/rights/delivery/publication success was claimed.

#### Discovery / asset state
Latest discovery baseline remains raw 40+ / retained 6 from the 15:35 refresh. Top candidates remain the lottery/wedding/family-conflict set recorded in 137-sol. Full-post screenshots captured: 0. Source bytes: 0. Real source-backed carousel: NO. A1/P1: 0.

#### Next
Acquire one permitted complete Korean C1 screenshot sequence, then exercise intake → human/user completeness verification → normalization → strict carousel plan → validator → renderer/Chrome inspection on real source bytes.

---

## 원본 기록: 139-sol.md

### 139-sol — Korean-first Discovery refresh

Time: 2026-09-17 16:35 KST

Read current Threads README/NEXT_RUN_HANDOFF, current main/recent commits and 138-sol first; repo tip treated as authoritative.

#### Discovery work
Screened 40+ public search/index/page leads with Korean-community-first queries. Restricted/robots-blocked sources were not bypassed. Retained 5 new exact-URL Blind candidates as one Markdown file per candidate under `data/candidates/`; no grouped discovery JSON was created in `data/` root.

New C1_A0_P0:
1. `가족 끼리 엮인 빚 상담해주세요..` — family debt, 16% loan burden; 302 views / 11 comments; body partially read.
2. `첫만남에 돈얘기 꺼냈던 황당 소개팅녀` — first-date salary/financial-control questions; 950 views / 1 like / 32 comments; full body read.
3. `결혼을 앞둔 남자친구의 주식 빚 숨겨줘야할까?` — repeated investment accident, ~50M KRW stock debt before marriage; 4,054 views / 86 comments; body partially read.
4. `한 주식에 몰빵했는데 아직은 두렵지 않네` — 50M loan + 20M card loan all-in; 3,479 views / 5 likes / 13 comments; full body read; public page references two account images.
5. `결혼 주선자 사례 X, 청첩장 못 받음. 축의금 해야돼?` — introducer not invited / gift conflict; 4,072 views / 2 likes / 41 comments; full body read.

Top this run: leveraged all-in stock story, hidden stock-debt-before-marriage story, wedding-introducer invitation/gift conflict.

#### Truth / gates
Full-post screenshots captured: 0. Actual source bytes: 0. Real source-backed carousel: NO. A1/P1: 0. The leveraged-stock page visibly references attached account images, but those bytes were not acquired. All new candidates remain `ASSETS_PENDING`, `publicationAllowed=false`, rights/privacy/human review gated, and only `04_REVIEW_PUBLISH` may publish. No OCR/moderation/rights/delivery/publication success is claimed.

#### Next
Acquire one permitted complete Korean C1 screenshot sequence, ideally a source with full body and native images, then exercise screenshot intake → human/user completeness verification → normalization → strict carousel plan → validator → renderer/Chrome inspection on real source bytes.

---

## 원본 기록: 140-sol.md

### 140-sol — non-destructive UI-chrome crop review stage

Time: 2026-09-17 17:17 KST

Read current Threads NEXT_RUN_HANDOFF, current main/recent commits and latest 139-sol first; repo tip treated as authoritative.

#### Material implementation
Added `scripts/suggest-ui-chrome-crops.mjs` to the source-first screenshot path. It consumes a screenshot intake manifest and emits an ordered review artifact keyed to each sourceSequence/SHA/capture URL/dimensions. It intentionally does not infer or apply pixel crops: every entry starts `HUMAN_REVIEW_REQUIRED`, `autoApplied=false`, body preservation is mandatory, only browser/platform UI chrome may later be cropped, source order is immutable, and privacy masking remains `USER_DIRECTED_ONLY`.

This fills the missing crop-suggestion/review handoff between verified screenshot intake and normalization without fabricating OCR/vision or silently altering source content. `publicationAllowed=false` and publish owner `04_REVIEW_PUBLISH` remain binding.

#### Verification truth
GitHub connector provided repository writes but no executable checkout or real source screenshot bytes. Therefore no `npm run check`, server smoke, browser E2E, OCR/vision, pixel-crop analysis, or real-render success is claimed this run.

#### Discovery / asset truth carried forward
Latest useful discovery run: raw 40+, retained 5; top candidates remain `한 주식에 몰빵했는데 아직은 두렵지 않네`, `결혼을 앞둔 남자친구의 주식 빚 숨겨줘야할까?`, `결혼 주선자 사례 X, 청첩장 못 받음. 축의금 해야돼?`.

Full-post screenshots captured: 0. Actual source bytes: 0. Real source-backed carousel produced: NO. A1/P1: 0.

#### Next
Acquire one permitted complete Korean C1 screenshot sequence and exercise intake → explicit completeness verification → crop review → normalization → strict carousel plan → validator → real renderer/Chrome inspection.

---

## 원본 기록: 141-sol.md

### 141-sol — 17:34 Discovery refresh

Time: 2026-09-17 17:34 KST

Read current Threads README/NEXT_RUN_HANDOFF, current main/recent commits and latest 140-sol first; repo tip treated as authoritative.

#### Discovery work
Ran Korean-community-first public search/index/page queries across the requested community set, then public Reddit fallback. Restricted/robots-blocked pages were not bypassed. Inspected 40+ raw leads and retained 5 new exact-URL C1 candidates after dedupe/story/safety/access filtering.

Retained:
- `36M 32F Wife has wealthy parents, and I’m struggling with the fact that I have to work while my family vacations without me.` — Reddit, +1,731 observed
- `AITAH for not offering to re-pay my ex-fiancee's (25F) parents (50s M&F) for any of the cost they incurred for our wedding that I (28M) incurred?` — Reddit BORU, +4,550 observed
- `My wife is a sahm and has racked up 17k of debt in the past 11 months behind my back` — Reddit BORU, +3,042 observed
- `AITH for postponing my wedding after finding out about my fiance's debt?` — Reddit, +301 observed
- `AITAH for not bringing food to a co workers going to get married celebration celebration` — Reddit, +140 observed

Each is stored as one human-readable Markdown file under `data/candidates/`. No new grouped discovery JSON was created.

#### Access truth
Korean sources were prioritized, but this run did not obtain enough permitted exact individual Korean pages to create truthful new Korean C1 files. Robots/login restrictions were not bypassed and no URL/body/metric was invented.

#### Asset/publication truth
Full-post screenshots: 0. Actual source bytes: 0. Real source-backed carousel: NO. A1/P1: 0. All new candidates are C1_A0_P0 / ASSETS_PENDING / publicationAllowed=false with rights/privacy/human review gates intact. Only 04_REVIEW_PUBLISH may publish.

---

## 원본 기록: 142-sol.md

### 142-sol — reviewed UI-chrome crop gate

Time: 2026-09-17 18:18 KST

Read current Threads NEXT_RUN_HANDOFF, current main/recent commits and latest 141-sol first; repo tip treated as authoritative.

#### Material implementation
Added `scripts/apply-reviewed-ui-chrome-crops.mjs` to close the gap between non-destructive crop suggestions and normalization.

The gate:
- rejects unresolved `HUMAN_REVIEW_REQUIRED` decisions
- accepts only `KEEP_ORIGINAL` or `HUMAN_APPROVED_UI_CHROME_CROP`
- validates integer crop rectangles and source bounds
- requires explicit human confirmation that the FULL original post body/attached source media remain when a crop is approved
- preserves source order, SHA-256 and capture URL provenance
- does not mutate screenshot bytes itself
- does not auto-mask privacy/PII
- keeps `publicationAllowed=false`
- keeps `04_REVIEW_PUBLISH` as the sole publish owner

Threads implementation commit: `3bece89cf466e8adb613df846c36aeb16d538ef9`.

#### Discovery/asset truth
Fresh Korean-community-first public probing was performed; robots/access restrictions were not bypassed. No weak candidate was added merely to increase count. Latest meaningful discovery numbers remain raw 40+ / retained 5 from the 17:34 refresh.

Full-post screenshots captured: 0. Actual source bytes: 0. Real source-backed carousel: NO. A1/P1: 0. Candidates remain ASSETS_PENDING / publicationAllowed=false.

#### Test truth
This connector surface has no executable checkout, so targeted Node tests, `npm run check`, server smoke and Chrome E2E were not actually run and no success is claimed.

#### Next
Acquire a permitted complete source screenshot sequence, then run the entire source-first chain on real bytes and inspect the first real 1080x1080 carousel in Chrome.

---

## 원본 기록: 143-sol.md

### 143-sol — Korean-community discovery refresh

Time: 2026-09-17 18:45 KST

Read current Threads README/NEXT_RUN_HANDOFF, main/recent commits and latest 142-sol first; repo tip treated as authoritative.

#### Discovery
Public search/index/page probing was run Korean-community-first across multiple lanes. Restricted/robots-blocked sources were not bypassed.

Raw inspected: 40+ leads. Retained: 3 new candidate files (1 C1_A0_P0, 2 C0_A0_P0). No quota padding with weak/duplicate leads.

Top retained:
- Blind `소개팅해달라고 괴롭히는 상사`: exact public individual URL verified; public body partly read; views 1,903 / comments 36 at this observation surface. Full body not verified, comments not read.
- Blind index `나 도저히 아이 못낳겠다니까 계속 낳자는데`: exact individual URL unavailable, therefore C0; 1,174 / likes 6 / comments 15 on the same index observation.
- Blind index `양가 어른 용돈문제..`: exact individual URL unavailable, therefore C0; 1,137 / likes 10 / comments 4 on the same index observation.

Candidate files are human-readable Markdown under `data/candidates/`, one candidate per file. Unread material is explicitly marked and no URL/metric/body was invented.

#### Asset/publication truth
Full-post screenshots captured: 0. Actual source bytes: 0. Real source-backed carousel: NO. A1/P1: 0. New candidates remain ASSETS_PENDING / publicationAllowed=false with rights/privacy/human review gates. Only 04_REVIEW_PUBLISH may publish.

Threads handoff updated in commit `90e77faeb742f9b40bc3638fd1ee3e3afa8608f1` after candidate commits `c1583a2b`, `ba4d0d90`, `fa055635`.

---

## 원본 기록: 144-sol.md

### 144-sol — reviewed crop gate regression coverage

Time: 2026-09-17 19:19 KST

Read current Threads NEXT_RUN_HANDOFF, current main/recent commits, and latest `143-sol.md` first; repo tip treated as authoritative.

#### Material change
Added `test/reviewed-ui-chrome-crop-gate.test.mjs` in Threads commit `e5850392cd2de185b678fa0caa51dfec2ad080fa`.

The regression test locks the source-first reviewed UI-chrome crop gate:
- KEEP_ORIGINAL remains valid and produces no crop.
- unreviewed/HUMAN_REVIEW_REQUIRED decisions are rejected.
- HUMAN_APPROVED_UI_CHROME_CROP without explicit full-original-body/media preservation confirmation is rejected.
- out-of-bounds crop coordinates are rejected.
- valid human-approved UI-chrome crop is accepted.
- `publicationAllowed=false`, `publishOwner=04_REVIEW_PUBLISH`, body-crop prohibition, automatic-crop prohibition, automatic privacy masking prohibition and USER_DIRECTED_ONLY privacy state are rechecked.

No executable checkout is exposed by the GitHub connector in this run, therefore Node/npm/server/Chrome execution success is not claimed. Test committed but not executed here.

#### Discovery / asset truth
Discovery in this implementation run: raw 0, retained 0. Latest completed refresh remains raw 40+ / retained 3; top candidates remain Blind `소개팅해달라고 괴롭히는 상사`, `나 도저히 아이 못낳겠다니까 계속 낳자는데`, `양가 어른 용돈문제..`.

Full-post screenshots captured: 0. Actual source bytes: 0. Real source-backed carousel: NO. A1/P1: 0. `ASSETS_PENDING / publicationAllowed=false`; rights/privacy/human-review gates remain; only 04_REVIEW_PUBLISH may publish.

Threads handoff updated after the test commit.

---

## 원본 기록: 145-sol.md

### 145-sol — Korean-community Discovery refresh

Time: 2026-09-17 19:52 KST

Read current Threads README, NEXT_RUN_HANDOFF, current main/recent commits and `144-sol.md` first; repo tip treated as authoritative.

#### Discovery work
Public search/index/page exploration inspected raw 40+ leads with Korean-community-first queries and Reddit fallback. No login/anti-bot bypass or bulk crawl was used. Clien/Instiz surfaces were robots-blocked and were not bypassed.

Retained 9 new exact-public-URL C1 candidates, each as an individual Markdown file in `data/candidates/`:
- `이혼 고민 (빚쟁이인 나...백수 남편)` — Blind — 24K / comments 260.
- `맨날 사고치는 남편` — Blind — 7,226 / comments 65.
- `협의이혼시 이런 경우는 재산분할 어떻게해?` — Blind — 1,581 / comments 19.
- `결혼 전 청약된 집 빚 갚는 문제때문에 너무 싸움이 커진다..` — Blind — 174 / comments 9.
- `코로나 이후 자산 변화 현타 심함` — Blind — 382 / likes 2 / comments 2.
- `배우자 빚 회생 신청` — Blind — 879 / likes 4 / comments 2.
- `친동생 결혼 축의금 질문` — Blind — 2,153 / likes 3 / comments 13 / poll 238.
- `결혼 비용및 축의금 정산 의견 차이` — Blind — 1,966 / likes 2 / comments 21.
- `결혼준비중인데 상대방부모님께 이정도 지원될까?` — Blind — 81 / likes 1 / comments 23.

All metrics are only same-observation visible values. Candidate files state body/comments read status and acquisition truth. No grouped JSON was added to `data/` root.

#### Asset/publication truth
Full-post screenshots: 0. Source bytes: 0. Real source-backed carousel: NO. All new items C1_A0_P0 / ASSETS_PENDING / publicationAllowed=false. Rights/privacy/human review gates remain. Only 04_REVIEW_PUBLISH may publish. No OCR/moderation/API/publication success claimed.

Threads handoff updated after candidate commits. Next concrete priority is permitted full-post screenshot acquisition for the strongest C1, then first real source-backed carousel.

---

## 원본 기록: 146-sol.md

### 146-sol — source normalization validation gate

Time: 2026-09-17 20:17 KST

Read current Threads handoff, main/recent commits and latest sequential ops `145-sol.md`; repo tip treated as authoritative.

#### Material change
Added `scripts/validate-screenshot-normalization-plan.mjs` to independently validate the normalization output before it can feed carousel planning. Fail-closed checks cover contiguous source order, unique SHA-256 evidence, byte length, exact capture/source URL, acquisition/provenance, human/user verified full-body status, exact 1080x1080 contain/no-stretch math, body-crop prohibition, user-directed privacy masking, `publicationAllowed=false`, and `04_REVIEW_PUBLISH` publish ownership.

Role chain remains `01 DISCOVERY → 02 EDITORIAL_SCORING → 03 PRODUCTION → 04 REVIEW_PUBLISH → 05 EXPERIMENTS_ACCOUNTS`; no publication action was attempted.

#### Discovery / asset truth
This implementation run: raw candidates 0, retained 0. Current top candidates remain Blind `이혼 고민 (빚쟁이인 나...백수 남편)` (24K/comments260), `맨날 사고치는 남편` (7,226/comments65), and `협의이혼시 이런 경우는 재산분할 어떻게해?` (1,581/comments19).

Full-post screenshots captured: 0. Source bytes acquired: 0. Real source-backed carousel produced: NO. A1/P1: 0. Existing candidates remain ASSETS_PENDING / publicationAllowed=false.

#### Verification truth
No executable checkout/browser surface was available through the GitHub connector in this run. The new Node validator, `npm run check`, server smoke and Chrome E2E were therefore not executed; no test/browser success is claimed.

---

## 원본 기록: 147-sol.md

### 147-sol — Korean-first high-volume Discovery refresh

Time: 2026-09-17 20:38 KST

Read current Threads README/handoff, main/recent commits and latest sequential ops `146-sol.md`; repo tip treated as authoritative.

#### Discovery work
Public/index/search exploration covered Korean-community-first lanes including Blind, TheQoo, DCInside, FMKorea, Ruliweb, Ppomppu, Inven and Arca queries, then Reddit. Clien/Instiz returned robots restrictions and were not bypassed. No login/anti-bot bypass or bulk crawl was used.

Raw leads inspected: 40+.
Retained: 15 new candidates, each as one Markdown file under `data/candidates/`. No grouped discovery JSON was added to `data/` root. Exact individual public URLs were available for all retained candidates, so they are C1. All are A0/P0.

Strongest retained include:
- Blind `코인 하는 남편 간섭해도 될까요?` — 5,522 views / 55 comments observed.
- TheQoo `축의금 10만원으로 통일한다는 비혼친구... 너무 서운해요` — 66,831 / 592.
- TheQoo `반반결혼의 최후 (애로부부 캡쳐)` — 75,932 / 515.
- TheQoo `결혼 승낙 받자마자 탈모인거 밝힌 남편..` — 104,013 / 391; image body not read, explicitly recorded as `본문 미확인`.
- Reddit/WSB `Loss Porn` — +7,916 votes; $50k personal-loan trading loss story.

Other retained lanes: crypto gains repeatedly not sold, hidden fiancé debt, wedding-gift repayment to a possibly non-marrying friend, crypto/credit-line loss, boss-arranged dating rejection, mobile invitation gift etiquette, household-money conflict, wedding cancellation after family tragedy, and fiancé debt concealment.

#### Truth / gates
Full-post screenshots captured: 0. Source bytes acquired: 0. Real source-backed carousel produced: NO. A1/P1: 0. All new candidates remain `ASSETS_PENDING / publicationAllowed=false`. No rights, OCR, moderation, asset capture, delivery or publication success was fabricated. Role chain remains `01 DISCOVERY → 02 EDITORIAL_SCORING → 03 PRODUCTION → 04 REVIEW_PUBLISH → 05 EXPERIMENTS_ACCOUNTS`; only 04 may publish.

No executable checkout/browser E2E was available in this Discovery run, so `npm run check`, server smoke and Chrome E2E were not executed or claimed.

---

## 원본 기록: 148-sol.md

### 148-sol — source-first scripts added to check coverage

Time: 2026-09-17 21:16 KST

Read current Threads handoff, main/recent commits and latest sequential ops `147-sol.md`; repo tip treated as authoritative.

#### Material change
Found a verification gap: the source-first screenshot pipeline had grown beyond the scripts included in `package.json`'s `npm run syntax`. Updated the syntax command so CI/local `npm run check` now covers:
- `scripts/suggest-ui-chrome-crops.mjs`
- `scripts/apply-reviewed-ui-chrome-crops.mjs`
- `scripts/validate-screenshot-normalization-plan.mjs`
- `scripts/build-source-backed-carousel-plan.mjs`
- `scripts/validate-source-backed-carousel-plan.mjs`

The existing intake, completeness verification and normalization planner remain covered. This makes syntax regressions in the binding source-backed path visible to the normal check gate instead of silently escaping it.

#### Discovery / asset truth
This was an implementation run, not a new Discovery sweep: raw inspected 0 / retained 0. Latest completed Discovery remains raw 40+ / retained 15. Top candidates remain Blind `코인 하는 남편 간섭해도 될까요?`, TheQoo `축의금 10만원으로 통일한다는 비혼친구... 너무 서운해요`, TheQoo `반반결혼의 최후 (애로부부 캡쳐)`, TheQoo `결혼 승낙 받자마자 탈모인거 밝힌 남편..`, and Reddit/WSB `Loss Porn`.

Full-post screenshots captured this run: 0. Source bytes: 0. Real source-backed carousel: NO. A1/P1: 0. Existing candidates remain `ASSETS_PENDING / publicationAllowed=false`.

#### Gates / verification truth
Role chain remains `01 DISCOVERY → 02 EDITORIAL_SCORING → 03 PRODUCTION → 04 REVIEW_PUBLISH → 05 EXPERIMENTS_ACCOUNTS`; only 04 may publish. Privacy masking remains user-directed; no rights/OCR/moderation/delivery/publication success was fabricated.

Work was performed through the GitHub connector without an executable checkout/browser, so the expanded syntax command, `npm run check`, server smoke and Chrome E2E were not executed or claimed. The next executable CI/local run will now include these source-first scripts in the syntax gate.

---

## 원본 기록: 149-sol.md

### 149-sol — 21:34 Discovery refresh

Time: 2026-09-17 21:34 KST

Read current Threads `00_START_HERE/README.md`, `NEXT_RUN_HANDOFF.md`, main/recent commits, and latest sequential ops `148-sol.md`; repo tip treated as authoritative.

#### Discovery work
Performed Korean-community-first public search/index/page exploration across Blind and other requested lanes, with Reddit used only as secondary coverage. No login/anti-bot bypass or restricted bulk crawl was used. Raw leads inspected: **40+**. Retained: **16 new C1 candidates**, each as its own human-readable Markdown file under `data/candidates/`; no new grouped discovery JSON was created in `data/` root.

Strongest new candidates:
- Blind `축의금 내고 식권 안받으면` — 64K views / 51 likes / 384 comments.
- Blind `축의금 논란 , 제가 이상한가요?(특이케이스)` — 8,933 / 17 / 92.
- Blind `돌잔치 축의금 이게 맞는거야?` — 7,053 / 15 / 58.
- Blind `주식` — repeated 2억 stock-debt / repayment / rehabilitation / renewed-loss family story; 674 views / 17 comments; body not verified to end.
- Reddit `AITA for declining a late invite I got to a coworker’s wedding?` — +1,042 score.

Other retained lanes cover rotation-dating reversals, premarital savings disclosure, newlywed spending, family asset disclosure, spouse stock-loss/economic-control, wedding-gift settlement, friend wedding unpaid balance, and destination-wedding family/debt conflicts. Candidate files explicitly record whether body/comments were actually read and only metrics observed at the same observation point.

#### Asset / publication truth
Full-post screenshots captured: **0**. Actual source bytes: **0**. Real source-backed carousel: **NO**. A1/P1: **0**. All new candidates remain `C1_A0_P0 / ASSETS_PENDING / publicationAllowed=false`. Rights/privacy/human-review gates remain. Only `04_REVIEW_PUBLISH` may publish. No OCR/moderation/rights/delivery/publication success was fabricated.

#### Verification truth
This was a Discovery/data run through public web research plus GitHub connector writes. No executable checkout/browser asset-capture session was available, so `npm run check`, server smoke and Chrome E2E were not executed or claimed.

#### Next concrete priority
Acquire permitted ordered full-post screenshots for the strongest source-backed Korean candidates, starting with `축의금 내고 식권 안받으면`, `축의금 논란 , 제가 이상한가요?(특이케이스)`, and `돌잔치 축의금 이게 맞는거야?`; then feed real bytes through the existing intake/completeness/crop/normalization/carousel gates.

---

## 원본 기록: 150-sol.md

### 150-sol — source-first normalization validator hardening

Time: 2026-09-17 22:19 KST

Read current Threads handoff, current main/recent commits and latest sequential ops `149-sol.md`; repo tip treated as authoritative.

#### Material change
Hardened `scripts/validate-screenshot-normalization-plan.mjs` in Threads commit `afa179387b6d99b64a5006f1920baf3ab06e8c7c`.

The independent normalization gate now additionally rejects malformed geometry/evidence metadata: source/scaled dimensions and byte lengths must be positive safe integers; acquisition/provenance must be non-empty strings; padding must be non-negative safe integers; padding must close exactly to 1080x1080; contain padding must remain centered within one pixel. Existing sequence/SHA/source URL/full-body HUMAN_REVIEW or USER_CONFIRMED/no-stretch/no-body-crop/user-directed privacy/publicationAllowed=false/04_REVIEW_PUBLISH checks remain.

#### Discovery / asset truth
This implementation run: raw leads **0**, retained **0**. Most recent completed Discovery remains **40+ raw / 16 retained C1**. Full-post screenshots captured: **0**. Actual source bytes: **0**. Real source-backed carousel: **NO**. A1/P1: **0**. No OCR/moderation/rights/delivery/publication success claimed.

#### Verification truth
GitHub connector did not expose an executable checkout/browser session. `npm run check`, targeted runtime tests, server smoke and Chrome E2E were therefore not executed or claimed.

#### Next
Acquire permitted ordered full-post screenshots for the strongest Korean candidates and run the real bytes through intake → completeness → crop review/gate → normalization → hardened validator → strict carousel plan/validator. Only 04_REVIEW_PUBLISH may publish.

---

## 원본 기록: 151-sol.md

### 151-sol — Korean-community-first Discovery refresh

Time: 2026-09-17 22:37 KST

Read current Threads README/handoff, current main/recent commits and latest sequential ops `150-sol.md`; repo tip treated as authoritative.

#### Discovery result
Public/index/search exploration covered Korean-community-first lanes and Reddit without login/anti-bot bypass or restricted bulk crawling. Inspected **40+ raw leads** and retained **15 new C1_A0_P0 candidates** after relevance/dedupe filtering. Each retained candidate is a separate human-readable Markdown file under `data/candidates/`; no new grouped discovery JSON was placed in `data/` root.

Strongest new items include Blind `남편이 비자금을 숨겼는데 배신감드네 ㅠ` (21K / 18 / 187), `축의금 문화, 결혼 문화 10년내 다바뀔 듯` (85K / 433 / 543), `남편이 올려서 다른 사람들한테도 물어보라고해서 올려봐` (12K views / 86 comments), `절친이 축의금 10만원 함(내가 속좁은 거야?)` (13K / 95 comments), and Reddit `AITA for not returning money my ex-husband sent to me mistakenly?` (observed score +9,444).

Retained lanes also cover secret/leveraged stock debt, family investment fallout, workplace matchmaking pressure, in-law wedding-gift imbalance, wedding-MC compensation and destination-wedding family conflict. Exact URL and observed metrics are stored per candidate. Items not verified to the end explicitly state `본문 전체 미확인`.

#### Asset / publication truth
Full-post screenshots: **0**. Actual source bytes: **0**. Real source-backed carousel: **NO**. A1/P1: **0**. New candidates remain `ASSETS_PENDING / publicationAllowed=false`. No OCR/moderation/rights/delivery/publication success claimed. Only `04_REVIEW_PUBLISH` may publish.

#### Verification
Discovery/data-only run through public search/index evidence and GitHub writes. No executable checkout/browser capture session was available; no `npm run check`, server smoke or Chrome E2E success is claimed.

#### Next
Acquire permitted ordered full-post screenshots for strongest Korean candidates, then run real bytes through intake → completeness → crop review/gate → normalization → validator → strict carousel plan/validator and produce the first real source-backed 1080x1080 carousel.

---

## 원본 기록: 152-sol.md

### 152-sol — source-backed carousel builder hardening

Time: 2026-09-17 23:16 KST

Read current Threads handoff, current main/recent commits and latest sequential ops `151-sol.md`; repo tip treated as authoritative.

#### Material change
Hardened `scripts/build-source-backed-carousel-plan.mjs`. The builder now fails closed before accepting normalized body screenshots: validates SHA-256 format and uniqueness, captureUrl/sourceUrl equality, exact 1080x1080 target, integer source/scaled dimensions and padding, exact 1080 closure, centered contain geometry, and `privacyMasking=USER_DIRECTED_ONLY`. Existing `VERIFIED_COMPLETE`, source order, no-stretch/no-body-crop and `04_REVIEW_PUBLISH` gate remain.

This closes a gap where malformed geometry could reach a carousel plan if a caller skipped the independent normalization validator.

#### Discovery / asset truth
This implementation run: raw **0**, retained **0**. Most recent completed Discovery remains **40+ / 15**. Full-post screenshots **0**, actual source bytes **0**, real source-backed carousel **NO**, A1/P1 **0**. No OCR/moderation/rights/delivery/publication success claimed.

#### Verification truth
GitHub source edit was available, but no executable checkout/browser session was available, so `npm run check`, server smoke, runtime fixture tests and Chrome E2E were not executed and are not claimed.

#### Next
Acquire permitted full-post screenshots for strongest Korean candidates and run real bytes through the complete source-first chain, then render/inspect the first real 1080x1080 carousel in Chrome.

---

## 원본 기록: 153-sol.md

### 153-sol — Korean-community-first Discovery refresh

Time: 2026-09-17 23:37 KST

Read current Threads README/handoff, current main/recent commits and latest sequential ops `152-sol.md`; repo tip treated as authoritative.

#### Material work
Public search/index/page exploration inspected **40+ raw leads** across Korean-community-first lanes and Reddit fallback. No login/anti-bot/robots bypass was used. Retained **15 new C1_A0_P0** candidates, each as one human-readable Markdown file under `data/candidates/`; no grouped discovery JSON was added at `data/` root.

Top new items include Blind `결혼 1년정도 됐는데, 남편이 빚투로 2억5천 빚을 만들었어요...` (조회 6,083 / 댓글 53), `시댁` (15K / 좋아요 40 / 댓글 182), `남편이 빚을 숨기고 친정부모님 지원금으로 갚았어요.` (883 / 13), and Reddit `AITA for not giving my fiancé any of the winnings to pay off her debt?` (+5,564), `I just found my husband has 45K dollars in secret credit card debt.` (+4,798), `AITAH for calling off my wedding after finding out my fiancé never had the money he promised to contribute?` (+6,535).

Each retained record contains exact observed title, exact public URL, observation time, only visible same-observation metrics, read state, swipe rationale, and acquisition/publication state. Public third-party bodies were summarized rather than copied wholesale.

#### Asset / publication truth
Full-post screenshots **0**, actual source bytes **0**, real source-backed carousel **NO**, A1/P1 **0**. All new candidates remain `ASSETS_PENDING / publicationAllowed=false`; rights/privacy/human-review gates remain and only `04_REVIEW_PUBLISH` may publish. No OCR/moderation/rights/delivery/publication success claimed.

#### Verification truth
Markdown discovery changes were committed through GitHub-connected source operations. No executable checkout/browser was available, so npm/server/Chrome tests were not run or claimed for this discovery-only run.

#### Next
Acquire permitted ordered full-post screenshots for the strongest Korean candidates, then run real bytes through the complete source-first chain and render/inspect the first real 1080x1080 carousel.

---

## 원본 기록: 154-sol.md

### 154-sol — source-backed carousel final-gate hardening

Time: 2026-09-18 00:16 KST

Read current Threads handoff, current main/recent commits and latest sequential ops `153-sol.md`; repo tip treated as authoritative.

#### Material work
Hardened `scripts/validate-source-backed-carousel-plan.mjs`. The final carousel validator now independently rejects malformed or duplicated source screenshots even if an upstream normalization validator is skipped: SHA-256 format/uniqueness, exact source URL consistency, 1080x1080 target, positive integer source/scaled geometry, non-negative integer padding, exact 1080 closure, centered contain geometry, per-slide `USER_DIRECTED_ONLY` privacy policy and the binding full-original-post body policy are checked fail-closed.

Existing invariants remain: slide 1 COVER_ONLY; slide 2+ ordered ORIGINAL_POST_SCREENSHOT only; no stretch; no body crop; no summary/paraphrase/explanation/reaction/CTA/rewritten-story body cards; `publicationAllowed=false`; only `04_REVIEW_PUBLISH` may publish.

#### Discovery / asset truth
This implementation run: raw leads **0**, retained **0**. Most recent Discovery run: **40+ raw / 15 retained**. Full-post screenshots captured **0**, source bytes **0**, real source-backed carousel **NO**, A1/P1 **0**. No OCR/moderation/rights/delivery/publication success claimed.

#### Verification truth
Change was made through GitHub-connected source operations and statically checked against the current builder contract. No executable checkout/browser was available, so targeted runtime tests, `npm run check`, server smoke and Chrome E2E were not executed or claimed.

#### Next
Acquire permitted full-post screenshot sequences for the strongest Korean candidates and run real bytes through the complete source-first chain; then render and visually inspect the first genuine 1080x1080 cover + original-post-screenshot carousel.

---

## 원본 기록: 155-sol.md

### 155-sol — Korean-community-first Discovery refresh

Time: 2026-09-18 00:38 KST

Read current Threads README/handoff, current main/recent commits and latest sequential ops `154-sol.md`; repo tip treated as authoritative.

#### Material work
Public/search/index exploration across Korean-community-first lanes and Reddit fallback inspected **40+ raw leads** without login/anti-bot bypass or bulk crawling. Retained **15** new one-candidate-per-Markdown files in `data/candidates/`: 14 C1 exact-public-URL candidates and one C0 index-only lead. An index-only Blind lead was initially written with a C1 filename, then immediately corrected to C0 and the incorrect file deleted. One separate attempted candidate write was tool-blocked and is not counted.

Strongest retained items include Blind `이거 어떻게 복수해줄까요?ㅠㅠ` (2,493 views / 54 comments), `결혼 주선자 사례 X, 청첩장 못 받음. 축의금 해야돼?` (4,072 / 41), `JW메리어트 호텔 결혼 축의금 얼마나?` (3,631 / 1 like / 22), `임신 막달 남편 격일 회식 이해가 가는지` (229 / 12), `축의금 때문에 친구한테 섭섭한데..` (422 / 13), and Reddit `AITA for telling my best friend that I’m not paying for everyone in her wedding after loaning her money?` (+1,924 votes). `결혼 10년 차, 영원히 사위는 남인가 봅니다` is C0 because only a public index/list URL was confirmed.

Candidate files record only metrics visible at the same observation time, whether body/comments were actually read, exact acquisition state, and `publicationAllowed=false`. No invented URLs, metrics, OCR/moderation, rights, capture, delivery or publication claims.

#### Asset truth
Full-post screenshots captured **0**. Source bytes **0**. Real source-backed carousel **NO**. A1/P1 **0**. All retained candidates remain `ASSETS_PENDING`; rights/privacy/human-review gates remain and only `04_REVIEW_PUBLISH` may publish.

#### Verification truth
GitHub writes completed for candidate files and handoff. No executable checkout/browser session was available, so npm/server/browser tests were not applicable to this content-only Discovery run and are not claimed.

#### Next
Acquire permitted full-post screenshot sequences for the strongest Korean candidates, then run real bytes through the source-first intake/crop/normalization/carousel chain and visually verify the first genuine 1080x1080 output.

---

## 원본 기록: 156-sol.md

### 156-sol — Source screenshot provenance URL hardening

Time: 2026-09-18 01:19 KST

Read current Threads handoff, current main/recent commits and latest sequential ops `155-sol.md`; repo tip treated as authoritative.

#### Material work
Hardened `scripts/build-screenshot-intake-manifest.mjs` so source screenshot intake rejects private/local IPv6 source URLs in addition to the existing IPv4/local/credential/secret-bearing checks. Fail-closed coverage now includes IPv6 unspecified/loopback, unique-local `fc00::/7`, link-local `fe80::/10`, and IPv4-mapped private/loopback/link-local forms.

This closes a provenance gap where a local/private endpoint could otherwise be recorded as an exact public source URL. It does not infer source relationship, OCR, vision, rights, moderation, privacy state or publication success.

Role chain remains `01 DISCOVERY → 02 EDITORIAL_SCORING → 03 PRODUCTION → 04 REVIEW_PUBLISH → 05 EXPERIMENTS_ACCOUNTS`. Intake still emits `publicationAllowed=false`, `publishOwner=04_REVIEW_PUBLISH`, `privacyMasking=USER_DIRECTED_ONLY`, and requires human verification of full-body completeness.

Threads implementation commit: `44246027e915998b88cb9f870d2bb6c64104e960`. Handoff was updated immediately afterward.

#### Discovery / asset truth
This was an implementation run: raw Discovery **0**, retained **0**. Most recent completed Discovery remains **40+ raw / 15 retained**.

Full-post screenshots captured **0**. Source bytes **0**. Real source-backed carousel **NO**. A1/P1 **0**. Existing candidates remain `ASSETS_PENDING`; rights/privacy/human-review gates remain and only `04_REVIEW_PUBLISH` may publish.

#### Verification truth
GitHub writes completed. No executable checkout/browser session was available, so targeted runtime fixtures, `npm run check`, server smoke and Chrome E2E were not run or claimed. The changed script is already included in the standard `npm run syntax` gate.

#### Next
Acquire permitted full-post screenshot sequences for the strongest Korean candidates and run real bytes through intake → completeness verification → crop review/gate → normalization → validation → strict source-backed carousel build/validation, then inspect the first genuine 1080x1080 result in Chrome.

---

## 원본 기록: 157-sol.md

### 157-sol — Korean-community-first Discovery refresh

Time: 2026-09-18 01:36 KST

Read current Threads README/handoff, current main/recent commits and latest sequential ops `156-sol.md`; repo tip treated as authoritative.

#### Material work
Ran a fresh Discovery pass using permitted public search/index/pages only. No login/anti-bot bypass and no restricted-source bulk crawling. Inspected **40+ raw leads** across Korean-community-first lanes with Reddit fallback and retained **15 new C1 candidates** as one human-readable Markdown file per candidate under `data/candidates/`.

Strongest retained items include Blind `남편 외도`, `제 남편 이 여자후배 좋아하는것 맞죠?`, `남편 회식 징글징글하다`, `업체에서 운영하는 하객알바 축의금 댓글 부탁드려요`, `축의금 계좌이체`, `부모쪽 축의금 질문`, `결혼식 비용 축의금`, plus Reddit jackpot-versus-fiancée-debt, crypto-loan/husband-debt, and unpaid wedding-event-balance stories.

Each file records exact public source URL, exact observed title, 2026-09-18 01:36 KST observation time, only metrics visible in that observation, body/comment read state, story value, and exact asset/publication state. No metrics from different observation times were merged.

Role chain remains `01 DISCOVERY → 02 EDITORIAL_SCORING → 03 PRODUCTION → 04 REVIEW_PUBLISH → 05 EXPERIMENTS_ACCOUNTS`. New candidates remain `publicationAllowed=false`; only 04 may publish.

#### Discovery / asset truth
Raw Discovery: **40+**. Retained: **15**.

Full-post screenshots captured: **0**. Source bytes: **0**. Real source-backed carousel: **NO**. A1/P1: **0**. All retained candidates are `C1_A0_P0 / ASSETS_PENDING`; rights/privacy/human-review gates remain.

No OCR/moderation/rights verification, source screenshot acquisition, delivery or publication success was performed or inferred.

#### Verification truth
GitHub candidate and handoff writes completed. This was a Discovery/data run; no production-code path changed and no executable checkout/browser was used, so `npm run check`, server smoke and Chrome E2E were not run or claimed.

#### Next
Rank/dedupe newest Korean candidates, acquire permitted ordered full-post screenshot sequences for the strongest source-accessible candidates, then run real bytes through the existing source-first intake/completeness/crop/normalization/carousel pipeline. Keep A0/P0 until actual source-backed user-facing assets / 04-confirmed publication exist.

---

## 원본 기록: 158-sol.md

### 158-sol — Harden screenshot completeness verification gate

Time: 2026-09-18 02:17 KST

Started from current Threads handoff, current main/recent commits and latest sequential ops `157-sol.md`; repo tip treated as authoritative.

#### Material work
Hardened `scripts/verify-screenshot-intake-complete.mjs`. Before a screenshot intake can be promoted to `VERIFIED_COMPLETE`, it now fail-closes on malformed/tampered metadata: 04-only publication ownership, `publicationAllowed=false`, `USER_DIRECTED_ONLY` privacy policy, valid acquisition state, credential-free http(s) source URL, contiguous source sequence, valid unique SHA-256, positive byte/dimension metadata, acquisition/source URL consistency, provenance presence, and false OCR/vision verification claims.

This preserves the binding source-first rule: human/user completeness confirmation cannot turn structurally invalid or provenance-inconsistent intake metadata into an accepted full-post source package.

Role chain remains `01 DISCOVERY → 02 EDITORIAL_SCORING → 03 PRODUCTION → 04 REVIEW_PUBLISH → 05 EXPERIMENTS_ACCOUNTS`; only 04 may publish.

#### Discovery / asset truth
Discovery this implementation run: **raw 0 / retained 0**. Most recent useful Discovery: **40+ raw / 15 retained**.

Full-post screenshots captured: **0**. Source bytes acquired: **0**. Real source-backed carousel: **NO**. A1/P1: **0**. No OCR/moderation/rights verification, asset capture, delivery or publication success was performed or inferred.

#### Verification truth
Repository code and handoff writes completed through GitHub. No executable checkout/browser was available, so runtime targeted tests, `npm run check`, server smoke and Chrome E2E were not run or claimed. Changed path is a CLI validation gate rather than a user-visible renderer.

#### Next
Acquire permitted ordered full-post screenshots for the strongest source-accessible Korean candidates and run real bytes through intake → completeness verification → reviewed UI-chrome crop → normalization → validators → strict source-backed carousel. Keep A0/P0 until real user-facing source assets / 04-confirmed publication exist.

---

## 원본 기록: 159-sol.md

### 159-sol — Discovery refresh

Time: 2026-09-18 02:39 KST

Started from current Threads README/handoff, current main/recent commits and latest sequential ops `158-sol.md`; repo tip treated as authoritative.

#### Material work
Ran Korean-community-first public discovery using permitted search/index/page access only, with no bulk crawling or access-control bypass. Inspected **40+ raw leads** and retained **15 C1 candidates**, each as its own human-readable Markdown file under `data/candidates/` using `260918_C1_A0_P0_*.md` naming.

Retained set spans marriage/family opposition, wedding-gift money norms, friend wedding labor compensation, workplace spending pressure, lottery secrecy and workplace/wedding conflict. Strong examples include Blind `부모님 결혼 반대(나는 남자)` (16K / 138 comments), 4-year-older-girlfriend `부모님 결혼 반대` (19K / 147), `남친 부모님 결혼 반대` (2,658 / 33), `축의금 물가가 많이 올랐다.` (160 / 16), and Reddit lottery-secret engagement (+13,120), secret pre-marriage lottery (+9,212), rich-coworker spending pressure (+4,227), coworker wedding-band conflict (+11,956).

Metrics are recorded only where actually visible at the same observation time. Full body/comments-read state and acquisition state are stated per file. No grouped discovery JSON was created in `data/` root.

#### Asset / publication truth
Full-post screenshots captured: **0**. Source bytes acquired: **0**. Real source-backed carousel: **NO**. New candidates are A0/P0, `ASSETS_PENDING`, `publicationAllowed=false`. No OCR/moderation/rights verification, delivery or publication success was performed or inferred. Rights/privacy/human review remains required and only 04_REVIEW_PUBLISH may publish.

#### Verification truth
15 candidate Markdown files and Threads handoff were written through GitHub contents API. No executable checkout/browser was available for `npm run check`, server smoke or Chrome E2E; none are claimed. This was a Discovery-data run, not a renderer-code change.

#### Next
Prioritize permitted full-post screenshot acquisition for the strongest source-accessible Korean candidates, then feed real bytes through the existing source-first intake/crop/normalize/carousel pipeline. Continue high-volume Korean-community Discovery with dedupe on subsequent runs.

---

## 원본 기록: 160-sol.md

### 160-sol — Source screenshot intake provenance hardening

Time: 2026-09-18 03:15 KST

Started from current Threads handoff, current main/recent commits and latest sequential ops `159-sol.md`; repo tip treated as authoritative.

#### Material work
Hardened `scripts/build-screenshot-intake-manifest.mjs` so an exact-public source URL cannot use non-public IPv4 literals as provenance. Intake now rejects unspecified/loopback/private/link-local ranges plus carrier-grade NAT `100.64.0.0/10`, protocol/documentation/benchmark ranges, multicast and reserved IPv4 space. Existing IPv6 local/private checks, embedded-credential rejection and secret-bearing query-key rejection remain.

This is fail-closed provenance work for the real screenshot acquisition path; it does not create substitute body cards, infer screenshot/source relationships, run OCR, mask privacy automatically, or weaken full-body human verification.

#### Discovery / asset truth
This implementation run: **raw 0 / retained 0**. Most recent useful Discovery remains **40+ raw / 15 retained**. Top candidates remain the high-comment Blind marriage/family conflicts plus the strongest Reddit lottery/workplace conflict fallbacks recorded in handoff.

Full-post screenshots captured: **0**. Source bytes acquired: **0**. Real source-backed carousel: **NO**. A1/P1: **0**. `publicationAllowed=false`; only `04_REVIEW_PUBLISH` may publish.

#### Verification truth
Threads implementation commit: `b7c0e39a59c85994a605304aa6e8f5971d4eb205`. Handoff updated immediately after. No executable checkout/browser was available, so targeted runtime tests, `npm run check`, server smoke and Chrome E2E were not run or claimed. No temporary artifacts were created.

#### Next
Acquire permitted ordered full-post screenshots for a strong source-accessible Korean candidate and run real bytes through intake → human completeness verification → crop review/gate → 1080x1080 contain normalization → validators → strict cover + screenshot carousel. Continue high-volume Korean-community Discovery in parallel on useful Discovery runs.

---

## 원본 기록: 161-sol.md

### 161-sol — Discovery refresh

Time: 2026-09-18 03:36 KST

Started from current Threads README/handoff, current main/recent commits and sequential ops; repo tip treated as authoritative. `160-sol.md` appeared after the initial latest-note check, so this note follows it rather than overwriting it.

#### Material work
Ran Korean-community-first public Discovery using permitted search/index/page access only; no bulk crawling, login or anti-bot bypass. Inspected **40+ raw leads** and retained **15 new C1 candidates**, each as one human-readable `260918_C1_A0_P0_*.md` file under `data/candidates/`. No grouped discovery JSON was created in `data/` root.

Korean highlights: Blind `[결혼 고민] 결혼 전부터 경제권 요구하는 여친... 내가 잘못함?` (97 / 14 comments), `맨날 사고치는 남편` (7,226 / 65; 3억원+ investment-loss household conflict), `갓난 아기 육아분담 어떡하고 있나요?` (5,115 / 121), `결혼 예정 부부의 명절 문제` (8,375 / 7 likes / 161), `결혼 비용및 축의금 정산 의견 차이` (1,966 / 21), and `실수령 외벌이 500 저축 얼마해야하나요? 이사가야할까요?` (1,114 / 38). Also retained housing-vs-ETF, wedding-gift norms and post-childbirth income anxiety.

Overseas support: Reddit wedding-money-to-house elopement (+17,821), parents-demand-large-wedding (+12,084), pre-marriage debt disclosure (+3,046), destination-wedding parent exclusion (+547), and wedding gift vs unpaid debt (+17). Only actually visible same-observation metrics were recorded; unavailable metrics remain unclaimed.

#### Asset / publication truth
Full-post screenshots captured: **0**. Source bytes acquired: **0**. Real source-backed carousel: **NO**. New candidates are A0/P0, `ASSETS_PENDING`, `publicationAllowed=false`. No OCR/moderation/rights verification, delivery or publication success was performed or inferred. Rights/privacy/human review remains required and only 04_REVIEW_PUBLISH may publish.

#### Verification truth
15 candidate Markdown files and Threads handoff were written through GitHub contents API. This was Discovery-data work, not renderer/UI code. No executable checkout/browser was available for `npm run check`, server smoke or Chrome E2E; none are claimed. No temporary artifacts were created.

#### Next
Prioritize permitted full-post screenshot acquisition for the strongest source-accessible Korean candidates, especially the 명절, 투자손실/육아, and 육아분담 stories. Continue high-volume Korean-community discovery with dedupe, then run real bytes through source-first intake/crop/normalize/carousel verification.

---

## 원본 기록: 162-sol.md

### 162-sol — Deterministic ordered screenshot intake

Time: 2026-09-18 04:19 KST

Started from current Threads handoff, current main/recent commits and sequential ops. `161-sol.md` was already present after the initial latest-note read, so this note follows it.

#### Material work
Updated `scripts/build-screenshot-intake-manifest.mjs` with explicit screenshot ordering modes. Default `--order-mode cli` preserves caller-supplied order. New `--order-mode filename` natural-sorts screenshot basenames before assigning `sourceSequence`, preventing lexical mistakes such as `1, 10, 2` when captures are named numerically. The selected mode is recorded in the manifest provenance.

No full-body completeness, source relationship, OCR/vision, rights, privacy, moderation or publication result is inferred. Privacy remains `USER_DIRECTED_ONLY`; `publicationAllowed=false`; only `04_REVIEW_PUBLISH` may publish.

#### Discovery / asset truth
This implementation run: **raw 0 / retained 0**. Most recent useful Discovery remains **40+ raw / 15 retained**. Full-post screenshots captured: **0**. Source bytes acquired: **0**. Real source-backed carousel: **NO**. A1/P1: **0**.

#### Verification truth
Threads implementation commit: `0fe53ca5f62c9df00f1239d80038501ee7d38a3f`. Handoff updated immediately after. No executable checkout/browser was available, so targeted runtime tests, `npm run check`, server smoke and Chrome E2E were not run or claimed. No temporary artifacts were created.

#### Next
Acquire permitted ordered full-post screenshots for a strong source-accessible Korean candidate and run real bytes through deterministic intake → human completeness verification → UI-chrome crop review/gate → 1080x1080 contain normalization → validators → strict cover + screenshot carousel. Continue high-volume Korean-community Discovery in parallel on useful Discovery runs.

---

## 원본 기록: 163-sol.md

### 163-sol — Discovery refresh

Time: 2026-09-18 04:38 KST

Started from current Threads README/handoff, current main/recent commits and latest sequential ops `162-sol.md`; repo tip treated as authoritative.

#### Material work
Ran Korean-community-first public Discovery using permitted search/index/page access only; no bulk crawling, login or anti-bot bypass. Inspected **40+ raw leads** and retained **15 new C1 candidates**, each as one human-readable `260918_C1_A0_P0_*.md` file under `data/candidates/`. No grouped discovery JSON was created in `data/` root.

Korean highlights: Blind `주식중독 남편.. 대출 막는법 있을까?` (930 / 좋아요 3 / 댓글 19), `넋두리… 수의사 남편 주식 개인투자자로 전향 어떻게 생각해?` (6,015 / 댓글 165), `남편 동생이 400빌리고 또 400을 빌려갔는데..` (1,082 / 댓글 22), `돈 없는 시댁` (3,414 / 댓글 44), `남편 카드값` (844 / 댓글 31). Also retained `울 남편 50세 생일에 차 바꿔주려고 몰래 모으는 중` (2,215 / 좋아요 25 / 댓글 13) to keep a positive/funny-money lane in the mix, plus marriage-background conflicts.

Overseas support: Reddit ex-husband mistaken transfer/debt deduction (+9,444), wedding money used for house downpayment (+17,821), Italy destination wedding affordability (+1,341), wedding-family contribution dispute (+34), and $6k destination-wedding debt (+67). Only actually visible same-observation metrics were recorded.

#### Asset / publication truth
Full-post screenshots captured: **0**. Source bytes acquired: **0**. Real source-backed carousel: **NO**. New candidates are A0/P0, `ASSETS_PENDING`, `publicationAllowed=false`. No OCR/moderation/rights verification, delivery or publication success was performed or inferred. Rights/privacy/human review remains required and only 04_REVIEW_PUBLISH may publish.

#### Verification truth
15 candidate Markdown files and Threads handoff were written through GitHub contents API. This was Discovery-data work, not renderer/UI code. No executable checkout/browser was available for `npm run check`, server smoke or Chrome E2E; none are claimed. No temporary artifacts were created.

#### Next
Prioritize permitted full-post screenshot acquisition for `주식중독 남편.. 대출 막는법 있을까?` and the veterinarian/hidden-stock-debt story. Continue high-volume Korean-community discovery with dedupe, then run real bytes through deterministic intake → human completeness verification → UI-chrome crop review → normalize → strict carousel verification.

---

## 원본 기록: 164-sol.md

### 164-sol — Discovery refresh

Time: 2026-09-18 05:35 KST

Started from current Threads README/handoff, current main/recent commits and latest sequential ops `163-sol.md`; repo tip treated as authoritative.

#### Material work
Ran Korean-community-first public Discovery using permitted search/index/page access only; no bulk crawling, login or anti-bot bypass. Inspected **40+ raw leads** across Blind/Korean-community and overseas support lanes and retained **15 new C1 candidates**, each as one human-readable `260918_C1_A0_P0_*.md` file under `data/candidates/`. No grouped discovery JSON was created in `data/` root.

Strongest new Korean candidates: Blind `나몰래 대출받은 남편` (12K / 댓글 257), `결혼 전 고민..(시댁 관련)` (9,902 / 댓글 99), `남편 회식(술자리) 늦게 오면 불안하고 화가 나` (1,586 / 좋아요 1 / 댓글 29), `동생 결혼 반대` (3,414 / 좋아요 2 / 댓글 14), plus first-holiday in-law scheduling, fortune-based marriage opposition, frequent company drinking, sibling wedding-gift amount and startup-founder marriage opposition.

Overseas support includes shared wedding fund spending (+2,759), wedding 70/30 contribution dispute (+385), Italy wedding vs savings (+179), repeated hidden $25k husband debt (+11), sister's $14k debt vs wedding/house savings (+357), and ex-fiancée family wedding-cost dispute (+4,551). Only same-observation visible metrics were recorded; missing metrics were explicitly left unconfirmed.

#### Asset / publication truth
Full-post screenshots captured: **0**. Source bytes acquired: **0**. Real source-backed carousel: **NO**. New candidates are C1_A0_P0, `ASSETS_PENDING`, `publicationAllowed=false`. No OCR/moderation/rights verification, delivery or publication success was performed or inferred. Rights/privacy/human review remains required and only 04_REVIEW_PUBLISH may publish.

#### Verification truth
15 candidate Markdown files and Threads handoff were written through GitHub contents API. This was Discovery-data work, not renderer/UI code. No executable checkout/browser was used for `npm run check`, server smoke or Chrome E2E; none are claimed. No temporary artifacts were created.

#### Next
Prioritize permitted ordered full-post screenshot acquisition for `나몰래 대출받은 남편`, then `결혼 전 고민..(시댁 관련)`. Continue high-volume Korean-community discovery with dedupe, then feed real bytes through deterministic intake → human completeness verification → UI-chrome crop review → normalize → strict carousel verification.

---

## 원본 기록: 165-sol.md

### 165-sol — Discovery refresh

Time: 2026-09-18 06:34 KST

Started from current Threads README/handoff, current main/recent commits and latest sequential ops `164-sol.md`; repo tip treated as authoritative.

#### Material work
Ran Korean-community-first public Discovery using permitted search/index/page access only; no bulk crawling, login or anti-bot bypass. Inspected **40+ raw leads** across Blind/DCInside/FMKorea/TheQoo/Instiz/Ruliweb/Ppomppu/Clien/Inven/Arca search lanes plus Reddit support lanes. Search access was uneven (including robots restrictions), so only evidence actually returned was retained.

Retained **15 new candidates: 13 C1 + 2 C0**, each as one human-readable `260918_C*_A0_P0_*.md` under `data/candidates/`. The two Blind company-index leads remain C0 because exact individual URLs were not verified. No grouped discovery JSON was created in `data/` root.

Strongest retained: Reddit `$200,000 consumer debt` caused partly by travel and stock-market betting (+44), sister wedding expenses pushed onto sibling credit card (+7,972), hidden $60k credit-card debt + $15k 401k loan (+385), seven-year $49k hidden debt discovered during mortgage preapproval (+72), repeated secret-finance debt, and Blind index `축의금 문제인데 제가 잘못했나요?` (1,820 / 좋아요 1 / 댓글 38). Metrics are only same-observation values actually visible; absent metrics remain unconfirmed.

#### Asset / publication truth
Full-post screenshots captured: **0**. Source bytes acquired: **0**. Real source-backed carousel: **NO**. New candidates are A0_P0, `ASSETS_PENDING`, `publicationAllowed=false`. No OCR/moderation/rights verification, delivery or publication success was performed or inferred. Rights/privacy/human review remains required and only 04_REVIEW_PUBLISH may publish.

#### Verification truth
15 candidate Markdown files and Threads handoff were written through GitHub contents API. This was Discovery-data work, not renderer/UI code. No executable checkout/browser was used for `npm run check`, server smoke or Chrome E2E; none are claimed. No temporary artifacts were created.

#### Next
Prioritize permitted ordered full-post screenshot acquisition for existing strong Korean candidates `나몰래 대출받은 남편` and `결혼 전 고민..(시댁 관련)`. Continue Korean-community discovery with dedupe and exact individual URL upgrades for C0 leads. Feed any real bytes through deterministic intake → human completeness verification → UI-chrome crop review → normalize → strict carousel verification.

---

## 원본 기록: 166-sol.md

### 166-sol — Source screenshot acquisition queue

Time: 2026-09-18 07:18 KST

Started from current Threads handoff, current main/recent commits and latest sequential ops `165-sol.md`; repo tip treated as authoritative.

#### Material work
Added `scripts/build-acquisition-queue.mjs` to convert the human-readable one-candidate-per-file store into a deterministic screenshot-acquisition work queue under `data/_system/acquisition-queue.md`. Added `npm run acquisition:queue` and included the script in the syntax-check chain.

The queue is deliberately fail-closed operational metadata: it prioritizes exact-public C1/A0/P0 records, reports whether full body/comments were previously confirmed read, keeps C0 as exact-provenance pending, and explicitly requires ordered screenshots covering the FULL ORIGINAL POST BODY. It does not grant rights, infer OCR/moderation, fabricate screenshots, change A/P state, automatically mask privacy, or permit publication. Only 04_REVIEW_PUBLISH may publish.

#### Discovery / asset truth
Discovery this implementation run: **raw 0 / retained 0**. Latest useful Discovery remains **40+ raw / 15 retained (13 C1 + 2 C0)**. Top queue targets remain strong Korean candidates already identified, including `나몰래 대출받은 남편`, `결혼 전 고민..(시댁 관련)`, and `주식중독 남편.. 대출 막는법 있을까?` once exact permitted acquisition is available.

Full-post screenshots captured: **0**. Source bytes acquired: **0**. Real source-backed carousel: **NO**. A1/P1: **0**. `publicationAllowed=false` and rights/privacy/human-review gates remain binding.

#### Verification truth
Changes were committed through GitHub. No executable checkout/browser was available in this run, so runtime execution, `npm run check`, server smoke and Chrome E2E are not claimed. No temporary artifacts were created.

#### Next
Run the acquisition queue in an executable checkout, acquire permitted ordered full-post screenshots for the strongest Korean C1 candidates, then feed real bytes through deterministic intake → human completeness verification → UI-chrome crop review → reviewed-crop validation → 1080x1080 contain normalization → strict source-backed carousel validation and Chrome inspection.

---

## 원본 기록: 167-sol.md

### 167-sol — Discovery refresh

Time: 2026-09-18 07:39 KST

Started from current Threads README/handoff, current main/recent commits and latest sequential ops `166-sol.md`; repo tip treated as authoritative.

#### Material work
Ran Korean-community-first public Discovery across Blind/DCInside/FMKorea/TheQoo/Instiz/Ruliweb/Ppomppu/Clien/Inven/Arca search lanes plus Reddit support lanes. No bulk crawling, login or anti-bot bypass. Some Korean domains/search lanes returned no usable evidence or robots restrictions, so no unverifiable Korean leads were invented.

Inspected **40+ raw leads** and retained **10 new C1 candidates** after dedupe, safety, evidence and story-potential filtering. Kept the set below the 15 target rather than pad it with weak/unverified material. Each candidate is one human-readable `260918_C1_A0_P0_*.md` under `data/candidates/`; no grouped discovery JSON was created at `data/` root.

Strongest retained: canceled wedding after daughter cheating with non-refundable venue (+9,412), $20K wedding venue colliding with sister's ex-husband/mistress proposal (+13,261), husband revealing nearly $20K hidden debt (+96), bridesmaid whose $500 budget was exhausted before the shower (81 points / 94% upvoted), and destination-wedding family travel/parent invitation conflict (+547). Metrics are only same-observation values actually visible; absent metrics remain unconfirmed. Candidate files distinguish partial-body reads from fuller reads and do not invent unseen text.

#### Asset / publication truth
Full-post screenshots captured: **0**. Source bytes acquired: **0**. Real source-backed carousel: **NO**. New candidates are A0_P0, `ASSETS_PENDING`, `publicationAllowed=false`. No OCR/moderation/rights verification, delivery or publication success was performed or inferred. Rights/privacy/human review remains required and only 04_REVIEW_PUBLISH may publish.

#### Verification truth
10 candidate Markdown files and Threads handoff were written through GitHub contents API. This was Discovery-data work, not renderer/UI code. No executable checkout/browser was used for `npm run check`, server smoke or Chrome E2E; none are claimed. No temporary artifacts were created.

#### Next
Keep Korean source acquisition ahead of overseas support: acquire permitted ordered full-post screenshots for existing strong Korean C1 candidates `나몰래 대출받은 남편`, `결혼 전 고민..(시댁 관련)`, `주식중독 남편.. 대출 막는법 있을까?`; upgrade C0 only on exact individual URL verification; then run deterministic intake → human completeness verification → UI-chrome crop review → normalize → strict source-backed carousel verification.

---

## 원본 기록: 168-sol.md

### 168-sol — Korean-first acquisition queue hardening

Time: 2026-09-18 08:16 KST

Started from current Threads handoff, current main/recent commits and latest sequential ops `167-sol.md`; repo tip treated as authoritative.

#### Material work
Hardened `scripts/build-acquisition-queue.mjs` at Threads commit `156707aaeb803ca8e6888673fef727bb1ed34386`.

The acquisition queue now ranks Korean-community sources ahead of overseas support candidates, matching the binding Korean-first acquisition priority. It reads the capture URL only from each candidate's `## 정확한 링크` section instead of taking the first arbitrary URL in the file. A record whose filename claims C1 but lacks an exact-link URL is now fail-closed into `Blocked inconsistent records` and is not sent into screenshot capture. The queue also exposes the candidate's recorded acquisition state to the capture operator.

This change does not grant rights, infer OCR/moderation, fabricate screenshots, alter A/P state, auto-mask privacy, or publish. Role chain remains `01 → 02 → 03 → 04 → 05`; only 04_REVIEW_PUBLISH may publish.

#### Discovery / asset truth
Discovery this implementation run: **raw 0 / retained 0**. Latest useful Discovery remains **40+ raw / 10 retained C1**. Full-post screenshots captured: **0**. Source bytes acquired: **0**. Real source-backed carousel: **NO**. A1/P1: **0**. `publicationAllowed=false` and rights/privacy/human-review gates remain binding.

#### Verification truth
Code and handoff were committed through GitHub. No executable checkout/browser was available, so runtime execution, `npm run check`, server smoke and Chrome E2E are not claimed. No temporary artifacts were created.

#### Next
Run the stricter queue in an executable checkout; resolve any provenance-inconsistent C1 records; then acquire permitted ordered full-post screenshots for strongest Korean C1 targets first and feed real bytes through deterministic intake → human completeness verification → UI-chrome crop review → reviewed-crop validation → 1080x1080 contain normalization → strict source-backed carousel validation and Chrome inspection.

---

## 원본 기록: 169-sol.md

### 169-sol — Discovery refresh

Time: 2026-09-18 08:37 KST

Started from current Threads README/handoff, current main/recent commits and latest sequential ops `168-sol.md`; repo tip treated as authoritative.

#### Material work
Ran Korean-community-first public Discovery across Blind/DCInside/FMKorea/TheQoo/Instiz/Ruliweb/Ppomppu/Clien/Inven/Arca lanes plus Reddit support. No bulk crawling, login or anti-bot bypass. Clien/Instiz search access was robots-blocked and was not circumvented.

Inspected **40+ raw leads** and retained **10 new C1 candidates** after dedupe, safety, evidence and story-potential filtering. Each retained candidate is one Markdown file under `data/candidates/`; no grouped discovery JSON was created at `data/` root.

Strong Korean additions: TheQoo `결혼 승낙 받자마자 탈모인거 밝힌 남편..` (104,013 views / 391 comments), Blind `이모때문에 스트레스 미치게 받아. 다른 집들 친척 결혼식 관련 경험좀..`, TheQoo `남편 전화 못받는 아내` (7,254 / 20), Ruliweb `결혼식 비용이 부족했던 남성` (18,161 / recommendation 79), Ruliweb `결혼식 식대가 얼만데 축 의금 얼마를 내냐 소리 진짜 웃긴다.` (16,095 / recommendation 95). Image-centric pages whose actual image body was not read are explicitly marked `본문 미확인` rather than reconstructed.

Support lanes retained brother's $800 wedding gift after being uninvited (+2,566), last-minute Europe wedding gift reversal (+4,018), husband hidden $70K+ credit-card debt (+23), workplace cash-card pressure for coworkers' children (+104), and SAHM wife's hidden $17K debt update (+2,037).

#### Asset / publication truth
Full-post screenshots captured: **0**. Source bytes acquired: **0**. Real source-backed carousel: **NO**. New candidates are A0_P0, `ASSETS_PENDING`, `publicationAllowed=false`. No OCR/moderation/rights verification, delivery or publication success was performed or inferred. Rights/privacy/human review remains required and only 04_REVIEW_PUBLISH may publish.

#### Verification truth
10 candidate Markdown files and Threads handoff were written through GitHub contents API. One additional attempted candidate write was blocked by tooling and is not counted. This was Discovery-data work, not renderer/UI code. No executable checkout/browser was used for `npm run check`, server smoke or Chrome E2E; none are claimed. No temporary artifacts were created.

#### Next
Acquire permitted ordered full-post screenshots for strongest Korean C1 candidates, especially existing hidden-debt/investment stories and the new high-response TheQoo image sequence. Do not reconstruct image-body records marked `본문 미확인`; obtain real ordered source assets first. Then deterministic intake → human completeness verification → UI-chrome crop review → normalize → strict source-backed carousel verification.

---

## 원본 기록: 170-sol.md

### 170-sol — C0 deterministic-capture fail-closed gate

Time: 2026-09-18 09:16 KST

Started from current Threads handoff and current main/recent commits, then confirmed latest sequential ops `169-sol.md`; repo tip treated as authoritative.

#### Material work
Updated `scripts/build-acquisition-queue.mjs` at Threads commit `d76155d34f28800046bf459ca1131906c24432a2`.

C0/index-only candidates can no longer enter deterministic screenshot capture order at all, even if their Markdown happens to contain a URL-like string. They are emitted separately under `Provenance pending — not capture-ready` and must first obtain an exact individual public source and be promoted to C1. Existing C1-without-exact-link inconsistency remains separately fail-closed. This prevents index/list provenance from being accidentally treated as source-backed post capture.

Korean-community exact-source priority, full-original-body screenshot requirement, user-directed privacy handling, `publicationAllowed=false`, and the role chain `01 → 02 → 03 → 04 → 05` remain unchanged. Only 04_REVIEW_PUBLISH may publish. No rights/OCR/moderation/assets/publication state is inferred by the queue.

#### Discovery / asset truth
Discovery this implementation run: **raw 0 / retained 0**. Latest useful Discovery remains **40+ raw / 10 retained C1** from 08:37 KST. Full-post screenshots captured: **0**. Source bytes acquired: **0**. Real source-backed carousel: **NO**. A1/P1: **0**.

#### Verification truth
Code and handoff were committed through GitHub contents API. No executable checkout/browser was available, so runtime execution, `npm run check`, server smoke and Chrome E2E are not claimed. No temporary artifacts or screenshot bytes were created.

#### Next
Run the stricter queue in an executable checkout; acquire permitted ordered full-post screenshots for strongest Korean C1 candidates first; then deterministic intake → human completeness verification → UI-chrome crop review → reviewed-crop validation → 1080x1080 contain normalization → strict source-backed carousel validation and Chrome inspection. C0 records must resolve exact provenance before entering that path.

---

## 원본 기록: 171-sol.md

### 171-sol — Discovery refresh

Time: 2026-09-18 09:33 KST

Started from current Threads README/handoff and main/recent commits. Initial directory result was truncated, so `169-sol.md` was read; during write finalization `170-sol.md` was discovered and read, and this note correctly takes the next sequence number. Repo tip wins.

#### Material work
Ran Korean-community-first public Discovery across requested community/search lanes plus Reddit support. No bulk crawling, login or anti-bot bypass. Inspected **40+ raw leads** and retained **15 new C1 candidates** after dedupe, safety, evidence and story-potential filtering. Each retained candidate is one Markdown file under `data/candidates/`; no grouped discovery JSON was created at `data/` root.

Strongest Korean additions include Blind `남편의 비밀적금` (55K views / 64 likes / 710 comments), `남편이 제 몰래 대출받아 코인을 하다가 다 날렸어요` (406 / 12), `남편 주식` (988 / 25), `코인 하는 남편 간섭해도 될까요?` (5,522 / 55), `남편 몰래 재산 탕진하고 대출까지 받은 아내` (6,318 / 48), `배우자의 동의없는 대출 및 주식투자는 이혼사유?` (1,958 / 19), `빚 안 갚고 주식 하겠다는 남편` (1,412 / like 1 / comments 33), plus Inven `자체생산)본인 일본 유학 및 결혼 썰` (15,261 / recommendation 9 / comments 21). Reddit support retained five money/wedding/family conflicts with exact public URLs. One attempted candidate creation hit an existing path and is not counted.

#### Asset / publication truth
Full-post screenshots captured: **0**. Source bytes acquired: **0**. Real source-backed carousel: **NO**. New candidates are A0_P0, `ASSETS_PENDING`, `publicationAllowed=false`. No OCR/moderation/rights verification, delivery or publication success was performed or inferred. Rights/privacy/human review remains required and only 04_REVIEW_PUBLISH may publish.

#### Verification truth
15 candidate Markdown files and Threads handoff were written through GitHub contents API. This was Discovery-data work, not renderer/UI code. No executable checkout/browser was used for `npm run check`, server smoke or Chrome E2E; none are claimed. No temporary artifacts were created.

#### Next
Acquire permitted ordered full-post screenshots for strongest Korean C1 candidates, prioritizing `남편의 비밀적금`, `남편이 제 몰래 대출받아 코인을 하다가 다 날렸어요`, `남편 주식`, then existing `나몰래 대출받은 남편`. Preserve full original body and do not reconstruct missing source assets. Then deterministic intake → human completeness verification → UI-chrome crop review → normalize → strict source-backed carousel verification.

---

## 원본 기록: 172-sol.md

### 172-sol — first real source-media acquisition

Time: 2026-09-18 10:24 KST

Started from current Threads handoff/main and latest sequential ops `171-sol.md`; repo tip wins.

#### Material work
Used an executable Windows checkout and public source inspection to acquire the first real source-backed body asset sequence. TheQoo post `결혼 승낙 받자마자 탈모인거 밝힌 남편..` (`https://theqoo.net/square/3826792703`) exposes exactly eight source-linked JPEGs. Downloaded all eight in page order and committed them under `data/source-packages/theqoo-3826792703/original/01.jpg` … `08.jpg`, with `SOURCE.md` provenance. Total real source bytes: 534,020. Threads source commit: `01475a8`; handoff commit: `2e5efbc3`.

The source page showed 104,013 views / 391 comments when inspected. No OCR was run. No automatic privacy masking was applied. Rights clearance is not claimed. `publicationAllowed=false`; only `04_REVIEW_PUBLISH` may publish.

A headless Chrome capture attempt for Blind `나몰래 대출받은 남편` rendered only Blind's error page; the failed image was deleted and is not counted. A TheQoo page-level headless screenshot also failed to produce a usable file. Failed temporary captures were cleaned.

#### Run truth
Discovery this implementation run: **raw 0 / retained 0**; latest useful Discovery remains **40+ raw / 15 retained C1** from 09:33 KST. Real full-post source sequence acquired: **8 ordered source images for 1 image-only post**. Source bytes: **8 / 534,020 bytes**. Human full-body completeness approval: **not yet**. Final real 1080x1080 source-backed carousel: **NO**. A1/P1: **0**.

#### Verification
`npm run check` was started in the executable checkout; syntax phase started, but final completion was not observed before handoff, so full check success is not claimed. Server smoke and Chrome carousel E2E are not claimed yet.

#### Next
Verify the eight-image TheQoo sequence for full-body completeness, run deterministic intake/crop/normalization, build the first 1080x1080 cover + ordered original-source carousel, and inspect it in Chrome. Continue Korean-community capture attempts without treating access/error pages as successful captures.

---

## 원본 기록: 173-sol.md

### 173-sol — 10:37 Discovery refresh

Time: 2026-09-18 10:37 KST

Started from current Threads README/handoff/main recent commits and latest sequential ops `172-sol.md`; repo tip wins.

#### Material work
Ran Korean-community-first public discovery plus Reddit backup lanes. Inspected **40+ raw leads** and retained **15 new C1 candidates**, each as one human-readable Markdown file under `data/candidates/`. No new grouped discovery JSON was created at `data/` root. Restricted sources were not bulk crawled and no login/anti-bot controls were bypassed.

Strongest new lanes include Blind `공직자윤리법 재산등록 대출???` (hidden spouse loan may surface through asset reporting; 220 views / 1 like / 14 comments), TheQoo coworker 70억원 investment-fraud report (22,704 views; legal/defamation review required), TheQoo `[네이트판] 취집한 친구 너무 얄밉네요...` (61,875 views; repost provenance/rights review), Reddit nearly-$300k hidden spouse debt followed by another secret loan, and a +412-vote bridesmaid story where a $500-per-person requested bracelet was followed by complaints about missing cash gifts.

All metrics recorded are from the same current observation where visible. Missing metrics were left unfilled rather than inferred. Posts whose image body was not actually read are marked `본문 미확인`.

#### Run truth
Raw/retained: **40+ / 15 C1**. New full-post screenshots: **0**. New source bytes: **0**. New real source-backed carousel: **NO**. All new candidates remain `A0_P0 / ASSETS_PENDING / publicationAllowed=false`; rights/privacy/human-review gates remain. Only `04_REVIEW_PUBLISH` may publish.

Existing first real source package for TheQoo 3826792703 remains 8 ordered JPEGs / 534,020 bytes; completeness is not yet human-approved and no final 1080x1080 carousel is claimed.

#### Verification
Candidate Markdown/handoff only; no runtime path changed. No npm check/server smoke/browser E2E/OCR/moderation was run or claimed in this Discovery refresh.

#### Next
Verify the existing eight-image TheQoo sequence and build/inspect the first strict cover + original-source carousel. Continue permitted screenshot/media acquisition for strongest Korean C1 candidates without counting error/access pages as captures.

---

## 원본 기록: 174-sol.md

### 174-sol — runtime regression fix

Time: 2026-09-18 11:17 KST

Started from current Threads handoff/main and latest sequential ops `173-sol.md`; repo tip wins.

#### Material work
Used the online Windows executable checkout, pulled current `main`, and ran `npm run check`. This exposed a real regression in `test/card-story-model.test.mjs`: legacy assertions still expected automatic PII masking, while the binding source-first rule and runtime storyboard model require user-directed privacy handling (`automaticMasking=false`, `automaticPiiMutation=false`, manual review required).

Updated only the regression expectations to the binding behavior; no source screenshot/body was rewritten or auto-masked. Threads commit `1a62842` was pushed to `main`, followed by handoff update `60ffaba5`.

#### Verification
After the fix, `npm run check` passed. The suite included syntax checks, regression tests, and an actual ffmpeg 1080x1920 H.264 render + ffprobe validation. No Chrome user-facing E2E was run in this implementation run, so none is claimed.

#### Discovery / asset truth
Discovery this implementation run: **raw 0 / retained 0**; latest useful Discovery remains **40+ / 15 C1** from 10:37 KST. New full-post screenshots: **0**. New source bytes: **0**. Real final source-backed carousel: **NO**. Existing TheQoo 3826792703 package remains eight ordered real JPEGs / 534,020 bytes, pending human completeness approval. A1/P1 remain 0; `publicationAllowed=false`; only `04_REVIEW_PUBLISH` may publish.

#### Next
Human-verify the eight-image TheQoo sequence, build the first strict 1080x1080 cover + ordered original-source carousel, inspect it in Chrome, and continue permitted Korean C1 full-post acquisition.

---

## 원본 기록: 175-sol.md

### 175-sol — source-media provenance correction

Time: 2026-09-18 12:17 KST

Started from current Threads handoff/main and latest sequential ops `174-sol.md`; repo tip wins.

#### Material work
Pulled current `main` into the executable Windows checkout and visually inspected all eight files in `data/source-packages/theqoo-3826792703/original/`. They are real source-attached TV-frame JPEG media, not browser/full-post screenshots. This distinction matters because attached source media must not be allowed to imply full-post screenshot capture or full-body completeness.

Updated `scripts/build-screenshot-intake-manifest.mjs` with an explicit `SOURCE_MEDIA_DOWNLOAD` acquisition state. Its provenance text states that source-linked downloaded media does **not** claim a full-post screenshot or full-body completeness. Generated `data/source-packages/theqoo-3826792703/intake-manifest.json` from the actual eight JPEGs, preserving filename order and recording dimensions, byte lengths and SHA-256 while leaving `fullBodyCaptureStatus: pending`, OCR/vision false, privacy user-directed, publication false and publish owner 04. Threads implementation commit: `ca6808e`; handoff update: `bab285e1`.

#### Verification
`npm run check` passed after the change. The eight files were actually opened and visually inspected. No OCR was run. No Chrome user-facing E2E was run this implementation run, so none is claimed.

#### Discovery / asset truth
Discovery this implementation run: **raw 0 / retained 0**; latest useful Discovery remains **40+ / 15 C1** from 10:37 KST. Full-post screenshots captured this run: **0**. New source bytes: **0**. Existing real source media: **8 JPEGs / 534,020 bytes**. Real final source-backed carousel: **NO**. A1/P1 remain 0; `publicationAllowed=false`; only `04_REVIEW_PUBLISH` may publish.

#### Next
Acquire actual permitted full-post/browser screenshots for a strong Korean C1 candidate, run them through intake/completeness/crop/normalization, build the first strict 1080x1080 cover + ordered-original carousel, and inspect it in Chrome. Attached media alone must not be promoted to full-post screenshot completeness without source-backed evidence.

---

## 원본 기록: 176-sol.md

### 176-sol — ddalkkak Threads Community v1.11.7 benchmark analysis

Updated: 2026-09-18 KST

#### Request

User asked for a detailed analysis of:

- https://github.com/apache3563-bit/ddalkkak-threads-community/releases/tag/v1.11.7

and asked to leave the report in the communication channel.

#### What was inspected

Pinned to tag `v1.11.7`, not only the Release description.

Inspected:
- Release metadata / installer digest
- `README_KO.md`
- `package.json`
- `.env.example`
- `src/server.ts`
- `src/scheduler.ts`
- `src/threads-api.ts`
- `src/rss.ts`
- `src/urlSafety.ts`
- `src/materialSelector.ts`
- `src/claude.ts`
- `src/builtInFeeds.ts`
- productRadar module inventory and deterministic media renderer
- Threads OAuth/token refresh search hits
- media publication/relay architecture

#### Main conclusion

This repository is not stronger than our system in community/viral discovery. Its strongest reference value is **operational publishing safety and local Windows productization**.

High-value ideas to clean-room reimplement in our own architecture:

1. atomic publication claim and explicit `publishing` state
2. interrupted publish recovery that treats uncertain outcome as fail-closed and never auto-reposts
3. duplicate/idempotency publish guard
4. DRY_RUN + auto-publish-off safe defaults
5. per-account daily publish cap
6. no silent media→text fallback
7. Threads token expiry/refresh lifecycle
8. shared safe URL/media fetch with private-IP, redirect, response-size and DNS-rebinding defenses
9. dedicated media-public URL/provider abstraction instead of exposing the main local app
10. strict LLM context budgets and same-source caps
11. optional local CLI AI provider pattern
12. deterministic media export guards (hashes, fixed dimensions, max input pixels)

#### What not to copy

- Discovery is mostly RSS / manual URL / text / keyword, so our Theme Lane + Viral + Comfort + community-first discovery remains the canonical direction.
- Some source files are very large (especially database-centric modules); do not abandon our feature-folder/ownership architecture.
- Default topic weighting is biased toward AI/automation when user keywords are absent; unsuitable as a global policy for our multi-theme system.
- The source is AGPL-3.0. Do not copy implementation code into our repo unless licensing implications are explicitly accepted. Use clean-room behavioral reimplementation only.
- README release wording appears stale relative to the actual published v1.11.7 installer; treat documentation state as fallible.

#### Current Threads comparison

Current repo is already `0.42.1` and has:
- Scheduler planner with theme/source/format spacing
- Direct Threads / Buffer targets
- Official Media capability model
- SQLite persistence with revisions/migration
- source package/intake and screenshot provenance
- Card/privacy/Warehouse/video pipelines

Therefore the useful delta is mainly:

```text
publication transaction + crash recovery
safe external fetch
token lifecycle
real-media publish fail-closed
```

#### Canonical report

Full detailed analysis written to:

`kimjae134679/Threads/docs/REFERENCE_DDALKKAK_V1.11.7_ANALYSIS.md`

#### Next implementation priority suggested

P0:
- add publication attempt ledger
- atomic claim
- publishing recovery
- unknown-outcome manual review
- idempotency guard
- daily account cap
- no media fallback

P1:
- shared safe-fetch module with DNS pinning for media
- token lifecycle state + refresh
- media-publication provider abstraction

P2:
- explicit source-context budgets for research/draft
- optional local CLI generation provider

No live Threads publication was performed as part of this analysis.
No external source code was copied into our project.

---

## 원본 기록: 177-sol.md

### 177-sol — first real full-page browser screenshot

Time: 2026-09-18 13:25 KST

Started from current Threads handoff/main and inspected the latest sequential ops `176-sol.md` (ddalkkak benchmark); repo tip wins.

#### Material work
Used the executable Windows checkout and real installed Chrome in headless CDP mode against the exact public TheQoo source for `결혼 승낙 받자마자 탈모인거 밝힌 남편..`. A naive viewport screenshot showed only the first media frame, so it was not counted as full-post capture. Then used Chrome DevTools Protocol, scrolled the entire document to trigger lazy-loaded media, returned to top, and captured beyond viewport.

The retained source-backed browser screenshot is `data/source-packages/theqoo-3826792703/screenshots/full-page-20260918-1319.png`: **1103×13187, 1,442,565 bytes**. It was actually opened and visually inspected. The title/post header and all eight previously acquired source-attached TV-frame images render in order. Comments/list UI remain below the post, so reviewed crop boundaries are still required before carousel use.

Added dependency-free `scripts/capture-full-post-cdp.mjs` to make this capture path reproducible with an already-authorized local Chrome CDP port. It scrolls to load lazy media and does not equate full-page capture with human completeness. No OCR, rewriting, privacy masking, rights inference or publication is performed.

Threads commit pushed to main: `6201a68`.

#### Verification
`node --check scripts/capture-full-post-cdp.mjs` passed. Full `npm run check` passed, including actual ffmpeg/ffprobe 1080×1920 regression. The retained PNG was visually inspected. No final carousel/Chrome carousel E2E is claimed yet.

#### Discovery / asset truth
Discovery this implementation run: **raw 0 / retained 0**; latest useful Discovery remains **40+ / 15 C1**. Full-post/browser screenshots captured this run: **1 full-page PNG / 1,442,565 bytes**. Existing attached source media remains **8 JPEGs / 534,020 bytes**. Real final source-backed carousel: **NO**. A1/P1 remain 0; `publicationAllowed=false`; only `04_REVIEW_PUBLISH` may publish.

#### Next
Review/crop the actual post-content region while preserving the full eight-image body, split/normalize it into strict 1080×1080 ordered body slides, create slide 1 from the exact original title plus selected image, validate, then inspect the first real source-backed carousel in Chrome. Continue Korean-community-first high-volume Discovery in parallel.

---

## 원본 기록: 178-sol.md

### 178-sol — reviewed real body carousel slices

Time: 2026-09-18 14:19 KST

Started from current Threads handoff/main and latest sequential ops `177-sol.md`; repo tip wins.

#### Material work
Used the real Chrome full-page TheQoo capture already acquired for `결혼 승낙 받자마자 탈모인거 밝힌 남편..`. Human visual review established a post-content crop (`x=15,y=150,width=760,height=3850`) that keeps the title/header and all eight source-body images while excluding comments/list/advertising below. Stored this as `review/crop-plan.json` with no OCR, no automatic privacy masking, no rights inference and publication disabled.

Generated five contiguous 1080×1080 screenshot-derived body slides (`carousel/body-00.png`..`body-04.png`) and visually inspected all five. The final slice uses contain/padding so the last two source frames are preserved rather than destructively cropped. Threads material commit: `74355af`; handoff commit follows separately.

#### Verification truth
The five output PNGs were actually opened and inspected. `npm run check` was started; syntax output was observed but final completion was not obtained before handoff, so PASS is not claimed. No cover, complete carousel, Chrome carousel E2E, rights approval or publication is claimed.

#### Discovery / asset truth
Discovery this run: **raw 0 / retained 0**; latest useful remains **40+ / 15 C1**. New full-page screenshots this run: **0**; existing full-page capture reused. New reviewed 1080×1080 source-backed body slides: **5**. Full source media remains 8 JPEGs. Final source-backed carousel: **NO — cover pending**. A1/P1 remain 0; only 04_REVIEW_PUBLISH may publish.

#### Next
Create slide 1 using the exact original title plus a selected image, assemble it with the five body slides, validate and inspect the complete carousel in real Chrome, then finish a full `npm run check`. Continue Korean-community-first discovery/acquisition in parallel.

---

## 원본 기록: 179-sol.md

### 179-sol — rendered source-backed carousel validation gate

Time: 2026-09-18 15:15 KST

Started from current Threads handoff/main and latest sequential ops `178-sol.md`; repo tip wins.

#### Material work
Added `scripts/validate-rendered-source-carousel.mjs` to Threads at commit `b891dc73fd13717f2ea2f700bc4243783647a120`.

The validator bridges the strict source-backed carousel plan to actual rendered PNG artifacts and fails closed unless: slide count exactly matches the plan; filenames/order are deterministic `slide-01.png` onward; slide 1 is COVER_ONLY; every later slide is ORIGINAL_POST_SCREENSHOT; every rendered asset is a real 1080x1080 PNG; and publication remains disabled with owner `04_REVIEW_PUBLISH`. It records bytes and SHA-256 per rendered slide. Its output explicitly keeps OCR, automatic privacy masking, rights clearance and publication false/unclaimed.

This is a material production/review guard, not a claim that a final carousel was rendered.

#### Discovery / asset truth
Discovery this run: **raw 0 / retained 0**; latest useful remains **40+ / 15 C1**. New full-post screenshots: **0**. Existing TheQoo package remains one real full-page Chrome PNG, eight attached source JPEGs and five visually reviewed 1080x1080 screenshot-derived body slides. Final source-backed carousel: **NO — cover and assembled six-slide rendered package remain pending**. A1/P1 remain 0; only 04_REVIEW_PUBLISH may publish.

#### Verification truth
Remote executable/browser device was unavailable, so runtime execution, `npm run check`, server smoke and Chrome E2E are not claimed. GitHub contents API committed the validator and handoff. No temporary artifacts or source bytes were created/altered.

#### Next
On an executable device, render the exact-original-title cover, assemble cover + five reviewed body slides, run the plan validator and this rendered-artifact validator, inspect all six in Chrome, and finish `npm run check`. Continue Korean-community-first high-volume discovery/acquisition in parallel.

---

## 원본 기록: 180-sol.md

### 180-sol — deterministic rendered carousel assembler

Time: 2026-09-18 16:19 KST

Started from current Threads handoff/main and latest sequential ops `179-sol.md`; repo tip wins.

#### Material work
Added `scripts/assemble-rendered-source-carousel.mjs` to Threads at commit `b77fa4fd5ded63e0134a3164d6b46dfb0cfe99de`.

The assembler takes one real 1080x1080 cover plus sequential reviewed `body-00.png` onward and creates deterministic `slide-01.png` onward. It fails closed on non-PNG/non-1080 assets or sequence gaps. Body slides are byte-copied rather than rewritten, summarized, stretched, automatically masked, or regenerated. It records source SHA-256, byte count and dimensions for every assembled slide and clears stale slide outputs before assembly.

Publication remains `false`, owner remains `04_REVIEW_PUBLISH`, and OCR/privacy masking/rights/publication/human visual approval are explicitly unclaimed. This is the production bridge before `validate-rendered-source-carousel.mjs`, not a publication action.

#### Discovery / asset truth
Discovery this run: **raw 0 / retained 0**; latest useful remains **40+ / 15 C1**. New full-post screenshots: **0**. Existing TheQoo package remains one real full-page Chrome PNG, eight attached source JPEGs and five reviewed 1080x1080 screenshot-derived body slides. Final source-backed carousel: **NO — exact-title cover and assembled six-slide package remain pending**. A1/P1 remain 0.

#### Verification truth
Remote Windows/browser device `KJW` was checked and is currently offline, so runtime execution, `npm run check`, server smoke and Chrome E2E are not claimed. GitHub contents API committed the implementation and handoff. No source bytes or temporary artifacts were altered.

#### Next
When KJW is online, render the exact-original-title cover for `결혼 승낙 받자마자 탈모인거 밝힌 남편..`, run the assembler, validate the strict plan/rendered package, inspect all six slides in Chrome, and complete `npm run check`. Continue Korean-community-first high-volume discovery/acquisition in parallel.

---

## 원본 기록: 181-sol.md

### 181-sol — strict source-backed carousel plan generator

Time: 2026-09-18 17:15 KST

Started from current Threads handoff/main and latest sequential ops `180-sol.md`; repo tip wins.

#### Material work
Added `scripts/create-source-carousel-plan.mjs` to Threads at commit `eb41e85e329eb9443e42d5f6615b334eb726f891`.

The generator creates a fail-closed `SOURCE_BACKED_CAROUSEL_PLAN`: slide 1 is `COVER_ONLY` carrying the exact original title supplied by the operator, and slide 2 onward may only be sequential `body-00.png` onward marked `ORIGINAL_POST_SCREENSHOT`. It rejects sequence gaps, non-PNG inputs and non-1080x1080 assets, and records SHA-256/bytes/dimensions for every slide. It does not summarize/rewrite body content, auto-mask privacy, infer OCR, clear rights, publish, or claim human approval. `publicationAllowed=false`, owner `04_REVIEW_PUBLISH`.

This fills the deterministic plan-generation gap before `assemble-rendered-source-carousel.mjs` and `validate-rendered-source-carousel.mjs` without fabricating the missing cover.

#### Discovery / asset truth
Discovery this run: **raw 0 / retained 0**; latest useful remains **40+ / 15 C1**. New full-post screenshots: **0**. Existing TheQoo package remains one real full-page Chrome PNG, eight attached source JPEGs and five reviewed 1080x1080 screenshot-derived body slides. Final source-backed carousel: **NO — exact-title cover and assembled six-slide package remain pending**. A1/P1 remain 0.

#### Verification truth
Remote executable/browser device returned `No devices available`, so runtime execution, `npm run check`, server smoke and Chrome E2E are not claimed. GitHub contents API committed the implementation and handoff. No source bytes or temporary artifacts were altered.

#### Next
When executable device returns, render exact-original-title cover `결혼 승낙 받자마자 탈모인거 밝힌 남편..`, generate strict plan, assemble six slides, validate rendered package, inspect all six in Chrome, and run `npm run check`. Continue Korean-community-first high-volume discovery/acquisition.

---

## 원본 기록: 182-sol.md

### 182-sol — focused Korean-community acquisition refresh

Time: 2026-09-18 18:14 KST

Started from current Threads handoff/main and latest sequential ops `181-sol.md`; repo tip wins.

#### Material work
Ran a public/indexed Korean-community-focused acquisition refresh across TheQoo/Ruliweb/FMKorea/Ppomppu/Inven/Arca search lanes. Inspected **13 raw indexed results** in this focused pass and retained **6 new exact-public-source leads** after dedupe, audience-comfort and provenance filtering. This is explicitly a focused acquisition refresh, not falsely labeled as the requested 40–80-lead high-volume run.

Committed the complete observed metadata to Threads at `data/_raw_batches/discovery-2026-09-18-1814.md` (commit `65b35ed4fba7230e41fbfb244cbd3db5658c8787`). Each retained record includes source/public URL, exact observed title, observation time, only visible metrics, body-read state, comments-read state, image/source-asset visibility, swipe rationale and exact acquisition state.

Strong additions include TheQoo `[판] 전업주부 하려고 대학 나왔냐는 시어머니` (84,564 views / 576 comments observed), `판) 임밍아웃 몰카 왜 하는 건가요?` (146,987 / 701), and image-centric `결혼 5년차 아내 불면증 고친 남편` (15,726 / 30; three source image entries observed but their contents were not read). Also retained the 4억원 재산 “시험” marriage reversal, a lighter two-year marriage review, and an Inven passive-income/work-values retelling. Serious disappearance/sexual-violence results were rejected for comfort; duplicate/provenance-incomplete fragments were not promoted.

All new leads remain `ASSETS_PENDING`. Indexed/partial body visibility is not screenshot acquisition, OCR success, rights clearance or publication approval.

#### Existing production truth
TheQoo `결혼 승낙 받자마자 탈모인거 밝힌 남편..` still has one real Chrome full-page PNG, eight attached source JPEGs and five human-reviewed 1080x1080 screenshot-derived body slides. Strict plan/assemble/render-validation code exists, but the exact-title cover and final six-slide rendered package remain missing.

#### Verification truth
Remote Windows/browser device returned `No devices available`. Therefore no runtime execution, `npm run check`, server smoke, new screenshot capture, cover render or Chrome E2E is claimed. Restricted sources were not bypassed or bulk crawled. GitHub contents API committed the discovery batch and updated handoff (`f13ed404da830b32758256271c2c19c6dfb50710`).

#### Run counts
- Focused discovery/acquisition: **raw 13 / retained 6**.
- Latest full high-volume useful Discovery remains **40+ / 15 C1**.
- New full-post screenshots: **0**.
- Existing full-page screenshots: **1 real PNG**.
- Existing attached source media: **8 JPEGs**.
- Existing reviewed body slides: **5**.
- Final real source-backed carousel: **NO**.
- A1/P1: **0**; only `04_REVIEW_PUBLISH` may publish.

#### Next
When executable device returns: render exact-original-title cover for the existing TheQoo package, run strict plan → assemble → validate, inspect all six in Chrome and run `npm run check`. Then capture full-post screenshots for the two new high-response TheQoo leads and obtain actual ordered source images for the image-centric insomnia post. Continue a full 40–80 raw Korean-community discovery pass when coverage permits.

---

## 원본 기록: 183-sol.md

### 183-sol — exact-title cover preview builder

Time: 2026-09-18 19:19 KST

Started from current Threads handoff/main and latest sequential ops `182-sol.md`; repo tip wins.

#### Material work
Added `scripts/build-exact-title-cover-html.mjs` to Threads at commit `f26699b40b7c1b7ebb5cd0509c9b0e14d21c5328`, then wired it into `npm run syntax` at `b8ad2176dcb7c92907f7baec96a801c4d5eb90b5`.

The builder accepts only an explicitly supplied exact title, a real non-empty jpg/jpeg/png/webp file and an output HTML path. It creates a fixed 1080x1080 cover-only browser composition with the supplied source image, restrained readability gradient and the exact title; it performs no title rewriting. Output metadata explicitly says `renderedPng:false`, preventing an HTML preview from being counted as a finished carousel asset.

This closes the missing deterministic handoff between a chosen real source image/title and browser cover rendering while preserving the binding carousel rule: slide 1 cover only, slide 2+ original screenshots only.

#### Verification truth
Remote device `KJW` is offline, so runtime execution, `npm run check`, server smoke, actual cover PNG capture and Chrome E2E are **not claimed**. No OCR, moderation, privacy masking, rights clearance, delivery or publication action occurred.

#### Counts
- Discovery this run: **raw 0 / retained 0**.
- Latest full useful Discovery: **40+ raw / 15 C1**; latest focused refresh: **13 / 6**.
- New full-post screenshots: **0**.
- Existing full-page screenshots: **1 real PNG**.
- Existing attached source media: **8 JPEGs**.
- Existing reviewed body slides: **5**.
- Final real source-backed carousel: **NO**; exact-title cover PNG and rendered six-slide package remain pending.
- A1/P1: **0**; only `04_REVIEW_PUBLISH` may publish.

#### Next
When KJW returns, build the cover using exact title `결혼 승낙 받자마자 탈모인거 밝힌 남편..` plus one existing real source image, render and visually inspect the 1080x1080 PNG in Chrome, then run strict plan → assemble → validator and inspect all six slides. Run `npm run check`/relevant smoke tests and record only observed results. Continue Korean-community-first acquisition/discovery without bypassing restricted access.

---

## 원본 기록: 184-sol.md

### 184-sol — strict cover-only cleanup

Time: 2026-09-18 20:16 KST

Started from current Threads handoff/main and latest sequential ops `183-sol.md`; repo tip wins.

#### Material work
Corrected `scripts/build-exact-title-cover-html.mjs` in Threads at commit `cedd219999e921245037a57975e3dacdfaf21b7f`.

The previous preview contained an additional `원문 표지` source-label pill. The binding format says slide 1 is cover only with one image plus the original-title/hook text, so that extra label was removed. The HTML now contains only the supplied real image, readability gradient and exact supplied title. It also emits `data-cover-content=image-plus-exact-title-only` plus matching output metadata for downstream/browser verification. It still performs no title rewriting and does not claim HTML generation is a rendered PNG.

#### Verification truth
Remote Windows/browser device is unavailable this run, so runtime execution, `npm run check`, server smoke, actual cover PNG capture and Chrome E2E are **not claimed**. No OCR, moderation, automatic privacy masking, rights clearance, delivery or publication action occurred.

#### Counts
- Discovery this run: **raw 0 / retained 0**.
- Latest full useful Discovery: **40+ raw / 15 C1**; latest focused refresh: **13 / 6**.
- New full-post screenshots: **0**.
- Existing full-page screenshots: **1 real PNG**.
- Existing attached source media: **8 JPEGs**.
- Existing reviewed body slides: **5**.
- Final real source-backed carousel: **NO**; exact-title cover PNG and rendered six-slide package remain pending.
- A1/P1: **0**; only `04_REVIEW_PUBLISH` may publish.

#### Next
When the browser device returns, render and visually inspect the strict 1080x1080 cover using exact title `결혼 승낙 받자마자 탈모인거 밝힌 남편..` plus an existing real source image, then run strict plan → assemble → validator and inspect all six slides. Run `npm run check`/relevant smoke tests and record only observed results. Continue Korean-community-first acquisition/discovery without bypassing restricted access.

---

## 원본 기록: 185-sol.md

### 185-sol — rendered carousel byte-fidelity gate

Time: 2026-09-18 21:16 KST

Started from current Threads handoff/main and latest sequential ops `184-sol.md`; repo tip wins.

#### Material work
Strengthened `scripts/validate-rendered-source-carousel.mjs` in Threads at commit `747f400ce82ac55b0594abfc4f3f6cb60012f9f4`.

The validator previously proved slide count, order/kind, PNG format and 1080x1080 dimensions, but a different 1080x1080 PNG could still satisfy those structural checks. It now fails closed unless each assembled slide's SHA-256 exactly matches the corresponding planned source slide; planned byte count and dimensions are also checked when present. Successful validation records `byteIdenticalToPlan:true` for each slide and `allSlidesByteIdenticalToPlan:true` for the package.

This specifically protects the binding rule that slide 2+ remain the ordered source-backed screenshot derivatives rather than silently altered/replaced cards. It does **not** claim that the screenshots themselves cover the full post; that still requires source/crop provenance and visual review.

#### Verification truth
No executable browser/runtime was available this run, so `npm run check`, server smoke, cover PNG rendering and Chrome E2E are not claimed. GitHub contents API accepted the material commit and handoff update. No OCR, moderation, automatic privacy masking, rights clearance, delivery or publication occurred.

#### Counts
- Discovery this run: **raw 0 / retained 0**.
- Latest full useful Discovery: **40+ raw / 15 C1**; latest focused refresh: **13 / 6**.
- Top candidates remain `[판] 전업주부 하려고 대학 나왔냐는 시어머니`, `판) 임밍아웃 몰카 왜 하는 건가요?`, `결혼 5년차 아내 불면증 고친 남편`.
- New full-post screenshots: **0**.
- Existing full-page screenshots: **1 real PNG**.
- Existing attached source media: **8 JPEGs**.
- Existing reviewed body slides: **5**.
- Full-post screenshots captured this run: **NO**.
- Real final source-backed carousel: **NO**; exact-title cover PNG and rendered six-slide package remain pending.
- A1/P1: **0**; only `04_REVIEW_PUBLISH` may publish.

#### Next
When browser/runtime access returns, render and inspect the strict 1080x1080 exact-title cover, run plan → assemble → byte-fidelity validator, visually inspect all six slides in Chrome, and run `npm run check`/relevant smoke. Continue Korean-community-first full-post acquisition and high-volume discovery without bypassing restricted access.

---

## 원본 기록: 186-sol.md

### 186-sol — first real source-backed six-slide carousel

Time: 2026-09-18 22:14 KST

Started from current Threads handoff/main and latest sequential ops `185-sol.md`; repo tip wins.

#### Material work
Remote browser/runtime access returned. For TheQoo `결혼 승낙 받자마자 탈모인거 밝힌 남편..`, rendered a real 1080x1080 Chrome cover from existing source image `original/01.jpg` plus the exact original title. Visual inspection caught a real regression: the first cover render clipped the title at the bottom. Fixed `scripts/build-exact-title-cover-html.mjs` by reducing title size and moving the title safe area upward, rerendered, and visually confirmed the complete title is visible.

Generated `source-carousel-plan.json`, assembled six deterministic slides, and validated them. Output is `rendered/slide-01.png` through `slide-06.png`: slide 1 is cover only; slides 2–6 are the five previously reviewed screenshot-derived original-post body slides. `validate-rendered-source-carousel.mjs` returned `validated:true`, `slideCount:6`; each assembled slide is SHA-256 byte-identical to its planned source. Material Threads commit: `90466f181676fb4a180f48585733cd6d34ff855c`.

#### Verification truth
`npm.cmd run check` completed with exit code 0. The included actual ffmpeg/ffprobe regression passed at 1080x1920 H.264/yuv420p/30fps. Corrected cover was actually rendered in Chrome and visually inspected. Body slides were visually inspected in the preceding crop run; assembly copies them byte-for-byte and the current validator proves that fidelity. No OCR, moderation, automatic privacy masking, rights clearance, delivery or publication occurred.

#### Counts
- Discovery this run: **raw 0 / retained 0**.
- Latest full useful Discovery: **40+ raw / 15 C1**; latest focused refresh: **13 / 6**.
- Top pending candidates remain `[판] 전업주부 하려고 대학 나왔냐는 시어머니`, `판) 임밍아웃 몰카 왜 하는 건가요?`, `결혼 5년차 아내 불면증 고친 남편`.
- New full-post screenshots this run: **0**.
- Existing selected-post full-page screenshot: **1 real PNG**; attached source media: **8 JPEGs**; reviewed body slides: **5**.
- Full-post screenshots captured for selected post: **YES (existing capture)**.
- Real final source-backed carousel produced: **YES — 6 slides**, strict structure + byte-fidelity validated.
- This is not rights clearance or publication approval. A1/P1 remain **0**; only `04_REVIEW_PUBLISH` may publish.

#### Next
Return priority to Korean-community-first acquisition: acquire full-post screenshots for the strongest pending candidates and run another 40–80 raw discovery pass when coverage permits. Feed the next real Source Package through the same screenshot intake/crop/normalization/strict-carousel pipeline. Keep unavailable assets `ASSETS_PENDING` and do not advance rights/publication state without evidence.

---

## 원본 기록: 187-sol.md

### 187-sol — Korean community acquisition refresh

Time: 2026-09-18 23:22 KST

Started from current Threads handoff/main and latest sequential ops `186-sol.md`; repo tip wins.

#### Material work
Returned priority to `01_DISCOVERY`. Fresh public/index Korean-community-first search inspected **29 raw returned leads** and retained **10** after dedupe, safety/comfort, source fit and story-potential filtering. Coverage was thinner than the 40–80 target, so the batch records the actual count rather than padding it.

Added `data/_raw_batches/discovery-2026-09-18-2315.md` with source, public URL, exact observed title, observation time, only surfaced metrics, full-body/comments read state, observed source image state, swipe rationale and exact acquisition state for every retained candidate. Material Threads commit: `646348dfb5b9ed94b7af527b8de98759076094a7`.

Top acquisition targets: `[네이트판] 너무 많이 먹는 남편 ㅠㅠ` (67,558 / 599; three source image URLs observed), `네이트판) 아이이름 짓는데 술집여자 같다는 남편` (37,327 / 256; three source images observed), `[네이트판] 우리집 홈캠을 보고 계셨던 시어머니.` (71,391 surfaced views; strong boundary/reveal story), and `[네이트판] 찬밥에 쉰김치만 주는 시엄마, 그럴 수도 있다는 남편` (strong mirrored-retaliation reversal). All remain `ASSETS_PENDING`.

#### Safety/editorial exclusions
Deprioritized a long post containing repeated homicidal/violent language, a marriage post containing suicide-attempt and intimate medical/sexual detail, public-figure affair accusation/gossip framing, and generic news/statistics items with weak source-story fit. No moderation success is claimed; these are Discovery editorial filters only.

#### Verification truth
Discovery-data-only repo change; no renderer path changed, so no new browser E2E claim is made. No OCR, automatic privacy masking, moderation API, rights clearance, delivery or publication occurred. Existing first source-backed six-slide carousel remains the prior verified output.

#### Counts
- Discovery this run: **29 raw / 10 retained**.
- Latest full useful Discovery remains **40+ raw / 15 C1**.
- New full-post screenshots this run: **0**.
- Existing selected-post full-page screenshot: **1 real PNG**; attached source media: **8 JPEGs**; reviewed body slides: **5**.
- Full-post screenshots captured: **YES for existing first selected post; NO new capture this run**.
- Real source-backed carousel produced: **YES existing first 6-slide carousel; NO second carousel this run**.
- A1/P1 remain **0**; only `04_REVIEW_PUBLISH` may publish.

#### Next
Acquire complete source screenshots/media for `너무 많이 먹는 남편 ㅠㅠ` first, then `아이이름 짓는데 술집여자 같다는 남편` and `우리집 홈캠을 보고 계셨던 시어머니.`. Preserve exact source order/body and keep candidates `ASSETS_PENDING` until real assets exist. Continue broad Korean-community discovery toward the 40–80/15–30 target without padding weak leads.

---

## 원본 기록: 188-sol.md

### 188-sol — executable carousel revalidation

Time: 2026-09-19 01:16 KST

Started from current Threads handoff/main and latest sequential ops `187-sol.md`; repo tip wins.

#### Material work
Windows execution device returned online. Pulled Threads current main and ran full project verification. Persisted runtime truth in Threads `docs/run-verification-2026-09-19-0116.md` at commit `5de677049ee1e612deb72be14126b94546ef0d66`; handoff updated at `7676292fe9ace5bcd777ba01d00b6a92ca2b12ca`.

#### Verification
- `npm run check`: PASS.
- Strict rendered source-carousel validator: PASS.
- Six rendered files are 1080x1080 and byte-identical to the strict plan.
- Slide 1 = COVER; slides 2–6 = ordered ORIGINAL_POST_SCREENSHOT.
- Temporary local `e2e48.mjs` was removed; working tree clean afterward.

No OCR/moderation/automatic privacy masking/rights clearance/delivery/publication is inferred. Only 04_REVIEW_PUBLISH may publish.

#### Counts
Discovery this run: raw 0 / retained 0. Latest full useful discovery remains 40+ / 15 C1; latest additional refresh 29 / 10. New screenshots: 0. Existing first package: 1 real full-page PNG, 8 source JPEGs, 5 reviewed body slides, 1 cover, 6 assembled rendered slides. Real source-backed carousel: YES, existing first package revalidated. A1/P1 remain 0.

#### Next
Acquire full-post assets for the strongest pending Korean candidates, starting with `[네이트판] 너무 많이 먹는 남편 ㅠㅠ`, then build and visually inspect the second strict source-backed carousel. Continue high-volume Korean discovery without padding weak leads.

---

## 원본 기록: 189-sol.md

### 189-sol — Nate Pann homecam provenance work

Time: 2026-09-19 02:16 KST

Started from current Threads handoff/main and latest sequential ops `188-sol.md`; repo tip wins.

#### Material work
Performed a focused public provenance/acquisition search over the three top pending Nate Pann leads. Added Threads `01_DISCOVERY/candidates/2026-09-19-c1-natepann-homecam-mother-in-law.md` at commit `1d3cca06657d84273d03daf6174c9cecb40ee7c6`, then updated handoff at `89e800400a7aecb4f88b16cf947168e9f3f17c45`.

For `우리집 홈캠을 보고 계셨던 시어머니.` public historical corroboration was recovered: a community-best index reports 274,169 views / 1,969 recommendations / 660 comments, while a 2024-12-17 NewsBalance report says the post had exceeded 160,000 views and 1,200 recommendations by 09:00 that day. The original Nate Pann body and comments were NOT directly read this run and the exact individual Nate Pann URL was NOT recovered. Therefore the candidate is explicitly `PROVENANCE_PENDING`; it must not enter deterministic screenshot capture. Reporting text must never be substituted for original-post screenshots.

#### Counts
Focused acquisition search: raw 3 / materially corroborated 1. This is not counted as a full high-volume discovery pass. New screenshots: 0. Existing first package remains 1 real full-page PNG + 8 source JPEGs + 5 reviewed body slides + exact-title cover + strict six-slide source-backed carousel. A1/P1 remain 0.

#### Verification truth
Public web search and GitHub contents writes only. No executable/browser runtime was used this run, so `npm run check`, server smoke, Chrome E2E, screenshot capture, OCR, moderation, rights clearance, delivery, or publication are not claimed. Only 04_REVIEW_PUBLISH may publish.

#### Next
Recover the exact individual public Nate Pann URL for the homecam story without bypassing access controls; acquire full-post screenshots for the strongest exact-source pending candidate; continue high-volume Korean-community discovery without padding weak leads.

---

## 원본 기록: 190-account-b-nova.md

### 190 — [B계정] Nova: 표지 규칙 최신 결정 반영 제안

Time: 2026-09-19 KST

최신 사용자 지시와 현재 T-0008 기록을 대조했습니다. `040-sol-to-astra.md`에 적힌 “첫 원본 이미지를 블러/암전해서 표지로 사용”은 이제 최신 결정과 충돌하므로 더 이상 기본 규칙으로 쓰면 안 됩니다.

#### 최신 사용자 결정

- 첫 장 제목은 짧고 자극적으로 구성해 한눈에 들어와야 한다.
- 제목 글씨는 크게 쓰고 기존 예시보다 조금 더 위에 배치한다.
- 배경을 블러 처리하지 않는다.
- 글씨에는 두꺼운 효과가 아니라 얇은 테두리만 적용한다.
- 원문에 이미지가 있으면 해당 원문 자산을 이용해 표지를 만들 수 있다.
- 원문이 글뿐이라면 별도 생성 이미지를 만들지 않고 글만으로 표지를 구성해도 된다.
- 두 번째 장 이후의 원문 스크린샷 순서와 내용은 임의로 바꾸지 않는다.

#### 구현 경계 제안

표지를 본문 캡처와 분리된 `COVER` 렌더 유형으로 유지하되, 입력 모드는 두 가지로 고정하는 편이 안전합니다.

1. `source_media_cover`: 실제 원문 이미지를 사용하되 블러 없이 제목을 위쪽에 얹는다.
2. `text_only_cover`: 원문에 적합한 이미지가 없으면 텍스트와 단색/기본 배경만 사용한다.

공통 검증 항목은 `headline length`, `headline top offset`, `outline width`, `blur = false`, `generated imagery = false`로 두는 것을 제안합니다. 이렇게 하면 표지 디자인 변경이 슬라이드 2 이후의 원문 보존 규칙을 건드리지 않습니다.

#### 현재 상태 판단

이 글은 최신 사용자 결정을 소통방에 전달하는 설계 의견입니다. Threads 실제 렌더러 코드 수정, 브라우저 실행, 새 이미지 생성, E2E 검증은 이 글에서 수행했다고 주장하지 않습니다. 실제 저장소 구현 상태가 다르면 저장소 tip을 우선하고, 다음 구현 회차에서 위 기준을 반영해 검증해야 합니다.

#### 요청

Sol은 이후 Threads 구현과 인수인계에서 기존 “블러 표지” 문구를 최신 규칙으로 교체해 주세요. Astra가 `040`을 검토할 때도 이 변경을 전제로 판단해 주세요.

---

## 원본 기록: 190-sol.md

### 190-sol — discovery-only Korean story refresh

Time: 2026-09-19 03:15 KST

Started from current Threads handoff/main, recent commits, and latest sequential ops `189-sol.md`; repo tip wins. User override remains DISCOVERY ONLY.

#### Discovery work
Public Korean-community search was run without login/anti-bot bypass. Search coverage was weaker than the target 40–80 because some prioritized communities were blocked from search access and many returned results were duplicate/low-fit. Approximately 20 visible leads/results were inspected across the query batch; 3 clearly usable, non-duplicate candidates were retained and written to canonical `data/candidates/` as C1 records:

1. `[네이트판] ATM남편 된 것 같은데.. 이혼해야 할까요?`
2. `반반결혼의 최후 (애로부부 캡쳐)`
3. `[네이트판] 아침밥 때문에 결혼식하고 이혼`

The first candidate's TheQoo page visibly includes the individual Nate Pann source URL. The other two have exact individual public TheQoo pages. Candidate files record only observed metrics and whether body/comments were actually read.

#### User-review accommodation
Candidate records now include a concise `내용 확인 요약` so the user can evaluate the premise without opening every file. They do not duplicate full copyrighted posts; exact public source pages remain recorded for full-source review.

#### Scope truth
No screenshots, image downloads, carousel/video rendering, Chrome E2E, provider work, moderation/OCR, rights clearance, publishing or delivery were performed. `publicationAllowed=false`; A1/P1 remain 0. Only 04_REVIEW_PUBLISH may publish.

#### Next
Continue discovery only. Prefer fresh Korean-community leads and exact-source verification. Aim for 40–80 raw / 15–30 retained when accessible coverage allows; do not pad with weak or duplicate material.

---

## 원본 기록: 191-sol.md

### 191 — Sol

Date: 2026-09-19 KST

#### User instruction
The user explicitly superseded the prior discovery-only restriction:
- read all current/latest `Threads/data/candidates`
- load all accumulated materials into the finished program
- process them one-by-one sequentially
- run the workflow every hour

#### What changed
Threads now has a repository-candidate ingest path:
- `repo-candidates.mjs`
- `GET /api/repo-candidates`
- `app/repo-candidate-sync.js`
- program version `0.42.3`

The app automatically merges every candidate Markdown record into the Inbox without overwriting richer user-edited state.

A deterministic queue is now persisted at:
- `data/_system/candidate-program-queue.json`

Progress/blockers are persisted at:
- `data/_system/candidate-program-progress.json`

Stable identity is `candidateKey`, so C/A/P filename-state changes do not cause already handled material to be treated as a new topic.

#### Current candidate truth
After the first provenance pass:
- total 583
- C0 76
- C1 507
- all 583 remain A0/P0
- no A1/P1 was invented

#### First sequential candidate
`2026년 회사별 느낌 NEW ver.`
- exact Blind public post URL recovered
- C0 -> C1
- current file: `260916_C1_A0_P0_2026회사별느낌.md`
- next stage requires manual screenshot/source acquisition under the source policy
- no login/anti-bot/access-control bypass attempted
- progress records it as BLOCKED_NEXT_STAGE until a real permitted/manual source asset exists

Next queue item after refresh:
- `260916_C0_A0_P0_25살연애불가능할까.md`

#### Cover rule synchronized
The latest user decision from ops 190 is now binding in the implementation:
- cover blur removed
- source image/media cover when real visual media exists
- text-only cover for text-only source; no generated imagery
- larger title positioned higher with thin outline
- slide 2+ original source order/content preserved
- generated-image fallback disabled

Updated runtime/source-package/card-factory/source-intake and regression expectations accordingly.

#### Verification
Clean clone on the connected Windows machine:
- `npm run check` PASS on v0.42.3
- repo-candidate sync PASS
- source-package PASS
- card storyboard PASS
- all existing regression tests PASS
- actual ffmpeg vertical render/ffprobe PASS

#### Hourly behavior
Each scheduled cycle should:
1. read current Threads main + handoff + latest T-0008 note;
2. refresh queue from every current `data/candidates` file;
3. re-evaluate previously blocked entries only if their unblock condition changed;
4. process exactly one next candidateKey as far as safely possible;
5. record progress/blocker;
6. never publish or mark P1; 04_REVIEW_PUBLISH remains human-gated;
7. commit meaningful repo changes and append the next Sol ops note.

---

## 원본 기록: 192-sol.md

### 192 — Sol

Date: 2026-09-19 KST

#### New user instruction
The user wants the accumulated implementation work to start being exercised as small real conversion tests, but explicitly says these may be test-only rather than real-use outputs.

The user specifically requires:
- add this work to the hourly schedule;
- progress little by little rather than treating implementation as finished just because code/tests exist;
- make temporary/test folders unmistakable so they cannot be confused with production.

#### Isolation implemented
Created:
- `Threads/03_PRODUCTION/_TEMP_TEST_ONLY_DO_NOT_PUBLISH/README.md`
- `Threads/03_PRODUCTION/_TEMP_TEST_ONLY_DO_NOT_PUBLISH/TEMP_TEST_PROGRESS.json`

All temporary conversion artifacts must remain under this root or a descendant still named `TEMP_TEST_ONLY` / `DO_NOT_PUBLISH`.

These files are never evidence of:
- production approval,
- A1,
- P1,
- rights/privacy clearance,
- live publishing success.

`temporaryTestOnly=true` and `publicationAllowed=false` remain binding.

#### First real conversion prototype
Started a test package for:
- candidate: `260916_C1_A0_P0_2026회사별느낌.md`
- temp path: `03_PRODUCTION/_TEMP_TEST_ONLY_DO_NOT_PUBLISH/260916_2026회사별느낌/`

Created:
- `conversion-manifest.json`
- `cover-temp-test-only.html`

The test cover follows the current rule:
- text-only source → no generated image;
- no blur;
- large title placed high;
- thin readability outline.

Three title suggestions were recorded only as test suggestions, not replacements of the canonical source record.

Full body transformation is intentionally blocked because the whole Blind body and real source screenshots are not acquired. No missing content was inferred.

#### Hourly schedule update
The existing `Threads 후보 순차 처리` hourly automation now has two lanes per run:

A. Existing sequential candidate lane:
- refresh queue;
- process one next candidateKey safely;
- record blockers/progress.

B. TEMP TEST ONLY conversion lane:
- process one small unit from items 9–16;
- write only under `03_PRODUCTION/_TEMP_TEST_ONLY_DO_NOT_PUBLISH/`;
- never let test results upgrade canonical C/A/P state.

The temporary test ladder is:
9. permitted URL→full source/media acquisition wiring
10. UI-chrome exclusion/crop review
11. natural paragraph/scene split
12. title suggestion wiring
13. browser generate→download→reconnect/restore review
14. review-screen connection
15. approved-test-output runtime/PR reflection
16. real publishing/metrics — disabled until separate explicit user approval for a specific live post.

#### Safety
No login/anti-bot/paywall/access-control bypass.
No fabricated source body/assets.
No live publish.
No P1.
Only 04_REVIEW_PUBLISH may ever handle real publication after human review.

---

## 2026-09-20 · 컷메이트 · 컷 편집기 마무리

사용자는 승인한 컷 편집기의 분할선 설명, 긴 원문 스크롤, 제목 테두리 강화와 단어별 색상 강조를 요청했습니다. Canva는 나중으로 미뤘습니다.

- 실제 변경: `app/source-cut-editor.html`과 모델·편집기·ZIP 코드, 기존 원문 입력 화면 연결. 표지/본문 선택·원본 좌표 분할·스크롤/확대·경계 드래그 자동 스크롤, 여러 원문 순서, 제목 테두리 기본 8px(2~20px), 기본색/강조색과 강조 단어.
- 저장/출력: 원본을 포함한 편집 JSON 복원, 전체 PNG ZIP. 출력 너비 1080px, 선택 비율 유지. 이미지당/합계 메모리 제한과 너무 긴 컷의 추가 분할 안내.
- 코드 기준: Threads `bc9cac17833c596168c67449ccbb87969a2f2bdf`, [기존 PR #1](https://github.com/kimjae134679/Threads/pull/1). main 병합하지 않음.
- 검증: 로컬 `npm run check` 47개 suite 및 142개 JS 구문 검사 통과. 실제 스크립트와 native Canvas/DOM 더블로 스크롤·분할·제목·저장/복원·ZIP 생성 확인. 독립 Python ZIP 판독과 실제 HTTP 경로 200 확인. 최종 커밋의 GitHub Actions syntax 작업 통과 (run 35526470782). 최신 main bc255bd의 후보 동기화·WebP·Discovery 기록을 검토 브랜치에 통합하고 충돌을 정리함.
- 미검증: 실제 브라우저 E2E/Windows 설치·다운로드. 미구현: URL 전체 자동 캡처, 기존 04 검수에 결과 되돌려 쓰기. 외부 계정 게시 없음. Canva 미연결.
- 첨부 원문 기반 예시 PNG의 공개 GitHub 업로드는 자동 승인 검토에서 외부 공개 승인 부족으로 차단되어 제외함. 코드는 사용자 요청 범위에서 PR에 반영.
- 다음 작업: 실제 브라우저에서 긴 원문 하나를 편집하고 ZIP 다운로드와 편집 JSON 복원 확인 → 기존 검수 단계 연결.
- 사용법 원본: Threads `docs/SOURCE_CUT_EDITOR.md`. 진행표: `docs/PRODUCTION_PROGRESS.md`.

작업 위치: 관리형 Linux `/workspace/scratch/7dc461d71eca/Threads`. Windows PC에 프로그램을 설치하거나 이동하지 않았으므로 `C:\Program Files\_My\AI` 경로 변경 없음. 새 외부 프로그램 설치 없음. 실행은 저장소 루트 `npm start`, 페이지는 `/app/source-cut-editor.html`; 이동 시 저장소 상대경로를 유지합니다.


---

## 2026-09-20 · 컷메이트 · Windows 앱 설치와 편집 레퍼런스 기록 완료

사용자 추가 요청: URL 캡처 후 열기/HTML 대신 앱, 각 컷 여백과 작성 의견, 탐색 때 중간 문구 초안, 상업 사용 가능한 폰트/실제 굵기/프리셋, 편집 기준을 남겨 향후 자동화에 활용.

- 코드: Threads `592434f25d783d96ecb6ef3c19410a3497f958af`; 설치 검증 문서 `8d35f79e9316e92786b48ee18adb896221c61c52`. 기존 PR #1 갱신, main 미병합.
- 공개 HTTPS 페이지를 격리된 Electron 창으로 캡처하고 긴 원문을 연속 타일로 편집기에 전달. 로그인/접근 제한 우회 없음. 가상/무한 스크롤은 수동 확인 필요.
- 장별 위/아래/좌우 여백, 배경, 위/아래 의견, 실제 폰트/굵기와 스타일 프리셋. Noto Sans KR 파생 400/900, Cut Gothic 800와 OFL 동봉. Regular Cut Gothic 파일 업로드는 자동 승인 검토에서 거절되어 제외했고 재시도하지 않음.
- 원본 좌표/정규화 비율, 분할선, 설정 전후, 제목/문구/폰트, 직접 입력한 이유를 기록. 원본 바이트/해시와 append-only JSONL 및 최종 상태를 PC 전용 폴더에 자동 보관. '기준 예시'는 사용자 선택이며 수정하면 해제. 자동 학습/자동 게시/Canva는 미구현.
- Discovery 지침·후보 파서·원문 입력 전달에 중간 문구 초안 연결. 기존 후보 사실/점수/C-A-P 상태 수정 없음.
- 검증: 로컬 전체 50 suites/153 JS syntax, 마지막 보완 후 history/syntax 검사, 코드 commit GitHub Actions syntax 통과. 실제 Windows 18000px→3장 누락 없는 테스트 캡처, example.com 캡처, 앱 버튼→편집기, 여백/의견/Cut Gothic 800 프리셋/원본해시/로그 저장 통과. x64 ZIP 빌드 및 배포 앱 실행/응답 확인.
- 설치: `C:\\Users\\user\\AppData\\Local\\Programs\\ThreadsCutEditor\\app\\Threads Cut Editor.exe`, 바탕화면 `원문 컷 편집기.lnk`. 소스는 같은 프로그램 폴더의 `source`, 최종 ZIP과 `verification-report.json` 보관. `C:\\Program Files\\_My\\AI`가 없어 관리자 권한이 필요 없는 표준 per-user Programs 경로 사용. 기존 프로젝트 이동 없음. 이동할 때 바로가기 대상/시작 위치 갱신, EXE와 resources 함께 유지.
- ZIP SHA-256: `95F4F65F3D41485C06675627AAC54560A5154462CEBE246B5F5E47C1730B9BD7`.
- 임시 smoke 프로필/테스트 이미지/setup probe 정리. 실제 사용자 데이터와 원격 연결 서비스 유지. 사용자 첨부/편집 로그는 GitHub에 올리지 않음.
- 다음: 사용자가 실제 소재 결과를 검수 → 04 자산 전달. 이후 축적된 기준 예시를 확인하며 자동화 규칙 제안. 현재 기록만으로 사용자 의도를 추측해 자동 학습했다고 표시하지 않음.

로컬 Git 작업 경로: `/workspace/scratch/7dc461d71eca/Threads`. 사용법과 최신 상태 원본은 Threads 저장소의 desktop guide/handoff.
