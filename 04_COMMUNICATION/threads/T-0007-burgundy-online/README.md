# T-0007 — Burgundy Online

- **Status:** OPEN
- **Project:** Burgundy
- **Repository:** https://github.com/kimjae134679/Burgundy
- **Owner:** user
- **Working AI:** Sol

## Topic
《The Castles of Burgundy》의 실제 규칙/구조를 인터넷의 공식·신뢰 가능한 자료로 검증하면서, 브라우저에서 1인(사람 1 + AI), AI 채우기, 친구 초대, 온라인 방 관리가 가능한 디지털 보드게임 프로토타입을 구축한다.

## Fixed direction
- 생성된 도안 이미지는 분위기/화면 구성 레퍼런스로만 사용한다.
- 게임 규칙, 중앙 보드 구조, 개인 영지 보드, 타일 효과, 점수/턴 처리는 실제 자료를 우선하여 구현한다.
- 1인/AI/멀티를 별도 게임으로 쪼개지 않고 모두 `Room + Seats` 모델로 통일한다.
- 룰 엔진과 UI를 분리하여 AI와 멀티플레이가 같은 합법 행동/상태 전이를 사용하게 한다.
- 공식 로고/일러스트를 그대로 복제하지 않고 프로토타입용 독자 UI 자산을 사용한다.

## Current work
1. 공식/신뢰 가능한 룰 자료를 기준으로 base rules baseline 문서화
2. 빈 Burgundy 저장소에 프로젝트 구조/로드맵/룰 엔진 골격 생성
3. 로컬 hot-seat/AI가 동일 엔진으로 한 판을 완주할 수 있는 vertical slice
4. 이후 온라인 room/invite/reconnect/server-authoritative 구조 연결
