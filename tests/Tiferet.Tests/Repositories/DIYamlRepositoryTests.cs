using System.Text;

using Tiferet.Domain;
using Tiferet.Mappers;
using Tiferet.Repositories;

namespace Tiferet.Tests.Repositories;

public class DIYamlRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _yamlFile;

    public DIYamlRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tiferet_repo_di_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _yamlFile = Path.Combine(_tempDir, "di.yaml");
        File.WriteAllText(_yamlFile, """
            services:
              error_service:
                Name: Error Service
                AssemblyName: MyApp
                TypeName: MyApp.ErrorService
                Parameters:
                  config_file: app/configs/error.yml
            const:
              APP_NAME: tiferet
            """, Encoding.UTF8);
    }

    public void Dispose() { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); }

    [Fact]
    public void ConfigurationExists_ReturnsTrue()
    {
        var repo = new DIYamlRepository(_yamlFile);
        Assert.True(repo.ConfigurationExists("error_service"));
    }

    [Fact]
    public void ConfigurationExists_ReturnsFalse()
    {
        var repo = new DIYamlRepository(_yamlFile);
        Assert.False(repo.ConfigurationExists("nonexistent"));
    }

    [Fact]
    public void GetConfiguration_ReturnsAggregate()
    {
        var repo = new DIYamlRepository(_yamlFile);
        var config = repo.GetConfiguration("error_service");
        Assert.NotNull(config);
        Assert.Equal("error_service", config.Domain.Id);
        Assert.Equal("Error Service", config.Domain.Name);
        Assert.Equal("MyApp", config.Domain.AssemblyName);
        Assert.NotNull(config.Domain.Parameters);
        Assert.Equal("app/configs/error.yml", config.Domain.Parameters["config_file"]);
    }

    [Fact]
    public void GetConfiguration_ReturnsNullForMissing()
    {
        var repo = new DIYamlRepository(_yamlFile);
        Assert.Null(repo.GetConfiguration("nonexistent"));
    }

    [Fact]
    public void ListAll_ReturnsConfigsAndConstants()
    {
        var repo = new DIYamlRepository(_yamlFile);
        var (configs, constants) = repo.ListAll();
        Assert.Single(configs);
        Assert.Equal("error_service", configs[0].Domain.Id);
        Assert.Equal("tiferet", constants["APP_NAME"]);
    }

    [Fact]
    public void SaveConfiguration_PersistsNew()
    {
        var repo = new DIYamlRepository(_yamlFile);
        var config = new ServiceConfiguration("feature_service", "Feature Service", "MyApp", "MyApp.FeatureService");
        repo.SaveConfiguration(new ServiceConfigurationAggregate(config));

        var loaded = repo.GetConfiguration("feature_service");
        Assert.NotNull(loaded);
        Assert.Equal("Feature Service", loaded.Domain.Name);
    }

    [Fact]
    public void DeleteConfiguration_Removes()
    {
        var repo = new DIYamlRepository(_yamlFile);
        repo.DeleteConfiguration("error_service");
        Assert.False(repo.ConfigurationExists("error_service"));
    }

    [Fact]
    public void DeleteConfiguration_IsIdempotent()
    {
        var repo = new DIYamlRepository(_yamlFile);
        repo.DeleteConfiguration("nonexistent");
        Assert.True(repo.ConfigurationExists("error_service"));
    }

    [Fact]
    public void SaveConstants_MergesWithExisting()
    {
        var repo = new DIYamlRepository(_yamlFile);
        repo.SaveConstants(new Dictionary<string, string> { ["VERSION"] = "1.0" });

        var (_, constants) = repo.ListAll();
        Assert.Equal("tiferet", constants["APP_NAME"]);
        Assert.Equal("1.0", constants["VERSION"]);
    }
}
