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
TBD

## Evidence
- Commits:
- Tests:
- PRs:
