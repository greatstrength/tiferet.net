using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers.Error;

namespace Tiferet.Events.Error;

public sealed record ListErrorsParams(bool IncludeDefaults = false);

public class ListErrors : DomainEvent<ListErrorsParams, IReadOnlyList<ErrorAggregate>>
{
    private readonly IErrorService _errorService;
    public ListErrors(IErrorService errorService) => _errorService = errorService;

    public override IReadOnlyList<ErrorAggregate> Execute(ListErrorsParams p)
    {
        if (!p.IncludeDefaults)
            return _errorService.List();

        // Merge defaults with repository errors (repo wins on conflicts).
        var errors = new Dictionary<string, ErrorAggregate>();
        foreach (var (id, defaultError) in DefaultErrors.All)
        {
            errors[id] = ErrorAggregate.Create(defaultError.Id, defaultError.Name,
                defaultError.ErrorCode, defaultError.Description, defaultError.Messages);
        }
        foreach (var error in _errorService.List())
            errors[error.Id] = error;

        return errors.Values.ToList();
    }
}
