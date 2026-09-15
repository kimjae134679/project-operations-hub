# 03_KNOWLEDGE

프로젝트에서 재사용할 수 있는 검증 패턴, 경험, 도구 후보를 관리합니다.

- `PATTERNS.md` — 공통 구현/검증 패턴
- `LESSONS.md` — 프로젝트별 경험 요약
- [`CANDIDATE_CATALOG/`](CANDIDATE_CATALOG/) — **후보 도구의 canonical 조사/분류 위치**
  - `MASTER_INDEX.md` — 상태/설치 여부/설치 부담/상세문서 위치를 한 번에 보는 인덱스
  - `INSTALLATION_BURDEN.md` — ⚪/🟢/🟡/🟠/🔴 설치·운영 부담 분류
  - 주제별 상세 조사 문서
  - `SOURCE_BOOKMARKS.md` — 사용자가 준 원문과 공식 원본
- `TOOL_CANDIDATES.md` — 이전에 누적한 장문 후보 카탈로그. **호환/과거 참고용이며 새 후보의 canonical 위치가 아님. 새 내용은 CANDIDATE_CATALOG에 넣습니다.**
- `KNOWN_GOOD_GUIDE.md` — 정상 기준판 기록 방식

## 후보 상태를 섞지 않기

세 가지는 별개입니다.

```text
ADOPTION  = 실제 채택 상태
INSTALLED = 실제 PC 설치 여부
BURDEN    = 설치/운영 무게
```

- 실제 채택 상태 원본 → `01_CONTROL/TOOLS.md`
- 실제 버전/설치 위치 원본 → `01_CONTROL/AI_INSTALLATIONS.md`
- 조사/비교/후보 원본 → `03_KNOWLEDGE/CANDIDATE_CATALOG/`

따라서 `후보 문서에 있음 = 설치됨`, `설치됨 = ACTIVE`로 판단하지 않습니다.

## 후보 도구 조사 원칙

새 후보를 추가할 때 가능한 한 공식 README/문서 기준으로 다음을 확인합니다.

- 정확히 무엇인지: 프로그램 / Agent / Skill / MCP / framework / 참고자료
- canonical repo와 redirect/이름 변경 여부
- 실제 기능과 동작 구조
- 설치·OS·runtime 요구사항
- **설치 부담: NONE/LIGHT/MEDIUM/HEAVY/VERY_HEAVY**
- Cloud/API와 Self-host 부담 차이
- 계정/OAuth/API key/권한
- 라이선스·비용·self-host 범위
- 우리 프로젝트의 구체적 적용처
- 위험요소와 먼저 해야 할 테스트
- SNS 홍보문구와 공식 원본의 차이

🔴 `VERY_HEAVY` 후보는 ComfyUI/vLLM/RAGFlow/대형 로컬 모델처럼 GPU·대형 model·수십 GB 저장공간·고 RAM·다중 컨테이너 등이 따라오는 계열입니다. 가벼운 CLI와 같은 설치 후보처럼 취급하지 않고 별도 표기합니다.

후보는 실제 설치·테스트 후에만 `01_CONTROL/TOOLS.md`의 `ACTIVE` 또는 `PROJECT`로 승격합니다. 자동 hook/MCP/rules/config를 설치하는 도구는 빈 테스트 저장소에서 먼저 diff와 rollback을 확인합니다.

새 팁을 의무적으로 추가하지 않습니다. 구체적인 실패를 막거나 다시 재사용된 경우에만 공통 패턴으로 승격합니다.

## 추가 공통 패턴
- [`WINDOWS_TERMINAL_PROGRESS.md`](WINDOWS_TERMINAL_PROGRESS.md) — Windows 빌드/설치용 Unicode `━` Rich 스타일 터미널 진행 표시와 fallback 규칙
- [`CANDIDATE_CATALOG/UI_DESIGN_REFERENCES.md`](CANDIDATE_CATALOG/UI_DESIGN_REFERENCES.md) — UI/motion 참고자료
