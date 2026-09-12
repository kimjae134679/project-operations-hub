# T-0006 — Codex / ChatGPT 로컬 툴링 스택 정리

- 상태: OPEN / SETUP REVIEW
- 작성일: 2026-09-13
- 범위: Windows 로컬 개발, Codex, ChatGPT 연동, 브라우저 자동화, MCP/Skills, GitHub 작업
- 목적: 무작정 도구 수를 늘리는 것이 아니라, 현재 설치한 3개를 먼저 안정화하고 실제로 도움이 되는 도구만 단계적으로 추가한다.

## 현재 설치/진행한 3개

### 1. Gentle AI
- 저장소: https://github.com/Gentleman-Programming/gentle-ai
- 역할: Codex 같은 AI 코딩 환경에 Skills/Memory/개발 워크플로 등의 구성을 배치하는 환경 설정기.
- 현재 판단: 설치 후 초기 구성을 끝냈다면 매번 실행할 필요는 없음.
- 다음 확인:
  1. `gentle-ai doctor`
  2. 이미 Context7 또는 비슷한 Skill/MCP가 들어갔는지 확인
- 주의: Gentle AI가 이미 설치한 기능을 따로 또 설치해 중복시키지 않는다.

### 2. Camofox Browser
- 저장소: https://github.com/jo-inc/camofox-browser
- 역할: AI가 웹페이지를 열고 읽고 조작할 수 있게 하는 브라우저 자동화 서버.
- 확인된 상태: Camoufox 바이너리/GeoIP 다운로드, npm 패키지 설치, `server started`, `browserConnected: true`까지 확인.
- 기본 포트: `9377`
- `idle_shutdown`은 오류가 아니라 유휴 브라우저 인스턴스 종료.
- 남은 핵심: Codex 쪽에서 실제로 호출할 수 있도록 MCP/연결 설정 후 한 번 실제 페이지 열기 테스트.
- 운영 원칙: 브라우저 자동화가 필요할 때만 서버를 띄우는 방향을 우선. 자동 시작은 안정화 후 판단.

### 3. Codex with ChatGPT
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

## 먼저 해야 할 것 — 이 순서 권장

1. **현재 3개부터 검증**한다. 새 툴을 계속 깔기 전에 `Gentle AI doctor → Camofox 실제 호출 → Codex with ChatGPT 실제 연결`을 통과시킨다.
2. `%USERPROFILE%\.codex\config.toml`, `%USERPROFILE%\.codex\skills\`를 한 번 백업한다. 새 MCP/Skill 추가 전후 차이를 보기 위함.
3. 중복 브라우저 자동화/MCP/Skill은 동시에 활성화하지 않는다. 기능이 겹치면 하나를 주력으로 두고 다른 하나는 fallback으로 둔다.
4. 새 도구는 `설치 → 1개 실제 프로젝트에서 검증 → 유지/삭제 결정` 순서로 처리한다.

## 추가로 설치 가치가 높은 후보

### A. Superpowers — 추천도: 높음
- 저장소: https://github.com/obra/superpowers
- 현재 Codex는 공식 plugin marketplace 경로를 지원.
- 역할: brainstorming, 계획 작성, 계획 실행, 테스트 중심 작업, subagent 기반 개발 같은 반복 가능한 개발 절차를 Skills로 제공.
- 설치: Codex에서 `/plugins` → `superpowers` 검색 → 설치.
- 왜 유용한가: 사용자가 여러 프로젝트를 병렬로 굴리고 인수인계/검증을 중요하게 보므로, 작업 절차를 매번 프롬프트로 다시 쓰는 비용을 줄일 수 있음.
- 주의: Codex with ChatGPT/Gentle AI가 이미 강한 작업 규칙을 넣고 있으므로 먼저 현재 3개를 안정화한 뒤 추가.

### B. Context7 — 추천도: 높음, 단 중복 확인 필수
- 저장소: https://github.com/upstash/context7
- 역할: 라이브러리/API 최신 문서를 코딩 에이전트가 직접 조회하도록 함. 오래된 모델 지식으로 잘못된 설치법/API를 쓰는 문제를 줄이는 용도.
- 기본 설치 진입: `npx ctx7 setup`
- CLI+Skills 또는 MCP 모드를 선택 가능.
- 중요: Gentle AI 구성에 Context7가 이미 포함됐는지 먼저 확인. 이미 있으면 중복 설치하지 않음.

### C. GitHub CLI (`gh`) — 추천도: 높음
- 저장소: https://github.com/cli/cli
- 역할: 로컬 Codex/스크립트가 GitHub repo, issue, PR, release 작업을 CLI로 처리하기 쉬워짐.
- Windows 설치:
  `winget install --id GitHub.cli --source winget`
- 설치 후: `gh auth login`
- 활용: PR 생성/확인, issue 관리, release 확인, repo clone, GitHub API 호출.

### D. ripgrep (`rg`) + fd — 추천도: 높음
- ripgrep: https://github.com/BurntSushi/ripgrep
- fd: https://github.com/sharkdp/fd
- 역할: 큰 프로젝트에서 파일/문자열 검색을 매우 빠르게 처리. Codex가 shell을 쓸 때도 유용.
- Windows 설치:
  - `winget install BurntSushi.ripgrep.MSVC`
  - `winget install sharkdp.fd`
- 비고: 이미 설치되어 있을 가능성이 높으니 `rg --version`, `fd --version`부터 확인.

### E. Gitleaks — 추천도: 높음
- 저장소: https://github.com/gitleaks/gitleaks
- 역할: Git에 API key, token, password 같은 비밀값이 실수로 올라가는 것을 검사.
- 설치 예: `winget install Gitleaks.Gitleaks`
- 검증 예: repo 루트에서 `gitleaks git .`
- 사용자 환경처럼 AI가 GitHub에 직접 파일을 자주 올리는 경우 특히 가치가 큼.

## 상황 따라 설치할 후보

### F. Serena — 대형 코드베이스에서 유용
- 저장소: https://github.com/oraios/serena
- 역할: semantic code retrieval/editing MCP. 심볼/코드 구조 단위로 탐색해 큰 저장소에서 무식하게 전체 파일을 읽는 비용을 줄이는 데 유리.
- 권장 시점: Unreal/Python, Android/HealthAPK, 큰 웹 프로젝트 등 파일이 많고 참조관계 파악이 어려운 저장소에서 체감 문제가 생겼을 때.
- 지금 당장 필수는 아님.

### G. MCP Inspector — MCP 고장났을 때 유용
- 저장소: https://github.com/modelcontextprotocol/inspector
- 역할: MCP 서버의 tools/resources/prompt와 연결 상태를 Web/CLI/TUI로 검사.
- 실행 예: `npx @modelcontextprotocol/inspector`
- 권장 시점: Camofox/Context7/Serena 같은 MCP가 '등록은 됐는데 호출이 안 됨' 상태일 때.
- 상시 실행 도구가 아니라 진단 도구.

### H. Playwright MCP — Camofox의 대안/fallback
- 저장소: https://github.com/microsoft/playwright-mcp
- 역할: Playwright 기반 일반 브라우저 자동화 MCP.
- 현재는 Camofox가 이미 설치되어 있으므로 **동시에 주력으로 쓰지 않음**.
- Camofox에서 특정 사이트/세션/호환성 문제가 반복될 때 fallback으로 검토.
- 참고: Microsoft 문서도 코딩 에이전트에는 경우에 따라 Playwright CLI + Skills가 MCP보다 토큰 효율적일 수 있다고 설명함.

### I. uv — Python 프로젝트가 많아질 때 추천
- 저장소: https://github.com/astral-sh/uv
- 역할: 빠른 Python 버전/가상환경/패키지/프로젝트 관리.
- Windows 설치: `winget install --id=astral-sh.uv -e`
- Unreal Python 보조도구, 자동화 스크립트, 데이터 분석 툴 환경을 재현 가능하게 만들 때 유용.

### J. just — 반복 명령이 많아질 때 추천
- 저장소: https://github.com/casey/just
- 역할: 긴 build/test/deploy 명령을 `just build`, `just test` 같은 짧은 명령으로 표준화.
- Windows 설치: `winget install --id Casey.Just --exact`
- 각 프로젝트마다 배치파일이 난립하기 시작하면 도입 가치가 큼.

## 당장 다 깔지 말 것

- 브라우저 자동화: Camofox + Playwright MCP를 둘 다 항상 켜두지 않는다.
- 코드 탐색 MCP: Serena는 대형 repo에서 필요가 확인된 뒤 설치.
- Context7: Gentle AI 설치 결과를 먼저 보고 중복이면 추가 설치하지 않는다.
- workflow Skill: Superpowers는 현재 Codex with ChatGPT 연결 검증 후 추가한다.
- 새 MCP를 한꺼번에 여러 개 추가하면 어떤 설정이 문제인지 찾기 어려워진다.

## 권장 최종 스택

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

## 다음 액션 체크리스트

- [ ] `gentle-ai doctor` 통과 확인
- [ ] Gentle AI가 Context7를 이미 넣었는지 확인
- [ ] Camofox를 Codex에서 실제 호출해 페이지 1개 열기
- [ ] Codex 새 세션에서 `Set up Codex with ChatGPT.` 완료
- [ ] 브리지로 실제 프로젝트 파일 read-only 테스트 1회
- [ ] Codex 설정/skills 백업
- [ ] `gh --version`, `rg --version`, `fd --version`, `gitleaks version` 확인 후 없는 것만 설치
- [ ] 현재 스택 안정화 후 Superpowers 설치
- [ ] 이후 프로젝트 규모에 따라 Serena/uv/just 선택

## 설치 자동화 원칙

가능하면 사용자가 직접 명령을 하나씩 치지 않게 한다. Codex/원격 실행 환경에서 설치와 검증을 자동으로 처리하고, 사용자가 직접 해야 하는 것은 로그인, CAPTCHA, 2FA, 권한 승인처럼 사람 확인이 필요한 단계만 남긴다. 다만 자동 설치 전에 기존 설치 여부와 충돌 가능성을 먼저 검사한다.
