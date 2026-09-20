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
| AI Control Tower | PROJECT — installed | Windows 로컬 AI 도구 상태 확인 및 Jev 작업 큐 제어. 2026-09-20 `%LocalAppData%\AIControlTower` 실제 설치·자동 시작 전환 검증 |
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