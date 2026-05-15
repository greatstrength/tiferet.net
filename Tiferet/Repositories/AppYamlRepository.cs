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
/// YAML-backed repository for app interface configurations.
/// Flat key structure: <c>interfaces.{id}</c>.
/// </summary>
public class AppYamlRepository : YamlRepository<AppInterfaceAggregate>, IAppService
{
    /// <summary>
    /// Initializes the app YAML repository.
    /// </summary>
    /// <param name="appYamlFile">Path to the app YAML config file.</param>
    /// <param name="encoding">File encoding (default: utf-8).</param>
    public AppYamlRepository(string appYamlFile, string encoding = "utf-8")
        : base(appYamlFile, "interfaces", encoding)
    {
    }

    /// <inheritdoc/>
    protected override AppInterfaceAggregate Hydrate(Dictionary<string, object> data, string id)
        => AppInterfaceYamlObject.FromYaml(data, id).Map();

    /// <inheritdoc/>
    protected override Dictionary<object, object> Dehydrate(AppInterfaceAggregate entity)
        => AppInterfaceYamlObject.FromAggregate(entity).ToYamlDict();
}
