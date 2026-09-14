# UI / Design / Agent-Rule References

확인일: **2026-09-15**

이 문서는 설치 도구뿐 아니라 앞으로 UI/AGENTS/Skills를 정리할 때 다시 볼 참고자료를 모읍니다.

---

## 1. SceneAI

- 사이트: https://sceneai.art/
- 종류: **웹 UI/landing page용 AI prompt library**
- 상태: `REFERENCE`

### 확인된 정체
사이트가 스스로 설명하는 핵심은 **“Beautiful AI Prompt Library, Made for Creators”**입니다.
완성된 앱 프레임워크가 아니라 AI에게 웹 섹션/랜딩 페이지/배경을 만들게 할 때 재사용할 수 있는 prompt와 시각 레퍼런스를 모은 서비스입니다.

현재 사이트에서 확인되는 범주:
- landing page / hero section prompt
- gradients
- animated backgrounds
- portfolio/agency/product page 계열 디자인 prompt
- premium all-access 영역

### 우리한테 의미
AI가 UI를 만들 때 항상 비슷한 카드/그라데이션/대시보드를 뱉는 문제를 줄이기 위한 **시각 레퍼런스/프롬프트 아이디어 저장소**로 유용합니다.

### 쓰는 방식
UI 프로젝트를 시작할 때:
1. 현재 앱의 목적/사용 흐름 먼저 결정
2. SceneAI에서 분위기/section 레퍼런스 선택
3. prompt를 그대로 복사하는 대신 우리 정보구조와 component에 맞게 축약
4. 실제 PC/모바일 화면에서 사용성 검증

### 주의
- 유료/Premium 콘텐츠가 있음
- prompt 결과물이 곧 좋은 UX라는 뜻은 아님
- 라이선스/상업 이용조건은 실제 사용 전에 사이트 License Agreement 확인

---

## 2. Adam Kucharski — “Ten reasons your vibe-coded dashboard looks terrible”

- 원문: https://kucharski.substack.com/p/ten-reasons-your-vibe-coded-dashboard
- 게시: 2026-09-02
- 종류: **AI 생성 dashboard 디자인 비평/참고 글**
- 상태: `REFERENCE`

### 확인된 핵심
글은 AI에게 CSV와 “visually attractive interactive dashboard”만 주었을 때 나오는 전형적인 결과가 왜 보기 나쁜지 분석합니다.

현재 원문에서 직접 확인된 핵심 문제 중 대표:
- **No user journey** — 사용자가 왜 이 화면을 보고 무엇을 해야 하는지 흐름이 없음
- **No visual hierarchy** — 모든 카드/그래프가 비슷한 강조도로 보여 중요도 구분이 없음

### Workbench UI 원칙과 연결
우리 공통 UX 규칙의:

```text
현재 상태
→ 지금 할 행동
→ 결과
→ 상세
```

과 같은 방향입니다.

즉 디자인을 “예쁜 카드 많이 놓기”로 보지 않고:
- 가장 먼저 봐야 하는 정보
- 다음 행동
- 주요/보조 정보의 위계
- 화면을 따라가는 사용자 여정
을 먼저 정해야 합니다.

### 적용처
- Market Radar
- ChungYack
- ORV Inspector
- Burgundy web UI
- 각종 dashboard/settings 화면

---

## 3. AGENTS.md / Skills 경량화 관련 SNS 참고

사용자가 전달한 스크린샷/링크의 요지는 다음과 같습니다.

### 문제 제기
- AGENTS.md가 오래된 지침을 계속 누적
- Skill trigger가 너무 넓음
- 현재 작업과 상관없는 문서를 강제로 로드
- 중복 확인/충돌 규칙으로 Agent가 저위험 작업까지 멈춤
- 모델은 좋아졌는데 옛 모델용 안전장치가 그대로 남음

### 유지해야 한다고 강조된 것
- production environment 보호
- 삭제/권한 변경 같은 위험 작업 경계
- secret/민감정보 보호
- 비가역 작업 승인
- 필요한 테스트

### Workbench에 적용할 원칙
**“무조건 문서를 줄여라”가 아니라 “현재 Agent가 스스로 처리할 수 있는 저위험 절차는 줄이고, 위험 경계와 검증은 유지한다.”**

따라서 정리 순서는:

```text
현재 AGENTS / Skills 조사
→ 실제 방해가 되는 broad trigger / forced loading / duplicate check 식별
→ 최소 수정 제안
→ 위험 경계가 사라지지 않는지 확인
→ 적용
→ 같은 작업을 다시 시켜 속도/정확도 비교
```

### 현재 Workbench와의 관계
이미 적용한 `AGENTS = 영구 역사창고가 아니라 현재 실행본` 원칙과 일치합니다.
추가로 앞으로 Skill을 만들 때 **trigger scope를 좁게 쓰고, 필요한 순간에만 로드**하는 기준을 유지합니다.

---

## 4. 디자인/Agent 참고자료를 후보 도구와 섞지 않는 이유

SceneAI나 디자인 비평 글은 설치할 프로그램이 아닙니다.
따라서 `ACTIVE/PROJECT` 도구로 승격하지 않고 `REFERENCE`로 유지합니다.

반대로 이 참고에서 실제 reusable rule이 검증되면 그때 `PATTERNS.md`나 `USER_POLICIES.md`로 승격합니다.
