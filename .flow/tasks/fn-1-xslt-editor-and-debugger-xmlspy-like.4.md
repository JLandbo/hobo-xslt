---
satisfies: [R9]
---
# fn-1-xslt-editor-and-debugger-xmlspy-like.4 XPath evaluator

## Description
Evaluate an XPath expression against the current input XML and show the result.

**Size:** S
**Files:** `src/HoboXslt.Core/XPathEvaluator.cs`, `tests/HoboXslt.Core.Tests/XPathEvaluatorTests.cs`, `src/HoboXslt.App/MainWindow.xaml(.cs)`
**Touches:** [src/HoboXslt.Core/**, tests/HoboXslt.Core.Tests/**, src/HoboXslt.App/**]

### Approach
- Core: XPath 3.1 via Saxon `XPathCompiler`; context item = the input document node; namespace prefixes declared on the input root element are bound. Result rendered one item per line (nodes serialized, atomic values as strings).
- App: an expression box and result area; evaluates the XML editor's current text (unsaved edits included).
- Invalid XPath or unparsable/missing input → error text in the result area.

### Acceptance
- [ ] Test: expression selecting nodes → serialized nodes
- [ ] Test: atomic result (`count(//x)`) → value
- [ ] Test: prefixed expression using a prefix declared on the input root works
- [ ] Test: invalid XPath → error result, no exception
- [ ] Manual run: evaluator panel shows results and errors

## Acceptance
- [ ] TBD

## Done summary
Added `XPathEvaluator` in Core (XPath 3.1 via Saxon s9api, context item = input document node, prefixes declared on the input root element bound, result as one string per item: nodes serialized, atomic values as string values; Saxon parse/compile/dynamic errors returned as error text, no exception) and an XPath bar under the toolbar in the App (expression box, "Evaluér" button or Enter, read-only result box to its right; errors shown in red, an empty sequence shows "Tom sekvens"). The bar evaluates the XML editor's current text, so unsaved edits count, and is enabled once the Saxon warm-up finishes.

baseline: green via handoff (verified at 47c039b6 by fn-1-xslt-editor-and-debugger-xmlspy-like.2; only .flow/ changed since)
Gates (measured): `dotnet build HoboXslt.slnx` rc=0, 0 warnings; `dotnet test tests/HoboXslt.Core.Tests` rc=0, 30/30 passed (6 new in XPathEvaluatorTests: nodes, atomic count, root prefix, and a theory for invalid XPath / empty input / malformed input).
Manual run (measured, UI Automation script in the session scratchpad xpath-smoke/drive.ps1, screenshot xpath-bar.png): no input -> "SXXP0003 ... Premature end of file"; orders.xml `//customer` -> two serialized nodes, one per line; `count(//order)` -> 2; `//nothing` -> "Tom sekvens"; `//[` -> Saxon syntax error text; ns.xml `string-join(//o:order/@id, ",")` -> 1,2; unsaved comment typed into the XML editor counted by `count(//comment())` -> 1; unsaved malformed edit -> parse error; Enter in the box evaluates (`1 + 1` -> 2). The app stayed up through every error case.
Integration warning: MainWindow.xaml row 0 now wraps the ToolBar in a StackPanel with the XPath bar; task 5's toolbar edits will conflict textually there (note in NOTES_DIR fn-1.4-xpath-bar.md).
Not added: a result count ("1 resultat · mod orders.xml" in the mockup) and a default-namespace binding for unprefixed names; neither is in the task ACs.

Tier: session (jev-unavailable(no_key))

stage: impl-review - ran (host, one reviewer, SHIP; receipt /tmp/impl-review-receipt-22e37af6a82b-fn-1-xslt-editor-and-debugger-xmlspy-like.4.json) (model: fable)
Follow-up (P3): root element found via a second XPath; a default namespace on the input root is not bound (XPath semantics).
Integrated verify at f7d9cfc: dotnet build HoboXslt.slnx rc=0; dotnet test tests/HoboXslt.Core.Tests green.

stage: plan-sync - skipped(config: planSync.enabled != true)
## Evidence
- Commits: f7d9cfc4d75b2c434b40eb77333ac0cdf5a20d1b
- Tests: dotnet build HoboXslt.slnx, dotnet test tests/HoboXslt.Core.Tests
- PRs: