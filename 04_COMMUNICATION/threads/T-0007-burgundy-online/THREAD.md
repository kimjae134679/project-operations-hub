# T-0007 — Burgundy Online

> 이 파일은 이 주제의 전체 소통 기록입니다. 새 댓글도 별도 파일을 만들지 않고 이 파일 맨 아래에 새 구역으로 추가합니다.

> 통합일: 2026-09-19 KST. 병합 전 개별 파일은 Git 이력에서 확인할 수 있습니다.

---

## 원본 기록: README.md

### T-0007 — Burgundy Online

- **Status:** OPEN
- **Project:** Burgundy
- **Repository:** https://github.com/kimjae134679/Burgundy
- **Owner:** user
- **Working AI:** Sol

#### Topic
《The Castles of Burgundy》의 실제 규칙/구조를 인터넷의 공식·신뢰 가능한 자료로 검증하면서, 브라우저에서 1인(사람 1 + AI), AI 채우기, 친구 초대, 온라인 방 관리가 가능한 디지털 보드게임 프로토타입을 구축한다.

#### Fixed direction
- 생성된 도안 이미지는 분위기/화면 구성 레퍼런스로만 사용한다.
- 게임 규칙, 중앙 보드 구조, 개인 영지 보드, 타일 효과, 점수/턴 처리는 실제 자료를 우선하여 구현한다.
- 1인/AI/멀티를 별도 게임으로 쪼개지 않고 모두 `Room + Seats` 모델로 통일한다.
- 룰 엔진과 UI를 분리하여 AI와 멀티플레이가 같은 합법 행동/상태 전이를 사용하게 한다.
- 공식 로고/일러스트를 그대로 복제하지 않고 프로토타입용 독자 UI 자산을 사용한다.

#### Current work
1. 공식/신뢰 가능한 룰 자료를 기준으로 base rules baseline 문서화
2. 빈 Burgundy 저장소에 프로젝트 구조/로드맵/룰 엔진 골격 생성
3. 로컬 hot-seat/AI가 동일 엔진으로 한 판을 완주할 수 있는 vertical slice
4. 이후 온라인 room/invite/reconnect/server-authoritative 구조 연결

---

## 원본 기록: 001-sol.md

### 001 — Sol

사용자 요청 확인.

이번 프로젝트는 UI 이미지를 먼저 따라 만드는 방식이 아니라 **규칙 정확도 → 상태 모델 → 합법 행동 판정 → AI/멀티 공용 엔진 → 화면** 순서로 잡는 것이 안전하다.

특히 확인할 항목은 다음과 같다.
- 5 phases × 5 rounds
- 각 턴의 2개 주사위 = 정확히 2 actions
- workers의 ±1 보정 및 1↔6 순환
- 1~6 numbered depots, central black depot, goods 전달용 white die
- tile take는 먼저 개인 storage로, estate 직접 배치 금지
- estate 배치는 색 일치 + 기존 타일 인접 + 주사위 숫자 일치
- ship / animal / castle / mine / 8 building types / knowledge tiles 효과
- black depot 구매는 Silverlings 2, 턴당 1회
- 영역 완성 점수와 전체 색 완성 bonus
- 플레이 인원수에 따른 공급량/상품 판매 점수/bonus 차이

웹 구조는 `Room -> Seat[] -> human | ai | empty`를 기준으로 하며, 1인 버튼은 실제로는 사람 1명 + AI가 있는 비공개 room을 즉시 생성하는 단축 동작으로 처리할 예정이다.

초기 구현은 저장소에 규칙 기준 문서, 로드맵, TypeScript 룰 엔진 골격, 웹 UI 골격을 먼저 올리고 이후 실제 한 판 완주가 가능한 vertical slice를 만든다.

공식/신뢰 가능한 룰 자료와 구현이 충돌하면 이미지 도안보다 룰 자료를 우선한다.

---

## 원본 기록: 002-sol.md

### 002 — Sol progress

Burgundy 저장소 초기 작업 완료.

#### 확인한 규칙 자료
- Awaken Realms Special Edition 공식 rulebook/downloads를 canonical source로 사용
- Board Game Arena / UltraBoardGames를 교차 검증 자료로 사용
- player estate가 총 37 hex spaces라는 점, 6 numbered depots + central black depot, 5 phases × 5 rounds, 2 dice actions, white die goods 처리, worker 1↔6 보정, region/color completion scoring 등을 구현 기준에 반영
- Special Edition에는 공식 Solo expansion과 Chateauma가 별도로 존재하므로, 사용자가 원하는 `사람 1 + AI` 일반게임과 공식 Solo를 섞지 않고 별도 모드로 둘 계획

#### Burgundy repo에 추가한 것
- `README.md`
- `docs/ROADMAP.md`
- `docs/RULES_BASELINE.md`
- `docs/ARCHITECTURE.md`
- npm workspace
- `packages/game-core`: room/seat model, 규칙 type, 점수/worker 보정 helper, 회귀 테스트
- `apps/web`: React/Vite 로비 + 방 인원/AI 관리 + 테이블 구조 vertical slice
- GitHub Actions CI

#### 현재 검증
CI에서 dependency install / game-core tests / typecheck / web build 모두 성공.

#### 다음 구현 순서
1. Special Edition duchy board #1의 37 hex 좌표/색/printed die number를 실제 자료에서 데이터화
2. numbered depot의 2/3/4인 공급 slot 규칙 데이터화
3. `take tile / place tile / sell goods / take workers / black-depot buy` command validator/reducer
4. building 8종 및 ship/animal/castle/mine 효과
5. phase/round 진행과 final scoring
6. 그 후 AI legal-action search 연결

도안 이미지는 계속 레이아웃 참고로만 사용하고 실제 보드 데이터는 rulebook/실물 레퍼런스를 기준으로 고정한다.

