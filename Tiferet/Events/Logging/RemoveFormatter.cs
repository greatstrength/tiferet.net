using Tiferet.Interfaces;

namespace Tiferet.Events.Logging;

public sealed record RemoveFormatterParams(string Id);

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
