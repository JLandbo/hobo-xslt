# fn-1-xslt-editor-and-debugger-xmlspy-like.26 Remember window size and pane layout

## Description
Save the window's normal size, the three pane widths and the bottom panel height on close; restore on start. The window always starts centred; position and maximized state are not saved (user). If the saved size does not fit the work area, the default layout is used.

## Acceptance
- [ ] Changed layout comes back after restart (measured)

## Done summary
Settings gained Layout(Width, Height, XmlWidth, XsltWidth, OutputWidth, BottomHeight). MainWindow saves it in OnClosing (RestoreBounds size, pane ActualWidths, bottom row ActualHeight) and applies it in the constructor when it fits SystemParameters.WorkArea; pane widths are restored as star proportions. WindowStartupLocation=CenterScreen. Position and maximized state are deliberately not saved (user).
Manual run (own instance, user's settings.json backed up and restored): dragged XML/XSLT splitter +150 and the bottom splitter up 60, resized to 1300x760, closed -> settings.json layout 1300x760, panes 524/362/385, bottom 240; restart -> window 1300x760 centred at 310,136, panes 523/360/383, bottom panel the same; layout width 5000 -> default 1400x800 and default panes.
Gates: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests 51/51 (new Load_WhenLayoutWasSaved_ThenLayoutIsLoaded).
stage: impl-review - skipped(conductor: small settings change; verified in the app)
## Evidence
- Commits: 92dc6ceb7e2d3b479007b6a0457350d8b18d75b1
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: