using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers;

namespace Tiferet.Repositories;

/// <summary>
/// YAML-backed repository for app interface configurations.
/// Flat key structure: <c>interfaces.{id}</c>.
/// </summary>
public class AppYamlRepository : YamlRepository<AppInterfaceAggregate, AppInterface>, IAppService
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
