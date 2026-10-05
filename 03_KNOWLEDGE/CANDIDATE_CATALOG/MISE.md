# mise — 후보 도구 기록

검증일: 2026-09-24
공식 저장소: https://github.com/jdx/mise
공식 문서: https://mise.jdx.dev/

## 판단

`mise`는 개발 도구 버전, 환경변수, 반복 task를 `mise.toml`에 선언해 프로젝트별로 재현하는 무료 오픈소스 CLI다. `uv`가 Python 중심이라면 `mise`는 Node.js/Python/Go 등 여러 런타임과 프로젝트 task를 한 설정으로 묶는 쪽이다.

단순 프로젝트에 억지로 넣지 않고, 여러 런타임 버전이 섞이거나 새 AI 세션/PC에서 개발환경 재현 문제가 반복될 때 우선 검토한다.

## 12축 상태

| 축 | 상태 |
|---|---|
| 현재 바로 사용 | `INSTALL` — 실제 설치 여부는 `01_CONTROL/AI_INSTALLATIONS.md`가 원본이며 여기서 설치됐다고 가정하지 않음 |
| 설치 없이 사용 | 아니오. 로컬 CLI 설치 필요 |
| AI 단독 설치·설정 | 가능. Windows 공식 설치 경로에 Scoop/winget이 있으며 `mise exec`/`mise run`은 shell activation 없이도 사용 가능 |
| 핵심 기능 AI 단독 운용 | `AI 단독 가능` — 검토된 프로젝트 설정을 기준으로 tool install/exec/task 실행 가능 |
| 사용자 1회 도움 | 보통 없음. 다만 설치 대상 패키지가 UAC·로그인·라이선스 동의를 요구하면 해당 단계는 사용자 도움이 필요할 수 있음 |
| 사용자 GUI 필요 | 없음 |
| 주 사용 주체 | `AI / BOTH` |
| 실제 채택 | `CANDIDATE` — 설치/채택으로 승격하지 않음 |
| 설치·운영 부담 | `🟢 가벼움` — mise 자체 기준. mise가 내려받는 SDK/도구 용량은 별도 |
| Cloud / self-host | 로컬 CLI. Cloud 서비스가 핵심인 도구가 아님 |
| 보안·권한·라이선스 | MIT. 외부 저장소의 `mise.toml`/task/bootstrap은 명령·패키지 설치를 유발할 수 있으므로 처음 실행 전 검토. mise 자체의 라이선스와 mise가 설치하는 제3자 도구/API의 라이선스·비용은 별개 |
| 비용 | `무료/오픈소스` — mise 자체. 외부 SDK·패키지·API·클라우드 비용은 별도 |

## Windows 메모

공식 문서는 Windows에서 Scoop을 권장 경로로, `winget install jdx.mise`를 대안으로 안내한다. WinGet bootstrap은 설치 프로그램에 따라 UAC가 필요할 수 있으며 mise가 UAC를 우회하지 않는다.

shell activation은 필수가 아니다. 자동화에서는 먼저 `mise exec`/`mise run` 방식으로 사용하면 shell profile 변경을 줄일 수 있다.

## AI 운용 원칙

1. 저장소에 기존 `mise.toml`이 있으면 먼저 내용을 읽는다.
2. 신뢰하지 않은 repo의 task/bootstrap을 바로 실행하지 않는다.
3. 단순 단일-runtime 프로젝트에는 새 설정을 억지로 추가하지 않는다.
4. 도입 시 `mise install` 후 실제 프로젝트 build/test가 재현되는지 확인한다.
5. `mise 자체 무료`와 `mise를 통해 설치·호출하는 외부 도구의 비용`을 분리한다.

## 현재 결론

사용자용 문서에 이미 후보로 등장했지만 canonical 후보 파일이 없던 상태를 보완했다. 지금 자동 설치할 대상은 아니며, 환경 재현 문제가 실제로 생기는 프로젝트에서 선택 적용한다.
