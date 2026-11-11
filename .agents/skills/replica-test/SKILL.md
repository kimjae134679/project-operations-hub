---
name: replica-test
description: "기존 앱 재현·클론 맥락에서 재현 앱의 동작·오류·흐름 검사 단계가 필요할 때만 사용합니다. 일반 개발이나 URL만으로 활성화하지 않습니다."
---

## 현재 호출 범위
- 기존 앱 재현·클론을 요청한 작업 맥락에서 이 단계가 필요할 때만 선택한다.
- URL만 있는 질문, 일반 개발·로그인 추가·디자인 수정 요청에 클론 작업을 시작하지 않는다.
- 다음 단계 추천은 그 단계의 구현·배포·게시·결제 승인이 아니다.
- 사용자 승인 범위와 실제 도구·계정 권한을 우선하고 기존 안전·저작권 경계를 유지한다.

# replica-test

## Codex compatibility and authorization

This installed copy is adapted for Codex; the upstream workflow remains scoped to
this skill and the current user-authorized project. Mentions of the next stage
are suggestions, not permission to start it. Do not deploy, publish, change live
accounts/DNS, enable paid services, or perform real payment/refund tests without
separate explicit authorization. Use existing/free local tools by default; do
not call external models or paid APIs merely to run this pack.

Keep the working directory in the authorized project. Resolve bundled helpers
from the actual installed skill directory, not the project current directory.
Discover an existing supported Python interpreter from this runtime or approved
local installation metadata; do not assume a particular PC account or Python version.
In a repository environment, these helpers live beside the SKILL.md files under
.agents/skills. Do not install an interpreter or execute a remote script merely
to use this pack.

Helpers are local standard-library tools, not sandboxes. Use owned, bounded local
inputs; never sweep credential/home directories or symlink/junction/reparse
inputs. PNG decoding has no resource cap, so use trusted ordinary screenshots.
`imgdiff.py --out` and `reviews.py --out` can overwrite a named file: choose a new
owned output, preserve existing data, and do not run on untrusted output paths.
Verify changing service/pricing/store/legal claims with current primary sources;
these instructions and listing limits are not a legal-clearance guarantee.

Reads the flows in `replica/recon.md`. Writes `replica/test-plan.md`,
`replica/bugs.md`, and end-to-end tests in the project (`e2e/`). Templates in
this folder: `test-plan.md`, `bug-report.md`, `e2e.example.spec.ts`.

## The rule

**Test your clone, not the original.** Never load test, fuzz, script or
hammer the original app's servers. Using the original by hand, as a normal
user, to see how it behaves is fine.

## Step 1: the plan

For every flow F01, F02... in the recon map, write:

- **Happy path**: the steps, and what the user should see at the end.
- **Edge cases** that apply. Go down this list for every flow:
  empty input, very long input, emoji and accents, two tabs at once,
  double click on submit, back button mid-flow, refresh mid-flow, slow
  network, offline, expired session, second user's data (must be invisible),
  time zones and daylight saving, mobile width, keyboard only, screen reader
  labels.
- **Negative cases**: wrong password, card declined (Stripe test card
  `4000 0000 0000 0002`), permission denied, deleted record.

Number every case: F01-H1, F01-E3, F01-N2.

## Step 2: automate what you can

Playwright, one spec per flow, against the local dev server with seed data.
Use roles and labels for selectors (`getByRole('button', { name: 'Book' })`),
never CSS classes. See `e2e.example.spec.ts`.

```bash
npm i -D @playwright/test && npx playwright install chromium
npx playwright test
```

Add to every spec: fail on console errors, fail on any 5xx response, and an
axe accessibility scan (`@axe-core/playwright`) on each screen.

## Step 3: click through the rest

What cannot be automated (emails arriving, OAuth with real providers,
payments end to end, visual glitches) gets a manual pass. If a browser tool is
available, drive the local clone with it and screenshot each step. Otherwise
give the user the checklist and wait for answers.

## Step 4: report bugs

Every bug goes in `replica/bugs.md` in the `bug-report.md` format: an ID, a
severity, exact steps, expected, actual, evidence. Severity:

| | means |
| --- | --- |
| S1 | data loss, security hole, payments wrong, core flow blocked |
| S2 | a feature broken, no workaround |
| S3 | broken with a workaround, or visibly wrong |
| S4 | cosmetic |

Only report what you reproduced. "Might be an issue" goes in a separate
"to check" list.

## Step 5: fix loop

Fix S1 and S2 first. For every fix: write the failing test first, fix, watch
it pass, keep the test. Re-run the whole suite after each batch. Update
`bugs.md` with the commit that fixed each one.

## Output

`test-plan.md`, the specs, `bugs.md`, and a summary: cases run, passed,
failed, bugs by severity, fixed so far. Ship nothing with an open S1. Next:
`/replica-diff`.
