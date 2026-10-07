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
Folding and indentation guides in the XML and XSLT editors (incl. extra XSLT tabs): EditorFolding.cs installs AvalonEdit's XmlFoldingStrategy with a 400 ms debounce and a narrow fold margin (about 13 px, app brushes) after the line numbers; IndentGuideRenderer.cs draws faint dotted guides for visible lines; MainWindow.xaml.cs gets 5 wiring lines in SetUpEditor (Output editor excluded); README one line each.

Tier: session (jev-unavailable(no_key))
baseline: green via handoff (dotnet test 44/44 at ae0c8db)
Gates: dotnet build HoboXslt.slnx rc=0 (0 warnings), dotnet test tests/HoboXslt.Core.Tests rc=0 (44/44).
Manual run (UI Automation, measured): XML, main XSLT and the extra inc.xsl tab show narrow fold markers; collapse and expand work by clicking them; the guides show; breakpoint, unbound-ring margin and the orange paused line still work while debugging. Screenshots: C:\Users\JSL\AppData\Local\Temp\claude\C--Users-JSL-Documents-hobo-xslt\bdbba1d5-d9de-4a5e-ba81-448ec5825a09\scratchpad\t21\shots\1-open.png, 2-folded.png, 3-expanded-main.png, 4-paused.png, 6-final.png.
Responsiveness (measured, 22,860-line XML, 60 keys back to back): head avg 23 ms / max 33 ms per key against base avg 25 ms / max 45 ms; the UI ping during the pause after typing (fold rebuild) peaked at 142 ms against 106 ms on base. UI Automation's own round trip is about 110 ms, so the rebuild costs a single hitch of tens of ms after the pause (inferred).
Deviation: guides sit at the indentation of each enclosing line, not at multiples of the editor's indentation size, because the sample files use 2-space indentation and size 4 would skip every other level. Tabs expand with the indentation size.
App.xaml unchanged: existing brushes FaintBrush/PanelBrush/AccentBrush/AccentSoftBrush are used.
Follow-ups (not done): a paused line or breakpoint inside a collapsed fold is not unfolded; the orange shows on the folded line, and a breakpoint dot inside a fold is hidden. The orange paused line covers the guides on that line.

stage: impl-review - skipped(conductor: two new separate files + 6 wiring lines; conductor read both files and the screenshots) Integrated verify at merge 154bba3: dotnet build rc=0; dotnet test 49/49.
## Evidence
- Commits: 3a8c5009480a743b63834a1e96d537c03ed45d0b
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: