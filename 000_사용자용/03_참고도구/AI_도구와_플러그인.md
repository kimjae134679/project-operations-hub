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
- Apify는 무료 코드와 별개로 무료 사용량 한도가 있고 local MCP도 수집/저장은 cloud입니다. Firecrawl/Context7도 계정/한도 확인이 먼저입니다.
- MarkItDown은 텍스트 추출이지 원본 레이아웃 보장 변환기가 아닙니다. LLM/음성/플러그인을 자동 켜지 않습니다.
- agent-desktop은 UIA 보조 CLI입니다. 잠금/UAC/서비스 세션·최소화 앱에 한계가 있고 일부 입력은 실제 마우스/포커스를 쓰므로 항상 화면을 안 건드린다고 보장하지 않습니다. 이번 help/version만으로 화면 성공이라고 하지 않습니다.
- 기존 n8n/Jev는 보존하되 새 상시 작업/공개 endpoint/유료 라우팅 호출 없음. Jev 키 존재는 인증·작업 성공·절감 증거가 아닙니다.

[설치·콘솔 사용](../02_프로그램/설치_도구_현황.md) · [내부 사용 조건](../../03_KNOWLEDGE/CANDIDATE_CATALOG/05_도입기준/USAGE_AND_ASSISTANCE.md). 새 결제·구독·autostart·보안 완화 없이 사용하며 기존 프로젝트·모델·자료를 보존합니다.
