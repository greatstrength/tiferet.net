using Tiferet.Domain;
using Tiferet.Mappers;
using Tiferet.Mappers.Error;
using Tiferet.Domain.Error;

namespace Tiferet.Tests.Domain;

public class ErrorRecordTests
{
    [Fact]
    public void Create_DerivesErrorCodeFromId()
    {
        var e = ErrorConfiguration.Create(id: "invalid_input", name: "Invalid Input");
        Assert.Equal("INVALID_INPUT", e.ErrorCode);
    }

    [Fact]
    public void Create_ExplicitErrorCode()
    {
        var e = ErrorConfiguration.Create(id: "test", name: "Test", errorCode: "CUSTOM_CODE");
        Assert.Equal("CUSTOM_CODE", e.ErrorCode);
    }

    [Fact]
    public void FormatMessage_ReturnsFormattedText()
    {
        var e = ErrorConfiguration.Create(id: "err", name: "Err", messages: [
            new ErrorMessageConfiguration("en_US", "Value {value} must be a number")
        ]);
        var msg = e.FormatMessage("en_US", new() { ["value"] = "abc" });
        Assert.Equal("Value abc must be a number", msg);
    }

    [Fact]
    public void FormatMessage_ReturnsNull_NoMatchingLang()
    {
        var e = ErrorConfiguration.Create(id: "err", name: "Err", messages: [
            new ErrorMessageConfiguration("en_US", "ErrorConfiguration")
        ]);
        Assert.Null(e.FormatMessage("es_ES"));
    }

    [Fact]
    public void FormatMessage_NoArgs_ReturnsRawText()
    {
        var e = ErrorConfiguration.Create(id: "err", name: "Err", messages: [
            new ErrorMessageConfiguration("en_US", "Cannot divide by zero")
        ]);
        Assert.Equal("Cannot divide by zero", e.FormatMessage("en_US"));
    }

    [Fact]
    public void FormatResponse_ReturnsTypedResponse()
    {
        var e = ErrorConfiguration.Create(id: "err", name: "Err Name", messages: [
            new ErrorMessageConfiguration("en_US", "Bad value {val}")
        ]);
        var resp = e.FormatResponse("en_US", new() { ["val"] = "x" });
        Assert.NotNull(resp);
        Assert.Equal("err", resp!.ErrorCode);
        Assert.Equal("Err Name", resp.Name);
        Assert.Equal("Bad value x", resp.Message);
        Assert.NotNull(resp.Context);
        Assert.Equal("x", resp.Context!["val"]);
    }

    [Fact]
    public void FormatResponse_ReturnsNull_NoMessage()
    {
        var e = ErrorConfiguration.Create(id: "err", name: "Err");
        Assert.Null(e.FormatResponse());
    }
}

public class ErrorMessageTests
{
    [Fact]
    public void Format_WithPlaceholders()
    {
        var msg = new ErrorMessageConfiguration("en_US", "Value {value} is invalid");
        Assert.Equal("Value 42 is invalid", msg.Format(new() { ["value"] = 42 }));
    }

    [Fact]
    public void Format_NoArgs_ReturnsRaw()
    {
        var msg = new ErrorMessageConfiguration("en_US", "ErrorConfiguration occurred");
        Assert.Equal("ErrorConfiguration occurred", msg.Format());
    }

    [Fact]
    public void Format_MissingKey_PreservesPlaceholder()
    {
        var msg = new ErrorMessageConfiguration("en_US", "Value {missing} here");
        Assert.Equal("Value {missing} here", msg.Format(new() { ["other"] = "x" }));
    }
}

public class ErrorAggregateTests
{
    [Fact]
    public void Rename_Updates()
    {
        var agg = new ErrorAggregate(ErrorConfiguration.Create(id: "err", name: "Old"));
        agg.Rename("New");
        Assert.Equal("New", agg.Domain.Name);
    }

    [Fact]
    public void SetMessage_AddsNew()
    {
        var agg = new ErrorAggregate(ErrorConfiguration.Create(id: "err", name: "Err"));
        agg.SetMessage("en_US", "ErrorConfiguration text");
        Assert.Single(agg.Domain.Messages!);
        Assert.Equal("ErrorConfiguration text", agg.Domain.Messages![0].Text);
    }

    [Fact]
    public void SetMessage_UpdatesExisting()
    {
        var agg = new ErrorAggregate(ErrorConfiguration.Create(id: "err", name: "Err",
            messages: [new ErrorMessageConfiguration("en_US", "Old")]));
        agg.SetMessage("en_US", "New");
        Assert.Single(agg.Domain.Messages!);
        Assert.Equal("New", agg.Domain.Messages![0].Text);
    }

    [Fact]
    public void RemoveMessage_Removes()
    {
        var agg = new ErrorAggregate(ErrorConfiguration.Create(id: "err", name: "Err",
            messages: [new ErrorMessageConfiguration("en_US", "Text"), new ErrorMessageConfiguration("es_ES", "Texto")]));
        agg.RemoveMessage("en_US");
        Assert.Single(agg.Domain.Messages!);
        Assert.Equal("es_ES", agg.Domain.Messages![0].Lang);
    }
}
