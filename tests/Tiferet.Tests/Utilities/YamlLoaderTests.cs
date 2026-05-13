using System.Text;

using Tiferet.Core;
using Tiferet.Utilities;

namespace Tiferet.Tests.Utilities;

public class YamlLoaderTests : IDisposable
{
    private readonly string _tempDir;

    public YamlLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tiferet_yaml_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteYaml(string content, string name = "test.yaml")
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content, Encoding.UTF8);
        return path;
    }

    [Fact]
    public void Load_ReturnsDeserializedData()
    {
        var path = WriteYaml("name: hello\ncount: 42\n");
        var loader = YamlLoader.ForReading(path);
        var result = loader.Load() as Dictionary<object, object>;
        Assert.NotNull(result);
        Assert.Equal("hello", result["name"]);
        Assert.Equal("42", result["count"]);
    }

    [Fact]
    public void Load_EmptyYaml_ReturnsEmptyDict()
    {
        var path = WriteYaml("");
        var loader = YamlLoader.ForReading(path);
        var result = loader.Load();
        Assert.IsType<Dictionary<object, object>>(result);
    }

    [Fact]
    public void Load_WithStartNode_SelectsSubtree()
    {
        var path = WriteYaml("features:\n  calc:\n    add: true\n");
        var loader = YamlLoader.ForReading(path);
        var result = loader.Load(
            startNode: data =>
            {
                var root = (Dictionary<object, object>)data!;
                var features = (Dictionary<object, object>)root["features"];
                return features["calc"];
            });
        var calc = result as Dictionary<object, object>;
        Assert.NotNull(calc);
        Assert.Equal("true", calc["add"]?.ToString());
    }

    [Fact]
    public void Load_WithDataFactory_TransformsResult()
    {
        var path = WriteYaml("value: 42\n");
        var loader = YamlLoader.ForReading(path);
        var result = loader.Load(
            dataFactory: data =>
            {
                var dict = (Dictionary<object, object>)data!;
                return dict["value"];
            });
        Assert.Equal("42", result);
    }

    [Fact]
    public void Save_WritesYaml()
    {
        var path = Path.Combine(_tempDir, "out.yaml");
        var loader = YamlLoader.ForWriting(path);
        loader.Save(new Dictionary<string, object> { ["name"] = "test", ["count"] = 5 });

        var content = File.ReadAllText(path);
        Assert.Contains("name: test", content);
        Assert.Contains("count: 5", content);
    }

    [Fact]
    public void Load_FileNotFound_ThrowsTiferetException()
    {
        var path = Path.Combine(_tempDir, "missing.yaml");
        var loader = YamlLoader.ForReading(path);
        var ex = Assert.Throws<TiferetException>(() => loader.Load());
        Assert.Equal(ErrorCodes.FileNotFound, ex.ErrorCode);
    }

    [Fact]
    public void VerifyYamlExtension_ValidExtensions_Pass()
    {
        YamlLoader.VerifyYamlExtension("test.yaml");
        YamlLoader.VerifyYamlExtension("test.yml");
        YamlLoader.VerifyYamlExtension("test.YAML");
    }

    [Fact]
    public void VerifyYamlExtension_InvalidExtension_Throws()
    {
        var ex = Assert.Throws<TiferetException>(
            () => YamlLoader.VerifyYamlExtension("test.json"));
        Assert.Equal(ErrorCodes.InvalidYamlFile, ex.ErrorCode);
    }

    [Fact]
    public void ForReading_FactoryReturnsYamlLoader()
    {
        var path = WriteYaml("x: 1");
        using var loader = YamlLoader.ForReading(path);
        Assert.IsType<YamlLoader>(loader);
        Assert.Equal(FileMode.Open, loader.FileMode);
    }

    [Fact]
    public void Load_InvalidYaml_ThrowsLoadError()
    {
        var path = WriteYaml(":\n  : :\n  invalid: [unclosed");
        var loader = YamlLoader.ForReading(path);
        var ex = Assert.Throws<TiferetException>(() => loader.Load());
        Assert.Equal(ErrorCodes.YamlFileLoadError, ex.ErrorCode);
    }
}
