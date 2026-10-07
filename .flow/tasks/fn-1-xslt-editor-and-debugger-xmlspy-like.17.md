---
satisfies: [R25]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.17 Licenses window under Hjælp

## Description
Add one Hjælp menu item, 'Licenser…' / 'Licenses…', that opens a small modal window in the app's own style listing the shipped third-party components.

**Size:** S
**Files:** `src/HoboXslt.App/LicensesWindow.xaml(.cs)` (new), `src/HoboXslt.App/MainWindow.xaml(.cs)`, `src/HoboXslt.Core/Languages/Dansk.json`, `src/HoboXslt.Core/Languages/English.json`, `README.md`
**Touches:** [src/HoboXslt.App/**, src/HoboXslt.Core/Languages/**, README.md]

### Approach
- Keep it simple: a small window (owner = main window, centered on owner, not resizable or minimal), the app's existing brushes/fonts/card style from App.xaml, one row per component: name + version, license, clickable link. A close button.
- Components (verified by the conductor from the package files and project pages; use exactly these):
  - Saxon-HE 12.10 — Mozilla Public License 2.0 — https://github.com/Saxonica/Saxon-HE
  - IKVM 8.15 — zlib License; OpenJDK class library: GPL v2 with Classpath Exception — https://github.com/ikvmnet/ikvm
  - IKVM.ByteCode 9.3 — MIT — https://github.com/ikvmnet/ikvm-bytecode
  - XML Resolver 5.3 — Apache License 2.0 — https://codeberg.org/xmlresolver/xmlresolver
  - AvalonEdit 6.3 — MIT — https://github.com/icsharpcode/AvalonEdit
  - .NET 10 / WPF — MIT — https://github.com/dotnet/runtime and https://github.com/dotnet/wpf
- Links open in the default browser (Process.Start with UseShellExecute); a failure shows a message box in the app's existing style.
- Window title, menu item, headers and button text from Dansk.json/English.json; switch language live like the rest.
- README: mention the menu item in one line.

### Acceptance
- [ ] Manual run: Hjælp → Licenser… opens the window; it looks like the rest of the app (screenshot); a link opens the browser
- [ ] Switching language updates the window texts
- [ ] `dotnet build HoboXslt.slnx` and `dotnet test tests/HoboXslt.Core.Tests` green (language key parity test covers the new keys)

## Acceptance
- [ ] TBD

## Done summary
Hjælp → Licenser… opens a small modal licenses window (owner-centered, not resizable) in the app's card/table style, listing Saxon-HE, IKVM (zlib + OpenJDK class library GPL v2 with Classpath Exception), IKVM.ByteCode, XML Resolver, AvalonEdit and .NET / WPF with version, license and clickable links; links open via Process.Start (UseShellExecute) and a Win32Exception shows the existing Document.OpenFailed message box. Texts (menu item, title, card header, column headers, OpenJDK label, Luk/Close) come from Dansk.json/English.json via DynamicResource, so they follow the live language switch; README lists the menu item and no longer says Hjælp is empty.

Conductor addition (user request): App.xaml now gives every menu item below the top level the app's style - popup like MenuHeader's (PanelBrush, LineBrush border, CornerRadius 6, Padding 4), LineSoftBrush highlight, InkBrush text, muted input gesture text on the right, accent check mark for IsChecked, right arrow and right-opening popup for SubmenuHeader (Vis → Sprog), disabled items at 0.4 opacity like the tool buttons, and menu separators as a soft line. It is an implicit MenuItem style, so the editors' default TextBox context menu items (e.g. the XPath box) also pick up the item template inside the default context-menu chrome.

Verification: dotnet build HoboXslt.slnx green (0 warnings); dotnet test tests/HoboXslt.Core.Tests 44/44 green (language key parity covers the 9 new keys). Manual UI Automation run: licenses window opened from Hjælp in Danish and, after Vis → Sprog → English, from Help in English with all headers/title/button translated (screenshot checked); Vis → Sprog, Filer, Debug and Hjælp menus screenshotted in the new style. Link click: the Saxon-HE Hyperlink was invoked once via UIA with no app error; the browser tab itself was not observed (inconclusive). The link-failure message box path was not exercised. settings.json left on Dansk; no app/worker process left running.
baseline: green via handoff (green (verified at 248cb263 by fn-1-xslt-editor-and-debugger-xmlspy-like.16))

Tier: session (jev-unavailable(no_key))


stage: impl-review - skipped(user request: 'Bare spring review over'; reviewer stopped before a verdict, round refunded)
Conductor checked the screenshots (licenses window, Vis → Sprog submenu). Integrated verify at 73ff989: dotnet build rc=0; dotnet test 44/44 on two consecutive runs (one earlier run had 1 failure while the stopped reviewer was still building/testing the same checkout).

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: fb65b8ecefc2c78dd1f0a97d4b1ffd664f5bf8db, 73ff9890996cf4bb6b8f369af6cd5aafd3f2ee02
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: