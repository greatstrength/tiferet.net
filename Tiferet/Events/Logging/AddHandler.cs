using Microsoft.Extensions.Logging;
using Tiferet.Domain.Logging;
using Tiferet.Interfaces;
using Tiferet.Mappers.Logging;

namespace Tiferet.Events.Logging;

public sealed record AddHandlerParams(
    string Id, string Name, string AssemblyName, string TypeName,
    LogLevel Level, string FormatterId,
    string? Description = null, string? Stream = null, string? Filename = null);

public class AddHandler : DomainEvent<AddHandlerParams, HandlerAggregate>
{
    private readonly ILoggingService _loggingService;
    public AddHandler(ILoggingService loggingService) => _loggingService = loggingService;

    public override HandlerAggregate Execute(AddHandlerParams p)
    {
        var record = new HandlerConfiguration(p.Id, p.Name, p.AssemblyName, p.TypeName,
            p.Level, p.FormatterId, p.Description, p.Stream, p.Filename);
        var aggregate = new HandlerAggregate(record);
        _loggingService.SaveHandler(aggregate);
        return aggregate;
    }
}
