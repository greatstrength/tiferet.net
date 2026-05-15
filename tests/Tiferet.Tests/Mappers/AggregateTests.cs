using Tiferet.Events;
using Tiferet.Domain;
using Tiferet.Mappers;

namespace Tiferet.Tests.Mappers;

// Test domain record.
public sealed record SampleDomain(string Id, string Name, int Value) : DomainObject;

// Concrete aggregate for testing — uses adapter pattern.
public record SampleAggregate : Aggregate<SampleDomain>
{
    public SampleAggregate(SampleDomain state) : base(state) { }

    // Delegated properties.
    public string Id => State.Id;
    public string Name => State.Name;
    public int Value => State.Value;

    /// <summary>Domain-specific mutation shorthand.</summary>
    public void Rename(string name) => Mutate(s => s with { Name = name });

    /// <summary>Domain-specific mutation for Value.</summary>
    public void SetValue(int value) => Mutate(s => s with { Value = value });

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
        Assert.Equal("1", agg.Id);
        Assert.Equal("Alpha", agg.Name);
        Assert.Equal(42, agg.Value);
    }

    [Fact]
    public void ToDomainObject_ReturnsInternalState()
    {
        var agg = CreateAggregate();
        var domain = agg.ToDomainObject();
        Assert.Equal("1", domain.Id);
        Assert.Equal("Alpha", domain.Name);
        Assert.Equal(42, domain.Value);
    }

    [Fact]
    public void Rename_UpdatesProperty()
    {
        var agg = CreateAggregate();
        agg.Rename("Beta");
        Assert.Equal("Beta", agg.Name);
    }

    [Fact]
    public void Rename_PreservesOtherProperties()
    {
        var agg = CreateAggregate();
        agg.Rename("Beta");
        Assert.Equal("1", agg.Id);
        Assert.Equal(42, agg.Value);
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
        Assert.Equal(99, agg.Value);
    }

    [Fact]
    public void Mutate_UpdatesToDomainObject()
    {
        var agg = CreateAggregate();
        agg.Rename("Gamma");
        Assert.Equal("Gamma", agg.ToDomainObject().Name);
    }

    [Fact]
    public void Equality_DelegatesToState()
    {
        var agg1 = CreateAggregate();
        var agg2 = CreateAggregate();
        Assert.Equal(agg1, agg2);
        agg1.Rename("Different");
        Assert.NotEqual(agg1, agg2);
    }

    [Fact]
    public void IsAbstractRecord()
    {
        Assert.True(typeof(Aggregate).IsAbstract);
        Assert.True(typeof(Aggregate<SampleDomain>).IsAbstract);
    }
}
