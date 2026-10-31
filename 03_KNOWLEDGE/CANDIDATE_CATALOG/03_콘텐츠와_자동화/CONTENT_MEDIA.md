# Content / Video / Audio

확인 기준: 공식 GitHub README/사이트, 2026-09-15.

---

## 1. MoneyPrinterTurbo

- 공식: https://github.com/harry0703/MoneyPrinterTurbo
- 종류: **AI short-video automation application / WebUI / API / CLI / Agent Skill**
- 상태: `HIGH-FIT`
- 라이선스: MIT

### 현재 실제 기능
공식 README 기준 단순 데모 수준이 아니라 다음 흐름을 한 번에 묶습니다.

```text
주제/키워드
→ AI 영상 스크립트 생성
→ 소재 검색/선택
→ TTS
→ 자막
→ 배경음악
→ 세로/가로 영상 합성
→ 여러 후보 생성
→ 게시
```

### 확인된 세부 기능
- AI Agent / WebUI / API / CLI 제공
- 9:16 1080x1920, 16:9 1920x1080 지원
- batch 생성
- 다국어 script
- Edge TTS, Azure Speech, Gemini, ElevenLabs, Chatterbox 등 TTS provider
- subtitle 스타일 제어
- background music
- local media 사용 가능
- Pexels, Pixabay, Coverr 소재 검색
- Kimi/OpenAI/Gemini/DeepSeek/Qwen/Azure/xAI/MiniMax 등 여러 LLM provider
- README 현재판에는 **TikTok, Instagram, YouTube Shorts 자동 업로드**도 명시됨
- Agent가 Skill 문서를 읽고 설치/설정/생성까지 자동 처리하는 사용법 제공

### 설치 후보
Windows용 release one-click package가 있고, source/Docker 방식도 제공합니다. 공식 README는 local source 기준 Python 3.11+를 권장하며 Windows 경로에 한글/특수문자/공백을 피하라고 명시합니다.

### 현재 프로젝트에 대한 가치
Instagram 게시물 → Reels/Shorts까지 확장하려는 콘텐츠 파이프라인과 가장 직접적으로 맞는 후보입니다.

특히 “우리가 처음부터 영상 pipeline 전체를 직접 다시 짤 필요가 있나?”를 판단할 때 먼저 실제 설치 테스트할 가치가 큽니다.

### 반드시 확인할 것
- Pexels/Pixabay/Coverr가 연결돼도 **모든 소재가 아무 조건 없이 저작권 자유**라는 뜻은 아님
- 음악/음성/외부 LLM provider의 이용약관은 별도
- Instagram/TikTok/YouTube 자동 게시가 실제 계정 환경에서 얼마나 안정적인지 검증 필요
- 생성된 script/영상 품질은 자동 생성만 믿지 말고 human review gate 유지
- API key를 repo/config에 commit하지 않음

### 추천 테스트
1. 로컬에서 20~30초 9:16 한 편 생성
2. script → 소재 → 자막 → 음성 → render 품질 확인
3. 동일 주제로 3개 batch 생성 후 품질 편차 확인
4. 게시 기능은 테스트/비공개 계정에서 별도로 검증
5. 성공하면 Threads/Instagram 프로젝트에 `PROJECT` 후보로 승격 검토

---

## 2. HyperFrames — HeyGen

- 공식: https://github.com/heygen-com/hyperframes
- 종류: **HTML-native deterministic video rendering framework**
- 상태: `HIGH-FIT`
- 라이선스: Apache-2.0

### 정확한 정체
“AI 비디오 생성 모델”이 아니라 **HTML/CSS/media/animation을 frame-by-frame capture해서 MP4로 만드는 렌더링 엔진/프레임워크**입니다.

### 강점
- HTML native
- React 강제 없음
- same input → same frames/output를 목표로 하는 deterministic rendering
- Puppeteer + FFmpeg 기반 capture/encode pipeline
- GSAP/CSS/Lottie/Three.js/Anime.js/WAAPI 등 사용 가능
- CLI non-interactive workflow
- Agent용 Skills 제공
- browser studio/player/catalog/AWS Lambda 구성 존재

### 현재 프로젝트에 대한 가치
MoneyPrinterTurbo가 “자료/스크립트/소재 자동 조합”에 가깝다면 HyperFrames는 **우리가 원하는 고정 디자인 템플릿으로 결과물을 안정적으로 렌더링**하는 쪽에 강합니다.

예:
```text
커뮤니티 인기글
→ 요약 JSON
→ 고정 9:16 템플릿
→ 제목/본문/강조/댓글/출처 애니메이션
→ MP4
```

이렇게 쓰면 매 영상 디자인이 랜덤하게 흔들리는 문제를 줄일 수 있습니다.

### 설치 포인트
- npm/Node 기반
- FFmpeg/Puppeteer 계열 사용
- full dev clone은 golden test media 때문에 Git LFS 용량이 커질 수 있음
- 단순 사용 시 CLI/package만 먼저 검토

### 추천 역할
`MoneyPrinterTurbo = 빠른 완성형 파이프라인 후보`
`HyperFrames = 우리 브랜드/레이아웃을 유지하는 커스텀 렌더러 후보`

둘을 경쟁 도구로만 볼 필요는 없습니다.

---

## 3. VoxCPM / VoxCPM2 — OpenBMB

- 공식: https://github.com/OpenBMB/VoxCPM
- 종류: **local TTS + voice design + voice cloning model/toolkit**
- 상태: `HIGH-FIT / VERIFY-FIRST`
- 라이선스: Apache-2.0

### 현재판
공식 README의 최신 major release는 VoxCPM2입니다.

### 공식 특징
- 2B parameter
- 30 languages, 한국어 포함
- natural-language Voice Design
- short reference audio 기반 controllable voice cloning
- reference audio + transcript 기반 high-fidelity/continuation cloning
- 48kHz output
- streaming
- CLI/Web demo/Python API
- fine-tuning/SFT/LoRA

### 요구환경
공식 quick start:
- Python >= 3.10, < 3.13
- PyTorch >= 2.5
- CUDA >= 12 권장 경로
- VoxCPM2 공식 표 기준 대략 **VRAM ~8GB**

### 현재 프로젝트에 대한 가치
릴스/쇼츠에 매번 cloud TTS 비용을 내지 않고 로컬 음성을 만들거나, 콘텐츠별 캐릭터 보이스를 일정하게 유지하는 후보입니다.

### 주의
- 실제 사람의 목소리를 clone할 때는 **본인 또는 명확한 허가를 받은 음성만 사용**
- 사칭/기만 용도로 사용 금지
- GPU와 driver/CUDA 조합 실검증 필요
- 한국어 품질/속도/장문 안정성은 실제 샘플로 확인

### 추천 테스트
- 한국어 30초 narration
- 같은 문장 3회 seed/스타일 비교
- reference clone은 허가된 테스트 음성으로만
- 9:16 영상 pipeline에서 실제 render 시간/VRAM 측정
