---
satisfies: [R5, R6, R8, R12]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.2 Debug engine: breakpoints, stepping, stop, variables

## Description
UI-free debug session on top of task 1's trace hook. The WPF app later only sends commands and reads snapshots.

**Size:** M
**Files:** `src/HoboXslt.Core/DebugSession.cs`, `src/HoboXslt.Core/PauseSnapshot.cs`, `src/HoboXslt.Core/DebugTraceListener.cs`, `tests/HoboXslt.Core.Tests/DebugSessionTests.cs`
**Touches:** [src/HoboXslt.Core/**, tests/HoboXslt.Core.Tests/**]

### Approach
- Session runs the transform on a background thread; the trace listener decides per `enter` whether to pause (breakpoint hit, step-into, step-over at same or lower depth, step-out at lower depth) using an enter/leave depth counter.
- Commands: Continue, StepInto, StepOver, StepOut, Stop. Stop sets a flag and releases the wait; the listener then throws to abort Saxon. The session ends with a result (completed / stopped / failed + diagnostics from task 1).
- Pause raises an event with a snapshot: normalized file key + line, and in-scope variables/params (name + string value). Variable values: try the `XPathContext` stack frame in the `enter` callback; any value that cannot be read becomes `unavailable` (R8 error clause). Record in the task summary whether this needed internal Saxon APIs.
- Breakpoints: set of (file key, line). A breakpoint is "bound" once the run has entered an instruction on that line; expose bound/unbound per breakpoint at session end.

### Investigation targets
**Required**:
- Task 1's run engine and trace proof tests
- Saxon 12 javadoc: `TraceListener.enter/leave`, `XPathContext.getStackFrame`, `SlotManager`

### Key context
- Never block the UI thread; the wait must also be released on Stop and on dispose so the process cannot hang.

### Acceptance
- [ ] Test: breakpoint on a line → session pauses with that file and line
- [ ] Test: breakpoint in an included stylesheet → pause reports the included file
- [ ] Test: StepInto / StepOver / StepOut each pause at the expected next line on a fixture with a called template
- [ ] Test: Stop while paused and Stop during an infinite `xsl:iterate`/recursion → session ends as stopped
- [ ] Test: pause snapshot lists a local variable and a template param with values (or `unavailable`)
- [ ] Test: breakpoint on a line without instructions → reported unbound, run completes
- [ ] Test: dispose during pause ends the background thread

## Acceptance
- [ ] TBD

## Done summary
Added a UI-free debug engine in Core: `DebugSession` runs the traced transform on a dedicated background thread, pauses on breakpoints and on step-into/over/out (enter/leave depth counter), stops via a flag that makes the trace listener throw out of Saxon, and returns `DebugResult` (Completed/Stopped/Failed, output, diagnostics, unbound breakpoints). Pause snapshots carry file key, line and local variables/params, with unreadable values as null (unavailable).

baseline: green via handoff (verified at fd213dbc by fn-1-xslt-editor-and-debugger-xmlspy-like.1; only .flow/ changed since)
Gates (measured, after final edit): `dotnet build HoboXslt.slnx` rc=0, 0 warnings; `dotnet test tests/HoboXslt.Core.Tests` rc=0, 19 passed / 0 failed (12 new in DebugSessionTests.cs).

AC tests (tests/HoboXslt.Core.Tests/DebugSessionTests.cs): breakpoint pause file+line; included-file breakpoint; StepInto/StepOver/StepOut theory on a called-template fixture; Stop while paused; Stop during a running effectively-infinite xsl:iterate; snapshot lists param p=42 and variable v=84; R8 error clause (function-item variable -> unavailable); line without instruction -> unbound while run completes; Dispose while paused ends the run as Stopped.

Decisions:
- Internal Saxon APIs ARE needed for variables: `XPathContext.getStackFrame()`, `StackFrame.getStackFrameValues()`, `SlotManager.getVariableMap()` (net.sf.saxon.expr / expr.instruct), plus `Sequence.materialize()` for lazy closures. Instruction types `Block` and `LetExpression` are checked so depth reflects source nesting (Saxon nests every instruction after an xsl:variable inside its LetExpression; a Block's location duplicates its first child).
- A breakpoint pauses once per visit to its line: without this, Continue on a line holding nested instructions (e.g. `<inner><xsl:value-of/></inner>`) re-paused on the same line (failing test observed before the fix).
- Task 1 review follow-up (P2, FileKey case): fixed by `FileKey.Comparer` (OrdinalIgnoreCase) used by `Breakpoint` equality; FileKey keeps the original path case so the editor can open files. Test: breakpoint path upper-cased still pauses.
- Stop uses a private .NET exception thrown from `enter`; probed that it propagates through `xsl:try` (IKVM does not map it to a Java catchable type).
- Variable values: null slots are skipped (not yet bound); values shown are the slot's current value, so a variable from an exited scope or earlier loop iteration can still be listed. Globals are not listed.

Follow-ups (not part of this task):
- Global variables/params are not in the snapshot.
- Saxon inlines/eliminates variables it can fold (unused or constant variables do not appear in the snapshot).

Integration notes for task 5: .git/flow-notes/.../fn-1.2-debug-session-api.md

Tier: session (jev-unavailable(no_key))



Review fix (47c039b): per-iteration pause on one-line loop bodies; infinite recursion ends as Failed with a RuntimeError diagnostic (16 MB debug thread + stack guard) instead of crashing; literal-valued variables shown (MISCELLANEOUS optimizer option off for traced compiles); xsl:message diagnostics kept on Stop. Variables use internal Saxon APIs (XPathContext stack frame, SlotManager).
Follow-up (P2, from re-review): a breakpoint pauses once per sibling instruction on the same line, not once per visit.
stage: impl-review - ran (host; round 1 NEEDS_WORK 3 draws, round 2 SHIP; receipt /tmp/impl-review-receipt-22e37af6a82b-fn-1-xslt-editor-and-debugger-xmlspy-like.2.json) (model: fable)
Integrated verify at 47c039b: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests green.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: 8897ab788ae4b10cc417709179addd350790fa81, 47c039b66feb97b491093d19667aa09c6e6d976b
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: