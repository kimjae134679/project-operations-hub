# actionlint

- 공식 저장소: `rhysd/actionlint`
- 분류: GitHub Actions workflow 정적 검사 CLI
- 비용 상태: `무료/오픈소스` (MIT)
- 현재 바로 사용: 가능
- ChatGPT/연결 서비스라 설치 불필요: 아니오. 단일 CLI/공식 Docker/브라우저 WASM playground 선택 가능
- AI 설치·설정: 가능
- 핵심 기능 AI 단독 운용: `AI 단독 가능`
- 사용자 1회 도움: 없음
- GUI 필수: 없음
- 주 사용자: AI/사용자 둘 다, 특히 AI가 workflow YAML을 수정한 직후 검증하는 용도
- 실제 설치/채택: 후보. 설치 확인 전에는 설치됨으로 올리지 않음
- 부담: 🟢 가벼움 (CLI). Docker 방식은 기존 Docker가 있을 때만 고려
- Cloud/self-host: 별도 클라우드 서비스 불필요. 로컬 CLI가 기본
- 외부 실행비용: 없음. 단 GitHub-hosted Actions에서 반복 실행하면 해당 계정/저장소의 Actions 사용량 정책은 별개

## 왜 후보인가

GitHub Actions workflow의 YAML 문법뿐 아니라 `${{ }}` expression 타입/속성, `needs`, runner label, cron/glob, reusable workflow 입력·출력·secret 등을 검사한다. `run:` 스크립트는 설치돼 있으면 `shellcheck`/`pyflakes`와 연계할 수 있고, untrusted input을 shell에 직접 넣는 script injection 및 hard-coded credential 검사도 포함한다.

현재 여러 프로젝트에서 GitHub Actions/배포 workflow를 AI가 직접 수정하므로, **workflow를 push한 뒤 실패 로그를 보고 고치는 대신 push 전에 로컬에서 잡는 검증기**로 역할이 명확하다.

기본 사용은 저장소 루트에서 `actionlint` 한 번이면 `.github/workflows`를 찾아 검사한다. JSON 형태 출력도 가능해 AI 자동 판독에 적합하다.

## 운용 기준

1. `.github/workflows/*.yml|yaml`을 수정한 뒤 push 전에 `actionlint` 실행.
2. 오류가 있으면 workflow와 오류 위치를 먼저 수정하고 다시 검사.
3. `shellcheck`/`pyflakes`는 보조 통합이며 없어도 actionlint 핵심 검사는 동작한다.
4. 처음부터 CI job을 추가하기보다 로컬 검증부터 사용한다. CI를 추가하면 GitHub Actions 사용량과 workflow 자체 복잡도가 늘 수 있다.
5. 공식 download script를 원격에서 즉시 실행하는 방식보다 가능하면 공식 release/prebuilt binary 또는 검증 가능한 package manager 경로를 선호한다.

## 보안/권한/라이선스

- MIT 무료 오픈소스.
- 로그인/OAuth/API key/CAPTCHA 불필요.
- workflow에 hard-coded credential이나 일부 script injection 패턴을 검사하지만 전문 secret scanner/Gitleaks의 대체품으로 취급하지 않는다.
- workflow가 실제로 호출하는 제3자 Action의 안전성까지 전부 보증하는 도구는 아니다.
- 브라우저 playground는 WebAssembly로 브라우저 내부에서 실행되며 공식 문서상 입력을 외부로 보내지 않는다.

## 채택 판단

**추천 후보.** GitHub Actions workflow가 있는 프로젝트에서만 설치 가치가 있다. 모든 프로젝트 공통 필수 CLI로 강제하지 않고, Actions 수정이 잦은 저장소의 `수정 → actionlint → push` 검증 단계에 우선 적용한다.

확인 기준: 2026-09-17 공식 `rhysd/actionlint` README/usage 문서.