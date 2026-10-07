# fn-1-xslt-editor-and-debugger-xmlspy-like.28 Hide the XML and Output panes

## Description
A triangle button in the XML and Output pane headers hides the pane; it becomes a 28 px rail with an arrow and the pane name, and a click on the rail brings it back at its old width. The width goes to the XSLT pane and back, so the other side pane keeps its size. Design agreed in the canvas mockup.

## Acceptance
- [ ] Hide/show XML and Output; widths return exactly (measured)

## Done summary
XML header gets a left triangle (Skjul XML) at its left edge, Output header a right triangle (Skjul Output) at its right edge. Hiding sets the column to Auto (MinWidth 0), collapses the card and its splitter and shows a 28 px rail button (arrow + rotated name; Vis XML / Vis Output). The hidden width is kept; hiding and showing move that width to and from the XSLT column only (pixel star widths), so the other side pane keeps its size. Reset layout shows hidden panes first; on close a hidden pane's width is saved as it was. Hidden state is not remembered across restarts.
Manual run (own instance, settings backed up and restored): 414/538/414 -> hide XML 0/926/414 -> hide Output 0/1314/0 (rails 28) -> show XML 414/926 -> show Output 414/538/414. Screenshots scratchpad\guides\shots\panes-1-shown.png, panes-2-hidden.png, panes-3-back.png.
Gates: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests 51/51.
stage: impl-review - skipped(conductor: UI toggle; measured in the app)
## Evidence
- Commits: bad60efe84900547470e7070e7b299f823a748b1
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: