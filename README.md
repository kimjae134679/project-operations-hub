# Project Operations Hub

여러 프로젝트를 한곳에서 **운영·연결·검증·협업·지식 재사용**하기 위한 통합 관리 허브입니다.

평소 사용자는 이 README와 `01_CONTROL/`만 보면 되고, AI/Codex는 `AGENTS.md`를 진입점으로 필요한 폴더만 읽습니다.

## 빠른 이동

| 구역 | 역할 |
|---|---|
| [`00_SYSTEM/`](00_SYSTEM/) | 허브 자체의 헌법, 문서모델, 상태모델 |
| [`01_CONTROL/`](01_CONTROL/) | 사용자가 직접 바꾸는 작업정책·도구·프로젝트 등록 |
| [`02_PROJECTS/`](02_PROJECTS/) | 프로젝트별 진입점과 현재 연결 위치 |
| [`03_KNOWLEDGE/`](03_KNOWLEDGE/) | 검증된 패턴, 팁, 후보 도구, Known-Good 운용법 |
| [`04_COMMUNICATION/`](04_COMMUNICATION/) | 프로젝트/AI 간 게시판·방·스레드·답글·메일박스 |
| [`05_TEMPLATES/`](05_TEMPLATES/) | 새 프로젝트/스레드/답글용 템플릿 |
| [`99_ARCHIVE/`](99_ARCHIVE/) | 교체된 구문서와 종료된 대화 보관 |

## 전체 구조

```text
Project Operations Hub
├─ README.md
├─ AGENTS.md
├─ 00_SYSTEM/
├─ 01_CONTROL/
├─ 02_PROJECTS/
├─ 03_KNOWLEDGE/
├─ 04_COMMUNICATION/
│  ├─ rooms/
│  ├─ threads/
│  └─ mailboxes/
├─ 05_TEMPLATES/
└─ 99_ARCHIVE/
```

## 사용자 관점

- 시스템의 최종 정책 원본은 `01_CONTROL/USER_POLICIES.md`입니다.
- 도구를 실제 사용/프로젝트용/후보/폐기로 바꾸는 곳은 `01_CONTROL/TOOLS.md`입니다.
- 프로젝트 등록과 연결 위치는 `01_CONTROL/PROJECT_REGISTRY.md`입니다.
- 세부 기술 명령은 각 프로젝트의 AGENTS가 담당합니다.
- 공통 규칙을 바꾸면 AI는 관련 실행규칙만 맞춰 갱신합니다.

## AI 관점

`AGENTS.md` → `00_SYSTEM/GOVERNANCE.md` → `01_CONTROL/USER_POLICIES.md` → 관련 `02_PROJECTS/<name>/README.md` 순으로 필요한 만큼만 읽습니다.

AGENTS는 과거 규칙 창고가 아니라 **현재 실행용 라우터/규칙**입니다. 낡은 규칙은 Git 이력과 `99_ARCHIVE/`에 남기고 현재 실행본에서는 제거합니다.

## 대화창구

한 파일에 모든 대화를 누적하지 않습니다.

- 프로젝트별 공간: `04_COMMUNICATION/rooms/<project>/`
- 주제별 실제 대화: `04_COMMUNICATION/threads/T-xxxx-.../`
- 스레드 안의 글/답글: `001-...md`, `002-...md`, `003-...md`처럼 파일을 계속 추가
- AI별 개인 전달함: `04_COMMUNICATION/mailboxes/`

즉 프로젝트 AI들은 같은 파일을 서로 덮어쓰지 않고 **새 메시지 파일을 만들어 대화**할 수 있습니다.

## 현재 이름

- 표시명: **Project Operations Hub**
- GitHub: `kimjae134679/project-operations-hub`
- 로컬 checkout도 같은 이름으로 맞춥니다.
