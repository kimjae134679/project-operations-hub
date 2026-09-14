# Windows Terminal Rich Progress Pattern

확인일: **2026-09-15**

Windows 빌드/설치 스크립트의 진행 화면을 구형 CMD 로그처럼 보이지 않게 만드는 공통 패턴입니다.
핵심은 GUI ProgressBar가 아니라 **터미널 한 줄을 다시 그리는 Rich 스타일 진행 바**입니다.

## 채택할 핵심

```text
2/3 [package-name]  ━━━━━━━━━━━━━━━━────  73%  42.8 MB/s  ETA 00:18
```

- 기본 막대는 Unicode `━`(U+2501) 사용
- 진행 구간은 ANSI/VT 색상 적용
- `\r` 또는 `ESC[2K + \r`로 같은 줄 갱신
- 완료량/전체량, 퍼센트, 속도, ETA를 상황에 맞게 표시
- 여러 항목 설치 시 `2/3 [패키지명]` 같이 현재 단계 표시
- 터미널 폭은 고정값보다 현재 폭을 읽어 bar 길이를 조절
- 갱신 빈도는 대략 8~20 FPS로 제한해 깜빡임/CPU 낭비 방지

## subprocess 규칙

`subprocess.run(cmd)` 기본 동작처럼 stdout/stderr를 현재 터미널에 상속합니다.
`stdout=PIPE`, `stderr=PIPE`로 숨긴 뒤 마지막에 몰아서 출력하는 방식은 기본값으로 쓰지 않습니다.

단, **부모 진행 바와 자식 프로세스 출력이 동시에 같은 줄을 쓰면 화면이 깨집니다.**
자식이 직접 로그/진행률을 출력하는 동안 부모 Live bar를 잠시 멈추고, 자식 종료 후 다시 그리는 방식으로 처리합니다.

## pip 작업

pip가 자체 Rich progress를 제공하면 그것을 살립니다.
필요하면 TTY 환경에서 `--progress-bar on`을 사용합니다.
부모 bar를 억지로 덮어씌우지 않습니다.

## fallback

다음 상황에서는 Unicode/ANSI bar를 강제하지 않습니다.

- stdout이 TTY가 아님
- `TERM=dumb`
- ANSI/VT 미지원 환경
- 글꼴/인코딩 문제로 `━`를 안정적으로 표시하지 못함

이때는 ASCII `====----` bar 또는 일반 단계 로그로 내려갑니다.
`NO_COLOR`가 설정되어 있으면 색상만 끄고 bar 자체는 유지할 수 있습니다.

## 정확도 관련

- 속도/ETA는 `time.monotonic()` 기반으로 계산
- 파일 다운로드는 bytes/s, 항목 처리라면 items/s처럼 **실제로 측정 가능한 단위** 사용
- 전송이 없는 compile/install 단계에 가짜 MB/s를 표시하지 않음
- 남은 시간이 계산 불가능하면 `ETA --:--` 또는 해당 필드 생략
- 짧아진 문자열의 잔상이 남지 않도록 줄 지우기 후 다시 그림

## Windows Terminal 실행

이미 Windows Terminal 안이면 현재 shell을 그대로 사용하고 새 창을 중첩해서 띄우지 않습니다.
별도 launcher가 필요할 때만 `wt.exe`로 cmd.exe/PowerShell을 엽니다.
Windows Terminal이 없거나 실행할 수 없으면 기존 console + fallback 출력을 허용합니다.

## 2026-09-15 실제 PC 확인

사용자 PC의 Python 3.13.1에서 `━`가 UTF-8 `E2 94 81`로 정상 기록되고 ANSI escape sequence도 손실 없이 생성되는 것을 확인했습니다.
Remote Desktop 서비스 세션에서는 Windows Terminal package/`wt.exe`를 확인하지 못해 **실제 Windows Terminal 화면 렌더링 자체는 이번 검사에서 미검증**입니다.
따라서 Unicode/ANSI 생성 로직은 PASS, Windows Terminal 시각 렌더링은 별도 실제 창 확인 대상입니다.
