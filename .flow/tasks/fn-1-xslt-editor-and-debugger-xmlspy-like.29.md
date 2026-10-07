# fn-1-xslt-editor-and-debugger-xmlspy-like.29 Separate pane layouts for editing and debugging

## Description
Two pane layouts (three pane widths, which panes are hidden, bottom panel height): one for editing, one for debugging. The app switches to the debug layout when a debug session starts and back when it ends; both are saved. The first debug uses the default layout; reset resets both. The window size stays shared.

## Acceptance
- [ ] Switching measured in the app, both layouts survive a restart

## Done summary
Layout is now (Width, Height, Edit, Debug) with PaneLayout(XmlWidth, XsltWidth, OutputWidth, BottomHeight, XmlHidden, OutputHidden). CurrentPanes stores the XSLT width as if all panes were shown (it subtracts the width a hidden pane lent it), which also fixes the review's P2 (a hidden pane's width was counted twice on save). ApplyPanes restores widths and hidden panes (SetPane shared with TogglePane). Debug_Click switches to the debug panes (default the first time) when the session starts and back when it ends; OnClosing saves both; reset sets both to the default. Answers from the user: switch back when debug ends, first debug uses the default layout, reset resets both.
Manual run (own instance, settings backed up and restored): edit hid Output (414/926/rail) -> debug paused showed default 414/538/414 -> hid XML in debug -> stop restored edit (Output hidden) -> second debug restored debug (XML hidden) -> stop back to edit; settings.json had both layouts with the hidden flags; restart showed the edit layout with Output hidden. Screenshots scratchpad\guides\shots\two-debug.png, two-edit.png.
Gates: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests 51/51 (layout test now covers both layouts).
stage: impl-review - skipped(conductor: builds on the reviewed layout code; measured in the app)
## Evidence
- Commits: 7242837a5d509115c500684c5ed6b3e0699bb577
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: