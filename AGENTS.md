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
- **Remote Desktop Commander/데스크톱 원격을 사용한 작업은 종료 전에 그 회차가 만든 임시 빌드 산출물·중간 APK/ZIP/IDSIG·cache/probe·`__pycache__`·불필요 로그를 정리하고, 최종 산출물과 재현에 필요한 소스/스크립트만 남기는 것까지 완료 조건으로 봅니다.** 사용자 데이터·Known-Good·원본/reference 자료는 임의 삭제하지 않습니다.
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

## AI 설치/작업 루트
- 새 AI 관리 프로그램·도구·실험 프로젝트는 기본적으로 `C:\Program Files\_My\AI` 아래에서 설치/작업합니다.
- 목적별 하위 폴더를 사용하고 루트에 파일을 흩어놓지 않습니다.
- npm/pip/winget/공식 업데이터처럼 기본 경로 유지가 더 안전하면 억지로 이동하지 말고 실제 위치와 예외 이유를 `01_CONTROL/AI_INSTALLATIONS.md`와 해당 인수인계에 남깁니다.
- 단순 이동이 안전한 기존 AI 설치물은 새 관리 루트로 옮기고 launcher/config/path를 함께 갱신한 뒤 실행 검증합니다.
- 새 프로젝트/인수인계 작성 시 설치·작업 위치를 반드시 명시합니다. 기존 안정 저장소는 사용자 지시 없이 일괄 이동하지 않습니다.
