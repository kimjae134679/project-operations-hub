# yamllint

- 공식 프로젝트: `adrienverge/yamllint`
- 분류: YAML 정적 검사 / 설정·CI 품질 CLI
- 현재 바로 사용: 가능
- ChatGPT 연결 서비스/무설치: 아니오. 로컬 CLI 또는 CI에서 사용
- AI 설치·설정: 가능
- 핵심 기능 AI 단독 운용: **AI 단독 가능**
- 사용자 1회 도움: 기본 로컬 검사는 없음
- GUI 필요: 없음
- 주 사용자: AI/사용자 둘 다
- 실제 설치/채택: **후보 — 자동 설치하지 않음**
- 설치·운영 부담: **🟢 가벼움**
- Cloud/self-host: 별도 서버 없음
- 비용 상태: **무료/오픈소스**
- 라이선스: GPL-3.0

## 용도

일반 YAML 설정, CI 설정, Docker Compose/Kubernetes 등 YAML 파일의 문법·들여쓰기·중복 key·공백·스타일 문제를 빠르게 검사한다. `actionlint`가 GitHub Actions workflow의 expression/action 구조까지 이해하는 전용 검사기라면 `yamllint`는 YAML 자체의 일반적인 구조와 스타일 검사에 가깝다.

## AI 운용

기존 저장소에서는 우선 `yamllint <대상>`으로 검사만 하고, 프로젝트의 기존 스타일과 실제 parser 동작을 확인한 뒤 `.yamllint` 규칙을 최소한으로 조정한다. formatter처럼 파일 전체를 자동 재작성하는 도구가 아니므로 검사 결과를 보고 필요한 부분만 수정한다.

## GitHub Actions 주의

GitHub Actions의 `on:` 같은 key는 YAML 1.1 계열 truthy 해석 때문에 `truthy` 경고가 생길 수 있다. workflow에서는 무작정 파일을 바꾸지 말고 `truthy.check-keys: false` 같은 프로젝트별 설정을 검토한다. Actions 자체 의미 검증은 `actionlint`, 보안 패턴은 `zizmor`가 담당하도록 역할을 분리한다.

## 역할 구분

- `yamllint`: 일반 YAML 문법·스타일 검사
- `actionlint`: GitHub Actions workflow 정합성
- `zizmor`: GitHub Actions 보안 정적 분석
- 실제 앱/배포 도구 parser: 최종 의미 검증

## 보안/권한/비용

프로그램 자체는 GPL-3.0 무료 오픈소스이며 로그인/API key/GUI가 필요 없다. Python/pip 또는 OS package manager 설치 경로를 사용할 수 있다. 외부 CI 실행 비용은 별개다.

## 확인 기준

2026-09-18 공식 `adrienverge/yamllint` 저장소와 공식 문서 기준. 저장소는 2026-08-19까지 push가 확인되고, 공식 releases의 최신 표시는 v1.38.0이다.