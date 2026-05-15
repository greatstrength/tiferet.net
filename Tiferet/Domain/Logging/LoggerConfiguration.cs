using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace Tiferet.Domain.Logging;

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
public sealed record LoggerConfiguration(
    [Required] string Id,
    [Required] string Name,
    LogLevel Level,
    string? Description = null,
    IReadOnlyList<string>? HandlerIds = null,
    bool Propagate = false,
    bool IsRoot = false) : DomainObject;
