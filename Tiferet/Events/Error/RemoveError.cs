using Tiferet.Interfaces;

namespace Tiferet.Events.Error;

public sealed record RemoveErrorParams(string Id);

public class RemoveError : DomainEvent<RemoveErrorParams, string>
{
    private readonly IErrorService _errorService;
    public RemoveError(IErrorService errorService) => _errorService = errorService;

    public override string Execute(RemoveErrorParams p)
    {
        _errorService.Delete(p.Id);
        return p.Id;
    }
}
