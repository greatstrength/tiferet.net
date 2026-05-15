using Tiferet.Domain;
using Tiferet.Domain.Cli;

namespace Tiferet.Mappers.Cli;

/// <summary>Aggregate for mutable CLI command operations.</summary>
public record CliCommandAggregate : Aggregate<CliCommandConfiguration>
{
    public CliCommandAggregate(CliCommandConfiguration state) : base(state) { }

    // Delegated properties.
    public string Id => State.Id;
    public string Name => State.Name;
    public string Key => State.Key;
    public string GroupKey => State.GroupKey;
    public string? Description => State.Description;
    public IReadOnlyList<CliArgumentConfiguration>? Arguments => State.Arguments;

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
        var record = new CliCommandConfiguration(
            Id: id,
            Name: name,
            Key: key,
            GroupKey: groupKey,
            Description: description,
            Arguments: arguments);

        // Validate — throws TiferetDomainException on failure.
        DomainObject.Validate(record);

        // Return the aggregate wrapping the validated record.
        return new CliCommandAggregate(record);
    }

    public void Rename(string name) => Mutate(s => s with { Name = name });
    public void SetDescription(string? description) => Mutate(s => s with { Description = description });

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

        var args = new List<CliArgumentConfiguration>(Arguments ?? []) { arg };
        Mutate(s => s with { Arguments = args });
    }

    /// <summary>
    /// Check if the command has an argument with the given flags.
    /// Delegates to the domain record.
    /// </summary>
    public bool HasArgument(IReadOnlyList<string> flags)
        => State.HasArgument(flags);
}
