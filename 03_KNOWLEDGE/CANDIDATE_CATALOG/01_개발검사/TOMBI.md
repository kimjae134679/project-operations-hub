# Tombi — TOML Formatter / Linter / Language Server

확인일: 2026-09-18
상태: `CANDIDATE`
공식 저장소: `tombi-toml/tombi`

## 한줄 판단

TOML 파일이 많은 프로젝트에서 포맷·lint·schema 기반 검증을 한 도구로 처리하는 가벼운 후보. `pyproject.toml`, `Cargo.toml`, 각종 `.toml` 설정을 AI가 CLI로 직접 검사하기 좋다.

## 운용 축

| 축 | 판단 |
|---|---|
| ① 현재 바로 쓸 수 있는지 | `INSTALL` — CLI 설치 또는 `uvx tombi ...`로 시험 가능 |
| ② ChatGPT/연결 서비스라 설치 없이 가능한지 | 아니오. 단, `uvx`로 영구 설치 없이 일회 실행 가능 |
| ③ AI가 설치·설정 가능한지 | 가능 |
| ④ 핵심 기능 AI 단독 운용 | `AI 단독 가능` — CLI format/lint/check 계열을 AI가 끝까지 실행·판독 가능 |
| ⑤ 사용자 1회 도움 | 보통 없음. UAC/회사 정책 등 OS 권한 문제가 있을 때만 예외 |
| ⑥ 사용자 GUI 필요 | 없음. VS Code/JetBrains/Zed 확장은 선택사항 |
| ⑦ 주 사용자 | `AI / BOTH` |
| ⑧ 실제 설치/채택 | 미설치·미채택 후보. `01_CONTROL/TOOLS.md`, `AI_INSTALLATIONS.md`를 임의 변경하지 않음 |
| ⑨ 설치·운영 부담 | `🟢 가벼움` |
| ⑩ Cloud/self-host | Cloud 서비스가 아니라 로컬 CLI/LSP. 별도 서버 운영 없음 |
| ⑪ 보안/권한/라이선스 | MIT. 외부 schema URL을 쓰는 구성은 네트워크/출처를 별도로 검토 |
| ⑫ 비용 | `무료/오픈소스` — 프로그램 자체 추가 비용 없음 |

## 확인 근거

- 공식 저장소 설명: `TOML Formatter / Linter / Language Server`.
- 공식 README는 빠른 시험 방법으로 `uvx tombi format`을 제시한다.
- 저장소 라이선스는 MIT.
- 2026-09-17에도 공식 저장소 push가 확인되어 현재 유지보수 중인 후보로 판단한다.
- 별도 `setup-tombi` GitHub Action은 checksum 검증 옵션을 제공한다. CI 도입 시 tag만 맹신하기보다 SHA/checksum 고정을 우선 검토한다.

## 기존 도구와 역할

- `dprint` — 여러 언어 formatter를 한 흐름으로 묶는 통합 계층.
- `Tombi` — TOML 자체의 format + lint + language-server/schema 검증에 특화.
- `pre-commit` — Tombi를 포함한 여러 검사를 commit gate로 묶는 실행 계층.

따라서 TOML이 거의 없는 프로젝트에는 굳이 추가하지 않는다. `pyproject.toml`, `Cargo.toml`, `mise.toml` 등 TOML 설정이 많아지고 설정 오류가 실제 문제를 만들 때 우선 검토한다.

## 안전 운용

기존 저장소에서는 바로 전체 포맷 덮어쓰기를 하지 않는다.

1. 우선 lint/check 또는 format diff/check 성격의 비파괴 검사를 사용한다.
2. formatter 적용 전 기존 프로젝트의 key ordering/생성 파일 규칙을 확인한다.
3. 적용 후 `git diff`와 해당 패키지 관리자/빌드 검증을 같이 실행한다.
4. 자동 생성 TOML은 formatter 대상에서 제외할지 먼저 판단한다.

## 주의할 점

2026년 일부 프로젝트가 Taplo에서 Tombi로 이동하는 사례가 있지만, 이것만으로 Tombi가 모든 TOML 프로젝트의 기본 정답이라는 뜻은 아니다. Cargo.toml 포맷 관련 사용자 이슈 사례도 있었으므로 기존 대형 저장소에서는 먼저 diff-only로 시험한다.
