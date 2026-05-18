using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Tiferet.Assets;
using Tiferet.Contexts;
using Tiferet.Domain.Feature;
using Tiferet.Events;
using Tiferet.Events.Feature;
using Tiferet.Interfaces;

namespace Tiferet.Blueprints;

/// <summary>
/// Static blueprint for mapping feature definitions to ASP.NET Minimal API endpoints.
/// Generates <c>POST /api/{group}/{key}</c> routes that dispatch feature execution
/// via <see cref="AppInterfaceContext.RunAsync"/>, mirroring the CLI dispatch pattern
/// of <see cref="CliBlueprint"/>.
/// </summary>
public static class WebBlueprint
{
    // *** methods

    // ** method: map_features (with context)
    /// <summary>
    /// Map all features from the feature service to Minimal API endpoints.
    /// Each feature becomes a <c>POST /api/{GroupId}/{FeatureKey}</c> endpoint.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder (typically a <c>WebApplication</c>).</param>
    /// <param name="app">The wired application interface context.</param>
    /// <param name="featureService">The feature service for listing features.</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapFeatures(
        IEndpointRouteBuilder endpoints,
        AppInterfaceContext app,
        IFeatureService featureService)
    {
        // Load all features from configuration.
        var listEvent = new ListFeatures(featureService);
        var features = listEvent.Execute(new ListFeaturesParams());

        // Map each feature to a POST endpoint.
        foreach (var feature in features)
        {
            MapFeature(endpoints, app, feature);
        }

        return endpoints;
    }

    // ** method: map_features (standalone)
    /// <summary>
    /// Convenience overload that bootstraps the app and maps all features.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="interfaceId">The interface identifier.</param>
    /// <param name="configDir">The configuration directory.</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapFeatures(
        IEndpointRouteBuilder endpoints,
        string interfaceId,
        string configDir = ConfigurationDefaults.DefaultConfigDir)
    {
        // Bootstrap the app.
        var app = AppBlueprint.BuildApp(interfaceId, configDir);

        // Resolve the feature service for listing.
        var configFile = Path.Combine(configDir, ConfigurationDefaults.ConfigFile);
        var featureServiceType = ImportDependency.Resolve(
            ConfigurationDefaults.DefaultAssembly,
            ConfigurationDefaults.DefaultFeatureServiceType);
        var ctor = featureServiceType.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .First();
        var ctorParams = ctor.GetParameters();
        var args = new object?[ctorParams.Length];
        args[0] = configFile;
        for (int i = 1; i < ctorParams.Length; i++)
            args[i] = ctorParams[i].HasDefaultValue ? ctorParams[i].DefaultValue : null;
        var featureService = (IFeatureService)ctor.Invoke(args);

        return MapFeatures(endpoints, app, featureService);
    }

    // ** method: map_feature
    /// <summary>
    /// Map a single feature to a <c>POST /api/{GroupId}/{FeatureKey}</c> endpoint.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="app">The application interface context.</param>
    /// <param name="feature">The feature configuration to map.</param>
    private static void MapFeature(
        IEndpointRouteBuilder endpoints,
        AppInterfaceContext app,
        FeatureConfiguration feature)
    {
        var route = $"/api/{feature.GroupId}/{feature.FeatureKey}";
        var featureId = feature.Id;
        var description = feature.Description ?? feature.Name;

        endpoints.MapPost(route, async (HttpContext context) =>
        {
            // Deserialize JSON body into a data dictionary.
            Dictionary<string, object?>? data = null;
            try
            {
                data = await context.Request.ReadFromJsonAsync<Dictionary<string, object?>>();
            }
            catch
            {
                // Allow empty body — data will be null/empty.
            }

            data ??= new Dictionary<string, object?>();

            try
            {
                // Dispatch to the feature pipeline.
                var result = await app.RunAsync(featureId, data: data);

                // Return the result.
                if (result is null)
                    return Results.NoContent();

                return Results.Ok(result);
            }
            catch (TiferetApiException ex)
            {
                return Results.BadRequest(new
                {
                    error = ex.ErrorCode,
                    name = ex.Name,
                    message = ex.Message
                });
            }
            catch (TiferetException ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: ex.ErrorCode);
            }
        })
        .WithName(featureId)
        .WithDescription(description);
    }
}
