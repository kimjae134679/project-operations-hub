# UI / Design Reference Library

확인일: **2026-10-05**

이 문서는 AI에게 UI/UX/웹/데스크톱 앱 디자인을 맡길 때 **AI 서비스가 아니라, AI에게 보여주고 참고시키는 무료·오픈소스 레퍼런스**를 모읍니다.

핵심 목적은 하나입니다.

> **AI가 첫 생각대로 평범한 카드/그라데이션/대시보드 UI를 찍어내지 않게 하고, 실제 좋은 레퍼런스를 여러 개 비교한 뒤 더 완성도 높은 결과를 만들게 한다.**

---

## 0. 디자인 작업 기본 규칙 — 반드시 먼저 적용

UI/UX를 새로 만들거나 크게 손볼 때 바로 코딩부터 시작하지 않습니다.

### 필수 순서

1. 화면 목적과 가장 중요한 사용자 행동을 먼저 정의합니다.
2. 아래 무료 레퍼런스에서 **최소 3개 이상** 가까운 사례를 찾습니다.
3. 한 사이트만 복제하지 말고 각 레퍼런스에서 좋은 점을 분리합니다.
   - 전체 레이아웃
   - 버튼/입력창/카드
   - 아이콘
   - 색상
   - 여백/타이포
   - 상태 표시
   - 애니메이션/피드백
4. 현재 프로젝트 성격에 맞게 재조합합니다.
5. 구현 후 실제 화면에서 정보 위계, 조작성, 밀도, 가독성을 다시 검증합니다.

### 금지

- 아무 레퍼런스도 안 보고 AI 기본 스타일 그대로 구현
- 모든 화면을 둥근 카드 + 그라데이션 + 큰 제목으로 통일
- 기능만 들어갔다는 이유로 디자인 완료 처리
- "예쁘게", "모던하게" 같은 추상적인 말만으로 디자인 결정
- 한 레퍼런스를 거의 그대로 복제해 프로젝트 정체성을 없애기
- 유료 템플릿/유료 에셋을 기본 전제로 설계

### 목표

**레퍼런스를 베끼는 것이 아니라 좋은 디자인 결정을 수집해서 조합한다.**

---

# 1. UI 컴포넌트 / 앱 화면

## Uiverse

- https://uiverse.io/
- 비용: **무료**
- 라이선스: 주로 MIT 기반 공개 컴포넌트
- 용도: 버튼, 토글, 체크박스, 입력창, 카드, 로더, 애니메이션 UI

### AI에게 주는 방식

```text
이 Uiverse 컴포넌트의 구조와 인터랙션을 참고하되
현재 프로젝트 디자인 시스템에 맞게 색상, 크기, 여백, 상태를 다시 설계해.
그대로 복붙하지 말고 더 일관되게 통합해.
```

---

## shadcn/ui

- https://ui.shadcn.com/
- 비용: **무료 / 오픈소스**
- 용도: 대시보드, 사이드바, 로그인, 설정, 폼, 테이블, 차트, 전체 앱 구조

Uiverse가 작은 부품이라면 shadcn/ui는 **화면과 앱 구조 참고용**입니다.

특히 참고할 것:
- sidebar
- dashboard
- forms
- dialogs
- data table
- command palette
- settings layout

---

## HyperUI

- https://www.hyperui.dev/
- 비용: **무료 / MIT**
- 용도: 앱 UI, 관리자 화면, 모달, 테이블, 탭, 진행상태, 폼

실사용 프로그램 UI를 만들 때 화려함보다 **구조와 조작성** 참고용으로 좋습니다.

---

## daisyUI

- https://daisyui.com/
- 비용: **무료 / 오픈소스**
- 용도: 컴포넌트, 테마, 상태 스타일, 빠른 디자인 시스템 비교

특정 컴포넌트를 복사하기보다 **한 앱 전체의 스타일 일관성**을 잡을 때 참고합니다.

---

# 2. 아이콘

## Lucide

- https://lucide.dev/
- 비용: **무료 / 오픈소스**
- 라이선스: ISC
- 용도: 프로그램 기본 아이콘

기본 우선순위가 가장 높습니다.

예:
- Settings
- FolderOpen
- Trash2
- Download
- Upload
- RefreshCw
- Search
- Play
- Pause
- CircleCheck
- TriangleAlert

가능하면 화면마다 제멋대로 다른 아이콘 세트를 섞지 않습니다.

---

## Tabler Icons

- https://tabler.io/icons
- 비용: **무료 / 오픈소스**
- 라이선스: MIT
- 용도: Lucide와 다른 느낌이 필요할 때

---

## Heroicons

- https://heroicons.com/
- 비용: **무료 / MIT**
- 용도: 단순하고 정돈된 앱/웹 아이콘

---

## Bootstrap Icons

- https://icons.getbootstrap.com/
- 비용: **무료 / MIT**
- 용도: 범용 아이콘, 상태/도구/파일 관련 아이콘

---

## Iconoir

- https://iconoir.com/
- 비용: **무료 / 오픈소스**
- 용도: 조금 더 개성 있는 선형 아이콘 대안

---

## Simple Icons

- https://simpleicons.org/
- 비용: **무료 / 오픈소스**
- 용도: GitHub, YouTube, Discord 등 브랜드 아이콘

브랜드 로고는 임의로 비슷하게 그리지 말고 공식/공개 아이콘을 우선합니다.

---

# 3. SVG / 일러스트 / 그래픽

## SVG Repo

- https://www.svgrepo.com/
- 비용: **무료 자료 중심**
- 용도: SVG 아이콘, 그림, 벡터

주의:
- 사이트 자체는 무료 자료 저장소지만 **각 SVG 라이선스는 개별 확인**합니다.
- 상업 사용 프로젝트라면 라이선스가 명확한 항목을 고릅니다.

---

## unDraw

- https://undraw.co/illustrations
- 비용: **무료**
- 용도:
  - 빈 화면
  - 완료 화면
  - 로그인
  - 검색 결과 없음
  - 업로드/다운로드
  - 오류/안내 상태

기능 화면에 장식 일러스트를 남발하지 말고 실제로 빈 상태나 설명이 필요한 곳에만 사용합니다.

---

## Openverse

- https://openverse.org/
- 비용: **무료 검색**
- 용도: Creative Commons 이미지/오디오 레퍼런스 검색

각 결과의 실제 라이선스를 확인합니다.

---

# 4. 배경 / 패턴 / 장식

## BGJar

- https://bgjar.com/
- 비용: **무료**
- 용도: SVG 배경, 패턴, wave, grid, blob, circuit 등

AI가 쓸데없이 무거운 배경 이미지를 새로 생성하는 대신 가벼운 SVG 장식이 더 적합한 경우 사용합니다.

---

## Get Waves

- https://getwaves.io/
- 비용: **무료**
- 용도: SVG wave 생성

---

## Blobmaker

- https://www.blobmaker.app/
- 비용: **무료**
- 용도: 간단한 SVG blob 생성

---

## Neumorphism.io

- https://neumorphism.io/
- 비용: **무료**
- 용도: shadow 값을 시각적으로 확인하는 CSS 생성기

Neumorphism 스타일을 그대로 쓰라는 의미가 아니라 **shadow 강도와 방향을 빠르게 비교**할 때 사용합니다.

---

# 5. 색상 / 타이포그래피

## Open Color

- https://yeun.github.io/open-color/
- 비용: **무료 / 오픈소스**
- 용도: 검증된 색상 팔레트

AI가 임의의 색을 너무 많이 만드는 대신 기준 팔레트로 사용하기 좋습니다.

---

## Google Fonts

- https://fonts.google.com/
- 비용: **무료 / 오픈소스 폰트**
- 용도: UI 폰트, 글꼴 조합, 가변 폰트

프로젝트 배포 방식과 실제 폰트 라이선스를 확인합니다.

---

## Fontshare

- https://www.fontshare.com/
- 비용: **무료 폰트**
- 용도: Google Fonts 외의 개성 있는 무료 폰트 탐색

---

# 6. CSS / 애니메이션 레퍼런스

## Animista

- https://animista.net/
- 비용: **무료**
- 용도: CSS animation 동작 참고 및 값 조정

애니메이션은 장식보다:
- 상태 변화
- 완료 피드백
- 패널 전환
- hover/focus
를 이해시키는 데 우선 사용합니다.

---

## CSSFX

- https://cssfx.netlify.app/
- 비용: **무료 / 오픈소스**
- 용도: 작은 CSS 효과와 인터랙션 참고

---

# 7. AI가 실제로 디자인할 때의 조합 예시

## 데스크톱 관리 프로그램

```text
전체 구조
→ shadcn/ui + HyperUI에서 각각 2개 이상 레이아웃 비교

세부 컨트롤
→ Uiverse

아이콘
→ Lucide 우선, 부족하면 Tabler Icons

색상
→ Open Color

빈 상태/안내
→ unDraw 또는 SVG Repo

미세한 전환
→ Animista / CSSFX
```

## 다운로드/변환 도구

```text
1. HyperUI에서 실제 application layout 참고
2. Uiverse에서 progress / button / toggle 참고
3. Lucide에서 Download / FolderOpen / RefreshCw / CircleCheck 사용
4. Open Color에서 상태색 통일
5. 진행상황이 가장 먼저 보이도록 정보 위계 설계
```

## 대시보드

```text
1. shadcn/ui dashboard 사례 여러 개 비교
2. 카드 수를 먼저 줄이고 정보 우선순위를 정의
3. HyperUI table/form 패턴 참고
4. Lucide 아이콘 통일
5. 장식보다 현재 상태 → 행동 → 결과 순서를 우선
```

---

# 8. 디자인 품질 체크리스트

구현 완료 전에 아래를 확인합니다.

- [ ] 실제 레퍼런스를 최소 3개 이상 봤는가
- [ ] 각각에서 무엇을 참고했는지 설명할 수 있는가
- [ ] 화면에서 가장 중요한 정보가 1초 안에 보이는가
- [ ] 대표 CTA가 하나로 명확한가
- [ ] 모든 카드와 버튼이 똑같이 강조되어 있지 않은가
- [ ] 여백과 정렬 규칙이 일정한가
- [ ] 아이콘 세트가 통일되어 있는가
- [ ] 필요 없는 테두리/그라데이션/그림자를 줄였는가
- [ ] hover/focus/disabled/loading/error/success 상태가 있는가
- [ ] 창 크기가 작아져도 핵심 기능이 유지되는가
- [ ] 한글 텍스트가 길어져도 깨지지 않는가
- [ ] 기능 구현만 끝내고 디자인 완료라고 부르지 않았는가
- [ ] 유료 에셋 없이도 재현 가능한가

---

# 9. 운영 원칙

이 문서의 사이트들은 **AI 도구 후보가 아니라 REFERENCE**입니다.

- 설치할 필요 없음
- AI 기본 컨텍스트에 항상 넣지 않음
- UI/UX 작업이 발생했을 때 필요한 항목만 확인
- 최신 디자인이 필요한 작업은 이 목록에만 갇히지 말고 무료·오픈소스 레퍼런스를 추가 조사
- 좋은 새 레퍼런스가 확인되면 이 문서에 추가
- 유료 전용/무료체험 중심 자료는 기본 목록에 넣지 않음

최종 목표는 **AI가 매번 같은 디자인을 반복하는 것을 막고, 실제 좋은 사례를 조사해서 프로젝트마다 더 적합하고 완성도 높은 UI를 만드는 것**입니다.
