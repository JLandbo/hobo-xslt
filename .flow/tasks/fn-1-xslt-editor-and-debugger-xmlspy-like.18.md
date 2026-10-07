---
satisfies: [R1, R13]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.18 Vertical scrollbars missing in the XML and XSLT editors

## Description
Defect reported by the user with a screenshot: the XML input and XSLT editors show no vertical scrollbar, only the Output editor does (a 300+ line XSLT shows a horizontal scrollbar but no vertical one). Reproduce, find the cause, fix it.

**Size:** S
**Files:** `src/HoboXslt.App/App.xaml`, `src/HoboXslt.App/MainWindow.xaml(.cs)` (wherever the cause is)
**Touches:** [src/HoboXslt.App/**]

### Approach
- Reproduce first: open a long XML file and a long XSLT (also an extra included-file tab) in the built app and capture the app window. Read the visual tree (UI Automation or a debug probe) to see whether PART_VerticalScrollBar exists, its Visibility/ActualWidth/position, and whether the editor's ScrollViewer is wider/taller than its card (clipped).
- All three editors share the same implicit TextEditor style (MainWindow.xaml Window.Resources) and the implicit ScrollViewer/ScrollBar templates in App.xaml; Output works, XML/XSLT do not — find what differs (layout container, margins, breakpoint margin, focus/editing, content length).
- Fix the cause, not the symptom; keep the app's scrollbar look.

### Acceptance
- [ ] Before/after screenshots: XML pane, XSLT main tab and an extra XSLT tab show a vertical scrollbar when content is taller than the pane, and it scrolls
- [ ] Output still correct
- [ ] `dotnet build HoboXslt.slnx` and `dotnet test tests/HoboXslt.Core.Tests` green

## Acceptance
- [ ] TBD

## Done summary
The scrollbars were never missing. On a very large file, WPF's Track shrank the thumb to its minimum: 8 px, or a 4 px dot after the thumb's 2 px margin. The user's screenshot shows those dots: the XML pane at y 50-53, and the XSLT pane at x 872-877, y 58-61. The ScrollBar style in App.xaml now sets the two resource keys that Track reads for its minimum thumb length (VerticalScrollBarButtonHeightKey and HorizontalScrollBarButtonWidthKey, 56, so the minimum is 28 px). This applies to vertical and horizontal thumbs in every editor and keeps the app's scrollbar look. Per the conductor's re-target, the fix sets only the thumb minimum. Nothing was changed for the earlier "missing scrollbar" theory.

Why the resource keys and not Thumb.MinHeight: Track computes the thumb length as max(floor(buttonSize * 0.5), track * viewport / extent), and it looks up buttonSize through those keys (dotnet/wpf Track.ComputeScrollBarLengths). Its drag density is range / (track - thumb), so dragging still covers the full range. A MinHeight on the Thumb would not enter that calculation.

Defect route:
- prior fixes: git log on App.xaml/MainWindow.xaml shows no scrollbar fix; no open PRs or memory checked (memory not initialized)
- diagnosis: eliminated "scrollbar absent" (UIA: PART_VerticalScrollBar present, 10 px wide, on screen, in XML, XSLT main and extra tabs; 404-line files show visible thumbs); confirmed min-length thumb (UIA thumb rect 10x8 on 20,000-line files, before-2-extra.png matches the user's screenshot)
- introduced by: skipped: no known-good revision
- base: 805c718, 20,000-line XML, XSLT main, XSLT extra tab and Output: vertical thumb 8 px; 200,000-char line: horizontal thumb 8 px | head: f7a72dd, the same cases measure 28 px
- live: head app, drag to the bottom gives VerticalScrollPercent=100 with the thumb ending at the track bottom (575+28=603), drag to the top gives 0. Drag to the right on the wide line gives HorizontalScrollPercent=100. SetScrollPercent(50) puts the thumb centre at the track middle (407 vs 406.5). Output is unchanged. Shots: C:\Users\JSL\AppData\Local\Temp\claude\C--Users-JSL-Documents-hobo-xslt\bdbba1d5-d9de-4a5e-ba81-448ec5825a09\scratchpad\t18\shots\before-*.png, after-*.png; logs: before.log, after.log, before-wide.log, after-wide.log

baseline: green via handoff (verified at 73ff9890 by fn-1-xslt-editor-and-debugger-xmlspy-like.17; only .flow/ changed since)
Gates: dotnet build HoboXslt.slnx rc=0. dotnet test tests/HoboXslt.Core.Tests failed twice in a row with 1/44 (WorkerSessionTests.Debug_WhenStoppedWhileRunningEndless_ThenStopsWithoutOutputAndNextRunCompletes: Assert.False(run.IsCompleted) at the Stop helper), then passed with 44/44 on the third run. The test project does not reference HoboXslt.App, and the same suite passed with 44/44 at base 805c718, so the failure is a load-dependent timing flake (parallel workers were running), not this change. Follow-up: that test asserts the run is still going after a fixed 500 ms delay.


Tier: session (jev-unavailable(no_key))
stage: impl-review - skipped(policy: risk - 7-line scrollbar style change, display only; before/after screenshots and drag measurements cover it)
Integrated verify at 3153233: dotnet build rc=0; dotnet test 44/44.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: f7a72ddad4f0001e2cb48d5b1af21beec2ef9236
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: