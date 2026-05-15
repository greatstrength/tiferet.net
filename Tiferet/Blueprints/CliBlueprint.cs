using System.CommandLine;
using System.CommandLine.Invocation;
using Tiferet.Contexts;
using Tiferet.Assets;
using Tiferet.Events;
using Tiferet.Domain;
using Tiferet.Domain.App;
using Tiferet.Domain.Cli;
using Tiferet.Domain.Feature;
using Tiferet.Events.Cli;
using Tiferet.Repositories;

namespace Tiferet.Blueprints;

/// <summary>
/// Static blueprint for building a CLI application from YAML configuration.
/// Maps <see cref="CliCommandConfiguration"/> definitions to <see cref="System.CommandLine"/>
/// commands and wires handlers to dispatch feature execution via
/// <see cref="AppInterfaceContext.Run"/>.
/// </summary>
public static class CliBlueprint
{
    /// <summary>Default CLI config file name.</summary>
    private const string DefaultCliConfigFile = "cli.yml";

    /// <summary>
    /// Build a <see cref="RootCommand"/> from a CLI config file and an existing
    /// <see cref="AppInterfaceContext"/>.
    /// </summary>
    /// <param name="app">The wired application interface context.</param>
    /// <param name="cliConfigFile">Path to the CLI YAML config file.</param>
    /// <param name="description">Optional root command description.</param>
    /// <returns>A configured <see cref="RootCommand"/> ready for invocation.</returns>
    public static RootCommand BuildCli(
        AppInterfaceContext app,
        string cliConfigFile,
        string? description = null)
    {
        // Load all CLI commands from configuration.
        var cliRepo = new CliYamlRepository(cliConfigFile);
        var listEvent = new ListCliCommands(cliRepo);
        var commands = listEvent.Execute(new ListCliCommandsParams());

        // Create the root command.
        var root = new RootCommand(description ?? "Tiferet CLI");

        // Group commands by GroupKey → subcommands.
        var groups = new Dictionary<string, Command>();

        foreach (var cmd in commands)
        {
            // Ensure the group command exists.
            if (!groups.TryGetValue(cmd.Domain.GroupKey, out var groupCmd))
            {
                groupCmd = new Command(cmd.Domain.GroupKey, $"{cmd.Domain.GroupKey} commands");
                root.AddCommand(groupCmd);
                groups[cmd.Domain.GroupKey] = groupCmd;
            }

            // Create the leaf command.
            var subCmd = new Command(cmd.Domain.Key, cmd.Domain.Description);

            // Map CLI arguments/options and build the handler.
            var argBindings = new List<(Argument<string> Arg, string Name)>();
            var optBindings = new List<(Option<string> Opt, string Name)>();

            if (cmd.Domain.Arguments is not null)
            {
                foreach (var cliArg in cmd.Domain.Arguments)
                {
                    MapArgument(cliArg, subCmd, argBindings, optBindings);
                }
            }

            // Wire the handler to dispatch feature execution.
            var featureId = cmd.Domain.Id;
            SetCommandHandler(subCmd, app, featureId, argBindings, optBindings);

            groupCmd.AddCommand(subCmd);
        }

        return root;
    }

    /// <summary>
    /// Convenience overload that bootstraps both the app and CLI in one step.
    /// </summary>
    /// <param name="interfaceId">The interface identifier for <see cref="AppBlueprint.BuildApp"/>.</param>
    /// <param name="configDir">The configuration directory.</param>
    /// <param name="cliConfigFile">Optional CLI config file (defaults to <c>{configDir}/cli.yml</c>).</param>
    /// <param name="description">Optional root command description.</param>
    /// <returns>A configured <see cref="RootCommand"/> ready for invocation.</returns>
    public static RootCommand BuildCli(
        string interfaceId,
        string configDir = AppBlueprint.DefaultConfigDir,
        string? cliConfigFile = null,
        string? description = null)
    {
        var app = AppBlueprint.BuildApp(interfaceId, configDir);
        var cliFile = cliConfigFile ?? Path.Combine(configDir, DefaultCliConfigFile);
        return BuildCli(app, cliFile, description);
    }

    /// <summary>
    /// Map a <see cref="CliArgumentConfiguration"/> to either a positional <see cref="Argument{T}"/>
    /// or a named <see cref="Option{T}"/>.
    /// </summary>
    /// <param name="cliArg">The CLI argument definition.</param>
    /// <param name="command">The System.CommandLine command to add to.</param>
    /// <param name="argBindings">Accumulator for positional argument bindings.</param>
    /// <param name="optBindings">Accumulator for option bindings.</param>
    private static void MapArgument(
        CliArgumentConfiguration cliArg,
        Command command,
        List<(Argument<string> Arg, string Name)> argBindings,
        List<(Option<string> Opt, string Name)> optBindings)
    {
        var firstName = cliArg.NameOrFlags[0];

        if (firstName.StartsWith("-"))
        {
            // Named option (e.g., -f, --file).
            var opt = new Option<string>(
                cliArg.NameOrFlags.ToArray(),
                cliArg.Description ?? "");

            if (cliArg.Default is not null)
                opt.SetDefaultValue(cliArg.Default);

            command.AddOption(opt);

            // Use the longest flag name, stripped of dashes, as the data key.
            var name = cliArg.NameOrFlags
                .OrderByDescending(f => f.Length)
                .First()
                .TrimStart('-');
            optBindings.Add((opt, name));
        }
        else
        {
            // Positional argument.
            var arg = new Argument<string>(firstName, cliArg.Description ?? "");

            if (cliArg.Default is not null)
                arg.SetDefaultValue(cliArg.Default);

            command.AddArgument(arg);
            argBindings.Add((arg, firstName));
        }
    }

    /// <summary>
    /// Set the command handler to collect parsed values and dispatch
    /// feature execution via <see cref="AppInterfaceContext.Run"/>.
    /// </summary>
    private static void SetCommandHandler(
        Command command,
        AppInterfaceContext app,
        string featureId,
        List<(Argument<string> Arg, string Name)> argBindings,
        List<(Option<string> Opt, string Name)> optBindings)
    {
        // Capture bindings for the closure.
        var capturedArgs = argBindings.ToList();
        var capturedOpts = optBindings.ToList();

        command.SetHandler((InvocationContext ctx) =>
        {
            // Collect parsed argument values.
            var data = new Dictionary<string, object?>();

            foreach (var (arg, name) in capturedArgs)
            {
                var value = ctx.ParseResult.GetValueForArgument(arg);
                data[name] = value;
            }

            foreach (var (opt, name) in capturedOpts)
            {
                var value = ctx.ParseResult.GetValueForOption(opt);
                if (value is not null)
                    data[name] = value;
            }

            try
            {
                // Dispatch to the feature pipeline.
                var result = app.Run(featureId, data: data);
                if (result is not null)
                    Console.WriteLine(result);
            }
            catch (TiferetApiException ex)
            {
                Console.Error.WriteLine($"ErrorConfiguration: {ex.Message}");
                ctx.ExitCode = 1;
            }
            catch (TiferetException ex)
            {
                Console.Error.WriteLine($"ErrorConfiguration: {ex.Message}");
                ctx.ExitCode = 1;
            }
        });
    }
}
