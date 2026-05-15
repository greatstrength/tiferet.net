using Tiferet.Domain;
using Tiferet.Domain.Error;
using Tiferet.Interfaces;
using Tiferet.Mappers.Error;

namespace Tiferet.Events.Error;

public sealed record GetErrorParams(string Id, bool IncludeDefaults = false);

public class GetError : DomainEvent<GetErrorParams, ErrorConfiguration>
{
    private readonly IErrorService _errorService;
    public GetError(IErrorService errorService) => _errorService = errorService;

    public override ErrorConfiguration Execute(GetErrorParams p)
    {
        // Try the repository first.
        var error = _errorService.Get(p.Id);
        if (error is not null) return error.ToDomainObject();

        // Fall back to default errors if requested.
        if (p.IncludeDefaults)
        {
            var defaultError = DefaultErrors.Get(p.Id);
            if (defaultError is not null)
                return defaultError;
        }

        // Not found.
        RaiseError(ErrorCodes.ErrorNotFound, $"ErrorConfiguration not found: {p.Id}.", ("id", p.Id));
        return null!; // unreachable
    }
}
