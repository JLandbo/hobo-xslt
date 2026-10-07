---
satisfies: [R26]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.23 Unfold around the paused line; guides above the paused highlight

## Description
Follow-ups from task 21: a collapsed fold that holds the paused XSLT line or the XML context node is opened on pause; the paused/context highlight is drawn under the indentation guides.

## Acceptance
- [ ] Folds around the paused line and the context node open on pause (screenshot)

## Done summary
Folds around the paused XML context node open on pause (EditorFolding.Reveal); in XSLT the caret move to the paused line already unfolds (AvalonEdit FoldingManager behaviour), so no extra call. PausedLineRenderer is inserted first, so indentation guides draw on top of the orange/blue highlight.
Review (quality-auditor, fable, correctness): SHIP; follow-ups applied with user OK: guide indent scan reads EndOffset once (measured 22k-line XML back-walk ~10 ms -> ~5-7 ms, identical result), redundant XSLT Reveal removed, XSLT colours set from App.xaml brushes (new XslElementBrush), pixel-identical XSLT pane.
Manual run: folded XSLT for-each and XML ordre, debug paused at main.xsl:9 -> both folds open, guides visible on the orange line (pixel check). Screenshots in scratchpad\guides\shots (f2-folded, f3-paused, brushes).
Gates: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests 49/49.
stage: impl-review - done(quality-auditor correctness SHIP; P2/P3 follow-ups applied)
## Evidence
- Commits: c5207252e38c867d4ab91335b84b458ddb6a63d8, e527146d78c222f4ae158ac2c8c027f87992fd30
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: