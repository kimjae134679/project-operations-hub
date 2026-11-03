# AI 도구를 실제로 쓰는 순서

기준 **2026-11-03**. [작은 목록](후보_도구_요약.md)에서 시작하며 같은 도구·기억 서버를 중복 설치하지 않습니다.

1. 기존 Codex/GitHub/ProjectBridge·프로젝트 안내와 설치 현황을 확인합니다.
2. rg/fd로 파일을 찾고 필요한 내용만 읽습니다. JSON은 jq/PowerShell, 문서는 현재 문서 도구부터 씁니다.
3. 작업 후 Python은 Ruff, PowerShell은 PSScriptAnalyzer, 비밀값은 Gitleaks로 확인합니다. 자동 수정·전체 포맷은 먼저 영향/diff를 봅니다.
4. 문서는 링크·내용, 코드는 해당 test/build를 확인합니다. 설치/help·실제 기능·사용자 흐름 완료를 구분합니다.
5. OAuth/2FA/CAPTCHA·새 계정·유료 API가 필요하면 대기 이유를 남깁니다.

## 비용·권한 주의

- Codex는 기존 ChatGPT 로그인/한도 범위이며 **무제한 무료가 아닙니다**. 별도 API 과금 전환·이번 CLI 업그레이드 없음.
- Gmail/Drive/Notion 등은 과거 READY를 믿지 않고 현재 도구·계정 권한을 확인합니다.
- Apify **문서 전용 apify-docs**는 실제 설치/등록했고 토큰없이도구 2개 연결·공식 문서 조회가 성공했습니다. **Actor 수집**은 별도인증/비용대기이며 local MCP라도 Actor실행/결과저장은cloud입니다. Firecrawl/Context7도계정/한도확인이먼저입니다. [추가설치/실행](../02_프로그램/사진도구_설치와_실행.md).
- MarkItDown은 텍스트 추출이지 원본 레이아웃 보장 변환기가 아닙니다. LLM/음성/플러그인을 자동 켜지 않습니다.
- agent-desktop은 UIA 보조 CLI입니다. 후속 실제자체창에서 한글 입력/버튼/결과 조회는PASS였지만 **strict-headless만 사용했는데도 is_focused false→true로 전경 비간섭 FAIL**이었습니다. 특정명령/Windows provider/framework의 원인은 미확정입니다. 사용자가 다른 일을 하는 동안 전경을 보존해야 하면 파일/API/구조화된 연결을 우선하며, 허가된 좁은 UI 범위에서도 focus 변화 시 멈춰 확인합니다. 강제 포커스 복구나vendor수정으로 감추지 않습니다. 잠금/UAC/서비스세션/최소화 한계도 유지됩니다. [실제 테스트/실패 범위](../02_프로그램/도구_실제테스트_결과_20261103.md).
- 기존 n8n/Jev는 보존하되 새 상시 작업/공개 endpoint/유료 라우팅 호출 없음. Jev 키 존재는 인증·작업 성공·절감 증거가 아닙니다.

[설치·콘솔 사용](../02_프로그램/설치_도구_현황.md) · [내부 사용 조건](../../03_KNOWLEDGE/CANDIDATE_CATALOG/05_도입기준/USAGE_AND_ASSISTANCE.md). 새 결제·구독·autostart·보안 완화 없이 사용하며 기존 프로젝트·모델·자료를 보존합니다.
