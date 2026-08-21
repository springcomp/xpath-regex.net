using XPath.Regex.NET.Errors;

namespace XPath.Regex.NET.UnitTests;

public sealed class ErrorTypeTests
{
    [Fact]
    public void Forx0001_HasCorrectErrorCode()
    {
        var ex = new Forx0001Exception("bad flag", 'z');
        Assert.Equal("FORX0001", ex.ErrorCode);
        Assert.Equal('z', ex.InvalidFlag);
    }

    [Fact]
    public void Forx0001_NullFlag_WholesaleRejection()
    {
        var ex = new Forx0001Exception("no flags in XSD");
        Assert.Equal("FORX0001", ex.ErrorCode);
        Assert.Null(ex.InvalidFlag);
    }

    [Fact]
    public void Forx0001_DefaultAndInnerConstructors()
    {
        var defaultException = new Forx0001Exception();
        var inner = new InvalidOperationException("inner");
        var withInner = new Forx0001Exception("bad flag", inner);

        Assert.Equal("Invalid flags.", defaultException.Message);
        Assert.Same(inner, withInner.InnerException);
    }

    [Fact]
    public void Forx0002_HasCorrectErrorCode_NoOffset()
    {
        var ex = new Forx0002Exception("bad pattern");
        Assert.Equal("FORX0002", ex.ErrorCode);
        Assert.Equal(-1, ex.PatternOffset);
    }

    [Fact]
    public void Forx0002_HasCorrectErrorCode_WithOffset()
    {
        var ex = new Forx0002Exception("bad pattern", 5);
        Assert.Equal("FORX0002", ex.ErrorCode);
        Assert.Equal(5, ex.PatternOffset);
    }

    [Fact]
    public void Forx0002_DefaultAndInnerConstructors()
    {
        var defaultException = new Forx0002Exception();
        var inner = new InvalidOperationException("inner");
        var withInner = new Forx0002Exception("bad pattern", inner);

        Assert.Equal("Invalid pattern.", defaultException.Message);
        Assert.Same(inner, withInner.InnerException);
    }

    [Fact]
    public void Forx0003_HasCorrectErrorCode()
    {
        var ex = new Forx0003Exception("zero-length match");
        Assert.Equal("FORX0003", ex.ErrorCode);
    }

    [Fact]
    public void Forx0003_DefaultAndInnerConstructors()
    {
        var defaultException = new Forx0003Exception();
        var inner = new InvalidOperationException("inner");
        var withInner = new Forx0003Exception("zero-length match", inner);

        Assert.Equal("Pattern can match the empty string.", defaultException.Message);
        Assert.Same(inner, withInner.InnerException);
    }

    [Fact]
    public void Forx0004_HasCorrectErrorCode_AndOffset()
    {
        var ex = new Forx0004Exception("bad replacement", 3);
        Assert.Equal("FORX0004", ex.ErrorCode);
        Assert.Equal(3, ex.ReplacementOffset);
    }

    [Fact]
    public void Forx0004_DefaultMessageAndInnerConstructors()
    {
        var defaultException = new Forx0004Exception();
        var messageException = new Forx0004Exception("custom replacement");
        var inner = new InvalidOperationException("inner");
        var withInner = new Forx0004Exception("bad replacement", inner);

        Assert.Equal("Invalid replacement string.", defaultException.Message);
        Assert.Equal("custom replacement", messageException.Message);
        Assert.Same(inner, withInner.InnerException);
    }

    [Fact]
    public void AllExceptions_AreForxException()
    {
        Assert.IsAssignableFrom<ForxException>(new Forx0001Exception("x"));
        Assert.IsAssignableFrom<ForxException>(new Forx0002Exception("x"));
        Assert.IsAssignableFrom<ForxException>(new Forx0003Exception("x"));
        Assert.IsAssignableFrom<ForxException>(new Forx0004Exception("x", 0));
    }

    [Theory]
    [InlineData(RegexEngineLimit.TimeoutMs)]
    [InlineData(RegexEngineLimit.MaxThreadCount)]
    [InlineData(RegexEngineLimit.MaxStackDepth)]
    public void RegexEngineLimitExceeded_IsNotForxException(RegexEngineLimit limit)
    {
        var ex = new RegexEngineLimitExceededException(limit, "runtime limit");

        Assert.Equal(limit, ex.Limit);
        Exception baseException = ex;
        Assert.False(baseException is ForxException);
    }
}
