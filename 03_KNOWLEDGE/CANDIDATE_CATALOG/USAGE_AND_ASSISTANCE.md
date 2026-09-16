# AI Tool Usage & Assistance Matrix

AI/Plugin/Skill/MCP/CLI/Agent 후보를 단순히 `좋다/나쁘다`나 `설치됨/안 됨`으로만 관리하지 않고, **누가 실제로 쓰는지, AI가 혼자 끝까지 다룰 수 있는지, 사용자가 얼마나 개입해야 하는지, 추가 비용이 필요한지**를 함께 기록하는 내부 기준입니다.

사용자용 요약은 `000_사용자용/06_AI_도구_플러그인_사용구분.md`에 동기화합니다.

## 분리해서 보는 축

- `ACCESS` — 지금 이 환경에서 바로 호출 가능한가
  - `READY` / `CONNECT` / `INSTALL` / `REFERENCE`
- `OPERATOR` — 주 사용 주체
  - `AI` / `USER` / `BOTH`
- `AUTONOMY` — AI가 기능을 실제로 얼마나 독립적으로 운용 가능한가
  - `FULL` — 필요한 입력만 있으면 AI가 기능을 처음부터 끝까지 처리 가능
  - `AFTER_SETUP` — 로그인/OAuth/UAC/API key 같은 1회 준비 후에는 AI가 대부분 독립 운용 가능
  - `PARTIAL` — AI가 상당 부분 처리하지만 GUI 선택·시각 판단·수동 단계가 자주 필요
  - `USER_DRIVEN` — 핵심 사용은 사용자가 직접 해야 함
  - `UNKNOWN` — 아직 실제 운용성 미검증
- `ASSISTANCE` — 사용자 개입 빈도
  - `NONE` / `ONCE` / `FREQUENT`
- `BLOCKER` — 필요한 경우
  - `LOGIN` / `OAUTH` / `UAC` / `API_KEY` / `2FA` / `PAYMENT` / `CAPTCHA` / `GUI_CHECK` / `GPU`
- `COST` — 추가 비용 정책
  - `FREE_OSS` — 완전 무료/오픈소스 자체 사용
  - `FREE_TIER` — 기간 제한 없는 무료 플랜이 존재하나 사용량 제한 가능
  - `INCLUDED` — 사용자가 이미 쓰는 요금제/서비스에 포함되어 추가 결제가 없음
  - `TRIAL` — 기간/크레딧 제한 무료체험
  - `PAID` — 추가 결제 필요
  - `UNKNOWN` — 공식 가격/조건 미확인
- `INSTALLED` — 실제 PC 설치 여부
  - `YES` / `NO` / `UNKNOWN` / `SERVICE`
- `ADOPTION` — 실제 채택 여부
  - `ACTIVE` / `PROJECT` / `CANDIDATE` / `REFERENCE` / `RETIRED`
- `BURDEN` — 설치·운영 부담
  - `⚪ NONE` / `🟢 LIGHT` / `🟡 MEDIUM` / `🟠 HEAVY` / `🔴 VERY_HEAVY`

`ACCESS`, `AUTONOMY`, `INSTALLED`, `ADOPTION`, `COST`는 서로 다른 의미입니다. 예를 들어 Plugin Directory에 보이는 도구는 `CONNECT`일 수 있지만 아직 연결되지 않았고, 연결해도 사용자가 매번 GUI를 만져야 하면 `AUTONOMY=PARTIAL/USER_DRIVEN`입니다.

## 사용자 비용 선호 정책

사용자 기본 선호는 다음과 같습니다.

```text
FREE_OSS
→ FREE_TIER
→ INCLUDED
→ TRIAL
→ PAID
```

- `PAID`는 기본 후보/추천/설치 대상에서 제외합니다. 사용자가 명시적으로 요청한 경우만 검토합니다.
- `TRIAL`도 기본적으로 피합니다. 다른 무료 대안이 있으면 무료 대안을 우선합니다.
- 카드 등록, 체험 종료 후 자동결제, 크레딧 소진 후 과금 가능성이 있으면 `무료`로 취급하지 않습니다.
- 가격이 확인되지 않으면 `UNKNOWN`으로 두고 추천 우선순위를 올리지 않습니다.
- 오픈소스 프로그램 자체가 무료여도 외부 모델/API/클라우드 비용이 필요하면 `FREE_OSS + external cost possible`처럼 분리해 적습니다.

## 현재 세션에서 바로 활용 가능한 연결 도구

현재 ChatGPT에서 연결 도구로 노출되어 실제 작업에 사용할 수 있는 축입니다. 계정 권한이나 서비스 상태가 바뀌면 다시 확인합니다.

| Tool | ACCESS | OPERATOR | AUTONOMY | ASSISTANCE | COST | BURDEN | 메모 |
|---|---|---|---|---|---|---|---|
| GitHub | READY | BOTH | FULL~AFTER_SETUP | NONE~ONCE | 기존 계정/서비스 | ⚪ | 노출된 repo/PR/Issue/Actions 작업은 AI가 직접 처리 가능. 관리자 전용 기능은 별도 |
| Gmail | READY | BOTH | AFTER_SETUP | ONCE when auth needed | 기존 계정/서비스 | ⚪ | 연결 후 검색/읽기/작성 가능 |
| Google Calendar | READY | BOTH | AFTER_SETUP | ONCE when auth needed | 기존 계정/서비스 | ⚪ | 연결 후 일정 조회/변경 가능 |
| Google Contacts | READY | AI | AFTER_SETUP | ONCE when auth needed | 기존 계정/서비스 | ⚪ | 수신자/참석자 확인 |
| Google Drive | READY | BOTH | AFTER_SETUP | ONCE when auth needed | 기존 계정/서비스 | ⚪ | 연결 후 Drive/Docs/Sheets/Slides 작업 가능 |
| Notion | READY | BOTH | AFTER_SETUP | ONCE when auth needed | 기존 계정/서비스 | ⚪ | 연결 후 문서/DB/워크플로 작업 가능 |
| Supabase | READY | AI | AFTER_SETUP | ONCE when permission needed | 기존 계정/서비스 | ⚪ | 연결 범위 내 DB/프로젝트 작업 가능 |
| Remote Desktop Commander | READY when PC online | AI | PARTIAL~AFTER_SETUP | ONCE/FREQUENT for blockers | 기존 설치 | 🟢 | 일반 파일/명령은 AI 처리 가능. UAC/CAPTCHA/특정 GUI는 사용자 필요 가능 |
| ChatGPT Files / Library | READY | BOTH | FULL | NONE | 현재 ChatGPT 기능 | ⚪ | 파일/라이브러리 검색·읽기 |

## Plugin Directory에서 확인된 연결 후보

아래는 2026-09-16 Plugin Directory 검색에서 확인한 후보입니다. **검색 결과에 보인 것과 현재 설치/연결 완료, 무료 여부는 다릅니다.** 공식 가격을 확인하지 않은 항목은 `UNKNOWN`으로 둡니다.

| Tool | ACCESS | OPERATOR | AUTONOMY 예상 | 사용자 도움 | COST | 부담 | 적용 후보 |
|---|---|---|---|---|---|---|---|
| Figma | CONNECT | BOTH | PARTIAL | OAuth/검수 | UNKNOWN | ⚪ | 디자인→코드 |
| Codex Security | CONNECT | AI | AFTER_SETUP | 연결/권한 1회 가능 | UNKNOWN | ⚪ | 코드 보안 점검 |
| Superpowers | CONNECT | AI | AFTER_SETUP | 연결 1회 가능 | UNKNOWN | ⚪ | 계획·개발·디버깅 |
| OpenAI Library | CONNECT | AI | AFTER_SETUP | 연결 1회 가능 | UNKNOWN/현재 서비스 조건 | ⚪ | Codex에서 Library 활용 |
| Airtable | CONNECT | BOTH | AFTER_SETUP | OAuth 가능 | UNKNOWN | ⚪ | 구조화 데이터 |
| Canva | CONNECT | BOTH | PARTIAL | OAuth/최종 검수 | UNKNOWN | ⚪ | 디자인 제작/편집 |
| HeyGen | CONNECT | BOTH | PARTIAL | 로그인/요금 조건 가능 | UNKNOWN | ⚪ local / cloud service | 영상·아바타 |
| HyperFrames by HeyGen | CONNECT | AI | AFTER_SETUP | 연결 가능 | UNKNOWN | ⚪ local install 없음 | HTML→영상 |
| Remotion | CONNECT | AI | AFTER_SETUP | 연결 가능 | UNKNOWN | ⚪~🟢 | 코드 기반 영상 |
| Longbridge | CONNECT | BOTH | AFTER_SETUP | 계정 연결 가능 | UNKNOWN | ⚪ | 주가/금융 데이터 |
| Quartr | CONNECT | BOTH | AFTER_SETUP | 연결 가능 | UNKNOWN | ⚪ | 기업 리서치 |
| Financial Datasets | CONNECT | AI | AFTER_SETUP | 계정/API 조건 확인 | UNKNOWN | ⚪ | 금융 데이터 |
| Stocktwits | CONNECT | BOTH | AFTER_SETUP | 연결 가능 | UNKNOWN | ⚪ | 투자자 커뮤니티 |
| Bigdata.com | CONNECT | AI | AFTER_SETUP | 계정 조건 확인 | UNKNOWN | ⚪ | 금융 뉴스/리서치 |
| Consensus | CONNECT | BOTH | AFTER_SETUP | 연결 가능 | UNKNOWN | ⚪ | 과학 논문 검색 |
| Sider Scholar | CONNECT | BOTH | AFTER_SETUP | 연결 가능 | UNKNOWN | ⚪ | 논문 검색/저장 |

**COST=UNKNOWN인 후보는 무료 여부를 확인하기 전에는 자동으로 설치/연결 후보 상위에 올리지 않습니다.**

## AI가 사용자 도움 없이 처리하기 쉬운 계열

다음 조건이면 원격 PC가 온라인일 때 AI가 설치·검증·운용까지 진행하기 쉽습니다.

- 단일 npm/pip/uv/winget/CLI 설치
- 프로젝트 내부에만 추가하는 Skill/스크립트
- MarkItDown 같은 경량 변환 도구
- Vercel Skills 같은 Skill 관리 CLI
- 관리자 권한·로그인·유료결제·2FA가 필요 없는 GitHub 기반 도구
- CLI/API/MCP 형태로 기능이 노출되어 GUI 수동 조작이 필요 없는 도구

반대로 설치는 쉬워도 **매번 노드/화면을 직접 만지고 결과를 골라야 하는 GUI 중심 도구는 `FULL`로 분류하지 않습니다.**

## 사용자가 직접 만지는 가치가 큰 계열

- `ComfyUI` — 노드 그래프와 결과 비교를 직접 보는 비중이 큼
- `Blender` — 자동화 가능하지만 최종 형태/비율/재질 검수는 사용자 체감이 중요
- `Canva`, `Figma` — AI가 작업해도 최종 디자인 선택은 사용자 쪽 가치가 큼
- `Dify`, `Langflow`, `Open WebUI`, `LobeHub` — 플랫폼 UI 자체를 사용자가 직접 운영할 수 있음

## 무거운 도구 별도 취급

`Plugin/Skill`과 같은 목록에서 단순히 “설치 후보”로 놓지 않습니다.

- 🔴 `ComfyUI` — 모델/LoRA/VAE/PyTorch/CUDA/VRAM/디스크가 본체
- 🔴 `vLLM` — GPU/CUDA/대형 모델 기반 serving
- 🔴 `RAGFlow self-host` — 높은 RAM/디스크 + Docker 다중 서비스
- 🔴 대형 로컬 LLM/DeepSeek 계열 — 모델별 용량·VRAM 재평가
- 🟠 `Ollama` — 앱은 단순하지만 모델 파일이 무거움
- 🟠 `Dify self-host`, `Firecrawl self-host` — 직접 운영 시 서비스 계층 증가

## 갱신 규칙

새 후보를 발견하면 가능하면 다음 순서로 기록합니다.

```text
정체 확인
→ ACCESS
→ OPERATOR
→ AUTONOMY
→ ASSISTANCE / BLOCKER
→ COST
→ INSTALLED
→ ADOPTION
→ BURDEN
→ Cloud / self-host 차이
→ 권한·보안·라이선스
→ 사용자용 06 문서에 필요한 만큼만 한글 요약
```

가격은 가능한 경우 공식 pricing/README/license를 기준으로 확인합니다. SNS의 `무료`, `free`, `$0` 문구만으로 무료라고 확정하지 않습니다.

같은 도구가 여러 SNS 목록에 반복 등장하면 새 항목을 복제하지 않고 기존 항목을 갱신합니다.

마지막 정리: **2026-09-16**
