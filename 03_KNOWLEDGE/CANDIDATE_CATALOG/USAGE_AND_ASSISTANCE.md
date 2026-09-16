# AI Tool Usage & Assistance Matrix

AI/Plugin/Skill/MCP/CLI/Agent 후보를 단순히 `좋다/나쁘다`나 `설치됨/안 됨`으로만 관리하지 않고, **누가 실제로 쓰는지와 사용자가 얼마나 개입해야 하는지**를 함께 기록하는 내부 기준입니다.

사용자용 요약은 `000_사용자용/06_AI_도구_플러그인_사용구분.md`에 동기화합니다.

## 분리해서 보는 축

- `ACCESS` — 지금 이 환경에서 바로 호출 가능한가
  - `READY` / `CONNECT` / `INSTALL` / `REFERENCE`
- `OPERATOR` — 주 사용 주체
  - `AI` / `USER` / `BOTH`
- `ASSISTANCE` — 사용자 개입
  - `NONE` / `ONCE` / `FREQUENT`
- `BLOCKER` — 필요한 경우
  - `LOGIN` / `OAUTH` / `UAC` / `API_KEY` / `2FA` / `PAYMENT` / `GUI_CHECK` / `GPU`
- `INSTALLED` — 실제 PC 설치 여부
  - `YES` / `NO` / `UNKNOWN` / `SERVICE`
- `ADOPTION` — 실제 채택 여부
  - `ACTIVE` / `PROJECT` / `CANDIDATE` / `REFERENCE` / `RETIRED`
- `BURDEN` — 설치·운영 부담
  - `⚪ NONE` / `🟢 LIGHT` / `🟡 MEDIUM` / `🟠 HEAVY` / `🔴 VERY_HEAVY`

`ACCESS`, `INSTALLED`, `ADOPTION`은 같은 의미가 아닙니다. 예를 들어 Plugin Directory에 보이는 도구는 `CONNECT`일 수 있지만 아직 설치/연결되지 않았고, PC에 설치된 CLI도 실제 프로젝트에서 채택되지 않았을 수 있습니다.

## 현재 세션에서 바로 활용 가능한 연결 도구

현재 ChatGPT에서 연결 도구로 노출되어 실제 작업에 사용할 수 있는 축입니다. 계정 권한이 만료되거나 서비스 연결 상태가 바뀌면 다시 확인합니다.

| Tool | ACCESS | OPERATOR | ASSISTANCE | BURDEN | 메모 |
|---|---|---|---|---|---|
| GitHub | READY | BOTH | NONE~ONCE | ⚪ | 저장소/PR/Issue/Actions |
| Gmail | READY | BOTH | ONCE when auth needed | ⚪ | 메일 검색/읽기/작성 |
| Google Calendar | READY | BOTH | ONCE when auth needed | ⚪ | 일정 조회/변경 |
| Google Contacts | READY | AI | ONCE when auth needed | ⚪ | 수신자/참석자 확인 |
| Google Drive | READY | BOTH | ONCE when auth needed | ⚪ | Drive/Docs/Sheets/Slides |
| Notion | READY | BOTH | ONCE when auth needed | ⚪ | 문서/DB/워크플로 |
| Supabase | READY | AI | ONCE when permission needed | ⚪ | DB/프로젝트 관리 |
| Remote Desktop Commander | READY when PC online | AI | ONCE for PC/UAC blockers | 🟢 | 실제 PC 파일/명령/앱 작업 |
| ChatGPT Files / Library | READY | BOTH | NONE | ⚪ | 파일/라이브러리 검색·읽기 |

## Plugin Directory에서 확인된 연결 후보

아래는 2026-09-16 Plugin Directory 검색에서 확인한 후보입니다. **검색 결과에 보인 것과 현재 설치/연결 완료는 다릅니다.** 필요할 때만 연결합니다.

| Tool | ACCESS | OPERATOR | 예상 도움 | 부담 | 적용 후보 |
|---|---|---|---|---|---|
| Figma | CONNECT | BOTH | OAuth 1회 가능 | ⚪ | 디자인→코드 |
| Codex Security | CONNECT | AI | 연결/권한 1회 가능 | ⚪ | 코드 보안 점검 |
| Superpowers | CONNECT | AI | 연결 1회 가능 | ⚪ | 계획·개발·디버깅 |
| OpenAI Library | CONNECT | AI | 연결 1회 가능 | ⚪ | Codex에서 Library 활용 |
| Airtable | CONNECT | BOTH | OAuth 1회 가능 | ⚪ | 구조화 데이터 |
| Canva | CONNECT | BOTH | OAuth 1회 가능 | ⚪ | 디자인 제작/편집 |
| HeyGen | CONNECT | BOTH | 로그인/요금제 가능 | ⚪ local / cloud cost | 영상·아바타 |
| HyperFrames by HeyGen | CONNECT | AI | 연결 1회 가능 | ⚪ local install 없음 | HTML→영상 |
| Remotion | CONNECT | AI | 연결 1회 가능 | ⚪~🟢 | 코드 기반 영상 |
| Longbridge | CONNECT | BOTH | 계정 연결 가능 | ⚪ | 주가/금융 데이터 |
| Quartr | CONNECT | BOTH | 연결 1회 가능 | ⚪ | 기업 리서치 |
| Financial Datasets | CONNECT | AI | 계정/API 조건 확인 | ⚪ | 금융 데이터 |
| Stocktwits | CONNECT | BOTH | 연결 1회 가능 | ⚪ | 투자자 커뮤니티 |
| Bigdata.com | CONNECT | AI | 계정 조건 확인 | ⚪ | 금융 뉴스/리서치 |
| Consensus | CONNECT | BOTH | 연결 1회 가능 | ⚪ | 과학 논문 검색 |
| Sider Scholar | CONNECT | BOTH | 연결 1회 가능 | ⚪ | 논문 검색/저장 |

## AI가 사용자 도움 없이 처리하기 쉬운 계열

다음 조건이면 원격 PC가 온라인일 때 AI가 설치·검증까지 진행하기 쉽습니다.

- 단일 npm/pip/uv/winget/CLI 설치
- 프로젝트 내부에만 추가하는 Skill/스크립트
- MarkItDown 같은 경량 변환 도구
- Vercel Skills 같은 Skill 관리 CLI
- 관리자 권한·로그인·유료결제·2FA가 필요 없는 GitHub 기반 도구

막히는 지점이 `UAC`, `OAuth`, `2FA`, `API key 발급`, `결제`, `CAPTCHA`라면 그 순간만 사용자 도움을 요청합니다.

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
→ ASSISTANCE / BLOCKER
→ INSTALLED
→ ADOPTION
→ BURDEN
→ Cloud / self-host 차이
→ 비용·권한·보안·라이선스
→ 사용자용 06 문서에 필요한 만큼만 한글 요약
```

같은 도구가 여러 SNS 목록에 반복 등장하면 새 항목을 복제하지 않고 기존 항목을 갱신합니다.

마지막 정리: **2026-09-16**
