# OSV-Scanner

## 한줄 판단
Google의 OSV 데이터베이스를 이용해 프로젝트 의존성 취약점을 검사하는 무료 오픈소스 CLI. Trivy보다 범위를 좁혀 **소스/lockfile 의존성 취약점 확인을 가볍게 시작할 때** 우선 검토할 가치가 있다.

## 분류
- 현재 바로 사용 가능: 예 — 공식 prebuilt binary 또는 `go install github.com/google/osv-scanner/v2/cmd/osv-scanner@latest`
- ChatGPT/연결 서비스라 설치 없이 사용: 아니오
- AI가 사용자 도움 없이 설치·설정: 가능(일반 사용자 권한으로 설치 가능한 환경 기준)
- 핵심 기능 AI 단독 운용: `AI 단독 가능`
- 사용자 1회 도움: 보통 없음. UAC/기업 보안정책/프록시 등이 막을 때만 필요
- 사용자 GUI 필요: 없음
- 주 사용자: AI 중심, 사람도 CLI 사용 가능
- 실제 설치/채택: 후보만 등록. 설치/채택 확인 없음
- 설치·운영 부담: 🟢 가벼움
- Cloud/self-host: 별도 서비스 운영 불필요. 기본 스캔은 OSV 서비스 조회, offline DB 다운로드 후 오프라인 검사도 지원
- 비용 상태: `무료/오픈소스`
- 라이선스: Apache-2.0

## 핵심 기능
- npm/pip/yarn/maven/Go modules/Cargo/NuGet 등 여러 생태계의 lockfile/manifest 탐지
- `osv-scanner scan source -r <dir>`로 소스 트리 의존성 취약점 검사
- container image와 Linux OS package 검사 지원
- SPDX 기준 dependency license 요약/allowlist 검사 지원
- 로컬 OSV DB를 내려받은 뒤 offline scanning 가능
- guided remediation 기능이 있으나 자동 수정은 별도 위험 검토 필요

## Trivy와 구분
- `OSV-Scanner`: 의존성/lockfile 취약점과 OSV 생태계 중심. 단순 프로젝트 dependency 검사 시작점으로 가벼움.
- `Trivy`: dependency 외 container/IaC/secret/license/SBOM까지 더 넓은 통합 보안 검사.
- 둘을 모든 프로젝트에 중복 설치하지 않는다. 단순 dependency 검사는 OSV-Scanner, container/IaC/secret까지 한 번에 볼 필요가 있으면 Trivy를 우선 검토한다.

## AI 운용 예
```text
osv-scanner scan source -r .
```
결과를 읽어 취약 package/버전/수정 가능 버전을 분류한 뒤 실제 dependency 변경은 프로젝트 테스트와 diff 확인을 거친다.

## 보안·운영 주의
- `fix`/guided remediation은 untrusted project에서 package manager script나 외부 registry를 실행할 수 있다고 공식 문서가 경고한다. 기본값은 **scan only**로 둔다.
- 자동 remediation은 AI가 결과를 검토하고 사용 중 dependency/lockfile 영향까지 확인한 뒤 제한적으로 사용한다.
- Docker/GitHub Action에서는 floating `latest`보다 검증된 버전 pinning을 우선한다.
- 프로그램 자체 비용은 무료지만 네트워크 정책/CI 실행비 같은 외부 환경 비용은 별개다.

## 2026-09 확인 메모
- 공식 releases에는 v2.6.0 계열이 확인되며 2026-09에도 유지보수가 이어지고 있다.
- 최근 릴리스에는 archive extraction의 tar bomb 관련 OOM/disk exhaustion 방어와 path traversal 관련 보강도 포함됐다.

## 공식 자료
- GitHub: `google/osv-scanner`
- Docs: `https://google.github.io/osv-scanner/`
- License scanning: `https://google.github.io/osv-scanner/usage/license-scanning/`

마지막 확인: 2026-09-18
