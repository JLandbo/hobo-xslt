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
Blocked:
Paused by the user's new requests: README screenshots and texts wait for tasks .10 (orange paused line) and .11 (language files). Partial handover in .flow/tmp/handover; old screenshots saved in the session scratchpad.
## Evidence
- Commits:
- Tests:
- PRs:
