---
satisfies: [R1, R2, R4, R11]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.3 WPF shell: editors, open/save, run, diagnostics, output

## Description
The WPF app with three editors and a plain Run; the base the debugger UI and XPath panel plug into.

**Size:** M
**Files:** `src/HoboXslt.App/HoboXslt.App.csproj`, `src/HoboXslt.App/App.xaml(.cs)`, `src/HoboXslt.App/MainWindow.xaml(.cs)`, `HoboXslt.slnx`
**Touches:** [HoboXslt.slnx, src/HoboXslt.App/**]

### Approach
- `dotnet new wpf` net10.0-windows, reference Core and `AvalonEdit` (latest). XML highlighting via AvalonEdit's built-in `XML` definition.
- Panes: XML input editor, XSLT editor, read-only output editor, diagnostics list. Open/save per editor; open failure → message box, content unchanged.
- Run: save dirty documents first; untitled stylesheet → message, no run. Run on a background task; Run disabled while running and until Saxon warm-up (started at launch on a background task) has finished.
- Output: XML highlighting; if output is not well-formed XML, show it as plain text. Failed run clears output.
- Diagnostics: double-click navigates; if the file is not the open stylesheet (included file), open it in the XSLT editor first. Entries without location are not navigable.

### Investigation targets
**Required**:
- Task 1's run engine API and diagnostic shape
- AvalonEdit docs: `TextEditor`, `HighlightingManager`, caret/scroll APIs

### Key context
- Keep logic that is testable in Core; the app has no test project.

### Acceptance
- [ ] App starts, opens/edits/saves XML and XSLT files with highlighting (manual run)
- [ ] Run shows output; a failing stylesheet shows diagnostics and an empty output pane (manual run)
- [ ] Double-click on a diagnostic in an included file opens that file at the line (manual run)
- [ ] UI stays responsive during warm-up and runs (manual run)

## Acceptance
- [ ] TBD

## Done summary
Added the WPF app `src/HoboXslt.App` (net10.0-windows, AvalonEdit 6.3.1.120, references Core). It has XML input, XSLT and a read-only output editor side by side with splitters, a "Fejl og beskeder" diagnostics tab below, a toolbar Run button and a status line. UI text is Danish to match the approved mockup. Open/save is per editor. Run saves dirty documents, refuses untitled ones with a message, runs on a background task, and stays disabled until the Saxon warm-up (`new XsltRunner()` on a background task at Loaded) finishes. Output is XML-highlighted only when well-formed, and a failed run clears it. Double-clicking a diagnostic goes to file:line: the XML input editor when the file is the input, otherwise the XSLT editor, which opens an included file first. Entries without file+line do nothing.

baseline: green via handoff (verified at fd213dbc by fn-1-xslt-editor-and-debugger-xmlspy-like.1; only .flow/ changed since)
Gates (measured): `dotnet build HoboXslt.slnx` rc=0 (0 warnings on an incremental build; a clean build of the App shows IKVM0100/IKVM0117 warnings from the transitive MavenReference, the same family as Core's); `dotnet test tests/HoboXslt.Core.Tests` rc=0, 7/7 passed.
Manual ACs driven through UI Automation (measured, scratchpad smoke/drive.ps1):
- warm-up: window up at ~1.5s, Run enabled ~1s later, 0 unresponsive samples
- main.xsl + include: output highlighted, 2 xsl:message entries at inc.xsl:5; double-click opened inc.xsl in the XSLT editor with the editor focused
- deleting inc.xsl before double-click: error message box, XSLT editor unchanged (main.xsl)
- bad.xsl: status "Kørsel fejlede", empty output, compile error at bad.xsl:4
- text output: shown as plain text
- untitled Run: message, no run
- dirty XSLT: saved before the run, edit written to disk
- 6s run: Run disabled, 0/55 unresponsive samples
- closing the window during a run: the process exited
Not checked by eye: the caret line after navigation (only the focus was confirmed) and the highlight colours (screenshots looked right).

Decision / ask the user: opening a file (Open, or diagnostic navigation into an included file) replaces a dirty editor without a save prompt. The spec does not ask for a prompt, so I did not add one. Follow-up question for the user.


Tier: session (jev-unavailable(no_key))
Review fix (cc1e1c0): XSLT pane tabs (first tab = run file; included files open in extra tabs via ShowXsltLine) and save-before-replace on Åbn… for modified XSLT/XML, per user decisions recorded in the spec. Declined P3 follow-up: IsWellFormedXml lives in App (not unit-tested), DTD-entity edge.
stage: impl-review - ran (host; round 1 NEEDS_WORK 3 draws, round 2 SHIP; receipt /tmp/impl-review-receipt-22e37af6a82b-fn-1-xslt-editor-and-debugger-xmlspy-like.3.json) (model: fable)
Integrated verify at cc1e1c0: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests 19/19.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: 018ee84049e06154c1b21f2f5316144b93b959ce, cc1e1c0bcaf994e9e7f7b8a65a54ad428e00dfbe
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: