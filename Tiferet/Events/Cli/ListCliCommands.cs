using Tiferet.Domain.Cli;
using Tiferet.Interfaces;

namespace Tiferet.Events.Cli;

public sealed record ListCliCommandsParams();

public class ListCliCommands : DomainEvent<ListCliCommandsParams, IReadOnlyList<CliCommandConfiguration>>
{
    private readonly ICliService _cliService;
    public ListCliCommands(ICliService cliService) => _cliService = cliService;

    public override IReadOnlyList<CliCommandConfiguration> Execute(ListCliCommandsParams p)
        => _cliService.List().Select(c => c.ToDomainObject()).ToList();
}
