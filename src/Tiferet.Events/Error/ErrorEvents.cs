using Tiferet.Core;
using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers;

namespace Tiferet.Events.Error;

// *** Parameter records

public sealed record AddErrorParams(
    string Id, string Name, string Message,
    string Lang = "en_US",
    IReadOnlyList<ErrorMessage>? AdditionalMessages = null);

public sealed record GetErrorParams(string Id, bool IncludeDefaults = false);

public sealed record ListErrorsParams(bool IncludeDefaults = false);

public sealed record RenameErrorParams(string Id, string NewName);

public sealed record SetErrorMessageParams(string Id, string Message, string Lang = "en_US");

public sealed record RemoveErrorMessageParams(string Id, string Lang = "en_US");

public sealed record RemoveErrorParams(string Id);

// *** Events

public class AddError : DomainEvent<AddErrorParams, ErrorAggregate>
{
    private readonly IErrorService _errorService;
    public AddError(IErrorService errorService) => _errorService = errorService;

    public override ErrorAggregate Execute(AddErrorParams p)
    {
        Verify(!_errorService.Exists(p.Id), ErrorCodes.ErrorAlreadyExists,
            $"An error with ID {p.Id} already exists.", ("id", p.Id));

        var messages = new List<ErrorMessage> { new(p.Lang, p.Message) };
        if (p.AdditionalMessages is not null)
            messages.AddRange(p.AdditionalMessages);

        var error = Domain.Error.Create(id: p.Id, name: p.Name, messages: messages);
        var aggregate = new ErrorAggregate(error);
        _errorService.Save(aggregate);
        return aggregate;
    }
}

public class GetError : DomainEvent<GetErrorParams, ErrorAggregate>
{
    private readonly IErrorService _errorService;
    public GetError(IErrorService errorService) => _errorService = errorService;

    public override ErrorAggregate Execute(GetErrorParams p)
    {
        // Try the repository first.
        var error = _errorService.Get(p.Id);
        if (error is not null) return error;

        // Fall back to default errors if requested.
        if (p.IncludeDefaults)
        {
            var data = DefaultErrors.Get(p.Id);
            if (data is not null)
            {
                var domain = DomainObject.FromDictionary<Domain.Error>(data);
                return new ErrorAggregate(domain);
            }
        }

        // Not found.
        RaiseError(ErrorCodes.ErrorNotFound,
            $"Error not found: {p.Id}.", ("id", p.Id));
        return null!; // unreachable
    }
}

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
        foreach (var (id, data) in DefaultErrors.All)
        {
            var domain = DomainObject.FromDictionary<Domain.Error>(data);
            errors[id] = new ErrorAggregate(domain);
        }
        foreach (var error in _errorService.List())
            errors[error.Domain.Id] = error;

        return errors.Values.ToList();
    }
}

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
