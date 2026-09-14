# Reusable Patterns

## UI
- 현재 상태 → 다음 행동 → 결과 → 상세
- strong primary CTA 하나
- 모바일 기능 삭제 대신 layout/scroll
- 눈에 보이는 성공 토스트 절제

## 데이터
- Unknown != Zero
- Source of Truth 하나
- user data와 app binary 분리
- schema change는 migration/restore 검증

## 빌드/배포
- environment → static/test → build/package → install/run → final artifact
- CI PASS != physical device PASS
- helper READY != process spawned
- 실제 최종 파일 기준 hash/서명/버전 검증

## Windows
- UTF-8
- 한글 IME
- 한글/공백 경로
- 취약한 toolchain은 임시 ASCII path로만 우회

## Windows 터미널 진행 표시
- Windows 빌드/설치 진행은 가능하면 Unicode `━` + ANSI/VT + 같은 줄 redraw 형태의 terminal progress bar 사용
- 항목 진행은 `2/3 [package]`, 측정 가능한 경우 percent/rate/ETA 표시
- child subprocess가 직접 출력할 때는 부모 bar를 suspend해서 출력 충돌 방지
- 비TTY/ANSI·Unicode 미지원 환경은 ASCII bar 또는 일반 로그로 fallback
- 상세 기준: [`WINDOWS_TERMINAL_PROGRESS.md`](WINDOWS_TERMINAL_PROGRESS.md)
