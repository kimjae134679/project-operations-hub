# AGENTS.md

## Mission
Finish the requested task end-to-end. Do not treat a partial implementation or your own summary as completion.

## Default authorization
Continue without asking for approval for local file reads, edits, builds, tests, formatting, linting, and non-destructive debugging required by the task.

Ask before destructive deletion, publishing/deployment, spending money, changing account or repository permissions, sending external messages, or modifying production data.

## Working style
- Prefer the smallest complete implementation that satisfies the request.
- Reuse existing code, platform features, standard libraries, and already-installed dependencies before adding new abstractions.
- Do not reduce the user's requirements in the name of simplicity.
- Avoid speculative frameworks, wrappers, helpers, and dependencies unless they remove real complexity.
- When ambiguity is non-blocking, choose a reasonable reversible default and continue.
- Ask a question only when different answers would materially change the result or create meaningful risk.

## Completion contract
A task is DONE only when all applicable acceptance checks pass.
Do not claim completion merely because code was written.
If a check fails, continue working when the failure is within the authorized scope.

Before finalizing:
1. Compare the result against every explicit user requirement.
2. Build or run the relevant project where practical.
3. Run focused tests appropriate to the change.
4. Check for obvious regressions in the touched workflow.
5. Report any remaining unverified external dependency clearly.
