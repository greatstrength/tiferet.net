using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tiferet.Blueprints;

namespace Tiferet.DependencyInjection;

/// <summary>
/// Extension methods for integrating Tiferet with <see cref="IHostBuilder"/>.
/// Delegates to <see cref="ServiceCollectionExtensions.AddTiferet(IServiceCollection, IConfiguration)"/>
/// which in turn delegates to <see cref="AppBlueprint.ConfigureServices"/>.
/// </summary>
public static class TiferetHostExtensions
{
    /// <summary>
    /// Add Tiferet framework services to the host, reading
    /// <see cref="TiferetOptions"/> from the host's <see cref="IConfiguration"/>.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    /// <returns>The host builder for chaining.</returns>
    public static IHostBuilder UseTiferet(this IHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.AddTiferet(context.Configuration);
        });
        return builder;
    }

    /// <summary>
    /// Add Tiferet framework services to the host with explicit configuration.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    /// <param name="configure">Callback to configure <see cref="TiferetOptions"/>.</param>
    /// <returns>The host builder for chaining.</returns>
    public static IHostBuilder UseTiferet(
        this IHostBuilder builder,
        Action<TiferetOptions> configure)
    {
        builder.ConfigureServices((_, services) =>
        {
            services.AddTiferet(configure);
        });
        return builder;
    }
}
