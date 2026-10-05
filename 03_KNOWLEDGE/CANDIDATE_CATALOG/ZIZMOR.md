# zizmor

- 공식 프로젝트: `zizmorcore/zizmor`
- 분류: GitHub Actions / CI-CD 보안 정적 분석
- 현재 바로 사용: 가능
- ChatGPT 연결 서비스/무설치: 아니오. 로컬 CLI 또는 CI에 추가해서 사용
- AI 설치·설정: 가능
- 핵심 기능 AI 단독 운용: **AI 단독 가능**
- 사용자 1회 도움: 기본 로컬 검사는 없음. GitHub Code Scanning/SARIF 게시 등 저장소 권한 작업은 권한 상태에 따라 필요할 수 있음
- GUI 필요: 없음
- 주 사용자: AI/사용자 둘 다, 특히 AI가 workflow를 수정한 뒤 검증하기 좋음
- 실제 설치/채택: **후보 — 자동 설치하지 않음**
- 설치·운영 부담: **🟢 가벼움**
- Cloud/self-host: 별도 서버 없음. 로컬 CLI와 CI 실행 모두 가벼운 편
- 비용 상태: **무료/오픈소스**
- 라이선스: MIT

## 무엇을 잡는가

GitHub Actions와 관련 CI/CD 설정에서 보안 문제를 정적 분석한다. 대표적으로 template injection, credential persistence/leakage, 과도한 permissions, 신뢰하기 어려운 Git ref/commit, pinning 관련 문제 등을 찾는다. Dependabot과 pre-commit 관련 설정도 일부 분석 범위에 포함된다.

## actionlint와 역할 구분

- `actionlint`: workflow YAML/expression/구조와 shell 연계 오류를 폭넓게 검사하는 **lint/정합성 검사**
- `zizmor`: workflow에서 공격면·권한·credential·supply-chain 패턴을 찾는 **보안 검사**

둘은 중복 대체재라기보다 함께 쓰는 편이 맞다. 실제 GitHub의 `gh-aw` 프로젝트에서도 2026년 zizmor와 actionlint를 함께 사용한 정적 분석 사례가 확인된다.

## 운용 권장

1. `.github/workflows/`가 있는 프로젝트에서 먼저 로컬 검사
2. 결과를 검토하고 workflow를 수정
3. `actionlint`로 문법/정합성도 재검사
4. `git diff` 확인
5. CI에 넣을 필요가 있을 때만 추가

외부 Action은 mutable tag보다 검증된 full commit SHA pinning을 우선 검토한다. 자동 수정 기능이 있더라도 처음부터 무조건 적용하지 않고 finding과 diff를 확인한다.

## 보안/권한 주의

보안 도구 자체와 설치 경로도 공급망 검증 대상이다. 공식 `zizmorcore/zizmor` 배포 경로와 문서를 기준으로 사용한다. CI에서 SARIF/Code Scanning을 게시하려면 GitHub token permissions를 최소 범위로 설정한다.

## 비용 주의

`zizmor` 프로그램 자체는 MIT 무료 오픈소스다. 다만 GitHub Actions runner/Code Scanning 등 주변 서비스의 비용·제한은 저장소/계정 조건과 별개이므로 `zizmor 무료 = 모든 CI 실행 비용 무료`로 해석하지 않는다.

## 확인 기준

2026-09-18 공식 GitHub 저장소/문서 기준. 공식 저장소는 프로젝트를 CI/CD static analysis 도구로 설명하고 MIT 라이선스를 명시한다.