using Tiferet.Domain.Logging;

namespace Tiferet.Mappers.Logging;

/// <summary>Aggregate for logging formatter configuration.</summary>
public class FormatterAggregate : Aggregate<FormatterConfiguration>
{
    public FormatterAggregate(FormatterConfiguration domain) : base(domain) { }
}

/// <summary>Aggregate for logging handler configuration.</summary>
public class HandlerAggregate : Aggregate<HandlerConfiguration>
{
    public HandlerAggregate(HandlerConfiguration domain) : base(domain) { }
}

/// <summary>Aggregate for logger configuration.</summary>
public class LoggerAggregate : Aggregate<LoggerConfiguration>
{
    public LoggerAggregate(LoggerConfiguration domain) : base(domain) { }
}
