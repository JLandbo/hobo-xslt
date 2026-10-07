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
Added the app icon: the mockup's </> brand mark on the #2a58c4 rounded square, rendered locally (WPF script, no downloads) to src/HoboXslt.App/HoboXslt.ico (16, 24, 32, 48, 64, 128, 256 px) and a 256 px HoboXslt.png. The app project sets `<ApplicationIcon>HoboXslt.ico</ApplicationIcon>` as Hoboman does; the README header shows the PNG at width 160 above the centered h1, like Hoboman's.

- Measured: `dotnet build HoboXslt.slnx` green (0 warnings), `dotnet test tests/HoboXslt.Core.Tests` 36/36 passed; baseline: green via handoff (48276a5, only .flow/ changed since).
- Measured: `dotnet publish src\HoboXslt.App -c Release` to a scratch folder; the exe's shell icon is the new icon. The published exe was started: its title bar and taskbar button show the icon (screenshots in .flow/tmp/handover/: exe-icon.png, window-titlebar.png, taskbar-button.png). MainWindow.xaml needed no change: WPF uses the exe icon when the window sets none.
- Inferred, not run: the Start menu shortcut from install.ps1 points at the exe, so it shows the same icon (install.ps1 not run per host note).
- Follow-up (not part of this task): the taskbar button's accessible name reads "HoboXslt.App" (assembly name), not "hobo-xslt".
- Note: the worktree started at 6bb72a2, one commit behind the declared base; it was fast-forwarded to 3356b3d (task files) before work began.


Tier: session (jev-unavailable(no_key))
stage: impl-review - skipped(policy: risk - icon asset and one csproj line, display only; covered by the spec completion review)
Integrated verify at f12ad64: dotnet build rc=0; dotnet test green.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: f12ad64776dd25f6b2a1af58d6c116771b11f972
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: