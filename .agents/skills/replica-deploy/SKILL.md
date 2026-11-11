---
name: replica-deploy
description: "기존 앱 재현·클론 맥락에서 명시 승인한 재현 앱의 배포 준비 단계가 필요할 때만 사용합니다. 일반 개발이나 URL만으로 활성화하지 않습니다."
---

## 현재 호출 범위
- 기존 앱 재현·클론을 요청한 작업 맥락에서 이 단계가 필요할 때만 선택한다.
- URL만 있는 질문, 일반 개발·로그인 추가·디자인 수정 요청에 클론 작업을 시작하지 않는다.
- 다음 단계 추천은 그 단계의 구현·배포·게시·결제 승인이 아니다.
- 사용자 승인 범위와 실제 도구·계정 권한을 우선하고 기존 안전·저작권 경계를 유지한다.

# replica-deploy

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

Reads everything in `replica/`. Writes `replica/deploy.md` (the checklist in
`preflight.md` in this folder, filled in).

## The rules

- **Nothing goes live without the user's go.** Show the preflight results and
  ask.
- **The user buys and signs in.** Codex never buys a domain, enters a card,
  types a password or pastes a live key. Codex writes the exact DNS records,
  env var names and commands; the user does the account steps.
- **Not until it is rebranded.** The sweep must be clean. No exceptions.

## Step 1: preflight

Run every check and paste the results into `deploy.md`:

```bash
npx playwright test                                                  # replica-test
python3 ../replica-diff/parity.py replica/features.csv               # must-haves done
python3 ../replica-brand/sweep.py . --config replica/brand.json      # exit 0: clean
python3 ../replica-launch/listing.py replica/launch/listing.json     # if shipping to stores
npm run build                                                        # production build passes
```

(Paths are relative to wherever the pack is installed. Under
`~/.codex/skills/` that is `~/.codex/skills/replica-diff/parity.py` and so
on.)

Plus by hand: no open S1 or S2 bugs, privacy policy and terms pages live
(listing every processor), cookie banner if you use non-essential cookies in
the EU or UK, account deletion works, the favicon, titles and OG image are
yours.

Any failure stops the deploy. Say which and why.

## Step 2: production services

- A **separate production project** for the database (never the dev one),
  backups on, migrations run by the deploy, not by hand.
- Env vars set in the host for production, matching `.env.example`. Live
  keys only here.
- **Stripe**: keep test mode by default. Document production products,
  prices, webhook endpoint and signing-secret variable names. Live activation
  and any real purchase/refund are excluded from this installed workflow unless
  separately and explicitly authorized; never charge as an installation test.
- **OAuth**: add the production domain to every provider's redirect URIs and
  authorised origins. Google scopes that need verification must be approved,
  or only test users can sign in.
- **Email**: the sending domain verified with the provider.

## Step 3: host and domain

Default: Vercel for Next.js (Netlify, Cloudflare Pages, Fly or Render
otherwise). Connect the repo so `main` deploys and pull requests get preview
URLs.

The domain, after the user buys it at any registrar:

| record | name | value |
| --- | --- | --- |
| A | @ | the host's apex IP (Vercel: shown in the domain settings) |
| CNAME | www | the host's target (Vercel: `cname.vercel-dns.com`) |
| TXT | @ or a subdomain | the host's verification value, if asked |

Email DNS from the email provider: SPF (TXT), DKIM (CNAME or TXT), and a
DMARC record starting at `v=DMARC1; p=none; rua=mailto:you@yourdomain` then
tightened to `quarantine` once reports are clean. Without these, your
confirmation emails go to spam.

Pick one canonical host (apex or www) and redirect the other. HTTPS is
automatic on the hosts above; check it.

## Step 4: watch it

Error tracking (Sentry or the host's), uptime checks on the home page and the
core flow's API, logs kept, analytics (a privacy-friendly one avoids the
cookie banner), and an alert to the user's email or phone. Then do the core
flow on the live site yourself, and ask the user to do it on their phone.

## Step 5: mobile, if there is an app

Expo: `eas build` then `eas submit` to TestFlight and Play internal testing.
Native: archive in Xcode, upload to App Store Connect; Gradle bundle to Play
Console. The user owns the developer accounts ($99 a year for Apple, $25 once
for Google). Beta first, then review with the listing from replica-launch.

## Output

`replica/deploy.md` with every check and its result, the live URL, the DNS
records set, and what to watch in the first week. The clone is now an app
with your name on it.
