# lychee

- 공식 프로젝트: `lycheeverse/lychee`
- 분류: 문서 / 링크 무결성 검사 CLI
- 현재 바로 사용: 가능
- ChatGPT 연결 서비스/무설치: 아니오. 로컬 CLI 또는 CI에서 사용
- AI 설치·설정: 가능
- 핵심 기능 AI 단독 운용: **AI 단독 가능**
- 사용자 1회 도움: 기본 로컬 검사는 없음. 인증이 필요한 사설 URL 검사는 credential/header 준비가 필요할 수 있음
- GUI 필요: 없음
- 주 사용자: AI/사용자 둘 다
- 실제 설치/채택: **후보 — 자동 설치하지 않음**
- 설치·운영 부담: **🟢 가벼움**
- Cloud/self-host: 별도 서버 없음. 로컬 CLI/CI 실행
- 비용 상태: **무료/오픈소스**
- 라이선스: Apache-2.0

## 용도

Markdown, HTML, reStructuredText, 웹사이트 등의 URL/메일 링크를 검사해 깨진 링크를 찾는다. 문서와 인수인계 파일, 사용자용 요약, GitHub README처럼 링크가 계속 누적되는 저장소에서 특히 유용하다.

`project-operations-hub`처럼 여러 프로젝트 링크와 도구 공식 문서 링크를 장기간 유지하는 저장소에서는 코드 lint와 다른 **문서 무결성 검사 계층**으로 볼 수 있다.

## AI 운용 흐름

1. 변경된 Markdown/HTML부터 검사
2. 실패 URL을 실제 삭제/이동/인증 필요/일시적 rate-limit로 구분
3. 잘못된 링크만 수정하고 `git diff` 확인
4. 재검사
5. 반복 문제가 있을 때만 `lychee.toml`/ignore를 추가

AI가 검사 실행부터 결과 분류, 링크 수정, 재검사까지 처리할 수 있다. 다만 외부 사이트의 403/429나 bot 차단을 곧바로 '링크가 깨졌다'고 단정하지 않는다.

## 주의

- 네트워크 검사 결과는 rate-limit, bot 차단, 일시 장애 때문에 false positive가 날 수 있다.
- ignore 목록을 넓혀 검사를 억지로 통과시키지 않는다.
- 사이트 생성기 자체가 navigation/route 의미를 검증하는 경우에는 범용 파일 기반 link checker보다 그 사이트 생성기의 native checker가 더 정확할 수 있다.
- protocol-relative URL 등 일부 URL 표현은 별도 제약이 있을 수 있으므로 프로젝트 패턴에 맞춰 시험한다.
- GitHub Actions에 넣을 때는 다른 외부 Action과 마찬가지로 공급망/pinning 정책을 적용한다.

## 비용 주의

`lychee` 프로그램 자체는 Apache-2.0 무료 오픈소스다. GitHub Actions runner 등 외부 CI 비용은 별개다.

## 확인 기준

2026-09-18 공식 `lycheeverse/lychee` 저장소 기준. 공식 저장소는 Rust 기반 link checker로 Markdown/HTML/reStructuredText/웹사이트 등의 broken URL과 mail address 검사를 명시하며 Apache-2.0 라이선스다. 저장소는 2026-09에도 업데이트되고 있다.