using System.Text.Json;
using Tiferet.Domain;
using Tiferet.Events;
using Tiferet.Utilities.Json;

namespace Tiferet.Tests.Utilities.Json;

// Test DTOs with different naming conventions.

[JsonNaming(NamingConvention.SnakeCase)]
public class SnakeCaseDto
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public int ItemCount { get; set; }
}

[JsonNaming(NamingConvention.CamelCase)]
public class CamelCaseDto
{
    public string FirstName { get; set; } = "";
    public int TotalItems { get; set; }
}

// No attribute — defaults to PascalCase (null policy).
public class PascalCaseDto
{
    public string FirstName { get; set; } = "";
}

// Inheriting attribute from base class.
[JsonNaming(NamingConvention.SnakeCase)]
public class BaseDto
{
    public string SomeField { get; set; } = "";
}

public class DerivedDto : BaseDto
{
    public string AnotherField { get; set; } = "";
}

public class ConventionNamingResolverTests
{
    [Fact]
    public void ResolvePolicy_SnakeCase_ReturnsSnakeCaseLower()
    {
        var policy = ConventionNamingResolver.ResolvePolicy(NamingConvention.SnakeCase);
        Assert.NotNull(policy);
        Assert.Equal("first_name", policy!.ConvertName("FirstName"));
    }

    [Fact]
    public void ResolvePolicy_CamelCase_ReturnsCamelCasePolicy()
    {
        var policy = ConventionNamingResolver.ResolvePolicy(NamingConvention.CamelCase);
        Assert.NotNull(policy);
        Assert.Equal("firstName", policy!.ConvertName("FirstName"));
    }

    [Fact]
    public void ResolvePolicy_PascalCase_ReturnsNull()
    {
        var policy = ConventionNamingResolver.ResolvePolicy(NamingConvention.PascalCase);
        Assert.Null(policy);
    }

    [Fact]
    public void GetOptionsForType_SnakeCase_SetsPropertyNamingPolicy()
    {
        var options = ConventionNamingResolver.GetOptionsForType<SnakeCaseDto>();
        Assert.NotNull(options.PropertyNamingPolicy);
        Assert.Equal("first_name", options.PropertyNamingPolicy!.ConvertName("FirstName"));
    }

    [Fact]
    public void GetOptionsForType_CamelCase_SetsPropertyNamingPolicy()
    {
        var options = ConventionNamingResolver.GetOptionsForType<CamelCaseDto>();
        Assert.NotNull(options.PropertyNamingPolicy);
        Assert.Equal("firstName", options.PropertyNamingPolicy!.ConvertName("FirstName"));
    }

    [Fact]
    public void GetOptionsForType_NoAttribute_DefaultsToPascalCase()
    {
        var options = ConventionNamingResolver.GetOptionsForType<PascalCaseDto>();
        Assert.Null(options.PropertyNamingPolicy);
    }

    [Fact]
    public void GetOptionsForType_IsCached()
    {
        var options1 = ConventionNamingResolver.GetOptionsForType<SnakeCaseDto>();
        var options2 = ConventionNamingResolver.GetOptionsForType<SnakeCaseDto>();
        Assert.Same(options1, options2);
    }

    [Fact]
    public void GetOptionsForType_InheritedAttribute_ResolvesFromBase()
    {
        var options = ConventionNamingResolver.GetOptionsForType<DerivedDto>();
        Assert.NotNull(options.PropertyNamingPolicy);
        Assert.Equal("another_field", options.PropertyNamingPolicy!.ConvertName("AnotherField"));
    }

    [Fact]
    public void GetOptionsForType_PropertyNameCaseInsensitive_IsTrue()
    {
        var options = ConventionNamingResolver.GetOptionsForType<SnakeCaseDto>();
        Assert.True(options.PropertyNameCaseInsensitive);
    }
}

public class JsonSerializerHelperTests
{
    [Fact]
    public void Serialize_SnakeCase_ProducesSnakeCaseJson()
    {
        var dto = new SnakeCaseDto { FirstName = "John", LastName = "Doe", ItemCount = 5 };
        var json = JsonSerializerHelper.Serialize(dto);
        Assert.Contains("\"first_name\"", json);
        Assert.Contains("\"last_name\"", json);
        Assert.Contains("\"item_count\"", json);
    }

    [Fact]
    public void Deserialize_SnakeCase_ParsesCorrectly()
    {
        var json = """{"first_name":"Jane","last_name":"Smith","item_count":3}""";
        var dto = JsonSerializerHelper.Deserialize<SnakeCaseDto>(json);
        Assert.Equal("Jane", dto.FirstName);
        Assert.Equal("Smith", dto.LastName);
        Assert.Equal(3, dto.ItemCount);
    }

    [Fact]
    public void Serialize_CamelCase_ProducesCamelCaseJson()
    {
        var dto = new CamelCaseDto { FirstName = "Alice", TotalItems = 10 };
        var json = JsonSerializerHelper.Serialize(dto);
        Assert.Contains("\"firstName\"", json);
        Assert.Contains("\"totalItems\"", json);
    }

    [Fact]
    public void Deserialize_CamelCase_ParsesCorrectly()
    {
        var json = """{"firstName":"Bob","totalItems":7}""";
        var dto = JsonSerializerHelper.Deserialize<CamelCaseDto>(json);
        Assert.Equal("Bob", dto.FirstName);
        Assert.Equal(7, dto.TotalItems);
    }

    [Fact]
    public void Deserialize_PascalCase_ParsesCorrectly()
    {
        var json = """{"FirstName":"Charlie"}""";
        var dto = JsonSerializerHelper.Deserialize<PascalCaseDto>(json);
        Assert.Equal("Charlie", dto.FirstName);
    }

    [Fact]
    public void Deserialize_InvalidJson_ThrowsTiferetException()
    {
        var ex = Assert.Throws<TiferetException>(
            () => JsonSerializerHelper.Deserialize<SnakeCaseDto>("not json"));
        Assert.Equal(ErrorCodes.HttpDeserializationFailed, ex.ErrorCode);
    }

    [Fact]
    public void RoundTrip_SnakeCase_PreservesData()
    {
        var original = new SnakeCaseDto { FirstName = "Test", LastName = "User", ItemCount = 42 };
        var json = JsonSerializerHelper.Serialize(original);
        var restored = JsonSerializerHelper.Deserialize<SnakeCaseDto>(json);
        Assert.Equal(original.FirstName, restored.FirstName);
        Assert.Equal(original.LastName, restored.LastName);
        Assert.Equal(original.ItemCount, restored.ItemCount);
    }

    [Fact]
    public void GetOptions_ReturnsCachedInstance()
    {
        var opts1 = JsonSerializerHelper.GetOptions<SnakeCaseDto>();
        var opts2 = JsonSerializerHelper.GetOptions<SnakeCaseDto>();
        Assert.Same(opts1, opts2);
    }

    [Fact]
    public void Serialize_NullProperties_AreExcluded()
    {
        var dto = new SnakeCaseDto { FirstName = "Only", LastName = null!, ItemCount = 0 };
        var json = JsonSerializerHelper.Serialize(dto);
        Assert.DoesNotContain("\"last_name\"", json);
        // Zero is not null — should be present.
        Assert.Contains("\"item_count\"", json);
    }
}
