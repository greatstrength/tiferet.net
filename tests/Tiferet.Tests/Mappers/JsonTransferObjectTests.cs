using Tiferet.Domain;
using Tiferet.Mappers;
using Tiferet.Utilities.Json;

namespace Tiferet.Tests.Mappers;

// Reuse SampleDomain and SampleAggregate from AggregateTests.

// A concrete JsonTransferObject for testing with snake_case JSON.
[JsonNaming(NamingConvention.SnakeCase)]
public class SampleJsonTransferObject : JsonTransferObject<SampleAggregate>
{
    public string Id { get; set; } = "";
    public string UserName { get; set; } = "";
    public int ItemCount { get; set; }

    public override SampleAggregate Map(Dictionary<string, object?>? overrides = null)
    {
        return new SampleAggregate(new SampleDomain(Id, UserName, ItemCount));
    }
}

public class JsonTransferObjectTests
{
    [Fact]
    public void Map_ConstructsAggregate()
    {
        var to = new SampleJsonTransferObject { Id = "1", UserName = "Alpha", ItemCount = 42 };
        var agg = to.Map();
        Assert.Equal("1", agg.Id);
        Assert.Equal("Alpha", agg.Name);
        Assert.Equal(42, agg.Value);
    }

    [Fact]
    public void DeserializeAndMap_SnakeCaseJson_ProducesCorrectAggregate()
    {
        var json = """{"id":"2","user_name":"Beta","item_count":99}""";
        var agg = JsonTransferObject<SampleAggregate>
            .DeserializeAndMap<SampleJsonTransferObject>(json);
        Assert.Equal("2", agg.Id);
        Assert.Equal("Beta", agg.Name);
        Assert.Equal(99, agg.Value);
    }

    [Fact]
    public void DeserializeAndMap_WithOverrides()
    {
        var json = """{"id":"3","user_name":"Gamma","item_count":10}""";
        // Overrides are passed to Map() — our test Map() ignores them,
        // but verify the method accepts them without error.
        var agg = JsonTransferObject<SampleAggregate>
            .DeserializeAndMap<SampleJsonTransferObject>(json, new() { ["extra"] = "data" });
        Assert.Equal("3", agg.Id);
        Assert.Equal("Gamma", agg.Name);
    }

    [Fact]
    public void RoundTrip_Serialize_Then_DeserializeAndMap()
    {
        var original = new SampleJsonTransferObject { Id = "4", UserName = "Delta", ItemCount = 7 };
        var json = JsonSerializerHelper.Serialize(original);
        var agg = JsonTransferObject<SampleAggregate>
            .DeserializeAndMap<SampleJsonTransferObject>(json);
        Assert.Equal("4", agg.Id);
        Assert.Equal("Delta", agg.Name);
        Assert.Equal(7, agg.Value);
    }

    [Fact]
    public void InheritsFromTransferObject()
    {
        Assert.True(typeof(TransferObject<SampleAggregate>)
            .IsAssignableFrom(typeof(JsonTransferObject<SampleAggregate>)));
    }
}
