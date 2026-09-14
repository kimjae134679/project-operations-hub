# Candidate Catalog — 2026-09-15 조사본

사용자가 2026-09-15에 전달한 GitHub 프로젝트·웹사이트·SNS 링크를 **실제 원본 README/사이트 기준으로 다시 확인해 정리한 후보 카탈로그**입니다.

이 폴더의 항목은 전부 기본적으로 `CANDIDATE`입니다. 유명하거나 Star가 많다는 이유만으로 설치·채택·신뢰 상태로 올리지 않습니다.

## 분류

| 문서 | 내용 |
|---|---|
| [AI_AGENTS_AND_DEV.md](AI_AGENTS_AND_DEV.md) | OpenHands, Hermes Agent, CrewAI, Aider, LangGraph, browser-use, awesome-mcp-servers, Task Master, LibreChat, Anthropic Agent Skills |
| [AUTOMATION_AND_INTEGRATIONS.md](AUTOMATION_AND_INTEGRATIONS.md) | n8n, Agentic Inbox, Nango |
| [CONTENT_MEDIA.md](CONTENT_MEDIA.md) | MoneyPrinterTurbo, HyperFrames, VoxCPM |
| [FINANCE_AND_OSINT.md](FINANCE_AND_OSINT.md) | TradingAgents, Fincept Terminal, Flowsint |
| [UI_DESIGN_REFERENCES.md](UI_DESIGN_REFERENCES.md) | SceneAI, dashboard 디자인 참고자료, AGENTS/Skills 경량화 참고 |
| [SOURCE_BOOKMARKS.md](SOURCE_BOOKMARKS.md) | 사용자가 준 원문 링크와 확인 상태 |

## 상태 표기

- `VERIFY-FIRST` — 관심도는 높지만 설치/연결 전 별도 테스트가 필요
- `HIGH-FIT` — 현재 프로젝트와 직접 연결점이 큼
- `PROJECT-FIT` — 특정 프로젝트에서만 가치가 큼
- `REFERENCE` — 설치 도구라기보다 참고자료/아이디어 원천
- `SOURCE-UNVERIFIED` — 링크는 보관했지만 현재 조사 환경에서 본문을 독립적으로 읽지 못함

## 이번 조사에서 바로잡은 홍보문구

- **n8n**은 흔히 “오픈소스 Zapier”라고 불리지만 공식 설명은 **fair-code workflow automation**입니다. self-host는 가능하지만 일반적인 OSI 오픈소스와 동일하다고 적지 않습니다.
- **Fincept Terminal**은 “무료 Bloomberg”라고 부르기보다 **오픈 에디션 금융 리서치 터미널**로 보는 게 정확합니다. 오픈 에디션은 AGPL-3.0이고 데이터/LLM 비용은 별도일 수 있습니다.
- **HyperFrames**는 영상 생성 모델이 아니라 **HTML/CSS/애니메이션을 결정론적으로 MP4로 렌더링하는 프레임워크**입니다.
- **TradingAgents**는 자동수익 보장 시스템이 아니라 **다중 LLM 기반 금융 분석/트레이딩 연구 프레임워크**입니다.
- **MoneyPrinterTurbo**가 Pexels/Pixabay/Coverr 소재를 연결하더라도 “모든 결과물이 무조건 저작권 문제 없음”으로 간주하지 않습니다. 각 제공처·소재·음원의 라이선스를 최종 사용 전에 확인해야 합니다.
- SNS 게시물의 Star 수, 출시 후 성장 속도, “완전 자동”, “무료” 같은 문구는 스냅샷/홍보 표현일 수 있어 **원본 저장소의 현재 기능·라이선스·설치 요구사항을 우선**합니다.

## 현재 우선 검토 순서

### 콘텐츠 자동화 쪽
1. `MoneyPrinterTurbo` — 주제→스크립트→소재→자막→음악→영상, 현재 README에는 TikTok/Instagram/YouTube Shorts 게시 연동도 명시됨
2. `n8n` — 수집·분류·생성·검수·업로드를 연결하는 오케스트레이션 후보
3. `HyperFrames` — 템플릿 기반 세로/가로 영상의 안정적 렌더링 후보
4. `VoxCPM` — 로컬 TTS/보이스 디자인/보이스 클로닝 후보
5. `browser-use` — 웹 작업 자동화 후보. 로그인/약관/사이트 안정성 때문에 별도 격리 테스트 필수

### 개발환경 쪽
1. `Anthropic Agent Skills` — Skill 구조/트리거 설계 참고용
2. `OpenHands` — 여러 코딩 Agent를 한 화면/백엔드에서 운영하는 control-center 후보
3. `Hermes Agent` — 지속 기억·Skill 생성/개선·스케줄 자동화가 강한 독립 Agent 후보
4. `Aider` — 기존 저장소를 빠르게 수정하는 터미널 pair programmer 후보
5. `CrewAI` / `LangGraph` — 직접 멀티 Agent 시스템을 만들 때 검토
6. `Task Master` — AI 작업을 task/dependency 구조로 관리할 때 검토

### 주식/리서치 쪽
1. `TradingAgents` — Investment-Lab 연구 모듈 참고
2. `Fincept Terminal` — Market Radar/Investment-Lab UI·리서치 기능 참고
3. `Flowsint` — 공개정보 관계 그래프/조사 UX 참고

마지막 확인일: **2026-09-15**
