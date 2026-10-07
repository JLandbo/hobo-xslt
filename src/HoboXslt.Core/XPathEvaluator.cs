using javax.xml.transform.stream;
using net.sf.saxon.s9api;
using JavaStringReader = java.io.StringReader;

namespace HoboXslt.Core;

public sealed class XPathEvaluator
{
    private readonly Processor _processor = new(false);

    public XPathResult Evaluate(string expression, string xml)
    {
        try
        {
            var document = _processor.newDocumentBuilder().build(new StreamSource(new JavaStringReader(xml)));
            var compiler = _processor.newXPathCompiler();
            DeclareRootNamespaces(compiler, document);

            var selector = compiler.compile(expression).load();
            selector.setContextItem(document);
            var value = selector.evaluate();

            var items = new List<string>(value.size());
            for (var i = 0; i < value.size(); i++)
                items.Add(value.itemAt(i) is XdmAtomicValue atomic ? atomic.getStringValue() : value.itemAt(i).toString());
            return new(items, null);
        }
        catch (SaxonApiException e)
        {
            return new([], e.getMessage());
        }
    }

    private void DeclareRootNamespaces(XPathCompiler compiler, XdmNode document)
    {
        if (_processor.newXPathCompiler().evaluateSingle("*", document) is not XdmNode root)
            return;

        var namespaces = root.axisIterator(Axis.NAMESPACE);
        while (namespaces.hasNext())
        {
            var ns = (XdmNode)namespaces.next();
            var prefix = ns.getNodeName()?.getLocalName();
            if (!string.IsNullOrEmpty(prefix) && prefix != "xml")
                compiler.declareNamespace(prefix, ns.getStringValue());
        }
    }
}
