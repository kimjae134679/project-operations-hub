# reviewdog

## 요약
- 종류: CLI / CI 코드리뷰 결과 집계·게시 도구
- 공식 저장소: `reviewdog/reviewdog`
- 역할: Ruff, actionlint, Gitleaks 등 여러 검사기의 출력을 받아 변경된 diff 중심으로 필터링하고 로컬 출력 또는 GitHub PR/Checks에 전달
- 라이선스: MIT

## 운용성 분류
| 축 | 판단 |
|---|---|
| 현재 바로 쓸 수 있는지 | 예. 로컬 CLI는 별도 서비스 없이 사용 가능 |
| ChatGPT 연결 서비스라 설치 없이 사용 | 아니오 |
| AI가 설치·설정 가능 | 가능. 단 GitHub PR reporter 권한 설정은 저장소/토큰 정책에 따라 1회 준비가 필요할 수 있음 |
| 핵심 기능 AI 단독 운용 | 로컬: `AI 단독 가능`; GitHub PR/Checks 게시: `1회 준비 후 가능` |
| 사용자 1회 도움 | 로컬은 없음. GitHub reporter는 권한/OAuth/token 정책에 따라 필요 가능 |
| 사용자 GUI 필요 | 없음 |
| 주 사용자 | AI/자동화 중심, 사람도 결과 확인 가능 |
| 실제 설치/채택 | 후보. 설치됨으로 기록하지 않음 |
| 설치·운영 부담 | 로컬 🟢 / GitHub Actions 🟢 |
| Cloud/self-host | 별도 self-host 불필요. GitHub Actions reporter는 GitHub 측 CI 사용 |
| 비용 | 프로그램: `무료/오픈소스`; GitHub Actions 실행비용은 해당 GitHub 계정/저장소 정책과 별도 |

## 왜 후보인가
현재 후보군에는 Ruff, actionlint, Gitleaks, typos처럼 검사기 자체가 늘고 있다. reviewdog는 또 하나의 검사기가 아니라 이 결과들을 `변경된 코드에 해당하는 문제` 중심으로 모아서 보여주는 계층이다. 검사기마다 별도 PR 코멘트 방식을 만들 필요를 줄일 수 있다.

로컬에서도 `-diff`로 변경분에 해당하는 진단만 걸러낼 수 있고, GitHub에서는 Checks/PR review reporter를 사용할 수 있다. SARIF 2.1.0, checkstyle, errorformat, RDFormat 등 여러 입력 형식을 지원한다.

## 안전/권한 주의
- 로컬 diff 필터링은 인증 없이 가능하다.
- GitHub PR/Checks에 결과를 게시하려면 `GITHUB_TOKEN`, PAT 또는 GitHub App 등 reporter별 권한 구성이 필요할 수 있다.
- fork PR에서는 GitHub 보안 제한으로 write 권한이 없어 reporter가 annotation 방식으로 degrade될 수 있다.
- `.reviewdog.yml`의 `cmd`는 실제 명령을 실행하므로 처음 보는 외부 저장소의 설정은 실행 전에 검토한다.
- GitHub App 방식은 외부 reviewdog 서버 가용성에 의존할 수 있으므로 기본 자동화는 가능하면 GitHub Actions의 `GITHUB_TOKEN` 경로를 우선 검토한다.
- reviewdog 자체가 무료여도 연결한 외부 검사기/API/CI 실행비용까지 무료라는 뜻은 아니다.

## 권장 위치
`검사기(Ruff/actionlint/Gitleaks/typos 등) → reviewdog로 diff 필터/통합 → 사람/AI가 결과 확인`

pre-commit과는 역할이 다르다. `pre-commit`은 commit 전에 검사를 실행하는 실행 게이트이고, `reviewdog`는 여러 진단 결과를 diff 기준으로 정리해 로컬/PR 리뷰에 전달하는 결과 계층이다.
