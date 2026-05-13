using Microsoft.Extensions.DependencyInjection;

namespace Tiferet.DependencyInjection;

/// <summary>
/// Extension methods for integrating Tiferet DI with
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Tiferet framework services to the DI container.
    /// Registers a singleton <see cref="IServiceResolver"/> backed by
    /// <see cref="DynamicServiceResolver"/>.
    /// </summary>
    /// <param name="services">The host service collection.</param>
    /// <param name="configure">Optional callback to configure the resolver.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTiferet(
        this IServiceCollection services,
        Action<IServiceResolver>? configure = null)
    {
        var resolver = new DynamicServiceResolver();
        configure?.Invoke(resolver);
        services.AddSingleton<IServiceResolver>(resolver);
        return services;
    }
}
