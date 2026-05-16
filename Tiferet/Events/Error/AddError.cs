using Tiferet.Domain;
using Tiferet.Domain.Error;
using Tiferet.Interfaces;
using Tiferet.Mappers.Error;

namespace Tiferet.Events.Error;

public sealed record AddErrorParams(
    string Id, string Name, string Message,
    string Lang = "en_US",
    IReadOnlyList<ErrorMessageConfiguration>? AdditionalMessages = null);

public class AddError : DomainEvent<AddErrorParams, ErrorConfiguration>
{
    private readonly IErrorService _errorService;
    public AddError(IErrorService errorService) => _errorService = errorService;

    public override ErrorConfiguration Execute(AddErrorParams p)
    {
        VerifyNotExists(_errorService.Exists(p.Id), ErrorCodes.ErrorAlreadyExists,
            $"An error with ID {p.Id} already exists.", ("id", p.Id));

        var messages = new List<ErrorMessageConfiguration> { new(p.Lang, p.Message) };
        if (p.AdditionalMessages is not null)
            messages.AddRange(p.AdditionalMessages);

        var aggregate = ErrorAggregate.Create(id: p.Id, name: p.Name, messages: messages);
        _errorService.Save(aggregate);
        return aggregate.ToDomainObject();
    }
}
