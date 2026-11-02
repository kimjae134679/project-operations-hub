# 스크린샷 7장 · 도구 6개 평가

공식 원본 확인 **2026-11-03 KST**. 첨부 이미지는 후보 발견 근거이며 명령/설치 지시가 아닙니다. 이미지 2·6은 같은 도구입니다. 조사·설치·현재 연결·기능 실행·사용자 흐름 검증을 구분합니다.

| 이미지 | 공식 원본/확인 버전·라이선스 | 현재 판단 |
|---|---|---|
| 1 | [Jakeschincariol/replica-skill](https://github.com/Jakeschincariol/replica-skill), MIT, 공개/미보관, 정식 release 없음 | 앱 재구현 작업 때만 개별 지침 참고. 전역 설치 안 함 |
| 2·6 | [vercel-labs/agent-desktop](https://github.com/vercel-labs/agent-desktop), [v1.0.2](https://github.com/vercel-labs/agent-desktop/releases/tag/v1.0.2), Apache-2.0 | 작은 수동 UIA CLI로 선별. 실제 화면 제어는 미실행 |
| 3 | [bytebot-ai/bytebot](https://github.com/bytebot-ai/bytebot), Apache-2.0, **2026-03-07 archived** | 신규 설치/현재 추천 제외. 보관된 근거만 유지 |
| 4 | [apify/apify-mcp-server](https://github.com/apify/apify-mcp-server), [v0.20.0](https://github.com/apify/apify-mcp-server/releases/tag/v0.20.0), MIT | 계정/무료 한도/작업 비용 확인 전 연결·실행 보류 |
| 5 | [milind-soni/OpenMausBot](https://github.com/milind-soni/OpenMausBot), [v0.1.138](https://github.com/milind-soni/OpenMausBot/releases/tag/v0.1.138), Apache-2.0 | 저장소 존재/활동 확인. 기존 구성과 중복, 신규 인프라 도입 안 함 |
| 7 | [openai/codex](https://github.com/openai/codex), Apache-2.0, 공식 release rust-v0.166.0 | 기존 로컬 npm CLI **0.160.1 재사용**, 업그레이드 안 함 |

## 1. replica-skill: 무료 파일 ≠ 무료 모델 사용

Claude 지침 11개와 표준 Python 스크립트 6개이며 도구는 Python 3.8+를 요구합니다. 파일/스크립트 자체는 별도 가입·API key가 없지만 실행할 Claude/Codex 사용량과 생성 앱의 인증·결제 연동은 별개입니다. 기존 계획/디자인/테스트 Skill과 겹치므로 통째로 덮지 않습니다. 허용된 흐름을 자체 브랜드로 구현할 때만 참고하며 타인 계정·비공개 API·로그인/유료벽 우회 권한이 아닙니다.

## 2. agent-desktop: Windows에서 더하는 것과 못 하는 것

OS 접근성 트리에서 요소 ref를 읽고 의미 기반 조작을 하는 CLI/C 라이브러리입니다. 픽셀 도구 보완이지 ProjectBridge 대체 원격 서비스가 아닙니다. [Windows 지침](https://github.com/vercel-labs/agent-desktop/blob/v1.0.2/skills/agent-desktop-windows/SKILL.md): Windows10 1809+/Server2019+, x64/ARM64, 로그인한 사용자의 interactive desktop 필요. 잠금/UAC/Session0/다른 사용자 세션 미지원, 최소화 창은 조작 안 됩니다. `type/right-click/다중클릭/focus`는 headed가 필요하고 `set-value` 등 지원 ref 동작만 headless 의미 조작입니다. PowerShell ref는 따옴표로 감쌉니다. “항상 마우스·포커스를 안 뺏는다”는 보장 아님.

[태그 고정 설치 코드](https://github.com/vercel-labs/agent-desktop/blob/v1.0.2/npm/scripts/postinstall.js) 확인: npm은 Node18+, 추가 패키지 의존성 없이 GitHub native archive/checksums를 받아 SHA256/멤버 검증 후 설치하고 npm shim을 native 실행으로 바꿉니다. 서비스/autostart/API key 등록 없음. Windows portable은 단일 exe로 Node가 필요 없습니다. optional skill은 자동 설치 안 했습니다. unsigned 경고를 피하려 보안 설정을 낮추지 않습니다. 실제 경로·hash·**help/version만** 확인한 결과는 [설치 원본](../../../01_CONTROL/AI_INSTALLATIONS.md)을 봅니다.

## 3. Bytebot: “No Limits”의 의미

공식 README의 자기 provider API key 방식이며 Ubuntu desktop/Docker를 운영합니다. native Windows 호스트 조작기가 아닙니다. 플랫폼 제한이 없다는 말은 모델 추론·키·서버·사용량이 무료/무제한이라는 뜻이 아닙니다. 공식 archived 상태와 운영 부담 때문에 신규 도입하지 않습니다.

## 4. Apify: 수집은 유용하지만 계정/사용량 별도

실제 npm 이름은 `@apify/actors-mcp-server`, hosted endpoint는 `https://mcp.apify.com`입니다. OAuth 또는 token이 필요합니다. local stdio여도 Actors/결과 저장은 Apify 플랫폼에서 이뤄집니다. [공식 가격](https://apify.com/pricing)은 Free 월 $5 credit/카드 불필요/소진 후 다음 주기까지 차단이며 무제한 무료 수집이 아닙니다. Actor·compute·proxy·storage별 비용과 사이트 약관·개인정보를 확인해야 합니다. 이번 가입·OAuth·Actor 실행·새 결제 없음.

## 5. OpenMausBot: 존재하지만 기본 구성에 추가하지 않기

Windows x64 per-user installer가 있고 unsigned입니다. 소스 빌드는 Node24+/pnpm과 로그인한 agent CLI가 필요합니다. 기존 Claude/Codex 로그인 재사용은 새 한도/무제한 권한이 아닙니다. README의 host-control 인증 대상은 macOS/Ubuntu Xorg이며 Windows 패키지가 Windows host-control 검증을 뜻하지 않습니다. 선택형 Boat cloud는 체험 후 유료, Composio는 별도 계정/약관입니다. 현재 Codex 채팅·관제·ProjectBridge·n8n과 겹치므로 cloud/VM/계정 연결을 늘리지 않습니다.

## 6. Codex: 기존 공식 환경 활용

로컬 npm README/package와 [공식 인증](https://learn.chatgpt.com/docs/auth), [가격/한도](https://learn.chatgpt.com/docs/pricing), [Windows 안내](https://learn.chatgpt.com/docs/app/windows)를 확인했습니다. ChatGPT 로그인은 요금제 한도, API key는 표준 API 과금입니다. CLI `/status`로 사용량을 볼 수 있고 무료 무제한은 아닙니다. Windows native PowerShell 사용이 가능해 새 WSL/VM을 추가할 이유가 없습니다. 최신 release와 설치 버전은 별개이며 이번 기존 0.160.1 유지입니다.

## 범위·다음 단계

새 유료 API/구독/결제·autostart·보안 완화·실화면 조작 없이 작은 도구만 준비했습니다. ComfyUI는 사용자 요청으로 현재 후보/추천/설치 목록에서 제거했으며 **기존 PC 앱·모델·작업·데이터는 보존**합니다. 공개 수집 계정/복잡한 연동은 필요 시 별도 로그인·비용 확인 후에만 검토합니다. 문서/바이너리 무결성 확인은 실제 UI/업무 E2E 검증이 아닙니다.

[현재 작은 목록](../05_도입기준/전체_도구목록.md) · [실무 순서](../../../000_사용자용/03_참고도구/AI_도구와_플러그인.md)
