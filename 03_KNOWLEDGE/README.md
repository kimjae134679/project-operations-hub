# 03_KNOWLEDGE

프로젝트에서 재사용할 수 있는 검증 패턴, 경험, 도구 후보를 관리합니다.

- `PATTERNS.md` — 공통 구현/검증 패턴
- `LESSONS.md` — 프로젝트별 경험 요약
- `TOOL_CANDIDATES.md` — 기존 후보 도구의 **종류·용도·우리 적용처·설치법·계정/비용·Windows 호환·위험·채택 우선순위**까지 조사한 장문 카탈로그
- [`CANDIDATE_CATALOG/`](CANDIDATE_CATALOG/) — 새로 전달받은 후보들을 주제별로 나눠 공식 원본 기준으로 재검증한 구조화 카탈로그
- `KNOWN_GOOD_GUIDE.md` — 정상 기준판 기록 방식

## 후보 도구 조사 원칙

단순 북마크로 끝내지 않습니다. 새 후보를 추가할 때 가능한 한 공식 README/문서 기준으로 다음을 확인합니다.

- 정확히 무엇인지: 프로그램 / Agent / Skill / MCP / framework / 참고자료
- 실제 기능과 동작 구조
- 설치·OS·runtime 요구사항
- 계정/OAuth/API key/권한
- 라이선스·비용·self-host 범위
- 우리 프로젝트의 구체적 적용처
- 위험요소와 먼저 해야 할 테스트
- SNS 홍보문구와 공식 원본의 차이

후보는 기본적으로 `CANDIDATE`이며 실제 설치·테스트 후에만 `01_CONTROL/TOOLS.md`의 `ACTIVE` 또는 `PROJECT`로 승격합니다. 자동 hook/MCP/rules/config를 설치하는 도구는 빈 테스트 저장소에서 먼저 diff와 rollback을 확인합니다.

`CANDIDATE_CATALOG/SOURCE_BOOKMARKS.md`에는 사용자가 준 원문 링크도 보존하고, 현재 조사 환경에서 본문을 읽지 못한 링크는 `SOURCE-UNVERIFIED`로 명시합니다.

새 팁을 의무적으로 추가하지 않습니다. 구체적인 실패를 막거나 다시 재사용된 경우에만 공통 패턴으로 승격합니다.

## 추가 공통 패턴
- [`WINDOWS_TERMINAL_PROGRESS.md`](WINDOWS_TERMINAL_PROGRESS.md) — Windows 빌드/설치용 Unicode `━` Rich 스타일 터미널 진행 표시와 fallback 규칙
- [`CANDIDATE_CATALOG/UI_DESIGN_REFERENCES.md`](CANDIDATE_CATALOG/UI_DESIGN_REFERENCES.md) — SceneAI, Scrolltide 등 UI/motion 참고자료
