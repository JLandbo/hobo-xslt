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
TBD

## Evidence
- Commits:
- Tests:
- PRs:
