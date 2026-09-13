# Threads — AI 콘텐츠 수익화

- Repo: https://github.com/kimjae134679/Threads
- 방향: 허용된 공개 소스/공식 API로 트렌드를 탐지하고 AI로 원본성 있는 멀티플랫폼 콘텐츠로 변환해 수익화 가능성을 실험
- 현재 상태: 실행형 `Trend Inbox MVP`가 `수집 → 조사 → Draft Studio → Rights/Safety Gate → 사람 승인 → Threads 공식 게시 → Insights`까지 연결됨
- 입력: Google Trends KR RSS + 선택적 YouTube Data API + 수동 URL/메모
- AI: 선택적 `OPENAI_API_KEY`로 Responses API + web search 조사/멀티플랫폼 초안. AI 결과는 자동 승인하지 않음
- 게시: 선택적 `THREADS_ACCESS_TOKEN`. 승인 Queue를 모두 통과한 텍스트만 공식 Threads API 2단계 publish 가능. `auto_publish_text` 사용 안 함
- 실행: 저장소 루트 `npm start` → `http://127.0.0.1:4173/app/`
- 검증: GitHub Actions syntax + 로컬 서버 smoke PASS. 키 없는 OpenAI/Threads는 fail-closed 확인. 실제 Threads 사용자 token 공개 게시 E2E는 아직 미검증
- 재개 시: repo `README.md` → `AGENTS.md` → `app/README.md` → `docs/THREADS_API_SETUP.md` 순서로 확인
- 다음: 실제 운영 source 추가 → publication/insight Experiment 모델 → KEEP/KILL/SCALE → 실제 Threads 소량 E2E → 타 플랫폼 확장
- 검증 핵심: 소스 약관/권리, 원본성, 일반인 안전, 사람 승인, 플랫폼별 성과·수익, fake success 금지
- Room: `04_COMMUNICATION/rooms/Threads/`
- Main thread: `04_COMMUNICATION/threads/T-0008-ai-content-monetization/`
