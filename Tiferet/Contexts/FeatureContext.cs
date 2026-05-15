using System.Text.RegularExpressions;
using Tiferet.Assets;
using Tiferet.Events;
using Tiferet.Domain;
using Tiferet.Domain.Error;
using Tiferet.Domain.Feature;
using Tiferet.Events.Feature;
using Tiferet.Mappers;
using Tiferet.Mappers.Feature;
using Tiferet.Mappers.DI;
using Tiferet.Mappers.Logging;

namespace Tiferet.Contexts;

/// <summary>
/// Context for loading and executing feature workflows.
/// Orchestrates step resolution, parameter parsing, condition evaluation,
/// and sequential event execution.
/// </summary>
public class FeatureContext
{
    private readonly GetFeature _getFeatureEvent;
    private readonly DIContext _services;
    private readonly CacheContext _cache;

    /// <summary>
    /// Initializes the feature context.
    /// </summary>
    /// <param name="getFeatureEvent">The event used to retrieve features by ID.</param>
    /// <param name="services">The DI context for dependency resolution.</param>
    /// <param name="cache">Optional cache context for caching loaded features.</param>
    public FeatureContext(
        GetFeature getFeatureEvent,
        DIContext services,
        CacheContext? cache = null)
    {
        _getFeatureEvent = getFeatureEvent;
        _services = services;
        _cache = cache ?? new CacheContext();
    }

    /// <summary>
    /// Load a feature by ID, using the cache when available.
    /// </summary>
    /// <param name="featureId">The feature identifier.</param>
    /// <returns>The loaded feature aggregate.</returns>
    public FeatureAggregate LoadFeature(string featureId)
    {
        // Try cache first.
        var cached = _cache.Get<FeatureAggregate>(featureId);
        if (cached is not null)
            return cached;

        // Load via the GetFeature event.
        var feature = _getFeatureEvent.Execute(new GetFeatureParams(featureId));

        // Cache and return.
        _cache.Set(featureId, feature);
        return feature;
    }

    /// <summary>
    /// Load a feature event step from the DI context using its service ID
    /// and any configured flags.
    /// </summary>
    /// <param name="featureEvent">The feature event metadata.</param>
    /// <param name="featureFlags">Optional feature-level flags (higher priority).</param>
    /// <returns>The resolved domain event.</returns>
    public DomainEvent LoadFeatureStep(FeatureEventConfiguration featureEvent, IReadOnlyList<string>? featureFlags = null)
    {
        var serviceId = featureEvent.ServiceId;

        // Combine flags: feature-level first, then step-level.
        var combined = new List<string>();
        if (featureFlags is not null) combined.AddRange(featureFlags);
        if (featureEvent.Flags is not null) combined.AddRange(featureEvent.Flags);

        try
        {
            var resolved = _services.GetDependency(serviceId, combined.ToArray());
            if (resolved is DomainEvent domainEvent)
                return domainEvent;

            DomainEvent.RaiseError(
                ErrorCodes.FeatureCommandLoadingFailed,
                $"Resolved dependency for {serviceId} is not a DomainEvent.",
                ("serviceId", serviceId));
            return null!; // unreachable
        }
        catch (Exception e) when (e is not TiferetException)
        {
            DomainEvent.RaiseError(
                ErrorCodes.FeatureCommandLoadingFailed,
                $"Failed to load feature step: {serviceId}.",
                ("serviceId", serviceId),
                ("exception", e.Message));
            return null!; // unreachable
        }
    }

    /// <summary>
    /// Parse a request-aware parameter.
    /// Handles <c>$r.</c> (request-backed) and <c>$env.</c> (environment) prefixes.
    /// </summary>
    /// <param name="parameter">The parameter string to parse.</param>
    /// <param name="request">The request context for resolving $r. references.</param>
    /// <returns>The resolved parameter value.</returns>
    public static string ParseRequestParameter(string parameter, RequestContext? request = null)
    {
        // Non-request parameters delegate to Core.ParseParameter.
        if (!parameter.StartsWith("$r."))
            return ParseParameter.Parse(parameter);

        // Request-backed parameter requires a request context.
        if (request is null)
        {
            DomainEvent.RaiseError(
                ErrorCodes.RequestNotFound,
                "Request data is not available for parameter parsing.",
                ("parameter", parameter));
        }

        // Extract the key and look it up in request data.
        var key = parameter[3..];
        if (request!.Data.TryGetValue(key, out var value) && value is not null)
            return value.ToString()!;

        DomainEvent.RaiseError(
            ErrorCodes.ParameterNotFound,
            $"Parameter {parameter} not found in request data.",
            ("parameter", parameter));
        return null!; // unreachable
    }

    /// <summary>
    /// Evaluate a boolean expression against request data.
    /// Returns true when condition is null or empty (unconditional step).
    /// Uses <c>$r.</c> prefix to reference values from request data.
    /// </summary>
    /// <param name="condition">The boolean expression to evaluate.</param>
    /// <param name="request">The request context containing the data.</param>
    /// <returns>The boolean result of the evaluated expression.</returns>
    public static bool EvaluateCondition(string? condition, RequestContext request)
    {
        // Unconditional step.
        if (string.IsNullOrWhiteSpace(condition))
            return true;

        // Resolve $r.<key> references by substituting values from request data.
        var resolved = Regex.Replace(condition, @"\$r\.(\w+)", match =>
        {
            var key = match.Groups[1].Value;
            if (request.Data.TryGetValue(key, out var value) && value is not null)
                return value.ToString()!;
            return "null";
        });

        // Simple equality/inequality evaluation for common patterns.
        // Supports: "value != null", "value == 'something'", truthy checks.
        try
        {
            // Handle "!= null" / "== null" patterns.
            if (resolved.Contains("!= null"))
            {
                var left = resolved.Replace("!= null", "").Trim();
                return left != "null" && !string.IsNullOrEmpty(left);
            }
            if (resolved.Contains("== null"))
            {
                var left = resolved.Replace("== null", "").Trim();
                return left == "null" || string.IsNullOrEmpty(left);
            }

            // Truthy check: non-null, non-empty, non-"False".
            return resolved != "null"
                && !string.IsNullOrEmpty(resolved)
                && !string.Equals(resolved, "False", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Handle execution of a single command (domain event) within the feature pipeline.
    /// </summary>
    /// <param name="command">The domain event to execute.</param>
    /// <param name="request">The request context.</param>
    /// <param name="dataKey">Optional key to store the result in request data.</param>
    /// <param name="passOnError">If true, swallow errors and set result to null.</param>
    /// <param name="extraParams">Additional parameters to merge with request data.</param>
    public void HandleCommand(
        DomainEvent command,
        RequestContext request,
        string? dataKey = null,
        bool passOnError = false,
        Dictionary<string, object?>? extraParams = null)
    {
        // Merge request data with extra parameters.
        var data = new Dictionary<string, object?>(request.Data);
        if (extraParams is not null)
        {
            foreach (var (key, value) in extraParams)
                data[key] = value;
        }

        try
        {
            // Execute via the non-generic dictionary overload.
            var result = command.Execute(data);

            // Store the result via the request context.
            request.SetResult(result, dataKey);
        }
        catch (Exception)
        {
            if (!passOnError)
                throw;

            // Set result to null when passing on the error.
            request.SetResult(null, dataKey);
        }
    }

    /// <summary>
    /// Execute a feature by its ID with the provided request.
    /// Iterates over configured steps, evaluates conditions, resolves events,
    /// parses parameters, and executes each step sequentially.
    /// </summary>
    /// <param name="featureId">The feature identifier.</param>
    /// <param name="request">The request context.</param>
    public void ExecuteFeature(string featureId, RequestContext request)
    {
        // Load the feature by ID, using cache when possible.
        var feature = LoadFeature(featureId);

        // Execute by iterating over configured steps.
        if (feature.Domain.Steps is null) return;

        foreach (var step in feature.Domain.Steps)
        {
            // Evaluate the step condition; skip if false.
            if (!EvaluateCondition(step.Condition, request))
                continue;

            // Load the event dependency for this step.
            var command = LoadFeatureStep(step, feature.Domain.Flags);

            // Parse the step parameters.
            var parsedParams = new Dictionary<string, object?>();
            if (step.Parameters is not null)
            {
                foreach (var (paramKey, paramValue) in step.Parameters)
                    parsedParams[paramKey] = ParseRequestParameter(paramValue, request);
            }

            // Execute the step.
            HandleCommand(
                command,
                request,
                dataKey: step.DataKey,
                passOnError: step.PassOnError,
                extraParams: parsedParams);
        }
    }
}
