---
name: agent-desktop-windows
description: "Use when authorized Windows app UI inspection or input is needed, or when using or diagnosing the installed agent-desktop CLI. Select only a currently permitted surface; this skill grants no screen or control permission."
---

## Portable runtime boundary
This is a source-only guide, not a bundled Windows executable or an account connection.
Use an already installed, verified agent-desktop binary or a currently permitted connected UI surface.
In cloud environments, do not assume this Windows CLI is present. Use the connected PC capability only when the host actually exposes and authorizes it.
Resolve the existing binary from the environment or approved local installation metadata; verify its identity and scope before acting.
A missing tool or policy denial is not permission to install, elevate, change focus, or bypass security.

# agent-desktop-windows

## Codex installation and permission boundary

This guide targets the verified local variant `1.0.2-local.nofocus.1`, based on
upstream v1.0.2 (Apache-2.0), not an official new upstream release. Its `--version`
still says 1.0.2; identify the variant by absolute path, SHA-256 and provenance.
In PowerShell, use this absolute executable path for upstream commands:

```powershell
& "<verified-agent-desktop-binary>" --help
```

The local patch disables the UIA client's implicit AutoSetFocus, failing if
that setting cannot be applied. Fresh owned Korean SetValue/Invoke fixtures
kept foreground false and emitted no Activated/GotFocus events. This is bounded
evidence, NOT a guarantee for every app/provider, command or headed action.
Original upstream binary remains untouched. Start a new development console
or use this refreshed absolute path; existing consoles are not auto-restarted.
Never restore focus or replay an action to hide interference: stop and report
unexpected foreground changes, then read the result without another mutation.

Existing repository instructions, current user permissions and host tool rules
always win. Installing this guide grants no permission to read screens, capture
UI, inspect clipboard/notifications, change focus/windows, type, click or enable
an overlay. Run only the explicitly authorized operations against the specified
apps; do not execute the examples as installation or startup self-tests.

Stop and report OS/security/permission denial. Do not elevate, bypass shortcut
safety flags, change antivirus/SmartScreen/Smart App Control, restore quarantined
files, alter ACLs/ownership, or automate UAC/lock screens. Choose an existing
allowed connector or leave the step blocked. A suggested restore/minimize/focus
or headed action still requires current permission; never disturb other work.

Only this skill is installed. The common CLI guide is available as bundled text
from `skills get agent-desktop`; retrieving documentation does not authorize its
GUI examples. Preserve action-once/readback semantics and do not equate
`delivered_unverified` with success or a safe retry. Local source adaptations
and provenance are listed in `AGENT_DESKTOP_SOURCE.json`.

The observe-act loop, ref system, JSON envelope, sessions and tracing are the
same on every platform. This file covers only what differs on Windows.

Requires Windows 10 1809+ / Windows Server 2019+ (x64 or ARM64). Run it in the
interactive desktop session of the signed-in user. Session 0 (services and
SYSTEM tasks), Server Core, the secure desktop (UAC prompts, the sign-in
screen), a locked desktop and another user's session are not supported:
observation, actions and capture there fail or return nothing.

## Capability Table

| Group | Commands | Status on Windows |
|-------|----------|-------------------|
| Observation | `snapshot`, `find`, `get`, `is`, `screenshot` | Works. Surfaces are `window`, `focused`, a Chromium modal as `sheet`, and an open application menu as `menu` |
| Surfaces | `list-surfaces` | Works. Lists `window`, `focused` and `sheet` per process, plus `menu` with `item_count` while a menu is open |
| Shell surfaces | `open-system-surface` | Works for `start-menu`, `taskbar`, `system-tray`, `system-tray-overflow` and `action-center`, and returns the window `snapshot --surface <kind>` reads. `quick-settings` returns `PLATFORM_NOT_SUPPORTED` naming `action-center` |
| Ref interaction | `click`, `right-click`, `type`, `set-value`, `clear`, `select`, `toggle`, `check`, `uncheck`, `expand`, `collapse`, `scroll`, `scroll-to` | Works, semantic in headless and `--headed` mode. `right-click` and `type` need `--headed` and return `POLICY_DENIED` headless |
| Multi-click and focus | `double-click`, `triple-click`, `focus` | Works with global `--headed` only |
| Keyboard and mouse | `press`, `hover`, `drag`, `mouse-click`, `mouse-move`, `mouse-wheel` | Works. Cursor-moving input needs `--headed`. First Contact covers `press --app` |
| Held input | `key-down`, `key-up`, `mouse-down`, `mouse-up` | Fails closed on every platform with `ACTION_NOT_SUPPORTED`. A headless `mouse-down` or `mouse-up` returns `POLICY_DENIED` first |
| App and window | `launch`, `close-app`, `list-windows`, `list-apps`, `focus-window`, `resize-window`, `move-window`, `minimize`, `maximize`, `restore` | Works. First Contact covers the name `launch` takes |
| Displays | `list-displays` | Works |
| Clipboard | `clipboard-get`, `clipboard-set`, `clipboard-clear` | Works; typed text and image content |
| Wait | `wait` | Works for ms, `--element`, `--window`, `--text`, `--menu`, `--menu-closed`, `--event` and `--notification` |
| Notifications | `list-notifications`, `dismiss-notification`, `dismiss-all-notifications`, `notification-action` | Works over the Action Center. Pass `--headed` when the center is closed |
| Cursor overlay | `cursor-overlay` | Works. `enable` returns `data.rendered: true` only after the renderer acknowledges over its pipe. See `references/shell-and-overlay.md` |
| System | `status`, `permissions`, `version`, `batch`, `skills`, `session`, `trace` | Works |

## First Contact

- **Quote every ref in PowerShell.** PowerShell reads a bare `@token` as its
  splatting operator and deletes the argument before the binary sees it, so
  `set-value @s8f3k2p9:e1 hi` fails `INVALID_ARGS`. Write
  `set-value '@s8f3k2p9:e1' hi`. cmd.exe and bash need no quoting.
- **A window behind another window is drivable. A minimized one is not.**
  Every element of a minimized window reports `offscreen` and ref actions
  fail. Run `restore --app <image>` and snapshot again. Do not use
  `focus-window` by reflex: it steals the user's foreground.
- **No permission dialog exists.** `permissions` reports `automation` as
  `not_required`. Elevation boundaries are the only access control.
- **Chromium and Electron apps read thin before they settle.** Pass a longer
  `--timeout-ms` to `snapshot` instead of concluding the tree is empty.
- **Blocked shortcuts.** `alt+f4`, `win+l`, `win+d`, `alt+tab`, `win+r`,
  `win+x`, `win+e`, `ctrl+shift+esc` and modifier supersets are refused.
  Do not override the guard. In combos, `cmd` and `meta` are the Windows key;
  `meta+c` is a shell shortcut. Write `ctrl+c` to copy when authorized.
- **`type` needs `--headed`.** It is keystroke synthesis through `SendInput`,
  so a strict-headless `type` returns `POLICY_DENIED`. Use `set-value`, or pass
  `--headed`. It ends `delivered_unverified` (`verification_scope:
  "unavailable"`) when the field exposes no readable Text selection, and can
  verify when one is readable: confirm with a fresh `get`.
- **`press --app` writes to the foreground queue** with no per-process
  targeting. A headless press to a non-frontmost app fails closed. A
  non-interactive caller (service, scheduled task, CI job) gets
  `delivered_unverified`.
- **`launch` takes an absolute path or a bare name** under System32 or the
  Windows directory, with or without `.exe` (`notepad` or `notepad.exe`), not
  a display name. Use `list-apps` to find a running app.
- **Notifications:** `list-notifications` with the Action Center closed needs
  `--headed`. Single-item `dismiss-notification` and `notification-action` need
  `--headed` and `--expected-app` or `--expected-title` for the entry at INDEX.
- **Surfaces:** an open or context menu is the `menu` surface. Windows does
  not advertise a `menubar` surface.
- **Commands that raise shell chrome take the foreground.** Strict headless
  refuses them with `POLICY_DENIED` before anything is raised.
- **Stateful actions run once, then verify by re-reading.** `delivered_verified`
  means the readback matched. `ACTION_FAILED` with
  `details.kind: "post_action_verification"` means the action landed but the
  readback differs: read `details.post_state` and do not retry. A `set-value`
  the app reformats reads as failed. See `references/troubleshooting.md`.
- **Unsigned binary warnings are expected.** See `references/troubleshooting.md`.

## What differs on Windows

- Ref actions are semantic UI Automation calls in headless and `--headed` mode, except `right-click`, `double-click`, `triple-click`, `type` and `focus`, which are physical or take focus and need `--headed`.
- The cursor overlay plays its per-action travel only for headless semantic actions. A `--headed` action moves the real cursor instead.

## References

- `references/shell-and-overlay.md`: open it for `open-system-surface`, tray
  and taskbar surfaces, notifications, the cursor overlay, hosted (UWP) window
  identity and menu detection coverage.
- `references/permissions-and-elevation.md`: integrity levels, UIPI, blocked
  combos, protected processes, the cross-process interaction lease.
- `references/chromium-and-electron.md`: settle timing, covered-window hazard,
  WPF wrong-provider trap, unnamed web content.
- `references/troubleshooting.md`: symptom-to-cause map, action verification
  outcomes, saving a document headless, install warnings.

Read any file from the binary:
`agent-desktop skills get agent-desktop-windows references/troubleshooting.md`.
