---
satisfies: [R14]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.7 Global variables and parameters in the Variabler panel

## Description
The pause snapshot reads only the current stack frame, so global variables and stylesheet parameters never appear. Add them, with a scope per row, and show the scope in the panel.

**Size:** S
**Files:** `src/HoboXslt.Core/DebugTraceListener.cs`, `src/HoboXslt.Core/PauseSnapshot.cs`, `tests/HoboXslt.Core.Tests/DebugSessionTests.cs`, `src/HoboXslt.App/MainWindow.xaml`
**Touches:** [src/HoboXslt.Core/**, tests/HoboXslt.Core.Tests/**, src/HoboXslt.App/MainWindow.xaml]

### Approach
- Saxon keeps global values in the controller's bindery (`XPathContext.getController().getBindery()`), keyed by `GlobalVariable`; the declared globals come from the compiled package/executable. Read each global's current value without forcing evaluation of globals not yet evaluated (those show as unavailable, R14 error clause).
- Add a scope (Lokal/Global) to each variable in the snapshot; locals first, then globals.
- Panel: add a Scope column (the mockup has Navn, Værdi, Type, Scope; add only Scope — type is not exposed).
- Keep the stack-guard and re-entrancy rules of `ReadVariables`.

### Acceptance
- [ ] Test: paused inside a template, snapshot lists a global `xsl:variable` and a global `xsl:param` with values and scope Global, plus locals with scope Lokal
- [ ] Test: a global not yet evaluated is listed as unavailable and the session continues
- [ ] Manual run: Variabler panel shows the Scope column with Lokal/Global rows while paused
- [ ] `dotnet build HoboXslt.slnx` and `dotnet test tests/HoboXslt.Core.Tests` green

## Acceptance
- [ ] TBD

## Done summary
The pause snapshot now lists global variables and stylesheet parameters after the locals. Each variable carries a scope (Lokal or Global), and the Variabler panel has a Scope column. Globals are read straight from Saxon's bindery, so reading them never forces an evaluation. A global that has not been evaluated yet shows as utilgængelig.

- Saxon inlines references to a constant global variable such as `<xsl:variable name="rate" select="0.25"/>`, so its value never reaches the bindery. The snapshot reads such a constant from its literal body (under tracing it is wrapped in a ComponentTracer). Without that, the mockup's own example ($rate, Global) would show as unavailable. Params are excluded from this fallback.
- The globals come from the top-level package's component index, sorted by name. `PackageData.getGlobalVariableList()` is empty in Saxon 12.10.
- The Værdi column shrank from 900 to 780 so the 120-wide Scope column fits at the default 1400 window width.
- Tests: `Paused_WhenGlobalsEvaluated_ThenSnapshotListsLocalsThenGlobals` checks the exact list (lv Local, gp Global, gv Global; gv is a constant). `Paused_WhenGlobalNotYetEvaluated_ThenValueIsUnavailableAndRunCompletes` covers the R14 error clause. The existing variable tests now expect scope Local.
- Manual run (measured, UI Automation, scratchpad smoke-t7/drive.ps1): paused at main.xsl:8, the panel headers were NAVN, VÆRDI, SCOPE, and the rows were $vat 62.25 Lokal, $count 1 Global, $currency utilgængelig Global, $later utilgængelig Global, $rate 0.25 Global. One Continue then completed the session with the expected output.
- baseline: green via handoff (verified at 503d6f47 by .6; only .flow/ changed since)
- Gates: dotnet build HoboXslt.slnx (0 warnings, 0 errors), dotnet test tests/HoboXslt.Core.Tests (32 passed)

Tier: session (jev-unavailable(no_key))


stage: impl-review - ran (host, one reviewer, SHIP; receipt /tmp/impl-review-receipt-22e37af6a82b-fn-1-xslt-editor-and-debugger-xmlspy-like.7.json) (model: fable)
Integrated verify at e9f526d: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests green.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: e9f526d449e156e92d2b1cab5a92d18575e1a478
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests, manual UI Automation run: scratchpad/smoke-t7/drive.ps1 (log drive.log, screenshot paused-vars.png)
- PRs: