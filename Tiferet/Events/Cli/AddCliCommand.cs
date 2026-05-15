using Tiferet.Assets;
using Tiferet.Domain.Cli;
using Tiferet.Interfaces;
using Tiferet.Mappers.Cli;

namespace Tiferet.Events.Cli;

public sealed record AddCliCommandParams(
    string Name, string Key, string GroupKey,
    string? Id = null, string? Description = null,
    IReadOnlyList<CliArgumentConfiguration>? Arguments = null);

public class AddCliCommand : DomainEvent<AddCliCommandParams, CliCommandAggregate>
{
    private readonly ICliService _cliService;
    public AddCliCommand(ICliService cliService) => _cliService = cliService;

    public override CliCommandAggregate Execute(AddCliCommandParams p)
    {
        var command = CliCommandConfiguration.Create(
            name: p.Name, key: p.Key, groupKey: p.GroupKey,
            id: p.Id, description: p.Description, arguments: p.Arguments);

        Verify(!_cliService.Exists(command.Id),
            ErrorCodes.CliCommandAlreadyExists, null, ("id", command.Id));

        var aggregate = new CliCommandAggregate(command);
        _cliService.Save(aggregate);
        return aggregate;
    }
}
