---
satisfies: [R15, R16]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.8 Single-file exe publish and install.ps1

## Description
Publish the WPF app as one exe like the user's Hoboman app, and add `install.ps1` modelled on Hoboman's script.

**Size:** M
**Files:** `src/HoboXslt.App/HoboXslt.App.csproj`, `src/HoboXslt.Core/HoboXslt.Core.csproj`, `install.ps1`, `.gitignore`
**Touches:** [src/HoboXslt.App/HoboXslt.App.csproj, src/HoboXslt.Core/HoboXslt.Core.csproj, install.ps1, .gitignore]

### Approach
- Reference: `C:\Users\JSL\Documents\Hoboman\src\Hoboman\Hoboman.csproj` publish properties (`PublishSingleFile`, `IncludeNativeLibrariesForSelfExtract`, `PublishReferencesDocumentationFiles=false`, `DebugType=embedded`) and `C:\Users\JSL\Documents\Hoboman\install.ps1` (wait while the app from the publish folder runs, `dotnet publish ... -c Release -o publish`, throw on failure, Start menu shortcut via WScript.Shell, `explorer.exe $exe`). Do NOT copy Hoboman's Claude/MCP registration step. Messages in Danish.
- Risk to prove first: IKVM (Saxon) under single-file. IKVM ships runtime files (an `ikvm` folder with per-RID native/Java image files). Verify the published single exe, run alone from an empty folder, can run a transformation, a debug pause and an XPath evaluation. If IKVM cannot work from a single file, stop and report with evidence and the options (e.g. what extra files must sit next to the exe) instead of hacking around it.
- Make the app not depend on `Assembly.Location` (empty in single-file); use `AppContext.BaseDirectory` where a path is needed.
- Add `publish/` to `.gitignore`.

### Acceptance
- [ ] `dotnet publish src/HoboXslt.App -c Release -o publish` produces one exe (plus nothing required next to it)
- [ ] Copied alone to an empty folder, the exe starts and runs a transformation, a debug session that pauses at a breakpoint, and an XPath evaluation (manual run, evidence saved)
- [ ] `install.ps1` publishes, creates the Start menu shortcut and starts the app; a failed publish stops with a message (manual run)
- [ ] `dotnet build HoboXslt.slnx` and `dotnet test tests/HoboXslt.Core.Tests` green

## Acceptance
- [ ] TBD

## Done summary
TBD

## Evidence
- Commits:
- Tests:
- PRs:
