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
Added PausedLineRenderer (AvalonEdit background renderer, one per XSLT tab editor) that fills the paused line with PausedLineBrush (#FCD9A5, App.xaml); ShowPause sets it on the tab ShowXsltLine returns, and SetPaused(false) clears it on every editor, so step, continue, stop, completion and failure all remove or move it.

Manual run (measured, screenshots in C:\Users\JSL\AppData\Local\Temp\claude\C--Users-JSL-Documents-hobo-xslt\bdbba1d5-d9de-4a5e-ba81-448ec5825a09\scratchpad\t10\shots, log run.log beside it): orange pixels on ordrer.xsl:18 at break, on moms.xsl:5 after Step into with zero on the ordrer.xsl tab, on ordrer.xsl:13 after Step over, :18 after Step out and Continue; zero after completion and after Stop.
Not run: the failure path (inferred: same SetPaused(false) call as completion) and window close (no explicit clear added; the editors close with the window).

baseline: green via handoff (d531e525, only .flow/ changed since)
Gates: dotnet build HoboXslt.slnx rc=0 (0 warnings); dotnet test tests/HoboXslt.Core.Tests rc=0 (32 passed)
Tier: session (jev-unavailable(no_key))


stage: impl-review - ran (host, one reviewer, SHIP; receipt /tmp/impl-review-receipt-22e37af6a82b-fn-1-xslt-editor-and-debugger-xmlspy-like.10.json) (model: fable)
Integrated verify at 6e59cac: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests green.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: 6e59cacdedc0a6e10fcf04833c7689e6f60a29ae
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests, manual UI run: drive10.ps1 (scratchpad t10) - break/step into/over/out/continue/stop/complete with orange-pixel counts
- PRs: