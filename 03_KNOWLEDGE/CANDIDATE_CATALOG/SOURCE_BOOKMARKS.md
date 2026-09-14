# Source Bookmarks — 2026-09-15

사용자가 직접 전달한 링크 + 조사 중 확인한 공식 원본을 한 곳에 보관합니다.

상태:
- `VERIFIED` — 현재 조사 환경에서 실제 페이지/README 확인
- `BOOKMARKED` — 링크는 보관
- `SOURCE-UNVERIFIED` — 링크 접근/본문 확인이 현재 조사 환경에서 실패. 내용은 추측하지 않음

---

## 사용자가 직접 준 웹/SNS 링크

| 상태 | 링크 | 정리 |
|---|---|---|
| VERIFIED | https://sceneai.art/ | UI/landing page/background용 AI prompt library. Premium 영역 존재 |
| SOURCE-UNVERIFIED | https://x.com/Aura_lixx/status/2099285949268279353 | X 원문은 현재 web cache에서 본문 조회 실패. 사용자가 준 링크 그대로 보관 |
| VERIFIED | https://kucharski.substack.com/p/ten-reasons-your-vibe-coded-dashboard | Adam Kucharski, 2026-09-02. vibe-coded dashboard의 사용자 여정/시각 위계 문제 등 디자인 비평 |
| SOURCE-UNVERIFIED | https://x.com/shanyanggm/status/2099196413649490398 | X 원문은 현재 web cache에서 본문 조회 실패. 사용자가 준 링크 그대로 보관 |

### X 링크 처리 원칙
스크린샷에 보이는 주장과 URL을 연결해 임의로 “이 글이 정확히 이 내용을 말했다”고 단정하지 않습니다. X 본문을 나중에 직접 읽을 수 있을 때 author/date/body를 다시 채웁니다.

---

# 공식 GitHub — AI Agent / 개발

- OpenHands — https://github.com/OpenHands/OpenHands
- Hermes Agent — https://github.com/NousResearch/hermes-agent
- CrewAI — https://github.com/crewAIInc/crewAI
- Aider — https://github.com/Aider-AI/aider
- LangGraph — https://github.com/langchain-ai/langgraph
- browser-use — https://github.com/browser-use/browser-use
- awesome-mcp-servers — https://github.com/punkpeye/awesome-mcp-servers
- Task Master / claude-task-master — https://github.com/eyaltoledano/claude-task-master
- LibreChat — https://github.com/danny-avila/LibreChat
- Anthropic Agent Skills — https://github.com/anthropics/skills
- Agent Skills specification — https://agentskills.io/

---

# 공식 GitHub — 자동화 / 외부 서비스 연동

- n8n — https://github.com/n8n-io/n8n
- Agentic Inbox — https://github.com/cloudflare/agentic-inbox
- Nango — https://github.com/NangoHQ/nango

---

# 공식 GitHub — 콘텐츠 / 영상 / 음성

- MoneyPrinterTurbo — https://github.com/harry0703/MoneyPrinterTurbo
- HyperFrames — https://github.com/heygen-com/hyperframes
- VoxCPM / VoxCPM2 — https://github.com/OpenBMB/VoxCPM

---

# 공식 GitHub — 금융 / 조사

- TradingAgents — https://github.com/TauricResearch/TradingAgents
- Fincept Terminal — https://github.com/Fincept-Corporation/FinceptTerminal
- Flowsint — https://github.com/reconurge/flowsint

---

# 사용자가 전달한 홍보/요약 문구를 볼 때 주의할 것

SNS 게시물에는 Star 수, 출시 시점, “무료”, “완전 자동”, “저작권 없음”, “스스로 개선” 같은 요약이 자주 들어갑니다.

이 카탈로그에서는 항상 다음 순서로 확인합니다.

```text
SNS/스크린샷에서 후보 발견
→ 공식 GitHub/사이트 확인
→ 현재 README 기능 확인
→ 설치/OS/권한 확인
→ 라이선스/비용 확인
→ 우리 프로젝트 적용처 구분
→ CANDIDATE 유지
→ 실제 설치 테스트 후에만 PROJECT/ACTIVE 승격
```

Star 수는 매우 빠르게 변하므로 카탈로그의 핵심 판단 기준으로 쓰지 않습니다.
