using System.CommandLine;
using System.Text;
using Tiferet.Blueprints;
using Tiferet.Contexts;

namespace Tiferet.Tests.Blueprints;

// *** CliBlueprint Tests

public class CliBlueprintTests : IDisposable
{
    private readonly string _configDir;

    public CliBlueprintTests()
    {
        _configDir = Path.Combine(Path.GetTempPath(), $"tiferet_cli_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_configDir);
        WriteMinimalConfigs();
    }

    public void Dispose()
    {
        if (Directory.Exists(_configDir))
            Directory.Delete(_configDir, true);
    }

    private void WriteMinimalConfigs()
    {
        File.WriteAllText(Path.Combine(_configDir, "config.yml"), """
            interfaces:
              basic_calc:
                Name: Basic Calculator
                AssemblyName: Tiferet.Contexts
                TypeName: Tiferet.Contexts.AppInterfaceContext
            features:
              calc:
                add:
                  Name: Add Number
                  Description: Adds one number to another
                  Steps: []
                sqrt:
                  Name: Square Root
                  Description: Calculates the square root
                  Steps: []
            errors: {}
            services: {}
            const: {}
            logging:
              formatters: {}
              handlers: {}
              loggers: {}
            cli:
              cmds:
                calc:
                  add:
                    Name: Add Number Command
                    Description: Adds two numbers.
                    Arguments:
                      - NameOrFlags:
                          - a
                        Description: The first number to add.
                      - NameOrFlags:
                          - b
                        Description: The second number to add.
                  sqrt:
                    Name: Square Root Command
                    Description: Calculates the square root of a number.
                    Arguments:
                      - NameOrFlags:
                          - a
                        Description: The number to square root.
            """, Encoding.UTF8);
    }

    // *** Tests

    // ** test: BuildCli returns RootCommand
    [Fact]
    public void BuildCli_ReturnsRootCommand()
    {
        var app = AppBlueprint.BuildApp("basic_calc", _configDir);
        var configFile = Path.Combine(_configDir, "config.yml");

        var root = CliBlueprint.BuildCli(app, configFile);

        Assert.NotNull(root);
        Assert.IsType<RootCommand>(root);
    }

    // ** test: BuildCli creates group commands
    [Fact]
    public void BuildCli_CreatesGroupCommands()
    {
        var app = AppBlueprint.BuildApp("basic_calc", _configDir);
        var configFile = Path.Combine(_configDir, "config.yml");

        var root = CliBlueprint.BuildCli(app, configFile);

        // Should have one group command: "calc"
        var calcGroup = root.Children.OfType<Command>()
            .FirstOrDefault(c => c.Name == "calc");
        Assert.NotNull(calcGroup);
    }

    // ** test: BuildCli creates subcommands with arguments
    [Fact]
    public void BuildCli_CreatesSubcommands()
    {
        var app = AppBlueprint.BuildApp("basic_calc", _configDir);
        var configFile = Path.Combine(_configDir, "config.yml");

        var root = CliBlueprint.BuildCli(app, configFile);

        var calcGroup = root.Children.OfType<Command>().First(c => c.Name == "calc");

        // Should have "add" and "sqrt" subcommands.
        var addCmd = calcGroup.Children.OfType<Command>().FirstOrDefault(c => c.Name == "add");
        var sqrtCmd = calcGroup.Children.OfType<Command>().FirstOrDefault(c => c.Name == "sqrt");

        Assert.NotNull(addCmd);
        Assert.NotNull(sqrtCmd);
    }

    // ** test: add command has two positional arguments
    [Fact]
    public void BuildCli_AddCommand_HasTwoArguments()
    {
        var app = AppBlueprint.BuildApp("basic_calc", _configDir);
        var configFile = Path.Combine(_configDir, "config.yml");

        var root = CliBlueprint.BuildCli(app, configFile);
        var calcGroup = root.Children.OfType<Command>().First(c => c.Name == "calc");
        var addCmd = calcGroup.Children.OfType<Command>().First(c => c.Name == "add");

        // Two positional arguments: "a" and "b".
        var args = addCmd.Children.OfType<Argument>().ToList();
        Assert.Equal(2, args.Count);
        Assert.Contains(args, a => a.Name == "a");
        Assert.Contains(args, a => a.Name == "b");
    }

    // ** test: sqrt command has one positional argument
    [Fact]
    public void BuildCli_SqrtCommand_HasOneArgument()
    {
        var app = AppBlueprint.BuildApp("basic_calc", _configDir);
        var configFile = Path.Combine(_configDir, "config.yml");

        var root = CliBlueprint.BuildCli(app, configFile);
        var calcGroup = root.Children.OfType<Command>().First(c => c.Name == "calc");
        var sqrtCmd = calcGroup.Children.OfType<Command>().First(c => c.Name == "sqrt");

        var args = sqrtCmd.Children.OfType<Argument>().ToList();
        Assert.Single(args);
        Assert.Equal("a", args[0].Name);
    }

    // ** test: BuildCli with custom description
    [Fact]
    public void BuildCli_WithCustomDescription()
    {
        var app = AppBlueprint.BuildApp("basic_calc", _configDir);
        var configFile = Path.Combine(_configDir, "config.yml");

        var root = CliBlueprint.BuildCli(app, configFile, description: "My Calculator CLI");

        Assert.Equal("My Calculator CLI", root.Description);
    }

    // ** test: convenience overload bootstraps everything
    [Fact]
    public void BuildCli_ConvenienceOverload_Works()
    {
        var root = CliBlueprint.BuildCli(
            "basic_calc",
            configDir: _configDir);

        Assert.NotNull(root);
        var calcGroup = root.Children.OfType<Command>().FirstOrDefault(c => c.Name == "calc");
        Assert.NotNull(calcGroup);
    }

    // ** test: parse result recognizes valid command structure
    [Fact]
    public void BuildCli_ParseResult_ValidArgs()
    {
        var app = AppBlueprint.BuildApp("basic_calc", _configDir);
        var configFile = Path.Combine(_configDir, "config.yml");

        var root = CliBlueprint.BuildCli(app, configFile);

        // Verify the parser recognizes "calc add 1 2".
        var result = root.Parse("calc add 1 2");
        Assert.Empty(result.Errors);
    }

    // ** test: parse result recognizes sqrt command
    [Fact]
    public void BuildCli_ParseResult_SqrtCommand()
    {
        var app = AppBlueprint.BuildApp("basic_calc", _configDir);
        var configFile = Path.Combine(_configDir, "config.yml");

        var root = CliBlueprint.BuildCli(app, configFile);

        var result = root.Parse("calc sqrt 16");
        Assert.Empty(result.Errors);
    }
}
