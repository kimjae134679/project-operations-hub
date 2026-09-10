# Known-Good Guide

Known-Good는 단순히 "예전에 됐음"이 아닙니다.

권장 한 줄 형식:

```text
<version/commit/artifact> — <환경> — <확인된 경계> — <미검증 경계> — <날짜>
```

예:
```text
APK 1.2.3 / sha256:... — Galaxy S22 — 로그인 PASS, 방 생성 PASS — 2대 동시 게임 미검증 — 2026-09-09
```

새 변경이 Known-Good에 영향을 주면 영향받는 검증만 다시 수행합니다. 과거 PASS를 새 HEAD에 자동 상속하지 않습니다.
