using System.Text;

using Tiferet.Domain;
using Tiferet.Mappers;
using Tiferet.Repositories;
using Tiferet.Mappers.App;
using Tiferet.Mappers.Error;
using Tiferet.Domain.Error;
using Tiferet.Domain.App;

namespace Tiferet.Tests.Repositories;

public class ErrorYamlRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _yamlFile;

    public ErrorYamlRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tiferet_repo_err_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _yamlFile = Path.Combine(_tempDir, "error.yaml");
        File.WriteAllText(_yamlFile, """
            errors:
              invalid_input:
                Name: Invalid Input
                ErrorCode: INVALID_INPUT
                Messages:
                  - Lang: en_US
                    Text: "Value {value} is invalid"
              not_found:
                Name: Not Found
            """, Encoding.UTF8);
    }

    public void Dispose() { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); }

    [Fact]
    public void Exists_ReturnsTrueForExisting()
    {
        var repo = new ErrorYamlRepository(_yamlFile);
        Assert.True(repo.Exists("invalid_input"));
    }

    [Fact]
    public void Exists_ReturnsFalseForMissing()
    {
        var repo = new ErrorYamlRepository(_yamlFile);
        Assert.False(repo.Exists("nonexistent"));
    }

    [Fact]
    public void Get_ReturnsAggregate()
    {
        var repo = new ErrorYamlRepository(_yamlFile);
        var error = repo.Get("invalid_input");
        Assert.NotNull(error);
        Assert.Equal("invalid_input", error.Domain.Id);
        Assert.Equal("Invalid Input", error.Domain.Name);
        Assert.Equal("INVALID_INPUT", error.Domain.ErrorCode);
        Assert.NotNull(error.Domain.Messages);
        Assert.Single(error.Domain.Messages);
        Assert.Equal("en_US", error.Domain.Messages[0].Lang);
    }

    [Fact]
    public void Get_ReturnsNullForMissing()
    {
        var repo = new ErrorYamlRepository(_yamlFile);
        Assert.Null(repo.Get("nonexistent"));
    }

    [Fact]
    public void List_ReturnsAll()
    {
        var repo = new ErrorYamlRepository(_yamlFile);
        var all = repo.List();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void Save_PersistsNewEntity()
    {
        var repo = new ErrorYamlRepository(_yamlFile);
        repo.Save(ErrorAggregate.Create("server_error", "Server ErrorConfiguration"));

        var loaded = repo.Get("server_error");
        Assert.NotNull(loaded);
        Assert.Equal("Server ErrorConfiguration", loaded.Domain.Name);
    }

    [Fact]
    public void Save_UpdatesExistingEntity()
    {
        var repo = new ErrorYamlRepository(_yamlFile);
        var error = repo.Get("invalid_input")!;
        error.Rename("Updated Name");
        repo.Save(error);

        var reloaded = repo.Get("invalid_input");
        Assert.Equal("Updated Name", reloaded!.Domain.Name);
    }

    [Fact]
    public void Delete_RemovesEntity()
    {
        var repo = new ErrorYamlRepository(_yamlFile);
        repo.Delete("invalid_input");
        Assert.False(repo.Exists("invalid_input"));
    }

    [Fact]
    public void Delete_IsIdempotent()
    {
        var repo = new ErrorYamlRepository(_yamlFile);
        repo.Delete("nonexistent");
        // Should not throw.
        Assert.Equal(2, repo.List().Count);
    }
}

public class AppYamlRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _yamlFile;

    public AppYamlRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tiferet_repo_app_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _yamlFile = Path.Combine(_tempDir, "app.yaml");
        File.WriteAllText(_yamlFile, """
            interfaces:
              basic_calc:
                Name: Basic Calculator
                AssemblyName: MyApp
                TypeName: MyApp.BasicCalc
                Description: A simple calculator
            """, Encoding.UTF8);
    }

    public void Dispose() { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); }

    [Fact]
    public void Get_ReturnsAppInterface()
    {
        var repo = new AppYamlRepository(_yamlFile);
        var app = repo.Get("basic_calc");
        Assert.NotNull(app);
        Assert.Equal("basic_calc", app.Domain.Id);
        Assert.Equal("Basic Calculator", app.Domain.Name);
        Assert.Equal("MyApp", app.Domain.AssemblyName);
    }

    [Fact]
    public void Save_RoundTrip()
    {
        var repo = new AppYamlRepository(_yamlFile);
        var iface = new AppInterfaceConfiguration("cli_app", "CLI App", "MyApp", "MyApp.CliApp");
        repo.Save(new AppInterfaceAggregate(iface));

        var loaded = repo.Get("cli_app");
        Assert.NotNull(loaded);
        Assert.Equal("CLI App", loaded.Domain.Name);
        Assert.Equal(2, repo.List().Count);
    }

    [Fact]
    public void Delete_RemovesInterface()
    {
        var repo = new AppYamlRepository(_yamlFile);
        repo.Delete("basic_calc");
        Assert.Null(repo.Get("basic_calc"));
    }
}
