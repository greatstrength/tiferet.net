using System.Text;
using System.Text.Json;

using Tiferet.Core;
using Tiferet.Utilities;

namespace Tiferet.Tests.Utilities;

public class JsonLoaderTests : IDisposable
{
    private readonly string _tempDir;

    public JsonLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tiferet_json_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteJson(string content, string name = "test.json")
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content, Encoding.UTF8);
        return path;
    }

    [Fact]
    public void Load_ReturnsJsonElement()
    {
        var path = WriteJson("""{"name":"hello","count":42}""");
        var loader = JsonLoader.ForReading(path);
        var result = loader.Load();
        Assert.Equal(JsonValueKind.Object, result.ValueKind);
        Assert.Equal("hello", result.GetProperty("name").GetString());
        Assert.Equal(42, result.GetProperty("count").GetInt32());
    }

    [Fact]
    public void Load_WithStartNode_SelectsSubtree()
    {
        var path = WriteJson("""{"features":{"calc":{"add":true}}}""");
        var loader = JsonLoader.ForReading(path);
        var result = loader.Load(
            startNode: root => root.GetProperty("features").GetProperty("calc"));
        Assert.True(result.GetProperty("add").GetBoolean());
    }

    [Fact]
    public void Save_WritesJson()
    {
        var path = Path.Combine(_tempDir, "out.json");
        var loader = JsonLoader.ForWriting(path);
        loader.Save(new { Name = "test", Count = 5 });

        var content = File.ReadAllText(path);
        Assert.Contains("\"Name\"", content);
        Assert.Contains("\"test\"", content);
    }

    [Fact]
    public void Load_FileNotFound_ThrowsTiferetException()
    {
        var path = Path.Combine(_tempDir, "missing.json");
        var loader = JsonLoader.ForReading(path);
        var ex = Assert.Throws<TiferetException>(() => loader.Load());
        Assert.Equal(ErrorCodes.FileNotFound, ex.ErrorCode);
    }

    [Fact]
    public void Load_InvalidJson_ThrowsLoadError()
    {
        var path = WriteJson("{invalid json");
        var loader = JsonLoader.ForReading(path);
        var ex = Assert.Throws<TiferetException>(() => loader.Load());
        Assert.Equal(ErrorCodes.JsonFileLoadError, ex.ErrorCode);
    }

    [Fact]
    public void VerifyJsonExtension_ValidExtension_Passes()
    {
        JsonLoader.VerifyJsonExtension("data.json");
        JsonLoader.VerifyJsonExtension("DATA.JSON");
    }

    [Fact]
    public void VerifyJsonExtension_InvalidExtension_Throws()
    {
        var ex = Assert.Throws<TiferetException>(
            () => JsonLoader.VerifyJsonExtension("data.yaml"));
        Assert.Equal(ErrorCodes.InvalidJsonFile, ex.ErrorCode);
    }

    // --- ParseJsonPath tests ---

    [Fact]
    public void ParseJsonPath_NavigatesObjectKeys()
    {
        using var doc = JsonDocument.Parse("""{"a":{"b":{"c":"found"}}}""");
        var result = JsonLoader.ParseJsonPath(doc.RootElement, "a.b.c");
        Assert.NotNull(result);
        Assert.Equal("found", result.Value.GetString());
    }

    [Fact]
    public void ParseJsonPath_NavigatesArrayIndices()
    {
        using var doc = JsonDocument.Parse("""{"items":["zero","one","two"]}""");
        var result = JsonLoader.ParseJsonPath(doc.RootElement, "items.1");
        Assert.NotNull(result);
        Assert.Equal("one", result.Value.GetString());
    }

    [Fact]
    public void ParseJsonPath_ReturnsNullForMissingKey()
    {
        using var doc = JsonDocument.Parse("""{"a":1}""");
        var result = JsonLoader.ParseJsonPath(doc.RootElement, "b");
        Assert.Null(result);
    }

    [Fact]
    public void ParseJsonPath_ReturnsNullForNullValue()
    {
        using var doc = JsonDocument.Parse("""{"a":null}""");
        var result = JsonLoader.ParseJsonPath(doc.RootElement, "a");
        Assert.Null(result);
    }

    [Fact]
    public void ParseJsonPath_ThrowsOnInvalidNavigation()
    {
        using var doc = JsonDocument.Parse("""{"a":"string_not_object"}""");
        var ex = Assert.Throws<TiferetException>(
            () => JsonLoader.ParseJsonPath(doc.RootElement, "a.b"));
        Assert.Equal(ErrorCodes.InvalidJsonPath, ex.ErrorCode);
    }

    [Fact]
    public void ParseJsonPath_ThrowsOnArrayIndexOutOfRange()
    {
        using var doc = JsonDocument.Parse("""{"items":["a"]}""");
        var ex = Assert.Throws<TiferetException>(
            () => JsonLoader.ParseJsonPath(doc.RootElement, "items.5"));
        Assert.Equal(ErrorCodes.InvalidJsonPath, ex.ErrorCode);
    }

    [Fact]
    public void ForReading_FactoryReturnsJsonLoader()
    {
        var path = WriteJson("{}");
        using var loader = JsonLoader.ForReading(path);
        Assert.IsType<JsonLoader>(loader);
    }
}
