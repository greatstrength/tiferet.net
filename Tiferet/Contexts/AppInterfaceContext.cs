using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Tiferet.Domain;
using Tiferet.Events;

namespace Tiferet.Contexts;

/// <summary>
/// Top-level application interface context that composes feature execution,
/// error handling, and logging into a unified runtime pipeline.
/// Implements <see cref="IDisposable"/> as the composition root; disposes
/// any owned disposable dependencies (e.g., <see cref="LoggingContext"/>).
/// </summary>
public class AppInterfaceContext : IDisposable
{
    /// <summary>The interface identifier.</summary>
    public string InterfaceId { get; }

    private readonly FeatureContext _features;
    private readonly ErrorContext _errors;
    private readonly LoggingContext _logging;

    /// <summary>
    /// Initializes the application interface context.
    /// </summary>
    /// <param name="interfaceId">The interface identifier.</param>
    /// <param name="features">The feature context for executing workflows.</param>
    /// <param name="errors">The error context for formatting errors.</param>
    /// <param name="logging">The logging context for building loggers.</param>
    public AppInterfaceContext(
        string interfaceId,
        FeatureContext features,
        ErrorContext errors,
        LoggingContext logging)
    {
        InterfaceId = interfaceId;
        _features = features;
        _errors = errors;
        _logging = logging;
    }

    /// <summary>
    /// Parse an incoming request into a <see cref="RequestContext"/>.
    /// </summary>
    /// <param name="headers">Optional request headers.</param>
    /// <param name="data">Optional request data.</param>
    /// <param name="featureId">Optional feature identifier.</param>
    /// <returns>A new request context.</returns>
    public RequestContext ParseRequest(
        Dictionary<string, string>? headers = null,
        Dictionary<string, object?>? data = null,
        string? featureId = null)
    {
        // Ensure headers include the interface ID.
        headers ??= new Dictionary<string, string>();
        headers["InterfaceId"] = InterfaceId;

        return new RequestContext(
            headers: headers,
            data: data,
            featureId: featureId);
    }

    /// <summary>
    /// Execute a feature by its ID with the provided request.
    /// </summary>
    /// <param name="featureId">The feature identifier.</param>
    /// <param name="request">The request context.</param>
    public void ExecuteFeature(string featureId, RequestContext request)
    {
        // Add the feature ID to headers.
        request.Headers["FeatureId"] = featureId;

        // Delegate to the feature context.
        _features.ExecuteFeature(featureId, request);
    }

    /// <summary>
    /// Handle an error by formatting it via <see cref="ErrorContext"/>
    /// and throwing a <see cref="TiferetApiException"/>.
    /// </summary>
    /// <param name="error">The exception to handle.</param>
    /// <returns>Never returns — always throws.</returns>
    public object HandleError(Exception error)
    {
        // Wrap non-Tiferet exceptions.
        var tiferetError = error as TiferetException
            ?? new TiferetException(
                ErrorCodes.AppError,
                $"An error occurred in the app: {error.Message}",
                ("error", error.Message));

        // Format the error response.
        var formatted = _errors.HandleError(tiferetError);

        // Throw the API exception.
        throw new TiferetApiException(
            errorCode: formatted.ErrorCode,
            name: formatted.Name,
            message: formatted.Message);
    }

    /// <summary>
    /// Handle the response from a request.
    /// </summary>
    /// <param name="request">The request context.</param>
    /// <returns>The final result.</returns>
    public object? HandleResponse(RequestContext request)
    {
        return request.HandleResponse();
    }

    /// <summary>
    /// Releases resources held by this context, including the
    /// <see cref="LoggingContext"/> and its logger factories.
    /// </summary>
    public void Dispose() => _logging.Dispose();

    /// <summary>
    /// Run the full application pipeline: parse request, execute feature,
    /// handle response — with timing and error handling.
    /// </summary>
    /// <param name="featureId">The feature identifier.</param>
    /// <param name="headers">Optional request headers.</param>
    /// <param name="data">Optional request data.</param>
    /// <returns>The feature execution result.</returns>
    public object? Run(
        string featureId,
        Dictionary<string, string>? headers = null,
        Dictionary<string, object?>? data = null)
    {
        var stopwatch = Stopwatch.StartNew();

        // Build a logger for this execution.
        var logger = _logging.BuildLogger();

        // Parse the request.
        logger.LogDebug("Parsing request for feature: {FeatureId}", featureId);
        var request = ParseRequest(headers, data, featureId);

        try
        {
            // Execute the feature.
            logger.LogDebug("Executing feature: {FeatureId}", featureId);
            ExecuteFeature(featureId, request);
        }
        catch (TiferetException e)
        {
            logger.LogError("ErrorConfiguration executing feature {FeatureId}: {ErrorConfiguration}", featureId, e.Message);
            return HandleError(e);
        }

        stopwatch.Stop();
        var durationMs = stopwatch.ElapsedMilliseconds;

        logger.LogDebug("FeatureConfiguration {FeatureId} executed successfully, handling response.", featureId);
        logger.LogInformation("Executed FeatureConfiguration - {FeatureId} ({DurationMs}ms)", featureId, durationMs);

        return HandleResponse(request);
    }
}
