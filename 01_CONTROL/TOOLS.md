# Tool Registry

## 상태
- `ACTIVE` — 허브 전반에서 채택
- `PROJECT` — 특정 프로젝트에서 채택
- `CANDIDATE` — 후보 보관
- `RETIRED` — 사용 중단

| 도구 | 상태 | 역할 |
|---|---|---|
| GitHub | ACTIVE | 저장소·문서·commit·PR·Actions |
| Remote Desktop Commander / Desktop Remote | ACTIVE | 실제 PC 파일·명령·빌드·실행·기기 검증 |
| ChatGPT Files / Library | ACTIVE | 현재/과거 첨부와 저장자료 회수 |
| n8n | ACTIVE — local | 로컬 자동화/워크플로. `AI Ops Hub - Local Task Bridge`로 localhost task receipt 검증 완료 |
| AI Control Tower | PROJECT — 0.2 workbench | 프로젝트·기능·프로그램 발견/등록·명령 실행·중지·실제 로그와 종료 코드·공유 리모트 시작 등록 관리. 2026-10-05 Windows 검증은 ai-control-tower/docs/VERIFICATION-20261005.md 참고. Jev는 선택 사용 설정이며 옛 텍스트 큐는 자동 실행하지 않음 |
| Supabase | PROJECT — chunkyack | DB/인증/프로젝트 연결. 비밀값은 문서화 금지 |
| Google Drive / OAuth / Android SDK / Blender 연동 등 | PROJECT when verified | 실제 프로젝트에서 확인된 경우만 등록 |

n8n bridge는 현재 `127.0.0.1:5678`에만 연결하며 외부 공개 endpoint로 취급하지 않습니다.

## 후보 도구 위치

후보 조사/비교의 canonical 위치는:

- `03_KNOWLEDGE/CANDIDATE_CATALOG/MASTER_INDEX.md` — 전체 후보 상태/설치부담/상세문서 연결
- `03_KNOWLEDGE/CANDIDATE_CATALOG/INSTALLATION_BURDEN.md` — 설치 무게 분류
- `03_KNOWLEDGE/CANDIDATE_CATALOG/` — 주제별 상세 조사

`03_KNOWLEDGE/TOOL_CANDIDATES.md`는 이전 장문 카탈로그의 과거 참고용입니다.

중요: `설치됨`과 `ACTIVE/PROJECT`는 같은 뜻이 아닙니다. 실제 설치 버전/경로는 `01_CONTROL/AI_INSTALLATIONS.md`에서 별도로 관리합니다.

## 추천과 현재 상태

- [작업별 선택과 과용 비용](../03_KNOWLEDGE/CANDIDATE_CATALOG/TOOL_SELECTION_PLAYBOOK.md)을 사용합니다. 과거 탐색을 고정 규칙으로 삼지 않고 최신 공식 자료와 실제 환경을 다시 비교합니다.
- Jev 사용은 허용되었습니다. 0.2.0 설치, Windows 사용자 환경의 키 존재, Codex ChatGPT 로그인 확인과 실제 라우팅 왕복은 별개입니다. 추가 결제는 이 허가에 포함되지 않습니다.
- 원격 연결의 여러 Node PID는 부모·자식일 수 있으므로 독립 연결 수와 구분합니다. n8n은 Node 이름 대신 실제 healthz를 확인합니다. GitHub Actions 서비스와 AI Ops 예약 작업을 혼동하지 않습니다.
