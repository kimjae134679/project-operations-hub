# 03_KNOWLEDGE

프로젝트에서 재사용할 수 있는 검증 패턴, 경험, 도구 후보를 관리합니다.

- `PATTERNS.md` — 공통 구현/검증 패턴
- `LESSONS.md` — 프로젝트별 경험 요약
- `TOOL_CANDIDATES.md` — 후보 도구의 **종류·용도·우리 적용처·설치법·계정/비용·Windows 호환·위험·채택 우선순위**까지 조사한 카탈로그
- `KNOWN_GOOD_GUIDE.md` — 정상 기준판 기록 방식

`TOOL_CANDIDATES.md`의 링크는 단순 북마크가 아닙니다. 새 후보를 추가할 때 가능한 한 공식 README/문서 기준으로 실제 용도와 설치·권한·비용·호환성·주의점을 함께 적습니다.

후보는 기본적으로 `CANDIDATE`이며 실제 설치·테스트 후에만 `01_CONTROL/TOOLS.md`의 `ACTIVE` 또는 `PROJECT`로 승격합니다. 자동 hook/MCP/rules/config를 설치하는 도구는 빈 테스트 저장소에서 먼저 diff와 rollback을 확인합니다.

새 팁을 의무적으로 추가하지 않습니다. 구체적인 실패를 막거나 다시 재사용된 경우에만 공통 패턴으로 승격합니다.
