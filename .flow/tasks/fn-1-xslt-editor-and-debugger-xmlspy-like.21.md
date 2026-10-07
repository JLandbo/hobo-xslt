---
satisfies: [R1]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.21 Folding and indentation guides in the editors

## Description
User request: XML folding (collapse/expand elements) with a narrow fold margin, and indentation guides (faint vertical dotted lines per indentation level, like XmlSpy) in the XML and XSLT editors (incl. extra XSLT tabs). Keep the code in its own small files and wire it in with a few lines.

**Size:** S
**Files:** `src/HoboXslt.App/EditorFolding.cs` (new), `src/HoboXslt.App/IndentGuideRenderer.cs` (new), `src/HoboXslt.App/MainWindow.xaml.cs` (wiring in SetUpEditor/OpenXsltTab), `src/HoboXslt.App/App.xaml` (brushes/width if needed), `README.md`
**Touches:** [src/HoboXslt.App/EditorFolding.cs, src/HoboXslt.App/IndentGuideRenderer.cs, src/HoboXslt.App/MainWindow.xaml.cs, src/HoboXslt.App/App.xaml, README.md]

### Approach
- Folding: AvalonEdit's `FoldingManager.Install` + `XmlFoldingStrategy`; update folds after text changes with a short debounce (e.g. 300-500 ms DispatcherTimer) so huge files do not stutter. Make the fold margin narrow (the user asked it not to take much space) and style its markers with the app's brushes.
- Indentation guides: an `IBackgroundRenderer` drawing faint dotted vertical lines at each indentation level of visible lines (use the leading whitespace and the editor's indentation size / tab width); app brush (faint). Visible lines only.
- Keep breakpoints, the orange paused line and the breakpoint margin working; the fold margin goes after the line numbers.
- README: one line each.

### Acceptance
- [ ] Manual run: XML and XSLT (incl. an extra tab) show narrow fold markers; collapsing/expanding works; guides visible; screenshot
- [ ] A 20,000-line file stays responsive while typing (folds update after the pause)
- [ ] `dotnet build HoboXslt.slnx` and `dotnet test tests/HoboXslt.Core.Tests` green

## Acceptance
- [ ] TBD

## Done summary
TBD

## Evidence
- Commits:
- Tests:
- PRs:
