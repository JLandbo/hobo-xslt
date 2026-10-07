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
All UI texts now come from embedded language files (src/HoboXslt.Core/Languages/Dansk.json, English.json) through Translation/Translator in Core, modelled on Hoboman. XAML reads them as dynamic resources and code-set texts (status, meta, pause badge, caret, XPath empty sequence, document titles) are re-applied on a switch. Vis -> Sprog lists Dansk and English with the current one checked and switches at once. The choice is saved in %LOCALAPPDATA%\hobo-xslt\settings.json (Settings.cs); a missing or corrupt file gives Danish. The recursion diagnostic comes from the language file; DebugSession takes the Translator for it.

Search (measured): Select-String over src/**/*.cs,*.xaml,*.json,*.csproj (bin/obj excluded) for `[æøåÆØÅ]|\b(Kør|Gem|Åbn|Fejl|Klar|Vis|Sprog|Hjælp|Filer|Rediger|linje|linjer|Lokal|utilgængelig|Fortsæt|Evaluér|Variabler|Pauset|Starter|Stopper|Kunne)\b`: 43 hits, all in Dansk.json.
Left in source as language-neutral: product name "hobo-xslt", type badges XML/XSLT, "Saxon-HE 12 · XSLT 3.0", encoding/line-ending values, key gestures.
Tests: LanguageTests.cs - key parity Dansk/English, missing key falls back to Danish, missing and corrupt settings file give Danish.

Manual run (measured; screenshots and UIA dumps in C:\Users\JSL\AppData\Local\Temp\claude\C--Users-JSL-Documents-hobo-xslt\bdbba1d5-d9de-4a5e-ba81-448ec5825a09\scratchpad\t11, driver drive11.ps1, logs run-*.log):
- corrupt settings.json -> started in Danish ('Klar', menu 'Filer'), no crash.
- paused at ordrer.xsl:18, Vis -> Sprog -> English: every differing text changed at once (menus, buttons, tooltips, pane meta, tab headers, column headers, Local/Global, pause badge, status bar; dump-1 vs dump-2). XPath empty result showed 'Empty sequence'; Stop showed 'Debug session stopped'. settings.json -> English.
- restart -> English ('Ready', 'not saved'); View -> Language -> Dansk -> 'Klar', 'ikke gemt'. settings.json left as {"languageName": "Dansk"} (no file existed before the run).
Not run: the Compile error / Runtime error kind labels after a switch (inferred: same DataTrigger + DynamicResource pattern as the measured Lokal/Global column), and message boxes / file dialogs in English (inferred: read via the translator at show time).
Changed: Debug.TooDeep is Danish in Dansk.json (it was English in the source); Open/Save dialogs now get a title from the language files ("Åbn" / "Gem som").

baseline: green via handoff (6e59cac, only .flow/ changed since)
Gates: dotnet build HoboXslt.slnx rc=0 (0 warnings); dotnet test tests/HoboXslt.Core.Tests rc=0 (36 passed)
Tier: session (jev-unavailable(no_key))



stage: impl-review - ran (host, 3 draws, SHIP; receipt /tmp/impl-review-receipt-22e37af6a82b-fn-1-xslt-editor-and-debugger-xmlspy-like.11.json) (model: fable)
Follow-up (P3): the recursion-limit diagnostic text keeps the language active when it was raised.
Integrated verify at d720b0e: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests green. Danish-string search over src excluding *.json: no matches.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: d720b0e567988dd35d733ba93ec9a587d2a3174a
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: