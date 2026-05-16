using Tiferet.Domain.Logging;
using Tiferet.Interfaces;

namespace Tiferet.Events.Logging;

public sealed record ListAllLoggingConfigsParams();

public class ListAllLoggingConfigs : DomainEvent<ListAllLoggingConfigsParams,
    (IReadOnlyList<FormatterConfiguration> Formatters, IReadOnlyList<HandlerConfiguration> Handlers, IReadOnlyList<LoggerConfiguration> Loggers)>
{
    private readonly ILoggingService _loggingService;
    public ListAllLoggingConfigs(ILoggingService loggingService) => _loggingService = loggingService;

    public override (IReadOnlyList<FormatterConfiguration>, IReadOnlyList<HandlerConfiguration>, IReadOnlyList<LoggerConfiguration>)
        Execute(ListAllLoggingConfigsParams p)
    {
        var (formatters, handlers, loggers) = _loggingService.ListAll();
        return (
            formatters.Select(f => f.ToDomainObject()).ToList(),
            handlers.Select(h => h.ToDomainObject()).ToList(),
            loggers.Select(l => l.ToDomainObject()).ToList());
    }
}
