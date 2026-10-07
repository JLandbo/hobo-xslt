---
satisfies: [R18]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.11 All UI texts from Danish/English language files

## Description
Move every user-facing text out of the source into language files, Danish and English, switchable under Vis → Sprog, remembered between starts.

**Size:** M
**Files:** `src/HoboXslt.Core/Languages/Translation.cs`, `src/HoboXslt.Core/Languages/Translator.cs`, `src/HoboXslt.Core/Languages/Dansk.json`, `src/HoboXslt.Core/Languages/English.json`, `src/HoboXslt.Core/HoboXslt.Core.csproj`, `src/HoboXslt.Core/Settings.cs` (new), `src/HoboXslt.App/**`, `tests/HoboXslt.Core.Tests/LanguageTests.cs` (new)
**Touches:** [src/HoboXslt.Core/**, src/HoboXslt.App/**, tests/HoboXslt.Core.Tests/**]

### Approach
- Follow the user's Hoboman app (read-only reference): `C:\Users\JSL\Documents\Hoboman\src\Hoboman.Core\Languages\Translation.cs`, `Translator.cs`, `Dansk.json`, `English.json`, the `EmbeddedResource ... LogicalName="Languages/%(Filename)%(Extension)"` item in `Hoboman.Core.csproj`, and how Hoboman's XAML and code bind to the translator (search its `src\Hoboman` for `Translator`/`Of(`) for live switching.
- Every text the user sees: menus, buttons, tooltips, pane headers and meta, tab headers, column headers, status bar texts, pause badge, message boxes, save prompt, open/save dialog filters and titles, variable scope labels, 'utilgængelig', diagnostic kind labels, Core's own diagnostic texts (e.g. the recursion message). Saxon's own messages stay as Saxon writes them.
- Vis → Sprog with Dansk and English (checked item = current); switching updates the window at once.
- Remember the choice in `%LOCALAPPDATA%\hobo-xslt\settings.json`; unreadable or missing file → Danish, no crash.
- Keep texts Danish-identical to today in Dansk.json; write natural English in English.json.

### Acceptance
- [ ] A search of src/ for Danish UI strings (æ/ø/å and the known words like 'Kør', 'Gem', 'Åbn', 'Fejl') finds them only in Dansk.json (list the search command and result)
- [ ] Test: every key in Dansk.json exists in English.json and vice versa
- [ ] Test: unknown key falls back to Danish; missing/corrupt settings file → Danish
- [ ] Manual run: switch to English under Vis → Sprog — all visible texts change at once; restart keeps English; switch back to Dansk (screenshots saved)
- [ ] `dotnet build HoboXslt.slnx` and `dotnet test tests/HoboXslt.Core.Tests` green

## Acceptance
- [ ] TBD

## Done summary
TBD

## Evidence
- Commits:
- Tests:
- PRs:
