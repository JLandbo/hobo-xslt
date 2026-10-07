---
satisfies: [R21]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.13 Ctrl+S saves the focused file

## Description
Ctrl+S saves the XML input or the selected XSLT tab, whichever editor has (or last had) focus.

**Size:** S
**Files:** `src/HoboXslt.App/MainWindow.xaml(.cs)`, language files if a new text is needed, `README.md`
**Touches:** [src/HoboXslt.App/**, src/HoboXslt.Core/Languages/**, README.md]

### Approach
- Track the last focused editor (XML editor or an XSLT tab editor; the Output editor never counts). Ctrl+S (KeyBinding or in `Window_PreviewKeyDown`) calls that document's existing Save path (untitled → save dialog; failure → existing message).
- Read-only editors during a debug session: saving is harmless but keep behaviour consistent with the existing Gem buttons (disabled or not during sessions — follow them).
- Show Ctrl+S as the gesture text on the Filer → Gem items; add it to the README's keyboard table and save section.

### Acceptance
- [ ] Manual run: edit XML, Ctrl+S saves XML; edit an included XSLT tab, Ctrl+S saves that tab; focus on Output after editing XSLT → Ctrl+S saves the XSLT
- [ ] README keyboard shortcuts updated
- [ ] `dotnet build HoboXslt.slnx` and `dotnet test tests/HoboXslt.Core.Tests` green

## Acceptance
- [ ] TBD

## Done summary
Ctrl+S saves the file of the XML editor or the selected XSLT tab, whichever editor last had keyboard focus; focus on Output (or anywhere else) keeps the last XML/XSLT choice. It works by pointing Ctrl+S at the existing pane save button (new names SaveXmlButton / SaveXsltButton) in `Window_PreviewKeyDown`, so it uses the existing Save path: untitled opens the save dialog, a failed save shows the existing message. Like those buttons, it stays enabled during debug sessions. Filer -> Gem XML / Gem XSLT show `Ctrl+S`; the README has a keyboard-table row and a bullet in "Det skal du vide om at gemme". No new language text: gesture texts are not translated, matching F5 and the other gestures.

- Measured: `dotnet build HoboXslt.slnx` rc=0, 0 warnings; `dotnet test tests/HoboXslt.Core.Tests` rc=0, 36/36 passed. Baseline: green via handoff (f12ad64; only .flow/ changed since).
- Measured (UI Automation drive of the Debug build, Danish, copies of the sample files): moms.xsl opened as an included tab by stepping into it. (1) Edited the XML, pressed Ctrl+S: the title lost `*` and ordrer.xml on disk changed; moms.xsl did not change. (2) Edited the moms.xsl tab, pressed Ctrl+S: the tab lost `*` and moms.xsl on disk changed. (3) Edited both XML and moms.xsl, clicked into Output, pressed Ctrl+S: moms.xsl was saved and the XML kept its `*`. A screenshot shows `Ctrl+S` beside Gem XML and Gem XSLT in the Filer menu.
- Not run: the untitled save dialog and the failed-save message through Ctrl+S. Inferred: both come from the same `EditorDocument.Save()` the buttons call.
- GATE classify: FULL (src/HoboXslt.App/MainWindow.xaml), so the focused Quick commands above ran.

Tier: session (jev-unavailable(no_key))


stage: impl-review - ran (host, one reviewer, SHIP; receipt /tmp/impl-review-receipt-22e37af6a82b-fn-1-xslt-editor-and-debugger-xmlspy-like.13.json) (model: fable)
Integrated verify at a8e1438: dotnet build rc=0; dotnet test green.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: a8e143886d1755cb76c364733fde0854c761ba67
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: