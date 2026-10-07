---
satisfies: [R20]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.12 App icon like Hoboman

## Description
Give the program its own icon, wired the way the user's Hoboman app does it, and show it in the README header.

**Size:** S
**Files:** `src/HoboXslt.App/HoboXslt.ico`, `src/HoboXslt.App/HoboXslt.png`, `src/HoboXslt.App/HoboXslt.App.csproj`, `README.md`
**Touches:** [src/HoboXslt.App/HoboXslt.ico, src/HoboXslt.App/HoboXslt.png, src/HoboXslt.App/HoboXslt.App.csproj, src/HoboXslt.App/MainWindow.xaml, README.md]

### Approach
- Reference (read-only): `C:\Users\JSL\Documents\Hoboman\src\Hoboman\Hoboman.csproj` (`<ApplicationIcon>Hoboman.ico</ApplicationIcon>`), `Hoboman.ico`/`Hoboman.png`, and the README header (`<p align="center"><img src="src/Hoboman/Hoboman.png" width="160" ...></p>` before the centered `<h1>`).
- Design: the mockup's brand mark (`docs/design/main-window-mockup.html`: `</>` in white on the blue rounded square, accent color #2a58c4). Render it yourself (e.g. a small PowerShell/WPF or System.Drawing script) to a multi-size .ico (16, 24, 32, 48, 64, 128, 256) and a 256px PNG. No downloads.
- Make sure the window/taskbar shows the icon (WPF uses the exe icon only when the window has no Icon set; set the window Icon if needed).
- The Start menu shortcut from install.ps1 points at the exe, so it inherits the icon; verify after a publish.

### Acceptance
- [ ] Published exe shows the icon in Explorer; running window and taskbar show it (screenshot)
- [ ] README header shows the PNG like Hoboman's
- [ ] `dotnet build HoboXslt.slnx` and `dotnet test tests/HoboXslt.Core.Tests` green

## Acceptance
- [ ] TBD

## Done summary
TBD

## Evidence
- Commits:
- Tests:
- PRs:
