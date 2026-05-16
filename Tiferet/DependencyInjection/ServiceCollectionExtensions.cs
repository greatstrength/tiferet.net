using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tiferet.Blueprints;

namespace Tiferet.DependencyInjection;

/// <summary>
/// Extension methods for integrating Tiferet with
/// <see cref="IServiceCollection"/>.
/// All overloads delegate to <see cref="AppBlueprint.ConfigureServices"/> —
/// the DI layer does not reference Assets directly.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Tiferet framework services to the DI container,
    /// binding <see cref="TiferetOptions"/> from the <paramref name="configuration"/> section.
    /// </summary>
    /// <param name="services">The host service collection.</param>
    /// <param name="configuration">The application configuration root.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTiferet(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = new TiferetOptions();
        configuration.GetSection(TiferetOptions.SectionName).Bind(options);
        return AppBlueprint.ConfigureServices(services, options);
    }

    /// <summary>
    /// Add Tiferet framework services to the DI container,
    /// using an explicit configuration callback.
    /// </summary>
    /// <param name="services">The host service collection.</param>
    /// <param name="configure">Callback to configure <see cref="TiferetOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTiferet(
        this IServiceCollection services,
        Action<TiferetOptions> configure)
    {
        var options = new TiferetOptions();
        configure(options);
        return AppBlueprint.ConfigureServices(services, options);
    }
}
