using Tiferet.Domain.Cli;
using Tiferet.Interfaces;

namespace Tiferet.Events.Cli;

public sealed record GetParentArgumentsParams();

public class GetParentArguments : DomainEvent<GetParentArgumentsParams, IReadOnlyList<CliArgumentConfiguration>>
{
    private readonly ICliService _cliService;
    public GetParentArguments(ICliService cliService) => _cliService = cliService;

    public override IReadOnlyList<CliArgumentConfiguration> Execute(GetParentArgumentsParams p)
        => _cliService.GetParentArguments();
}
