# fn-1-xslt-editor-and-debugger-xmlspy-like.27 Reset layout button

## Description
A small button in the top bar restores the default window size and pane layout, centres the window and saves the reset layout.

## Acceptance
- [ ] Reset restores default sizes and saves it (measured)

## Done summary
Top bar button (layout icon, tooltip/name Nulstil layout / Reset layout) left of the caption buttons. The default layout is captured from the XAML values before a saved layout is applied, so no numbers are duplicated; reset sets WindowState Normal, applies the default, centres on the work area and saves Layout = null (the close then saves the default layout's values).
Manual run (own instance, user's settings backed up and restored): changed layout to 1300x760 / 523,360,383 / bottom 240, restart, reset -> 1400x800 centred, panes 414/538/414, editors bottom back to 697, settings.json layout null; after close the default layout is saved. Top bar screenshot scratchpad\guides\shots\topbar.png.
Gates: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests 51/51.
stage: impl-review - skipped(conductor: one button; verified in the app)
## Evidence
- Commits: 37128904e0cd73225eee543b1122b2f389665dc7
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: