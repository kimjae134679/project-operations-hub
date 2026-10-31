# markdownlint-cli2

- 공식 프로젝트: `DavidAnson/markdownlint-cli2`
- 분류: Markdown 정적 검사 / 문서 품질 CLI
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
- 라이선스: MIT

## 용도

README, 인수인계, 사용자용 문서처럼 Markdown이 많이 쌓이는 저장소에서 heading, list, 공백, fenced code, 표 등 문서 구조 문제를 검사한다. `lychee`가 외부 링크 상태를 보는 쪽이라면 `markdownlint-cli2`는 Markdown 자체의 일관성과 구조를 보는 쪽이다.

## AI 운용

변경된 Markdown 또는 제한된 경로부터 검사하고 실제 오류와 기존 문서 스타일 충돌을 구분한다. 기존 문서가 많은 저장소에서는 기본 규칙 전체를 한꺼번에 강제하지 않고 최소 규칙부터 적용한다. 자동 수정 기능을 사용할 때는 변경 내용을 확인한 뒤 재검사한다.

## 역할 구분

- `markdownlint-cli2`: Markdown 구조·스타일 검사
- `lychee`: 깨진 URL/메일 링크 검사
- `typos`: 흔한 영문 오타 검사
- `pre-commit`: 검사 실행 gate

## 주의

custom rules, Markdown-it plugins, output formatter는 추가 모듈을 로드할 수 있으므로 출처를 확인한다. Docker, pre-commit, GitHub Actions 경로는 버전을 고정하고 공급망 정책을 적용한다. 기존 대형 문서 저장소에서 line-length 같은 규칙을 무리하게 켜 대량 수정이 생기지 않도록 한다.

## 비용 주의

프로그램 자체는 MIT 무료 오픈소스다. 외부 CI 실행 비용은 별개다.

## 확인 기준

2026-09-18 공식 `DavidAnson/markdownlint-cli2` 저장소와 npm 패키지 기준. 공개 버전 `0.23.2`, MIT 라이선스이며 로컬 CLI와 CI 사용을 지원한다.