using Tiferet.Domain;
using Tiferet.Interfaces;

namespace Tiferet.Events.Error;

public sealed record SetErrorMessageParams(string Id, string Message, string Lang = "en_US");

public class SetErrorMessage : DomainEvent<SetErrorMessageParams, string>
{
    private readonly IErrorService _errorService;
    public SetErrorMessage(IErrorService errorService) => _errorService = errorService;

    public override string Execute(SetErrorMessageParams p)
    {
        var error = VerifyNotNull(_errorService.Get(p.Id),
            ErrorCodes.ErrorNotFound, context: ("id", p.Id));

        error.SetMessage(p.Lang, p.Message);
        _errorService.Save(error);
        return p.Id;
    }
}
