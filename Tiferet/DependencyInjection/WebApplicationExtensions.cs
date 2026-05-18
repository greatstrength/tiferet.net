using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Tiferet.Blueprints;
using Tiferet.Contexts;
using Tiferet.Interfaces;

namespace Tiferet.DependencyInjection;

/// <summary>
/// Extension methods for mapping Tiferet feature endpoints on a <see cref="WebApplication"/>.
/// Requires services to have been registered via <see cref="ServiceCollectionExtensions.AddTiferet"/>.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Map all Tiferet features as Minimal API endpoints.
    /// Resolves <see cref="AppInterfaceContext"/> and <see cref="IFeatureService"/>
    /// from the DI container.
    /// </summary>
    /// <param name="app">The web application (endpoint route builder).</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapTiferetFeatures(this WebApplication app)
    {
        var context = app.Services.GetRequiredService<AppInterfaceContext>();
        var featureService = app.Services.GetRequiredService<IFeatureService>();
        return WebBlueprint.MapFeatures(app, context, featureService);
    }
}
