# Ruff

- 공식 프로젝트: `astral-sh/ruff`
- 종류: Python linter + formatter CLI
- 현재 바로 사용: 예. 설치 또는 `uvx ruff` 필요
- 설치 없이 ChatGPT 연결 서비스: 아니오
- AI가 설치·설정: 가능
- 핵심 기능 AI 운용: `AI 단독 가능`
- 사용자 1회 도움: 일반적인 로컬 사용에는 없음. 시스템 정책/UAC가 설치를 막는 경우만 예외
- 사용자 GUI 필요: 없음
- 주 사용자: AI/사용자 둘 다
- 실제 설치/채택: 후보. 설치 확인 전에는 설치됨으로 올리지 않음
- 부담: 🟢 가벼움
- Cloud/self-host: 해당 없음. 로컬 CLI
- 비용: 무료/오픈소스
- 라이선스: MIT
- 외부 실행비용: 없음

## 왜 후보인가

Python 프로젝트에서 lint와 format을 한 CLI로 처리한다. `ruff check`와 `ruff format`을 비대화된 Python 품질 도구 묶음 대신 사용할 수 있고, 기존에 정리한 `uv`와도 공식적으로 잘 맞는다. `uvx ruff check`, `uvx ruff format`처럼 별도 영구 설치 없이 실행하거나 `uv tool install ruff@latest`로 설치할 수 있다.

Windows용 standalone installer와 prebuilt binary도 제공되므로 Rust toolchain을 설치할 필요가 없다.

## AI 운용 관점

GUI나 로그인/API key/OAuth가 필요하지 않아 AI가 코드 검사→결과 판독→수정→재검사를 처음부터 끝까지 수행하기 쉽다. `pyproject.toml`, `ruff.toml`, `.ruff.toml`로 프로젝트 설정을 고정할 수 있어 새 세션에서도 같은 검사 기준을 재사용하기 좋다.

## 안전 운용

- 첫 적용은 `ruff check`와 `ruff format --check`처럼 비파괴 검사부터 한다.
- `ruff check --fix`와 `ruff format`은 소스를 변경하므로 기존 프로젝트에서는 diff를 확인한다.
- `--preview`는 불안정/변경 가능 기능이므로 기본 자동화에 임의 활성화하지 않는다.
- formatter가 import sorting까지 자동으로 하는 것으로 오해하지 않는다. 필요하면 lint의 import rule과 formatter를 순서대로 사용한다.
- 기존 Black/Flake8/isort 설정이 있는 프로젝트는 바로 제거하지 말고 결과 동등성 확인 후 통합 여부를 판단한다.

## 비용 정책 판정

Ruff 자체는 MIT 무료 오픈소스이며 로컬 lint/format에 외부 API 비용이 없다. 따라서 사용자 정책상 유료/무료체험 예외 없이 기본 후보로 검토 가능하다.

확인일: 2026-09-17
공식 문서 기준: `https://docs.astral.sh/ruff/`, `https://github.com/astral-sh/ruff`
