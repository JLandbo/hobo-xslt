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
Stop now asks the worker to flush its batched output and end itself; the app kills it only if it has not ended within 500 ms (user-chosen grace). Dispose still kills at once.
Test Run_WhenStoppedRightAfterOutput_ThenOutputIsKept failed 4/4 before the fix (output lost: empty or '<r'), passed 4/4 after.
stage: impl-review - ran (host, 3 draws, SHIP; receipt /tmp/impl-review-receipt-22e37af6a82b-fn-1-xslt-editor-and-debugger-xmlspy-like.16.json) (model: fable); review P3 (comment in Worker.cs) fixed with the 500 ms change.
Integrated verify: dotnet build rc=0; dotnet test tests/HoboXslt.Core.Tests green.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: cdafc7825ef82d03a395ac4e10d180f3088159ea, fb1329d5e979bb2ca810714db9b0a76bee310222
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: