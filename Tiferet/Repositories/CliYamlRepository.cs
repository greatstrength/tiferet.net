using Tiferet.Domain;
using Tiferet.Domain.App;
using Tiferet.Domain.Cli;
using Tiferet.Domain.DI;
using Tiferet.Domain.Error;
using Tiferet.Domain.Feature;
using Tiferet.Domain.Logging;
using Tiferet.Interfaces;
using Tiferet.Mappers;
using Tiferet.Mappers.App;
using Tiferet.Mappers.Cli;
using Tiferet.Mappers.DI;
using Tiferet.Mappers.Error;
using Tiferet.Mappers.Feature;
using Tiferet.Mappers.Logging;

namespace Tiferet.Repositories;

/// <summary>
/// YAML-backed repository for CLI command definitions.
/// Composite key structure: <c>cli.cmds.{groupKey}.{commandKey}</c>.
/// </summary>
public class CliYamlRepository : YamlRepository<CliCommandAggregate>, ICliService
{
    /// <summary>
    /// Initializes the CLI YAML repository.
    /// </summary>
    /// <param name="cliYamlFile">Path to the CLI YAML config file.</param>
    /// <param name="encoding">File encoding (default: utf-8).</param>
    public CliYamlRepository(string cliYamlFile, string encoding = "utf-8")
        : base(cliYamlFile, "cli", encoding)
    {
    }

    /// <summary>Split composite ID into <c>[cli, cmds, group, key]</c>.</summary>
    protected override string[] GetSectionPath(string id)
    {
        var parts = id.Split('.', 2);
        return [SectionKey, "cmds", parts[0], parts[1]];
    }

    /// <summary>Reconstruct composite ID as <c>group.key</c>.</summary>
    protected override string ReconstructId(params string[] segments)
        => string.Join(".", segments);

    /// <summary>
    /// List all CLI commands by navigating <c>cli.cmds</c> and flattening groups.
    /// </summary>
    public override IReadOnlyList<CliCommandAggregate> List()
    {
        var full = LoadFull();
        var cmds = YamlHelper.GetSection(full, "cli", "cmds");
        var results = new List<CliCommandAggregate>();
        foreach (var (groupKey, groupObj) in cmds)
        {
            var group = YamlHelper.ToStringDict(groupObj);
            foreach (var (commandKey, commandObj) in group)
            {
                var data = YamlHelper.ToStringDict(commandObj);
                var id = $"{groupKey}.{commandKey}";
                results.Add(Hydrate(data, id));
            }
        }
        return results;
    }

    /// <inheritdoc/>
    protected override CliCommandAggregate Hydrate(Dictionary<string, object> data, string id)
        => CliCommandYamlObject.FromYaml(data, id).Map();

    /// <inheritdoc/>
    protected override Dictionary<object, object> Dehydrate(CliCommandAggregate entity)
        => CliCommandYamlObject.FromAggregate(entity).ToYamlDict();

    /// <summary>Get all parent-level CLI arguments.</summary>
    public IReadOnlyList<CliArgumentConfiguration> GetParentArguments()
    {
        var full = LoadFull();
        var argsNode = YamlHelper.GetNestedValue(full, "cli", "parent_args");

        if (argsNode is not List<object> argList)
            return [];

        return argList
            .Select(a => CliArgumentYamlObject.FromYaml(YamlHelper.ToStringDict(a)).ToRecord())
            .ToList();
    }
}
