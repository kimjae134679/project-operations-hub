# Astra Codex Workbench

프로젝트마다 **딱 두 개의 관리 문서만 유지**하는 방식입니다.

## 내가 보는 파일 — `README.md`
이 파일은 사람이 빠르게 현재 상황을 파악하기 위한 문서입니다.

항상 아래 정도만 유지합니다.
- 이 프로젝트가 무엇인지
- 지금 어디까지 됐는지
- 현재 실제로 동작하는 것
- 중요한 결정/변경사항
- 다음에 할 일
- GitHub, 실행 파일, 주요 경로처럼 다시 찾을 위치
- 어떤 주요 도구·서비스를 쓰는지
- 내가 미리 설치/로그인/연결/승인해야 하는 것
- 다시 써먹을 만한 프로젝트별 팁과 주의점

예를 들어 APK 프로젝트라면 어떤 빌드 방식/도구를 썼는지, 어떤 기기에서 테스트했는지, 서명이나 Android SDK가 필요한지 정도를 사람이 다시 보기 쉽게 적습니다. 서버·웹 프로젝트라면 호스팅/DB/플러그인/OAuth 같은 의존성을 요약합니다.

비밀번호, API 토큰, 개인키 같은 **비밀값 자체는 절대 적지 않습니다.** 필요한 계정/권한이나 환경변수 이름만 기록합니다.

기술 로그나 에이전트용 장황한 규칙은 여기에 넣지 않습니다.

## AI가 보는 파일 — `AGENTS.md`
Codex/ChatGPT 같은 작업 에이전트가 다음 세션에서 바로 일을 이어가기 위한 문서입니다.

여기에는 정확한 경로, 빌드/실행/테스트 명령, 기술 제약, 권한 범위, 구현 원칙, 검증 기준과 함께 아래를 자세히 둡니다.
- 실제 사용한 언어·프레임워크·SDK·패키지 매니저·빌드/패키징 방식
- APK/앱 빌드 도구, IDE, 스크립트, 필요한 버전
- 사용한 플러그인, MCP, Skill, 외부 서비스, DB, 호스팅, CI/CD
- 필요한 로그인/OAuth/API 접근/저장소 권한/기기 승인/환경변수/인증서/서명 조건
- 재현에 필요한 PC·모바일·로컬 서비스 같은 환경 정보
- 작업하며 발견한 팁, 안정적인 명령, 함정, 실패 원인, 우회법
- 소스·산출물·배포 위치와 중요한 프로젝트 ID/링크

즉 `README.md`는 **내가 보는 설명서**, `AGENTS.md`는 **AI가 바로 작업에 들어가기 위한 작업 매뉴얼**입니다.

## 정리 규칙
프로젝트 진행을 기록한다는 이유로 폴더와 문서를 계속 만들지 않습니다.

`PLAN.md`, `STATUS.md`, `NOTES.md`, `HANDOFF.md`, 중복 README, 날짜별 인수인계 파일 등을 여기저기 만들지 않고 필요한 내용은 위 두 파일에 합칩니다. 실제 프로그램 구조상 필요한 `src`, `assets`, `tests` 같은 폴더는 당연히 그대로 사용합니다.

임시 결과물은 저장소 밖이나 기존 임시/빌드 위치에 두고, 최종 산출물만 프로젝트의 원래 구조에 맞춰 둡니다.

## 다른 프로젝트 채팅에서 받아오는 형식
파일 첨부보다 **채팅의 복사 버튼으로 바로 클립보드에 가져오는 방식**을 기본으로 합니다.

요청할 때 아래 형식으로 받습니다.
1. `AGENTS.md` 제목
2. AGENTS.md 전체가 들어간 Markdown 코드블록 1개
3. `README.md` 제목
4. README.md 전체가 들어간 Markdown 코드블록 1개

한 파일을 여러 코드블록으로 쪼개지 않습니다. 두 파일을 한 코드블록에 합치지도 않습니다. 그러면 각 파일마다 복사 버튼 한 번으로 전체 내용을 바로 복사할 수 있습니다.

## 다른 프로젝트 채팅에 정리를 부탁할 때
다음 형태로 요청합니다.

> 이 프로젝트의 기존 대화·파일·지침을 전체적으로 확인해서 앞으로 새 채팅에서도 바로 이어갈 수 있게 정리해줘. 결과는 루트에 둘 **AGENTS.md와 README.md 두 파일만** 만들어줘. AGENTS.md는 다음 AI가 읽을 실제 작업 인수인계로, 현재 목표/확정 요구사항/최근 진행상태/정확한 경로와 GitHub/빌드·실행·테스트 명령/사용한 언어·SDK·툴·APK 또는 패키징 방식/플러그인·MCP·외부 서비스/필요한 로그인·OAuth·권한·환경변수·인증서·서명 등 접근 조건(비밀값 자체는 쓰지 말 것)/기기·환경/알아낸 팁·주의점·실패하기 쉬운 부분·검증 방법/남은 작업을 포함해. README.md는 내가 읽을 요약으로 프로젝트 목적, 현재 상태, 실제 동작하는 것, 중요한 결정, 다음 할 일, 사용 중인 주요 도구·서비스, 내가 미리 준비하거나 로그인/승인해야 하는 것, 다시 쓸 만한 팁, 주요 링크·경로만 간결하게 정리해. **파일 첨부보다 채팅에서 바로 복사하기 쉽게 해줘. `AGENTS.md` 제목 아래에 파일 전체 내용을 하나의 Markdown 코드블록으로, `README.md` 제목 아래에 파일 전체 내용을 또 하나의 Markdown 코드블록으로 출력해. 각 코드블록은 복사 버튼 한 번으로 파일 전체를 클립보드에 넣을 수 있게 한 파일당 정확히 한 블록만 사용해.** 별도 status/plan/handoff/notes 문서나 새 폴더는 만들지 말고, 확인되지 않은 내용은 추측하지 마.

이 두 블록을 복사해서 기존 프로젝트의 같은 파일에 병합하면 되고, 별도의 인수인계 파일은 남기지 않습니다.

## 🧰 꿀팁 링크함
내가 나중에 다시 써먹기 좋은 도구·오픈소스·작업법을 모아두는 곳입니다. **여기에 있다고 실제 프로젝트에 설치되거나 채택된 것은 아닙니다.** 실제로 쓰게 되면 그 프로젝트의 `AGENTS.md`에 정확한 버전·설정·권한·명령만 승격해서 기록합니다.

### AI 개발 / 에이전트
- **OpenAI Plugins** — Skill + MCP + Agent + Command 같은 작업 구성을 참고하기 좋은 공식 예제 모음. https://github.com/openai/plugins
- **AGENTS.md** — 코딩 에이전트에게 프로젝트 규칙과 작업법을 전달하는 공용 포맷 참고. https://agents.md/
- **Ponytail** — 코딩 에이전트의 과잉 구현을 줄이고 작은 완성형 구현을 유도하는 참고 도구. 최소화 때문에 요구사항·QA까지 줄지 않게 주의. https://github.com/DietrichGebert/ponytail
- **FrontierAgent** — 긴 조사·파일 작업을 이어서 수행하고 ReAct/Agent Team 모드로 역할을 나누는 에이전트 프레임워크 참고. https://github.com/ApodexAI/FrontierAgent
- **Gentle-AI** — Codex/Claude Code/Cursor 등 기존 코딩 에이전트에 메모리, Skill, MCP, 검토 흐름 등을 구성하는 도구. https://github.com/Gentleman-Programming/gentle-ai
- **sandbox-runtime** — AI 에이전트 작업을 파일/네트워크 수준에서 격리하는 샌드박스 참고. https://github.com/anthropics/sandbox-runtime
- **Camofox Browser** — 에이전트용 headless 브라우저 후보. 웹 조사/테스트 자동화 참고용이며 사이트 약관·접근정책을 지키는 범위에서만 사용. https://github.com/jo-inc/camofox-browser

### 이미지 / 디자인 / Blender
- **awesome-gpt-image-2** — GPT Image 2용 대규모 프롬프트 예시·미리보기 라이브러리. 이미지 만들 때 아이디어/프롬프트 참고용. https://github.com/YouMind-OpenLab/awesome-gpt-image-2
- **Mimikyu** — 피그마 디자인을 코드로 가깝게 구현하는 Skill 참고. UI를 디자인 원본에 맞춰 재현할 때 후보. https://github.com/3x-haust/Mimikyu
- **Blender MCP** — Blender 애드온에서 Streamable HTTP MCP 서버를 띄워 에이전트와 연결하는 방식. 기본 서버 주소는 `http://localhost:9876`; Blender 애드온 설치/활성화가 필요. https://github.com/emeryporter/blender-mcp

### 영상 / 콘텐츠 제작
- **JoyAI-Video-Edit** — autoregressive diffusion 기반 실시간·open-ended 영상 편집 연구/도구. 영상 프레임이 들어오는 흐름에서 지시 기반 편집을 검토할 때 참고. https://github.com/jd-opensource/JoyAI-Video-Edit
- **Concat** — 로컬에서 돌아가는 오픈소스 CapCut 대체 편집기 후보. 멀티트랙, 자르기/합치기, 전환, 속도 조절, 로컬 자막/TTS 등을 참고. https://github.com/jub0t/Concat
- **HyperFrames** — 자연어 요청을 바탕으로 에이전트가 HTML/CSS/JS 프로젝트를 만들고 영상으로 렌더하는 방식. 빠른 설명영상/제품영상 자동 제작 아이디어에 유용. https://github.com/heygen-com/hyperframes
- **Ddalkkak Threads Community v1.11.7** — Windows 로컬 Threads 콘텐츠 제작·예약·미디어 처리·발행 도구. Community 버전에서 Threads API 연결을 하려면 본인의 Meta Developer 앱 설정이 필요. https://github.com/apache3563-bit/ddalkkak-threads-community/releases/tag/v1.11.7

### 브라우저 / 생산성
- **TabZipsa** — 크롬 탭이 너무 많을 때 업무/주제별로 정리하는 서비스 후보. 사용자 공유 링크 기준으로 저장했으며 실제 사용 전 현재 서비스 상태를 다시 확인. https://tabzipsa.com/

### AI 인프라 / 로컬 모델 참고
- **OpenLLM** — 오픈소스 LLM을 OpenAI 호환 API 형태로 띄우는 서버 구성 참고. https://github.com/bentoml/OpenLLM
- **BentoML** — 추론 스크립트/모델을 API 서비스로 패키징·배포하는 도구. https://github.com/bentoml/BentoML
- **xFormers** — 메모리 효율 attention 등 최적화된 Transformer 구성요소 라이브러리. https://github.com/facebookresearch/xformers
- **cuML** — scikit-learn 스타일 머신러닝을 NVIDIA GPU로 가속할 때 참고. https://github.com/NVIDIA/cuml
- **Heretic** — 로컬 언어모델의 safety alignment 제거/abliteration 연구 도구. 연구 목적 참고용이며 일반 프로젝트 기본 도구로 채택하지 않는다. https://github.com/p-e-w/heretic

### 관리 원칙
- 같은 저장소/서비스는 URL 기준으로 한 번만 둡니다.
- Threads/블로그/스크린샷보다 **원본 GitHub·공식 사이트 링크를 우선**합니다.
- 링크가 유용해 보여도 설치·로그인·비용·보안·라이선스 조건은 실제 사용 직전에 다시 확인합니다.
- 프로젝트에서 실제 사용하기 시작하면 그 프로젝트 `README.md`에는 사람이 알아야 할 요약을, `AGENTS.md`에는 정확한 설정·명령·권한을 기록합니다.