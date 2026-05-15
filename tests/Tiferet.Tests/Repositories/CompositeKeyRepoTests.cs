using System.Text;

using Tiferet.Domain;
using Tiferet.Mappers;
using Tiferet.Repositories;
using Tiferet.Mappers.Feature;
using Tiferet.Mappers.Cli;
using Tiferet.Domain.Feature;
using Tiferet.Domain.Cli;

namespace Tiferet.Tests.Repositories;

public class FeatureYamlRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _yamlFile;

    public FeatureYamlRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tiferet_repo_feat_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _yamlFile = Path.Combine(_tempDir, "feature.yaml");
        File.WriteAllText(_yamlFile, """
            features:
              calc:
                add:
                  Name: Add Numbers
                  Description: Adds two numbers
                  Steps:
                    - Name: Execute Add
                      ServiceId: add_event
                subtract:
                  Name: Subtract Numbers
              math:
                multiply:
                  Name: Multiply Numbers
            """, Encoding.UTF8);
    }

    public void Dispose() { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); }

    [Fact]
    public void Exists_CompositeKey()
    {
        var repo = new FeatureYamlRepository(_yamlFile);
        Assert.True(repo.Exists("calc.add"));
        Assert.True(repo.Exists("math.multiply"));
        Assert.False(repo.Exists("calc.divide"));
    }

    [Fact]
    public void Get_ReturnsFeatureWithDerivation()
    {
        var repo = new FeatureYamlRepository(_yamlFile);
        var feature = repo.Get("calc.add");
        Assert.NotNull(feature);
        Assert.Equal("calc.add", feature.Id);
        Assert.Equal("calc", feature.GroupId);
        Assert.Equal("add", feature.FeatureKey);
        Assert.Equal("Add Numbers", feature.Name);
        Assert.NotNull(feature.Steps);
        Assert.Single(feature.Steps);
        Assert.Equal("add_event", feature.Steps[0].ServiceId);
    }

    [Fact]
    public void List_FlattensAllGroups()
    {
        var repo = new FeatureYamlRepository(_yamlFile);
        var all = repo.List();
        Assert.Equal(3, all.Count);
    }

    [Fact]
    public void Save_PersistsNewFeature()
    {
        var repo = new FeatureYamlRepository(_yamlFile);
        repo.Save(FeatureAggregate.Create("Divide Numbers", groupId: "calc", featureKey: "divide"));

        var loaded = repo.Get("calc.divide");
        Assert.NotNull(loaded);
        Assert.Equal("Divide Numbers", loaded.Name);
        Assert.Equal(4, repo.List().Count);
    }

    [Fact]
    public void Delete_RemovesFeature()
    {
        var repo = new FeatureYamlRepository(_yamlFile);
        repo.Delete("calc.add");
        Assert.False(repo.Exists("calc.add"));
        Assert.True(repo.Exists("calc.subtract"));
    }

    [Fact]
    public void Delete_IsIdempotent()
    {
        var repo = new FeatureYamlRepository(_yamlFile);
        repo.Delete("calc.nonexistent");
        Assert.Equal(3, repo.List().Count);
    }
}

public class CliYamlRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _yamlFile;

    public CliYamlRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tiferet_repo_cli_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _yamlFile = Path.Combine(_tempDir, "cli.yaml");
        File.WriteAllText(_yamlFile, """
            cli:
              cmds:
                calc:
                  add:
                    Name: Add Command
                    Description: Adds two numbers
                    Arguments:
                      - NameOrFlags:
                          - a
                        Description: First number
                      - NameOrFlags:
                          - b
                        Description: Second number
              parent_args:
                - NameOrFlags:
                    - --verbose
                  Description: Enable verbose output
            """, Encoding.UTF8);
    }

    public void Dispose() { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); }

    [Fact]
    public void Get_ReturnsCliCommand()
    {
        var repo = new CliYamlRepository(_yamlFile);
        var cmd = repo.Get("calc.add");
        Assert.NotNull(cmd);
        Assert.Equal("calc.add", cmd.Id);
        Assert.Equal("Add Command", cmd.Name);
        Assert.NotNull(cmd.Arguments);
        Assert.Equal(2, cmd.Arguments.Count);
        Assert.Equal("a", cmd.Arguments[0].NameOrFlags[0]);
    }

    [Fact]
    public void List_FlattensGroups()
    {
        var repo = new CliYamlRepository(_yamlFile);
        var all = repo.List();
        Assert.Single(all);
    }

    [Fact]
    public void Save_RoundTrip()
    {
        var repo = new CliYamlRepository(_yamlFile);
        repo.Save(CliCommandAggregate.Create("Subtract Command", "subtract", "calc"));

        var loaded = repo.Get("calc.subtract");
        Assert.NotNull(loaded);
        Assert.Equal("Subtract Command", loaded.Name);
    }

    [Fact]
    public void GetParentArguments_ReturnsArgs()
    {
        var repo = new CliYamlRepository(_yamlFile);
        var args = repo.GetParentArguments();
        Assert.Single(args);
        Assert.Equal("--verbose", args[0].NameOrFlags[0]);
    }

    [Fact]
    public void Delete_RemovesCommand()
    {
        var repo = new CliYamlRepository(_yamlFile);
        repo.Delete("calc.add");
        Assert.Null(repo.Get("calc.add"));
    }
}
