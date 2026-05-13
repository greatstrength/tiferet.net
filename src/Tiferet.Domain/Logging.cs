using Microsoft.Extensions.Logging;

namespace Tiferet.Domain;

/// <summary>
/// A logging formatter configuration.
/// </summary>
/// <param name="Id">The unique identifier of the formatter.</param>
/// <param name="Name">The name of the formatter.</param>
/// <param name="Format">The format string for log messages.</param>
/// <param name="Description">The description of the formatter.</param>
/// <param name="DateFormat">The date format for log timestamps.</param>
public sealed record Formatter(
    string Id,
    string Name,
    string Format,
    string? Description = null,
    string? DateFormat = null) : DomainObject;

/// <summary>
/// A logging handler configuration.
/// </summary>
/// <param name="Id">The unique identifier of the handler.</param>
/// <param name="Name">The name of the handler.</param>
/// <param name="AssemblyName">The assembly name for the handler class.</param>
/// <param name="TypeName">The type name of the handler class.</param>
/// <param name="Level">The logging level for the handler.</param>
/// <param name="FormatterId">The ID of the formatter to use.</param>
/// <param name="Description">The description of the handler.</param>
/// <param name="Stream">The stream for StreamHandler (e.g., "ext://sys.stdout").</param>
/// <param name="Filename">The file path for FileHandler.</param>
public sealed record Handler(
    string Id,
    string Name,
    string AssemblyName,
    string TypeName,
    LogLevel Level,
    string FormatterId,
    string? Description = null,
    string? Stream = null,
    string? Filename = null) : DomainObject;

/// <summary>
/// A logger configuration.
/// </summary>
/// <param name="Id">The unique identifier of the logger.</param>
/// <param name="Name">The name of the logger.</param>
/// <param name="Level">The logging level for the logger.</param>
/// <param name="Description">The description of the logger.</param>
/// <param name="HandlerIds">List of handler IDs for the logger.</param>
/// <param name="Propagate">Whether to propagate messages to parent loggers.</param>
/// <param name="IsRoot">Whether this is the root logger.</param>
public sealed record Logger(
    string Id,
    string Name,
    LogLevel Level,
    string? Description = null,
    IReadOnlyList<string>? HandlerIds = null,
    bool Propagate = false,
    bool IsRoot = false) : DomainObject;
