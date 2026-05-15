using Microsoft.Extensions.Logging;
using Tiferet.Domain.Logging;

namespace Tiferet.Mappers.Logging;

/// <summary>Aggregate for logging formatter configuration.</summary>
public record FormatterAggregate : Aggregate<FormatterConfiguration>
{
    public FormatterAggregate(FormatterConfiguration state) : base(state) { }

    // Delegated properties.
    public string Id => State.Id;
    public string Name => State.Name;
    public string Format => State.Format;
    public string? Description => State.Description;
    public string? DateFormat => State.DateFormat;
}

/// <summary>Aggregate for logging handler configuration.</summary>
public record HandlerAggregate : Aggregate<HandlerConfiguration>
{
    public HandlerAggregate(HandlerConfiguration state) : base(state) { }

    // Delegated properties.
    public string Id => State.Id;
    public string Name => State.Name;
    public string AssemblyName => State.AssemblyName;
    public string TypeName => State.TypeName;
    public LogLevel Level => State.Level;
    public string FormatterId => State.FormatterId;
    public string? Description => State.Description;
    public string? Stream => State.Stream;
    public string? Filename => State.Filename;
}

/// <summary>Aggregate for logger configuration.</summary>
public record LoggerAggregate : Aggregate<LoggerConfiguration>
{
    public LoggerAggregate(LoggerConfiguration state) : base(state) { }

    // Delegated properties.
    public string Id => State.Id;
    public string Name => State.Name;
    public LogLevel Level => State.Level;
    public string? Description => State.Description;
    public IReadOnlyList<string>? HandlerIds => State.HandlerIds;
    public bool Propagate => State.Propagate;
    public bool IsRoot => State.IsRoot;
}
