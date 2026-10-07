---
satisfies: [R1, R2, R4, R11]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.3 WPF shell: editors, open/save, run, diagnostics, output

## Description
The WPF app with three editors and a plain Run; the base the debugger UI and XPath panel plug into.

**Size:** M
**Files:** `src/HoboXslt.App/HoboXslt.App.csproj`, `src/HoboXslt.App/App.xaml(.cs)`, `src/HoboXslt.App/MainWindow.xaml(.cs)`, `HoboXslt.slnx`
**Touches:** [HoboXslt.slnx, src/HoboXslt.App/**]

### Approach
- `dotnet new wpf` net10.0-windows, reference Core and `AvalonEdit` (latest). XML highlighting via AvalonEdit's built-in `XML` definition.
- Panes: XML input editor, XSLT editor, read-only output editor, diagnostics list. Open/save per editor; open failure → message box, content unchanged.
- Run: save dirty documents first; untitled stylesheet → message, no run. Run on a background task; Run disabled while running and until Saxon warm-up (started at launch on a background task) has finished.
- Output: XML highlighting; if output is not well-formed XML, show it as plain text. Failed run clears output.
- Diagnostics: double-click navigates; if the file is not the open stylesheet (included file), open it in the XSLT editor first. Entries without location are not navigable.

### Investigation targets
**Required**:
- Task 1's run engine API and diagnostic shape
- AvalonEdit docs: `TextEditor`, `HighlightingManager`, caret/scroll APIs

### Key context
- Keep logic that is testable in Core; the app has no test project.

### Acceptance
- [ ] App starts, opens/edits/saves XML and XSLT files with highlighting (manual run)
- [ ] Run shows output; a failing stylesheet shows diagnostics and an empty output pane (manual run)
- [ ] Double-click on a diagnostic in an included file opens that file at the line (manual run)
- [ ] UI stays responsive during warm-up and runs (manual run)

## Acceptance
- [ ] TBD

## Done summary
TBD

## Evidence
- Commits:
- Tests:
- PRs:
