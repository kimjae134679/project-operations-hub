---
name: replica-recon
description: "기존 앱 재현·클론 맥락에서 대상 앱의 화면·기능·흐름 조사 단계가 필요할 때만 사용합니다. 일반 개발이나 URL만으로 활성화하지 않습니다."
---

## 현재 호출 범위
- 기존 앱 재현·클론을 요청한 작업 맥락에서 이 단계가 필요할 때만 선택한다.
- URL만 있는 질문, 일반 개발·로그인 추가·디자인 수정 요청에 클론 작업을 시작하지 않는다.
- 다음 단계 추천은 그 단계의 구현·배포·게시·결제 승인이 아니다.
- 사용자 승인 범위와 실제 도구·계정 권한을 우선하고 기존 안전·저작권 경계를 유지한다.

# replica-recon

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

Everything else in the pack builds from what this skill writes. A bad recon
map means a bad clone, so take the time here.

Output goes in the user's project:

```
replica/recon.md        the recon map (template: recon-map.md in this folder)
replica/features.csv    the feature matrix (template: features.csv in this folder)
replica/screens/        reference screenshots of the original. Never shipped.
```

## The rules, before anything else

This skill rebuilds **functionality and UX patterns**, clean-room style. It
studies what the app does and how a user moves through it. It does not take
anything the app owns.

- **Public sources and the user's own account only.** Never log into an
  account that is not the user's, never ask for a password, never get past a
  paywall or a login by any trick.
- **Reading, not scraping.** No crawlers, no bulk downloads, no loops. If a
  browser tool is connected to the user's own browser, read pages at human
  speed with the user present.
- **No source code, no private APIs.** Do not read or save the app's
  JavaScript bundles, decompile its binary, or log its network calls to copy
  endpoints. Public API docs are fine to read.
- **Check the terms.** Some products' terms forbid using an account to build a
  competing product. If the user's account is under terms like that, say so
  and work from public sources only.
- **Screenshots are reference.** They live in `replica/screens/`, are used to
  compare layouts, and never go into the clone.

## Step 1: scope

Ask three things, or propose answers and get a yes:

1. **Which app, which platform.** Web, iOS, Android, desktop.
2. **Which slice.** "All of Notion" is not a project. "Notion's pages, blocks
   and sharing" is. Default to the core loop: the one flow users pay for.
3. **Who it is for.** The user's own business, a niche, a product to sell.

## Step 2: list the sources

Build a sources table first, with a URL on every row. In order of value:

| source | what it gives you |
| --- | --- |
| help center / docs | the most complete feature list there is, and the settings |
| pricing page | which features matter (they gate them) |
| changelog | what was added recently, what the team thinks is important |
| app store listing | screenshots of every key screen, the pitch, ratings |
| public walkthrough videos | real flows, click by click |
| marketing site | positioning, the core loop in their words |
| the user's own account | the real thing, every state, driven by the user |
| public API docs | the data model, almost for free |

## Step 3: screen inventory

One row per screen. IDs are stable: S01, S02... Every other file refers to them.

`ID | screen | route or how you get there | purpose | key components | states seen`

States matter: empty, loading, filled, error, permission denied, mobile. An
empty state you did not record is an empty state you will not build.

## Step 4: user flows

F01, F02... Each one is a goal and the screens it passes through:

```
F01 Guest books a meeting
    S07 booking page -> S08 pick a time -> S09 details form -> S10 confirmed
    edge: no slots this week, time zone differs, slot taken while filling the form
```

Count the clicks on the happy path. It becomes the number to beat.

## Step 5: components

Every repeated UI part: buttons, inputs, date pickers, modals, tables, toasts,
nav. Name, variants, states, which screens use it. This becomes
replica-design's component list.

## Step 6: inferred data model

Entities, fields and relationships, each with its evidence and a confidence:

```
Booking  id, event_type_id, start_at, end_at, guest_name, guest_email,
         status (confirmed | cancelled | rescheduled), answers (json)
         evidence: S09 form fields, S10 confirmation, help article "Cancel a booking"
         confidence: high
```

Mark guesses as guesses. replica-architect turns this into a real schema.

## Step 7: feature matrix

Write `replica/features.csv` (columns: feature, area, priority, original,
clone, notes). Priority is must / should / could. `clone` starts at `no` for
every row and gets filled in during the build. replica-diff scores it.

## Step 8: what cannot be cloned

List it honestly, as `skip` rows with a reason: licensed content (a music
catalogue, a stock library), the network and its users, data the app owns,
partner deals, hardware, regulated licences (banking, health). "Clone any
app" means the features and the flow, not what the app owns.

## Step 9: size it

Screens, flows, entities, and the hard parts (realtime, sync, payments,
calendar or email integrations, offline). Give a size: S (a weekend), M (a
few weeks), L (a quarter), XL (rescope it). No promises of a perfect clone.

## Output

`replica/recon.md` and `replica/features.csv`, then a five-line summary: the
core loop, screen and flow counts, the three hardest parts, what is out of
scope, and the next step: `/replica-architect`.
