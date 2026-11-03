# 스크린샷 7장 · 도구 6개: 조사와 실제 설치

공식 확인/후속 설치 **2026-11-03 KST**. 이미지2·6은 같은 도구입니다. 스크린샷 자체는 외부 참고자료이며, **후속 사용자 설치 지시**로 실제 설치를 진행했습니다. [사용자 실행 입구](../../../000_사용자용/02_프로그램/사진도구_설치와_실행.md) · [설치 원본](../../../01_CONTROL/AI_INSTALLATIONS.md).

## 최신 실제 상태

| 사진 | 공식 원본·버전·라이선스 | 설치/검증 상태 |
|---|---|---|
| 1 | [replica-skill](https://github.com/Jakeschincariol/replica-skill), MIT, source77c9436fb3d18c3d58169efb8caf4fe906b0dc51 | **11Skill 실제 설치**/Codex enabled인식, helper 검사 18/18. 실제 앱 복제·모델 동작 E2E 미실행 |
| 2·6 | [agent-desktop](https://github.com/vercel-labs/agent-desktop), [v1.0.2](https://github.com/vercel-labs/agent-desktop/releases/tag/v1.0.2), Apache-2.0 | CLI설치 유지 + Windows Skill 실제설치/enabled 인식. 버전·도움말·지침 확인; 화면/입력0회 |
| 3 | [Bytebot](https://github.com/bytebot-ai/bytebot), Apache-2.0, 2026-03-07 archived | 보관 저장소로 신규 설치 제외 |
| 4 | [Apify MCP](https://github.com/apify/apify-mcp-server), [v0.20.0](https://github.com/apify/apify-mcp-server/releases/tag/v0.20.0), MIT | **local runtime설치 + Codex apify-docs등록**, 토큰 없는 도구 2개 연결·공식 문서6,456자조회. Actor 인증/수집 비활성 |
| 5 | [OpenMausBot](https://github.com/milind-soni/OpenMausBot), [v0.1.138](https://github.com/milind-soni/OpenMausBot/releases/tag/v0.1.138), Apache-2.0 | **portable 전체프로그램 실제설치**, D 데이터/기존 Codex/gpt-6.1-sol 다음수동 시작 준비. 정상 GUI/인증/모델 응답 미검증 |
| 7 | [Codex](https://github.com/openai/codex), Apache-2.0 | 기존 CLI0.160.1 재사용, 업그레이드 없음 |

## 단계 이력 · 현재 상태와 혼동하지 않기

1차 조사/선별에서는 Replica는 참고만, Apify는 계정 필요 후보, OpenMaus는 중복/운영부담 때문에 미도입으로 기록했습니다. 사용자가 후속으로 **설치하라**고 명시한 뒤 위3항목의 실제 설치와 Windows Skill 추가를 수행했습니다. 1차 “미설치/미연결” 판단은 최신 상태가 아닙니다. 반대로 설치가 전체 사용자 흐름·실화면·모델 응답·유료수집 완료를 뜻하지도 않습니다.

## Replica · Codex 인식까지 확인

11Skill/표준Python helper6개를 확인하고 공식고정source에서 기존 Skill을 덮지 않고 설치했습니다. helper18검사와 Codex `skills/list(forceReload)`에서 Replica 11개 + Windows 1개 **12개 enabled/파서 오류 0**. 기존 Skill/AGENTS/globalhook는 덮지 않았습니다. 경로/WindowsPython호출을 맞추고, 다음단계 권고를 자동권한으로 보지 않도록 한정하며 기본 실결제/환불 지침은 제거·제한했습니다. 파일이 무료라는 사실은 Claude/Codex 사용량·외부 API·생성앱 운영비를 무제한 무료로 만들지 않습니다. 실제 cloning/배포/게시·모델 호출은 하지 않았습니다.

## agent-desktop · 설치된 보조 CLI와 Windows Skill

Windows Skill은 [v1.0.2 고정지침](https://github.com/vercel-labs/agent-desktop/tree/v1.0.2/skills/agent-desktop-windows)에서 설치했고 보안 완화/권한상승·강제단축키 우회 조언은 제거·제한했습니다. 기존1.0.2 binary는 교체하지 않았습니다. OS접근성UIA의 의미 기반 조작은 ProjectBridge대체 원격서비스가 아닙니다. Windows 1개0 1809+/Server2019+ 로그인한 사용자 desktop필요; 잠금/UAC/Session0/다른사용자/최소화창 한계가 있습니다. 일부 type/right-click/다중클릭/focus는 headed필요. “항상마우스/포커스를 안뺏는다” 보장 아님. 이번 버전·지침 확인 외 화면/클립보드/입력probe0회입니다. unsigned바이너리를 위해 AV/UAC/SmartAppControl을 낮추지 않습니다.

## Apify · 문서 연결과 Actor 인증을 분리

npm실명 `@apify/actors-mcp-server@0.20.0`을 D의 독립runtime에 lockfile과 설치했습니다. Node24.16.0재사용(요구>=22), npm `--ignore-scripts`로 lifecycle0, 게시dist로 시작했습니다. root가 기존MCP 11개와 동등한 설정을 보존/비공개백업한 뒤 **apify-docs**를 추가했습니다. 공개repo에 개인config/backup/키값 없음.

Docs는 APIFY_TOKEN을 비우고 NoAuthHome으로 기존CLI인증파일 fallback을 막으며 telemetry/Sentryoff입니다. 네트워크차단하 stdioinitialize/tools-list 성공, 도구는 `search-apify-docs`, `fetch-apify-docs`뿐입니다. 공식 [실행문서](https://docs.apify.com/actors/running)6456자 실제조회 성공. 구형/platform주소301을 guard가 막았으나 같은 공식 host의 canonical경로로 승인된조회에 성공했습니다. Actor/cloudcompute/dataset호출0입니다.

**계정이 필요한 것은 Actor/수집 모드**입니다. localMCP라고 Actor실행/결과저장이 offline무료가 되지 않습니다. [공식가격](https://apify.com/pricing)의 Free월$5credit/카드불필요/소진후차단은 무제한무료가 아닙니다. actor/proxy/storage/compute비용·약관·개인정보는 별도확인. 계정/OAuth/토큰 발급·결제없이 수집을 켜지 않았습니다.

## OpenMausBot · 설치했지만 정상 앱 시작은 미검증

공식NSISpayload를 D의버전폴더에 추출한 portable 전체Electron설치(970272192bytes)입니다. SHA256/GitHubassetdigest와 SHA512/latest.yml·EXE버전 확인. unsigned이며 installer실행/uninstall등록/autostart없음. 주바로가기 `D:/A_KJ/AI/Applications/OpenMausBot/OpenMausBot.lnk`는 문서화된 process-only OMB_DATA_DIR로 정상EXE를 다음사용자수동 시작 때 여는 래퍼를 가리킵니다. D 데이터의 기존 Codex/ChatGPT-plan/gpt-6.1-sol설정은 staged·미활성이고 추가로그인이 필요할수있습니다. 래퍼정적/합성 검사 3/3일뿐 실제실행하지 않았습니다.

별도 동봉CLIhelp의 상대모듈누락exit1은 특정packaging문제이며 정상 GUI/번들backend고장증거가 아닙니다. hiddenbackend시도는 도구정책에서 **실행 전에 거부**되어 실행/포트 개방/우회 재시도 없음. 정상앱 첫시작의 CUAdriver·Composio익명 설치 등록은 무단실행하지 않았습니다. 문서의host-control인증대상은 macOS/UbuntuXorg이며 Windows패키지가 Windows실제제어검증을 뜻하지 않습니다. Boat는체험후유료, Composio등은별도계정/약관입니다. cloud/OAuth/model응답은미실행입니다.

## Bytebot와 Codex

Bytebot는 archived이며 Ubuntu/Dockerdesktop과 자기providerAPIkey구조입니다. “No Limits”는 무료 무제한추론이 아니므로 설치하지 않았습니다. Codex는 [공식인증](https://learn.chatgpt.com/docs/auth)/[한도](https://learn.chatgpt.com/docs/pricing)/[Windows안내](https://learn.chatgpt.com/docs/app/windows)와 기존local환경을 사용하며 ChatGPT요금제한도와 API과금을 구분합니다.

## 유지한 경계

새 유료 API/구독/결제·autostart·보안 완화·실화면 조작 0. 허용된변경은 실제 프로그램/Skill설치·문서 전용MCP등록이며 기존 앱 0.9.18·계정 설정·모델·자료는보존했습니다. ComfyUI는 현재목록에서만제거했고 PC앱/모델/작업/데이터삭제는하지않았습니다. **모든 설치가 E2E 검증됐거나 모든 프로젝트가 연결됐다고 표현하지 않습니다.**

[현재작은목록](../05_도입기준/전체_도구목록.md) · [사진도구실행](../../../000_사용자용/02_프로그램/사진도구_설치와_실행.md)
