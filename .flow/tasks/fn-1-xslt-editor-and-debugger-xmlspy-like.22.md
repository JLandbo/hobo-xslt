---
satisfies: [R13]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.22 XSLT syntax colouring like XmlSpy

## Description
User request: XSLT colouring as readable as XmlSpy's: XPath inside select/test/match/use/... attributes and inside {...} attribute value templates coloured (variables, strings, numbers, functions, axes, operators), xsl: elements distinct from literal result elements, text content distinct. Keep it separate from existing code and do not overengineer.

**Size:** S
**Files:** `src/HoboXslt.App/Highlighting/XSLT.xshd` (new, embedded resource), `src/HoboXslt.App/XsltHighlighting.cs` (new, small loader), `src/HoboXslt.App/MainWindow.xaml.cs` (use it for XSLT editors), `src/HoboXslt.App/HoboXslt.App.csproj`, `README.md`
**Touches:** [src/HoboXslt.App/Highlighting/**, src/HoboXslt.App/XsltHighlighting.cs, src/HoboXslt.App/MainWindow.xaml.cs, src/HoboXslt.App/HoboXslt.App.csproj, README.md]

### Approach
- One XSHD file (AvalonEdit's highlighting format), regex/span based, embedded resource, loaded once; colours from the mockup's syntax tokens (docs/design/main-window-mockup.html: tag, attr, val, pi, expr) plus a couple more for XPath parts. Lexical only, not a full XPath parser.
- XSLT editors (main + extra tabs) use it; the XML input and Output keep the existing XML definition.
- README: one line.

### Acceptance
- [ ] Screenshot of a realistic XSLT (choose/when tests with $variables and string literals, select with functions and axes, AVTs) showing distinct colours
- [ ] Large XSLT stays responsive
- [ ] `dotnet build HoboXslt.slnx` and `dotnet test tests/HoboXslt.Core.Tests` green

## Acceptance
- [ ] TBD

## Done summary
The XSLT editors (the main one and the extra include/import tabs) now use their own lexical XSHD definition. xsl: elements are blue and literal result elements teal. XPath in select/test/match/use/count/from/value/group-*/use-when/xpath/... attributes and in {...} attribute value templates is coloured by token: variables purple, strings green, numbers red, functions blue, axes/@/operators/keywords bold ink, everything else in the mockup's orange expression colour. Text content keeps the default ink. The XML input and Output panes keep the existing XML definition.

Files: src/HoboXslt.App/Highlighting/XSLT.xshd (new, embedded resource), src/HoboXslt.App/XsltHighlighting.cs (new loader; a static property, so it loads once on first use), HoboXslt.App.csproj (EmbeddedResource), MainWindow.xaml (main XsltEditor uses x:Static, plus the local xmlns), MainWindow.xaml.cs (one line, extra XSLT tabs), README.md (one line).

Gates: baseline green (build + 49 tests) before the edit. After: `dotnet build HoboXslt.slnx` passed with 0 warnings and 0 errors, and `dotnet test tests/HoboXslt.Core.Tests` passed 49 of 49.

Manual run (Debug build, driven by UI Automation against the app's own process only):
- Token dump: a scratch AvalonEdit DocumentHighlighter probe over the sample checked each token's colour (choose/when with $ars and strings, functions, ancestor::/following-sibling::, AVTs including {{escaped}}, xsl:message, literal result elements, text, single-quoted attributes with "strings").
- Screenshot of the sample XSLT with input.xml loaded: C:\Users\JSL\AppData\Local\Temp\claude\C--Users-JSL-Documents-hobo-xslt\bdbba1d5-d9de-4a5e-ba81-448ec5825a09\scratchpad\t22\shots\xslt-colours.png. The XML input pane still shows the old XML colours.
- Zoomed crop of the XSLT pane: C:\Users\JSL\AppData\Local\Temp\claude\C--Users-JSL-Documents-hobo-xslt\bdbba1d5-d9de-4a5e-ba81-448ec5825a09\scratchpad\t22\shots\xslt-colours-zoom.png
- Large XSLT (4805 lines): the highlighter runs over the whole document in about 100 ms, cold. 300 characters typed (posted WM_CHAR) into line 21 appeared in 737 ms, polling included; screenshot: C:\Users\JSL\AppData\Local\Temp\claude\C--Users-JSL-Documents-hobo-xslt\bdbba1d5-d9de-4a5e-ba81-448ec5825a09\scratchpad\t22\shots\large-xslt-after-typing.png

Deviations:
- MainWindow.xaml is touched (it is not in the Touches list), because the main XsltEditor's highlighting is set there (SyntaxHighlighting="XML"). It now points to XsltHighlighting.Definition via x:Static.
- Three colours were added beyond the mockup tokens: Element #0E7490 (literal result elements), XPathNumber #B42346, and XPathOperator ink #1C2230 in bold. Colours are hard-coded in the XSHD (light theme only), not read from App.xaml brushes.
- The extra XSLT tab path (include/import) was not driven in the manual run. It uses the same one-line definition.
- There are no automated tests for the colouring, because no App test project exists. Verification is the probe token dump plus the screenshots.

Known lexical limits (by design, no XPath parser): the xsl prefix must be literally `xsl:`. Word operators such as div, and, or are only coloured when followed by whitespace or "(" (so match="div" stays a name). An unclosed string or brace recovers at the next "<".

Follow-ups (not done): dark theme colours if the app ever gets a dark theme.

stage: impl-review - skipped(conductor: two new separate files + 3 wiring lines; conductor read the diff and the screenshots) Integrated verify at merge 5c00cde: dotnet build rc=0; dotnet test 49/49.
## Evidence
- Commits: 324537fb5b16dc006111126b878be433a1d565ae
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: