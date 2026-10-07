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
TBD

## Evidence
- Commits:
- Tests:
- PRs:
