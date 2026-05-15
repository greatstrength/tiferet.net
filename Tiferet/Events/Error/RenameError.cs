using Tiferet.Assets;
using Tiferet.Interfaces;
using Tiferet.Mappers.Error;

namespace Tiferet.Events.Error;

public sealed record RenameErrorParams(string Id, string NewName);

public class RenameError : DomainEvent<RenameErrorParams, ErrorAggregate>
{
    private readonly IErrorService _errorService;
    public RenameError(IErrorService errorService) => _errorService = errorService;

    public override ErrorAggregate Execute(RenameErrorParams p)
    {
        var error = _errorService.Get(p.Id);
        Verify(error is not null, ErrorCodes.ErrorNotFound, null, ("id", p.Id));

        error!.Rename(p.NewName);
        _errorService.Save(error);
        return error;
    }
}
