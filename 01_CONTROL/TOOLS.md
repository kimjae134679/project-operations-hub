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
| ProjectBridge / 프로젝트 연결 | PROJECT — 2.0 실제 연결 / 3.0 개편·재검증 | 여러 프로젝트·명령 주체별 작업 공간·기록·상태 분리와 공용 PC 작업. 기존 404 주소 수정·실제 왕복 확인. 최신 3.0 로컬 API·MCP·병렬 처리·실행 단계 분리와 앱 내부 작업 화면은 구현·재검증 중 |
| ChatGPT Files / Library | ACTIVE | 현재/과거 첨부와 저장자료 회수 |
| n8n | ACTIVE — local | 로컬 자동화/워크플로. `AI Ops Hub - Local Task Bridge`로 localhost task receipt 검증 완료 |
| AI Control Tower | PROJECT — 0.7.0 / 0.8.0 적용 중 | 프로젝트·기능·프로그램·공용 연결과 소통 관리. 0.8.0 소스는 새로고침 조작 없이 자동 확인, 글 작성자·안 읽음·게시글 이동과 공지 명단 검색. 실제 설치 버전과 새 소스 검증을 구분. Jev는 선택 사용, 옛 텍스트 큐는 자동 실행하지 않음 |
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

## 2026-10-06 관제탑 표시와 설치 안내

관제탑0.7.0의 도구·연결에는 등록된 상태11개를 모두 표시합니다. 설치됨은 실제 연결·업무 성공과 다릅니다. Remote Desktop Commander와 Jev 제품명을 유지하며 n8n·GPT 전달·Aider·HyperFrames도 숨기지 않습니다.

- [VoiceStudio 사용 안내](D:/A_KJ/AI/Applications/VoiceStudio/프로젝트_사용안내.md): D실행기, 기존 음원·배역·더빙작업·공유모델·환경 위치.
- [Zonos2 사용 안내](D:/A_KJ/AI/Applications/Zonos2/프로젝트_사용안내.md): 앱/CLI/server와 외부Q8 모델 위치. 기본 BAT의 q6 신규다운로드 가능성 때문에 자동 실행하지 않습니다.

## 공용 PC 연결 — 기존 연결 복구 / 새 통합 버전 적용 중

ProjectBridge 2.0은 실제 PC에 설치되어 있고, 저장소 API 주소 오류를 수정한 뒤 `KJW-80ea388278fd` 장치의 파일·명령·창 목록 왕복을 확인했습니다. 공용 연결 3.0과 관리 앱 0.8.0은 이전 범위 검사를 통과했으나 최신 주체별 분리·실행기 단계·앱 내부 화면 수정은 재검증합니다. 실제 새 버전 적용·자동 시작·화면 검증과 구분합니다.

3.0의 PC 안 도구는 같은 로컬 API 또는 MCP 어댑터에 연결하며 파일 작업마다 GitHub를 거치지 않습니다. 외부 GPT는 지원되는 인증된 비공개 GitHub 중계를 사용합니다. 프로젝트와 명령 주체별 작업 공간·기록·상태를 분리하고 독립 작업은 병렬, 실제 겹치는 파일·공유 자원·프로세스·화면은 순서 조정합니다. 작업·상태·결과는 앱 내부에서 확인하고 실행 때문에 별도 창을 띄우거나 포커스를 가져가지 않습니다. 상세 요청·출력·화면은 비공개로, 통합소통에는 공개 요약만 남깁니다. 기존 제작 큐는 보존합니다. [설치·동작·검증](../04_COMMUNICATION/remote-bridge/README.md).

2026-10-07 추가 지시: 사용자 입구는 실행 파일·안내로 정리하고 보조 앱·CMD·PowerShell을 보이게 띄우지 않습니다. 내부 실행 자료는 Runtime 하위로 묶는 개편 중이며 소유한 검사·빌드·로그·배포 준비물은 `D:\A_KJ\AI\Workspace\ControlTower\shared-pc-20261006`에 모읍니다. 최신 관리 앱 로컬175/175 검사와 실제 PC 설치·화면·자동 시작 성공은 별도로 확인합니다. 기존 자료·설정·백업은 보존합니다.
