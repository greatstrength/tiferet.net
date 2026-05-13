using Microsoft.Extensions.Logging;
using Tiferet.Core;
using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers;

namespace Tiferet.Events.Logging;

// *** Parameter records

public sealed record ListAllLoggingConfigsParams();

public sealed record AddFormatterParams(
    string Id, string Name, string Format,
    string? Description = null, string? DateFormat = null);

public sealed record RemoveFormatterParams(string Id);

public sealed record AddHandlerParams(
    string Id, string Name, string AssemblyName, string TypeName,
    LogLevel Level, string FormatterId,
    string? Description = null, string? Stream = null, string? Filename = null);

public sealed record RemoveHandlerParams(string Id);

public sealed record AddLoggerParams(
    string Id, string Name, LogLevel Level, IReadOnlyList<string> HandlerIds,
    string? Description = null, bool Propagate = false, bool IsRoot = false);

public sealed record RemoveLoggerParams(string Id);

// *** Events

public class ListAllLoggingConfigs : DomainEvent<ListAllLoggingConfigsParams,
    (IReadOnlyList<FormatterAggregate> Formatters, IReadOnlyList<HandlerAggregate> Handlers, IReadOnlyList<LoggerAggregate> Loggers)>
{
    private readonly ILoggingService _loggingService;
    public ListAllLoggingConfigs(ILoggingService loggingService) => _loggingService = loggingService;

    public override (IReadOnlyList<FormatterAggregate>, IReadOnlyList<HandlerAggregate>, IReadOnlyList<LoggerAggregate>)
        Execute(ListAllLoggingConfigsParams p) => _loggingService.ListAll();
}

public class AddFormatter : DomainEvent<AddFormatterParams, FormatterAggregate>
{
    private readonly ILoggingService _loggingService;
    public AddFormatter(ILoggingService loggingService) => _loggingService = loggingService;

    public override FormatterAggregate Execute(AddFormatterParams p)
    {
        var domain = new Formatter(p.Id, p.Name, p.Format, p.Description, p.DateFormat);
        var aggregate = new FormatterAggregate(domain);
        _loggingService.SaveFormatter(aggregate);
        return aggregate;
    }
}

public class RemoveFormatter : DomainEvent<RemoveFormatterParams, string>
{
    private readonly ILoggingService _loggingService;
    public RemoveFormatter(ILoggingService loggingService) => _loggingService = loggingService;

    public override string Execute(RemoveFormatterParams p)
    {
        _loggingService.DeleteFormatter(p.Id);
        return p.Id;
    }
}

public class AddHandler : DomainEvent<AddHandlerParams, HandlerAggregate>
{
    private readonly ILoggingService _loggingService;
    public AddHandler(ILoggingService loggingService) => _loggingService = loggingService;

    public override HandlerAggregate Execute(AddHandlerParams p)
    {
        var domain = new Handler(p.Id, p.Name, p.AssemblyName, p.TypeName,
            p.Level, p.FormatterId, p.Description, p.Stream, p.Filename);
        var aggregate = new HandlerAggregate(domain);
        _loggingService.SaveHandler(aggregate);
        return aggregate;
    }
}

public class RemoveHandler : DomainEvent<RemoveHandlerParams, string>
{
    private readonly ILoggingService _loggingService;
    public RemoveHandler(ILoggingService loggingService) => _loggingService = loggingService;

    public override string Execute(RemoveHandlerParams p)
    {
        _loggingService.DeleteHandler(p.Id);
        return p.Id;
    }
}

public class AddLogger : DomainEvent<AddLoggerParams, LoggerAggregate>
{
    private readonly ILoggingService _loggingService;
    public AddLogger(ILoggingService loggingService) => _loggingService = loggingService;

    public override LoggerAggregate Execute(AddLoggerParams p)
    {
        var domain = new Logger(p.Id, p.Name, p.Level, p.Description,
            p.HandlerIds, p.Propagate, p.IsRoot);
        var aggregate = new LoggerAggregate(domain);
        _loggingService.SaveLogger(aggregate);
        return aggregate;
    }
}

public class RemoveLogger : DomainEvent<RemoveLoggerParams, string>
{
    private readonly ILoggingService _loggingService;
    public RemoveLogger(ILoggingService loggingService) => _loggingService = loggingService;

    public override string Execute(RemoveLoggerParams p)
    {
        _loggingService.DeleteLogger(p.Id);
        return p.Id;
    }
}
