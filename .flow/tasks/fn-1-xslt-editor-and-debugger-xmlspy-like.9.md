---
satisfies: [R17]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.9 Detailed Danish README with screenshots

## Description
Write `README.md` in Danish with real screenshots of the running app, in the style of the user's Hoboman README.

**Size:** M
**Files:** `README.md`, `docs/images/*.png`
**Touches:** [README.md, docs/images/**]

### Approach
- Reference structure: `C:\Users\JSL\Documents\Hoboman\README.md` (centered header, 'Det får du' bullet list with bold lead-ins, 'Kom hurtigt i gang' with Krav / Kør fra kildekoden / Installer i Start-menuen, feature sections, 'Projektet' with a box-drawing folder tree and Build og test, screenshots in `docs/images/` with descriptive Danish alt text).
- Sections for this app: what it does; requirements (.NET 10 SDK, Windows); install via `install.ps1` and run from source; running a transformation and the output pane; error list and navigation into included files (tabs); debugging (breakpoints, unbound breakpoints, step/continue/stop with keys, variables panel with Lokal/Global); XPath evaluator (incl. that a default namespace needs a prefix); save rules; keyboard shortcuts; project layout; build and test; known limits (Saxon-HE: no streaming/schema; a loop inside one XPath expression cannot be stopped).
- Screenshots: take them of the real app built from this branch, driven via UI Automation against the app's own window handle only (never send input to other windows). Use a small realistic sample (orders XML + an XSLT that includes another file). At least: main window after a run, debugger paused in an included file with variables, error list with a diagnostic, XPath evaluator result.
- Write facts only from the code and spec; do not invent features.

### Acceptance
- [ ] README.md covers every section above with accurate commands and keys
- [ ] At least four screenshots in docs/images/ referenced with alt text, each showing the current app
- [ ] Every command in the README was run once and works

## Acceptance
- [ ] TBD

## Done summary
README.md written in Danish (Hoboman structure) with four new screenshots in docs/images (koersel, fejl, debug, xpath), taken from the published single-file exe of this branch. Covers run and output, save rules, error list and navigation into included files, debugging (breakpoints, unbound ring, keys, orange paused line, Variabler with Lokal/Global), XPath incl. default namespace, Vis -> Sprog, keys, limits, install.ps1, first-start extraction to %TEMP%, project layout and build/test.

Measured (UI Automation on the app's own window, driver scratchpad\t9r\drive.ps1, logs main.log / fejl.log there; exe = publish\HoboXslt.App.exe built with install.ps1's publish command):
- Run: 'Kørsel fuldført', fakturaer output, two xsl:message rows at ordrer.xsl:15.
- Debug: breakpoint ordrer.xsl:18 -> 'Pauset ved ordrer.xsl:18'; Step into -> moms.xsl in a second tab, line 5; breakpoint moms.xsl:8 -> $beloeb 738 / $moms 184.5 Lokal, $moms-sats 0.25 / $valuta DKK Global, orange line visible in debug.png; Continue -> 397.5 / 99.375; Stop -> 'Debugsession stoppet'.
- Error list: 'moms.xsl:7 | Kompileringsfejl | Variable $rabat has not been declared ...'; double-click opened the moms.xsl tab at Ln 7.
- XPath: sum(...) -> 1135.5; invalid -> 'Unexpected token "<eof>" ...'; default namespace: //ordre -> 'Tom sekvens', //*:ordre/@kunde -> 2 results, count(//Q{urn:eksempel:ordrer}ordre) -> 2.
- First start of the new exe created %TEMP%\.net\HoboXslt.App\G17MHd5rbzZc (540 files, ~247 MB); older build folders stay beside it. Exe 257,643,468 bytes plus three pdbs.
- dotnet run --project .\src\HoboXslt.App: window 'hobo-xslt' opened, closed cleanly, rc 0.
- settings.json is {"languageName": "Dansk"} (unchanged).

From code / earlier tasks (inferred, not re-measured here): save rules, Stop only in debug, keys, Vis -> Sprog behaviour (task .11 measured it), install.ps1 wait/shortcut/failure message (task .8 measured it).
Not run: .\install.ps1 (forbidden by HOST_NOTE: it would put a shortcut to the temporary worktree in the Start menu).

baseline: green via handoff (verified at d720b0e5 by .11; only .flow/ changed since)
Gates: flowctl gate classify -> TIER_B docs-only. dotnet build HoboXslt.slnx rc=0 (0 warnings); dotnet test tests/HoboXslt.Core.Tests rc=0 (36 passed) - run as README command checks.



Tier: session (jev-unavailable(no_key))
stage: impl-review - skipped(policy: risk - docs-only change (README + screenshots); covered by the spec completion review)
Conductor check: README self-contained claim verified (msbuild publish properties: SelfContained=true, RuntimeIdentifier=win-x64).

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: cfc4311ec23415eda3ee42e6f80f5b6fbfed8874
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests, dotnet run --project .\src\HoboXslt.App, dotnet publish .\src\HoboXslt.App -c Release -o .\publish
- PRs: