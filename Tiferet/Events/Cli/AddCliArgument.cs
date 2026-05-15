using Tiferet.Domain;
using Tiferet.Domain.Cli;
using Tiferet.Interfaces;

namespace Tiferet.Events.Cli;

public sealed record AddCliArgumentParams(
    string CommandId, IReadOnlyList<string> NameOrFlags,
    string? Description = null, CliArgumentType Type = CliArgumentType.String,
    bool? Required = null, string? Default = null,
    IReadOnlyList<string>? Choices = null, string? Nargs = null,
    CliArgumentAction? Action = null);

public class AddCliArgument : DomainEvent<AddCliArgumentParams, string>
{
    private readonly ICliService _cliService;
    public AddCliArgument(ICliService cliService) => _cliService = cliService;

    public override string Execute(AddCliArgumentParams p)
    {
        var command = VerifyNotNull(_cliService.Get(p.CommandId),
            ErrorCodes.CliCommandNotFound, context: ("commandId", p.CommandId));

        command.AddArgument(p.NameOrFlags, p.Description, p.Type,
            p.Required, p.Default, p.Choices, p.Nargs, p.Action);
        _cliService.Save(command);
        return p.CommandId;
    }
}
