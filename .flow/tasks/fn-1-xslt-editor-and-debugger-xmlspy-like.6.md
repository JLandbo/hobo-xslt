---
satisfies: [R13]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.6 Apply the approved mockup design to the main window

## Description
Restyle the existing WPF main window so it looks like the approved mockup. No new behaviour beyond R13's menus, pause badge and status bar fields; every existing feature (tabs, save rule, run, debug, XPath, breakpoints, variables) keeps working.

**Size:** M
**Files:** `src/HoboXslt.App/MainWindow.xaml`, `src/HoboXslt.App/MainWindow.xaml.cs`, `src/HoboXslt.App/App.xaml` (shared styles/brushes), possibly `src/HoboXslt.App/BreakpointMargin.cs` (marker colors)
**Touches:** [src/HoboXslt.App/**]

### Approach
- Open `docs/design/main-window-mockup.html` and read its `:root` light tokens (colors, syntax colors) and CSS; define them once as WPF resources (brushes) in App.xaml and use them everywhere — no hard-coded colors in controls.
- Layout per mockup: title row (brand mark, Menu with Filer/Rediger/Vis/Kør/Debug/Hjælp holding the existing commands only, run file path right-aligned), toolbar (icon buttons drawn with Path geometry; labeled Kør/Debug; Fortsæt, Step into/over/out, Stop; pause badge right-aligned while paused), XPath bar, three card panes with GridSplitters, GridSplitter above the bottom panel, bottom TabControl (Variabler, Fejl og beskeder), status bar.
- Pane headers: type badge (XML / XSLT / XML), file name, meta text (e.g. line count, breakpoint count, last run time).
- Status bar: Saxon-HE 12 · XSLT 3.0, session state, Ln/Col of the focused editor, encoding, line endings.
- AvalonEdit syntax colors: adjust the XML highlighting definition's colors to the mockup's tag/attribute/value/comment colors.
- Fonts: Segoe UI for UI, Cascadia Mono for code.

### Investigation targets
**Required**:
- `docs/design/main-window-mockup.html` — the design to match
- `src/HoboXslt.App/MainWindow.xaml` — current layout to restyle
- Run notes in the flow-notes dir (fn-1.3, fn-1.4, fn-1.5) for integration details

### Key context
- Light theme only (user decision).
- Do not change Core.

### Acceptance
- [ ] Side-by-side screenshot of the running app (paused debug session with an included file tab open) next to the mockup shows the same structure, colors and typography (manual run, screenshot saved)
- [ ] All six menus present; Filer/Kør/Debug hold the existing commands and they work (manual run)
- [ ] Splitters drag pane widths and the bottom panel height (manual run)
- [ ] Status bar shows engine, session state, Ln/Col, encoding and line endings (manual run)
- [ ] Existing behaviour unchanged: run, diagnostics navigation into tabs, save rule, XPath, breakpoints, stepping, variables (manual run)
- [ ] `dotnet build HoboXslt.slnx` and `dotnet test tests/HoboXslt.Core.Tests` green

## Acceptance
- [ ] TBD

## Done summary
TBD

## Evidence
- Commits:
- Tests:
- PRs:
