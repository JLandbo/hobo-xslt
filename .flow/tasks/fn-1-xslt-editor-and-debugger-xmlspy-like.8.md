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
The app now publishes as one exe (Hoboman's four publish settings plus IncludeAllContentForSelfExtract, which bundles IKVM's Java image and extracts it at startup), Core embeds its symbols, publish/ is ignored, and install.ps1 publishes, waits for a running copy, creates the Start menu shortcut hobo-xslt.lnk and starts the app; a failed publish throws "Publish af hobo-xslt fejlede." before the shortcut.

- Measured: with Hoboman's settings alone the lone exe starts but Saxon fails ("Saxon kunne ikke starte: The type initializer for '<Module>' threw an exception") because IKVM needs the ikvm\ folder and ikvm.properties next to the exe. With IncludeAllContentForSelfExtract the lone exe (257 MB, empty folder) ran a transformation, paused at a breakpoint (test.xsl:4, variable $n = 3, Lokal) and continued to completion, and evaluated XPath (3 / b). Log: C:\Users\JSL\Documents\hobo-xslt\.flow\tmp\handover\fn-1-xslt-editor-and-debugger-xmlspy-like.8-manual-run.log
- Measured: install.ps1 real run exit 0, shortcut created, app started; failed-publish run exit 1 with the message and no shortcut. The test shortcut was removed afterwards because it pointed into the temporary worktree; the user gets a real one when running install.ps1 from the repo.
- Note: publish\ also holds three IKVM-generated pdbs (Saxon.HE.pdb, org.xmlresolver.xmlresolver*.pdb). They are not needed at runtime (the lone-exe run proves it); removing them would mean DebugSymbols=false for the Maven reference, not done.
- Deviation to confirm: IncludeAllContentForSelfExtract is one setting beyond Hoboman's (R15 says "same publish settings"); without it R15's no-files-next-to-the-exe requirement cannot hold. Side effect: content is extracted to %TEMP%\.net\HoboXslt.App\<hash> on first start.
- No Assembly.Location use existed in src; nothing to change there.
- baseline: green via handoff (verified at e9f526d4 by .7; only .flow/ changed since)
- Gates: dotnet build HoboXslt.slnx rc=0 (0 warnings, 0 errors); dotnet test tests/HoboXslt.Core.Tests rc=0 (32 passed).

Tier: session (jev-unavailable(no_key))


Decision: IncludeAllContentForSelfExtract added on top of Hoboman's four publish settings; without it IKVM's ikvm.properties and Java image (52 files) must sit next to the exe and Saxon fails to start (reviewer confirmed with a scratch publish).
stage: impl-review - ran (host, one reviewer, SHIP; receipt /tmp/impl-review-receipt-22e37af6a82b-fn-1-xslt-editor-and-debugger-xmlspy-like.8.json) (model: fable)
Integrated verify at d531e52: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests green.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: d531e525b851bda88fe3da4ac412bbaf835c3e17
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests, dotnet publish src/HoboXslt.App -c Release -o publish (via install.ps1), manual UIA run of lone exe: C:\Users\JSL\Documents\hobo-xslt\.flow\tmp\handover\fn-1-xslt-editor-and-debugger-xmlspy-like.8-manual-run.log
- PRs: