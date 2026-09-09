# AGENTS.md

## Purpose
This is the single agent-facing source of truth for the Workbench. Read it together with `README.md` before doing project work.

The Workbench exists to keep project handoff simple, preserve verified working behavior, and transfer useful development lessons between projects without blindly copying project-specific assumptions.

Do not create extra planning, status, notes, handoff, archive, or instruction documents unless the user explicitly asks or the actual toolchain requires them.

---

## Two-file project memory protocol
Each real project should keep exactly two root management files when practical:

- `README.md` — user-facing: purpose, current state, what works, important decisions, next work, major tools/services, access requirements, practical tips, key paths/links.
- `AGENTS.md` — agent-facing: exact commands, paths, architecture, SDK/tooling, permissions, build/package rules, failure modes, verification gates, device/environment facts, remaining work, reusable lessons.

Do not duplicate the same long explanation in both files.

Do not scatter `PLAN.md`, `STATUS.md`, `NOTES.md`, `HANDOFF.md`, `TODO*.md`, duplicate READMEs, date-stamped handoff files, or note-only folders. Necessary source folders such as `src`, `assets`, `tests`, `android`, `scripts`, or existing architecture folders are fine.

When context arrives from another chat or prior handoff, merge useful facts into `README.md` and `AGENTS.md` instead of preserving another overlapping handoff file.

---

## Default authorization
Proceed without re-asking for reversible local project work that is clearly required by the task, including:

- reading project files
- editing project files
- builds, linting, typechecks, tests
- non-destructive debugging and log inspection
- local packaging and verification
- temporary build/test setup

Ask before:

- destructive deletion of user/project data
- irreversible migration
- production deployment or production-data modification
- spending money
- changing account/repository permissions
- exposing or transmitting credentials
- sending external messages not already requested

---

## Working style
- Finish the requested task end-to-end; partial implementation is not completion.
- Prefer the smallest complete implementation that satisfies every requirement.
- Minimalism applies to code/architecture complexity, not to requirements, QA, security, migration safety, or error handling.
- Reuse working project code, platform-native capabilities, standard libraries, and already-installed dependencies before adding new abstractions/packages.
- Protect working behavior. Prefer a small verified change over a broad rewrite when fixing compatibility, removing one dependency, or repairing one workflow.
- Do not hide failure with placeholders, fake data, fake success states, swallowed errors, or unverified claims.
- If ambiguity is reversible and low-risk, choose a sensible default and continue.
- Ask only when different answers materially change the result or create meaningful risk.

---

## State / completion model
Use this mental model:

`ASSIGNED -> EXEC -> CLAIMED -> GATED -> ACCEPTED`

An agent saying “done” is only `CLAIMED`.

A task is `ACCEPTED` only when all applicable evidence is satisfied:

1. Every explicit user requirement is accounted for.
2. Relevant static checks pass.
3. Focused automated tests or direct workflow checks pass.
4. The actual build/package succeeds where practical.
5. The built artifact is the one that was actually tested.
6. The touched user workflow works end-to-end.
7. Persistence/restart/re-entry is verified where relevant.
8. No obvious regression remains in adjacent working functionality.
9. No temporary fake result or known broken path remains.
10. Any external dependency that cannot be verified is clearly marked as unverified/blocked.

Keep evidence classes separate:

- code complete
- static/CI PASS
- package/build PASS
- install PASS
- physical/manual workflow PASS
- final ACCEPTED

Never inherit an old PASS onto a changed HEAD without re-running the relevant gate.

---

## Repository and remote-machine hygiene
Preserve project architecture and keep disposable work out of long-term structure.

### During work
Temporary files, build folders, logs, staging credentials, test copies, archives, or helper scripts may be created when useful.

### At the end of work
Especially when using Desktop Remote / Remote Desktop Commander, remove **disposable leftovers** that are no longer needed:

- temporary build directories
- failed/intermediate artifacts
- one-off test copies
- obsolete temp ZIP/APK/EXE files
- disposable caches
- debug logs no longer needed for diagnosis
- temporary staging secrets/configs
- one-off scripts that have no future operational value

Do **not** delete:

- actual source code needed for future edits
- project configuration/build scripts needed to reproduce the app
- user data
- required credentials/keystores stored in their approved private location
- final deliverables
- known-good rollback baselines that are still useful
- minimal verification records required to prove what was built

Target final state: **the working project + final deliverables + only the support files needed to modify/rebuild/verify it later**.

Do not confuse cleanup with destructive simplification.

---

# Cross-project lesson exchange
Reuse these patterns when they fit the project. Never apply them blindly.

## 1. One obvious normal-use entry point — Investment-Lab
For Windows tools, normal use should have one obvious launcher when practical, e.g. `RUN_START_HERE.cmd` or the final EXE.

Advanced maintenance can live behind a menu/flag/secondary script, but the user should not need to choose among many similar launchers.

Korean-safe CMD/Python output pattern when relevant:

```cmd
@echo off
chcp 65001 >nul
set PYTHONUTF8=1
set PYTHONIOENCODING=utf-8
```

For PowerShell text/log output, explicitly use UTF-8 (`Set-Content -Encoding UTF8`, `Out-File -Encoding utf8`).

For old Android/Java/Unity/CLI tools that are path-sensitive, use a short ASCII-only temp build/signing path while keeping user-visible Korean names/UI intact.

## 2. Local-first and real user loop — HealthAPK
- If the core product does not need a server, prefer local storage/assets and avoid making login/payment/network a prerequisite.
- Test the full real workflow, not just isolated screens.
- Include persistence after restart/re-entry.
- Debug/Metro-connected builds and standalone/offline packages are separate contracts; test them separately.
- Unknown metadata should remain unknown rather than be invented.

## 3. Content/native split and storage compatibility — ChungYack
- If frequent UI/data/content changes do not require native changes, separate the web/content layer from the installed native shell.
- Rebuild APK only when package/native bridge/permissions/signing/native assets actually change.
- Treat persistent IDs, DB keys, and localStorage keys as compatibility contracts.
- Before signing/origin/storage migrations, prepare `backup/export -> migrate/reinstall -> restore/import`.
- Keep active/actionable information first and verification source detail behind a secondary layer.

## 4. Exact baseline + physical evidence — PhoneLOL
- When patching a known-good binary/app lineage, bind the work to exact SHA/expected baseline and fail closed if it differs.
- Static validation is not a physical-device pass.
- Bind artifact SHA, signer, installed build, device/session identity, and trial evidence before promotion when applicable.
- On Windows self-hosted build machines, prefer `temp -> build/sign/verify -> final copy` to avoid permissions/locks.
- Release bundles may include the primary artifact plus archive/hash report where that improves traceability.
- Monitors should separate current state from historical context, and primary channel health from diagnostics/logging health.
- Large logs should be incrementally tailed where possible instead of reparsed from the beginning repeatedly.

## 5. Visual QA + automation boundaries — Market Radar
- Treat desktop and mobile as explicit QA targets.
- Check clipping, horizontal overflow, unreadable text, modal/back behavior, and page/runtime errors.
- Do not shrink dense information until it becomes unreadable merely to fit mobile; use responsive rearrangement/local scrolling.
- Keep frequently regenerated data separate from hand-tuned UI/source so automation cannot overwrite UI work.
- Avoid timestamp-file explosion; use a canonical live state + archive model when appropriate.
- Keep a known-good rollback point until the new version is verified.

## 6. Explicit selection, compact editor UX, restrained feedback — SideMemojang
- Destructive object actions such as Delete/Backspace should apply only to explicitly selected objects, not guessed neighbors.
- In dense desktop tools, move low-frequency actions to context menus rather than filling every row/tab with permanent buttons.
- Preserve working space; reduce unnecessary whitespace between editor content and toolbars.
- If success is already visually obvious (copy/paste/resize result), avoid redundant success toasts. Reserve stronger feedback for failure, destructive outcomes, or hidden asynchronous work.
- For contenteditable formatting controls, save selection on toolbar interaction and restore the range before applying formatting.
- Korean IME/Shift shortcuts may need both `event.key` and `event.code` consideration.
- Program uninstall and user-data deletion are separate concepts. Preserve user data unless explicit deletion is requested.

## 7. Shared data model, Unknown != Zero, migration-first — FinanceOne
- For major visual rewrites use `current UI -> target mock/design -> feature-preservation check -> implementation -> regression QA`.
- The same concept/metric should come from one source-of-truth function/model across dashboard, list, chart, calendar, statistics, etc.
- Repeated-entry convenience values may need TTL instead of permanent persistence.
- `Unknown != 0`. Use explicit states such as unknown, not priced, conversion unavailable, pending calculation.
- When font size increases, expand surrounding layout/rows/buttons/inputs too; do not scale text alone.
- Verify that system font scaling and app-level scaling do not compound unexpectedly.
- Do not redesign a stable auth/signing flow merely because a newer SDK exists.
- If required signing/OAuth configuration is missing, fail the build instead of silently using a wrong debug/default key.
- Inject private build configuration through an approved private source/staging path; validate presence without printing secret values; remove staging secrets after build.
- Create normalization/migration before changing schema and verify old backups restore correctly.

## 8. Human status vs raw logs, self-update handshake, path QA — ASCII Aquarium
- Separate a concise human-facing progress/status area from a full raw log file.
- User-initiated update checks should show immediate progress; background automatic checks should stay quiet when nothing needs attention.
- Portable self-update should use an updater/helper when necessary; process spawn alone is not readiness.
- Require an explicit READY/handshake before closing the running app and replacing its executable.
- Validate downloaded replacement before overwrite; keep backup until new version is proven to start.
- Include space-containing paths and Korean paths in Windows updater/install QA when applicable.
- Instructional images should place labels close to the actual controls with clear 1:1 mapping.
- Derived variants (e.g. Wallpaper Engine) should not damage the working desktop baseline; separate variant-specific entry points/folders/builds.

---

# Shared UX defaults
Use these as candidates, not rigid rules:

- Show `current state -> next action -> result -> detail` in that order.
- Prefer one strong primary CTA per screen.
- Avoid duplicate controls for the same purpose.
- A clickable control must do something, offer a fallback, or explain why it cannot proceed.
- On mobile, preserve important functionality; change layout/scrolling instead of deleting features.
- Background work should be quiet when healthy; user-triggered work should provide visible progress.
- Re-clicking an already-selected setting can be a no-op.
- Dangerous actions should target explicit selections and require appropriate confirmation when irreversible.
- Keep raw diagnostic detail available but do not make it dominate the default UI.

---

# Shared data / persistence defaults
- Separate installed app files from user data.
- Treat deployed IDs/keys/storage names as compatibility contracts.
- Use schema versioning + normalization/migration for persistence changes.
- Verify old data/backup restoration after migration.
- Do not convert missing/unknown values into zero unless zero is the true measured value.
- Prefer local-first onboarding and optional cloud/sync connection when product requirements allow it.

---

# Shared build / package defaults
When project type allows, aim for:

`environment check -> dependency check -> static checks -> tests -> build -> package -> install/run verify -> SHA-256 -> final output path`

Operational defaults:

1. One obvious normal-use entry point.
2. Predictable output directory.
3. Artifact names include meaningful version/build identity.
4. File/app/package versions should match or be validated intentionally.
5. Signed/installed/transferred artifacts should record SHA-256 when useful.
6. CI/static PASS and physical/manual PASS stay separate.
7. Data/UI-only changes should not force native rebuilds when architecture safely allows separation.
8. Do not commit secret values; document variable names/requirements only.
9. After a meaningful build, record the exact command that actually worked.
10. Before claiming release readiness, verify the actual packaged artifact, not just source code.

---

# Shared Windows / Korean defaults
When relevant:

- UTF-8 for text/JSON/logs.
- Explicit UTF-8 in PowerShell output.
- Korean IME shortcut testing.
- Space-path testing.
- Korean-path testing.
- ASCII-only temp path fallback for fragile legacy toolchains.
- User-visible Korean names/UI do not need to be removed just because the toolchain is fragile internally.

---

# Shared logging / observability defaults
Human-facing status and raw evidence should be separated.

Human status should answer:

- What is happening now?
- Is it waiting, blocked, failed, or passed?
- What should the user do next?

Raw logs may carry:

- timestamp
- run/session ID
- command/stack trace
- raw event/protocol details
- exact reason/error

Where practical, use incremental/tail parsing for large logs.

---

# Shared update / rollback defaults
For self-updating desktop apps or similar flows, consider:

`discover -> download -> validate -> backup -> helper READY -> close old app -> replace -> start new app -> verify -> cleanup backup`

Do not delete rollback backup before the new version is proven to start and retain required user data.

---

# Verified project registry
Use only confirmed mappings. Never guess a repository because its name looks related.

Confirmed mappings as of 2026-09-09:

- `운동앱` -> `kimjae134679/HealthAPK`
- `주식자동매매` -> `kimjae134679/Investment-Lab`
- `청약` -> hub `kimjae134679/ChungYack`; live shell under `kimjae134679/stock/chungyack-apk/`
- `멀티의신` -> `kimjae134679/PhoneLOL`
- `주식 앱 / Market Radar` -> `kimjae134679/stock` root

Projects without a confirmed GitHub mapping are **not automatically incomplete** and may intentionally have no repository:

- `피규어만들기_01`
- `동물의숲 / Tiny Village`

Do not map `동물의숲` to `SideMemojang_01`, `Ascii_Aquarium`, or another repo without direct evidence.

Do not create a replacement/new repository merely to make the registry look complete. Only create/connect one if the user explicitly asks or the project workflow genuinely requires it.

Additional projects used as experience sources (not necessarily part of the main GitHub-mapped project registry):

- `사이드메모장`
- `FinanceOne 리뉴얼`
- `사이버 아쿠아리움 / ASCII Aquarium`

Their verified lessons may be promoted here even when the user does not want GitHub handling for them.

---

# Tip-link library policy
The user may save useful tools, repos, websites, workflows, or social-post discoveries even when they are not adopted.

Keep friendly references in root `README.md` under `🧰 꿀팁 링크함`.

Rules:

- A saved link is a reference candidate, not proof it is installed/trusted/approved/currently used.
- Deduplicate by canonical repo/site URL.
- Prefer original GitHub/project docs over reposts/screenshots.
- Before actual use, re-check install steps, current version, compatibility, security, license, login, and cost.
- When actually adopted in a project, promote operational facts (exact version/commands/permissions/config/paths) into that project's `AGENTS.md`.
- Do not create a separate links folder/bookmarks file/tips document.

---

# Handoff transport
When asking another project chat to prepare context, prefer two fenced Markdown blocks so each file can be copied with one UI copy action.

Required output shape:

1. Heading `AGENTS.md`
2. One fenced Markdown block containing the complete AGENTS.md body
3. Heading `README.md`
4. One fenced Markdown block containing the complete README.md body

Do not split one file across multiple blocks. Do not combine both files into one block.

---

# Standard handoff request
Use this unless the project needs a narrower variant:

> 이 프로젝트의 기존 대화·파일·지침을 전체적으로 확인해서 앞으로 새 채팅에서도 바로 이어갈 수 있게 정리해줘. 결과는 루트에 둘 `AGENTS.md`와 `README.md` 두 파일만 만들어줘. `AGENTS.md`는 다음 AI가 읽을 실제 작업 인수인계로, 현재 목표/확정 요구사항/최근 진행상태/정확한 경로와 GitHub/빌드·실행·테스트 명령/사용한 언어·SDK·툴·APK 또는 패키징 방식/플러그인·MCP·외부 서비스/필요한 로그인·OAuth·권한·환경변수·인증서·서명 등 접근 조건(비밀값 자체는 쓰지 말 것)/기기·환경/알아낸 팁·주의점·실패하기 쉬운 부분·검증 방법/남은 작업을 포함해. `README.md`는 내가 읽을 요약으로 프로젝트 목적, 현재 상태, 실제 동작하는 것, 중요한 결정, 다음 할 일, 사용 중인 주요 도구·서비스, 내가 미리 준비하거나 로그인/승인해야 하는 것, 다시 쓸 만한 팁, 주요 링크·경로만 간결하게 정리해. 이 프로젝트에서 반복해서 잘 먹힌 UX·편의성·시각화·빌드·배포·한글/인코딩·로그·테스트·권한 관리 팁 중 다른 프로젝트에도 재사용할 만한 것 3~10개를 골라 두 문서에 반영해. 다른 프로젝트의 검증된 팁을 적용할 수 있다면 프로젝트 특성에 맞게 가져오되 무작정 복사하지 마. 작업 중 임시 빌드·테스트 복사본·불필요 로그/캐시/중간 산출물이 생겼다면 최종 작업 뒤 실제 프로젝트/재빌드/검증에 필요 없는 찌꺼기는 정리하고, 실제 소스·사용자 데이터·rollback 기준판·최종 산출물은 보존해. 파일 첨부보다 채팅에서 바로 복사하기 쉽게 해줘. `AGENTS.md` 제목 아래에 파일 전체 내용을 하나의 Markdown 코드블록으로, `README.md` 제목 아래에 파일 전체 내용을 또 하나의 Markdown 코드블록으로 출력해. 각 코드블록은 복사 버튼 한 번으로 파일 전체를 클립보드에 넣을 수 있게 한 파일당 정확히 한 블록만 사용해. 별도 status/plan/handoff/notes 문서나 새 폴더는 만들지 말고, 확인되지 않은 내용은 추측하지 마.

---

# Lesson contribution protocol
After meaningful work:

1. Identify 1–3 lessons that are genuinely reusable elsewhere.
2. Record project-specific operational detail in that project's `AGENTS.md`.
3. Put only user-useful summaries in that project's `README.md`.
4. Promote a lesson here only if it has broad reuse value.
5. Do not create a separate tips file.
6. Do not label a preference as a universal engineering truth; distinguish user-specific preference from generally useful engineering practice where possible.
