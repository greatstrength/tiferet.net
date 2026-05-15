using Tiferet.Interfaces;

namespace Tiferet.Events.Logging;

public sealed record RemoveHandlerParams(string Id);

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
