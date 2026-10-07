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
TBD

## Evidence
- Commits:
- Tests:
- PRs:
