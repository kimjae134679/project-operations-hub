# Threads — AI 콘텐츠 수익화

- Repo: https://github.com/kimjae134679/Threads
- 방향: 허용된 공개 소스/공식 API로 트렌드를 탐지하고 AI로 원본성 있는 멀티플랫폼 콘텐츠로 변환해 수익화 가능성을 실험
- 현재 상태: 실행형 `Trend Inbox MVP` 있음. Google Trends KR RSS + 선택적 YouTube Data API + 수동 URL/메모 → 소스 위험 판정 → 사람 평가 → Research Bundle 프롬프트 → 조사/제작/패스 상태 관리
- 실행: 저장소 루트 `npm start` → `http://127.0.0.1:4173/app/`
- 검증: GitHub Actions `npm run check` syntax PASS. 사용자 PC 브라우저 실기동 검증과 외부 API live-fetch 성공은 별도 검증 필요
- 재개 시: 실제 repo `README.md`, `AGENTS.md`, `app/README.md`와 최신 날짜의 플랫폼/소스 정책 문서부터 확인
- 검증 핵심: 소스 약관/권리, 원본성, 고위험 콘텐츠 사람 승인, 플랫폼별 성과·수익 추적, fake success 금지
- Room: `04_COMMUNICATION/rooms/Threads/`
- Main thread: `04_COMMUNICATION/threads/T-0008-ai-content-monetization/`
