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
Added the debugger UI to the WPF shell. There is a breakpoint margin on every XSLT tab editor. Breakpoints are tracked with TextAnchors, kept per file key in `BreakpointStore` when the main tab switches file, and drawn hollow when unbound after a completed session. The toolbar has Debug, Fortsæt, Step into, Step over, Step out and Stop, with F5/F10/F11/Shift+F11/Shift+F5. A pause opens the paused file through `ShowXsltLine` (an included file gets an extra tab) and moves the caret to the line, with no line highlight. The "Variabler" tab lists the snapshot. During a session the editors are read-only and Run/Debug are disabled. Closing the window stops the session. The session result is shown the same way as a Run result.

baseline: green via handoff (verified at f7d9cfc4 by fn-1-xslt-editor-and-debugger-xmlspy-like.4; only .flow/ changed since)
Gates (measured, after final edit): `dotnet build HoboXslt.slnx` rc=0, 0 warnings; `dotnet test tests/HoboXslt.Core.Tests` rc=0, 30 passed / 0 failed. Core is unchanged and the App has no test project.

Manual ACs, driven through UI Automation (measured; scratchpad smoke-t5/drive.ps1, log drive.log, screenshots pause-inc.png and after-complete-main.png):
- Margin clicks set breakpoints at main.xsl:7 and :9, and toggled line 1 on and off. F5 paused at main.xsl:7.
- A breakpoint set at inc.xsl:6 while inc.xsl was loaded in the main tab survived reopening main.xsl there. On pause, inc.xsl opened in a new selected tab ("Pauset ved inc.xsl:6").
- Keys F5/F10/Shift+F11/F11/Shift+F5 and the toolbar buttons Debug, Step into, Step over, Step out, Fortsæt and Stop each produced a pause or ended the session. Run/Debug are disabled and both editors are read-only while paused; both are restored after the session.
- Variables at inc.xsl:6 show `$id=1001` and `$f=utilgængelig` (function item). On the next iteration they show `$id=1002`.
- The caret was checked after stopping at inc.xsl:4 by typing a marker: it was on line 4.
- After a completed session, main.xsl:9 (no instruction) is drawn as a hollow circle and line 7 is filled. The output and the "Fejl og beskeder" tab are shown as for Run.
- Closing the window while paused: the process exited (WaitForExit true). Plain Run is still OK ("Kørsel fuldført", output shown).

Decisions:
- Unbound marks are shown only after a Completed session. The engine reports every breakpoint that was never hit as unbound, so a stopped or failed run would wrongly mark breakpoints it never reached.
- The unavailable value is shown as "utilgængelig", because the UI texts are Danish.
- Variables are not cleared on each step (no flicker). They are cleared when the session ends.
- Breakpoint toggling stays enabled during a session. It applies to the next session, because the engine copies the breakpoints at start.
- `EditorDocument.Open()` now returns bool, so breakpoints are restored only when a file was actually loaded.

Observed engine behaviour (task 2, not changed here): Step over on inc.xsl:6 (a literal result element with AVTs) pauses again on the same line. This is the recorded follow-up "pauses once per sibling instruction on the same line".

GATE lines: none (full tier; classify said FULL).



Tier: session (jev-unavailable(no_key))
stage: impl-review - ran (host, 3 draws, SHIP; receipt /tmp/impl-review-receipt-22e37af6a82b-fn-1-xslt-editor-and-debugger-xmlspy-like.5.json) (model: fable)
Follow-ups from review: (P2) Åbn… stays enabled during a debug session and can replace the first tab mid-session; (P3) margin toggles during a session do not reach the running session; (P3) unbound markers keyed by line go stale after edits.
Integrated verify at f52c8fd: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests green.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: f52c8fded411728b1006dce0ff8aa727f5775c59
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests, manual UI Automation run: scratchpad/smoke-t5/drive.ps1 (log: scratchpad/smoke-t5/drive.log)
- PRs: