---
satisfies: [R2, R4]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.1 Solution skeleton, run engine and trace proof

## Description
Creates the solution and the UI-free run engine, and proves the debugger's foundation before anything is built on it (Early proof point).

**Size:** M
**Files:** `HoboXslt.slnx`, `src/HoboXslt.Core/HoboXslt.Core.csproj`, `src/HoboXslt.Core/XsltRunner.cs`, `src/HoboXslt.Core/Diagnostic.cs`, `tests/HoboXslt.Core.Tests/*`
**Touches:** [HoboXslt.slnx, src/HoboXslt.Core/**, tests/HoboXslt.Core.Tests/**]

### Approach
- `dotnet new sln` (slnx), `classlib` net10.0 for Core, `xunit` net10.0 for tests. No WPF reference in Core.
- Core references `IKVM.Maven.Sdk` with `<MavenReference Include="net.sf.saxon:Saxon-HE" Version="12.x" />` (latest 12.x; check current versions on NuGet/Maven Central).
- Run engine: compile from the stylesheet file path (so include/import resolve), transform the input file, return serialized output plus a list of diagnostics (kind: compile error / runtime error / message; file; line; text). Collect compile errors via `XsltCompiler.setErrorReporter`, messages via `Xslt30Transformer.setMessageHandler`.
- One helper that turns a Saxon system id (`file:///...`) into the normalized file key (see spec Architecture).
- Trace proof: compile with `setCompileWithTracing(true)`, attach a `TraceListener`, and show in tests that `enter` receives location (system id + line) for instructions in the main and an included stylesheet, and that blocking the transform thread inside `enter` and releasing it from the test thread completes the run.

### Investigation targets
**Required**:
- Saxon 12 s9api javadoc: `XsltCompiler`, `Xslt30Transformer`, `net.sf.saxon.lib.TraceListener`
- IKVM.Maven.Sdk README (MavenReference usage, supported TFMs)

### Key context
- Saxon-HE tracing support is unconfirmed; if `enter` never fires in HE, stop and report before task 2.
- IKVM first call is slow; tests should not assert timing.

### Acceptance
- [ ] `dotnet build HoboXslt.slnx` succeeds on .NET 10
- [ ] Test: valid XSLT + XML → expected output text
- [ ] Test: XSLT with a syntax error → compile diagnostic with file and line, no output
- [ ] Test: `xsl:message` → message diagnostic with file and line; runtime error → runtime diagnostic
- [ ] Test: error in an included stylesheet → diagnostic points to the included file
- [ ] Test: trace `enter` reports lines in main and included stylesheet
- [ ] Test: blocking in `enter` and releasing from another thread lets the run finish

## Acceptance
- [ ] TBD

## Done summary
TBD

## Evidence
- Commits:
- Tests:
- PRs:
