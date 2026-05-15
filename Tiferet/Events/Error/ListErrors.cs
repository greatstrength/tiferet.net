using Tiferet.Domain;
using Tiferet.Domain.Error;
using Tiferet.Interfaces;
using Tiferet.Mappers.Error;

namespace Tiferet.Events.Error;

public sealed record ListErrorsParams(bool IncludeDefaults = false);

public class ListErrors : DomainEvent<ListErrorsParams, IReadOnlyList<ErrorConfiguration>>
{
    private readonly IErrorService _errorService;
    public ListErrors(IErrorService errorService) => _errorService = errorService;

    public override IReadOnlyList<ErrorConfiguration> Execute(ListErrorsParams p)
    {
        if (!p.IncludeDefaults)
            return _errorService.List().Select(e => e.ToDomainObject()).ToList();

        // Merge defaults with repository errors (repo wins on conflicts).
        var errors = new Dictionary<string, ErrorConfiguration>();
        foreach (var (id, defaultError) in DefaultErrors.All)
        {
            errors[id] = defaultError;
        }
        foreach (var error in _errorService.List())
            errors[error.Id] = error.ToDomainObject();

        return errors.Values.ToList();
    }
}
