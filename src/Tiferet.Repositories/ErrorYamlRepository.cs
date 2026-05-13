using Tiferet.Domain;
using Tiferet.Interfaces;
using Tiferet.Mappers;

namespace Tiferet.Repositories;

/// <summary>
/// YAML-backed repository for error definitions.
/// Flat key structure: <c>errors.{id}</c>.
/// </summary>
public class ErrorYamlRepository : YamlRepository<ErrorAggregate, Error>, IErrorService
{
    /// <summary>
    /// Initializes the error YAML repository.
    /// </summary>
    /// <param name="errorYamlFile">Path to the error YAML config file.</param>
    /// <param name="encoding">File encoding (default: utf-8).</param>
    public ErrorYamlRepository(string errorYamlFile, string encoding = "utf-8")
        : base(errorYamlFile, "errors", encoding)
    {
    }

    /// <inheritdoc/>
    protected override ErrorAggregate Hydrate(Dictionary<string, object> data, string id)
        => ErrorYamlObject.FromYaml(data, id).Map();

    /// <inheritdoc/>
    protected override Dictionary<object, object> Dehydrate(ErrorAggregate entity)
        => ErrorYamlObject.FromAggregate(entity).ToYamlDict();
}
