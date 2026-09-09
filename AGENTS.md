# AGENTS.md

## Purpose
This file is the single agent-facing source of truth for project operation. Read it before working, together with `README.md` for the user-facing project state.

Do not create extra planning, handoff, status, archive, or instruction documents unless the user explicitly asks or the actual toolchain requires them.

## Repository hygiene
- Preserve the project's existing architecture; do not create folders just to organize notes.
- Put real source/assets/config/tests only where the project architecture expects them.
- Keep temporary outputs in an existing temp/build location or outside the repository; do not commit disposable artifacts.
- Do not scatter `PLAN.md`, `STATUS.md`, `NOTES.md`, `HANDOFF.md`, `TODO*.md`, duplicate READMEs, or similarly overlapping documents.
- Persistent agent instructions, paths, commands, constraints, and verification rules belong in this `AGENTS.md`.
- Information the user should read belongs in the root `README.md`.
- When context arrives from another chat/agent, merge the useful facts into these two files instead of keeping another handoff file.
- Necessary product folders are fine. Unnecessary meta/documentation folders are not.

## Default authorization
Continue without re-asking for local project reads, edits, builds, tests, formatting, linting, log inspection, and non-destructive debugging required by the task.

Ask before destructive deletion, irreversible migration, production deployment/data modification, spending money, changing account/repository permissions, exposing credentials, or sending external messages not already requested.

## Working style
- Finish the requested task end-to-end; a partial implementation is not completion.
- Prefer the smallest complete implementation that satisfies every requirement.
- Reuse working project code, platform-native features, standard libraries, and already-installed dependencies before adding abstractions or packages.
- Do not reduce requirements in the name of simplicity.
- Do not hide failures with placeholders, fake success states, or unverified claims.
- Minimalism applies to implementation complexity, not to QA, security review, error handling, or acceptance depth.
- If ambiguity is reversible and non-blocking, choose a reasonable default and continue.
- Ask only when different answers materially change the result or create meaningful risk.

## Completion gate
Treat the agent's own “done” statement as a claim, not evidence.

A task is DONE only when all applicable checks pass:
1. Every explicit user requirement is accounted for.
2. Relevant build/run succeeds where practical.
3. Focused tests or direct workflow checks pass.
4. The touched workflow has no obvious regression.
5. No temporary implementation, fake result, or known broken path remains.
6. Any external dependency that cannot be verified is clearly identified.

For UI work also check discoverability, repeated-use flow, small/large screen clipping, duplicate controls/information, and unnecessary complexity.

If an authorized local check fails, fix it and re-run it instead of stopping after the first implementation.

## Cross-project lesson exchange
The projects are allowed to teach each other. Reuse proven patterns when they fit, but do not copy project-specific assumptions blindly.

### User-preference baseline inferred from repeated project work
- Prefer **one obvious entry point** for normal use: one launcher, one main button, or one URL before exposing advanced tools.
- Prefer **low-friction operation**: avoid asking the user to repeat routine setup or manually hunt for outputs when automation can do it safely.
- Keep the **current/actionable state visually first**. Historical evidence, raw logs, long source tables, and debug detail should remain available but not dominate the default screen.
- Do not create duplicate controls for the same purpose. Avoid nested accordions/details that make the user repeatedly open layers just to reach normal actions.
- On mobile, keep the same important information as desktop whenever practical; solve layout/scrolling instead of deleting useful content.
- A clickable control should not silently do nothing. Provide the real action, a fallback, or a clear reason it cannot proceed.
- Protect already-working behavior. Prefer a small verified change over a broad rewrite when the goal is bug fixing, compatibility, or removing one dependency.
- Show progress and remaining blockers explicitly. Distinguish `not tested`, `blocked`, `safe idle`, and `passed` rather than collapsing them into success/failure.

### One-click Windows launcher pattern — contributed by Investment-Lab
For Windows tools, provide a root launcher such as `RUN_START_HERE.cmd` when practical. Normal use should work by double-click; advanced maintenance may live behind `--menu` or a secondary path.

For Korean-safe CMD/Python output, use the equivalent of:
```cmd
@echo off
chcp 65001 >nul
set PYTHONUTF8=1
set PYTHONIOENCODING=utf-8
```
For PowerShell-created text/log files, explicitly use UTF-8, for example `Set-Content -Encoding UTF8` / `Out-File -Encoding utf8`. When an old Android/Java/CLI tool is path-sensitive, keep temporary build/signing paths ASCII-only even if user-visible UI/text is Korean.

### Local-first and end-to-end flow — contributed by HealthAPK
- If the product does not need a server, prefer local storage/assets and make core actions work without account/login/payment/network dependencies.
- Do not validate only isolated screens. Test the real user loop end-to-end, including persistence after refresh/restart/re-entry.
- Debug/Metro-connected builds and standalone/offline packages are different products from the user's perspective; verify both contracts separately.
- Do not invent metadata/defaults merely to fill a gap. Unknown should remain unknown until evidence is available.

### Content/native split and migration safety — contributed by ChungYack
- If frequent UI/content/data updates do not require native changes, separate the content layer from the installed native shell so updates can ship without rebuilding/reinstalling the APK.
- Rebuild the APK only when package/native bridge/permissions/signing/native assets actually change.
- Treat persistent storage keys and record IDs as compatibility contracts. Do not rename them casually.
- Before origin/signing/storage migrations, provide backup/export → migrate/reinstall → restore/import flow.
- Keep the default UI concise: active items first, one action per purpose, raw verification material behind details.

### Exact baseline, packaging evidence, observability — contributed by PhoneLOL
- When patching a known-good binary/app lineage, bind work to exact baseline SHA/expected offsets and fail closed if the baseline differs.
- Static validation is not a real-device pass. Bind artifact SHA, signer, actually installed build, device/session identity, and trial evidence before promotion.
- For Windows self-hosted packaging, prefer building/signing in a writable temp directory (`RUNNER_TEMP`-style), verify there, then copy finalized outputs to the long-term candidate folder. This avoids many permission/lock failures.
- Useful release bundles should include the primary artifact plus an easy-to-share archive and hash report when appropriate: APK + ZIP + SHA-256/report.
- In monitors, separate **current state** from **recent/historical context**, and separate gameplay/primary channel health from diagnostics/logging channel health.
- For large logs, favor incremental/tail parsing and immutable raw evidence instead of repeatedly reparsing or rewriting the whole file.

### Visual QA, automation boundaries, rollback — contributed by Market Radar
- Treat desktop and mobile as explicit QA targets; check at least one desktop and one phone-sized viewport for clipping, horizontal overflow, unreadable text, modal/back behavior, and page errors.
- Do not compress charts or dense information until it becomes unreadable merely to fit mobile; use local scrolling/responsive layout where needed.
- Keep frequently refreshed data separate from UI/source code so hourly automation cannot overwrite hand-tuned UI.
- Avoid timestamp-named file explosion. Accumulate same-day live state in one canonical live file and keep one daily archive when that model fits.
- Keep a known-good rollback baseline and verify the new version before removing or superseding it.

### Shared build/release defaults
When the project type allows it:
1. User-facing normal entry point should be obvious and preferably one-click.
2. Build output location should be predictable and documented.
3. Artifact names should include meaningful version/build identity.
4. Release should record SHA-256 when the artifact is installed, signed, or passed between machines.
5. CI/static PASS and physical/manual workflow PASS are separate evidence classes.
6. Data-only/UI-only changes should not force an expensive native rebuild when architecture can safely avoid it.
7. Never commit secret values; keep `.env.example`/documented variable names and local secret storage separate.
8. After a meaningful build, update `AGENTS.md` with the exact command that actually worked, not the command that was merely expected to work.

### Tip contribution protocol
After meaningful work, identify 1–3 lessons that are genuinely reusable elsewhere (UX, build, encoding, packaging, deployment, debugging, permissions, device QA, rollback, data/persistence). Add them to the project `AGENTS.md` if operational and to its `README.md` if the user would benefit from knowing them. When a lesson proves broadly useful, promote it back into this Workbench section. Do not create a separate tips file.

## Two-file project memory protocol
At the end of meaningful work, keep project memory synchronized in exactly two root files:

- `AGENTS.md` — for the next agent: commands, exact paths, technical constraints, permissions, architecture notes, verification rules, tooling, access requirements, lessons learned, and machine-actionable current state.
- `README.md` — for the user: what the project is, current status, what works, important decisions, next work, major tools/services used, access requirements, practical tips, and key links/locations.

Do not duplicate the same long explanation in both. `AGENTS.md` should be operational and precise; `README.md` should be concise and readable.

### AGENTS.md must capture when applicable
- Exact build/run/test/lint/package commands and where to run them.
- Languages, frameworks, SDK/tool versions, package managers, build systems, APK/app packaging tools, scripts, and IDE/toolchain actually used.
- Plugins, connectors, MCP servers, skills, agents, browser/desktop automation, CI/CD, hosting, database, analytics, or third-party services actually used.
- Required accounts/logins, OAuth connections, API access, repository permissions, device authorization, local services, environment variables, certificates, signing keys, or paid-plan requirements. Record requirements and variable/key names, never secret values.
- Important machine/device/environment facts needed to reproduce the work.
- Known failure modes, gotchas, workarounds, commands that proved reliable, and project-specific tips discovered during development.
- Exact locations of source-of-truth files, generated artifacts, local repos, deploy targets, and external project IDs/URLs when safe and useful.
- What was tried and rejected only when that prevents repeating an expensive or dangerous mistake.

### README.md should summarize for the user
- What major tools/services are being used and why.
- What the user must have connected, logged in, approved, installed, or paid for before work can continue.
- Practical tips or lessons that are likely to matter again.
- Avoid secret values, raw tokens, passwords, private keys, or unnecessary implementation noise.

## Verified project registry
Use only confirmed mappings. Never guess a repository because its name looks related.

Confirmed mappings as of 2026-09-09:
- `운동앱` → `kimjae134679/HealthAPK`
- `주식자동매매` → `kimjae134679/Investment-Lab`
- `청약` → hub `kimjae134679/ChungYack`; live shell lives under `kimjae134679/stock/chungyack-apk/`
- `멀티의신` → `kimjae134679/PhoneLOL`
- `주식 앱 / Market Radar` → `kimjae134679/stock` root

Unmapped and must remain unguessed until evidence appears:
- `피규어만들기_01` — goal is confirmed, but no GitHub repo/local `.blend` path is confirmed.
- `동물의숲` — `Tiny Village` settings UI concept is confirmed, but no GitHub repo/local engine path is confirmed.

Do not map `동물의숲` to `SideMemojang_01`, `Ascii_Aquarium`, or another repo without direct evidence. Do not create a replacement repo for an unmapped project merely to make the registry look complete; first recover/confirm the intended existing project location.

## User tip-link library policy
The user may save tools, repos, websites, workflows, or social-post discoveries that are useful to them even when the agent is not currently using them.

- Keep these in the root `README.md` under the friendly section `꿀팁 링크함`.
- A saved link is a **reference candidate**, not evidence that the tool is installed, trusted, approved, or part of the current architecture.
- Deduplicate by canonical repository/site URL and repository name. Prefer the original GitHub/project page over a repost or screenshot.
- Give each item a short plain-Korean note: what it is, when it may be useful, and any important login/install/cost/risk caveat.
- If a saved tool is later actually adopted by a project, copy only the operational facts that matter (exact commands, versions, permissions, config, paths, verification) into that project's `AGENTS.md`; keep the friendly reference in `README.md` if it remains useful.
- Do not create a separate links folder, bookmarks file, or tips document just for these references.

## Handoff transport
When asking another project chat to prepare handoff context, request exactly two outputs and prefer **two fenced Markdown code blocks** over file attachments. Each block should contain the complete file body for one file so the chat UI exposes a copy button and the content can be copied directly to the clipboard.

Required format:
1. Heading `AGENTS.md`
2. One fenced Markdown code block containing only the full AGENTS.md contents
3. Heading `README.md`
4. One fenced Markdown code block containing only the full README.md contents

Do not wrap both files into one block, do not split one file across multiple blocks, and do not add prose inside the blocks. File attachments are optional, not preferred.

## Standard handoff request
Use this request in project chats unless a project needs a narrower version:

> 이 프로젝트의 기존 대화·파일·지침을 전체적으로 확인해서 앞으로 새 채팅에서도 바로 이어갈 수 있게 정리해줘. 결과는 루트에 둘 `AGENTS.md`와 `README.md` 두 파일만 만들어줘. `AGENTS.md`는 다음 AI가 읽을 실제 작업 인수인계로, 현재 목표/확정 요구사항/최근 진행상태/정확한 경로와 GitHub/빌드·실행·테스트 명령/사용한 언어·SDK·툴·APK 또는 패키징 방식/플러그인·MCP·외부 서비스/필요한 로그인·OAuth·권한·환경변수·인증서·서명 등 접근 조건(비밀값 자체는 쓰지 말 것)/기기·환경/알아낸 팁·주의점·실패하기 쉬운 부분·검증 방법/남은 작업을 포함해. `README.md`는 내가 읽을 요약으로 프로젝트 목적, 현재 상태, 실제 동작하는 것, 중요한 결정, 다음 할 일, 사용 중인 주요 도구·서비스, 내가 미리 준비하거나 로그인/승인해야 하는 것, 다시 쓸 만한 팁, 주요 링크·경로만 간결하게 정리해. **이 프로젝트에서 반복해서 잘 먹힌 UX·편의성·시각화·빌드·배포·한글/인코딩·로그·테스트·권한 관리 팁 중 다른 프로젝트에도 재사용할 만한 것 3~10개를 골라 두 문서에 반영해. 다른 프로젝트의 검증된 팁을 적용할 수 있다면 프로젝트 특성에 맞게 가져오되 무작정 복사하지 마.** 파일 첨부보다 채팅에서 바로 복사하기 쉽게 해줘. `AGENTS.md` 제목 아래에 파일 전체 내용을 하나의 Markdown 코드블록으로, `README.md` 제목 아래에 파일 전체 내용을 또 하나의 Markdown 코드블록으로 출력해. 각 코드블록은 복사 버튼 한 번으로 파일 전체를 클립보드에 넣을 수 있게 한 파일당 정확히 한 블록만 사용해. 별도 status/plan/handoff/notes 문서나 새 폴더는 만들지 말고, 확인되지 않은 내용은 추측하지 마.