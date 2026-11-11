---
name: replica-backend
description: "기존 앱 재현·클론 맥락에서 재현 앱의 인증·데이터·서버 기능 연결 단계가 필요할 때만 사용합니다. 일반 개발이나 URL만으로 활성화하지 않습니다."
---

## 현재 호출 범위
- 기존 앱 재현·클론을 요청한 작업 맥락에서 이 단계가 필요할 때만 선택한다.
- URL만 있는 질문, 일반 개발·로그인 추가·디자인 수정 요청에 클론 작업을 시작하지 않는다.
- 다음 단계 추천은 그 단계의 구현·배포·게시·결제 승인이 아니다.
- 사용자 승인 범위와 실제 도구·계정 권한을 우선하고 기존 안전·저작권 경계를 유지한다.

# replica-backend

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

Reads `replica/architecture.md`. Writes migrations and server code, and keeps
`replica/backend.md` (checklist below) up to date.

## The rules

- **Official, public APIs only, with the user's own keys.** Never call the
  original app's private endpoints, never reuse its OAuth client, never proxy
  through it.
- **The user creates accounts and keys.** Codex never signs up for services,
  never types a password, card or live key. The user creates the Stripe,
  Supabase, Resend or Google Cloud project and puts keys in `.env.local`.
  Codex writes `.env.example` with every variable name and no values.
- **Test mode first.** Stripe test keys and test cards until replica-deploy.

## Auth

- Email sign up with verification, password reset, magic link if the original
  has it. OAuth (Google, Apple) with the user's own developer apps.
- Sessions: http-only secure cookies. Sign out everywhere.
- Roles and teams if the recon map has them: owner, admin, member, with one
  function that answers "can this user do this to this record".
- Account deletion that actually deletes. Apple requires it for apps with
  sign up.

## Database

- Migrations from `architecture.md`, checked in, run by a script.
- Access rules on every table: row level security policies on Supabase, or
  the authorisation function called in every query. Test it: a second user
  must get nothing back.
- Seed script with realistic fake data (no real people).
- Backups on (the host's daily backups count, check they are enabled).

## Payments

- Stripe Checkout for sign up to a plan, the Customer Portal for changes and
  cancelling. Do not build card forms.
- Webhooks: verify the signature, store the event id, make every handler
  idempotent (Stripe retries). Handle `checkout.session.completed`,
  `customer.subscription.updated`, `customer.subscription.deleted`,
  `invoice.payment_failed`.
- Subscription status lives in your database, updated by webhooks, read by
  your app. Never trust the client.
- Cancelling is one click. Hard-to-cancel billing is a top complaint about
  most apps, and in many places it is illegal.

## Email and jobs

- Transactional email through Resend or Postmark from your own domain.
  Templates written fresh.
- Jobs for anything time-based (reminders, digests, sync, cleanup) with
  retries and a dead-letter log. Times in UTC, shown in the user's zone.

## Integrations

For each integration in the feature matrix: the official API, the OAuth
scopes needed (fewest possible), the provider's review process, rate limits.
Google scopes like Calendar need Google's OAuth verification before public
launch, which takes weeks. Start it early and write that in `backend.md`.

## Security checklist

- [ ] secrets only in env vars, `.env*` in `.gitignore`, nothing in client bundles
- [ ] input validated on the server (zod or similar) on every route
- [ ] authorisation checked on every read and write, tested with a second user
- [ ] rate limits on auth, sign up, and anything that sends email or SMS
- [ ] webhooks verify signatures
- [ ] uploads: size and type limits, served from a separate domain or bucket
- [ ] no user data in URLs or logs
- [ ] dependencies audited (`npm audit`)
- [ ] privacy policy lists every processor (Stripe, Resend, host, analytics)

## Output

Working auth, database, payments in test mode, email and the integrations,
`.env.example`, `replica/backend.md` with the checklist ticked, feature
matrix rows updated. Next: `/replica-test`.
