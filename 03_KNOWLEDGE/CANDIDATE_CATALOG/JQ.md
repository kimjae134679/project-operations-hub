# jq — JSON CLI

확인 기준: 2026-09-17

## 분류

- 현재 바로 사용: 설치되어 있으면 즉시 사용 / 설치 여부는 `01_CONTROL/AI_INSTALLATIONS.md` 원본 기준
- ChatGPT 연결 서비스: 아니오
- AI가 설치·설정: 가능(standalone binary/package manager 경로)
- 핵심 기능 AI 단독 운용: **AI 단독 가능**
- 사용자 1회 도움: 일반 사용은 없음. PC 권한/UAC가 필요한 설치 방식이면 예외
- GUI 필요: 없음
- 주 사용자: AI / 둘 다
- 실제 채택 여부: **CANDIDATE** — 채택 원본은 `01_CONTROL/TOOLS.md`
- 부담: 🟢 가벼움
- Cloud/self-host: 해당 없음. 로컬 CLI
- 비용: **무료/오픈소스**
- 라이선스: MIT (bundled 구성요소는 각 라이선스 확인)

## 역할

JSON 설정, API 응답, 메타데이터를 터미널에서 slice/filter/map/transform 하는 경량 CLI. `yq`를 YAML·설정 파일 중심으로 쓴다면 `jq`는 JSON 중심으로 구분한다.

## 보안 기준

**1.8.1 이하를 기준 버전으로 삼지 않는다.** 공식 jq 1.8.2 릴리스는 2026-06-20 공개되었고 여러 보안 문제를 수정했다. 공식 보안 advisory들에서 `<=1.8.1` 영향, `1.8.2` patched로 명시된 문제가 있으므로 새 설치/업데이트 시 **1.8.2 이상**을 우선한다.

특히 외부/신뢰하지 않는 JSON을 자동화에서 처리할 때 오래된 jq를 방치하지 않는다. 설치 버전은 실제 PC 확인 전 `설치됨`으로 올리지 않는다.

## 운영 주의

- jq 자체가 무료라고 해서 jq로 다루는 외부 API/서비스가 무료라는 뜻은 아니다.
- 외부 입력과 복잡한 filter를 자동 처리하는 파이프라인은 jq 버전을 먼저 확인한다.
- 설치 파일은 공식 `jqlang/jq` release 또는 신뢰 가능한 패키지 관리 경로를 우선한다.

## 공식 원본

- Repository: `jqlang/jq`
- Releases: `jqlang/jq/releases`
- Documentation: `jqlang.org`
