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
TBD

## Evidence
- Commits:
- Tests:
- PRs:
