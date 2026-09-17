# ty (Astral)

## 한줄 판단
Python 프로젝트의 정적 타입 검사를 빠르게 수행하는 CLI/LSP 후보. 이미 `uv`를 쓰는 환경에서는 `uvx ty check`로 영구 설치 없이 시험 가능하다.

## 분류
- 현재 바로 사용: 예 — `uvx ty check` 또는 standalone 설치
- ChatGPT/연결 서비스라 설치 없이 사용: 아니오. 단 `uv`가 있으면 `uvx`로 영구 설치 없이 실행 가능
- AI가 설치·설정: 가능
- 핵심 기능 AI 단독 운용: **AI 단독 가능**
- 사용자 1회 도움: 보통 없음. UAC가 필요한 설치 위치를 선택할 때만 예외
- 사용자 GUI 필요: 없음. 에디터 LSP는 선택 사항
- 대상: 주로 AI용 + 개발자용
- 실제 설치/채택: **후보 — 설치/채택 확인 전**
- 설치·운영 부담: 🟢 가벼움
- Cloud/self-host: 해당 없음, 로컬 CLI/LSP
- 비용: **무료/오픈소스**
- 외부 실행비용: 없음

## 왜 볼 가치가 있나
- Astral의 Python type checker/language server.
- `ty check`로 프로젝트 전체 또는 지정 경로의 타입 오류를 검사한다.
- `.venv`, 활성 virtualenv, Python 경로를 탐색해 dependency 정보를 사용한다.
- `ty check --watch`의 incremental watch mode도 제공한다.
- `uvx ty check`로 빠르게 시험할 수 있어 기존 `uv` 후보와 잘 맞는다.
- Ruff가 lint/format을 맡는다면 ty는 **정적 타입 검사**를 맡아 역할이 겹치지 않는다.

## 권장 운용
```text
Python 수정
→ ruff check / ruff format --check
→ ty check
→ 테스트
→ git diff 확인
```

처음부터 모든 Python 저장소의 필수 gate로 강제하지 않는다. 기존 mypy/Pyright 설정이 있는 프로젝트에서는 결과 차이를 먼저 비교하고, 충분히 검증한 뒤 대체 여부를 결정한다.

## 주의
- 타입 검사기는 실제 runtime test를 대체하지 않는다.
- 프로젝트의 `requires-python`, virtualenv 탐지 상태에 따라 결과가 달라질 수 있으므로 환경을 맞춰 검사한다.
- 공식 문서는 현재 기능을 적극적으로 제공하지만, 기존 mature type checker와 진단 차이가 있을 수 있으므로 도입 초기에는 보조 검사기로 취급하는 편이 안전하다.
- 에디터 extension/LSP 사용은 선택 사항이며 AI 자동 검사에는 GUI가 필요 없다.
- 인터넷에서 내려받은 installer script는 실행 전에 내용을 확인하거나 공식 release binary/`uvx`를 우선한다.

## 공식 자료
- https://docs.astral.sh/ty/
- https://docs.astral.sh/ty/type-checking/
- https://docs.astral.sh/ty/installation/
- https://github.com/astral-sh/ty

검토일: 2026-09-17
