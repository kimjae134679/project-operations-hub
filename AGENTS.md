# AGENTS.md — Project Operations Hub

이 파일은 AI/Codex용 **진입 라우터**입니다. 정책과 상태를 중복 복제하지 않습니다.

## 읽는 순서

1. 현재 채팅의 사용자 최신 지시
2. `00_SYSTEM/GOVERNANCE.md`
3. `01_CONTROL/USER_POLICIES.md`
4. 관련 프로젝트의 `02_PROJECTS/<name>/README.md`
5. 실제 프로젝트 저장소의 README/AGENTS
6. 필요할 때만 `03_KNOWLEDGE/`와 `04_COMMUNICATION/`

## 실행 원칙

- 사용자 정책이 기술 편의보다 우선합니다.
- 확인하지 않은 경로·버전·기기 상태·성공을 추측하지 않습니다.
- 작은 수정으로 요구사항을 모두 만족시키고 Known-Good를 보호합니다.
- 검증은 변경 영향에 맞게 선택합니다. 문서 작업에 앱 빌드를 강제하지 않습니다.
- GitHub 상태와 실제 PC/실기기 상태를 분리합니다.
- Remote Desktop 연결은 사용자가 켜둔 경우 임의 종료하지 않습니다.
- 비밀값은 GitHub/문서/채널에 기록하지 않습니다.
- AGENTS는 현재 실행본입니다. stale/duplicate/temp 규칙은 정리하고 Git/Archive에서 과거를 찾습니다.

## 문서 갱신

- 사용자 정책 변경 → `01_CONTROL/USER_POLICIES.md`
- 도구 채택 변경 → `01_CONTROL/TOOLS.md`
- 프로젝트 등록/위치 변경 → `01_CONTROL/PROJECT_REGISTRY.md` + 해당 `02_PROJECTS/`
- 재사용이 실제로 확인된 패턴 → `03_KNOWLEDGE/`
- 다른 프로젝트와 나눌 대화 → `04_COMMUNICATION/threads/`에 새 메시지 파일
- 세부 build/run/test/path → 실제 프로젝트 AGENTS

## 대화 규칙

기존 메시지를 계속 편집해 덮어쓰지 말고 스레드 폴더 안에 번호가 증가하는 새 파일을 추가합니다. 정책을 바꾸는 결정은 채널에서 끝내지 말고 사용자 정책 원본에 반영합니다.
