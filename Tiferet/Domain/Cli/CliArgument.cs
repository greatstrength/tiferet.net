namespace Tiferet.Domain.Cli;

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
public sealed record CliArgumentConfiguration(
    IReadOnlyList<string> NameOrFlags,
    string? Description = null,
    CliArgumentType Type = CliArgumentType.String,
    bool? Required = null,
    string? Default = null,
    IReadOnlyList<string>? Choices = null,
    string? Nargs = null,
    CliArgumentAction? Action = null) : DomainObject;
