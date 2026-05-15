using Tiferet.Assets;
using Tiferet.Events;
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

    /// <summary>Domain-specific mutation for Value.</summary>
    public void SetValue(int value) => SetAttribute(nameof(SampleDomain.Value), value);

    /// <summary>Test helper — exposes SetAttribute for invalid-attribute testing.</summary>
    public void SetAttributePublic(string attribute, object? value) => SetAttribute(attribute, value);
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
    public void Rename_UpdatesProperty()
    {
        var agg = CreateAggregate();
        agg.Rename("Beta");
        Assert.Equal("Beta", agg.Domain.Name);
    }

    [Fact]
    public void Rename_PreservesOtherProperties()
    {
        var agg = CreateAggregate();
        agg.Rename("Beta");
        Assert.Equal("1", agg.Domain.Id);
        Assert.Equal(42, agg.Domain.Value);
    }

    [Fact]
    public void SetAttribute_ThrowsOnInvalidAttribute()
    {
        var agg = CreateAggregate();
        var ex = Assert.Throws<TiferetException>(
            () => agg.SetAttributePublic("NonExistent", "val"));
        Assert.Equal(ErrorCodes.InvalidModelAttribute, ex.ErrorCode);
        Assert.Equal("NonExistent", ex.Context["attribute"]);
    }

    [Fact]
    public void SetValue_UpdatesValueType()
    {
        var agg = CreateAggregate();
        agg.SetValue(99);
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
    public void Mutation_ProducesNewRecordInstance()
    {
        var agg = CreateAggregate();
        var originalDomain = agg.Domain;
        agg.Rename("Delta");
        Assert.NotSame(originalDomain, agg.Domain);
    }

    [Fact]
    public void IsAbstractGenericClass()
    {
        Assert.True(typeof(Aggregate<>).IsAbstract);
        Assert.True(typeof(Aggregate<>).IsGenericTypeDefinition);
    }
}
