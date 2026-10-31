# ShellCheck — shell script 정적 분석 후보

## 판단

- 현재 바로 쓸 수 있는지: `INSTALL` — 로컬 CLI 설치 후 즉시 사용
- ChatGPT/연결 서비스라 설치 없이 쓸 수 있는지: 아니오. 공식 웹 검사기는 있으나 저장소 자동 작업에는 로컬 CLI가 적합
- AI가 사용자 도움 없이 설치·설정 가능한지: 대체로 가능. Windows는 `winget install --id koalaman.shellcheck` 계열 패키지 또는 공식 배포본 검토
- 핵심 기능 AI 단독 운용: `AI 단독 가능`
- 사용자 1회 도움: 일반적으로 없음. 단, Windows에서 unsigned binary 경고/UAC가 발생하면 사용자 확인이 필요할 수 있음
- GUI 필수: 없음
- 주 사용자: `AI / BOTH`
- 실제 설치/채택: `CANDIDATE`, 설치 여부는 `01_CONTROL/AI_INSTALLATIONS.md`가 원본
- 설치·운영 부담: `🟢 가벼움`
- Cloud/self-host: 로컬 CLI가 기본. 별도 서버 불필요
- 비용 상태: `무료/오픈소스`
- 라이선스: GPL-3.0

## 용도

Bash/sh 스크립트의 quoting, globbing, 변수 사용, 조건식, portability 등 흔한 오류와 미묘한 실패 가능성을 실행 전에 정적 분석한다. BAT/PowerShell 전용 검사기가 아니므로 Windows 프로젝트에서도 `.sh`, Git Bash, WSL, CI shell script가 있을 때 적용한다.

권장 흐름:

```text
shell script 수정
→ shellcheck 대상.sh
→ 경고 원인 확인
→ 필요한 수정
→ 재검사
→ diff 확인
```

`pre-commit`과 결합할 수 있지만 처음부터 모든 저장소에 강제하지 않는다. shell script가 실제로 존재하거나 CI에서 Bash를 많이 쓰는 프로젝트부터 적용한다.

## 보안/운영 주의

- ShellCheck 자체는 스크립트를 실행하지 않는 정적 분석 도구라 AI 자동 검증에 잘 맞는다.
- 경고 suppression을 무조건 추가하지 말고 원인을 먼저 확인한다.
- Windows 공식/패키지 배포 경로를 우선하고 출처 불명 binary를 사용하지 않는다.
- 2026-07 GitHub issue에서 precompiled binary의 디지털 서명 부재로 Windows Defender가 `Unknown Publisher`를 표시할 수 있다는 문제가 제기되어 있다. 이는 분석 기능 문제와 별개지만 자동 설치 시 사용자 개입 가능성으로 기록한다.

## 공식 확인

- 공식 사이트: `shellcheck.net`
- 공식 저장소: `koalaman/shellcheck`
- 공식 사이트 기준 GPLv3 무료 오픈소스
- 현재 확인된 안정 버전 계열: `0.11.0`

마지막 확인: **2026-09-17**
