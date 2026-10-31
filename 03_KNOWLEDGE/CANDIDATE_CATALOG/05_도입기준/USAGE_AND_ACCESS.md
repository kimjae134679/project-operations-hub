# AI Tool / Plugin Usage & Access Model

후보 도구를 `좋은 도구 목록`으로만 두지 않고 **누가 쓰는지, 지금 바로 쓸 수 있는지, 사용자 도움이 필요한지, 얼마나 무거운지**까지 함께 관리합니다.

## 관리 축

각 도구는 가능하면 아래를 따로 기록합니다.

1. `ACCESS` — 지금 바로 사용 가능한가
   - `READY` — 현재 ChatGPT/연결 도구에서 바로 호출 가능
   - `CONNECT` — Plugin/서비스 연결만 하면 사용 가능
   - `INSTALL` — PC/서버에 설치 필요
   - `REFERENCE` — 설치 없이 문서/자료로만 참고
2. `HELP` — 사용자 개입
   - `NONE` — AI가 현재 권한/도구 안에서 처리 가능
   - `ONCE` — OAuth, 로그인, UAC, API key, 2FA, 결제 등 1회 사용자 행동 필요
   - `USER-OPERATED` — 설치 이후에도 사용자가 GUI/앱을 직접 쓰는 비중이 큼
3. `WHO` — 주 사용 주체
   - `AI` — AI 작업 효율을 높이는 도구
   - `USER` — 사용자가 직접 얻는 기능/UX가 중심
   - `BOTH` — 둘 다 직접 이득
4. `ADOPTION` — ACTIVE / PROJECT / CANDIDATE / REFERENCE / RETIRED
5. `INSTALLED` — YES / NO / UNKNOWN / service
6. `BURDEN` — ⚪ NONE / 🟢 LIGHT / 🟡 MEDIUM / 🟠 HEAVY / 🔴 VERY_HEAVY
7. Cloud와 self-host 부담이 다르면 별도로 적습니다.

`ADOPTION` 원본은 `01_CONTROL/TOOLS.md`, 실제 설치 버전/경로 원본은 `01_CONTROL/AI_INSTALLATIONS.md`입니다.

---

## 현재 ChatGPT 환경에서 바로 활용 가능한 계열

아래는 현재 대화 환경에서 도구 인터페이스가 제공되는 계열입니다. 개별 계정 인증 상태가 필요한 작업은 실제 호출 때 확인합니다.

| 도구 | ACCESS | HELP | WHO | 부담 | 용도 |
|---|---|---|---|---|---|
| GitHub | READY | NONE~ONCE | BOTH | ⚪ | 저장소/파일/PR/이슈/Actions |
| ChatGPT Files / Library | READY | NONE | BOTH | ⚪ | 첨부·라이브러리 파일 검색/읽기 |
| Gmail | READY | NONE~ONCE | BOTH | ⚪ | 메일 검색/읽기/작성·변경 |
| Google Calendar | READY | NONE~ONCE | BOTH | ⚪ | 일정 조회/생성/수정 |
| Google Contacts | READY | NONE~ONCE | BOTH | ⚪ | 연락처 조회 |
| Google Drive | READY | NONE~ONCE | BOTH | ⚪ | Drive/Docs/Sheets/Slides 작업 |
| Notion | READY | NONE~ONCE | BOTH | ⚪ | 문서/DB/업무 흐름 |
| Supabase | READY | NONE~ONCE | AI/BOTH | ⚪ | DB/프로젝트 관리 |
| Remote Desktop Commander | READY when device online | NONE~ONCE | AI | 🟢 | 사용자 PC 파일/터미널/앱 작업 |

`READY`는 PC에 별도 프로그램을 새로 설치할 필요가 없다는 뜻이며, 외부 서비스 로그인·권한이 전혀 필요 없다는 뜻은 아닙니다.

---

## ChatGPT Plugin Directory에서 확인한 후보 — 2026-09-16

설치/연결을 임의로 수행하지 않고 후보로만 기록합니다.

### 개발/업무
- `GitHub` — PR/이슈/CI/publish 흐름
- `Notion` — 문서/워크플로
- `Figma` — 디자인→코드 흐름
- `Codex Security` — 코드베이스 보안 스캔 후보
- `Superpowers` — 계획/개발/디버깅용 Agent Skill 계열
- `OpenAI Library` — Codex에서 ChatGPT Library 활용
- `Airtable` — 구조화 데이터 작업

### 콘텐츠/영상
- `Canva` — 디자인 생성·검토·편집
- `HeyGen` — AI 영상/아바타
- `HyperFrames by HeyGen` — HTML 기반 영상 렌더
- `Remotion` — 코드 기반 영상 생성

### 금융/리서치
- `Longbridge` — 주가/금융 데이터
- `Quartr` — 기업 리서치 데이터
- `Financial Datasets` — 주식시장 데이터
- `Stocktwits` — 개인 투자자 커뮤니티 데이터
- `Bigdata.com` — 금융 리서치/뉴스/데이터
- `Consensus` — 과학 연구 검색
- `Sider Scholar` — 논문 검색/저장/대화

이 목록은 `설치 추천 순위`가 아니라 **ChatGPT 안에서 연결 후보로 존재하는 것**을 확인한 기록입니다. 실제 설치/연결은 구체적인 프로젝트 필요가 생겼을 때 결정합니다.

---

## AI가 사용자 도움 없이 처리하기 쉬운 계열

- 이미 연결된 GitHub/Files/Notion 등 읽기·정리 작업
- npm/pip/단일 CLI 설치 중 관리자 권한·로그인·비밀값이 필요 없는 것
- MarkItDown 같은 경량 CLI/라이브러리
- Vercel Skills 등 Skill 검색/프로젝트 격리 테스트
- GitHub 공식 문서/README 조사와 후보 분류

단, 실제 PC 설치는 Remote Desktop 연결 상태와 권한을 먼저 확인합니다.

## 사용자 1회 도움이 필요한 대표 상황

- 브라우저 OAuth 로그인
- UAC/관리자 권한 승인
- API key 발급/결제 등록
- 2FA/기기 인증
- 계정/서비스 약관 동의
- Cloudflare 등 외부 플랫폼의 수동 인증

이런 경우 AI가 가능한 부분까지 먼저 진행하고 필요한 순간에만 사용자 행동을 요청합니다.

## 사용자 직접 사용 비중이 큰 도구

- ComfyUI — 설치·모델 관리 후 노드 GUI 사용 비중이 큼
- Blender — AI 자동화도 가능하지만 결과 확인/편집은 사용자 체감이 큼
- Canva/Figma — Plugin 자동화가 가능해도 디자인 검수·수동 편집 가치가 큼
- Open WebUI/LobeHub/Dify/Langflow — AI 플랫폼 자체 UI를 사용자도 직접 사용하는 유형

---

## 무거운 도구는 별도 취급

가벼운 Plugin/Skill과 한 묶음으로 `깔아두자`고 처리하지 않습니다.

- 🔴 `ComfyUI` — 모델/LoRA/VAE/PyTorch/CUDA/VRAM/디스크 부담
- 🔴 `vLLM` — 고성능 LLM serving, GPU/CUDA/모델 부담
- 🔴 `RAGFlow self-host` — RAM/디스크/Docker/다중 서비스 부담
- 🔴 대형 로컬 LLM/DeepSeek 계열 — 선택 모델에 따라 수십 GB 이상 가능
- 🟠 `Ollama` — runtime은 쉬워도 모델 파일과 VRAM이 본체
- 🟠 `Dify/Firecrawl self-host` — Cloud/SDK보다 직접 운영 부담이 큼

세부 무게 기준은 `INSTALLATION_BURDEN.md`를 따릅니다.

---

## 지속 정리 규칙

새 도구를 발견하면:

```text
공식 원본 확인
→ 기존 후보와 중복 확인
→ ACCESS 분류
→ HELP 분류
→ WHO 분류
→ ADOPTION / INSTALLED 분리
→ BURDEN 분류
→ 비용·라이선스·보안 확인
→ 사용자용 한글 요약 동기화
```

도구가 새로 발견되지 않았거나 실제로 달라진 정보가 없으면 문서를 억지로 수정하지 않습니다.
