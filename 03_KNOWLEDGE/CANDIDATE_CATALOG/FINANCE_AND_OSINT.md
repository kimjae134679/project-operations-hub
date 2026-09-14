# Finance & OSINT

확인 기준: 공식 GitHub README/조직 페이지, 2026-09-15.

---

## 1. TradingAgents — Tauric Research

- 공식: https://github.com/TauricResearch/TradingAgents
- 종류: **multi-agent LLM financial trading research framework**
- 상태: `PROJECT-FIT / HIGH-FIT`
- 라이선스: Apache-2.0

### 정확한 정체
여러 역할의 LLM Agent가 실제 trading firm처럼 분석을 나눠 수행하고 토론해 결론을 내리는 연구 프레임워크입니다.

대표 역할:
- fundamental analyst
- sentiment analyst
- technical/market analyst
- researcher/debate agents
- trader
- risk management team
- portfolio/decision layer

### 현재 기술 구조
공식 README 기준 LangGraph 기반이며 OpenAI, Google, Anthropic, xAI, DeepSeek, Qwen, GLM, MiniMax, OpenRouter, Ollama, Azure OpenAI 등 여러 provider를 지원합니다.

2026-08 v0.4.0에서는 point-in-time/look-ahead 관련 수정, decision signal 개선, checkpoint resume, price grounding 등이 들어간 것으로 공식 changelog가 설명합니다.

### 우리한테 의미
Investment-Lab에 바로 “자동매매 엔진”으로 통째로 붙이기보다 **다중 Agent 리서치 구조와 역할 분담을 참고하거나 별도 연구 모듈로 시험**하는 게 먼저입니다.

### 특히 참고할 부분
- 분석 역할별 prompt/context 분리
- analyst 간 토론 구조
- risk manager와 trader를 분리
- checkpoint/resume
- 데이터 snapshot grounding
- look-ahead/PIT 오류를 명시적으로 다루는 방식

### 주의
공식 README도 backtest/수익 재현을 보장하지 않고 연구 scaffold라고 명시합니다.

우리 Investment-Lab에서 중요한 원칙:
- broker write와 research를 분리
- PIT 데이터 유지
- 실주문 전 read-only/diagnostic 단계 유지
- multi-agent 합의가 곧 신뢰 가능한 매매 signal은 아님

### 추천 검증
실주문과 분리된 sandbox에서 동일 날짜/동일 ticker를 여러 번 돌려:
- company identity
- 가격 근거
- 데이터 시점
- 최종 판단 편차
- API 비용
을 측정한 뒤 참고 가치 판단.

---

## 2. Fincept Terminal

- 공식: https://github.com/Fincept-Corporation/FinceptTerminal
- 종류: **desktop financial intelligence / research terminal**
- 상태: `PROJECT-FIT`
- 오픈 에디션 라이선스: AGPL-3.0

### 정확한 정체
“Bloomberg Terminal 무료판”이라는 커뮤니티식 표현보다는 **financial research/market analytics/economic data/AI automation을 한 데 묶은 데스크톱 터미널**이라고 보는 게 정확합니다.

현재 공식 README는 native C++20 + Qt6 UI, embedded Python 3.11 analytics, non-Electron desktop terminal을 강조합니다.

### 기능 범주
- market analytics
- investment research
- economic data
- data connectivity
- AI/agent-assisted analysis
- desktop-native research workspace

### 에디션/비용 주의
오픈 에디션은 AGPL-3.0이고 개인/학습/학술 사용을 주 대상으로 설명합니다.
기업용/상업 라이선스는 별도입니다.
또한 “무료 앱”이어도 외부 market data나 LLM API 비용은 사용자가 부담할 수 있습니다.

### 우리한테 의미
Market Radar / Investment-Lab에서 다음을 참고할 가치가 큽니다.
- 많은 금융 정보를 한 화면에 배치하는 UX
- chart/research/news/economic data를 어떻게 구획하는지
- desktop terminal형 정보 밀도
- native desktop + Python analytics 결합 방식

### 주의
AGPL code를 우리 프로젝트에 직접 복사/결합할 경우 라이선스 영향 검토가 필요합니다. 우선은 **기능/UX/아키텍처 참고**로 보는 편이 안전합니다.

---

## 3. Flowsint — Reconurge

- 공식: https://github.com/reconurge/flowsint
- 종류: **graph-based OSINT / investigation platform**
- 상태: `PROJECT-FIT / REFERENCE`

### 정체
공개정보 조사, cybersecurity investigation 등을 **entity + relationship graph**로 시각화하고 enricher를 실행해 관계를 확장하는 플랫폼입니다.

### 사용 흐름
공식 quickstart 기준:
1. Docker/Docker Compose/Make/Git 준비
2. repo clone
3. `make prod`
4. investigation 생성
5. domain/person/identifier 등 entity 추가
6. right-click enricher 실행
7. 결과 entity/relationship를 graph에서 추적

일부 enricher는 별도 API key가 필요합니다.

### 우리한테 의미
단순 웹 scraping 도구라기보다 **“정보를 연결해서 보는 조사 UX”**가 핵심입니다.

활용 참고:
- 회사/서비스/계정/도메인 관계 정리
- 공개 출처 조사 workflow
- 프로젝트 조사 내용을 graph로 시각화

### 주의
OSINT라고 해서 모든 데이터 수집이 허용되는 것은 아닙니다.
- 공개정보 중심
- 로그인 우회/비공개정보 수집 금지
- 개인정보/민감정보 최소화
- 각 enricher API의 약관/비용 확인

현재 우리 프로젝트에는 필수 도구라기보다 조사 UX/관계형 분석 아이디어 참고 우선입니다.
