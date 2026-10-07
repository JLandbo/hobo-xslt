---
satisfies: [R22]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.20 Deterministic stop tests without fixed delays

## Description
The worker stop tests waited fixed times (3 s, 500 ms) with an endless loop running, which burned CPU and raced the debug recursion guard under load (flaky). They now wait for a message emitted right before the loop (Run) or stop right after Continue (Debug).

**Size:** S
**Files:** `tests/HoboXslt.Core.Tests/WorkerSessionTests.cs`
**Touches:** [tests/HoboXslt.Core.Tests/WorkerSessionTests.cs]

### Acceptance
- [ ] WorkerSessionTests pass repeatedly; no fixed delays with a running loop

## Acceptance
- [ ] TBD

## Done summary
Stop tests now wait for a 'looping' message emitted right before the loop (Run) or stop right after Continue with the breakpoint on the loop line (Debug); fixed 3 s / 500 ms delays removed. WorkerSessionTests passed 3/3 runs; full suite 44/44.
stage: impl-review - skipped(policy: risk - test-only change)

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: 8845001f2c79ec128003948f0bcb5af4ce4e7b2c
- Tests: dotnet test tests/HoboXslt.Core.Tests
- PRs: