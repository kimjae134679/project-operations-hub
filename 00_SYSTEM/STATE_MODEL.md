# State Model

상태는 증거 수준을 섞지 않습니다.

```text
ASSIGNED → EXEC → CLAIMED → GATED → ACCEPTED
```

필요한 경우 다음을 별도로 기록합니다.
- CODE COMPLETE
- STATIC/CI PASS
- BUILD/PACKAGE PASS
- INSTALL PASS
- REAL USER FLOW PASS
- PHYSICAL DEVICE PASS
- BLOCKED / NOT RUN / N/A

`CLAIMED != ACCEPTED`이며 새 변경에 과거 PASS를 자동 상속하지 않습니다.

Known-Good는 `버전/commit/산출물 + 어디까지 실제 확인했는지` 한 줄로 남기는 것을 권장합니다.
