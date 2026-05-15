using Tiferet.Interfaces;

namespace Tiferet.Events.Logging;

public sealed record RemoveLoggerParams(string Id);

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
