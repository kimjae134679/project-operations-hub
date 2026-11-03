# 도구 등록부

기준 **2026-11-03 KST**. 설치 버전·경로·검사 증거는 [AI_INSTALLATIONS](AI_INSTALLATIONS.md)가 원본입니다. 등록·설치·현재 연결·실제 업무 성공은 별개입니다.

## 현재 유지할 핵심

| 도구 | 채택 범위 | 운용 기준 |
|---|---|---|
| Codex | ACTIVE · 코딩 | 기존 공식 환경/ChatGPT 로그인 재사용. npm CLI 0.160.1 유지, 이번 업그레이드 없음. 한도와 별도 유료 API 과금 구분 |
| GitHub | ACTIVE · 원본/협업 | 저장소·commit·Issue·PR·Actions. 현재 계정 권한·원격 반영 성공은 작업별 확인 |
| ProjectBridge / 프로젝트 연결 | PROJECT · PC 연결 | 최신 사용자 원격 경로. 현재 장치 응답으로 연결·파일·명령을 확인; 과거 버전/왕복을 현재 정상으로 복제하지 않음 |
| AI Control Tower | PROJECT · 프로젝트 운영 | 설치 앱 0.9.18.0 보존. 개발 소스·설치 버전·실행을 구분하며 낮은 버전으로 덮지 않음 |
| Engram · 현재 AGENTS/Skill | ACTIVE · 기억/기준 | 기존 구성 유지. 중복 기억 서버·대형 harness 추가 안 함 |
| 현재 파일/문서/브라우저 도구 | 작업별 | 현재 세션 도구·실제 파일/계정 권한 범위만 사용 |
| n8n | PROJECT · 기존 로컬 흐름 | 실제 반복 workflow에 한정. 과거 localhost bridge 검증 ≠ 현재 health/workflow 성공 |
| Supabase · Drive/OAuth · SDK/Blender 연동 | PROJECT · 대상별 | 실제 해당 프로젝트에서 필요·연결이 확인된 경우만 |

## 후속 지시로 실제 추가한 사진 속 도구

사용자의 후속 **설치 지시**를 반영했습니다. 이전 선별 조사에서의 “Replica 참고만/Apify 미연결/OpenMaus 미도입”은 이 상태로 대체합니다.

| 도구 | 설치/채택 범위 | 실제 검증과 남은 일 |
|---|---|---|
| Replica 11개 Skill | 설치됨 · 앱 재구현 작업별 선택 | Codex enabled 인식, 도우미 검사18/18. 기존 Skill/hook 덮지 않음; 모델/복제 E2E 미실행 |
| agent-desktop-windows Skill | 설치됨 · Windows CLI 지원 | Codex enabled 인식, 기존1.0.2 지침/버전 확인. 실제 화면 제어 미실행 |
| Apify MCP | 설치됨 · `apify-docs` 문서 모드 채택/등록 | 토큰 없는2도구 초기화/목록·공식 문서6,456자 조회. Actor·데이터셋·유료 호출0; 인증 수집 모드는 비활성 |
| OpenMausBot 0.1.138 | 실제 portable 설치 · 다음 정상 수동 시작 설정 | D 데이터/기존 Codex/gpt-6.1-sol 설정·수동 입구 준비. 정상 앱/로그인/모델 응답 미검증; 자동 실행·cloud 연결 없음 |

Replica 11개 + Windows 1개은 `skills/list`에서12개 enabled/오류0을 확인했습니다. 새 턴/세션 갱신 후 선택하며 설치가 다음 작업·배포·실결제·화면 제어의 자동 허가가 되지 않습니다. 안전하지 않은 기본 실결제/보안 우회 조언은 제한·제거했습니다. [실제 실행 안내](../000_사용자용/02_프로그램/사진도구_설치와_실행.md).

## 작은 실무 보조

기존 `rg/fd/gh/uv/Gitleaks CLI`를 우선 재사용합니다. `jq/Ruff/PSScriptAnalyzer`는 JSON/Python/PowerShell의 빈틈을 보완합니다. 실제 설치·제한된 검사 결과는 설치 원본에서 확인하며 이 문서만으로 전부 READY라고 하지 않습니다.

`agent-desktop`은 선택형 수동 UIA CLI이며 원격 연결 대체품이 아닙니다. help/version 검사는 실제 화면 조작 검증이 아닙니다. 잠금/UAC/서비스 세션·최소화 앱 미지원과 headed 입력 제한은 [공식 근거](../03_KNOWLEDGE/CANDIDATE_CATALOG/03_콘텐츠와_자동화/SCREENSHOT_TOOLS_20261103.md)를 따릅니다. 이번 실화면 probe·새 autostart는 없습니다.

## 현재 기본 운용에서 제외

- Remote Desktop Commander는 과거 설치·연결 기록만 보존합니다. 최신 지정은 **ProjectBridge 우선/이번 작업 RDC 미사용**이며 과거 ACTIVE를 현재 허가로 쓰지 않습니다.
- Jev 0.2.0 설치·키 존재 기록은 보존하되 인증·라우팅·절감은 미검증. 필수 실행기가 아니며 새 유료 호출 안 함. 옛 `--approve-for-me`/2분 예약 실행기는 활성화하지 않습니다.
- ComfyUI는 사용자 요청으로 현재 후보·추천·설치 목록에서 제거했습니다. **PC 앱·모델·작업·데이터 삭제 지시가 아닙니다.**
- Bytebot는 공식 보관 저장소로 신규 도입 제외. 중복 기억/Agent/대형 서버는 [비추천 과거 참고](../03_KNOWLEDGE/CANDIDATE_CATALOG/05_도입기준/전체_도구목록.md)로만 남깁니다.

[사용 순서](../000_사용자용/03_참고도구/AI_도구와_플러그인.md) · [설치 현황](../000_사용자용/02_프로그램/설치_도구_현황.md)
