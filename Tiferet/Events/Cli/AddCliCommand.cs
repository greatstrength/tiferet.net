using Tiferet.Domain;
using Tiferet.Domain.Cli;
using Tiferet.Interfaces;
using Tiferet.Mappers.Cli;

namespace Tiferet.Events.Cli;

public sealed record AddCliCommandParams(
    string Name, string Key, string GroupKey,
    string? Id = null, string? Description = null,
    IReadOnlyList<CliArgumentConfiguration>? Arguments = null);

public class AddCliCommand : DomainEvent<AddCliCommandParams, CliCommandConfiguration>
{
    private readonly ICliService _cliService;
    public AddCliCommand(ICliService cliService) => _cliService = cliService;

    public override CliCommandConfiguration Execute(AddCliCommandParams p)
    {
        var aggregate = CliCommandAggregate.Create(
            name: p.Name, key: p.Key, groupKey: p.GroupKey,
            id: p.Id, description: p.Description, arguments: p.Arguments);

        VerifyNotExists(_cliService.Exists(aggregate.Id),
            ErrorCodes.CliCommandAlreadyExists, context: ("id", aggregate.Id));

        _cliService.Save(aggregate);
        return aggregate.ToDomainObject();
    }
}
