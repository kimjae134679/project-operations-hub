# Threads — AI 콘텐츠 수익화

- Repo: https://github.com/kimjae134679/Threads
- 방향: 허용된 공식 API/RSS/직접 링크로 화제를 탐지하고, AI+사람 검토로 원본성 있는 멀티플랫폼 콘텐츠를 만든 뒤 실제 게시 성과로 학습
- 현재 상태: 실행형 `Trend Inbox MVP`가 `수집 → 자료 보강 → 조사 → Draft Studio → Rights/Safety Gate → 사람 승인 → Threads 공식 게시 → Insights → Experiment Lab`까지 연결됨
- 기본 입력: Google Trends KR RSS + 수동 URL/메모
- 선택 입력: YouTube Data API + NAVER API HUB 뉴스/블로그/카페/Search Trend
- AI: 선택적 `OPENAI_API_KEY`로 Responses API + web search 조사/멀티플랫폼 초안. AI 결과는 자동 승인하지 않음
- 게시: 선택적 `THREADS_ACCESS_TOKEN`. 사람 승인 체인을 모두 통과한 텍스트만 공식 Threads API 2단계 publish 가능
- 성과: `publications[]` + Threads Insights를 원본으로 `LEARN / SCALE / KEEP / KILL`; 클릭/전환/실수익은 실제 값만 수동 입력
- 실행: 저장소 루트 `npm start` → `http://127.0.0.1:4173/app/`
- 검증: `npm run check`가 syntax + Experiment regression test. GitHub Actions는 서버 smoke/browser script load/keyless fail-closed까지 확인
- 중요 수정: `ai-studio.js`가 HTML에서 누락돼 브라우저 미로드 상태였던 문제를 2026-09-13 수정했고 CI로 재발 방지
- 실제 E2E 미검증: 사용자 실제 NAVER/OpenAI/Threads credential이 없어 live 외부 호출 및 공개 게시/Insights 성공은 아직 기록하지 않음
- 재개 시: repo `README.md` → `AGENTS.md` → `app/README.md` → `docs/SOURCE_REGISTRY.md` → `docs/EXPERIMENT_LAB.md` → `docs/THREADS_API_SETUP.md`
- 다음: 실제 Threads 텍스트 1~3건 E2E → 실제 Insights 확인 → 5건 이상 Experiment 기준 보정 → 자체 이미지/영상 → 타 플랫폼 adapter → DB 동기화
- 검증 핵심: 소스 약관/권리, 원본성, 일반인 안전, 사람 승인, 실제 성과·수익, fake success 금지
- Room: `04_COMMUNICATION/rooms/Threads/`
- Main thread: `04_COMMUNICATION/threads/T-0008-ai-content-monetization/`
- Latest handoff: `04_COMMUNICATION/threads/T-0008-ai-content-monetization/008-sol.md`
