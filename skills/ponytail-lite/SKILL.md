# Ponytail Lite

Use this skill for implementation and refactoring, not as a substitute for acceptance testing or review.

## Rule
Write the least new code that completely satisfies the requirement.

Priority order:
1. Do not build functionality that was not requested.
2. Reuse working code already in the project.
3. Prefer platform-native features and standard libraries.
4. Prefer dependencies that are already installed.
5. Add a new dependency only when it materially reduces total complexity.
6. Add abstractions only after repeated real use justifies them.

## Guardrails
- Never simplify by dropping a user requirement.
- Never hide failures behind placeholders or fake success states.
- Do not apply minimalism to security review, QA depth, acceptance criteria, or required error handling.
- A one-line solution is better only when it is equally correct, maintainable, and testable.

## End condition
After implementation, leave this mode and run the project's normal completion/acceptance checks independently.
