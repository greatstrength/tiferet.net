using Tiferet.Mappers.DI;

namespace Tiferet.Interfaces;

/// <summary>Service interface for managing DI service configurations.</summary>
public interface IDIService : IService
{
    /// <summary>Check if a service configuration exists by ID.</summary>
    bool ConfigurationExists(string id);

    /// <summary>Retrieve a service configuration by ID.</summary>
    ServiceConfigurationAggregate? GetConfiguration(string id);

    /// <summary>List all service configurations and constants.</summary>
    (IReadOnlyList<ServiceConfigurationAggregate> Configurations, Dictionary<string, string> Constants) ListAll();

    /// <summary>Save or update a service configuration.</summary>
    void SaveConfiguration(ServiceConfigurationAggregate configuration);

    /// <summary>Delete a service configuration by ID (idempotent).</summary>
    void DeleteConfiguration(string id);

    /// <summary>Save or update constants.</summary>
    void SaveConstants(Dictionary<string, string> constants);
}
