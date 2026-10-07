---
satisfies: [R2, R22, R23, R24]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.15 Live messages and live output during Run and Debug

## Description
Stream diagnostics/messages and output from the worker process to the UI while the run is going, so nothing is lost on Stop and the user sees progress. Output written before Stop or failure stays.

**Size:** M
**Files:** `src/HoboXslt.Core/Worker.cs`, `src/HoboXslt.Core/WorkerSession.cs`, `src/HoboXslt.Core/XsltRunner.cs`, `src/HoboXslt.Core/DebugSession.cs`, `src/HoboXslt.App/MainWindow.xaml.cs`, language files, `tests/HoboXslt.Core.Tests/**`, `README.md`
**Touches:** [src/HoboXslt.Core/**, src/HoboXslt.App/**, tests/HoboXslt.Core.Tests/**, README.md]

### Approach
- Keep it simple (the user explicitly does not want overengineering): extend the existing line protocol with two optional fields on `WorkerMessage` — one diagnostic, and one output text chunk — written as they happen. No new transport, no extra threads beyond what is needed.
- Diagnostics: write each one from the existing message/error collection points as it is added.
- Output: give Saxon a serializer destination backed by a TextWriter that forwards text chunks to the protocol (small batching is fine, e.g. flush per Write call or per ~100 ms, whichever is simpler and keeps the UI smooth). The final result no longer needs to carry the whole output if the chunks already did — but keep R11's final well-formedness decision (on the accumulated text, off the UI thread).
- UI: append chunks to the Output editor and diagnostics to the list as they arrive (marshal to the UI thread; keep appends cheap). Clear both at the start of a run.
- Stop/failure: keep what was shown; status texts say stopped/failed with incomplete output (new texts in Dansk.json and English.json).
- The in-process `DebugSession` message-on-stop behaviour must now hold for the worker path too.
- Update README: messages and output appear while running; Stop/failure keep what was written.

### Acceptance
- [ ] Test: worker Run of a stylesheet that emits xsl:message then loops forever — the message arrives before Stop; after Stop the result is Stopped and the message is kept
- [ ] Test: output written before an infinite loop arrives as chunks and is kept after Stop
- [ ] Test: Debug paused at a breakpoint after some output — that output has already arrived
- [ ] Manual run (published single exe): Kør on a slow stylesheet shows messages and output growing; Stop keeps them and the status says stopped
- [ ] README updated
- [ ] `dotnet build HoboXslt.slnx` and `dotnet test tests/HoboXslt.Core.Tests` green

## Acceptance
- [ ] TBD

## Done summary
TBD

## Evidence
- Commits:
- Tests:
- PRs:
