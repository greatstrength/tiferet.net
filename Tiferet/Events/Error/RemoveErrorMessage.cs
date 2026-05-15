using Tiferet.Assets;
using Tiferet.Interfaces;

namespace Tiferet.Events.Error;

public sealed record RemoveErrorMessageParams(string Id, string Lang = "en_US");

public class RemoveErrorMessage : DomainEvent<RemoveErrorMessageParams, string>
{
    private readonly IErrorService _errorService;
    public RemoveErrorMessage(IErrorService errorService) => _errorService = errorService;

    public override string Execute(RemoveErrorMessageParams p)
    {
        var error = _errorService.Get(p.Id);
        Verify(error is not null, ErrorCodes.ErrorNotFound, null, ("id", p.Id));

        error!.RemoveMessage(p.Lang);

        Verify((error.Domain.Messages?.Count ?? 0) > 0,
            ErrorCodes.NoErrorMessages,
            $"No error messages remain for error ID {p.Id}.", ("id", p.Id));

        _errorService.Save(error);
        return p.Id;
    }
}
