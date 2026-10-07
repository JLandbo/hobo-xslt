---
satisfies: [R19]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.10 Orange background on the paused line

## Description
Show the paused line with an orange background so stepping is visible.

**Size:** S
**Files:** `src/HoboXslt.App/PausedLineRenderer.cs` (new), `src/HoboXslt.App/MainWindow.xaml.cs`, `src/HoboXslt.App/App.xaml`
**Touches:** [src/HoboXslt.App/**]

### Approach
- An AvalonEdit `IBackgroundRenderer` (KnownLayer.Background) that fills the paused line in the editor showing it; one instance per XSLT tab editor, or one that tracks file key + line.
- Set it in `ShowPause` (after `ShowXsltLine`), clear it when the session resumes (continue/step), stops, completes or fails, and when the window closes.
- Color: an orange brush defined once in App.xaml (pick from the mockup's palette: the pause badge's amber/orange family), readable with the syntax colors on top.

### Acceptance
- [ ] Manual run: paused line has an orange background; Step over/into/out and Fortsæt move it; Stop and completion remove it (screenshots saved)
- [ ] Pause in an included file highlights the line in that file's tab only
- [ ] `dotnet build HoboXslt.slnx` and `dotnet test tests/HoboXslt.Core.Tests` green

## Acceptance
- [ ] TBD

## Done summary
TBD

## Evidence
- Commits:
- Tests:
- PRs:
