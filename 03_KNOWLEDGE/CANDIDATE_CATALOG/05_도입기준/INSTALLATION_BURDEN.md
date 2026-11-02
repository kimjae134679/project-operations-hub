# 설치·운영 부담

기준 **2026-11-03**. 설치 후보 목록이 아니라 선택 비용 분류입니다. 현재 추천은 [작은 목록](전체_도구목록.md)만 따릅니다.

| 등급 | 실제 부담 | 예시/조건 |
|---|---|---|
| ⚪ NONE | 문서/기존 연결 | 참고 저장소. 계정 연결은 별도 |
| 🟢 LIGHT | 작은 CLI, 상시 서비스 없음 | rg/fd/jq/uv/Ruff/Gitleaks/PSScriptAnalyzer/agent-desktop native |
| 🟡 MEDIUM | Python/브라우저/workflow | MarkItDown 선택 extras, Playwright, 기존 n8n |
| 🟠 HEAVY | 다중 서비스·모델 운영 | self-host 크롤러/AI 플랫폼, 기본 도입 안 함 |
| 🔴 VERY_HEAVY | GPU·큰 모델·RAM/디스크 | 대형 inference/RAG/학습, 기본 도입 안 함 |

작은 repo라도 모델/브라우저/Docker/DB/CUDA가 실제 부담을 키웁니다. hosted MCP는 로컬 설치가 가벼워도 계정·원격 데이터·사용량 비용이 남습니다. Apify local MCP도 Actors/결과는 cloud입니다. 무료 바이너리는 모든 Windows 앱 자동화나 비용 없는 모델 사용을 보장하지 않습니다.

기존 안정 설치·환경·모델·데이터를 보존합니다. [설치 원본](../../../01_CONTROL/AI_INSTALLATIONS.md)에서 실제 결과를 확인하며 새 autostart·서비스·공개 endpoint·대형 모델·보안 완화는 하지 않습니다. ComfyUI는 도입 목록에서 제거했으며 PC uninstall·모델/데이터 삭제는 하지 않습니다.
