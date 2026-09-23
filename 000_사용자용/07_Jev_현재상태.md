# Jev 현재 상태

2026-09-23 기준

`jev-router@0.2.0`은 PC에 설치되어 있지만 **사용 중인 도구로 채택된 것은 아닙니다.**

- 상태: 설치됨 / 비활성 / 미채택 / 실제 라우팅 미검증
- 용도: Codex/Claude 요청 앞에서 사용할 모델 등급을 고르는 라우터
- AI 운용: 도구 자체는 `1회 준비 후 가능` 예상이지만 실제 핵심 기능은 아직 미검증
- 사용자 도움: API key 발급, 유료 사용 승인, 지출 한도 설정이 필요할 수 있음
- 부담: 🟢 가벼움
- 비용: **유료** — 외부 API 실행비용이 발생하는 구조
- 현재 과금: API key가 설정되지 않아 없음
- 기본 정책: 추가 결제 도구이므로 사용자가 명시적으로 요청하기 전에는 활성화·연결·추천하지 않음

## AI Control Tower에서의 현재 상태

AI Control Tower에는 Jev 입력/큐 UI가 존재하지만, 현재 사용자 OpenAI/ChatGPT 계정 사용 방침에 따라 `LocalExecutionPolicy`가 기본적으로 꺼져 있습니다.

따라서 현재는 다음 경로가 **하드 차단**되어 있습니다.

- Jev 직접 실행
- TXT 큐 수신 후 Jev 실행
- Jev 상태 명령 실행

즉 **설치되어 있다는 사실 ≠ 채택됨 ≠ 실행 가능**입니다. 사용자가 유료 API 사용과 정책 변경을 명시적으로 승인하기 전에는 이 차단을 풀거나 실제 라우팅 테스트를 진행하지 않습니다.

무료/오픈소스 또는 현재 요금제 안에서 해결할 수 있는 방법을 우선합니다.

상세 내부 기록: `03_KNOWLEDGE/CANDIDATE_CATALOG/JEV_ROUTER.md`, `01_CONTROL/AI_INSTALLATIONS.md`, `04_COMMUNICATION/threads/T-0009-ai-control-tower/THREAD.md`