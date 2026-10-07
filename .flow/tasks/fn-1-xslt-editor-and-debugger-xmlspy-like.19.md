---
satisfies: [R26]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.19 Highlight the current context node's start tag in the XML pane while paused

## Description
While paused, highlight the start tag of the current context node in the XML input pane (light blue) and scroll it into view.

**Size:** S
**Files:** `src/HoboXslt.Core/XsltRunner.cs`, `src/HoboXslt.Core/DebugTraceListener.cs`, `src/HoboXslt.Core/PauseSnapshot.cs`, `tests/HoboXslt.Core.Tests/DebugSessionTests.cs`, `src/HoboXslt.App/MainWindow.xaml.cs`, `src/HoboXslt.App/PausedLineRenderer.cs`, `src/HoboXslt.App/App.xaml`, `README.md`
**Touches:** [src/HoboXslt.Core/**, tests/HoboXslt.Core.Tests/**, src/HoboXslt.App/**, README.md]

### Approach (verified by the conductor with a probe against Saxon-HE 12.10)
- Turn on `Feature.LINE_NUMBERING` for debug runs only (the source tree then carries line/column per element).
- In `DebugTraceListener.enter`, read `context.getContextItem()`: a `NodeInfo` element gives `getLineNumber()`/`getColumnNumber()`/`getSystemId()`; attribute/text nodes -> their parent element; the document node reports line 0 -> no node; an atomic item -> no node. Only keep it when the system id is the XML input file (FileKey compare). Add it to `PauseSnapshot` as an optional context location (line + column).
- The column Saxon reports is the end of the start tag (probe: `    <line qty="2">` -> line 3, col 19). In the UI, find the start tag by scanning back from that position to `<` (can span lines) and highlight that range.
- Reuse `PausedLineRenderer` for the XML editor with a light-blue brush from App.xaml (generalize it to take a brush and a text range, minimal change). Set in `ShowPause`, clear wherever the orange XSLT highlight is cleared.
- Keep it simple; no highlight of the whole element (user: start with the start tag).
- README: one line in the debugging section.

### Acceptance
- [ ] Test: paused in a `match="line"` template -> snapshot context location is that line element's line/column
- [ ] Test: paused in an `xsl:for-each select="1 to 2"` body -> no context location
- [ ] Manual run: stepping moves the light-blue start-tag highlight in the XML pane; it disappears on stop/completion (screenshot)
- [ ] `dotnet build HoboXslt.slnx` and `dotnet test tests/HoboXslt.Core.Tests` green

## Acceptance
- [ ] TBD

## Done summary
While a debug run is paused, the XML pane now highlights the start tag of the current context node in light blue (#CCE0FA, ContextNodeBrush) and scrolls it into view. For an attribute or text node, the parent element's start tag is highlighted. Debug runs turn on Saxon's LINE_NUMBERING (plain runs turn it off). DebugTraceListener puts the context element's line and column into PauseSnapshot.Context (new ContextLocation record). This happens only when the element's system id is the XML input (FileKey compare). The document node, an atomic item, or a node from another document give null. MainWindow scans back from Saxon's position (just after the start tag) to '<', so a start tag over several lines works. PausedLineRenderer now takes a text range and a fullWidth flag. The XSLT editors use it full width, as before. The XML highlight is cleared in SetPaused(false) together with the orange line. README has one paragraph in the debugging section.

Tests (DebugSessionTests): Paused_WhenInTemplateMatchingLine_ThenContextIsLineElement covers match="line" and the @qty attribute -> parent, (2,17). Paused_WhenContextIsNotAnElementOfTheInput_ThenNoContext covers "1 to 2", the document node ".", and doc('other.xml')/*. The positive cases failed with LINE_NUMBERING forced off (2 failed) and pass with the change (measured).

Manual run (UI Automation against the app's own window, measured), with t19\eksempel\ordrer.xml where the ordre 1002 start tag spans 2 lines and a breakpoint at ordrer.xsl:14: pause -> blue on XML line 3 (`<ordre id="1001" ...>` only, x 59..320); step into -> blue stays (same context); continue -> blue on lines 7-8 (the start tag over 2 lines); completion -> blue=0 and orange=0; debug again then Stop -> blue=0 and orange=0. With lang.xml (ordre at line 203 of 212) the pane scrolled to show line 203 highlighted. The orange line is unchanged (x 479..965, as in t10). Shots and logs: C:\Users\JSL\AppData\Local\Temp\claude\C--Users-JSL-Documents-hobo-xslt\bdbba1d5-d9de-4a5e-ba81-448ec5825a09\scratchpad\t19\shots\*.png, run.log, run-lang.log. No app process left; settings.json is Dansk.

baseline: green via handoff (verified at 3153233 by fn-1-xslt-editor-and-debugger-xmlspy-like.18; only .flow/ changed since)
Gates (after the edit, at the committed tree): dotnet build HoboXslt.slnx rc=0 (0 warnings); dotnet test tests/HoboXslt.Core.Tests rc=0, 49/49 (44 + 5 new). The known flaky test did not fail.

Tier: session (jev-unavailable(no_key))

stage: impl-review - ran (host, one reviewer, SHIP; receipt /tmp/impl-review-receipt-22e37af6a82b-fn-1-xslt-editor-and-debugger-xmlspy-like.19.json) (model: fable)
Integrated verify at 13e396b: dotnet build rc=0; dotnet test 49/49.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: e10a53a22027f5ee1eec991a83f7f10c59c74d89
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: