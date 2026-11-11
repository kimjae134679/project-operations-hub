---
name: replica-brand
description: "기존 앱 재현·클론 맥락에서 재현 앱의 고유 이름·브랜드 구분 단계가 필요할 때만 사용합니다. 일반 개발이나 URL만으로 활성화하지 않습니다."
---

## 현재 호출 범위
- 기존 앱 재현·클론을 요청한 작업 맥락에서 이 단계가 필요할 때만 선택한다.
- URL만 있는 질문, 일반 개발·로그인 추가·디자인 수정 요청에 클론 작업을 시작하지 않는다.
- 다음 단계 추천은 그 단계의 구현·배포·게시·결제 승인이 아니다.
- 사용자 승인 범위와 실제 도구·계정 권한을 우선하고 기존 안전·저작권 경계를 유지한다.

# replica-brand

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

Nothing launches under the original's identity. This skill is the line
between "a clone" and "your app".

Tool in this folder:

```bash
python3 sweep.py . --avoid "Original Name,Its Company" --domains original.com --colors "#006bff"
python3 sweep.py . --config replica/brand.json        # same, from the brand file
```

Writes `replica/brand.md` and `replica/brand.json` (`avoid`, `domains`,
`colors`, used by `sweep.py`, `listing.py` and replica-deploy).

## Step 1: the name

Read the angle from `replica/fixes.md`. Generate 20 candidates across styles:
descriptive (Booklink), compound (Slotwise), invented (Calvo), metaphor
(Harbor), verb (Book). Then cut to 5 with these filters:

- **Not confusingly similar to the original** in sound, look or meaning, and
  not to any other app in the same category. That is the test trademark
  offices use, so it is the test here. No puns on their name, no "-ly" twin.
- Short, spellable after hearing it once, no awkward meaning in big languages.
- Says something about the angle, or at least does not fight it.

## Step 2: the checks, run, not assumed

For each of the 5, a row per check. Mark each **to run**, or the result with
the date it was run. Never write "available" for a check nobody ran.

| check | where |
| --- | --- |
| US trademark | tmsearch.uspto.gov, the app's class (usually 9 and 42) |
| EU trademark | euipo.europa.eu eSearch, or TMview for many offices |
| Canada | ised-isde.canada.ca trademarks database |
| global | WIPO Global Brand Database |
| domain | `whois name.com`, or the registrar's search |
| App Store and Play | search the exact name |
| handles | X, Instagram, TikTok, GitHub |
| the web | search "name + category" |

These are screening checks, not legal clearance. Before spending money on
the name, a trademark lawyer should do a proper search.

## Step 3: palette

A new palette, written into the same token roles replica-design set up.
Pick a primary brand hue from a different family than the original's (if
theirs is blue, yours is not a nearby blue). Then:

```bash
python3 ../replica-design/contrast.py replica/design/tokens.json
```

Zero AA failures. Add the original's brand colours to `brand.json` so the
sweep catches any that survive.

## Step 4: logo brief

Not a logo, a brief for whoever makes it (the user, a designer, an image
model):

- the idea in one line, tied to the name and angle
- mark type: wordmark, symbol plus wordmark, or monogram
- must work at 16px (favicon) and as a 1024px app icon
- deliverables: SVG, app icon 1024x1024 with no transparency for iOS,
  favicon set, social image 1200x630
- **must not resemble the original's mark**: no shared shape, colour pair or
  letterform trick. Put the original's logo next to the drafts and check.

## Step 5: voice

Three words for how it sounds, with what each does not mean ("direct, not
blunt"). Five do and don't pairs. Then rewrite the 10 most-seen strings in
the clone (sign up, empty states, the main button, the confirmation, the
error) in that voice. All fresh, none echoing the original's phrasing.

## Step 6: the sweep

Replace every placeholder name, colour and string. Then:

```bash
python3 sweep.py . --config replica/brand.json
```

It searches file contents and file names for the original's name (also inside
identifiers like `CalendlyEmbed`), domains and colours, skipping
`node_modules`, build output and the `replica/` planning folder. Exit 1 means
something is left. Fix until it says clean. Also check by eye: the favicon,
the page titles, the email templates, the OG image, the app icon.

## Output

`replica/brand.md` (name with checks, palette, logo brief, voice),
`replica/brand.json`, updated tokens, rewritten strings, and a clean sweep.
Next: `/replica-launch`.
