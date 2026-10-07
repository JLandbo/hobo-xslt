# fn-1-xslt-editor-and-debugger-xmlspy-like.25 Replace the Windows title bar with the app's top bar

## Description
WindowChrome makes the top bar (logo, menus, file path) the caption; the window draws its own minimize, maximize/restore and close buttons. The Windows 11 snap menu on the maximize button is not shown (user accepted).

## Acceptance
- [ ] No Windows title bar (screenshot)

## Done summary
WindowChrome (CaptionHeight 34, GlassFrameThickness 0,0,0,1 for the shadow) turns the app's top bar into the caption; the menu and the new caption buttons (Segoe Fluent Icons glyphs, close hover red) are hit-test visible. Maximized: RootGrid gets an 8 px margin, as the window reaches 8 px past the screen (measured -8,-8 - 1928,1040 on a 1920x1032 work area); the maximize glyph switches to restore.
Manual run: screenshot normal and maximized (no Windows bar, buttons at the right); maximize button -> maximized; close button -> app exited; drag on the bar moved the window 100,50; double-click on the bar maximized. Screenshots scratchpad\guides\shots\chrome-normal.png, chrome-max-top.png.
Gates: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests 50/50.
stage: impl-review - skipped(conductor: window chrome only; verified in the app)
## Evidence
- Commits: cb025283f16982e69d5903c7a4ec6d339f8a500d
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: