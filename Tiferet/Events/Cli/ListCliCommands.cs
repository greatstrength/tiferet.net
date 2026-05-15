using Tiferet.Interfaces;
using Tiferet.Mappers.Cli;

namespace Tiferet.Events.Cli;

public sealed record ListCliCommandsParams();

public class ListCliCommands : DomainEvent<ListCliCommandsParams, IReadOnlyList<CliCommandAggregate>>
{
    private readonly ICliService _cliService;
    public ListCliCommands(ICliService cliService) => _cliService = cliService;

    public override IReadOnlyList<CliCommandAggregate> Execute(ListCliCommandsParams p)
        => _cliService.List();
}
