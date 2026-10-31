# pre-commit

- 공식: https://pre-commit.com/
- 저장소: https://github.com/pre-commit/pre-commit
- 종류: Git hook 관리 CLI / 개발 자동화
- 확인 기준일: 2026-09-17

## 판단

| 축 | 상태 |
|---|---|
| 현재 바로 쓸 수 있는지 | 가능. Python/pipx/uv 등으로 설치 후 프로젝트에 `.pre-commit-config.yaml` 구성 |
| ChatGPT 연결 서비스라 설치 없이 가능한지 | 아니오 |
| AI가 설치·설정 가능한지 | 가능. 단 저장소의 hook 구성은 실행 전 검토 |
| 핵심 기능 AI 단독 운용 | AI 단독 가능 |
| 사용자 1회 도움 | 일반적인 로컬 사용에는 없음. 저장소/환경 권한 문제는 별도 |
| 사용자 GUI 필요 | 없음 |
| 주 사용자 | AI/사용자 둘 다 |
| 실제 설치/채택 | 후보. 설치 확인 전 `01_CONTROL/AI_INSTALLATIONS.md`에 올리지 않음 |
| 설치·운영 부담 | 🟢 가벼움. 단 hook별 런타임/환경 다운로드는 별도 |
| Cloud / self-host | 로컬 프레임워크. `pre-commit.ci` 같은 별도 서비스와 구분 |
| 비용 상태 | 프로그램: 무료/오픈소스(MIT). 외부 hook/서비스 비용은 각각 별도 확인 |

## 왜 후보인가

현재 후보로 정리된 Ruff, typos, Gitleaks 같은 검사를 개발자가 기억해서 각각 실행하는 대신 Git commit 전에 같은 검증 절차를 자동 실행하게 묶을 수 있다. 여러 AI가 저장소를 수정하는 환경에서는 `수정 → 검사 누락 → push`를 줄이는 공통 안전망 역할이 특히 유용하다.

예시 역할 분담:

```text
pre-commit = 검증 실행 시점/묶음 관리
Ruff       = Python lint/format
Gitleaks   = 비밀값 검사
typos       = 영문 오타 검사
```

## 운용 원칙

- 외부 저장소의 `.pre-commit-config.yaml`을 처음 봤을 때 바로 실행하지 않는다. hook은 외부 코드를 내려받아 실행할 수 있으므로 repo/rev/hook 내용을 먼저 검토한다.
- 자동 수정 hook은 Known-Good 파일을 조용히 바꾸지 않도록 `git diff` 확인을 포함한다.
- 모든 프로젝트에 억지로 도입하지 않는다. 반복적으로 같은 검사 누락이 발생하는 저장소부터 적용한다.
- `pre-commit autoupdate`로 rev를 올린 뒤에는 diff와 실제 hook 실행을 확인한다.
- 로컬 `pre-commit` 프레임워크가 무료라는 것과 `pre-commit.ci` 또는 hook이 호출하는 외부 API/서비스가 무료라는 것은 별개다.

## 현재성

공식 GitHub 기준 2026-08-10 `v4.6.2`가 확인되며 2026-08-17에도 main 커밋이 이어져 유지보수 중이다.

## 채택 우선도

중간~높음. 개별 검사기를 더 늘리는 것보다 이미 검증된 검사기를 한 실행 흐름으로 묶을 필요가 생겼을 때 가치가 크다.
