# 사용 가능 여부와 사용자 도움

기준 **2026-11-03 KST**. 조건표이지 현재 세션 전체 연결 성공표가 아닙니다. 이전 Gmail/Drive/Notion/Supabase 등의 READY를 현재 인증 증거로 재사용하지 않습니다.

## 별도로 기록할 축

- ADOPTION: ACTIVE/PROJECT/CANDIDATE/REFERENCE/RETIRED. [채택 원본](../../../01_CONTROL/TOOLS.md).
- INSTALLED: YES/NO/UNKNOWN/SERVICE. [설치 버전/경로/검사 증거](../../../01_CONTROL/AI_INSTALLATIONS.md).
- ACCESS: 현재 호출·권한 확인 READY / 연결 필요 CONNECT / 설치 필요 INSTALL / 문서 REFERENCE / 미확인 UNKNOWN.
- AUTONOMY: 확인된 범위 FULL / 준비 후 AFTER_SETUP / 일부 PARTIAL / USER_DRIVEN / UNKNOWN. FULL은 모든 기능 보장 아님.
- ASSISTANCE: NONE/ONCE/FREQUENT. 막힘: LOGIN/OAUTH/UAC/2FA/CAPTCHA/API_KEY/PAYMENT/GPU/GUI_CHECK.
- COST: FREE_OSS/FREE_TIER/INCLUDED/TRIAL/PAID/UNKNOWN. 부담/난이도와 별개.

| 실무 대상 | 비용·부담 | 조건/도움 |
|---|---|---|
| 기존 Codex | INCLUDED · 🟢 | 기존 로그인·한도 범위. 소진 시 대기, API 과금 전환 안 함 |
| GitHub/gh | 기존 서비스/무료 CLI · 🟢 | 현재 repo 권한·인증 확인, 필요 시 로그인 ONCE 대기 |
| ProjectBridge/관제탑 | 기존 프로젝트 · 🟡 | 현재 연결·경로·실행권한 확인. 과거 응답 ≠ 현재 Online |
| rg/fd/jq/uv/Ruff/PSScriptAnalyzer/Gitleaks CLI | FREE_OSS · 🟢 | 설치 증거 후 제한된 로컬 작업. Gitleaks redact; 자동 fix/설정 변경 별도 |
| agent-desktop + Windows Skill | FREE_OSS · 🟢 | CLI/Skill설치·네이티브인식·help/지침검증, 실제 UI 미검증. 잠금/UAC/서비스 세션/최소화 미지원, 일부 동작 headed 필요 |
| MarkItDown | FREE_OSS · 🟢~🟡 | 필요한 extras만. OCR/음성/LLM/플러그인 별도 요구·비용·검증 |
| n8n | 기존 로컬 · 🟡 | 현재 workflow/health 확인; fair-code 조건, 외부 비용 별도 |
| Playwright/브라우저 | 🟡 | 현재 도구/프로젝트 테스트 우선. 로그인/2FA/CAPTCHA 도움 가능 |
| Apify Docs | tokenless 공개문서 · 🟢 | runtime설치 + Codex apify-docs등록,2tools초기화/목록·문서6,456자조회확인. 빈HOME/토큰clear, Actor tools0 |
| Apify Actor/Firecrawl/Context7 | FREE_TIER 또는 UNKNOWN | 문서모드와별개. 계정·한도·작업별비용확인전 인증 수집/연결대기 |
| Gmail/Calendar/Drive/Notion/기타 plugin | UNKNOWN · ⚪ | 현재 노출·연결·계정권한 확인. 옛 READY 표 사용 금지 |
| Jev | 기존 설치 · 🟢 | 키 존재 ≠ 인증/라우팅/절감. 새 유료 호출 안 함 |

무료 도구 → 검증한 무료 플랜 → 기존 요금제 범위를 우선합니다. 체험/카드/과금 가능성을 무료 무제한으로 적지 않습니다. 새 결제·구독·유료 API·autostart·보안 완화 없이 진행합니다. 중복 서버/Agent는 [현재 제외 목록](전체_도구목록.md), 설치 대기열 아님.

[접근 판정](USAGE_AND_ACCESS.md) · [사용자 요약](../../../000_사용자용/03_참고도구/AI_도구와_플러그인.md)
