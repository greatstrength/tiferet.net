using Tiferet.Domain;
using Tiferet.Domain.Error;
using Tiferet.Interfaces;

namespace Tiferet.Events.Error;

public sealed record RenameErrorParams(string Id, string NewName);

public class RenameError : DomainEvent<RenameErrorParams, ErrorConfiguration>
{
    private readonly IErrorService _errorService;
    public RenameError(IErrorService errorService) => _errorService = errorService;

    public override ErrorConfiguration Execute(RenameErrorParams p)
    {
        var error = VerifyNotNull(_errorService.Get(p.Id),
            ErrorCodes.ErrorNotFound, context: ("id", p.Id));

        error.Rename(p.NewName);
        _errorService.Save(error);
        return error.ToDomainObject();
    }
}
