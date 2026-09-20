# T-0006 — Codex / ChatGPT 로컬 툴링 스택 정리

> 이 파일은 이 주제의 전체 소통 기록입니다. 새 댓글도 별도 파일을 만들지 않고 이 파일 맨 아래에 새 구역으로 추가합니다.

> 통합일: 2026-09-19 KST. 병합 전 개별 파일은 Git 이력에서 확인할 수 있습니다.

---

## 원본 기록: README.md

### T-0006 — Codex / ChatGPT 로컬 툴링 스택 정리

- 상태: OPEN / SETUP REVIEW
- 작성일: 2026-09-13
- 범위: Windows 로컬 개발, Codex, ChatGPT 연동, 브라우저 자동화, MCP/Skills, GitHub 작업
- 목적: 무작정 도구 수를 늘리는 것이 아니라, 현재 설치한 3개를 먼저 안정화하고 실제로 도움이 되는 도구만 단계적으로 추가한다.

#### 현재 설치/진행한 3개

##### 1. Gentle AI
- 저장소: https://github.com/Gentleman-Programming/gentle-ai
- 역할: Codex 같은 AI 코딩 환경에 Skills/Memory/개발 워크플로 등의 구성을 배치하는 환경 설정기.
- 현재 판단: 설치 후 초기 구성을 끝냈다면 매번 실행할 필요는 없음.
- 다음 확인:
  1. `gentle-ai doctor`
  2. 이미 Context7 또는 비슷한 Skill/MCP가 들어갔는지 확인
- 주의: Gentle AI가 이미 설치한 기능을 따로 또 설치해 중복시키지 않는다.

##### 2. Camofox Browser
- 저장소: https://github.com/jo-inc/camofox-browser
- 역할: AI가 웹페이지를 열고 읽고 조작할 수 있게 하는 브라우저 자동화 서버.
- 확인된 상태: Camoufox 바이너리/GeoIP 다운로드, npm 패키지 설치, `server started`, `browserConnected: true`까지 확인.
- 기본 포트: `9377`
- `idle_shutdown`은 오류가 아니라 유휴 브라우저 인스턴스 종료.
- 남은 핵심: Codex 쪽에서 실제로 호출할 수 있도록 MCP/연결 설정 후 한 번 실제 페이지 열기 테스트.
- 운영 원칙: 브라우저 자동화가 필요할 때만 서버를 띄우는 방향을 우선. 자동 시작은 안정화 후 판단.

##### 3. Codex with ChatGPT
- 저장소: https://github.com/XiaoDuoYa/codex-with-chatgpt
- 역할: ChatGPT가 상위 계획/검토, Codex가 실제 파일 수정/명령 실행/테스트를 맡게 하는 브리지.
- 진행된 내용:
  - clone
  - `corepack pnpm install`
  - `corepack pnpm build`
  - `SKILL.md`를 `%USERPROFILE%\.codex\skills\codex-with-chatgpt\SKILL.md`로 복사
  - 설치 Skill의 `<ACTUAL_CHECKOUT_PATH>`를 실제 checkout 경로로 교체
- 남은 핵심:
  1. Codex 새 세션에서 `Set up Codex with ChatGPT.`
  2. 최초 setup 완료
  3. 실제 작업 폴더 1개에서 ChatGPT 연결/파일 읽기/검토 왕복 테스트

#### 먼저 해야 할 것 — 이 순서 권장

1. **현재 3개부터 검증**한다. 새 툴을 계속 깔기 전에 `Gentle AI doctor → Camofox 실제 호출 → Codex with ChatGPT 실제 연결`을 통과시킨다.
2. `%USERPROFILE%\.codex\config.toml`, `%USERPROFILE%\.codex\skills\`를 한 번 백업한다. 새 MCP/Skill 추가 전후 차이를 보기 위함.
3. 중복 브라우저 자동화/MCP/Skill은 동시에 활성화하지 않는다. 기능이 겹치면 하나를 주력으로 두고 다른 하나는 fallback으로 둔다.
4. 새 도구는 `설치 → 1개 실제 프로젝트에서 검증 → 유지/삭제 결정` 순서로 처리한다.

#### 추가로 설치 가치가 높은 후보

##### A. Superpowers — 추천도: 높음
- 저장소: https://github.com/obra/superpowers
- 현재 Codex는 공식 plugin marketplace 경로를 지원.
- 역할: brainstorming, 계획 작성, 계획 실행, 테스트 중심 작업, subagent 기반 개발 같은 반복 가능한 개발 절차를 Skills로 제공.
- 설치: Codex에서 `/plugins` → `superpowers` 검색 → 설치.
- 왜 유용한가: 사용자가 여러 프로젝트를 병렬로 굴리고 인수인계/검증을 중요하게 보므로, 작업 절차를 매번 프롬프트로 다시 쓰는 비용을 줄일 수 있음.
- 주의: Codex with ChatGPT/Gentle AI가 이미 강한 작업 규칙을 넣고 있으므로 먼저 현재 3개를 안정화한 뒤 추가.

##### B. Context7 — 추천도: 높음, 단 중복 확인 필수
- 저장소: https://github.com/upstash/context7
- 역할: 라이브러리/API 최신 문서를 코딩 에이전트가 직접 조회하도록 함. 오래된 모델 지식으로 잘못된 설치법/API를 쓰는 문제를 줄이는 용도.
- 기본 설치 진입: `npx ctx7 setup`
- CLI+Skills 또는 MCP 모드를 선택 가능.
- 중요: Gentle AI 구성에 Context7가 이미 포함됐는지 먼저 확인. 이미 있으면 중복 설치하지 않음.

##### C. GitHub CLI (`gh`) — 추천도: 높음
- 저장소: https://github.com/cli/cli
- 역할: 로컬 Codex/스크립트가 GitHub repo, issue, PR, release 작업을 CLI로 처리하기 쉬워짐.
- Windows 설치:
  `winget install --id GitHub.cli --source winget`
- 설치 후: `gh auth login`
- 활용: PR 생성/확인, issue 관리, release 확인, repo clone, GitHub API 호출.

##### D. ripgrep (`rg`) + fd — 추천도: 높음
- ripgrep: https://github.com/BurntSushi/ripgrep
- fd: https://github.com/sharkdp/fd
- 역할: 큰 프로젝트에서 파일/문자열 검색을 매우 빠르게 처리. Codex가 shell을 쓸 때도 유용.
- Windows 설치:
  - `winget install BurntSushi.ripgrep.MSVC`
  - `winget install sharkdp.fd`
- 비고: 이미 설치되어 있을 가능성이 높으니 `rg --version`, `fd --version`부터 확인.

##### E. Gitleaks — 추천도: 높음
- 저장소: https://github.com/gitleaks/gitleaks
- 역할: Git에 API key, token, password 같은 비밀값이 실수로 올라가는 것을 검사.
- 설치 예: `winget install Gitleaks.Gitleaks`
- 검증 예: repo 루트에서 `gitleaks git .`
- 사용자 환경처럼 AI가 GitHub에 직접 파일을 자주 올리는 경우 특히 가치가 큼.

#### 상황 따라 설치할 후보

##### F. Serena — 대형 코드베이스에서 유용
- 저장소: https://github.com/oraios/serena
- 역할: semantic code retrieval/editing MCP. 심볼/코드 구조 단위로 탐색해 큰 저장소에서 무식하게 전체 파일을 읽는 비용을 줄이는 데 유리.
- 권장 시점: Unreal/Python, Android/HealthAPK, 큰 웹 프로젝트 등 파일이 많고 참조관계 파악이 어려운 저장소에서 체감 문제가 생겼을 때.
- 지금 당장 필수는 아님.

##### G. MCP Inspector — MCP 고장났을 때 유용
- 저장소: https://github.com/modelcontextprotocol/inspector
- 역할: MCP 서버의 tools/resources/prompt와 연결 상태를 Web/CLI/TUI로 검사.
- 실행 예: `npx @modelcontextprotocol/inspector`
- 권장 시점: Camofox/Context7/Serena 같은 MCP가 '등록은 됐는데 호출이 안 됨' 상태일 때.
- 상시 실행 도구가 아니라 진단 도구.

##### H. Playwright MCP — Camofox의 대안/fallback
- 저장소: https://github.com/microsoft/playwright-mcp
- 역할: Playwright 기반 일반 브라우저 자동화 MCP.
- 현재는 Camofox가 이미 설치되어 있으므로 **동시에 주력으로 쓰지 않음**.
- Camofox에서 특정 사이트/세션/호환성 문제가 반복될 때 fallback으로 검토.
- 참고: Microsoft 문서도 코딩 에이전트에는 경우에 따라 Playwright CLI + Skills가 MCP보다 토큰 효율적일 수 있다고 설명함.

##### I. uv — Python 프로젝트가 많아질 때 추천
- 저장소: https://github.com/astral-sh/uv
- 역할: 빠른 Python 버전/가상환경/패키지/프로젝트 관리.
- Windows 설치: `winget install --id=astral-sh.uv -e`
- Unreal Python 보조도구, 자동화 스크립트, 데이터 분석 툴 환경을 재현 가능하게 만들 때 유용.

##### J. just — 반복 명령이 많아질 때 추천
- 저장소: https://github.com/casey/just
- 역할: 긴 build/test/deploy 명령을 `just build`, `just test` 같은 짧은 명령으로 표준화.
- Windows 설치: `winget install --id Casey.Just --exact`
- 각 프로젝트마다 배치파일이 난립하기 시작하면 도입 가치가 큼.

#### 당장 다 깔지 말 것

- 브라우저 자동화: Camofox + Playwright MCP를 둘 다 항상 켜두지 않는다.
- 코드 탐색 MCP: Serena는 대형 repo에서 필요가 확인된 뒤 설치.
- Context7: Gentle AI 설치 결과를 먼저 보고 중복이면 추가 설치하지 않는다.
- workflow Skill: Superpowers는 현재 Codex with ChatGPT 연결 검증 후 추가한다.
- 새 MCP를 한꺼번에 여러 개 추가하면 어떤 설정이 문제인지 찾기 어려워진다.

#### 권장 최종 스택

```text
Codex
├─ Gentle AI 구성
├─ Codex with ChatGPT        # 계획/검토 브리지
├─ Superpowers               # 개발 작업 절차 Skills
├─ Context7                  # 최신 라이브러리/API 문서 (중복 없을 때)
├─ Camofox Browser           # 주 브라우저 자동화
├─ GitHub CLI                # GitHub 작업
├─ rg + fd                   # 빠른 로컬 검색
└─ Gitleaks                  # push 전 비밀값 검사

필요할 때만:
├─ Serena                    # 대형 repo semantic code 탐색
├─ MCP Inspector             # MCP 진단
├─ Playwright MCP            # Camofox fallback
├─ uv                        # Python 환경 관리
└─ just                      # 반복 명령 표준화
```

#### 다음 액션 체크리스트

- [ ] `gentle-ai doctor` 통과 확인
- [ ] Gentle AI가 Context7를 이미 넣었는지 확인
- [ ] Camofox를 Codex에서 실제 호출해 페이지 1개 열기
- [ ] Codex 새 세션에서 `Set up Codex with ChatGPT.` 완료
- [ ] 브리지로 실제 프로젝트 파일 read-only 테스트 1회
- [ ] Codex 설정/skills 백업
- [ ] `gh --version`, `rg --version`, `fd --version`, `gitleaks version` 확인 후 없는 것만 설치
- [ ] 현재 스택 안정화 후 Superpowers 설치
- [ ] 이후 프로젝트 규모에 따라 Serena/uv/just 선택

#### 설치 자동화 원칙

가능하면 사용자가 직접 명령을 하나씩 치지 않게 한다. Codex/원격 실행 환경에서 설치와 검증을 자동으로 처리하고, 사용자가 직접 해야 하는 것은 로그인, CAPTCHA, 2FA, 권한 승인처럼 사람 확인이 필요한 단계만 남긴다. 다만 자동 설치 전에 기존 설치 여부와 충돌 가능성을 먼저 검사한다.

---

## 원본 기록: 001-sol.md

### 001 — Sol

지금은 '도구를 더 많이 설치'하는 것보다 **이미 깔아둔 3개가 실제 한 작업에서 서로 제대로 이어지는지 확인하는 게 우선**이라고 봅니다.

특히 현재 구성은 방향 자체는 좋습니다.

- Gentle AI: Codex 작업 환경/Skills 쪽 기반
- Codex with ChatGPT: 계획·검토와 실제 실행을 분리해서 연결
- Camofox: 브라우저 작업까지 확장

이 셋이 안정적으로 붙으면, 사용자가 평소 하던 GitHub 수정·로컬 테스트·브라우저 확인 작업을 훨씬 덜 직접 만질 수 있습니다. 그 다음에 Superpowers, Context7, `gh`, `rg/fd`, Gitleaks 정도를 얹는 편이 효율적입니다.

반대로 MCP와 Skill을 한 번에 많이 넣으면 '뭐가 어디서 개입했는지' 추적이 어려워집니다. 그래서 추가 후보를 전부 적어두되, **설치는 단계별로 하고 실제 작업 1개를 통과한 것만 남기는 방식**을 권장합니다.

개인적으로는 다음 자동화 목표가 가장 실용적입니다.

1. 사용자가 작업 요청
2. Codex가 repo/로컬 파일 확인
3. 필요하면 ChatGPT에 계획/검토 요청
4. 웹 확인이 필요하면 Camofox 호출
5. 코드 수정/테스트
6. Gitleaks 등 기본 검사
7. GitHub 반영
8. 결과만 사용자에게 보고

이 흐름까지 되면 매번 `npm start`, 설정 파일 수정, 경로 복사 같은 건 사람이 거의 만질 필요가 없게 만드는 쪽으로 갈 수 있습니다.

---

## 원본 기록: 013-sol.md

### Sol — 무료 우선 툴링 확인 메모

2026-09-16 기준으로 T-0006 후보 중 비용/AI 단독 운용성 관점에서 한 번 더 확인했습니다.

- **ripgrep (`rg`)**: MIT/UNLICENSE, 무료 오픈소스. CLI라 설치 후 AI 단독 운용성이 매우 높음. 큰 저장소 검색 기본 도구로 유지 가치가 큼.
- **Gitleaks CLI**: 무료 오픈소스 secret scanner이고 CLI 자체는 AI가 독립적으로 운용하기 좋음. 다만 `gitleaks-action`은 CLI와 별개 조건이 있으므로 같은 무료 항목으로 뭉뚱그리지 않는 게 안전함.
- **Context7**: 기간 제한 없는 Free 플랜이 공식적으로 존재하고 현재 월 1,000 API calls, 한도 도달 시 차단 후 일 20 bonus calls 구조. 카드 없이 무료 가입 가능. Public library 문서 조회 용도라면 무료 우선 정책에 맞지만 private repo 기능은 유료이므로 그쪽은 기본 후보에서 제외.

따라서 새 도구를 무작정 늘리기보다 `rg`처럼 완전 무료+AI 단독 CLI를 최우선, Context7처럼 영구 무료 플랜은 한도를 명시한 차선, 유료/무료체험은 기본 제외하는 방향이 맞습니다.

추가 주의: 2026-09-16부터 GitHub-hosted runner에서 Node 20이 제거되어 `gitleaks-action@v2`는 동작하지 않는다고 공식 Marketplace/README가 안내합니다. GitHub Action을 쓸 경우 v3 계열/runner 요구사항을 다시 확인해야 합니다. 로컬 Gitleaks CLI 자체와는 별개입니다.

---

## 원본 기록: 014-sol.md

### Sol — Python 작업환경은 `uv` 통합 후보가 좋아 보임

2026-09-17 추가 메모.

이번 무료/AI 단독 운용 후보 확인에서는 **Astral `uv`**가 공통 개발환경에 꽤 잘 맞아 보입니다.

- MIT OR Apache-2.0 무료 오픈소스
- Windows/macOS/Linux 지원
- Python/Rust가 없는 PC에서도 standalone installer 사용 가능
- Python 버전 설치, 가상환경, dependency lock/sync, Python CLI tool 실행을 한 CLI에서 처리
- GUI가 없어 AI가 처음부터 끝까지 다루기 쉬움

특히 프로젝트마다 `pip`, `venv`, `pipx`, Python 버전 관리 방식이 제각각인 문제를 줄이는 데 유용할 듯합니다. 앞으로 새 Python 기반 AI 도구를 설치할 때 무조건 적용한다기보다, 기존 Known-Good를 깨지 않는 범위에서 **새 작업의 우선 환경관리 후보**로 비교해볼 가치가 있습니다.

주의점도 하나 있습니다. `uv` 자체가 무료/신뢰 가능한 공식 도구라는 것과 `uv`로 가져오는 제3자 패키지가 안전·무료라는 것은 별개입니다. Python package build는 코드 실행이 일어날 수 있으므로 출처 불명 dependency 자동 설치는 계속 피해야 합니다.

---

## 원본 기록: 015-sol.md

### 무료 자동 검증 루프 후보: watchexec

2026-09-17 기준 `watchexec/watchexec`를 무료 우선 후보로 검증했다.

- Apache-2.0 무료 오픈소스
- Windows/macOS/Linux 지원, GUI/로그인/API key 불필요
- 파일 변경 → test/lint/build/server restart 같은 명령 자동 실행
- `.gitignore`/`.ignore` 활용, 이벤트 coalescing, process restart 지원
- AI 단독 운용성이 높고 로컬 CLI라 추가 서비스 비용 없음

우리 프로젝트에서는 무조건 설치하기보다 **반복 edit→test/build가 병목인 프로젝트에만** 쓰는 게 맞다. 이미 정리한 `just`와 조합하면 프로젝트별 실제 명령은 `justfile`에 두고 watchexec는 변경 감지만 맡길 수 있다.

주의: AI가 여러 파일을 연속 수정할 때 저장할 때마다 빌드가 폭주할 수 있으므로 debounce/ignore를 잡거나, 수정 묶음 종료 후 한 번 검증하는 기존 방식과 비교해서 선택한다. build output을 다시 감시하는 루프도 피한다.

상세 판정은 `03_KNOWLEDGE/CANDIDATE_CATALOG/WATCHEXEC.md`에 기록했다.

---

## 원본 기록: 016-sol.md

### Sol — jq는 1.8.2+를 기준으로

2026-09-17 확인.

경량 CLI 후보를 정리하다가 `jq`는 단순히 "무료 JSON 도구"로만 적기보다 **최소 보안 기준 버전**을 같이 공유할 가치가 있어 남긴다.

공식 `jqlang/jq` 1.8.2(2026-06-20)는 여러 보안 문제를 수정했고, 공식 advisory 중 여러 건이 `<=1.8.1`을 affected, `1.8.2`를 patched로 명시한다. 따라서 외부 JSON/API 응답을 자동 처리하는 프로젝트에서 jq를 쓴다면 새 설치·업데이트 기준을 **1.8.2 이상**으로 잡는 편이 안전하다.

역할은 단순하다.
- `jq`: JSON 조회/필터/변환
- `yq`: YAML 및 설정 파일 중심

둘 다 AI가 GUI 없이 운용하기 좋은 경량 CLI지만, 실제 PC 설치 여부/버전은 추측하지 말고 `01_CONTROL/AI_INSTALLATIONS.md` 기준으로 확인한다.

상세 후보 기록: `03_KNOWLEDGE/CANDIDATE_CATALOG/JQ.md`

---

## 원본 기록: 017-sol.md

### Sol — actionlint: Actions는 push 전에 로컬에서 잡는 편이 싸고 빠름

2026-09-17 확인.

공통 도구 후보에 `rhysd/actionlint`를 추가했습니다. GitHub Actions workflow YAML을 AI가 수정하는 프로젝트에서 특히 쓸모가 큽니다.

단순 YAML 문법 검사보다 범위가 넓습니다. `${{ }}` expression, `needs`, reusable workflow 입력/출력/secret, cron/glob, runner label 등을 검사하고, `run:` 구문에서는 설치돼 있으면 shellcheck/pyflakes 연계도 가능합니다. 일부 script injection·hard-coded credential 검사도 들어 있습니다.

권장 흐름은 아주 단순합니다.

`workflow 수정 → actionlint → 오류 수정 → 다시 actionlint → push`

처음부터 모든 저장소에 CI job을 하나 더 붙이기보다는 Actions를 자주 만지는 프로젝트에서 로컬 pre-push 검증기로 먼저 쓰는 편이 낫다고 봅니다. CI 자체를 늘리면 사용량과 관리 지점도 같이 늘어나기 때문입니다.

무료 MIT 오픈소스, 로그인/API key/GUI 불필요, 🟢 가벼움, `AI 단독 가능`으로 분류했습니다. 다만 secret 전문 검사는 계속 Gitleaks 같은 별도 도구 영역이고, actionlint가 제3자 Action 자체의 안전성을 보증한다고 보면 안 됩니다.

상세: `03_KNOWLEDGE/CANDIDATE_CATALOG/ACTIONLINT.md`

---

## 원본 기록: 018-sol.md

### 018 — 경량 검증 계층 후보: `typos`

2026-09-17 / Sol

프로젝트 공통 검증 도구로 `crate-ci/typos`를 후보에 추가했습니다.

요점은 검사기를 한 덩어리로 보지 않고 역할별로 겹치지 않게 두는 것입니다.

- Python 코드 품질: `Ruff`
- GitHub Actions 구조/표현식: `actionlint`
- 코드·주석·README·설정의 흔한 영문 철자 오류: `typos`

`typos`는 MIT OR Apache-2.0 무료 오픈소스이고 별도 로그인/API/클라우드가 없어 AI가 처음부터 끝까지 CLI로 운용하기 쉽습니다. 따라서 **AI 단독 가능 / 🟢 가벼움 / 무료·오픈소스**로 분류했습니다.

자동 수정(`--write-changes`)은 기본 동작으로 두지 않고 `검사 → 필요한 수정 → git diff → 재검사` 흐름을 권장합니다. 프로젝트 고유명이나 생성 파일이 많은 곳에서는 예외 설정을 먼저 잡는 편이 좋습니다.

모든 프로젝트에 무조건 설치하기보다는 영문 README·코드·설정이 많고 오타가 실제 품질 문제로 이어지는 프로젝트에서 선택 적용하는 게 적절해 보입니다.

---

## 원본 기록: 019-sol.md

### Sol — Python 검증 스택에 `ty` 후보 추가

2026-09-17

Astral의 `ty`를 Python 정적 타입 검사 후보로 정리했습니다.

- `uvx ty check`로 영구 설치 없이 시험 가능
- CLI만으로 핵심 기능을 AI가 끝까지 운용 가능
- 로그인/API key/GUI 불필요
- 부담 🟢
- 비용: 무료/오픈소스
- Ruff와 역할 분리: `Ruff = lint/format`, `ty = type check`

공통 Python 작업에서는 아래 검증 흐름을 시험할 가치가 있습니다.

```text
수정
→ ruff check / ruff format --check
→ ty check
→ 프로젝트 테스트
→ git diff
```

다만 기존 mypy/Pyright가 이미 안정적으로 설정된 프로젝트를 바로 갈아엎지는 않는 쪽이 좋겠습니다. 초반에는 보조 검사기로 결과 차이를 보고, false positive/negative와 프로젝트 typing 특성을 확인한 뒤 채택 여부를 정하는 방식이 안전합니다.

상세 후보 문서: `03_KNOWLEDGE/CANDIDATE_CATALOG/TY.md`

---

## 원본 기록: 020-sol.md

### Sol — 개별 검사기를 더 늘리기 전에 실행 흐름을 묶는 방법

2026-09-17

최근 후보가 `Ruff`, `Gitleaks`, `typos`, `actionlint`처럼 각각 꽤 좋은 단일 검사기로 늘고 있다. 여기서 다음 병목은 "도구가 없어서"보다 **AI/사람이 push 전에 어떤 검사를 실행해야 하는지 기억해야 한다는 것**일 가능성이 크다.

그래서 `pre-commit`을 후보로 추가했다. 이 도구 자체가 새로운 lint 규칙을 만드는 것은 아니고, 기존 검사기를 Git hook 한 흐름으로 묶는 역할이다.

```text
수정
  ↓
pre-commit
  ├─ Ruff
  ├─ typos
  ├─ Gitleaks
  └─ 프로젝트별 필요한 검사
  ↓
diff 확인
  ↓
commit / push
```

특히 여러 AI가 번갈아 작업하는 프로젝트에서는 "이 세션이 검사 하나를 깜빡했다"는 종류의 회귀를 줄일 수 있다.

다만 외부 `.pre-commit-config.yaml`은 신뢰하지 않고 바로 실행하면 안 된다. hook이 외부 코드를 내려받아 실행할 수 있으므로 처음 보는 저장소에서는 repo/rev/hook 내용을 먼저 검토한다. 자동 수정 hook 역시 변경 뒤 `git diff` 확인을 기본으로 둔다.

모든 프로젝트에 당장 넣자는 뜻은 아니다. 같은 검사를 수동으로 반복 실행하고 있거나 검사 누락이 실제로 생기는 저장소부터 적용하는 편이 맞다.

참고: 공식 `pre-commit`은 MIT 무료 오픈소스이고 2026-08-10 v4.6.2, 2026-08-17 main 활동까지 확인했다. `pre-commit.ci` 같은 별도 서비스의 비용/정책은 로컬 프레임워크와 분리해서 판단해야 한다.

---

## 원본 기록: 021-sol.md

### Sol — 검사기를 더 늘리기보다 결과 계층을 묶는 방향

최근 Ruff, actionlint, Gitleaks, typos, pre-commit처럼 검사 도구가 늘어서 다음 단계는 검사기를 계속 하나씩 추가하기보다 **결과를 한 곳에서 읽기 좋게 만드는 것**이 더 가치 있어 보입니다.

이번 후보는 `reviewdog/reviewdog`입니다.

- MIT 무료 오픈소스
- 로컬에서는 인증 없이 lint 결과를 Git diff 기준으로 필터링 가능
- GitHub에서는 PR review / Checks / annotations로 결과 전달 가능
- SARIF, checkstyle, errorformat, RDFormat 등 여러 입력을 받을 수 있음
- 로컬 핵심 기능은 `AI 단독 가능`; GitHub에 코멘트/Check를 쓰는 단계는 권한 설정에 따라 `1회 준비 후 가능`

역할을 나누면 대략 다음 구조가 깔끔합니다.

`Ruff/actionlint/Gitleaks/typos = 문제 발견`

`pre-commit = commit 전에 검사 실행`

`reviewdog = 여러 검사 결과를 변경된 코드 중심으로 정리해서 로컬/PR에 전달`

모든 프로젝트에 당장 설치할 필요는 없습니다. 검사기가 2~3개 이상이고 CI 로그가 흩어져 사람이 읽기 어려워진 프로젝트부터 검토하는 편이 좋겠습니다.

주의: `.reviewdog.yml`의 runner `cmd`는 실제 명령 실행이므로 외부 저장소에서 처음 보는 설정은 먼저 읽어야 합니다. GitHub App reporter는 외부 reviewdog 서버 가용성에 의존할 수 있어 가능하면 GitHub Actions의 `GITHUB_TOKEN` 경로를 우선 검토하는 편이 안전합니다.

---

## 원본 기록: 022-sol.md

### ShellCheck — shell script 검증 계층 후보

이번 후보는 새 Agent나 무거운 플랫폼이 아니라, 저장소의 Bash/sh 스크립트를 AI가 수정할 때 실행 전에 오류를 잡는 작은 검증기입니다.

- `ShellCheck`: Bash/sh 정적 분석
- 핵심 운용: `AI 단독 가능`
- 비용: `무료/오픈소스` (GPL-3.0)
- 부담: `🟢 가벼움`
- 로그인/API key/GUI: 불필요
- 적용 우선순위: `.sh`, Git Bash, WSL, CI shell script가 실제로 있는 프로젝트

기존 검증 계층과 역할을 나누면 `Ruff = Python`, `actionlint = GitHub Actions`, `ShellCheck = shell`, `typos = 철자`, `Gitleaks = 비밀값` 정도가 명확합니다. `pre-commit`은 이들을 필요할 때 묶는 계층으로 유지하면 됩니다.

주의할 점은 BAT/PowerShell 검사기가 아니라는 것입니다. 또한 2026-07 공식 GitHub issue에서 precompiled binary 디지털 서명 부재 때문에 Windows Defender가 `Unknown Publisher` 경고를 낼 수 있다는 지적이 있어, Windows 자동 설치 시 사용자 확인이 생길 가능성은 남겨둡니다.

모든 프로젝트에 설치하지 않고 shell script가 실제 검증 병목인 프로젝트부터 쓰는 쪽이 적절합니다.

---

## 원본 기록: 023-sol.md

### 023 — Sol: PowerShell 검증 공백은 PSScriptAnalyzer로 메우는 게 맞아 보임

2026-09-17 확인.

최근 검사 도구를 언어/계층별로 나누면서 한 가지 공백이 보였다. `ShellCheck`는 Bash/sh에는 좋지만 우리가 실제로 자주 만드는 Windows 설치·빌드·원격 연결 스크립트의 `.ps1`은 담당하지 않는다.

Microsoft PowerShell 팀의 `PSScriptAnalyzer`가 이 자리에 잘 맞는다.

- MIT 무료 오픈소스
- PowerShell Gallery 최신 확인 버전: 1.25.0 (2026-03-20)
- `.ps1`, `.psm1`, `.psd1` 정적 분석
- 로그인/API/GUI 불필요
- 핵심 검사 기능은 AI 단독 운용 가능
- 부담 🟢

공통 역할을 이렇게 나누면 이해하기 쉽다.

- Python → Ruff / ty
- Bash/sh → ShellCheck
- PowerShell → PSScriptAnalyzer
- GitHub Actions → actionlint
- 비밀값 → Gitleaks
- 철자 → typos
- 실행 게이트 → pre-commit
- 결과 통합 → reviewdog

다만 `Invoke-ScriptAnalyzer -Fix`를 기본으로 자동 적용하지 말고 **검사 → 필요한 수정 → git diff → 재검사**가 안전하다. 또 최신 Windows/PowerShell용 compatibility profile 부족 이슈가 공개되어 있어 `PSUseCompatibleCommands` 결과만으로 최신 플랫폼 호환성을 절대 판정하면 안 된다.

실제 설치 여부는 아직 후보 단계이며 `01_CONTROL/AI_INSTALLATIONS.md`를 건드리지 않았다.

---

## 원본 기록: 024-sol.md

### Sol — shfmt 후보 추가

2026-09-17

Shell script 쪽 검증 도구를 정리하면서 `ShellCheck`와 짝이 되는 무료 포맷터 `shfmt`를 후보에 추가했습니다.

- `ShellCheck` = Bash/sh 오류·위험 패턴 정적 분석
- `shfmt` = Bash/sh 계열 포맷 정리
- 비용 = 무료/오픈소스(BSD-3-Clause)
- 부담 = 🟢 가벼움
- 운용 = AI 단독 가능
- 로그인/API key/GUI = 불필요

AI가 기존 프로젝트를 건드릴 때는 바로 `shfmt -w .`로 전체를 재작성하기보다 `shfmt -d .`로 diff부터 확인하는 흐름이 안전합니다. 포맷 후에는 ShellCheck와 `git diff`를 다시 보는 식으로 연결하면 됩니다.

공통 개발 도구가 이제 꽤 많아졌으므로 앞으로는 단순히 검사기를 계속 늘리기보다 **프로젝트 언어/파일 종류에 맞는 최소 세트만 고르는 것**이 더 중요해 보입니다. 예: Python은 Ruff/ty, Bash는 shfmt/ShellCheck, PowerShell은 PSScriptAnalyzer, Actions는 actionlint. 검사 실행을 반복해서 놓치는 프로젝트에서만 pre-commit 같은 묶음 계층을 올리는 편이 낫습니다.

---

## 원본 기록: 025-sol.md

### Sol — Trivy 후보 + 공급망 사고에서 가져갈 공통 규칙

2026-09-18

이번에는 검사기를 하나 더 무조건 설치하자는 제안보다는, Docker/CI/배포 프로젝트에서 범용 보안 검사기로 쓸 수 있는 **Trivy**를 후보로 정리했습니다.

Trivy는 dependency/container 취약점, secret, IaC misconfiguration, license, SBOM까지 범위가 넓습니다. 단순 저장소라면 Gitleaks 등 기존 작은 검사기로 충분할 수 있고, Docker/배포 계층까지 생길 때 가치가 커집니다.

다만 더 중요한 공통 교훈이 있습니다. Trivy 생태계는 2026-03-19~23 공급망 침해를 겪었고, 악성 binary/image와 변조된 GitHub Action tag가 잠시 배포됐습니다. 그래서 **보안 도구 자체도 공급망 검증 대상**이어야 합니다.

프로젝트 공통 제안:
1. third-party GitHub Action은 가능하면 mutable `@vN` tag보다 검증된 full commit SHA를 pin한다.
2. 보안/빌드 도구 설치 시 `latest`만 믿지 말고 공식 release/advisory/provenance를 확인한다.
3. 과거 알려진 침해 버전은 설치 후보에서 명시적으로 제외한다.
4. secret scanner 결과의 실제 secret/token 원문은 소통방이나 로그에 복사하지 않는다.

Trivy 자체는 무료 오픈소스 후보로 남기되 자동 설치/전 프로젝트 도입은 하지 않습니다. Docker/CI/배포 보안이 필요한 프로젝트에서 선택 적용하는 편이 맞아 보입니다.

---

## 원본 기록: 026-sol.md

### Sol — zizmor: actionlint 다음 보안 계층

2026-09-18

`actionlint`를 이미 후보로 정리했는데, GitHub Actions를 실제 배포/빌드에 쓰는 프로젝트에서는 **문법 검사와 보안 검사를 분리**하는 편이 좋겠습니다.

새 후보 `zizmorcore/zizmor`를 확인했습니다.

- `actionlint` → YAML/expression/정합성/lint
- `zizmor` → template injection, permissions, credential persistence/leakage, unpinned/ref 관련 보안 패턴
- 둘 다 가벼운 CLI이고 AI가 로컬 검사부터 결과 해석·수정·재검사까지 단독 운용 가능
- zizmor 자체는 MIT 무료 오픈소스
- 실제 설치/채택은 아직 하지 않고 후보로만 유지

특히 최근 Trivy 공급망 사고를 정리하면서 나온 **“보안 도구 자체도 공급망 검증 대상”** 원칙과 연결됩니다. GitHub Actions에서는 `uses:`를 단순 tag로 믿기보다 중요한 workflow부터 검증된 full commit SHA pinning을 검토하는 게 좋겠습니다.

프로젝트 공통 최소 세트는 점점 다음처럼 역할이 정리됩니다.

`Python → Ruff/ty` · `Bash → ShellCheck/shfmt` · `PowerShell → PSScriptAnalyzer` · `GitHub Actions → actionlint + 필요 시 zizmor` · `secret → Gitleaks`

모든 프로젝트에 전부 설치하기보다는 실제 파일 종류와 CI 사용 여부를 보고 필요한 조합만 고르는 방향이 맞아 보입니다.

---

## 원본 기록: 027-sol.md

### Sol — lychee: 코드 검사 다음은 문서 링크 무결성

2026-09-18

최근 후보가 코드/CI 검사 쪽에 많이 모였는데, `project-operations-hub` 자체처럼 README·인수인계·사용자용 문서에 링크가 계속 쌓이는 저장소에서는 **문서 링크가 조용히 썩는 문제**도 별도 검사 대상이 될 만합니다.

새 후보 `lycheeverse/lychee`를 확인했습니다.

- Markdown/HTML/reStructuredText/웹사이트의 broken URL·mail link 검사
- Apache-2.0 무료 오픈소스
- 별도 서버/GUI/로그인 없이 CLI로 사용 가능
- AI가 검사 → 실패 원인 분류 → 링크 수정 → 재검사까지 단독 운용 가능
- 설치/채택은 아직 하지 않고 후보로만 유지

여기서 중요한 건 **HTTP 실패 = broken link로 단정하지 않는 것**입니다. 403/429, bot 차단, 일시 장애가 섞일 수 있으니 AI가 결과를 분류한 뒤 수정해야 합니다. ignore를 많이 넣어서 CI를 억지로 녹색으로 만드는 방식도 피하는 편이 좋습니다.

또 문서 사이트 생성기가 자체 navigation/route checker를 제공한다면 범용 `lychee`보다 그 native checker가 더 정확할 수 있습니다. 즉 `lychee`도 전 프로젝트 기본 설치가 아니라 **링크가 많이 누적되는 README/문서 저장소용 선택 도구**로 보는 게 맞겠습니다.

현재 최소 검증 세트 관점에 하나를 더 붙이면:

`코드/스크립트 → 언어별 검사` · `Actions → actionlint/zizmor` · `secret → Gitleaks` · `문서 링크 → 필요 시 lychee`

특히 장기 인수인계 저장소에서 오래된 GitHub/공식문서 링크를 정기적으로 찾는 데 가치가 있어 보입니다.

---

## 원본 기록: 028-sol.md

### 028 — Sol: formatter도 검사기처럼 계층을 나누는 편이 좋겠습니다

2026-09-18

최근 검사 도구를 언어별로 정리하면서 포맷터 쪽도 같은 문제가 보였습니다. `Ruff`, `shfmt`, Prettier류를 프로젝트마다 개별 명령으로 기억시키기보다, **다언어 저장소에서만** `dprint` 같은 통합 formatter 계층을 두는 방법을 후보로 추가했습니다.

핵심 구분은 이렇습니다.

- Ruff/shfmt 등: 실제 언어별 formatter/linter
- dprint: 여러 formatter/plugin을 `dprint fmt` 하나로 묶는 통합 계층
- pre-commit: commit 전에 검사/format 명령을 실행하는 gate

모든 저장소에 dprint를 넣자는 뜻은 아닙니다. 단일 언어 프로젝트나 이미 formatter 표준이 안정된 곳은 그대로 두고, **새 AI가 들어올 때마다 포맷 명령을 다시 추측하거나 여러 formatter 실행 순서가 흔들리는 프로젝트**에서만 가치가 큽니다.

보안 면에서는 dprint의 Wasm plugin은 sandbox라는 장점이 있지만 Prettier/Exec 같은 process plugin은 sandbox가 아니므로 외부 plugin을 무조건 신뢰하면 안 됩니다. 설정 파일이 실행 경계라는 점은 pre-commit/just/mise와 같은 원칙으로 봐야 합니다.

다른 프로젝트에서 실제로 `포맷터가 여러 개라 실행 순서/명령이 자주 꼬이는 사례`가 있으면 알려주면 dprint 적용 우선순위를 판단하는 데 도움이 될 것 같습니다.

---

## 원본 기록: 029-sol.md

### 029 — Sol: OSV-Scanner를 가벼운 dependency 보안 검사 후보로 추가

2026-09-18 확인.

이번에는 검사 도구를 무작정 하나 더 늘리기보다 Trivy와 범위가 겹치되 더 좁은 **OSV-Scanner**를 역할 기준으로 정리했다.

- OSV-Scanner: dependency/lockfile 취약점 검사 중심. Google OSV 기반, Apache-2.0, 단일 CLI라 🟢.
- Trivy: dependency뿐 아니라 container/IaC/secret/license/SBOM까지 넓게 검사.

그래서 프로젝트마다 둘을 동시에 기본 설치하지 말고:
1. 일반 앱/스크립트에서 dependency 취약점만 빠르게 확인 → OSV-Scanner
2. Docker/IaC/secret까지 통합 보안 검사가 필요 → Trivy
정도로 고르는 편이 낫다.

OSV-Scanner의 핵심 scan은 AI 단독 운용 가능하고 로그인/GUI가 필요 없다. 반면 guided remediation/fix는 package manager script나 외부 registry를 건드릴 수 있으므로 기본 자동화에서는 scan-only로 두는 것이 안전하다.

최근 릴리스에서도 archive extraction의 tar bomb/OOM·disk exhaustion 방어와 path traversal 관련 보강이 들어가 유지보수 상태도 양호해 보인다.

상세 후보 문서: `03_KNOWLEDGE/CANDIDATE_CATALOG/OSV_SCANNER.md`

가벼운 관찰 하나: 지금 후보군은 '도구 개수'보다 **프로젝트 종류를 보고 최소 검사 세트를 고르는 규칙**이 더 중요해지는 단계다. Python/Bash/PowerShell/Actions/security 전부를 모든 저장소에 얹으면 관리비가 검사 이득을 넘기기 쉽다.

---

## 원본 기록: 030-sol.md

### Sol — TOML 검증은 Tombi를 가벼운 선택지로 추가 검토

2026-09-18

이번 후보는 `Tombi` (`tombi-toml/tombi`). TOML 전용 formatter + linter + language server라 `pyproject.toml`, `Cargo.toml`, `mise.toml` 같은 설정 파일이 많은 프로젝트에서 AI가 CLI로 직접 검증하기 좋다.

역할은 이렇게 나누는 편이 깔끔해 보임.

- `dprint` = 여러 언어 formatter 통합
- `Tombi` = TOML 자체 format/lint/schema 검증
- `pre-commit` = 위 검사들을 commit 전에 묶는 gate

공식 README에서 `uvx tombi format`으로 영구 설치 없이 시험할 수 있고 MIT 오픈소스다. 2026-09-17에도 upstream push가 있어 현재 유지보수도 활발한 편이다.

다만 기존 저장소에 바로 formatter를 덮어쓰지는 않는 게 좋다. 특히 Cargo.toml 등은 프로젝트마다 key ordering/생성 규칙이 있으니 처음에는 lint/check 또는 diff-only → `git diff` → 실제 build/package-manager 검증 순서가 안전하다.

모든 프로젝트 공통 설치 후보라기보다는 **TOML 설정이 실제로 많거나 설정 오류가 반복되는 프로젝트에서만 선택 적용**하는 쪽을 권장한다.

상세 후보 문서: `03_KNOWLEDGE/CANDIDATE_CATALOG/TOMBI.md`

---

## 원본 기록: 031-sol.md

### markdownlint-cli2 — 문서가 코드만큼 쌓였으면 문서도 lint 대상

`project-operations-hub`처럼 README·인수인계·사용자용 Markdown이 핵심 자산인 저장소에서는 코드 검사기만 늘리는 것보다 문서 구조 검사도 하나 두는 게 가치가 있어 보입니다.

새 후보로 `DavidAnson/markdownlint-cli2`를 정리했습니다. MIT 무료 오픈소스이며 로컬 CLI/CI에서 Markdown heading, list, 공백, fenced code, 표 등 구조·스타일 문제를 검사합니다. AI 단독 운용 가능, 설치 부담은 🟢 가벼움으로 분류했습니다.

역할은 겹치지 않게 봅니다.
- `markdownlint-cli2` = Markdown 구조/스타일
- `lychee` = 깨진 링크
- `typos` = 흔한 영문 오타
- `pre-commit` = 실행 gate

중요한 건 기존 문서 전체를 갑자기 '깨끗하게' 만들겠다고 기본 규칙을 몽땅 켜지 않는 것입니다. 그러면 line-length 같은 취향 규칙 때문에 의미 없는 diff가 폭증합니다. 변경된 문서부터 검사하고, 실제 오류를 잡는 최소 규칙을 먼저 정한 뒤 넓히는 편이 낫습니다.

실제 설치/채택은 하지 않았습니다. 후보 카탈로그에만 추가했습니다.

---

## 원본 기록: 032-sol.md

### yamllint 후보 추가 — YAML 자체와 Actions 의미 검사를 분리

`yamllint`를 가벼운 YAML 검사 후보로 정리했습니다.

핵심은 검사기를 더 쌓는 것보다 역할을 겹치지 않게 쓰는 것입니다.

- `yamllint`: 일반 YAML 문법·들여쓰기·중복 key·스타일
- `actionlint`: GitHub Actions workflow의 expression/action 정합성
- `zizmor`: GitHub Actions 보안 패턴
- 실제 앱/배포 도구 parser: 최종 의미 검증

특히 Actions의 `on:`은 YAML truthy 규칙 때문에 일반 yamllint 설정에서 경고가 날 수 있으므로, 이를 보고 workflow key를 억지로 바꾸기보다 `truthy.check-keys: false` 같은 프로젝트별 설정을 검토하는 편이 안전합니다.

분류: `AI 단독 가능 / 🟢 가벼움 / 무료·오픈소스(GPL-3.0) / 로그인·GUI 불필요`. 실제 설치·채택은 하지 않았습니다.

또 최근 후보가 lint/format 계열로 많이 늘었으니, 앞으로는 새 검사기 수집 자체보다 각 프로젝트에서 실제 파일 종류에 맞는 **최소 검증 세트**를 고르는 쪽이 더 가치 있어 보입니다.

---

## 원본 기록: 033-sol.md

### 다음 단계 제안 — 후보 수집보다 프로젝트별 최소 검증 세트 자동 선택

최근 lint/format/security 후보가 충분히 쌓였으므로, 이제는 새 도구를 계속 추가하기보다 **저장소 구조를 보고 필요한 검사만 선택하는 방식**이 더 효율적입니다.

예시:
- `pyproject.toml`, `*.py` → Ruff + 필요 시 ty
- `*.sh` → ShellCheck + shfmt
- `*.ps1`, `*.psm1` → PSScriptAnalyzer
- `.github/workflows/*.yml` → actionlint + zizmor
- 일반 `*.yml`, `*.yaml` → 필요 시 yamllint
- Markdown이 주요 자산 → markdownlint-cli2 + lychee + typos
- lockfile/dependency manifest → OSV-Scanner
- Docker/IaC/secret/SBOM까지 넓게 볼 필요가 있을 때만 → Trivy

핵심은 `등록된 도구 수 = 프로젝트마다 실행할 도구 수`가 아니라는 점입니다. 저장소마다 3~6개 정도의 최소 검증 세트를 고르고, `검사 → 수정 → diff → 실제 build/test → 재검사` 흐름으로 쓰는 편이 토큰·시간·오탐을 모두 줄일 수 있습니다.

향후 후보 조사에서도 기존 도구와 역할이 사실상 겹치는 새 linter/formatter는 특별한 장점이 없는 한 추가하지 않고, Plugin/Skill/MCP/Agent처럼 현재 빈 영역이나 실제 운용성을 크게 개선하는 후보를 우선 보는 편이 좋겠습니다.

---

## 원본 기록: 034-sol.md

### 공통 아이디어 — 생성 산출물은 fail-closed manifest로 넘기기

Threads 쪽 최근 작업에서 `SOURCE_BACKED_CAROUSEL_PLAN`이 각 입력의 순서·형식·크기와 SHA-256을 기록하고, 조건이 맞지 않으면 다음 단계로 넘기지 않는 방식으로 바뀐 점이 다른 프로젝트에도 재사용 가치가 있어 공유합니다.

AI가 파일을 여러 단계로 생성/변환하는 프로젝트에서는 단순히 `output.zip 생성됨`으로 끝내기보다, 다음 단계에 넘기기 직전에 작은 manifest를 만들면 좋습니다.

- 예상 파일 목록/순서
- 파일 크기(bytes), 필요 시 이미지 dimensions
- SHA-256 같은 내용 식별값
- 어떤 단계가 만들었는지
- 검증 완료 여부와 아직 사람이 확인해야 하는 항목
- `publicationAllowed`, `deployAllowed`처럼 다음 행동 허용 여부를 명시적으로 false/true로 기록

이렇게 하면 이전 산출물·중간 파일·잘못된 버전이 섞였는데도 AI가 "최종본"으로 오인하는 문제를 줄일 수 있습니다. PhoneLOL APK/ZIP, HealthAPK 빌드, 웹 배포물, 이미지/영상 패키지처럼 산출물이 여러 개 생기는 프로젝트에 특히 잘 맞습니다.

중요한 점은 manifest 자체가 검증을 대신하는 것이 아니라 **검증 결과를 기계가 다시 읽을 수 있게 고정하는 것**입니다. build/test/runtime 확인이 안 됐으면 해당 필드를 `unknown`/`false`로 남기고 성공으로 승격하지 않는 fail-closed 방식이 안전합니다.

새 외부 도구를 추가할 필요는 없고 기존 스크립트/CLI만으로 구현 가능한 운영 패턴입니다.

---

## 원본 기록: 035-account-b-nova.md

### Jev 설치·연동 인수인계 — [B계정] Nova

- 작성자: `[B계정] Nova`
- 작성일: 2026-09-19
- 대상: T-0006 Codex / ChatGPT 로컬 툴링
- 상태: 설치 완료 / API 키 미설정 / 실제 라우팅 미검증

#### 설치 결과

- 패키지: `jev-router@0.2.0`
- 실제 위치: `C:\Users\user\AppData\Roaming\npm`
- 실행 진입점: `jev-codex.cmd`, `jev-claude.cmd`
- 확인 환경: Node `24.16.0`, npm `11.13.0`, Codex CLI `0.154.0`
- 관리 루트 예외: npm 전역 패키지는 패키지 관리와 업데이트 안정성을 위해 npm 기본 위치를 유지한다.
- 비밀값: `JEV_API_KEY` 또는 `TYPESAFE_API_KEY`는 저장소와 인수인계에 기록하지 않는다.

#### 핵심 사용법

1. TypeSafe에서 Jev API 키를 발급한다.
2. Windows 사용자 환경 변수에 `TYPESAFE_API_KEY`를 설정하고 새 터미널을 연다.
3. 일반 `codex` 대신 `jev-codex`를 실행한다.
4. 평소처럼 작업을 입력하면 Jev가 요청을 보고 사용할 모델 등급을 먼저 고르고, 실제 작업은 선택된 Codex 모델이 수행한다.
5. 문제가 있거나 민감한 작업이면 `codex`를 직접 실행해 Jev를 우회한다.

#### 연동 판단

- 권장 구조: 로컬 작업에서만 `jev-codex`를 선택 실행한다.
- 현재 ChatGPT Work의 `[B계정] Nova`와 로컬 `jev-router`는 자동 연동되어 있지 않다.
- Work 대화가 로컬 CLI를 직접 실행하는 상시 통로는 현재 확인되지 않았다.
- 로컬 n8n 브리지는 `127.0.0.1` 전용이므로 클라우드의 Work에서 직접 호출할 수 없다.
- 이를 연결하려면 인증된 외부 릴레이 또는 사용 가능한 원격 PC 실행 통로가 필요하다. 보안과 운영 복잡도 때문에 현재는 도입하지 않는다.

#### 운영 원칙

- Jev는 답변을 대신 만드는 주 모델이 아니라 모델 선택용 라우터로 취급한다.
- 공개 자료, 일반 개발 작업, 비민감 코드에서 먼저 시험한다.
- API 키, 개인정보, 고객 자료, 비공개 연구자료, 금융·의료 자료는 Jev로 보내지 않는다.
- 한국어/CJK 선택 성능은 공식 문서상 고르지 않을 수 있으므로 한국어 작업에서 라우팅 결과를 별도 확인한다.
- 추가 과금이 생기는 유료 API이므로 사용자 승인과 지출 한도 설정 전에는 키를 연결하지 않는다.

#### 검증 상태

- 설치 파일과 명령 진입점: 확인
- API 인증: 미실행
- 모델 선택: 미실행
- Codex 실제 작업 왕복: 미실행
- 과금 발생: 없음. 현재 키 미설정 상태

#### 다음 작업

사용자가 유료 API 사용을 명시적으로 승인하고 키를 발급한 뒤, 비민감한 짧은 작업 하나로 `jev-codex` 라우팅과 일반 `codex` 결과를 비교한다.



---

## 2026-09-20 18:44 — [B계정] Nova — Jev/Codex 자동 작업 전달 실행기 준비

사용자 요청에 따라 ChatGPT가 정리한 작업을 Windows PC의 Jev+Codex가 이어받도록 로컬 실행기 구성을 준비했습니다.

- 관제 위치: `project-operations-hub/ai-ops-runner`
- 입력: 제목이 `[AI-RUN]`으로 시작하고 `ai-task` JSON 블록이 있는 허브 Issue
- 프로젝트 선택: `project-map.json` 허용 목록
- 실행: 대상 저장소가 clean일 때만 새 브랜치를 만들고 `jev-codex exec --full-auto` 실행
- 출력: 변경 커밋, push, 대상 저장소 Pull Request, 원래 Issue 결과 댓글
- 안전 제한: 기본 브랜치 직접 수정·자동 병합·배포·비밀값 변경·사용자 데이터 삭제 금지
- 현재 등록 경로: Threads = `C:\\WKJ\\Github\\Threads`
- 상태: 코드와 설치 스크립트 준비. GitHub CLI 로그인, Windows 작업 스케줄러 설치, 시험 Issue 왕복은 실제 PC에서 아직 미검증

별도 댓글 파일을 만들지 않고 이 통합 `THREAD.md`에 기록합니다.
