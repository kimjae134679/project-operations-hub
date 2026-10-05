# OpenAI Prism — 2026-09-17 검증 메모

공식 서비스: https://prism.openai.com/

공식 소개: https://openai.com/index/introducing-prism/

공식 도움말: https://help.openai.com/en/articles/20001050-troubleshooting-and-getting-help-in-prism

상태: `CANDIDATE / HIGH-FIT / VERIFY-FIRST`

설치 부담: ⚪ `NONE` — 웹 서비스. 다만 프로젝트 파일을 클라우드에 올려 쓰는 구조라 보안/데이터 경계는 반드시 별도로 봅니다.

---

## 먼저 결론

Prism은 현재 **Codex 대체재가 아니라, 문서/LaTeX 프로젝트 전체 맥락을 읽고 추론·편집하는 별도 AI 작업면(surface)** 으로 보는 것이 정확합니다.

사용자가 전달한 "Codex 한도를 다 쓴 뒤에도 Prism에서 GPT-6 Astra Extra High가 계속 작동했다 → 별도 무제한 풀이다"라는 주장은 **공식적으로 확인되지 않았습니다.**

2026-09-16 Reddit의 Codex 이슈 수집 스레드에는 실제로 다음 제목의 사용자 보고가 1건 확인됩니다.

> Potential new usage pool? Open AI Prism site still gives me GPT-6 Astra Extra High after my Codex limits are exhausted

하지만 현재 공식 Prism 문서는:

- Prism이 `frontier models` 기반이라고만 설명하는 최신 도움말과
- 출시 글에서 `GPT-5.2 Thinking`을 명시한 문서

까지 확인되며, **GPT-6 Astra Extra High를 Prism에서 무제한 제공한다거나 Codex와 quota pool이 분리되어 있다는 공식 설명은 찾지 못했습니다.**

따라서 현재 표기는:

- `Astra Extra High 노출 가능성` — **USER-REPORTED / NOT OFFICIALLY CONFIRMED**
- `Codex와 별도 usage pool` — **UNVERIFIED**
- `사실상 무제한` — **UNVERIFIED, 그렇게 취급하면 안 됨**

으로 둡니다.

---

## 중국 서비스인가?

**아닙니다.** 현재 Prism은 OpenAI가 직접 운영하는 공식 서비스입니다.

OpenAI 공식 소개에 따르면 Prism은 OpenAI가 인수한 클라우드 LaTeX 플랫폼 **Crixet**을 기반으로 발전했습니다. 외부 기업 프로필에서는 Crixet을 2024년 미국 Austin, Texas에서 설립된 회사로 기재하고 있습니다.

즉 현재 확인된 근거상 `중국 서비스`로 분류할 이유는 없습니다.

다만 **중국 서비스가 아니더라도 클라우드 AI에 비공개 소스·비밀키·민감 문서를 올리는 보안 위험은 별개**이므로 아래 데이터 정책은 엄격하게 봅니다.

---

## 실제 기능

공식 문서 기준 Prism은:

- 클라우드 기반 LaTeX editor/workspace
- 프로젝트 전체 문맥을 보는 AI assistant
- LaTeX 생성/수정
- 수식·표·참고문헌·figure 문맥 이해
- compile 오류 진단
- 문헌 검색/인용 보조
- Zotero 연동
- 음성 기반 편집
- 이미지/화이트보드 수식을 LaTeX로 변환
- 공동 편집
- 폴더 또는 `.zip` 프로젝트 import
- 프로젝트 `.zip` export

을 지원합니다.

반면 2026-09-17 공식 도움말 기준:

- **Git/GitHub/GitLab 동기화 없음** — high-priority future feature
- 로컬 repository에 직접 붙는 Codex식 작업 아님
- terminal / shell execution 없음
- 우리 `AGENTS.md`, Skill, MCP, harness를 자동으로 물려받는 개발 Agent가 아님

즉 현재는:

```text
Prism
= 강한 모델 + 문서 프로젝트 전체 context + 편집기

Codex / Work
= 강한 모델 + 파일/터미널/도구/실행/검증 harness
```

에 가깝습니다.

---

## 보안 / 개인정보 — 중요

공식 Prism 도움말에서 확인된 현재 상태:

1. **Zero Data Retention(ZDR)을 현재 사용하지 않음.**
2. 제품 개선을 위해 요청 후 **일정 기간 로그를 유지**한다고 명시.
3. `텍스트를 저장하지 않음 / 사람이 검토하지 않음`을 보장하는 별도 privacy mode는 현재 없음.
4. EU-only data residency도 현재 제공되지 않음.
5. 사용자는 자신의 콘텐츠를 모델 개선/학습에 사용할지 여부를 OpenAI 데이터 제어에서 선택할 수 있음.

따라서 우리 기준으로 Prism에는 다음을 올리지 않습니다.

- API key / token / password / cookie
- keystore / signing key
- `.env` 원본
- 고객 개인정보
- 원본 계정 DB / 인증 dump
- 비공개 운영 서버 자격증명
- 공개 전 민감 문서 전체
- 보안 취약점 분석 중 외부 유출되면 안 되는 원본

코드를 넣어야 한다면 먼저:

```text
secret scan
→ credential 제거
→ 필요한 파일/부분만 추출
→ Prism에 업로드
→ 결과 회수
```

방식으로 씁니다.

---

## 우리한테 쓸모 있는가?

### 쓸모 큼

Prism에서 실제로 Astra급 모델 접근이 유지되는 계정이라면:

- 복잡한 설계안 검토
- 긴 로그/문서의 원인 분석
- 인수인계 문서 재구성
- 여러 파일을 한 묶음으로 읽혀 구조 분석
- 논문/기술문서/수식 작업
- Codex가 손발을 쓰기 전에 `두뇌 작업`만 따로 시키기

에 가치가 있습니다.

특히 우리 구조에서는:

```text
Prism
  └─ 분석 / 설계 / 문서 / 검토
        ↓
ChatGPT Work / Codex / Remote Desktop
  └─ 실제 파일 수정 / terminal / build / device test / Git push
```

처럼 분업할 수 있습니다.

### 대체 불가

다음 작업에는 Prism만으로 부족합니다.

- repo 직접 수정
- terminal 명령
- build/test
- APK 설치
- 실기기 검증
- Git commit/push
- Remote Desktop 작업
- 자동 반복 실행

---

## 현재 시험 가치

`HIGH-FIT / VERIFY-FIRST`로 둡니다.

테스트할 때는 민감정보 없는 작은 샘플 프로젝트로 다음 4가지만 확인하면 됩니다.

1. Prism UI에서 실제 표시 모델명이 `GPT-6 Astra`인지
2. reasoning 옵션에 `Extra High`가 실제 있는지
3. Codex 5-hour / weekly limit 소진 후에도 Prism 요청이 성공하는지
4. Prism 쪽에 별도 usage/reset 표시 또는 throttling이 생기는지

한 번 성공했다고 `무제한`으로 확정하지 않습니다. 최소 여러 시간대와 limit reset 전후를 나눠 확인합니다.

---

## 현재 판단

- 공식 OpenAI 서비스: **YES**
- 중국 서비스: **NO evidence / 현재 근거상 아님**
- 무료 Prism workspace: **YES**
- GPT-6 Astra Extra High 공식 보장: **NO**
- Codex와 quota 완전 분리 공식 보장: **NO**
- 사실상 무제한 공식 보장: **NO**
- 설치 필요: **NO**
- 보안 민감자료 업로드 권장: **NO**
- 비민감 분석/문서 두뇌용 시험 가치: **YES**

최종 승격 조건: 사용자의 실제 계정에서 모델명/usage behavior를 직접 확인한 뒤 `CANDIDATE → PROJECT` 여부 결정.
