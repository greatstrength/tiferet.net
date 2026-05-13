namespace Tiferet.Domain;

/// <summary>The data type of a CLI argument.</summary>
public enum CliArgumentType { String, Int, Float }

/// <summary>The action to take when a CLI argument is encountered.</summary>
public enum CliArgumentAction
{
    Store, StoreConst, StoreTrue, StoreFalse,
    Append, AppendConst, Count, Help, Version
}

/// <summary>
/// Represents a command line argument.
/// </summary>
/// <param name="NameOrFlags">The name or flags of the argument (e.g., ["-f", "--flag"]).</param>
/// <param name="Description">A brief description of the argument.</param>
/// <param name="Type">The type of the argument.</param>
/// <param name="Required">Whether the argument is required.</param>
/// <param name="Default">The default value if not provided.</param>
/// <param name="Choices">A list of valid choices for the argument.</param>
/// <param name="Nargs">The number of arguments to consume ("?", "*", "+").</param>
/// <param name="Action">The action to take when the argument is encountered.</param>
public sealed record CliArgument(
    IReadOnlyList<string> NameOrFlags,
    string? Description = null,
    CliArgumentType Type = CliArgumentType.String,
    bool? Required = null,
    string? Default = null,
    IReadOnlyList<string>? Choices = null,
    string? Nargs = null,
    CliArgumentAction? Action = null) : DomainObject;

/// <summary>
/// Represents a command line command.
/// </summary>
/// <param name="Id">The unique identifier (GroupKey.Key).</param>
/// <param name="Name">The name of the command.</param>
/// <param name="Key">A unique key for the command.</param>
/// <param name="GroupKey">A unique key for the command group.</param>
/// <param name="Description">A brief description of the command.</param>
/// <param name="Arguments">A list of arguments for the command.</param>
public sealed record CliCommand(
    string Id,
    string Name,
    string Key,
    string GroupKey,
    string? Description = null,
    IReadOnlyList<CliArgument>? Arguments = null) : DomainObject
{
    /// <summary>
    /// Create a CliCommand with derivation logic for Id from GroupKey and Key.
    /// </summary>
    /// <param name="name">The command name.</param>
    /// <param name="key">The command key.</param>
    /// <param name="groupKey">The command group key.</param>
    /// <param name="id">The full ID (optional, derived from groupKey.key).</param>
    /// <param name="description">The command description.</param>
    /// <param name="arguments">The command arguments.</param>
    /// <returns>A fully-formed CliCommand record.</returns>
    public static CliCommand Create(
        string name,
        string key,
        string groupKey,
        string? id = null,
        string? description = null,
        IReadOnlyList<CliArgument>? arguments = null)
    {
        // Derive id from groupKey and key, normalizing hyphens to underscores.
        id ??= $"{groupKey.Replace('-', '_')}.{key.Replace('-', '_')}";

        return new CliCommand(
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
    /// <param name="flags">The flags to check for.</param>
    /// <returns>True if the command has a matching argument.</returns>
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
