# OpenAI Agents API

검증일: 2026-09-19
상태: 후보 기록만 / 미채택 / 자동 연결 금지

## 무엇인가

OpenAI가 2026-09-10 public beta로 공개한 cloud agent API. Codex를 구동하는 agent harness/infrastructure를 API로 제공하며, 장기 실행·도구 사용·subagent 조정·파일/코드 작업·중간 결과 저장을 지원한다. 실행 환경은 OpenAI-managed sandbox, 자체 infrastructure, sandbox partner 중 선택할 수 있다.

공식 소개: https://openai.com/index/introducing-the-agents-api/
공식 API 가격: https://developers.openai.com/api/docs/pricing

## 사용자 기준 판정

| 축 | 판정 |
|---|---|
| ① 현재 바로 쓸 수 있는지 | API 자체는 public beta. 이 저장소에서는 미채택 |
| ② 설치 없이 쓸 수 있는 연결 서비스인지 | Cloud API라 로컬 대형 설치는 불필요하지만 API 연동 코드는 필요 |
| ③ AI가 사용자 도움 없이 설치·설정 가능한지 | 아니오. API credential/결제 계정 준비가 필요 |
| ④ 핵심 기능 AI 단독 운용 | `1회 준비 후 가능`에 가깝지만 현재 미채택 |
| ⑤ 사용자 1회 도움 | API key/계정·과금 승인 필요 |
| ⑥ 사용자 GUI 필수 | 핵심 운용은 API 기반이라 GUI 필수 아님 |
| ⑦ 주 사용자 | AI/개발자 둘 다, 특히 agent backend 개발 |
| ⑧ 실제 설치/채택 | 미채택 |
| ⑨ 설치·운영 부담 | Cloud: 🟡 보통. 자체 infrastructure 선택 시 환경에 따라 🟠 이상 가능 |
| ⑩ Cloud/self-host | Cloud harness는 관리형. 자체 compute 선택 시 별도 운영 부담 발생 |
| ⑪ 보안/권한 | API key·secret 관리, agent가 접근할 tool/file/network 권한 최소화 필요 |
| ⑫ 비용 상태 | **유료**. 모델/API 사용량 과금. 프로그램 무료 후보로 취급하지 않음 |

## 비용 정책상 결론

현재 사용자 정책에서는 추가 결제가 필요한 유료 도구를 기본 추천/설치/연결하지 않는다. 따라서 Agents API는 **기술 동향/향후 아키텍처 참고 후보**로만 기록한다. 사용자가 명시적으로 API 비용 사용을 요청하기 전에는 key 생성·연결·테스트 호출을 하지 않는다.

OpenAI 공식 API pricing에는 현재 모델별 token 과금이 명시되어 있으므로 ChatGPT 구독에 포함된 무료 기능으로 간주하지 않는다. Agents API 소개 페이지 자체에서 별도 무료 플랜을 확인하지 못했으므로 무료로 추정하지 않는다.

## 기존 체계와의 관계

- ChatGPT Work / 현재 연결 도구: 이미 사용할 수 있는 작업 환경. 별도 Agents API 비용 없이 가능한 작업은 우선 기존 경로 사용.
- Codex/Skills/AGENTS.md: 저장소 내부 반복 작업 규칙과 재현성에 우선 활용.
- Agents API: 별도 제품/서비스에서 장기 실행 cloud agent backend를 직접 구축해야 할 때만 검토.

즉 현재는 `좋아 보이니 연결` 대상이 아니라, **향후 자체 agent 서비스가 필요해질 때 다시 비용·보안·실행환경을 비교할 후보**다.
