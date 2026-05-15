using Tiferet.Assets;
using Tiferet.Interfaces;

namespace Tiferet.Events.Error;

public sealed record SetErrorMessageParams(string Id, string Message, string Lang = "en_US");

public class SetErrorMessage : DomainEvent<SetErrorMessageParams, string>
{
    private readonly IErrorService _errorService;
    public SetErrorMessage(IErrorService errorService) => _errorService = errorService;

    public override string Execute(SetErrorMessageParams p)
    {
        var error = _errorService.Get(p.Id);
        Verify(error is not null, ErrorCodes.ErrorNotFound, null, ("id", p.Id));

        error!.SetMessage(p.Lang, p.Message);
        _errorService.Save(error);
        return p.Id;
    }
}
