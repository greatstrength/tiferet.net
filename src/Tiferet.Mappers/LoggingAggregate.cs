using Tiferet.Domain;

namespace Tiferet.Mappers;

/// <summary>Aggregate for logging formatter configuration.</summary>
public class FormatterAggregate : Aggregate<Formatter>
{
    public FormatterAggregate(Formatter domain) : base(domain) { }
}

/// <summary>Aggregate for logging handler configuration.</summary>
public class HandlerAggregate : Aggregate<Handler>
{
    public HandlerAggregate(Handler domain) : base(domain) { }
}

/// <summary>Aggregate for logger configuration.</summary>
public class LoggerAggregate : Aggregate<Logger>
{
    public LoggerAggregate(Logger domain) : base(domain) { }
}
