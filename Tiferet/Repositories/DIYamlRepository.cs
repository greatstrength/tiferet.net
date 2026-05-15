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
using Tiferet.Utilities;

namespace Tiferet.Repositories;

/// <summary>
/// YAML-backed repository for DI service configurations.
/// Implements <see cref="IDIService"/> directly (not via <see cref="YamlRepository{TAggregate, TDomain}"/>).
/// </summary>
public class DIYamlRepository : IDIService
{
    private readonly string _yamlFile;
    private readonly string _encoding;

    /// <summary>
    /// Initializes the DI YAML repository.
    /// </summary>
    /// <param name="diYamlFile">Path to the DI YAML config file.</param>
    /// <param name="encoding">File encoding (default: utf-8).</param>
    public DIYamlRepository(string diYamlFile, string encoding = "utf-8")
    {
        _yamlFile = diYamlFile;
        _encoding = encoding;
    }

    private Dictionary<object, object> LoadFull()
    {
        var loader = YamlLoader.ForReading(_yamlFile);
        var result = loader.Load();
        return result as Dictionary<object, object> ?? new Dictionary<object, object>();
    }

    private void SaveFull(Dictionary<object, object> data)
    {
        var loader = YamlLoader.ForWriting(_yamlFile);
        loader.Save(data);
    }

    /// <inheritdoc/>
    public bool ConfigurationExists(string id)
    {
        var full = LoadFull();
        return YamlHelper.GetNestedValue(full, "services", id) is not null;
    }

    /// <inheritdoc/>
    public ServiceConfigurationAggregate? GetConfiguration(string id)
    {
        var full = LoadFull();
        var node = YamlHelper.GetNestedValue(full, "services", id);
        if (node is null) return null;

        var data = YamlHelper.ToStringDict(node);
        return ServiceConfigurationYamlObject.FromYaml(data, id).Map();
    }

    /// <inheritdoc/>
    public (IReadOnlyList<ServiceConfigurationAggregate> Configurations, Dictionary<string, string> Constants) ListAll()
    {
        var full = LoadFull();

        // Load configurations.
        var servicesSection = YamlHelper.GetSection(full, "services");
        var configs = new List<ServiceConfigurationAggregate>();
        foreach (var (configId, configObj) in servicesSection)
        {
            var data = YamlHelper.ToStringDict(configObj);
            configs.Add(ServiceConfigurationYamlObject.FromYaml(data, configId).Map());
        }

        // Load constants.
        var constSection = YamlHelper.GetSection(full, "const");
        var constants = constSection.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? "");

        return (configs, constants);
    }

    /// <inheritdoc/>
    public void SaveConfiguration(ServiceConfigurationAggregate configuration)
    {
        var full = LoadFull();
        var dehydrated = ServiceConfigurationYamlObject.FromAggregate(configuration).ToYamlDict();
        YamlHelper.SetNestedValue(full, dehydrated, "services", configuration.Id);
        SaveFull(full);
    }

    /// <inheritdoc/>
    public void DeleteConfiguration(string id)
    {
        var full = LoadFull();
        YamlHelper.RemoveNestedValue(full, "services", id);
        SaveFull(full);
    }

    /// <inheritdoc/>
    public void SaveConstants(Dictionary<string, string> constants)
    {
        var full = LoadFull();

        // Load existing constants and merge.
        var existing = YamlHelper.GetSection(full, "const")
            .ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? "");

        foreach (var (key, value) in constants)
            existing[key] = value;

        // Write merged constants as Dictionary<object, object>.
        var constDict = new Dictionary<object, object>();
        foreach (var (key, value) in existing)
            constDict[key] = value;

        if (!full.ContainsKey("const"))
            full["const"] = constDict;
        else
            full["const"] = constDict;

        SaveFull(full);
    }

}
