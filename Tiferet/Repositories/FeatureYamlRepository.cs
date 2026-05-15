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
/// YAML-backed repository for feature workflow configurations.
/// Composite key structure: <c>features.{groupId}.{featureKey}</c>.
/// </summary>
public class FeatureYamlRepository : YamlRepository<FeatureAggregate>, IFeatureService
{
    /// <summary>
    /// Initializes the feature YAML repository.
    /// </summary>
    /// <param name="featureYamlFile">Path to the feature YAML config file.</param>
    /// <param name="encoding">File encoding (default: utf-8).</param>
    public FeatureYamlRepository(string featureYamlFile, string encoding = "utf-8")
        : base(featureYamlFile, "features", encoding)
    {
    }

    /// <summary>Split composite ID into <c>[features, group, key]</c>.</summary>
    protected override string[] GetSectionPath(string id)
    {
        var parts = id.Split('.', 2);
        return [SectionKey, parts[0], parts[1]];
    }

    /// <summary>Reconstruct composite ID as <c>group.key</c>.</summary>
    protected override string ReconstructId(params string[] segments)
        => string.Join(".", segments);

    /// <summary>
    /// Flatten nested group → feature structure into a list of aggregates.
    /// </summary>
    protected override IReadOnlyList<FeatureAggregate> ListFromSection(Dictionary<string, object> section)
    {
        var results = new List<FeatureAggregate>();
        foreach (var (groupId, groupObj) in section)
        {
            var group = YamlHelper.ToStringDict(groupObj);
            foreach (var (featureKey, featureObj) in group)
            {
                var data = YamlHelper.ToStringDict(featureObj);
                var id = $"{groupId}.{featureKey}";
                results.Add(Hydrate(data, id));
            }
        }
        return results;
    }

    /// <inheritdoc/>
    protected override FeatureAggregate Hydrate(Dictionary<string, object> data, string id)
        => FeatureYamlObject.FromYaml(data, id).Map();

    /// <inheritdoc/>
    protected override Dictionary<object, object> Dehydrate(FeatureAggregate entity)
        => FeatureYamlObject.FromAggregate(entity).ToYamlDict();
}
