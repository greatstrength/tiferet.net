using Tiferet.Domain.Logging;
using Tiferet.Interfaces;
using Tiferet.Mappers.Logging;

namespace Tiferet.Events.Logging;

public sealed record AddFormatterParams(
    string Id, string Name, string Format,
    string? Description = null, string? DateFormat = null);

public class AddFormatter : DomainEvent<AddFormatterParams, FormatterAggregate>
{
    private readonly ILoggingService _loggingService;
    public AddFormatter(ILoggingService loggingService) => _loggingService = loggingService;

    public override FormatterAggregate Execute(AddFormatterParams p)
    {
        var record = new FormatterConfiguration(p.Id, p.Name, p.Format, p.Description, p.DateFormat);
        var aggregate = new FormatterAggregate(record);
        _loggingService.SaveFormatter(aggregate);
        return aggregate;
    }
}
