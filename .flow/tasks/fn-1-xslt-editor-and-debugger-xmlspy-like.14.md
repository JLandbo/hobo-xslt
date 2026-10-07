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
TBD

## Evidence
- Commits:
- Tests:
- PRs:
