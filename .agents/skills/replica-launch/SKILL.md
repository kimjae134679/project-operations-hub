---
name: replica-launch
description: "기존 앱 재현·클론 맥락에서 재현 앱의 소개·출시 자료 준비 단계가 필요할 때만 사용합니다. 일반 개발이나 URL만으로 활성화하지 않습니다."
---

## 현재 호출 범위
- 기존 앱 재현·클론을 요청한 작업 맥락에서 이 단계가 필요할 때만 선택한다.
- URL만 있는 질문, 일반 개발·로그인 추가·디자인 수정 요청에 클론 작업을 시작하지 않는다.
- 다음 단계 추천은 그 단계의 구현·배포·게시·결제 승인이 아니다.
- 사용자 승인 범위와 실제 도구·계정 권한을 우선하고 기존 안전·저작권 경계를 유지한다.

# replica-launch

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

Reads `replica/fixes.md` (the angle and the evidence), `replica/brand.md`,
and `replica/brand.json`. Writes `replica/launch/`: `landing.md`,
`pricing.md`, `listing.json`, `launch-plan.md`.

```bash
python3 listing.py replica/launch/listing.json      # limits, claims, the original's name
```

Start from `listing.example.json` in this folder.

## The rules

- **No fake proof.** No invented testimonials, user counts, star ratings,
  press logos or "trusted by" rows. An empty proof section is better than a
  fake one. Real beta users, with permission, are fine.
- **No reviewer quotes as testimonials.** The original's reviews are research.
- **The original's name stays out** of your app name, store listing, keywords
  and ads. Apple and Google reject it, and it invites a takedown.

## Step 1: landing page

`landing.md`, section by section, then build it in the project:

1. **Hero**: the angle as a headline (what you do, for whom), one line under
   it, one button. Screenshot of the real product.
2. **The problem**: the top two "hate" themes, in plain words. Paraphrase,
   do not quote reviewers.
3. **How it works**: three steps, from the core flow.
4. **Features**: each tied to a fix from `fixes.md`. Lead with the fixes,
   not the parity features. Parity is the price of entry.
5. **Pricing**: the table from step 2.
6. **FAQ**: the real objections, including "can I import from {{category}}
   tools?" if you built an importer.
7. **Final call to action.**

Copy in the brand voice. Run replica-design's contrast check on the page.

## Step 2: pricing

`pricing.md`:

- The original's public pricing page and 2 or 3 alternatives, in one table,
  with the URL and the date read. Prices change, so the date matters.
- What reviewers said about price and billing, from `feedback.md`, with counts.
- The model: free plan or trial, per seat or flat, monthly and annual
  (annual usually 2 months free).
- Three tiers at most. Name them by who they are for.
- Fix the billing complaints in the product: one-click cancel, clear renewal
  emails, no surprise per-seat jumps.
- The Stripe products and prices to create (the user creates them).

## Step 3: store listing

Fill `listing.json`, then:

```bash
python3 listing.py replica/launch/listing.json
```

It checks App Store limits (name 30, subtitle 30, promotional text 170,
keywords 100, description 4000) and Google Play's (title 30, short
description 80, full 4000), counts characters the way a person does, flags
ranking and price claims in short fields, emoji, wasted keyword characters,
and **any name from `avoid`** anywhere in the listing. Exit 1 on an error.

Apple's App Review Guideline 4.1 (Copycats) rejects apps that simply copy a
popular app or make minor changes to another app's name or UI. Your fixes
and your own brand are what get you through review, so make them visible in
the screenshots and the first lines of the description.

Also prepare: screenshots at the current required sizes (check App Store
Connect and Play Console when you upload, they change), privacy labels and
the data safety form, privacy policy and support URLs, age rating, and review
notes with a demo account the reviewer can use.

## Step 4: launch plan

`launch-plan.md`: a waitlist or beta list before launch, analytics and error
tracking live, where the original's unhappy users talk (the Reddit threads
and communities from the research), a Product Hunt or Hacker News post that
leads with the fix, and the first 10 users to talk to by hand.

## Output

`replica/launch/` complete, the landing page built, `listing.py` passing.
Next: `/replica-deploy`.
