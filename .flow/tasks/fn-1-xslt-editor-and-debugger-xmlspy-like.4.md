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
TBD

## Evidence
- Commits:
- Tests:
- PRs:
