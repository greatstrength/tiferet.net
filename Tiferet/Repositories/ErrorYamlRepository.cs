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
/// YAML-backed repository for error definitions.
/// Flat key structure: <c>errors.{id}</c>.
/// </summary>
public class ErrorYamlRepository : YamlRepository<ErrorAggregate, ErrorConfiguration>, IErrorService
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
