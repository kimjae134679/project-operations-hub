# Tool Candidates — 조사·선택용 카탈로그

마지막 정리: **2026-09-13**

이 문서는 단순 링크함이 아니라 **실제로 무엇인지, 어디에 쓸지, 어떻게 설치하는지, 무엇을 조심할지**까지 확인해 두는 후보 카탈로그입니다.
아직 모두 `CANDIDATE`이며, 여기 적혀 있다는 이유만으로 설치·승인·신뢰·프로젝트 채택된 것으로 보지 않습니다.

## 상태 / 우선순위

- `CANDIDATE` — 조사 완료, 아직 미채택
- `TEST-FIRST` — 우리 환경에서 먼저 작은 샌드박스 시험 가치가 큼
- `PROJECT-FIT` — 특정 프로젝트에서 필요할 때 도입 검토
- `HOLD` — 당장 이득보다 비용·위험·환경제약이 큼
- 실제 채택 시 `01_CONTROL/TOOLS.md`의 `ACTIVE` 또는 `PROJECT`로 승격하고, 정확한 버전·명령·경로·권한·Known-Good는 해당 프로젝트 `AGENTS.md`에 기록

## 한눈에 보는 추천

| 후보 | 종류 | 우리에게 가장 맞는 용도 | 우선순위 |
|---|---|---|---|
| Tencent TeamAI | 규칙/Skill/MCP 배포·동기화 CLI | Project Operations Hub의 규칙·Skill을 Codex/Claude/Cursor 등에 배포 | **TEST-FIRST** |
| codex-with-chatgpt | Skill + 읽기전용 MCP bridge | ChatGPT=계획/리뷰, Codex=실행 역할분리 | **TEST-FIRST** |
| GPTaku Codex Plugins | Codex 플러그인 마켓 | 검색·문서·리서치·설계 보조를 필요한 것만 추가 | **TEST-FIRST** |
| Ponytail | Agent 규칙/Skill/Plugin | 과잉구현 억제, 작은 diff 우선 | **TEST-FIRST** |
| Mimikyu | Figma→웹 구현 Skill + 검증 파이프라인 | UI를 Figma와 수치/픽셀 기준으로 맞춤 | **PROJECT-FIT** |
| Blender MCP | Blender 애드온 + MCP | 피규어/Blender 작업을 AI가 실제 조작 | **PROJECT-FIT** |
| Ling 3.0 Flash VL | 원격 멀티모달 모델 | 이미지·영상·스크린샷 대량 보조검토 | **PROJECT-FIT** |
| HyperFrames | HTML→영상 프레임워크 | AI가 HTML/CSS로 자동 영상 제작 | **PROJECT-FIT** |
| OpenAI Plugins | Codex 플러그인 예제/마켓 구조 | 공식 플러그인 구조 참고 및 필요한 번들 도입 | **CANDIDATE** |
| Gentle-AI | 다중 Agent 설정 프레임워크 | 기존 Codex/Claude 등에 memory/SDD/RDD/Skill 구성 | **CANDIDATE** |

---

# 1. AI / Agent / Plugin

## OpenAI Plugins
- 링크: https://github.com/openai/plugins
- 종류: **Codex Plugin 예제/마켓플레이스 컬렉션**
- 하는 일: `.codex-plugin/plugin.json`을 중심으로 `skills/`, `.app.json`, `.mcp.json`, `agents/`, `commands/`, `hooks.json` 등을 묶는 공식 예시를 제공.
- 우리 용도: Project Operations Hub에서 후보 Skill/MCP를 **Codex-native Plugin**으로 포장할 때 기준 구조로 사용. Figma, Notion, 웹앱, Expo, Slides 등 공식 예제 참고.
- 설치: 필요한 플러그인을 Codex의 Plugin/Marketplace 흐름에서 설치. 저장소 자체는 주로 예제/마켓 소스.
- 계정/비용: 플러그인 자체와 별개로 연결 App/MCP에 따라 로그인·OAuth·API 비용이 생길 수 있음.
- Windows: Codex가 지원하는 환경이면 사용 가능하나 개별 플러그인 요구사항은 별도 확인.
- 주의: 예전의 구형 “ChatGPT Plugins” 개념과 혼동하지 말 것. 현재 저장소는 Codex Plugin 예제 중심.
- 상태: `CANDIDATE`

## AGENTS.md
- 링크: https://agents.md/
- 종류: **공개 Agent 지침 파일 규약**
- 하는 일: 사람용 README와 별도로 빌드·테스트·코딩 관례·보안 주의 등 AI 작업지침을 `AGENTS.md`에 둠. 중첩 AGENTS도 지원하는 생태계가 큼.
- 우리 용도: 이미 Project Operations Hub가 채택한 `README = 사용자 정책`, `AGENTS = AI 실행정보` 분리의 외부 기준 참고.
- 설치: 없음. 프로젝트 루트 또는 하위 폴더에 `AGENTS.md` 작성.
- 계정/비용: 없음.
- Windows: OS 무관.
- 주의: AGENTS를 과거 규칙 쓰레기통으로 누적하지 않고 현재 실행정보만 유지하는 우리 원칙은 계속 적용.
- 상태: `CANDIDATE`(형식 자체는 이미 참고 중)

## Ponytail
- 링크: https://github.com/DietrichGebert/ponytail
- 종류: **Agent Skill / Plugin / Ruleset**
- 하는 일: YAGNI → 표준라이브러리 → 플랫폼 기본기능 → 이미 설치된 의존성 → 최소 구현 순으로 과잉개발을 억제. review/audit/debt 명령 제공.
- 우리 용도: “구현 복잡도는 줄이되 요구사항·QA·보안은 줄이지 않는다”는 우리 규칙과 잘 맞는지 시험. 특히 작은 앱/UI 패치에서 불필요한 의존성 방지.
- 설치: Codex는 `codex plugin marketplace add DietrichGebert/ponytail` 후 Plugin UI에서 설치. Claude/Copilot/Gemini/OpenCode 등도 지원.
- 계정/비용: 보통 별도 API키 없음. 사용하는 Agent 자체 비용은 별도.
- Windows: PowerShell 관련 개선 기록이 있고 Codex Plugin 사용 가능. Node가 PATH에 있어야 lifecycle hook이 정상 동작하는 구성 존재.
- 주의: 제작자 벤치마크의 코드량/속도 개선 수치는 우리 프로젝트에서 재검증해야 함. `ultra` 같은 강한 축약 모드는 요구사항 누락 위험 때문에 기본 사용 금지 후보.
- 상태: `TEST-FIRST`

## FrontierAgent
- 링크: https://github.com/ApodexAI/FrontierAgent
- 종류: **독립 Agent Runtime / TUI / 멀티에이전트 프레임워크**
- 하는 일: ReAct 단일 장기 Agent와 coordinator + 병렬 sub-agent의 Agent Team 모드, 파일작업·명령 실행·평가 흐름 제공.
- 우리 용도: 대형 조사/파일 기반 작업에서 로컬 멀티 Agent 실험. 현재 ChatGPT Work/Codex 흐름과 기능 중복이 크므로 교체가 아니라 비교 실험용.
- 설치: upstream quickstart/설치 문서 기준. macOS/Linux 중심 one-command 흐름이 강조됨.
- 계정/비용: 연결하는 LLM endpoint에 따라 API키/비용 발생.
- Windows: 네이티브 Windows는 우선 확인 필요; 필요 시 WSL/컨테이너 검토.
- 주의: 실행권한이 큰 독립 Agent이므로 샌드박스·작업폴더 경계·비밀값 노출 검토 필수.
- 상태: `CANDIDATE`

## Gentle-AI
- 링크: https://github.com/Gentleman-Programming/gentle-ai
- 종류: **기존 Coding Agent 구성 프레임워크**
- 하는 일: Codex/Claude Code/Cursor/OpenCode 등 이미 설치된 Agent에 persistent memory, Spec-Driven Development, Receipt-Driven Development, skills, MCP, persona, bounded review 등을 구성.
- 우리 용도: Project Operations Hub의 정책/검증/receipt 체계와 비교해 좋은 패턴만 흡수하거나, 여러 Agent 환경 설정 자동화 후보로 사용.
- 설치: Go 기반 `go install .../gentle-ai/v2/cmd/gentle-ai@latest` 계열. 실행 후 Agent/구성요소 선택, `gentle-ai doctor`로 확인.
- 계정/비용: 자체는 오픈소스. 연결 Agent/서비스 비용은 별도.
- Windows: Go 실행환경에서 가능 여부를 설치 전 실제 테스트할 것.
- 주의: 이미 가진 Hub 규칙과 중복 가능성이 매우 큼. 둘 다 전역 규칙을 주입하면 충돌할 수 있으므로 샌드박스 프로젝트에서 먼저 시험.
- 상태: `CANDIDATE`

## Anthropic Sandbox Runtime (`srt`)
- 링크: https://github.com/anthropics/sandbox-runtime
- 종류: **OS-level sandbox CLI/library**
- 하는 일: 컨테이너 없이 프로세스의 파일시스템/네트워크/Unix socket 접근을 제한. Agent, MCP, shell command를 최소권한으로 감쌀 수 있음.
- 우리 용도: 외부 Agent/CLI를 시험할 때 `.ssh`, `.env`, 사용자 데이터 접근을 차단하는 안전층 후보.
- 설치: `npm install -g @anthropic-ai/sandbox-runtime`.
- 계정/비용: 없음.
- Windows: 현재 핵심 구현이 macOS `sandbox-exec`, Linux `bubblewrap` 기반이라 **Windows 네이티브용으로 보지 않음**. WSL에서 별도 검토.
- 주의: Research Preview/Beta이며 API·설정 변경 가능. 우리 Windows 주환경 때문에 즉시 도입 우선순위 낮음.
- 상태: `HOLD`

## Camofox Browser
- 링크: https://github.com/jo-inc/camofox-browser
- 종류: **AI Agent용 headless browser server**
- 하는 일: Camoufox 기반 Firefox fingerprint spoofing, 접근성 snapshot, 안정적 element ref, REST API/MCP 계열 자동화 지원.
- 우리 용도: 일반 브라우저 자동화가 기술적으로 막히는 합법적 내부/테스트 페이지의 Agent 브라우징 실험.
- 설치: `npm install @askjo/camofox-browser` 또는 repo clone 후 `npm install && npm start`; 최근 패키지는 Node 22+ 요구.
- 계정/비용: 자체 오픈소스. 대상 사이트/외부 서비스에 따라 별도.
- Windows: Node 패키지 자체는 가능성이 있으나 Camoufox 런타임 호환을 실제 PC에서 검증 후 판단.
- 주의: anti-bot 회피 기능이 핵심이라 사이트 약관·접근권한을 침해하는 용도로 사용하지 않음. 로그인 세션/쿠키 저장범위도 사전 확인.
- 상태: `HOLD`

## GPTaku Plugins — Claude Code
- 링크: https://github.com/fivetaku/gptaku_plugins
- 종류: **Claude Code Plugin Marketplace**
- 하는 일: insane-search/research/design, docs-guide, PRD, 병렬작업, Git 교육, workspace 도구 등 여러 플러그인 묶음.
- 우리 용도: Claude Code를 쓸 때 조사·공식문서 검색·설계/리뷰 기능을 필요한 것만 선택 설치. 작업패턴 자체는 Codex용 Skill 설계 참고자료로도 가치 있음.
- 설치: Claude Code marketplace 방식. 각 하위 플러그인 README의 의존성 확인.
- 계정/비용: 대부분 별도 credential 없음. `nopal`은 Google OAuth, `insane-review`는 로그인된 ChatGPT 웹 세션, `pumasi`는 외부 Codex CLI가 필요하다고 upstream이 명시.
- Windows: 개별 플러그인마다 다름.
- 주의: 제작자 성능수치는 제작자 벤치마크로 보고 재검증. 브라우저 세션을 쓰는 플러그인은 계정/세션 보안 확인.
- 상태: `PROJECT-FIT`

## GPTaku Plugins — Codex
- 링크: https://github.com/fivetaku/gptaku-plugins-codex
- 종류: **Codex-native Plugin Marketplace**
- 하는 일: 현재 공식 README 기준 docs-guide-codex, insane-search-codex, insane-design-codex 등 Codex용 플러그인 제공. 향후 추가 가능.
- 우리 용도: 우리 주력 흐름과 직접 맞음. 막힌 웹 자료 조사, 공식문서 기반 답변, 웹 디자인 추출 등을 필요한 프로젝트에만 추가.
- 설치: `codex plugin marketplace add https://github.com/fivetaku/gptaku-plugins-codex.git` → Codex 재시작 → `/plugins`에서 선택.
- 계정/비용: 플러그인별 외부 서비스가 없으면 별도 비용 없음.
- Windows: upstream은 **WSL2 권장**. 일부 플러그인은 git/gh/node/python3/tmux/gws 등 선택 의존성.
- 주의: 처음부터 전부 깔지 말고 1개씩 설치→권한/동작/로그 확인.
- 상태: `TEST-FIRST`

## Tencent TeamAI
- 링크: https://github.com/Tencent/teamai-cli
- 종류: **Skill/Rules/MCP/Knowledge 동기화 CLI**
- 하는 일: Git 저장소를 공통 경험 원본으로 삼고 Claude Code, Codex, Cursor, OpenCode, CodeBuddy 등 여러 Agent에 Skills·Rules·MCP·knowledge를 배포/동기화.
- 우리 용도: **Project Operations Hub를 사람이 관리하는 원본으로 두고 TeamAI를 배포층으로 쓸 수 있는지** 시험. 여러 프로젝트의 Agent 설정이 따로 노는 문제와 가장 직접적으로 맞닿음.
- 설치: `npm install -g teamai-cli`; 공유 repo 준비 후 `teamai init <repo>` 또는 project-scope 흐름.
- 계정/비용: npm/Git 자체는 무료. 원격 Git 권한과 각 Agent/서비스 비용은 별도.
- Windows: Node CLI이지만 hook/agent별 파일배치까지 포함하므로 우리 Windows에서 별도 샌드박스 검증 필요.
- 주의: `.codex`, `.claude`, rules/hooks 등을 자동 수정할 수 있으므로 **현재 Hub에 바로 init 금지**. 빈 테스트 repo에서 생성 파일/diff/삭제동작을 확인한 뒤 채택 판단.
- 상태: `TEST-FIRST`

## Codex with ChatGPT
- 링크: https://github.com/XiaoDuoYa/codex-with-chatgpt
- 종류: **Codex Skill + 읽기전용 MCP bridge + OAuth/Cloudflare tunnel**
- 하는 일: ChatGPT 웹을 계획/추론/리뷰 두뇌로, Codex를 실제 파일수정·shell·test 실행기로 분리. ChatGPT는 read-only MCP로 workspace 일부와 git diff/test evidence를 읽음.
- 우리 용도: Sol/ChatGPT의 장기 맥락·검토와 Codex 실행을 연결하는 실험. 우리 `CLAIMED != ACCEPTED`, 실제 diff/검증 증거 원칙과 궁합이 좋음.
- 설치: upstream 기준 Node 20+, git, cloudflared → repo clone/build → `skill/SKILL.md`를 Codex skill로 설치 → `c2c setup`에서 ChatGPT connector/pairing.
- 계정/비용: ChatGPT 로그인 필요. 안정 hostname 선택 시 Cloudflare 계정/도메인 선택사항. API key가 필수인 구조는 아님.
- Windows: upstream 설치문에 Windows/winget 흐름이 있으나 우리 PC에서 직접 재검증 전 미채택.
- 주의: **비공식 커뮤니티 프로젝트**. 로컬 workspace를 public tunnel 뒤 read-only MCP로 노출하므로 OAuth/경로격리/민감파일 deny가 실제 빌드와 동일한지 보안 검토 후 사용. repo는 통째 업로드하지 않는 설계지만 네트워크 공개면 자체가 추가 공격면.
- 상태: `TEST-FIRST`

---

# 2. Image / Design / Blender

## awesome-gpt-image-2
- 링크: https://github.com/YouMind-OpenLab/awesome-gpt-image-2
- 종류: **GPT Image 2 prompt/examples 라이브러리**
- 하는 일: 이미지 생성 프롬프트와 예시 이미지를 대량 분류한 참고 저장소. 실행 플러그인 자체가 핵심은 아님.
- 우리 용도: 이미지 생성 요청의 스타일/구도/제품샷/UI asset 프롬프트 참고. 피규어 레퍼런스나 프로젝트용 이미지 제작 아이디어 수집.
- 설치: 그냥 웹에서 찾아 써도 됨. 저장소 생성 스크립트를 직접 다룰 때만 Node/pnpm 등 필요.
- 계정/비용: 저장소 열람 무료. 실제 이미지 생성 서비스 비용은 별도.
- Windows: 참고자료이므로 제약 거의 없음.
- 주의: 프롬프트 예시의 결과가 항상 동일하게 재현되는 것은 아님. 이미지/상표/저작권 사용조건은 생성물별로 별도 판단.
- 상태: `CANDIDATE`

## Mimikyu
- 링크: https://github.com/3x-haust/Mimikyu
- 종류: **Figma→웹 구현 Skill + 자동 검증 파이프라인**
- 하는 일: Figma를 source of truth로 추출 → 웹 구현 → Playwright screenshot → PIL pixel diff → DOM 구조/스타일 검증 → 수정 반복. 99% match 및 mismatch gate를 둔 구조.
- 우리 용도: 웹 UI/대시보드/게임 메뉴 등을 **눈대중이 아니라 수치로 Figma와 맞추는 작업**. 특히 UI 품질 요구가 높은 프로젝트에 적합.
- 설치: upstream one-command shell installer 또는 수동 Skill 복사. 요구: Node 20+, pnpm, Python3+Pillow, Playwright Chromium, Figma MCP 또는 PAT.
- 계정/비용: Figma 파일 접근권한 및 경우에 따라 Figma token 필요. 나머지 로컬 도구는 무료.
- Windows: Skill 자체는 Codex 지원, 다만 공식 one-command가 bash 형태라 Windows는 WSL/Git Bash 또는 수동설치가 안전.
- 주의: Figma token을 repo/.mcp 공개파일에 박아 넣지 않도록 secret 관리 필요. API-driven 콘텐츠는 pixel gate만으로 무한수정하지 않도록 upstream도 예외를 둠.
- 상태: `PROJECT-FIT`

## Blender MCP
- 링크: https://github.com/emeryporter/blender-mcp
- 종류: **Blender Add-on + MCP server**
- 하는 일: Blender 안에서 MCP 서버를 띄우고 AI client가 장면/오브젝트 작업을 실제 Blender에 요청할 수 있게 연결.
- 우리 용도: `피규어만들기_01`, Blender 자동화, 블록아웃/반복수정/검사 작업.
- 설치: Release의 `blender_mcp.zip`을 Blender Add-ons로 설치→활성화→N 패널 MCP 탭에서 Server 시작. 기본 로컬 endpoint는 upstream 기준 `127.0.0.1:9876` 계열.
- 계정/비용: 로컬 서버 자체는 없음. 연결 AI client 비용은 별도.
- Windows: Blender Add-on 방식이라 Windows에 적합한 후보. 정확한 client 등록 명령은 사용하는 Codex/Claude 버전에 맞춰 확인.
- 주의: Blender 실제 scene을 바꾸는 write capability가 있으므로 저장본/백업/선택범위 검증 후 사용. 외부 공개 bind 금지, localhost 유지 우선.
- 상태: `PROJECT-FIT`

---

# 3. Video / Content

## JoyAI-Video-Edit
- 링크: https://github.com/jd-opensource/JoyAI-Video-Edit
- 종류: **로컬/서버형 AI video editing 모델·파이프라인**
- 하는 일: autoregressive diffusion 기반 open-ended video editing 서버/모델.
- 우리 용도: 영상 편집 AI 실험이 필요할 때만. 현재 일반 프로젝트 자동화 핵심과는 거리가 있음.
- 설치: Python 3.10 Conda 환경, CUDA PyTorch와 requirements, 별도 checkpoint/외부 runtime dependency 준비 후 `deploy/run_server.sh` 계열.
- 계정/비용: 모델 파일 다운로드/저장공간 필요. prompt enhancement를 외부 OpenAI-compatible endpoint에 붙이면 해당 API 비용 가능.
- Windows: 공식 deployment가 bash/Linux CUDA 환경 중심. Windows는 WSL/Linux 별도 환경이 현실적.
- 주의: 공개 테스트 환경이 **NVIDIA B200 1장**이었고, 소비자 GPU VRAM 요구량 질문도 upstream에서 아직 명확하지 않음. RTX 4070 Super 12GB에 바로 설치하는 후보가 아님. 의존성 runtime 이슈도 공개 issue에 있음.
- 상태: `HOLD`

## Concat
- 링크: https://github.com/jub0t/Concat
- 종류: **로컬 오픈소스 NLE / CapCut 대체 앱**
- 하는 일: multi-track 편집, split/trim/merge, transition, speed, local auto captions, local TTS/voice filter, templates 등을 로컬에서 처리.
- 우리 용도: 유료/클라우드 의존 없이 영상 편집이 필요할 때 사용자용 도구 후보. AI Agent 도구라기보다 실제 편집앱.
- 설치: Releases의 Windows build/portable archive 사용 가능.
- 계정/비용: 계정/구독 없음. 로컬 처리.
- Windows: upstream이 Windows 지원/테스트를 명시. portable 폴더 모드도 제공.
- 주의: 아직 beta/pre-release. 릴리스 이름/라이선스가 변한 이력도 보여 설치 시 최신 README/Release 확인. 프로젝트 파일 백업 권장.
- 상태: `PROJECT-FIT`

## HyperFrames
- 링크: https://github.com/heygen-com/hyperframes
- 종류: **HTML/CSS/JS → deterministic MP4 영상 프레임워크**
- 하는 일: HTML composition과 seekable animation을 Puppeteer/FFmpeg 기반으로 프레임 캡처·인코딩. Agent/CLI/CI 친화적이며 같은 입력→같은 프레임을 목표로 함.
- 우리 용도: 코드로 설명영상, UI demo, 데이터차트 영상, 반복 생성 콘텐츠를 자동 제작할 때 강함.
- 설치: CLI/packages 설치 후 composition 작성. 전체 repo 개발 clone 시 Git LFS를 권장하며 Windows는 `winget install GitHub.GitLFS` 예시가 있음.
- 계정/비용: 오픈소스 Apache-2.0, 로컬 render는 per-render 비용 없음. 별도 cloud 인프라 사용 시 비용 가능.
- Windows: Windows용 Git LFS 안내가 있고 HTML/Puppeteer/FFmpeg 계열이라 유력. 실제 renderer dependency는 테스트 필요.
- 주의: browser/FFmpeg 버전과 폰트가 render 재현성에 영향. 최종 영상 품질은 템플릿/asset 관리 필요.
- 상태: `PROJECT-FIT`

## Ddalkkak Threads Community
- 링크: https://github.com/apache3563-bit/ddalkkak-threads-community/releases/tag/v1.11.7
- 종류: **Windows용 Threads 콘텐츠 제작/자동화 도구 Community Edition**
- 하는 일: 저장소 설명 기준 Threads 콘텐츠 제작 및 자동화.
- 우리 용도: Threads 운영/콘텐츠 자동화가 실제 프로젝트가 될 때만 후보.
- 설치: GitHub Release의 Windows 배포물 기준. 현재는 특정 예전 release 링크가 저장되어 있으므로 사용 시 최신 release를 먼저 확인.
- 계정/비용: Threads/Meta 계정 관련 로그인이 필요할 수 있음. 제품 외 별도 서비스/API 여부는 버전별 문서 확인.
- Windows: Windows용 도구로 명시.
- 주의: 소셜 플랫폼 자동화는 계정 제한·약관·rate limit 위험이 있으므로 실제 계정 적용 전 테스트 계정/수동검토가 필요. AGPL-3.0 저장소.
- 상태: `CANDIDATE`

---

# 4. Infra / Local AI / GPU

## OpenLLM
- 링크: https://github.com/bentoml/OpenLLM
- 종류: **오픈소스 LLM self-hosting / OpenAI-compatible server**
- 하는 일: Llama/Qwen/Gemma 등 오픈모델을 한 명령으로 OpenAI-compatible endpoint와 chat UI 형태로 제공하고 Bento 계열 배포로 확장.
- 우리 용도: 나중에 Project Operations Hub나 자동매매 분석에 **로컬/사설 모델 endpoint**를 붙일 때 후보.
- 설치: `pip install openllm`, `openllm hello`, 모델별 `openllm serve ...`.
- 계정/비용: 로컬은 모델 다운로드/전기·GPU 비용. gated model은 Hugging Face token이 필요할 수 있음. BentoCloud는 별도 cloud 비용.
- Windows: Python 기반이지만 GPU inference backend에 따라 Linux/WSL이 더 현실적인 경우가 많음.
- 주의: upstream 예시 기준 다수 모델이 24GB~80GB 이상 GPU를 요구. RTX 4070 Super 12GB에서는 소형 모델만 현실적이고 모델별 요구량을 먼저 확인.
- 상태: `PROJECT-FIT`

## BentoML
- 링크: https://github.com/bentoml/BentoML
- 종류: **AI/ML model serving framework**
- 하는 일: Python model/inference code를 REST API, batching, multi-model pipeline, Docker image, cloud deployment로 패키징·서빙.
- 우리 용도: 로컬 모델을 다른 앱/프로젝트에서 공통 API로 쓰거나 AI 서비스 배포가 필요할 때 기반 레이어.
- 설치: Python 3.9+ 계열에서 `pip install -U bentoml`; `bentoml serve`, `bentoml build`, `bentoml containerize` 흐름.
- 계정/비용: 로컬 사용은 오픈소스. BentoCloud 사용 시 계정/비용 별도.
- Windows: Python 로컬 개발은 가능하지만 Docker/GPU serving 조합은 WSL/Linux가 편할 수 있음.
- 주의: 단순 로컬 모델 실행만 원하는 경우 OpenLLM/Ollama류보다 과한 구조일 수 있음. 실제 서비스화가 필요할 때 채택.
- 상태: `CANDIDATE`

## xFormers
- 링크: https://github.com/facebookresearch/xformers
- 종류: **PyTorch Transformer 최적화 라이브러리**
- 하는 일: memory-efficient attention, sparse/fused kernels 등 Transformer 연산을 빠르고 메모리 효율적으로 제공.
- 우리 용도: Stable Diffusion/Transformer 로컬 실행에서 이미 사용하는 stack이 xFormers를 지원하고 VRAM 절감이 필요할 때만.
- 설치: PyTorch/CUDA 조합에 맞는 공식 wheel index 사용. Windows wheel도 제공되는 버전이 있음.
- 계정/비용: 없음.
- Windows: 지원되지만 **PyTorch/CUDA 버전 정확히 일치**해야 함. long path 문제도 주의.
- 주의: “성능 좋아 보이니 일단 설치” 금지. 현재 torch/CUDA를 깨뜨릴 수 있으므로 프로젝트 venv에서 pin 후 벤치마크.
- 상태: `PROJECT-FIT`

## NVIDIA cuML
- 링크: https://github.com/NVIDIA/cuml
- 종류: **GPU 가속 Machine Learning 라이브러리 (RAPIDS)**
- 하는 일: scikit-learn 계열 ML 알고리즘을 NVIDIA GPU로 가속.
- 우리 용도: `Investment-Lab` 같은 대규모 feature/ML 분석에서 CPU 병목이 실제 확인될 때 후보.
- 설치: RAPIDS/cuML 공식 설치 matrix에서 CUDA/Python 버전에 맞춰 conda/pip/container 선택.
- 계정/비용: 오픈소스 Apache-2.0. NVIDIA GPU 필요.
- Windows: 네이티브 Windows보다 **WSL2/Linux 경로를 우선 검토**하는 것이 안전.
- 주의: 데이터가 작으면 GPU 전송/환경복잡도가 이득을 먹어버릴 수 있음. 기존 pandas/sklearn 기준과 결과 일치·속도 벤치마크 후 도입.
- 상태: `PROJECT-FIT`

## Heretic
- 링크: https://github.com/p-e-w/heretic
- 종류: **LLM safety-alignment 제거/abliteration 연구도구**
- 하는 일: directional ablation + optimizer로 transformer 모델의 refusal/safety alignment를 자동 약화하는 연구 프로젝트.
- 우리 용도: 현재 Project Operations Hub/앱 개발에는 **필요 없음**. 로컬 모델 연구를 명시적으로 할 때만 별도 격리 실험 대상.
- 설치: Python 3.10+, 적합한 PyTorch 후 `pip install -U heretic-llm` 계열.
- 계정/비용: 오픈소스 AGPL-3.0, 모델 다운로드/GPU 비용.
- Windows: PyTorch/model stack에 따라 가능하나 GPU 환경 부담 큼.
- 주의: 모델 안전행동을 의도적으로 제거하므로 생산환경·자동화 Agent 기본모델에 넣지 않음. 라이선스와 모델 원 라이선스도 함께 확인.
- 상태: `HOLD`

## Ling 3.0 Flash VL
- 공식 모델: https://huggingface.co/inclusionAI/Ling-3.0-flash-VL
- OpenRouter free route: https://openrouter.ai/inclusionai/ling-3.0-flash-vl:free
- 종류: **멀티모달 MoE 모델 / 원격 API 후보**
- 하는 일: text+image+video 입력, reasoning/tool-calling 계열. OpenRouter free endpoint는 현재 262K context로 노출.
- 우리 용도: 스크린샷 UI QA, 영상/이미지 대량 1차 분석, 저비용 보조 reviewer. Sol/Codex 대체가 아니라 병렬 보조모델 후보.
- 설치: 가장 현실적인 건 OpenRouter API 연결. 로컬 weight는 Hugging Face 기준 약 250GB 규모라 우리 PC용 주력 배포가 아님.
- 계정/비용: **2026-09-13 확인 시 OpenRouter `:free`는 입력/출력 $0**, 단 free endpoint rate limit과 가격정책은 언제든 바뀔 수 있음. OpenRouter 계정/API key 필요 가능.
- Windows: API 호출은 OS 무관.
- 주의: 무료라는 이유로 민감 프로젝트 파일을 자동 전송하지 않음. 외부 모델로 보내는 데이터 범위·보존정책을 먼저 결정.
- 상태: `PROJECT-FIT`

---

# 5. Productivity

## TabZipsa
- 링크: https://tabzipsa.com/
- Chrome Web Store: `TabZipsa - AI Tab Organizer`
- 종류: **Chrome 탭 관리 확장프로그램**
- 하는 일: 여러 Chrome 창의 탭을 한 패널에서 보고 AI 요청으로 제목/사이트 정보를 바탕으로 그룹화.
- 우리 용도: 조사할 때 GitHub/문서/공고 탭이 많이 쌓이는 사용자 브라우저 정리용. 개발 Agent 기능은 아님.
- 설치: Chrome Web Store 확장 설치.
- 계정/비용: Store 설명 기준 현재 월 30회 무료 AI 분류가 표시됨. 이후 정책/요금은 사이트 확인.
- Windows: Chrome 확장이므로 Windows 사용 가능.
- 주의: 탭 제목/URL 등 브라우징 메타데이터를 어떤 서버로 전송하는지 개인정보처리방침 확인 후 설치. 중요한 로그인/내부 URL이 많은 프로필은 별도 브라우저 프로필에서 먼저 시험.
- 상태: `CANDIDATE`

---

# 도입 원칙

1. **링크를 저장했다 = 설치했다가 아님.** 모두 기본은 `CANDIDATE`.
2. 먼저 공식 원본 README/Release/라이선스를 확인하고, SNS 캡처의 성능수치는 참고만 함.
3. 자동으로 `.codex/`, `.claude/`, hooks, MCP, shell config를 고치는 도구는 **빈 테스트 repo에서 diff를 본 뒤** 실제 Hub에 적용.
4. 로그인/OAuth/API key/token이 필요한 도구는 secret 값을 GitHub/README/AGENTS/로그에 기록하지 않음.
5. 외부 모델/브라우저/터널에 프로젝트 데이터를 보내는 경우 **무슨 파일/메타데이터가 나가는지** 먼저 확인.
6. Windows 네이티브가 애매하면 WSL2를 별도 환경으로 취급하고 기존 프로젝트 toolchain을 오염시키지 않음.
7. 후보를 실제로 시험한 뒤 `설치 성공 / 실제 사용 성공 / 제거·rollback 가능 / 남긴 파일`까지 확인해야 `ACTIVE` 또는 `PROJECT`로 승격.
8. 설치를 중단한 후보는 찌꺼기 hooks/config/service를 제거하고 필요하면 `RETIRED`로 기록.
