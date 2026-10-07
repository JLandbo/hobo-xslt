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
Created HoboXslt.slnx with a net10.0 Core library (Saxon-HE 12.10 via IKVM.Maven.Sdk 1.12.2, no WPF) and an xunit test project. XsltRunner.Run(xsltPath, xmlPath, TraceListener? = null) compiles from the file path, transforms, and returns RunResult(Output, Diagnostics) with compile errors, runtime errors and xsl:message entries (file via FileKey.FromSystemId, line); 7 tests prove the ACs, including that Saxon-HE trace `enter` fires with file+line in main and included stylesheets and can be blocked and released from another thread (Early proof point holds).

baseline: none (greenfield - no solution existed pre-edit)
Gates: `dotnet build HoboXslt.slnx` rc=0; `dotnet test tests/HoboXslt.Core.Tests` rc=0, 7 passed / 0 failed (measured).
Notes for later tasks: FileKey returns Path.GetFullPath (original case) - key comparisons must use OrdinalIgnoreCase. IKVM requires implementing Java default methods (TraceListener: checkpoint/recover/startRuleSearch/endRuleSearch(object, Mode, Item); Consumer.andThen via Consumer.__DefaultMethods). IKVM build warns IKVM0117 NoSuchMethodError in net.sf.saxon.value.AnyURIValue.decode (ByteBuffer.clear/flip covariant returns, Java 9+); paths with spaces/non-ASCII and include resolution were probed and work, so impact appears limited to that decode path.
Tier: session (jev-unavailable(no_key))

stage: impl-review - ran (host, 3 draws, SHIP; receipt /tmp/impl-review-receipt-22e37af6a82b-fn-1-xslt-editor-and-debugger-xmlspy-like.1.json) (model: fable)
Follow-up (P2, from review): FileKey is not case-normalized although the spec pins a case-insensitive key; task 2 owns the fix.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: fd213dbcca734e95765a00f221c8458804812fac
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: