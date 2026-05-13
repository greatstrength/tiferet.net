using Tiferet.Core;
using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers;

namespace Tiferet.Events.Cli;

// *** Parameter records

public sealed record ListCliCommandsParams();

public sealed record GetParentArgumentsParams();

public sealed record AddCliCommandParams(
    string Name, string Key, string GroupKey,
    string? Id = null, string? Description = null,
    IReadOnlyList<CliArgument>? Arguments = null);

public sealed record AddCliArgumentParams(
    string CommandId, IReadOnlyList<string> NameOrFlags,
    string? Description = null, CliArgumentType Type = CliArgumentType.String,
    bool? Required = null, string? Default = null,
    IReadOnlyList<string>? Choices = null, string? Nargs = null,
    CliArgumentAction? Action = null);

// *** Events

public class ListCliCommands : DomainEvent<ListCliCommandsParams, IReadOnlyList<CliCommandAggregate>>
{
    private readonly ICliService _cliService;
    public ListCliCommands(ICliService cliService) => _cliService = cliService;

    public override IReadOnlyList<CliCommandAggregate> Execute(ListCliCommandsParams p)
        => _cliService.List();
}

public class GetParentArguments : DomainEvent<GetParentArgumentsParams, IReadOnlyList<CliArgument>>
{
    private readonly ICliService _cliService;
    public GetParentArguments(ICliService cliService) => _cliService = cliService;

    public override IReadOnlyList<CliArgument> Execute(GetParentArgumentsParams p)
        => _cliService.GetParentArguments();
}

public class AddCliCommand : DomainEvent<AddCliCommandParams, CliCommandAggregate>
{
    private readonly ICliService _cliService;
    public AddCliCommand(ICliService cliService) => _cliService = cliService;

    public override CliCommandAggregate Execute(AddCliCommandParams p)
    {
        var command = CliCommand.Create(
            name: p.Name, key: p.Key, groupKey: p.GroupKey,
            id: p.Id, description: p.Description, arguments: p.Arguments);

        Verify(!_cliService.Exists(command.Id),
            ErrorCodes.CliCommandAlreadyExists, null, ("id", command.Id));

        var aggregate = new CliCommandAggregate(command);
        _cliService.Save(aggregate);
        return aggregate;
    }
}

public class AddCliArgument : DomainEvent<AddCliArgumentParams, string>
{
    private readonly ICliService _cliService;
    public AddCliArgument(ICliService cliService) => _cliService = cliService;

    public override string Execute(AddCliArgumentParams p)
    {
        var command = _cliService.Get(p.CommandId);
        Verify(command is not null, ErrorCodes.CliCommandNotFound, null, ("commandId", p.CommandId));

        command!.AddArgument(p.NameOrFlags, p.Description, p.Type,
            p.Required, p.Default, p.Choices, p.Nargs, p.Action);
        _cliService.Save(command);
        return p.CommandId;
    }
}
