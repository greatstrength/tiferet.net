using System.ComponentModel.DataAnnotations;

namespace Tiferet.Domain.Logging;

/// <summary>
/// A logging formatter configuration.
/// </summary>
/// <param name="Id">The unique identifier of the formatter.</param>
/// <param name="Name">The name of the formatter.</param>
/// <param name="Format">The format string for log messages.</param>
/// <param name="Description">The description of the formatter.</param>
/// <param name="DateFormat">The date format for log timestamps.</param>
public sealed record FormatterConfiguration(
    [Required] string Id,
    [Required] string Name,
    [Required] string Format,
    string? Description = null,
    string? DateFormat = null) : DomainObject;
