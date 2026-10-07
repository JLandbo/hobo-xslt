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
TBD

## Evidence
- Commits:
- Tests:
- PRs:
