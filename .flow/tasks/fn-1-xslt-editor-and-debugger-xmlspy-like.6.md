---
satisfies: [R13]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.6 Apply the approved mockup design to the main window

## Description
Restyle the existing WPF main window so it looks like the approved mockup. No new behaviour beyond R13's menus, pause badge and status bar fields; every existing feature (tabs, save rule, run, debug, XPath, breakpoints, variables) keeps working.

**Size:** M
**Files:** `src/HoboXslt.App/MainWindow.xaml`, `src/HoboXslt.App/MainWindow.xaml.cs`, `src/HoboXslt.App/App.xaml` (shared styles/brushes), possibly `src/HoboXslt.App/BreakpointMargin.cs` (marker colors)
**Touches:** [src/HoboXslt.App/**]

### Approach
- Open `docs/design/main-window-mockup.html` and read its `:root` light tokens (colors, syntax colors) and CSS; define them once as WPF resources (brushes) in App.xaml and use them everywhere — no hard-coded colors in controls.
- Layout per mockup: title row (brand mark, Menu with Filer/Rediger/Vis/Kør/Debug/Hjælp holding the existing commands only, run file path right-aligned), toolbar (icon buttons drawn with Path geometry; labeled Kør/Debug; Fortsæt, Step into/over/out, Stop; pause badge right-aligned while paused), XPath bar, three card panes with GridSplitters, GridSplitter above the bottom panel, bottom TabControl (Variabler, Fejl og beskeder), status bar.
- Pane headers: type badge (XML / XSLT / XML), file name, meta text (e.g. line count, breakpoint count, last run time).
- Status bar: Saxon-HE 12 · XSLT 3.0, session state, Ln/Col of the focused editor, encoding, line endings.
- AvalonEdit syntax colors: adjust the XML highlighting definition's colors to the mockup's tag/attribute/value/comment colors.
- Fonts: Segoe UI for UI, Cascadia Mono for code.

### Investigation targets
**Required**:
- `docs/design/main-window-mockup.html` — the design to match
- `src/HoboXslt.App/MainWindow.xaml` — current layout to restyle
- Run notes in the flow-notes dir (fn-1.3, fn-1.4, fn-1.5) for integration details

### Key context
- Light theme only (user decision).
- Do not change Core.

### Acceptance
- [ ] Side-by-side screenshot of the running app (paused debug session with an included file tab open) next to the mockup shows the same structure, colors and typography (manual run, screenshot saved)
- [ ] All six menus present; Filer/Kør/Debug hold the existing commands and they work (manual run)
- [ ] Splitters drag pane widths and the bottom panel height (manual run)
- [ ] Status bar shows engine, session state, Ln/Col, encoding and line endings (manual run)
- [ ] Existing behaviour unchanged: run, diagnostics navigation into tabs, save rule, XPath, breakpoints, stepping, variables (manual run)
- [ ] `dotnet build HoboXslt.slnx` and `dotnet test tests/HoboXslt.Core.Tests` green

## Acceptance
- [ ] TBD

## Done summary
The main window is restyled after `docs/design/main-window-mockup.html`, light theme only. App.xaml now defines the mockup's light tokens once (brushes and fonts) together with shared styles: tool buttons, tabs, cards, splitters, tables, thin scrollbars and the top-level menu. MainWindow.xaml provides:
- a title row with the brand mark, six menus and the run file path;
- a toolbar with Path icons, the labeled Kør and Debug buttons, the step buttons and a pause badge;
- the XPath bar;
- three card panes, each header carrying a type badge, a file name and meta text;
- a bottom card with Variabler and Fejl og beskeder and their count badges;
- a status bar.

The XML highlighting colors come from the mockup; tag brackets are faint. The UI font is Segoe UI and the code font is Cascadia Mono. Core is unchanged.

Conductor additions (user-approved):
1. Both "Åbn…" buttons are disabled while a debug session runs and enabled again when it ends. The Filer menu items "Åbn XML…" and "Åbn XSLT…" are bound to those buttons, so they follow.
2. The breakpoint margin ignores clicks during a session: `IsEnabled` is false on every XSLT tab margin, including tabs opened mid-session.

baseline: green (measured pre-edit at a88a7d7: build rc=0, 30/30 tests). The handoff was not reused, because docs/design changed after its verified SHA.
Gates (measured, after the final edit): `dotnet build HoboXslt.slnx` rc=0 with 0 warnings; `dotnet test tests/HoboXslt.Core.Tests` rc=0 with 30 passed and 0 failed. `gate classify` returned FULL.

Manual ACs (measured). The app was driven through UI Automation against its own window handle only; every mouse and key action checked the foreground handle first. Script and log: scratchpad `smoke-t6/drive.ps1` and `drive.log`. Screenshots: `pause-inc.png`, `idle.png`, `after-splitters.png`, `menu-open.png`, and `side-by-side.png`, which puts the mockup next to the app paused at inc.xsl:6 with the included tab open.

| Area | What the run showed |
|---|---|
| Menus | Filer = Åbn XML…, Gem XML, Åbn XSLT…, Gem XSLT. Kør = Kør, Evaluér XPath. Debug = Start debugging, Fortsæt, Step into, Step over, Step out, Stop. Rediger, Vis and Hjælp are empty. Kør > Kør ran to "Kørsel fuldført". Debug > Stop ended the session. Item enabled states follow the buttons. |
| Splitters | Dragging the first column splitter widened the XML editor from 409 to 489 px. Dragging the row splitter shrank the workspace from 403 to 343 px high. |
| Status bar | Shows "Saxon-HE 12 · XSLT 3.0" and the session state ("Debugsession aktiv · editorer er skrivebeskyttede" while paused). Ln/Col follows the focused editor ("Ln 3, Col 14" after a click in the XML editor). Encoding is UTF-8. Line endings are LF for the LF file and CRLF for the CRLF stylesheets. |
| Run and diagnostics | Run gave output plus two `inc.xsl:5` xsl:message rows. Double-clicking a row opened inc.xsl in a new tab with the caret on Ln 5. |
| Breakpoints and stepping | Margin clicks set main.xsl:7 and inc.xsl:6; the meta changed from "0 breakpoints" to "1 breakpoint". F5 paused at main.xsl:7. F5 again paused at inc.xsl:6 with `$id=1001`, and the badge read "Pauset ved inc.xsl:6". Step into, Step over and Step out each paused. |
| Session locks | While paused, a margin click on line 4 left the count at 1, and both Open buttons and their menu items were off. They were back on after Stop. |
| XPath | `//order/@id` returned `id="1001" / id="1002"`. `//(` showed the error text in the error color. |
| Closing | Closing the window: the process exited. |

Decisions:
- The OS title bar stays. The mockup's title row is drawn as the first row inside the window; a custom window chrome would mean drawing our own caption buttons.
- The mockup's toolbar "Åbn" and "Gem alle" buttons are not added, because the app has no such commands. Open and save stay as icon buttons in each pane header and as Filer menu items.
- The XSLT tab strip sits in the pane header next to the XSLT badge. The meta text and the buttons are laid over the header as normal elements, because content in TabControl.Tag is invisible to UI Automation. The TabControl templates name `PART_SelectedContentHost`, so tab content stays visible to UI Automation.
- Run now selects the "Fejl og beskeder" tab. Variabler comes first as in R13, and without this a run's errors would be hidden behind the Variabler tab.
- The pause text moved from the status bar into the badge. The status bar shows the session state.
- `EditorDocument` lost its label parameter, which became unused once the header shows "Input" itself.
- Hyperlink rendering in the editors is off, because the mockup shows namespace URIs as attribute values.

Known differences from the mockup:
- Code line height: AvalonEdit has no line-spacing option, so rows are about 14.5 px instead of the mockup's 1.7 line height.
- The mockup's web fonts (IBM Plex Sans, JetBrains Mono) are replaced by Segoe UI and Cascadia Mono, as R13 specifies.
- Variables show only Navn and Værdi. The engine has no type or scope data.

Feature-map route changes: the XML and XSLT open/save actions are now icon buttons in the pane headers (UIA names "Åbn…" and "Gem") and are also in the Filer menu. The toolbar step buttons are icon-only, with UIA names Fortsæt, Step into, Step over, Step out and Stop.

GATE lines: none (full tier).

Tier: session (jev-unavailable(no_key))


stage: impl-review - ran (host, 3 draws, SHIP; receipt /tmp/impl-review-receipt-22e37af6a82b-fn-1-xslt-editor-and-debugger-xmlspy-like.6.json) (model: fable)
Follow-ups from review: (P2) XSLT tab strip overlaid by meta/buttons — with 4-5 included-file tabs later tabs cannot be clicked; (P3) breakpoint count in header not refreshed when text deletion removes a breakpoint; (P3) status dot stays green when Saxon fails to start.
Integrated verify at 503d6f4: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests green.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: 503d6f47da83d5275ecf6d5c4505ad0f9f4afa05
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: