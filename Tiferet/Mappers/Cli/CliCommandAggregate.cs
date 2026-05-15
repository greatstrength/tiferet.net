using Tiferet.Domain.Cli;

namespace Tiferet.Mappers.Cli;

/// <summary>Aggregate for mutable CLI command operations.</summary>
public class CliCommandAggregate : Aggregate<CliCommandConfiguration>
{
    public CliCommandAggregate(CliCommandConfiguration domain) : base(domain) { }

    public void Rename(string name) => SetAttribute(nameof(CliCommandConfiguration.Name), name);
    public void SetDescription(string? description) => SetAttribute(nameof(CliCommandConfiguration.Description), description);

    public void AddArgument(
        IReadOnlyList<string> nameOrFlags,
        string? description = null,
        CliArgumentType type = CliArgumentType.String,
        bool? required = null,
        string? @default = null,
        IReadOnlyList<string>? choices = null,
        string? nargs = null,
        CliArgumentAction? action = null)
    {
        var arg = new CliArgumentConfiguration(
            NameOrFlags: nameOrFlags, Description: description, Type: type,
            Required: required, Default: @default, Choices: choices, Nargs: nargs, Action: action);

        var args = new List<CliArgumentConfiguration>(Domain.Arguments ?? []) { arg };
        SetAttribute(nameof(CliCommandConfiguration.Arguments), (IReadOnlyList<CliArgumentConfiguration>)args);
    }
}
