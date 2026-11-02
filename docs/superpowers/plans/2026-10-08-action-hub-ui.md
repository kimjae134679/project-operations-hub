# Action-oriented hub UI implementation plan

Goal: Make the existing project actions and actual work records easy to find, while preserving the running 0.9.11 hub and every bridge process.

Approved scope: MainWindow.xaml navigation and existing action presentation; WorkDashboardView.xaml; WorkDashboardViewModel.cs; dedicated UI tests and in-memory rendering verification; docs. No project.catalog.json, Threads app/reviews, App.xaml.cs, MainViewModel.cs, bridge code/settings/protocol, startup tasks, installation or operational activation.

Base: d36d9b0fe01838ac256e2ad7741934046ffe4ec3, corresponding to the observed 0.9.11 source and installed version.
Worktree: D:/A_KJ/AI/Workspace/ControlTower/ui-actions-20261008/source.
Branch: codex/hub-action-ui-20261008.
Baseline: 737 passed; MouseWheelRoutingTests excluded because they open windows. Existing nullable/analyzer warnings recorded.

- [x] Write regression tests for actual state filters: uncertain running records and interrupted/error states never appear as confirmed completion; project/worker/search intersection; snapshot updates notify counts without executing/stopping jobs.
- [x] Run the focused tests and observe the new feature missing.
- [x] Implement pure in-memory status filtering. Keep original status labels and source identities unchanged.
- [x] Change the workspace headers into a permanent left menu; preserve named tabs/events and PC connection content byte-for-byte.
- [x] Present the real existing project action selection as a three-step creation/result flow. Use registered action labels, never invented review/account/output entries.
- [x] Add work search/project/status filters, clear empty states and record counts. Put stage, next action, result and actual error first; collapse technical metadata and logs.
- [x] Run all appropriate tests, build and git diff --check.
- [x] Render actual WPF controls at wide/compact sizes and light/dark palettes; inspect PNG pixels. Use a separate LocalView/in-memory host without operational initialization or window activation.
- [x] Record user instructions, decisions, evidence and remaining gaps; commit only the explicit UI/test/doc allowlist, push the isolated branch and verify remote SHA.

Reference Library materialization was attempted with the current helper, but image transfer returned HTTP 403. Do not bypass it or claim reference pixel inspection. The attached reference descriptions support layout intent only; actual implementation pixels must still be inspected.

No deployment is part of this change. Running hub instances and ProjectBridge remain intact. Threads integration uses its independently registered catalog entries when they become available; ratings/dispositions stay in the Threads application.
