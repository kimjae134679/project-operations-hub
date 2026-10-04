# Control Tower design decisions

Reviewed on 2026-10-05 (Asia/Seoul). Native Windows WPF implementation. References are publicly readable; no paid template, font download, icon dependency or copied product asset is required.

## Task and visual hierarchy

The primary action is choosing a project, finding its function/program, and opening or running that program. The interface therefore follows **project → function → program → action → actual execution record**. Tool health and installation controls are secondary tabs. Counts come from the project registry and owned processes, not example numbers.

The former white/blue rounded dashboard is replaced by a graphite workbench with a subdued green accent, a flat project rail, compact function lists, one program inspector and a persistent log panel. Green emphasizes selection and the primary launch button. Grey retains navigation and metadata. Failure indicators use a restrained warm colour. No gradients, decorative fake charts or independent metric cards.

## References compared

| Official reference | Useful principle | Applied decision | Deliberate limit |
|---|---|---|---|
| [Linear: A calmer interface for a product in motion](https://linear.app/now/behind-the-latest-design-refresh), March 12, 2026 | Navigation should recede; action placement should stay predictable; borders should clarify hierarchy quietly. | Dim project rail, distinct project header and program controls, small text labels, restrained separators and warm graphite. | No Linear assets or its issue-tracking terminology. |
| [VS Code: User interface](https://code.visualstudio.com/docs/editing/getting-started/userinterface), current official documentation | Explorer, content workspace, secondary detail view and output panel have different jobs. | Project rail, central function/program explorer, program inspector and bottom execution record. | No editor controls that this application cannot perform. |
| [Raycast: List](https://developers.raycast.com/api-reference/user-interface/list), current official developer documentation | Compact rows, grouped sections and the selected item's actions keep utilities easy to scan. | Program name/detail/status rows under function headings; actions belong to the selected program inspector. | No Raycast runtime/API dependency; this remains a Windows application. |
| [shadcn/ui: Sidebar](https://ui.shadcn.com/docs/components/base/sidebar), current official component documentation | Sidebar grouping, clear active state and consistent compact navigation can scale. | Stable project list with an accent selection edge; folder discovery controls remain at the rail's bottom. | No React dependency or wholesale dashboard template. |

The existing hub document `03_KNOWLEDGE/CANDIDATE_CATALOG/UI_DESIGN_REFERENCES.md` was read from `work/phonelol-v1180-handoff`. Its three-reference comparison rule informed the process. Small component libraries such as Uiverse are useful for interaction details, while the above workspace references better match this application's hierarchy. No CSS component was pasted into WPF.

## Tokens and interaction

- Background `#111514`, rail `#0B100E`, surface `#181E1B`, raised control `#222A25`.
- Primary text `#EDF2EE`, secondary text `#A5B2A9`, separators `#303A33`.
- Accent `#AFE4B5` with dark button ink `#102918`; error `#ECAF9F`.
- Segoe UI with Malgun Gothic fallback; no external typeface dependency. Monospaced paths/logs use Consolas with Korean fallback.
- 232 px project rail, 280 px program inspector. Window minimum 1060 × 720. Scrollable program explorer, inspector and log preserve access at smaller sizes.
- Native OS title bar preserves standard move/resize/snap behaviour. No custom caption logic.
- Keyboard selection, visible focus, disabled launch state, hover feedback and truthful empty states.
- Entrance/selection fade is 160 ms, from 0.84 to 1 opacity. It is disabled when Windows `SystemParameters.ClientAreaAnimation` is false or the user enables reduced motion. No continuous decoration or layout animation.

## Functional bindings

- Project list → `Projects` / `SelectedProject`.
- Discovery root → `RootPath`; folder selection and dropped directories call `DiscoverAsync`, then dropped project selection uses `SelectProjectByPath`.
- Function groups → `SelectedProject.Functions`; program rows → `FunctionItem.Programs`.
- Inspector → `SelectedProgram`, `OpenProgram`, `LaunchProgramAsync`, `StopProgram`.
- Registry counts → `ProjectsCount`, `ProgramsCount`, `RunningCount`.
- Tool table → actual `Statuses`; bottom record → `JobLogs`; footer → `Message`.
- Jev controls retain their backend policy bindings. Installation and restore retain existing services.

## Verification boundary

XML structure is checked in the editing environment. Native Windows build, actual runtime binding diagnostics and screenshot inspection are required before calling the design verified. A generated image or HTML approximation does not count as evidence of this WPF application's layout. Test empty, discovered, selected, running and failed states; a narrow window; long Korean names/paths; keyboard navigation and reduced motion. The screenshots and exact build results belong in the release verification record.
