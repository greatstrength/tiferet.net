using Tiferet.Domain;
using Tiferet.Events;

namespace Tiferet.Tests.Events;

// Parameter records.
public record AddNumbersParams(int A, int B);
public record GreetParams(string Name);
public record DivideParams(double A, double B);

// A test event that adds two numbers.
public class AddNumbersEvent : DomainEvent<AddNumbersParams, int>
{
    public override int Execute(AddNumbersParams p) => p.A + p.B;
}

// A test event with an injected dependency.
public class GreetEvent : DomainEvent<GreetParams, string>
{
    private readonly string _prefix;

    public GreetEvent(string prefix)
    {
        _prefix = prefix;
    }

    public override string Execute(GreetParams p) => $"{_prefix} {p.Name}";
}

// A test event that uses Verify for domain rule enforcement.
public class DivideEvent : DomainEvent<DivideParams, double>
{
    public override double Execute(DivideParams p)
    {
        Verify(p.B != 0, "DIVISION_BY_ZERO", "Cannot divide by zero.");
        return p.A / p.B;
    }
}

public class DomainEventTests
{
    [Fact]
    public void Execute_ReturnsTypedResult()
    {
        var evt = new AddNumbersEvent();
        Assert.Equal(7, evt.Execute(new(3, 4)));
    }

    [Fact]
    public void Execute_WithDependency()
    {
        var evt = new GreetEvent("Hello");
        Assert.Equal("Hello World", evt.Execute(new("World")));
    }

    [Fact]
    public void Verify_PassesOnTrue()
    {
        var evt = new DivideEvent();
        Assert.Equal(5.0, evt.Execute(new(10.0, 2.0)));
    }

    [Fact]
    public void Verify_ThrowsOnFalse()
    {
        var evt = new DivideEvent();
        var ex = Assert.Throws<TiferetException>(
            () => evt.Execute(new(10.0, 0.0)));
        Assert.Equal("DIVISION_BY_ZERO", ex.ErrorCode);
    }

    [Fact]
    public void Verify_IncludesContextPairs()
    {
        var evt = new AddNumbersEvent();
        var ex = Assert.Throws<TiferetException>(
            () => evt.Verify(false, "TEST_ERR", "detail", ("key", "val")));
        Assert.Equal("val", ex.Context["key"]);
    }

    [Fact]
    public void RaiseError_Throws()
    {
        var ex = Assert.Throws<TiferetException>(
            () => DomainEvent.RaiseError("MY_ERROR", "boom"));
        Assert.Equal("MY_ERROR", ex.ErrorCode);
    }

    [Fact]
    public void RaiseError_WithContextPairs()
    {
        var ex = Assert.Throws<TiferetException>(
            () => DomainEvent.RaiseError("ERR", "msg", ("a", 1), ("b", "two")));
        Assert.Equal(1, ex.Context["a"]);
        Assert.Equal("two", ex.Context["b"]);
    }

    [Fact]
    public void GenericBase_InheritsFromDomainEvent()
    {
        Assert.True(typeof(DomainEvent).IsAssignableFrom(typeof(DomainEvent<AddNumbersParams, int>)));
    }
}

public class ParseParameterTests
{
    [Fact]
    public void Parse_ReturnsLiteralValue()
    {
        Assert.Equal("hello", ParseParameter.Parse("hello"));
    }

    [Fact]
    public void Parse_ResolvesEnvVar()
    {
        Environment.SetEnvironmentVariable("TIFERET_TEST_VAR", "resolved");
        try
        {
            Assert.Equal("resolved", ParseParameter.Parse("$env.TIFERET_TEST_VAR"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("TIFERET_TEST_VAR", null);
        }
    }

    [Fact]
    public void Parse_ThrowsOnMissingEnvVar()
    {
        var ex = Assert.Throws<TiferetException>(
            () => ParseParameter.Parse("$env.NONEXISTENT_VAR_XYZ_123"));
        Assert.Equal(ErrorCodes.ParameterParsingFailed, ex.ErrorCode);
    }
}

public class ImportDependencyTests
{
    [Fact]
    public void Resolve_ResolvesKnownType()
    {
        var type = ImportDependency.Resolve("System.Private.CoreLib", "System.String");
        Assert.Equal(typeof(string), type);
    }

    [Fact]
    public void Resolve_ThrowsOnBadAssembly()
    {
        var ex = Assert.Throws<TiferetException>(
            () => ImportDependency.Resolve("NonExistent.Assembly", "Foo.Bar"));
        Assert.Equal(ErrorCodes.ImportDependencyFailed, ex.ErrorCode);
    }
}
