using Tiferet.Domain;

namespace Tiferet.Mappers;

/// <summary>
/// Aggregate for mutable CLI command operations.
/// </summary>
public class CliCommandAggregate : Aggregate<CliCommand>
{
    public CliCommandAggregate(CliCommand domain) : base(domain) { }

    /// <summary>Rename the command.</summary>
    public void Rename(string name) => SetAttribute(nameof(CliCommand.Name), name);

    /// <summary>Update the command description.</summary>
    public void SetDescription(string? description) =>
        SetAttribute(nameof(CliCommand.Description), description);

    /// <summary>
    /// Add an argument to the command.
    /// </summary>
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
        var arg = new CliArgument(
            NameOrFlags: nameOrFlags,
            Description: description,
            Type: type,
            Required: required,
            Default: @default,
            Choices: choices,
            Nargs: nargs,
            Action: action);

        var args = new List<CliArgument>(Domain.Arguments ?? []) { arg };
        SetAttribute(nameof(CliCommand.Arguments), (IReadOnlyList<CliArgument>)args);
    }
}
