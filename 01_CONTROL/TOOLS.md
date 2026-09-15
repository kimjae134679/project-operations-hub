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
| Supabase | PROJECT — chunkyack | DB/인증/프로젝트 연결. 비밀값은 문서화 금지 |
| Google Drive / OAuth / Android SDK / Blender 연동 등 | PROJECT when verified | 실제 프로젝트에서 확인된 경우만 등록 |

n8n bridge는 현재 `127.0.0.1:5678`에만 연결하며 외부 공개 endpoint로 취급하지 않습니다.
후보 도구는 `03_KNOWLEDGE/TOOL_CANDIDATES.md`에서 관리합니다.
