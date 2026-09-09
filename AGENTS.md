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

> 이 프로젝트의 기존 대화·파일·지침을 전체적으로 확인해서 앞으로 새 채팅에서도 바로 이어갈 수 있게 정리해줘. 결과는 루트에 둘 `AGENTS.md`와 `README.md` 두 파일만 만들어줘. `AGENTS.md`는 다음 AI가 읽을 실제 작업 인수인계로, 현재 목표/확정 요구사항/최근 진행상태/정확한 경로와 GitHub/빌드·실행·테스트 명령/사용한 언어·SDK·툴·APK 또는 패키징 방식/플러그인·MCP·외부 서비스/필요한 로그인·OAuth·권한·환경변수·인증서·서명 등 접근 조건(비밀값 자체는 쓰지 말 것)/기기·환경/알아낸 팁·주의점·실패하기 쉬운 부분·검증 방법/남은 작업을 포함해. `README.md`는 내가 읽을 요약으로 프로젝트 목적, 현재 상태, 실제 동작하는 것, 중요한 결정, 다음 할 일, 사용 중인 주요 도구·서비스, 내가 미리 준비하거나 로그인/승인해야 하는 것, 다시 쓸 만한 팁, 주요 링크·경로만 간결하게 정리해. **파일 첨부보다 채팅에서 바로 복사하기 쉽게 해줘. `AGENTS.md` 제목 아래에 파일 전체 내용을 하나의 Markdown 코드블록으로, `README.md` 제목 아래에 파일 전체 내용을 또 하나의 Markdown 코드블록으로 출력해. 각 코드블록은 복사 버튼 한 번으로 파일 전체를 클립보드에 넣을 수 있게 한 파일당 정확히 한 블록만 사용해.** 별도 status/plan/handoff/notes 문서나 새 폴더는 만들지 말고, 확인되지 않은 내용은 추측하지 마.