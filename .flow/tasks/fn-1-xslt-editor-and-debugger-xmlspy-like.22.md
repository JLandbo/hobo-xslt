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
TBD

## Evidence
- Commits:
- Tests:
- PRs:
