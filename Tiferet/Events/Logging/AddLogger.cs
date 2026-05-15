using Microsoft.Extensions.Logging;
using Tiferet.Domain.Logging;
using Tiferet.Interfaces;
using Tiferet.Mappers.Logging;

namespace Tiferet.Events.Logging;

public sealed record AddLoggerParams(
    string Id, string Name, LogLevel Level, IReadOnlyList<string> HandlerIds,
    string? Description = null, bool Propagate = false, bool IsRoot = false);

public class AddLogger : DomainEvent<AddLoggerParams, LoggerAggregate>
{
    private readonly ILoggingService _loggingService;
    public AddLogger(ILoggingService loggingService) => _loggingService = loggingService;

    public override LoggerAggregate Execute(AddLoggerParams p)
    {
        var domain = new LoggerConfiguration(p.Id, p.Name, p.Level, p.Description,
            p.HandlerIds, p.Propagate, p.IsRoot);
        var aggregate = new LoggerAggregate(domain);
        _loggingService.SaveLogger(aggregate);
        return aggregate;
    }
}
