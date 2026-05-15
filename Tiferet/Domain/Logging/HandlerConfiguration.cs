using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace Tiferet.Domain.Logging;

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
public sealed record HandlerConfiguration(
    [Required] string Id,
    [Required] string Name,
    [Required] string AssemblyName,
    [Required] string TypeName,
    LogLevel Level,
    [Required] string FormatterId,
    string? Description = null,
    string? Stream = null,
    string? Filename = null) : DomainObject;
