using Tiferet.Core;
using Tiferet.Domain;
using Tiferet.Mappers;

namespace Tiferet.Tests.Mappers;

// Test domain record.
public sealed record SampleDomain(string Id, string Name, int Value) : DomainObject;

// Concrete aggregate for testing.
public class SampleAggregate : Aggregate<SampleDomain>
{
    public SampleAggregate(SampleDomain domain) : base(domain) { }

    /// <summary>Domain-specific mutation shorthand.</summary>
    public void Rename(string name) => SetAttribute(nameof(SampleDomain.Name), name);
}

public class AggregateTests
{
    private static SampleAggregate CreateAggregate() =>
        new(new SampleDomain("1", "Alpha", 42));

    [Fact]
    public void Domain_ReturnsWrappedRecord()
    {
        var agg = CreateAggregate();
        Assert.Equal("1", agg.Domain.Id);
        Assert.Equal("Alpha", agg.Domain.Name);
        Assert.Equal(42, agg.Domain.Value);
    }

    [Fact]
    public void SetAttribute_UpdatesProperty()
    {
        var agg = CreateAggregate();
        agg.SetAttribute("Name", "Beta");
        Assert.Equal("Beta", agg.Domain.Name);
    }

    [Fact]
    public void SetAttribute_PreservesOtherProperties()
    {
        var agg = CreateAggregate();
        agg.SetAttribute("Name", "Beta");
        Assert.Equal("1", agg.Domain.Id);
        Assert.Equal(42, agg.Domain.Value);
    }

    [Fact]
    public void SetAttribute_ThrowsOnInvalidAttribute()
    {
        var agg = CreateAggregate();
        var ex = Assert.Throws<TiferetException>(
            () => agg.SetAttribute("NonExistent", "val"));
        Assert.Equal(ErrorCodes.InvalidModelAttribute, ex.ErrorCode);
        Assert.Equal("NonExistent", ex.Context["attribute"]);
    }

    [Fact]
    public void SetAttribute_UpdatesValueType()
    {
        var agg = CreateAggregate();
        agg.SetAttribute("Value", 99);
        Assert.Equal(99, agg.Domain.Value);
    }

    [Fact]
    public void DomainSpecificMutation_Works()
    {
        var agg = CreateAggregate();
        agg.Rename("Gamma");
        Assert.Equal("Gamma", agg.Domain.Name);
    }

    [Fact]
    public void SetAttribute_ProducesNewRecordInstance()
    {
        var agg = CreateAggregate();
        var originalDomain = agg.Domain;
        agg.SetAttribute("Name", "Delta");
        Assert.NotSame(originalDomain, agg.Domain);
    }

    [Fact]
    public void IsAbstractGenericClass()
    {
        Assert.True(typeof(Aggregate<>).IsAbstract);
        Assert.True(typeof(Aggregate<>).IsGenericTypeDefinition);
    }
}
