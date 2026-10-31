# shfmt

- 분류: Shell formatter / CLI
- 공식 프로젝트: `mvdan/sh`의 `shfmt`
- 현재 바로 사용: 가능(설치 후)
- ChatGPT 연결 서비스/설치 불필요: 아니오
- AI 설치·설정: 가능
- 핵심 기능 AI 단독 운용: **AI 단독 가능**
- 사용자 1회 도움: 일반적으로 없음. 환경의 실행/설치 권한 제한 시에만 필요
- 사용자 GUI 필요: 없음
- 주 사용자: AI/사용자 둘 다
- 실제 설치/채택: 후보. 설치 확인 전에는 설치됨으로 올리지 않음
- 부담: 🟢 가벼움
- Cloud/self-host: 해당 없음(로컬 CLI)
- 비용: **무료/오픈소스**
- 라이선스: BSD-3-Clause

## 역할

Bash/POSIX sh/mksh/Bats 등 shell script를 일관된 형식으로 포맷한다. `ShellCheck`가 오류·위험 패턴을 찾는 정적 분석기라면 `shfmt`는 포맷터라 역할이 겹치지 않는다.

권장 흐름:

```text
shfmt -d .
→ diff 확인
→ 필요한 경우 shfmt -w ...
→ ShellCheck
→ git diff
```

`-d`는 파일을 바로 바꾸지 않고 포맷 차이를 보여주므로 AI 자동 작업의 기본 진입점으로 적합하다. `-w`는 실제 파일을 덮어쓰므로 기본 자동 실행보다 diff 확인 뒤 사용하는 편이 안전하다.

## 설치/운영

공식 README는 `go install mvdan.cc/sh/v3/cmd/shfmt@latest`를 안내하며 Alpine, Arch, Debian, Docker, Fedora, FreeBSD, Homebrew, MacPorts, NixOS, OpenSUSE, Scoop, Snapcraft, Void, webi 등의 패키지도 안내한다. Windows에서도 Scoop 등으로 설치 가능하다.

## 주의

- 포맷 변경은 대량 diff를 만들 수 있으므로 기존 프로젝트에 처음 적용할 때 전체 저장소 `-w`부터 실행하지 않는다.
- `.editorconfig`에 shfmt 관련 설정이 있으면 저장소 스타일을 먼저 따른다.
- formatter는 lint/security 검사를 대체하지 않는다. ShellCheck/Gitleaks 등은 별도로 유지한다.
- 외부 패키징 래퍼보다 공식 `mvdan/sh` 배포/문서를 우선한다.

## 비용/보안

프로그램 자체는 BSD-3-Clause 무료 오픈소스이며 API key, OAuth, 계정, 클라우드 사용료가 필요하지 않는다. 외부 API/모델 실행비도 없다.

## 검증 메모

2026-09-17 기준 공식 `mvdan/sh` README와 최근 프로젝트 활동을 확인했다. 최근 2026년에도 parser/formatter 관련 개발과 수정이 이어지고 있다.
