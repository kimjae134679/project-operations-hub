# dprint — 통합 코드 포맷터 후보

공식: `dprint/dprint`, `https://dprint.dev`

## 판단

| 축 | 상태 |
|---|---|
| 현재 바로 쓸 수 있는지 | 설치 후 가능 (`INSTALL`) |
| 설치 없이 ChatGPT/연결 서비스로 사용 | 아니오 |
| AI가 설치·설정 가능 | 가능. Windows PowerShell/npm/mise 등 공식 설치 경로 있음 |
| 핵심 기능 AI 단독 운용 | **AI 단독 가능** |
| 사용자 1회 도움 | 보통 없음. 환경의 UAC/보안 경고가 발생할 때만 필요 가능 |
| 사용자 GUI 필요 | 없음 |
| 주 사용자 | AI / 둘 다 |
| 실제 채택 | CANDIDATE, 설치 확인 안 됨 |
| 설치·운영 부담 | 🟢 가벼움 |
| Cloud/self-host | 로컬 CLI 중심, 별도 Cloud 서비스 불필요 |
| 비용 | **무료/오픈소스** |
| 라이선스 | MIT |

## 왜 볼 가치가 있나

여러 언어의 formatter를 `dprint fmt` 하나로 묶는 포맷팅 플랫폼이다. TypeScript/JavaScript, JSON, Markdown, TOML, Dockerfile 등 여러 Wasm plugin을 제공하고 Python Ruff, Biome, markup_fmt 등의 adapter도 있다. 프로젝트에 포맷터가 여러 개 섞여 AI가 명령을 매번 다시 판단하는 문제를 줄일 수 있다.

특히 Wasm plugin은 sandbox에서 실행되어 network/file-system 접근이 제한된다. 반면 Prettier/Exec 같은 **process plugin은 sandbox가 아니므로 별도 신뢰 검토가 필요**하다.

## 추천 운용

- 이미 Ruff/shfmt/Prettier 등 프로젝트 표준이 안정적으로 잡혀 있으면 억지로 교체하지 않는다.
- 여러 언어·포맷터가 섞여 `format` 명령 통합 가치가 큰 프로젝트에서 먼저 시험한다.
- 최초에는 `dprint check` 또는 diff 기반 확인을 우선하고 대량 포맷 변경을 바로 commit하지 않는다.
- plugin URL/version은 설정에 명시해 재현성을 유지한다.
- 외부 process plugin/`exec` plugin은 실행 코드와 출처를 먼저 확인한다.

## 기존 후보와 역할

- `Ruff` — Python lint/format 자체 도구
- `shfmt` — Bash/sh formatter
- `dprint` — 여러 formatter/plugin을 한 CLI 아래 묶는 통합 계층
- `pre-commit` — commit 전 검사/formatter 실행을 묶는 hook 계층

따라서 dprint가 Ruff/shfmt를 무조건 대체하는 것이 아니라, **다언어 저장소에서 포맷 실행 인터페이스를 통일할 필요가 있을 때** 가치가 크다.

## 검증 메모

2026-09-18 확인 시 공식 GitHub 저장소는 archived 상태가 아니며 최근 push가 2026-09-15로 확인됐다. 공식 사이트는 CLI를 단일 static binary로 설명하고 Windows PowerShell/npm/mise 등 설치 경로를 제공한다.

마지막 확인: **2026-09-18**
