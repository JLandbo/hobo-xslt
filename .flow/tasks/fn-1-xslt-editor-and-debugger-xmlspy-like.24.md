# fn-1-xslt-editor-and-debugger-xmlspy-like.24 Clearer scrollbars and a per-pane word wrap toggle

## Description
Scroll thumbs a little darker; horizontal scrollbar inset 10 px from the left edge. A wrap toggle in the XML, XSLT and Output pane headers wraps long lines visually only; the choice is saved per pane in settings.json (Settings.Update keeps other settings).

## Acceptance
- [ ] Thumb darker, horizontal bar inset (screenshot)

## Done summary
Scroll thumbs use a new ScrollThumbBrush (#BEC5CF, slightly darker than LineBrush); the horizontal scrollbar is inset 10 px from the left like the right end. Word wrap: a ToggleButton (PaneToggle style, wrap icon) in each pane header binds TextEditor.WordWrap (extra XSLT tabs bind to the XSLT toggle); Settings gained XmlWordWrap/XsltWordWrap/OutputWordWrap and Settings.Update (load, change, save), so the language switch no longer overwrites other settings. ToolButton/PaneButton TargetType widened to ButtonBase so the toggle reuses the look. README line.
Manual run: toggled XSLT wrap, settings.json got xsltWordWrap=true, restart showed XSLT wrapped with the toggle lit and XML/Output unwrapped; user's settings.json restored afterwards. Screenshots scratchpad\guides\shots\scroll.png, wrap1.png, wrap2.png.
Gates: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests 50/50 (new Update_WhenLanguageChanges_ThenWordWrapIsKept).
stage: impl-review - skipped(conductor: small UI change, user-driven; conductor verified in the app)
## Evidence
- Commits: a43d173, 7a11bb17f135d4a28234ee69f8ca78efb36aa3e0
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: