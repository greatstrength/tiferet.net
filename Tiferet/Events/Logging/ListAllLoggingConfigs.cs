using Tiferet.Interfaces;
using Tiferet.Mappers.Logging;

namespace Tiferet.Events.Logging;

public sealed record ListAllLoggingConfigsParams();

public class ListAllLoggingConfigs : DomainEvent<ListAllLoggingConfigsParams,
    (IReadOnlyList<FormatterAggregate> Formatters, IReadOnlyList<HandlerAggregate> Handlers, IReadOnlyList<LoggerAggregate> Loggers)>
{
    private readonly ILoggingService _loggingService;
    public ListAllLoggingConfigs(ILoggingService loggingService) => _loggingService = loggingService;

    public override (IReadOnlyList<FormatterAggregate>, IReadOnlyList<HandlerAggregate>, IReadOnlyList<LoggerAggregate>)
        Execute(ListAllLoggingConfigsParams p) => _loggingService.ListAll();
}
