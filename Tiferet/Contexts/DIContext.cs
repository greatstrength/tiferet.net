using Tiferet.Events;
using Tiferet.Domain;
using Tiferet.Domain.Error;
using Tiferet.Domain.Feature;
using Tiferet.DependencyInjection;
using Tiferet.Events.DI;
using Tiferet.Mappers;
using Tiferet.Mappers.Feature;
using Tiferet.Mappers.DI;
using Tiferet.Mappers.Logging;

namespace Tiferet.Contexts;

/// <summary>
/// Context for managing dependency injection configuration and service provider lifecycle.
/// Builds and caches <see cref="IServiceResolver"/> instances keyed by flag combinations.
/// </summary>
public class DIContext
{
    private readonly ListAllSettings _listAllSettingsEvent;
    private readonly CacheContext _cache;
    private readonly Func<Dictionary<string, Type>, Dictionary<string, string>, IServiceResolver> _createProvider;

    /// <summary>
    /// Initializes the DI context.
    /// </summary>
    /// <param name="listAllSettingsEvent">The event to list all service configurations and constants.</param>
    /// <param name="cache">Optional cache context (created if null).</param>
    /// <param name="createProvider">Optional factory for creating service providers.</param>
    public DIContext(
        ListAllSettings listAllSettingsEvent,
        CacheContext? cache = null,
        Func<Dictionary<string, Type>, Dictionary<string, string>, IServiceResolver>? createProvider = null)
    {
        _listAllSettingsEvent = listAllSettingsEvent;
        _cache = cache ?? new CacheContext();
        _createProvider = createProvider ?? DefaultServiceProvider;
    }

    /// <summary>
    /// Create the default service provider from a type map and constants.
    /// </summary>
    private static IServiceResolver DefaultServiceProvider(
        Dictionary<string, Type> typeMap,
        Dictionary<string, string> constants)
    {
        var resolver = new DynamicServiceResolver();

        // Register constants first so factories can wire them.
        foreach (var (key, value) in constants)
            resolver.AddService(typeof(string), value);

        // Register service types.
        foreach (var (id, type) in typeMap)
            resolver.AddService(type, Activator.CreateInstance(type)!);

        return resolver;
    }

    /// <summary>
    /// Create a cache key from the given flags.
    /// </summary>
    /// <param name="flags">The flags to create a cache key from.</param>
    /// <returns>The cache key.</returns>
    public static string CreateCacheKey(string[] flags)
    {
        return flags.Length > 0
            ? $"feature_services_{string.Join("_", flags)}"
            : "feature_services";
    }

    /// <summary>
    /// Build and cache a service provider for the given flags.
    /// </summary>
    /// <param name="flags">The flags to match against service configurations.</param>
    /// <returns>The service resolver instance.</returns>
    public IServiceResolver BuildServiceProvider(params string[] flags)
    {
        // Check cache first.
        var cacheKey = CreateCacheKey(flags);
        var cached = _cache.Get<IServiceResolver>(cacheKey);
        if (cached is not null)
            return cached;

        // Load all service configurations and constants.
        var (configurations, rawConstants) =
            _listAllSettingsEvent.Execute(new ListAllSettingsParams());

        // Parse constants.
        var constants = LoadConstants(configurations, rawConstants, flags);

        // Resolve service types from configurations.
        var typeMap = new Dictionary<string, Type>();
        foreach (var config in configurations)
        {
            var serviceType = config.GetServiceType(flags);
            if (serviceType is null)
            {
                DomainEvent.RaiseError(
                    ErrorCodes.DependencyTypeNotFound,
                    $"No dependency type found for service configuration {config.Id} with flags [{string.Join(", ", flags)}].",
                    ("configurationId", config.Id),
                    ("flags", string.Join(", ", flags)));
            }

            typeMap[config.Id] = serviceType!;
        }

        // Build the provider.
        var provider = _createProvider(typeMap, constants);

        // Cache provider and typeMap together so GetDependency can resolve by type.
        _cache.Set(cacheKey, provider);
        _cache.Set($"{cacheKey}_types", typeMap);
        return provider;
    }

    /// <summary>
    /// Get a resolved service by its configuration ID and flags.
    /// Looks up the service type from the cached type map and resolves
    /// the correct instance from the provider.
    /// </summary>
    /// <param name="configurationId">The service configuration identifier.</param>
    /// <param name="flags">The flags to use for provider resolution.</param>
    /// <returns>The resolved service instance.</returns>
    public object? GetDependency(string configurationId, params string[] flags)
    {
        // Ensure provider is built (and typeMap cached).
        var provider = BuildServiceProvider(flags);

        // Retrieve the cached type map to find the correct service type.
        var typeMapKey = $"{CreateCacheKey(flags)}_types";
        var typeMap = _cache.Get<Dictionary<string, Type>>(typeMapKey);
        if (typeMap is null || !typeMap.TryGetValue(configurationId, out var serviceType))
            return null;

        return provider.GetService(serviceType);
    }

    /// <summary>
    /// Build the constants dictionary by parsing top-level constants and per-configuration parameters.
    /// </summary>
    /// <param name="configurations">The service configurations.</param>
    /// <param name="rawConstants">The top-level constants.</param>
    /// <param name="flags">The flags to match flagged dependencies.</param>
    /// <returns>A dictionary of parsed constants.</returns>
    public Dictionary<string, string> LoadConstants(
        IReadOnlyList<ServiceConfigurationAggregate> configurations,
        Dictionary<string, string> rawConstants,
        string[] flags)
    {
        // Parse top-level constants via ParseParameter.
        var constants = new Dictionary<string, string>();
        foreach (var (key, value) in rawConstants)
            constants[key] = ParseParameter.Parse(value);

        // Merge per-configuration parameters (flagged or default).
        foreach (var config in configurations)
        {
            var dependency = config.GetDependency(flags);
            var parameters = dependency?.Parameters ?? config.Parameters;

            if (parameters is not null)
            {
                foreach (var (key, value) in parameters)
                    constants[key] = ParseParameter.Parse(value);
            }
        }

        return constants;
    }
}
