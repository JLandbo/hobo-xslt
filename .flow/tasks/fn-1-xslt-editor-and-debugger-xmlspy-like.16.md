---
satisfies: [R24]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.16 Stop sends the worker's remaining output before it ends

## Description
On Stop, ask the worker to flush its batched output and end itself; kill it only if it has not ended shortly after. Output written in the last batch window before Stop is no longer lost.

**Size:** S
**Files:** `src/HoboXslt.Core/Worker.cs`, `src/HoboXslt.Core/WorkerSession.cs`, `tests/HoboXslt.Core.Tests/WorkerSessionTests.cs`, `README.md`
**Touches:** [src/HoboXslt.Core/Worker.cs, src/HoboXslt.Core/WorkerSession.cs, tests/HoboXslt.Core.Tests/WorkerSessionTests.cs, README.md]

### Approach
- Worker command thread: a `Stop` line flushes the pending output and ends the process.
- WorkerSession.Stop: send `Stop`, then kill after ~200 ms if the worker is still running; Dispose still kills at once.

### Acceptance
- [ ] Test: output written right before an endless loop, Stop issued at once, is kept
- [ ] Existing worker tests stay green

## Acceptance
- [ ] TBD

## Done summary
TBD

## Evidence
- Commits:
- Tests:
- PRs:
