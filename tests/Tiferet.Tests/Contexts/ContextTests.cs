using Tiferet.Domain;
using Tiferet.Events;
using Tiferet.Contexts;
using Tiferet.Tests.Events; // AddNumbersEvent, AddNumbersParams, etc.

namespace Tiferet.Tests.Contexts;

// *** CacheContext Tests

public class CacheContextTests
{
    [Fact]
    public void Set_And_Get_ReturnsValue()
    {
        var cache = new CacheContext();
        cache.Set("key", "value");
        Assert.Equal("value", cache.Get("key"));
    }

    [Fact]
    public void Get_Missing_ReturnsNull()
    {
        var cache = new CacheContext();
        Assert.Null(cache.Get("missing"));
    }

    [Fact]
    public void GetTyped_ReturnsTypedValue()
    {
        var cache = new CacheContext();
        cache.Set("num", 42);
        Assert.Equal(42, cache.Get<int>("num"));
    }

    [Fact]
    public void GetTyped_WrongType_ReturnsDefault()
    {
        var cache = new CacheContext();
        cache.Set("str", "hello");
        Assert.Equal(0, cache.Get<int>("str"));
    }

    [Fact]
    public void Delete_RemovesEntry()
    {
        var cache = new CacheContext();
        cache.Set("key", "value");
        cache.Delete("key");
        Assert.Null(cache.Get("key"));
    }

    [Fact]
    public void Delete_NonExistent_DoesNotThrow()
    {
        var cache = new CacheContext();
        cache.Delete("nonexistent");
    }

    [Fact]
    public void Clear_RemovesAll()
    {
        var cache = new CacheContext();
        cache.Set("a", 1);
        cache.Set("b", 2);
        cache.Clear();
        Assert.Null(cache.Get("a"));
        Assert.Null(cache.Get("b"));
    }

    [Fact]
    public void Constructor_WithSeed()
    {
        var seed = new Dictionary<string, object?> { ["seeded"] = "value" };
        var cache = new CacheContext(seed);
        Assert.Equal("value", cache.Get("seeded"));
    }
}

// *** RequestContext Tests

public class RequestContextTests
{
    [Fact]
    public void Constructor_GeneratesSessionId()
    {
        var request = new RequestContext();
        Assert.NotNull(request.SessionId);
        Assert.NotEmpty(request.SessionId);
    }

    [Fact]
    public void Constructor_UsesProvidedSessionId()
    {
        var request = new RequestContext(sessionId: "custom-session");
        Assert.Equal("custom-session", request.SessionId);
    }

    [Fact]
    public void Constructor_SetsFeatureId()
    {
        var request = new RequestContext(featureId: "calc.add");
        Assert.Equal("calc.add", request.FeatureId);
    }

    [Fact]
    public void SetResult_WithoutDataKey_SetsResult()
    {
        var request = new RequestContext();
        request.SetResult(42);
        Assert.Equal(42, request.Result);
    }

    [Fact]
    public void SetResult_WithDataKey_StoresInData()
    {
        var request = new RequestContext();
        request.SetResult("computed", "output_key");
        Assert.Equal("computed", request.Data["output_key"]);
        Assert.Null(request.Result); // Result not set.
    }

    [Fact]
    public void HandleResponse_ReturnsResult()
    {
        var request = new RequestContext();
        request.SetResult("final");
        Assert.Equal("final", request.HandleResponse());
    }

    [Fact]
    public void GenericRequestContext_TypedResult()
    {
        var request = new RequestContext<int>();
        request.SetResult(42);
        Assert.Equal(42, request.Result);
        Assert.Equal(42, request.HandleResponse());
    }

    [Fact]
    public void Data_AccumulatesAcrossSteps()
    {
        var request = new RequestContext(
            data: new Dictionary<string, object?> { ["a"] = 1 });
        request.SetResult(2, "b");
        request.SetResult(3, "c");
        Assert.Equal(1, request.Data["a"]);
        Assert.Equal(2, request.Data["b"]);
        Assert.Equal(3, request.Data["c"]);
    }
}

// *** DomainEvent Dictionary Execute Tests

public class DomainEventDictionaryExecuteTests
{
    [Fact]
    public void Execute_FromDictionary_ConstructsParamsAndDelegates()
    {
        var evt = new AddNumbersEvent();
        var data = new Dictionary<string, object?>
        {
            ["A"] = 3,
            ["B"] = 4,
        };
        var result = evt.Execute(data);
        Assert.Equal(7, result);
    }

    [Fact]
    public void Execute_FromDictionary_CaseInsensitiveKeys()
    {
        var evt = new AddNumbersEvent();
        var data = new Dictionary<string, object?>
        {
            ["a"] = 10,
            ["b"] = 20,
        };
        var result = evt.Execute(data);
        Assert.Equal(30, result);
    }

    [Fact]
    public void Execute_FromDictionary_WithStringConversion()
    {
        var evt = new GreetEvent("Hi");
        var data = new Dictionary<string, object?>
        {
            ["Name"] = "World",
        };
        var result = evt.Execute(data);
        Assert.Equal("Hi World", result);
    }
}

// *** FeatureContext Static Method Tests

public class FeatureContextParseParameterTests
{
    [Fact]
    public void ParseRequestParameter_LiteralValue()
    {
        var result = FeatureContext.ParseRequestParameter("hello");
        Assert.Equal("hello", result);
    }

    [Fact]
    public void ParseRequestParameter_EnvVar()
    {
        Environment.SetEnvironmentVariable("TIFERET_CTX_TEST", "resolved_ctx");
        try
        {
            var result = FeatureContext.ParseRequestParameter("$env.TIFERET_CTX_TEST");
            Assert.Equal("resolved_ctx", result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TIFERET_CTX_TEST", null);
        }
    }

    [Fact]
    public void ParseRequestParameter_RequestBacked()
    {
        var request = new RequestContext(
            data: new Dictionary<string, object?> { ["myKey"] = "myValue" });
        var result = FeatureContext.ParseRequestParameter("$r.myKey", request);
        Assert.Equal("myValue", result);
    }

    [Fact]
    public void ParseRequestParameter_RequestBacked_MissingKey_Throws()
    {
        var request = new RequestContext();
        var ex = Assert.Throws<TiferetException>(
            () => FeatureContext.ParseRequestParameter("$r.missing", request));
        Assert.Equal(ErrorCodes.ParameterNotFound, ex.ErrorCode);
    }

    [Fact]
    public void ParseRequestParameter_RequestBacked_NoRequest_Throws()
    {
        var ex = Assert.Throws<TiferetException>(
            () => FeatureContext.ParseRequestParameter("$r.key"));
        Assert.Equal(ErrorCodes.RequestNotFound, ex.ErrorCode);
    }
}

public class FeatureContextEvaluateConditionTests
{
    [Fact]
    public void EvaluateCondition_NullCondition_ReturnsTrue()
    {
        var request = new RequestContext();
        Assert.True(FeatureContext.EvaluateCondition(null, request));
    }

    [Fact]
    public void EvaluateCondition_EmptyCondition_ReturnsTrue()
    {
        var request = new RequestContext();
        Assert.True(FeatureContext.EvaluateCondition("", request));
    }

    [Fact]
    public void EvaluateCondition_NotEqualNull_WithValue_ReturnsTrue()
    {
        var request = new RequestContext(
            data: new Dictionary<string, object?> { ["x"] = "present" });
        Assert.True(FeatureContext.EvaluateCondition("$r.x != null", request));
    }

    [Fact]
    public void EvaluateCondition_NotEqualNull_WithNull_ReturnsFalse()
    {
        var request = new RequestContext();
        Assert.False(FeatureContext.EvaluateCondition("$r.x != null", request));
    }

    [Fact]
    public void EvaluateCondition_EqualNull_WithNull_ReturnsTrue()
    {
        var request = new RequestContext();
        Assert.True(FeatureContext.EvaluateCondition("$r.x == null", request));
    }

    [Fact]
    public void EvaluateCondition_Truthy_WithValue_ReturnsTrue()
    {
        var request = new RequestContext(
            data: new Dictionary<string, object?> { ["flag"] = "yes" });
        Assert.True(FeatureContext.EvaluateCondition("$r.flag", request));
    }

    [Fact]
    public void EvaluateCondition_Truthy_WithFalse_ReturnsFalse()
    {
        var request = new RequestContext(
            data: new Dictionary<string, object?> { ["flag"] = "False" });
        Assert.False(FeatureContext.EvaluateCondition("$r.flag", request));
    }
}
