---
satisfies: [R5, R6, R8, R12]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.5 Debugger UI: breakpoints, stepping, caret, variables

## Description
Wire task 2's debug session into the WPF shell.

**Size:** M
**Files:** `src/HoboXslt.App/MainWindow.xaml(.cs)`, `src/HoboXslt.App/BreakpointMargin.cs`
**Touches:** [src/HoboXslt.App/**]

### Approach
- Breakpoint margin on the XSLT editor (AvalonEdit `AbstractMargin`); breakpoints tracked with `TextAnchor` so they follow edits; stored per normalized file key so breakpoints in included files survive switching files. Unbound breakpoints shown differently after a session.
- Debug commands: Start debugging, Continue, Step into/over/out, Stop (toolbar + F5/F10/F11/Shift+F11/Shift+F5).
- On pause: open the paused file in the XSLT editor if needed, move caret to the line and scroll it into view; fill a variables list from the snapshot.
- During a session: editors read-only, Run/Debug disabled. Closing the window stops the session.
- Session result: output and diagnostics shown as for Run.

### Investigation targets
**Required**:
- Task 2's `DebugSession` API and snapshot
- Task 3's main window

### Acceptance
- [ ] Clicking the margin toggles a breakpoint; debug run pauses there and caret moves to the line (manual run)
- [ ] Breakpoint in an included stylesheet opens that file on pause (manual run)
- [ ] Step into/over/out, continue and stop work from toolbar and keys (manual run)
- [ ] Variables list shows values or `unavailable` (manual run)
- [ ] Closing the window while paused exits the process (manual run)

## Acceptance
- [ ] TBD

## Done summary
TBD

## Evidence
- Commits:
- Tests:
- PRs:
