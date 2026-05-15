using Tiferet.Domain;
using Tiferet.Domain.Cli;

namespace Tiferet.Mappers.Cli;

/// <summary>Aggregate for mutable CLI command operations.</summary>
public class CliCommandAggregate : Aggregate<CliCommandConfiguration>
{
    public CliCommandAggregate(CliCommandConfiguration domain) : base(domain) { }

    /// <summary>
    /// Create a new CliCommandAggregate, deriving Id from GroupKey and Key when not provided.
    /// Validates the resulting domain record and throws on failure.
    /// </summary>
    public static CliCommandAggregate Create(
        string name,
        string key,
        string groupKey,
        string? id = null,
        string? description = null,
        IReadOnlyList<CliArgumentConfiguration>? arguments = null)
    {
        // Derive id from groupKey and key, normalizing hyphens to underscores.
        id ??= $"{groupKey.Replace('-', '_')}.{key.Replace('-', '_')}";

        // Construct the domain record.
        var instance = new CliCommandConfiguration(
            Id: id,
            Name: name,
            Key: key,
            GroupKey: groupKey,
            Description: description,
            Arguments: arguments);

        // Validate — throws TiferetDomainException on failure.
        DomainObject.Validate(instance);

        // Return the constructed aggregate.
        return new CliCommandAggregate(instance);
    }

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
