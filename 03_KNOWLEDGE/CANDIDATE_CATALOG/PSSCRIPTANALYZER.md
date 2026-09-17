# PSScriptAnalyzer

## 한줄 판단
Windows/PowerShell 작업이 많은 프로젝트에서 `.ps1`/`.psm1`/`.psd1`의 결함·품질 문제를 실행 전에 검사하는 Microsoft PowerShell 팀의 정적 분석기. BAT/cmd 검사기는 아니다.

## 상태
- 현재 바로 사용: PowerShell 환경에서 모듈 설치 후 가능
- ChatGPT 연결 서비스/무설치: 아니오
- AI 설치·설정: 대체로 가능 (`Install-Module -Name PSScriptAnalyzer`), 환경 정책/UAC/Repository trust 확인이 뜨면 사용자 도움이 필요할 수 있음
- 핵심 기능 AI 단독 운용: **AI 단독 가능**
- 사용자 1회 도움: 보통 없음; PowerShell 실행 정책·권한·Gallery trust 문제 시 가능
- GUI 필수: 없음
- 주 사용자: AI/개발자 둘 다
- 실제 설치/채택: **후보 — 설치 확인 안 됨**. 실제 상태는 `01_CONTROL/AI_INSTALLATIONS.md` 기준
- 설치·운영 부담: 🟢 가벼움
- Cloud/self-host: 로컬 PowerShell 모듈이라 구분 불필요
- 비용: **무료/오픈소스**
- 라이선스: MIT

## 공식 확인
- 공식 저장소: `PowerShell/PSScriptAnalyzer`
- 공식 PowerShell Gallery 최신 버전(2026-09-17 확인): **1.25.0**, 2026-03-20 게시
- 역할: PowerShell module/script에 규칙을 적용해 잠재 결함과 best-practice 위반을 DiagnosticResult로 보고
- 기본 실행: `Invoke-ScriptAnalyzer -Path <path>`
- 수정 기능도 있으나 `-Fix`는 파일을 바꿀 수 있으므로 기본 자동 운용은 검사 우선

## 권장 운용
1. `Invoke-ScriptAnalyzer`로 먼저 읽기 전용 검사
2. 결과를 보고 필요한 코드만 수정
3. `git diff` 확인
4. 다시 `Invoke-ScriptAnalyzer`
5. 자동 `-Fix`는 변경 범위를 이해한 경우에만 사용

## 왜 현재 작업에 유용한가
이 저장소/사용자 작업에는 Windows BAT·PowerShell 빌드/설치/원격 연결 스크립트가 자주 등장한다. ShellCheck는 Bash/sh 전용이므로 PowerShell 검증 공백을 PSScriptAnalyzer가 메운다.

역할 분리:
- `ShellCheck` → Bash/sh
- `PSScriptAnalyzer` → PowerShell
- `actionlint` → GitHub Actions workflow
- `Ruff` → Python

## 주의
- `Invoke-ScriptAnalyzer -Fix`는 자동 변경이므로 무조건 적용하지 않는다.
- custom rule/settings는 외부 코드를 포함하거나 동작 범위를 바꿀 수 있으므로 출처 불명 설정은 먼저 검토한다.
- `PSUseCompatibleCommands`용 compatibility profile이 최신 Windows/PowerShell 조합을 충분히 반영하지 못한다는 2026년 공개 이슈가 있으므로, 해당 규칙 결과를 최신 플랫폼 호환성의 절대 판정으로 취급하지 않는다.
- 모듈 자체가 무료라는 것과 분석 대상 스크립트가 호출하는 외부 API/프로그램의 비용·안전성은 별개다.

## 검증 기준일
2026-09-17
