---
satisfies: [R22]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.14 Stop always stops: Run and Debug, including loops inside one XPath expression

## Description
The Stop button and Shift+F5 must stop a plain Run and a Debug session at once, even an infinite loop inside a single XPath expression, without closing the program.

**Size:** M
**Files:** `src/HoboXslt.Core/**`, `src/HoboXslt.App/**`, `tests/HoboXslt.Core.Tests/**`, `README.md`
**Touches:** [src/HoboXslt.Core/**, src/HoboXslt.App/**, tests/HoboXslt.Core.Tests/**, README.md]

### Approach
- Research first and record the evidence in the summary: can Saxon-HE 12 interrupt a running transformation from another thread (e.g. a controller termination request checked inside loops/iterators)? Probe it with an infinite `for $i in 1 to 9223372036854775807 return $i` inside one `xsl:value-of` and with an infinite recursion. .NET cannot abort threads, so if Saxon has no in-engine interruption that reaches tight XPath loops, the reliable design is to run each transformation in a separate process that Stop kills (the exe can start itself with a worker argument; results, diagnostics and — for Debug — pause/step commands go over stdin/stdout or a pipe). Pick the simplest design that meets R22 for both Run and Debug, and say why.
- Run gets Stop enabled while running; Debug keeps its current Stop.
- Stopped run: no output, status says stopped (texts via the language files, Dansk and English).
- Keep single-file publish working (R15): verify the published exe can still run, debug, stop.
- Update README: remove the two 'cannot be stopped' limits and describe Stop.

### Acceptance
- [ ] Test: a Run with an infinite loop inside one XPath expression is stopped within ~2 s and the program can start a new Run afterwards
- [ ] Test: same for Debug (running, not paused) and for infinite recursion
- [ ] Manual run with the published single exe: Kør on an infinite-loop stylesheet → Stop → status stopped, app responsive, next Kør works
- [ ] README updated (limits removed, Stop described)
- [ ] `dotnet build HoboXslt.slnx` and `dotnet test tests/HoboXslt.Core.Tests` green

## Acceptance
- [ ] TBD

## Done summary
Stop (■) and Shift+F5 now stop a plain Kør and a Debug session (running or paused) at once, including an endless loop inside one XPath expression and endless recursion, without closing the program. Each Run and Debug runs in a worker process (the same exe started with `--worker`, so the single-file exe still works); Stop kills that process.

Research (measured, probe in scratchpad t14\probe): Saxon-HE 12.10 has no in-engine interruption. `javap` on `net.sf.saxon.Controller` shows no termination-request method, and none of the 2604 classes in the jar mentions `interrupt`. Probes with `count(for $i in 1 to 2000000000, $j in 1 to 2000000000 return $j)` in one `xsl:value-of`: Java `Thread.interrupt()` did not stop a plain run within 5 s, and `DebugSession.Stop()` did not stop a debug run within 5 s. Tail-recursive `xsl:call-template` in a plain run: Java interrupt did not stop it. (The task's `1 to 9223372036854775807` is rejected by Saxon at once with XPDY0130, so the nested range stands in for it.) .NET cannot abort threads, so a separate process is the only reliable design.

Design: `Worker.Serve()` (Core) reads one JSON request from stdin (paths, breakpoints or null for a plain run, language), runs `XsltRunner` or `DebugSession`, and writes pause snapshots and the result as JSON lines. Debug commands go in over stdin. `WorkerSession` (Core) is the client: `Run`/`Debug` return `DebugResult`; `Stop` kills the process, which gives `Stopped`, no output. A worker that ends without a result and without Stop gives `Failed` with the new text `Run.WorkerEnded`. When the app ends, even if killed, the worker's stdin closes and it exits, so no worker outlives the app. The app keeps one spare worker started ahead (Saxon loaded), so a Run takes about 1.1 to 1.6 s instead of about 3 s from a cold worker.

Conductor addition (approved by the user): the output well-formedness parse (`IsWellFormedXml`) now runs on a background task; only the text and highlighting assignment happens on the UI thread. Result JSON parsing also happens off the UI thread (`ConfigureAwait(false)` in the read loop).

Behaviour change to note: a stopped debug session no longer lists the `xsl:message` lines collected before Stop, because the worker is killed. Deep recursion in a plain Run that overflows the stack now ends only the worker ("Kørslen sluttede uventet."); before, it ended the app. The in-process `DebugSession.Stop` stays: the existing DebugSession tests use it.

New texts in Dansk.json and English.json: `Run.WorkerEnded`, `Status.RunStopped` ("Kørsel stoppet" / "Run stopped").

Tests: `WorkerSessionTests` (4 cases): Run with the endless XPath loop or endless recursion is stopped within 2 s, has no output, and a following Run completes. Debug, running and not paused (it continues after a breakpoint), works the same way for both stylesheets. The test project now has its own `Program.Main` (`GenerateProgramFile=false`, `UseAppHost=true`) that serves as the worker exe.

- Measured: `dotnet build HoboXslt.slnx` rc=0, 0 warnings; `dotnet test tests/HoboXslt.Core.Tests` rc=0, 40/40 passed (36 old + 4 new). Baseline: green via handoff (a8e1438; only .flow/ changed since).
- Measured (published single exe in a scratch folder, `dotnet publish src\HoboXslt.App -c Release -o <scratch>\publish`, UI Automation drive of the app's own window, Danish): ordrer.xsl Kør completed in 1378 ms. loop.xsl Kør: after 3 s the status was "Kører…" with Stop enabled. Stop button: "Kørsel stoppet" after 153 ms, Output empty, XPath `count(//*)` then gave 6. Kør again, then Shift+F5: stopped after 128 ms. Debug on loop.xsl, then Stop after 3 s: "Debugsession stoppet" after 115 ms. Then Kør on ordrer.xsl completed (1646 ms) and Debug completed (1834 ms). Process count: app + spare = 2, 3 while running, 2 after each stop, 0 after closing the window.
- Measured: a looping worker exits when its stdin closes (stands in for the app being killed); an idle spare worker exits the same way.
- Not run: Stop while Debug is paused, in the UI. Inferred: it uses the same kill path as running.
- Not run: install.ps1 (as instructed). settings.json is still Danish.
- GATE classify: code paths changed (FULL), so the Quick commands above ran.

Follow-up (not done): a warm-up transformation in the spare worker could bring a Run down to about 0.4 s (measured in the probe: 373 ms after a warm-up vs about 1 s with only `new Processor`).

Tier: session (jev-unavailable(no_key))


stage: impl-review - ran (host, 3 draws, SHIP; receipt /tmp/impl-review-receipt-22e37af6a82b-fn-1-xslt-editor-and-debugger-xmlspy-like.14.json) (model: fable)
Follow-up (P2): a stopped Debug session loses xsl:message lines emitted before Stop.
Integrated verify at 7c1418b: dotnet build rc=0; dotnet test green.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: 7c1418bfa0899411b963f70e71cd73dc1642960b
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: