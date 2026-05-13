using Tiferet.Domain;
using Tiferet.Mappers;

namespace Tiferet.Tests.Mappers;

// Reuse SampleDomain and SampleAggregate from AggregateTests.

// Concrete transfer object for testing.
public class SampleTransferObject : TransferObject<SampleDomain, SampleAggregate>
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Value { get; set; }
    public string? Optional { get; set; }

    protected override Dictionary<string, RoleConfig> Roles { get; } = new()
    {
        [SerializationRoles.ToModel] = new RoleConfig
        {
            Exclude = ["Optional"],
        },
        [SerializationRoles.ToDataYaml] = new RoleConfig
        {
            Exclude = ["Id"],
            ByAlias = true,
        },
    };
}

public class TransferObjectTests
{
    private static SampleTransferObject CreateTransfer() => new()
    {
        Id = "1",
        Name = "Alpha",
        Value = 42,
        Optional = null,
    };

    [Fact]
    public void ToDictionary_ReturnsAllNonNullProperties()
    {
        var to = CreateTransfer();
        var result = to.ToDictionary();
        Assert.Equal("1", result["Id"]);
        Assert.Equal("Alpha", result["Name"]);
        Assert.Equal(42, result["Value"]);
        Assert.False(result.ContainsKey("Optional")); // null excluded by default
    }

    [Fact]
    public void ToDictionary_WithRole_ExcludesProperties()
    {
        var to = CreateTransfer();
        to.Optional = "present";
        var result = to.ToDictionary(SerializationRoles.ToModel);
        Assert.True(result.ContainsKey("Id"));
        Assert.False(result.ContainsKey("Optional")); // excluded by role
    }

    [Fact]
    public void ToDictionary_ToDataYamlRole_ExcludesId()
    {
        var to = CreateTransfer();
        var result = to.ToDictionary(SerializationRoles.ToDataYaml);
        Assert.False(result.ContainsKey("Id"));
        Assert.True(result.ContainsKey("Name"));
    }

    [Fact]
    public void ToDictionary_WithOverrides()
    {
        var to = CreateTransfer();
        var result = to.ToDictionary(overrides: new() { ["Name"] = "Overridden" });
        Assert.Equal("Overridden", result["Name"]);
    }

    [Fact]
    public void ToDictionary_IncludesNonNullOptional()
    {
        var to = CreateTransfer();
        to.Optional = "present";
        var result = to.ToDictionary();
        Assert.Equal("present", result["Optional"]);
    }

    [Fact]
    public void Map_ConstructsAggregate()
    {
        var to = CreateTransfer();
        var agg = to.Map();
        Assert.Equal("1", agg.Domain.Id);
        Assert.Equal("Alpha", agg.Domain.Name);
        Assert.Equal(42, agg.Domain.Value);
    }

    [Fact]
    public void Map_WithOverrides()
    {
        var to = CreateTransfer();
        var agg = to.Map(new() { ["Name"] = "Overridden" });
        Assert.Equal("Overridden", agg.Domain.Name);
    }

    [Fact]
    public void TransferObject_ConstructedExplicitly_MapsToAggregate()
    {
        var domain = new SampleDomain("1", "Alpha", 42);
        var to = new SampleTransferObject { Id = domain.Id, Name = domain.Name, Value = domain.Value };
        Assert.Equal("1", to.Id);
        Assert.Equal("Alpha", to.Name);
        Assert.Equal(42, to.Value);
    }

    [Fact]
    public void TransferObject_ConstructedWithOverride_ReflectsOverride()
    {
        var domain = new SampleDomain("1", "Alpha", 42);
        var to = new SampleTransferObject { Id = domain.Id, Name = "Overridden", Value = domain.Value };
        Assert.Equal("Overridden", to.Name);
    }

    [Fact]
    public void RoundTrip_DomainToTransferToAggregate()
    {
        var domain = new SampleDomain("1", "Alpha", 42);
        var to = new SampleTransferObject { Id = domain.Id, Name = domain.Name, Value = domain.Value };
        var agg = to.Map();
        Assert.Equal(domain.Id, agg.Domain.Id);
        Assert.Equal(domain.Name, agg.Domain.Name);
        Assert.Equal(domain.Value, agg.Domain.Value);
    }

    [Fact]
    public void ToDictionary_UnknownRole_IgnoresRoleConfig()
    {
        var to = CreateTransfer();
        var result = to.ToDictionary("NonExistentRole");
        // Should just serialize everything (minus nulls) without role filtering.
        Assert.True(result.ContainsKey("Id"));
        Assert.True(result.ContainsKey("Name"));
    }

    [Fact]
    public void IsAbstractGenericClass()
    {
        Assert.True(typeof(TransferObject<,>).IsAbstract);
        Assert.True(typeof(TransferObject<,>).IsGenericTypeDefinition);
    }
}
