using Microsoft.Extensions.Logging;
using Tiferet.Domain.Logging;
using Tiferet.Interfaces;
using Tiferet.Mappers.Logging;

namespace Tiferet.Events.Logging;

public sealed record AddLoggerParams(
    string Id, string Name, LogLevel Level, IReadOnlyList<string> HandlerIds,
    string? Description = null, bool Propagate = false, bool IsRoot = false);

public class AddLogger : DomainEvent<AddLoggerParams, LoggerConfiguration>
{
    private readonly ILoggingService _loggingService;
    public AddLogger(ILoggingService loggingService) => _loggingService = loggingService;

    public override LoggerConfiguration Execute(AddLoggerParams p)
    {
        var record = new LoggerConfiguration(p.Id, p.Name, p.Level, p.Description,
            p.HandlerIds, p.Propagate, p.IsRoot);
        var aggregate = new LoggerAggregate(record);
        _loggingService.SaveLogger(aggregate);
        return aggregate.ToDomainObject();
    }
}
