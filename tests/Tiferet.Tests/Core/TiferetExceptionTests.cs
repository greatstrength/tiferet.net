using Tiferet.Domain;
using Tiferet.Events;
using Tiferet.Domain.Error;

namespace Tiferet.Tests.Core;

public class TiferetExceptionTests
{
    [Fact]
    public void Constructor_SetsErrorCode()
    {
        var ex = new TiferetException("TEST_ERROR");
        Assert.Equal("TEST_ERROR", ex.ErrorCode);
    }

    [Fact]
    public void Constructor_SetsContext()
    {
        var ex = new TiferetException("TEST_ERROR", "msg", ("key", "value"));
        Assert.Equal("value", ex.Context["key"]);
    }

    [Fact]
    public void Constructor_DefaultsContextToEmpty()
    {
        var ex = new TiferetException("TEST_ERROR");
        Assert.NotNull(ex.Context);
        Assert.Empty(ex.Context);
    }

    [Fact]
    public void Constructor_AcceptsMultipleContextPairs()
    {
        var ex = new TiferetException("CODE", "msg", ("a", 1), ("b", "two"));
        Assert.Equal(1, ex.Context["a"]);
        Assert.Equal("two", ex.Context["b"]);
    }

    [Fact]
    public void Message_IsHumanReadable()
    {
        var ex = new TiferetException("TEST_ERROR", "something broke");
        Assert.Equal("something broke", ex.Message);
    }

    [Fact]
    public void Message_FallsBackToErrorCode_WhenNoMessageProvided()
    {
        var ex = new TiferetException("TEST_ERROR");
        Assert.Equal("TEST_ERROR", ex.Message);
    }

    [Fact]
    public void IsException()
    {
        var ex = new TiferetException("CODE");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}

public class TiferetApiExceptionTests
{
    [Fact]
    public void Constructor_SetsNameAndMessage()
    {
        var ex = new TiferetApiException("CODE", "ErrorConfiguration Name", "ErrorConfiguration message text");
        Assert.Equal("CODE", ex.ErrorCode);
        Assert.Equal("ErrorConfiguration Name", ex.Name);
        Assert.Equal("ErrorConfiguration message text", ex.Message);
    }

    [Fact]
    public void InheritsFromTiferetException()
    {
        var ex = new TiferetApiException("CODE", "Name", "Msg");
        Assert.IsAssignableFrom<TiferetException>(ex);
    }

    [Fact]
    public void Context_IsPropagated()
    {
        var ex = new TiferetApiException("CODE", "Name", "Msg", ("id", "123"));
        Assert.Equal("123", ex.Context["id"]);
    }
}

public class ErrorCodesTests
{
    [Fact]
    public void Constants_HaveExpectedValues()
    {
        Assert.Equal("COMMAND_PARAMETER_REQUIRED", ErrorCodes.CommandParameterRequired);
        Assert.Equal("FEATURE_NOT_FOUND", ErrorCodes.FeatureNotFound);
        Assert.Equal("INVALID_MODEL_ATTRIBUTE", ErrorCodes.InvalidModelAttribute);
        Assert.Equal("APP_INTERFACE_NOT_FOUND", ErrorCodes.AppInterfaceNotFound);
        Assert.Equal("FILE_NOT_FOUND", ErrorCodes.FileNotFound);
    }
}
