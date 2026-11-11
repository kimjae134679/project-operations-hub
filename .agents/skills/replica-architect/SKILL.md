---
name: replica-architect
description: "기존 앱 재현·클론 맥락에서 재현 앱의 구조·데이터·API 설계 단계가 필요할 때만 사용합니다. 일반 개발이나 URL만으로 활성화하지 않습니다."
---

## 현재 호출 범위
- 기존 앱 재현·클론을 요청한 작업 맥락에서 이 단계가 필요할 때만 선택한다.
- URL만 있는 질문, 일반 개발·로그인 추가·디자인 수정 요청에 클론 작업을 시작하지 않는다.
- 다음 단계 추천은 그 단계의 구현·배포·게시·결제 승인이 아니다.
- 사용자 승인 범위와 실제 도구·계정 권한을 우선하고 기존 안전·저작권 경계를 유지한다.

# replica-architect

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

Reads `replica/recon.md` and `replica/features.csv`. Writes
`replica/architecture.md` (template: architecture.md in this folder).

If there is no recon map, stop and run `/replica-recon` first. Planning a
clone from memory of what an app does is how you miss half of it.

## Step 1: the stack

Use what the user already knows if they have a stack. Otherwise the default,
because every part is managed, documented and cheap at zero users:

| layer | default | swap for |
| --- | --- | --- |
| web app | Next.js (App Router) + TypeScript | Remix, SvelteKit, Rails |
| styling | Tailwind, tokens from replica-design | CSS modules |
| mobile | Expo (React Native) | SwiftUI, Kotlin |
| database | Postgres on Supabase or Neon | PlanetScale, SQLite (Turso) |
| ORM | Drizzle or Prisma | raw SQL |
| auth | Supabase Auth or Auth.js | Clerk |
| payments | Stripe Checkout + Billing | Lemon Squeezy, Paddle |
| email | Resend or Postmark | SES |
| jobs | Vercel Cron, Inngest or Trigger.dev | a worker on Fly |
| files | Supabase Storage or Cloudflare R2 | S3 |
| hosting | Vercel | Netlify, Fly, Render |

Write each choice with one line of why. One database. No microservices. The
clone does not need the original's architecture, it needs the original's
features.

## Step 2: the schema

Turn the inferred data model into SQL. For every table:

- `id uuid primary key default gen_random_uuid()`, `created_at`, `updated_at`
- an owner column (`user_id` or `org_id`) on everything a user owns
- foreign keys with an `on delete` rule decided, not defaulted
- indexes on every foreign key and every column you filter or sort by
- enums or check constraints for status fields
- times as `timestamptz`, always, stored in UTC
- money as integer cents plus a currency column
- access rules: Postgres row level security on Supabase, or one
  authorisation check per query in the data layer. Write which.

Example, for a booking app:

```sql
create table bookings (
  id uuid primary key default gen_random_uuid(),
  event_type_id uuid not null references event_types(id) on delete cascade,
  host_id uuid not null references users(id) on delete cascade,
  start_at timestamptz not null,
  end_at timestamptz not null,
  guest_name text not null,
  guest_email text not null,
  guest_timezone text not null,
  status text not null default 'confirmed'
    check (status in ('confirmed','cancelled','rescheduled')),
  answers jsonb not null default '{}',
  created_at timestamptz not null default now(),
  constraint no_zero_length check (end_at > start_at)
);
create index on bookings (host_id, start_at);
```

Then the hard constraints the recon found. Two guests booking the same slot
is a database problem (an exclusion constraint or a unique index), not a UI
problem.

## Step 3: the API

One table per flow from the recon map. For every route or server action:

`method path | what it does | who can call it | input | output | flow`

Plus webhooks in (Stripe, calendar providers) and out, and background jobs
(reminders, sync, cleanup) with their schedule.

Only official, public APIs with the user's own keys. Never the original app's
private endpoints, even if they are visible in a browser.

## Step 4: the parts that bite

Write a line on each that applies: time zones and daylight saving, idempotency
(webhooks arrive twice), race conditions, rate limits, file size limits,
search, realtime, offline, email deliverability, multi-tenancy, GDPR deletion.

## Step 5: build order

1. **Vertical slice.** The core loop end to end, ugly: sign up, do the one
   thing, see the result. Proves the stack.
2. **Must-haves** from `features.csv`, by area.
3. **Should-haves**, then could-haves.
4. **The fixes** replica-entrepreneur finds, once it has run.

Each milestone lists its screens (S-IDs), tables and routes.

## Output

`replica/architecture.md`, the SQL in `replica/schema.sql` or as the first
migration, and a summary: stack in one line, table count, route count, the
three riskiest parts, and the next step: `/replica-design`.
