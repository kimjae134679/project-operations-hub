# Astra Codex Workbench

여러 프로젝트의 작업 방식과 재사용 경험을 관리하는 사용자 컨트롤센터입니다.
평소에는 이 README에서 정책을 수정하고, 프로젝트별 진행 상황은 아래 링크로 확인합니다.

| 파일 | 역할 |
|---|---|
| [README.md](README.md) | 사용자 정책 원본, 도구 채택 상태, 프로젝트 안내, 재사용 팁 |
| [AGENTS.md](AGENTS.md) | 작업 시작 방법, 조건별 기술 절차, 검증·기록 방법 |
| [PROJECT_CHANNEL/CHANNEL.md](PROJECT_CHANNEL/CHANNEL.md) | 필요한 프로젝트 간 질문과 처리 결과 |
| [대화창구 사용법](PROJECT_CHANNEL/README.md) | 게시판과 임시 직접 전달문의 사용법 |

## 0. 기반 원칙

이 원칙은 사용자가 바꿀 수 있습니다. AI는 편의를 이유로 의미를 바꾸지 않습니다.

- 목적은 새 채팅에서도 실제 작업을 이어가는 것입니다. 문서 관리는 이를 돕는 만큼만 합니다.
- 프로젝트의 기본 관리 문서는 `README.md`와 `AGENTS.md` 두 개로 유지합니다. PLAN/STATUS/NOTES/HANDOFF 등을 습관적으로 늘리지 않습니다.
- 소스·에셋·테스트·빌드 스크립트 등 실제 프로그램에 필요한 파일은 유지합니다. 사용자가 요청한 공용 `PROJECT_CHANNEL/`은 별도 허용합니다.
- 사용자 정책은 README 한 곳에 두고, AGENTS는 그 정책을 적용하는 기술 정보를 담습니다. 정책·도구 목록·프로젝트 목록을 양쪽에 복제하지 않습니다.
- 현재 채팅의 최신 사용자 지시가 우선합니다. 프로젝트 문서끼리는 **프로젝트 README → Workbench README → 프로젝트 AGENTS → Workbench AGENTS → 채널 제안** 순서로 판단합니다.
- 이 순서는 프로젝트 문서 간 충돌 해결 기준이며, 실행 환경의 상위 지침이나 권한 제한을 바꾸지 않습니다.
- 작은 구현을 선호해도 요구사항, 필요한 QA, 보안, 데이터 보존, migration·rollback, 오류 처리를 줄이지 않습니다.
- 경로·버전·저장소·기기 상태를 추측하지 않습니다. 과거 파일을 최신 소스와 같다고 가정하지 않습니다.
- 비밀번호, token, private key, OAuth secret, keystore password는 문서·로그·GitHub·채널에 기록하지 않습니다. 필요한 권한·환경변수 **이름**·승인된 보관 위치만 기록합니다.

### 문서 갱신 시점

**필요한 규칙 확인 → 실제 작업 → 바뀐 사실 기록**을 기본 흐름으로 합니다.

- README 변경 중 현재 작업에 영향을 주는 충돌만 작업 전에 해결합니다. 문서 전체 정리나 AGENTS 재생성을 매번 선행하지 않습니다.
- 사용자에게 중요한 정책·준비조건·상태 변화는 README, 재현 가능한 명령·경로·환경·검증은 해당 프로젝트 AGENTS에 기록합니다.
- 현재도 필요한 명령, 권한, 정상 동작 기준판(Known-Good), 재현되는 실패 조건은 보존합니다. 해결된 임시 우회법과 중복·오래된 내용은 정리합니다.
- AGENTS 전체 재작성은 필요한 경우에만 합니다. 재작성 전 보존할 실행 정보를 확인하고, 삭제한 내용은 Git 이력 또는 버전 이력으로 복구할 수 있게 합니다.
- 새롭고 재사용 가치가 확인된 팁이 있을 때만 추가합니다. 작업마다 팁 개수를 채우지 않습니다.

## 1. 사용자 작업 정책

### 작업 범위와 권한

- 요청한 일은 끝까지 처리하고 부분 구현을 완료라고 부르지 않습니다.
- 정상 동작하는 기능과 Known-Good를 보호합니다. 전체 재작성보다 작은 검증된 수정을 우선합니다.
- 기존 코드, 플랫폼 기본 기능, 표준 라이브러리, 설치된 의존성을 먼저 검토합니다.
- placeholder·가짜 데이터·가짜 성공으로 미구현이나 실패를 숨기지 않습니다.
- 되돌릴 수 있고 위험이 작은 애매함은 합리적인 기본값으로 진행합니다. 결과가 크게 달라지거나 위험한 선택만 질문합니다.
- 요청 범위의 파일 수정, 비파괴 디버깅, 빌드·검사·테스트, 로컬 패키징·실행, 임시 검증 환경 구성은 재확인 없이 진행할 수 있습니다.
- 파괴적 데이터 삭제, 되돌리기 어려운 migration, production 배포·데이터 변경, 비용 발생, 계정·저장소 권한 변경은 해당 행동에 대한 사용자 승인이 필요합니다. 이미 승인한 범위를 다시 묻지 않습니다.
- 외부 메시지는 명시적으로 요청·승인된 경우에만 전송합니다. 비밀값을 외부로 전달하는 작업도 대상·방식에 대한 승인이 필요하며 문서나 로그에 노출하지 않습니다.
- 저장소를 참고하거나 검토하는 요청만으로 파일 수정·push·새 저장소 생성·다른 프로젝트 변경까지 승인됐다고 해석하지 않습니다.

### UI와 피드백

- 기본 화면은 **현재 상태 → 지금 할 행동 → 결과 → 상세** 순서를 우선합니다.
- 대표 행동 버튼 하나를 명확히 하고, 같은 목적의 버튼은 중복을 줄입니다.
- 버튼에는 실제 동작, 대체 동작, 불가 이유 중 하나가 있어야 합니다.
- 즉시 보이는 단순 성공에 토스트를 남발하지 않습니다. 실패·숨은 비동기 작업·위험한 변경은 명확히 알립니다.
- 정상적인 백그라운드 작업은 조용히, 사용자가 시작한 작업에는 진행상태를 보여줍니다. 상세 로그는 기본 화면을 지배하지 않게 분리합니다.
- 모바일에서도 중요한 기능·정보를 유지하고 재배치·스크롤로 해결합니다. 하나의 시각 장면은 필요하면 비율 축소를 검토합니다.
- 위험한 삭제·덮어쓰기는 명시적으로 선택한 대상에만 적용합니다.

### 데이터와 복구

- 같은 계산·지표는 단일 원본을 사용합니다. **모르는 값과 실제 0을 구분합니다.**
- 설치 파일과 사용자 데이터를 분리합니다. 삭제·재설치 시에도 데이터를 보존하거나 복원할 방법을 설계하고 실제 동작을 확인합니다.
- 배포된 record ID·DB key·localStorage key는 호환성 계약으로 다룹니다. 스키마 변경 전 migration·정규화·백업 복원을 준비합니다.
- 로그인·서버가 핵심에 필요하지 않으면 local-first를 먼저 검토합니다. 안정화된 인증·서명·네트워크 흐름은 이유 없이 다시 만들지 않습니다.
- 업데이트는 새 파일 검증, 기존판 백업, 교체 준비 확인, 실행·데이터 확인을 거칩니다. 복구가 확인되기 전에 rollback 백업을 지우지 않습니다.

### 빌드와 완료

- 대표 실행 명령과 산출물 위치를 명확히 합니다. 파일명·앱 내부·패키지 버전은 의도 없이 어긋나지 않게 합니다.
- 중요한 설치·서명·전송 산출물은 필요하면 SHA-256을 기록합니다. 검증한 파일과 전달한 파일이 같아야 합니다.
- HTML/UI/데이터와 native shell이 분리되어 있다면 변경 영향에 맞게 검증하고 불필요한 APK 재빌드를 강요하지 않습니다.
- 코드 작성, CI 통과, 빌드 성공, 설치 성공, 실제 사용자 흐름·실기기 검증은 각각 다른 증거입니다.
- 완료 기준은 **요청한 결과 + 변경에 필요한 검증**입니다. 문서 수정에 앱 빌드를 요구하지 않습니다. 미실행·실패·해당 없음을 구분합니다.
- 새 코드에 과거 PASS를 자동으로 물려주지 않습니다. 변경 영향을 확인해 필요한 검증만 다시 수행합니다.
- 실제 성공한 명령과 중요한 검증 결과를 해당 프로젝트 AGENTS에 남깁니다. 작업별 적용 기준은 [AGENTS.md](AGENTS.md)를 따릅니다.

### Windows와 실제 PC

- 텍스트·JSON·로그는 UTF-8을 기본으로 하고 PowerShell 출력 인코딩도 명시합니다.
- 관련 입력·경로 코드를 바꿀 때 한글 IME, 공백·한글 경로를 시험합니다. 취약한 구형 툴체인은 임시 빌드 경로만 ASCII로 우회하고 사용자 화면의 한글을 없애지 않습니다.
- 승인 후 온라인인 Remote Desktop 연결은 작업 종료를 이유로 끄지 않습니다. bridge/agent/service와 일회성 build/test 자식 프로세스를 구분합니다.
- 사용자 종료 지시, 보안 문제, 연결 복구용 재시작은 예외입니다. 재시작 시 가능한 경우 작업 가능한 상태로 복구합니다.
- PC가 offline이거나 접근하지 못했다면 실제 PC에서 작업·검증했다고 하지 않습니다. 저장소 증거로 충분하면 원격도구를 반복 호출하지 않습니다.
- 작업 후 임시 빌드·테스트 복사본, 실패 산출물, 불필요 캐시·로그·helper·staging secret/config는 정리합니다.
- 소스, 재빌드용 설정·스크립트, 사용자 데이터, 승인된 credential/keystore, 최종 산출물, 유용한 Known-Good, 최소 검증 기록은 보존합니다. 원격 연결 서비스는 찌꺼기가 아닙니다.

## 2. 도구·플러그인 상태

이 표는 **채택 상태**입니다. 지금 세션의 설치·로그인·OAuth 유효성·연결 상태는 사용할 때 확인합니다.

| 상태 | 의미 |
|---|---|
| ACTIVE | Workbench 전반에서 사용하도록 채택 |
| PROJECT | 특정 프로젝트에서 사용 |
| CANDIDATE | 후보 보관. 설치·채택·신뢰됐다고 가정하지 않음 |
| RETIRED | 사용 중단. 재채택 전까지 의존성으로 취급하지 않음 |

| 도구 | 채택 상태 | 용도와 확인 범위 |
|---|---|---|
| GitHub | ACTIVE | 소스·문서·commit·PR·issue·Actions. 사용자 PC 실행 상태의 증거는 아님 |
| Remote Desktop Commander / Desktop Remote | ACTIVE | 실제 PC 파일·명령·설치·실행·기기 검증. 세션 연결 상태 확인 필요 |
| ChatGPT Files / Library | ACTIVE | 첨부·과거 파일·저장 자료 회수. 과거 자료와 최신본 구분 |
| Supabase | PROJECT — chunkyack | 기존 기록: OAuth 완료 후 프로젝트 연결 단계. 현재 연결은 재확인하고 ID/URL/환경변수 이름은 해당 AGENTS에서 관리 |
| 기타 서비스 | 해당 프로젝트에서 확인 후 PROJECT | Google Drive, hosting, DB, Android SDK·서명, Blender 연동 등 |

현재 별도로 등록된 RETIRED 도구는 없습니다. 후보는 아래 링크함에서 관리합니다.

## 3. 프로젝트 안내

아래 경로와 설명은 기존 Workbench 등록 정보를 보존한 것입니다.
**2026-09-09 문서 정비에서는 다른 프로젝트의 최신 소스·진행 상태·실기기 동작을 재검증하지 않았습니다.**
재개할 프로젝트의 README/AGENTS를 먼저 확인하고, 확인한 경우에만 상태를 갱신합니다.

| 프로젝트 | 등록된 저장소·자료 위치 | 재개 시 먼저 확인할 내용 |
|---|---|---|
| 운동앱 | [HealthAPK](https://github.com/kimjae134679/HealthAPK) | 기존 수정본·원본 구분, 로컬 동작과 재실행 저장 상태 |
| 주식자동매매 | [Investment-Lab](https://github.com/kimjae134679/Investment-Lab) | 진단·백테스트·주문 실행 모드와 현재 허용 범위 |
| 청약 | [ChungYack](https://github.com/kimjae134679/ChungYack), [stock](https://github.com/kimjae134679/stock)의 `chungyack-apk/` | 현재 작업 원본이 어느 쪽인지, HTML과 APK 연결·저장 방식 |
| 멀티의신 | [PhoneLOL](https://github.com/kimjae134679/PhoneLOL) | 정상 기준판, 실제 연결·게임 진행 증거 |
| 주식 앱 / Market Radar | [stock](https://github.com/kimjae134679/stock) | 현재 화면·데이터 원본, 데스크톱·휴대폰 검증 상태 |
| 피규어만들기_01 | 참고 이미지·Blender 자료, 정확한 현재 위치 미확인 | 참고 이미지 유사도, 블록아웃 → 비율·실루엣 → 디테일. 기존 초안은 필요 시 교체 가능 |
| 동물의숲 / Tiny Village | 정확한 현재 자료 위치 미확인 | 기존 기록: 아늑한 desktop settings UI, 초록·베이지·목재·둥근 패널, 일반/주민/상호작용/화면/소리/기타 |
| 사이드메모장 | 정확한 현재 자료 위치 미확인 | 기존 파일 회수 후 이어갈 작업 확인 |
| FinanceOne 리뉴얼 | 정확한 현재 자료 위치 미확인 | 기존 파일 회수 후 이어갈 작업 확인 |
| 사이버 아쿠아리움 / ASCII Aquarium | 정확한 현재 자료 위치 미확인 | 기존 파일 회수 후 이어갈 작업 확인 |

GitHub가 필요 없는 프로젝트에 목록을 채우기 위한 저장소를 만들지 않습니다.
이름이 비슷하다는 이유로 저장소를 연결하지 않습니다.

상태를 추가할 때는 `현재 상태 / 다음 작업 / 확인 날짜 / 근거 링크`만 짧게 둡니다.
상세 진행 이력과 실행 정보는 해당 프로젝트 문서를 원본으로 유지합니다.

## 4. 재사용 팁

프로젝트에서 회수한 경험입니다. 현재 작업에 관련된 것만 적용하며, 보편적으로 검증된 사실이나 최신 프로젝트 상태로 간주하지 않습니다.

| 경험 출처 | 다시 쓸 만한 점 |
|---|---|
| HealthAPK | 로컬 우선, 실제 사용자 흐름 전체 확인, 재진입 저장, 개발 서버 실행과 독립·오프라인 패키지 검증 분리 |
| Investment-Lab | 한 번에 실행하는 진입점, 정상 대기·차단·오류 구분, 변경분 재검증, Windows UTF-8 |
| ChungYack | 자주 바꾸는 HTML·데이터와 native shell 분리, 저장 키 보존, 백업→변경→복원 검증, 판단 정보 먼저·원자료는 상세 |
| PhoneLOL | 정확한 기준 SHA와 작은 수정, 임시 위치에서 빌드·서명·검증 후 전달, 필요 시 hash·서명자·기기·세션 증거 |
| Market Radar | PC·폰 화면의 잘림·가독성·모달·뒤로가기·실행 오류 확인, 자동 생성 데이터와 수작업 UI 분리, 현재 원본과 보관본 구분 |
| 사이드메모장 | 선택 대상만 삭제, 드문 동작은 문맥 메뉴, 툴바 여백·성공 토스트 절제, 편집 선택영역 복원·한글 IME, 삭제 후 데이터 보존 |
| FinanceOne | 현재 기능 확인→시안→기능 보존→구현→회귀 검증, 단일 계산 원본, 필요 시 반복입력 값 TTL, 모름≠0, 글꼴·컨테이너 함께 조정, 인증·서명 보존, staging 비밀값 정리 |
| ASCII Aquarium | 상태와 raw log 분리, 사용자 시작·백그라운드 피드백 구분, updater READY 확인, 교체 파일 검증, 한글·공백 경로 시험, 파생판과 정상 기준판 분리 |

새 팁은 실패 조건·해결 방법·검증 근거가 있을 때 기존 항목에 합칩니다.
취향과 기술적 제약을 구분하고, 불필요해진 팁은 정리합니다.

## 5. 후보 링크함

아직 CANDIDATE입니다. 이번 문서 정비에서 외부 후보의 현재 상태를 검증하거나 설치하지 않았습니다.
### AI 개발 / 에이전트
- [OpenAI Plugins](https://github.com/openai/plugins)
- [AGENTS.md](https://agents.md/)
- [Ponytail](https://github.com/DietrichGebert/ponytail)
- [FrontierAgent](https://github.com/ApodexAI/FrontierAgent)
- [Gentle-AI](https://github.com/Gentleman-Programming/gentle-ai)
- [sandbox-runtime](https://github.com/anthropics/sandbox-runtime)
- [Camofox Browser](https://github.com/jo-inc/camofox-browser)

### 이미지 / 디자인 / Blender
- [awesome-gpt-image-2](https://github.com/YouMind-OpenLab/awesome-gpt-image-2)
- [Mimikyu](https://github.com/3x-haust/Mimikyu)
- [Blender MCP](https://github.com/emeryporter/blender-mcp)

### 영상 / 콘텐츠
- [JoyAI-Video-Edit](https://github.com/jd-opensource/JoyAI-Video-Edit)
- [Concat](https://github.com/jub0t/Concat)
- [HyperFrames](https://github.com/heygen-com/hyperframes)
- [Ddalkkak Threads Community](https://github.com/apache3563-bit/ddalkkak-threads-community/releases/tag/v1.11.7)

### 브라우저 / 생산성
- [TabZipsa](https://tabzipsa.com/)

### AI 인프라 / 로컬 모델
- [OpenLLM](https://github.com/bentoml/OpenLLM)
- [BentoML](https://github.com/bentoml/BentoML)
- [xFormers](https://github.com/facebookresearch/xformers)
- [cuML](https://github.com/NVIDIA/cuml)
- [Heretic](https://github.com/p-e-w/heretic)


사용 전에 공식 원본의 현재 버전·설치법·로그인·비용·라이선스·보안·호환성을 확인합니다.
정식 URL 기준으로 중복을 합치며, 채택 시 이 문서의 상태를 변경하고 정확한 설정·명령은 해당 프로젝트 AGENTS에 기록합니다.
더 이상 필요 없으면 RETIRED로 옮기거나 제거합니다.

## 6. 프로젝트 대화창구

[CHANNEL.md](PROJECT_CHANNEL/CHANNEL.md)는 필요할 때 읽고 쓰는 공유 게시판입니다.
자동으로 다른 AI를 실행하거나 실시간으로 알림을 보내는 기능은 없습니다.

- 다른 프로젝트에 도움이 되는 질문·답변·공통 버그·도구 변화가 있을 때만 사용합니다. 매 작업마다 읽거나 쓰지 않습니다.
- 문서 수정 권한과 다른 사람에게 메시지를 보내는 권한을 혼동하지 않습니다. 현재 요청 범위 안에서 사용합니다.
- 메시지는 짧게 쓰고, 비밀값·대형 코드·raw log를 올리지 않습니다. 채널 제안만으로 사용자 정책을 변경하지 않습니다.
- 확정된 정책은 해당 README, 검증된 기술 정보는 해당 AGENTS에 옮깁니다. 해결한 대화는 요지만 남깁니다.
- 특정 AI에게 전달할 맥락은 필요할 때만 `TO_<AI>.md`를 사용합니다. 완료된 요청은 상태를 표시해 다시 실행하지 않게 합니다.

세부 형식과 동시 수정 방법은 [대화창구 사용법](PROJECT_CHANNEL/README.md)에 둡니다.
