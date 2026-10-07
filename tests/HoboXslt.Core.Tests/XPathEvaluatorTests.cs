namespace HoboXslt.Core.Tests;

public sealed class XPathEvaluatorTests
{
    private readonly XPathEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_WhenExpressionSelectsNodes_ThenReturnsSerializedNodes()
    {
        // Arrange
        const string xml = "<root><x>1</x><x>2</x></root>";

        // Act
        var result = _evaluator.Evaluate("//x", xml);

        // Assert
        Assert.Equal(["<x>1</x>", "<x>2</x>"], result.Items);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Evaluate_WhenResultIsAtomic_ThenReturnsValue()
    {
        // Arrange
        const string xml = "<root><x/><x/></root>";

        // Act
        var result = _evaluator.Evaluate("count(//x)", xml);

        // Assert
        Assert.Equal(["2"], result.Items);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Evaluate_WhenPrefixIsDeclaredOnInputRoot_ThenPrefixResolves()
    {
        // Arrange
        const string xml = """<r:root xmlns:r="urn:r"><r:x>a</r:x></r:root>""";

        // Act
        var result = _evaluator.Evaluate("string(//r:x)", xml);

        // Assert
        Assert.Equal(["a"], result.Items);
        Assert.Null(result.Error);
    }

    [Theory]
    [InlineData("//[", "<root/>")]
    [InlineData("count(*)", "")]
    [InlineData("count(*)", "<root>")]
    public void Evaluate_WhenExpressionOrInputIsInvalid_ThenReturnsError(string expression, string xml)
    {
        // Act
        var result = _evaluator.Evaluate(expression, xml);

        // Assert
        Assert.Empty(result.Items);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }
}
