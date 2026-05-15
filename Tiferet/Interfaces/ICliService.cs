using Tiferet.Domain.Cli;
using Tiferet.Mappers.Cli;

namespace Tiferet.Interfaces;

/// <summary>Service interface for managing CLI command definitions.</summary>
public interface ICliService : IRepository<CliCommandAggregate>
{
    /// <summary>Get all parent-level CLI arguments.</summary>
    IReadOnlyList<CliArgumentConfiguration> GetParentArguments();
}
