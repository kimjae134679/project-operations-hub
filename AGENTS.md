# AGENTS.md — Project Operations Hub

이 파일은 AI/Codex용 **진입 라우터**입니다. 정책과 상태를 중복 복제하지 않습니다.

## 최우선 구분

- `000_사용자용/` — 사용자가 직접 읽는 한글 요약 전용
- 그 밖의 `00_SYSTEM/`, `01_CONTROL/`, `02_PROJECTS/`, `03_KNOWLEDGE/`, `04_COMMUNICATION/`, `05_TEMPLATES/`, `99_ARCHIVE/`, `AGENTS.md` — AI/내부 관리 영역

사용자가 평소 내부 문서를 뒤져야 현재 상태를 알게 만들지 않습니다.

`000_사용자용/`의 파일명과 설명은 한국어를 기본으로 합니다. 제품명, 저장소명, 명령, 실제 경로처럼 원문 유지가 필요한 고유명사만 영어를 허용합니다.

## 읽는 순서

1. 현재 채팅의 사용자 최신 지시
2. `00_SYSTEM/GOVERNANCE.md`
3. `01_CONTROL/USER_POLICIES.md`
4. 관련 프로젝트의 `02_PROJECTS/<name>/README.md`
5. 실제 프로젝트 저장소의 README/AGENTS
6. 필요할 때만 `03_KNOWLEDGE/`와 `04_COMMUNICATION/`

`000_사용자용/`은 AI의 정책 원본이 아니라 **사용자에게 보여주는 동기화된 요약판**입니다.

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

## AI 도구/Plugin 후보 정리 규칙

Plugin, Skill, MCP, CLI, Agent, 자동화 도구를 조사할 때 단순히 `좋아 보임`으로 적지 않습니다. 가능한 한 다음을 분리합니다.

- 지금 바로 사용 가능한지
- 연결/OAuth만 필요한지, PC 설치가 필요한지
- AI가 사용자 도움 없이 설치·설정할 수 있는지
- 로그인/OAuth/UAC/API key/2FA/결제 등 사용자 1회 도움이 필요한지
- 사용자가 직접 GUI를 써야 하는 도구인지
- 주 사용자가 AI인지, 사용자 본인인지, 둘 다인지
- 실제 설치 여부와 실제 채택 여부
- 설치·운영 부담 `⚪/🟢/🟡/🟠/🔴`
- Cloud와 self-host의 부담 차이
- 비용·보안·권한·라이선스 주의점

내부 상세 기준은 `03_KNOWLEDGE/CANDIDATE_CATALOG/USAGE_AND_ASSISTANCE.md`, 설치 부담은 `INSTALLATION_BURDEN.md`를 봅니다. ComfyUI, vLLM, RAGFlow, 대형 로컬 모델처럼 무거운 도구는 가벼운 Plugin/Skill과 같은 종류의 “그냥 설치 후보”로 취급하지 않습니다.

## 문서 갱신

내부 원본을 갱신했으면 같은 작업 안에서 대응되는 사용자용 문서도 갱신합니다.

- 사용자 정책 변경 → `01_CONTROL/USER_POLICIES.md` + `000_사용자용/05_내_작업규칙.md`
- 도구 채택 변경 → `01_CONTROL/TOOLS.md` + `000_사용자용/02_설치_도구_현황.md`
- 실제 설치/버전/위치 변경 → `01_CONTROL/AI_INSTALLATIONS.md` + `000_사용자용/02_설치_도구_현황.md`
- 프로젝트 등록/위치 변경 → `01_CONTROL/PROJECT_REGISTRY.md` + 해당 `02_PROJECTS/` + `000_사용자용/01_프로젝트_현황.md`
- 후보 조사/설치 부담 변경 → `03_KNOWLEDGE/CANDIDATE_CATALOG/` + `000_사용자용/03_후보_도구_요약.md`
- Plugin/Skill/MCP/CLI/Agent의 사용 가능 여부·사용 주체·사용자 도움 조건 변경 → `03_KNOWLEDGE/CANDIDATE_CATALOG/USAGE_AND_ASSISTANCE.md` + `000_사용자용/06_AI_도구_플러그인_사용구분.md`
- 소통창구의 주요 열린 주제/구조 변경 → `04_COMMUNICATION/` + `000_사용자용/04_소통창구.md`
- 재사용이 실제로 확인된 패턴 → `03_KNOWLEDGE/`
- 세부 build/run/test/path → 실제 프로젝트 AGENTS

사용자용 문서에는 내부 구현 세부·긴 로그·AI만 필요한 절차를 복제하지 않습니다. 사용자가 판단하거나 확인할 상태만 짧게 보여줍니다.

## 대화 규칙

기존 메시지를 계속 편집해 덮어쓰지 말고 스레드 폴더 안에 번호가 증가하는 새 파일을 추가합니다. 정책을 바꾸는 결정은 채널에서 끝내지 말고 사용자 정책 원본에 반영합니다.

## AI 설치/작업 루트

- 새 AI 관리 프로그램·도구·실험 프로젝트는 기본적으로 `C:\Program Files\_My\AI` 아래에서 설치/작업합니다.
- 목적별 하위 폴더를 사용하고 루트에 파일을 흩어놓지 않습니다.
- npm/pip/winget/공식 업데이터처럼 기본 경로 유지가 더 안전하면 억지로 이동하지 말고 실제 위치와 예외 이유를 `01_CONTROL/AI_INSTALLATIONS.md`와 해당 인수인계에 남깁니다.
- 단순 이동이 안전한 기존 AI 설치물은 새 관리 루트로 옮기고 launcher/config/path를 함께 갱신한 뒤 실행 검증합니다.
- 새 프로젝트/인수인계 작성 시 설치·작업 위치를 반드시 명시합니다. 기존 안정 저장소는 사용자 지시 없이 일괄 이동하지 않습니다.
