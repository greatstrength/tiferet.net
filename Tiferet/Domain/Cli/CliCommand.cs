namespace Tiferet.Domain.Cli;

/// <summary>
/// Represents a command line command.
/// </summary>
/// <param name="Id">The unique identifier (GroupKey.Key).</param>
/// <param name="Name">The name of the command.</param>
/// <param name="Key">A unique key for the command.</param>
/// <param name="GroupKey">A unique key for the command group.</param>
/// <param name="Description">A brief description of the command.</param>
/// <param name="Arguments">A list of arguments for the command.</param>
public sealed record CliCommandConfiguration(
    string Id,
    string Name,
    string Key,
    string GroupKey,
    string? Description = null,
    IReadOnlyList<CliArgumentConfiguration>? Arguments = null) : DomainObject
{
    /// <summary>
    /// Create a CliCommandConfiguration with derivation logic for Id from GroupKey and Key.
    /// </summary>
    public static CliCommandConfiguration Create(
        string name,
        string key,
        string groupKey,
        string? id = null,
        string? description = null,
        IReadOnlyList<CliArgumentConfiguration>? arguments = null)
    {
        // Derive id from groupKey and key, normalizing hyphens to underscores.
        id ??= $"{groupKey.Replace('-', '_')}.{key.Replace('-', '_')}";

        return new CliCommandConfiguration(
            Id: id,
            Name: name,
            Key: key,
            GroupKey: groupKey,
            Description: description,
            Arguments: arguments);
    }

    /// <summary>
    /// Check if the command has an argument with the given flags.
    /// </summary>
    public bool HasArgument(IReadOnlyList<string> flags)
    {
        if (Arguments is null) return false;

        foreach (var flag in flags)
        {
            foreach (var arg in Arguments)
            {
                if (arg.NameOrFlags.Contains(flag))
                    return true;
            }
        }

        return false;
    }
}
