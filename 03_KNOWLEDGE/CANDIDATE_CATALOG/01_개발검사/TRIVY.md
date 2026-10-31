# Trivy

- 분류: 보안 스캐너 CLI / CI
- 현재 바로 사용: 설치 후 가능
- ChatGPT 연결 서비스/설치 불필요: 아니오
- AI 설치·설정: 가능
- 핵심 기능 AI 단독 운용: AI 단독 가능
- 사용자 1회 도움: 일반 로컬 스캔은 없음. private repo/CI 권한 구성 시 필요할 수 있음
- GUI 필수: 아니오
- 주 사용자: AI/사용자 둘 다
- 실제 설치/채택: 후보 — 설치 확인 전
- 설치·운영 부담: 🟡 보통 (단일 CLI지만 vulnerability DB 다운로드/갱신과 스캔 비용 존재)
- Cloud/self-host: 기본 로컬 CLI 기준. 별도 Cloud 서비스는 이 항목의 기본 후보가 아님
- 비용 상태: 무료/오픈소스 (Trivy CLI 자체). 외부 CI/registry/cloud 비용은 별도

## 용도

로컬 filesystem, Git repository, container image 등을 대상으로 취약점·secret을 검사하고, 필요 시 misconfiguration·license 검사와 SBOM 생성까지 한 도구에서 수행한다. `trivy fs`/`trivy repo`는 기본적으로 vulnerability와 secret scanner가 켜지며 misconfiguration/license는 선택적으로 켠다.

Gitleaks가 secret 검출에 집중한다면 Trivy는 dependency vulnerability, container/image, IaC misconfiguration, SBOM까지 범위를 넓힐 때 검토할 후보이다. 단순 소스 저장소에 무조건 추가하지 않고 Docker/배포/의존성 보안 점검 필요가 커질 때 우선한다.

## 2026 공급망 사고 — 중요

2026-03-19~23 Trivy 배포 생태계에 공급망 침해가 있었다. GitHub Advisory 기준으로 악성 `trivy v0.69.4` binary와 일부 `v0.69.5/v0.69.6` DockerHub image, 변조된 `aquasecurity/trivy-action` 및 `aquasecurity/setup-trivy` tag가 배포된 기간이 있었다. 관련 악성 artifact/tag는 제거됐지만 중간 cache 등에 남을 가능성이 지적됐다.

따라서:
- `v0.69.4` binary와 당시 배포된 문제 image는 사용 금지/잔존 여부 확인 대상으로 취급한다.
- GitHub Actions에서 `@vX` 같은 mutable tag만 믿지 말고 검증된 full commit SHA pinning을 우선한다.
- 설치/업데이트 시 공식 release provenance/signature와 현재 advisory를 확인한다.
- 과거 2026-03-19~23 사이 Trivy Action/Setup-Trivy를 실행한 환경은 별도 노출 점검 가치가 있다.

2026-08-14 공식 발표 기준 `v0.74.0`이 공개되어 있다. 신규 설치 시 과거 사고 버전을 피하고 현재 안전한 최신 release를 다시 확인한다.

## 운용 예

- 기본 로컬 프로젝트 검사: `trivy fs <path>`
- 취약점+설정오류+secret: `trivy fs --scanners vuln,misconfig,secret <path>`
- 저장소: `trivy repo <repo>`

처음에는 report-only로 실행하고 자동 수정/삭제 작업과 연결하지 않는다. secret 결과는 로그나 소통방에 원문 비밀값을 복사하지 않는다.

## 판단

기능 가치는 높지만 2026 공급망 사고 이력 때문에 `가볍게 아무 버전이나 설치`하는 후보로 취급하지 않는다. Docker/CI/배포 보안이 필요한 프로젝트에서 provenance를 확인해 선택적으로 채택한다.

## 공식/검증 근거
- Trivy docs: filesystem/repository scanner — vulnerability, misconfiguration, secrets, licenses, SBOM
- GitHub Advisory `GHSA-69fq-xp46-6x23` / `CVE-2026-33634`: 2026 공급망 침해 범위와 대응
- Aqua Security Trivy announcement: v0.74.0 (2026-08-14)

확인일: 2026-09-18
