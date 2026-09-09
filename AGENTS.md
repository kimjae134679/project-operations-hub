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

## Two-file project memory protocol
At the end of meaningful work, keep project memory synchronized in exactly two root files:

- `AGENTS.md` — for the next agent: commands, exact paths, technical constraints, permissions, architecture notes, verification rules, and machine-actionable current state.
- `README.md` — for the user: what the project is, current status, what works, important decisions, next work, and key links/locations.

Do not duplicate the same long explanation in both. `AGENTS.md` should be operational and precise; `README.md` should be concise and readable.

When asking another project chat to prepare handoff context, request exactly these two Markdown files. Prefer downloadable `.md` attachments; if that interface cannot create files, request two clearly labeled copyable Markdown code blocks. Merge them into the repository and do not keep a third handoff document.