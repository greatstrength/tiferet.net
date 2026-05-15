namespace Tiferet.Contexts;

/// <summary>
/// Generic request container that flows through the feature pipeline.
/// <typeparamref name="TResult"/> provides type-safe access to the final result,
/// while intermediate step results flow through <see cref="Data"/>.
/// </summary>
/// <typeparam name="TResult">The type of the final result.</typeparam>
public class RequestContext<TResult>
{
    /// <summary>A unique session identifier.</summary>
    public string SessionId { get; }

    /// <summary>The feature identifier, if assigned.</summary>
    public string? FeatureId { get; set; }

    /// <summary>Request headers (interface_id, feature_id, etc.).</summary>
    public Dictionary<string, string> Headers { get; }

    /// <summary>
    /// Mutable data dictionary that accumulates step results via <c>dataKey</c>.
    /// Also carries initial user-supplied input.
    /// </summary>
    public Dictionary<string, object?> Data { get; }

    /// <summary>The final result of the feature execution.</summary>
    public TResult? Result { get; private set; }

    /// <summary>
    /// Initializes a new <see cref="RequestContext{TResult}"/>.
    /// </summary>
    /// <param name="headers">Optional request headers.</param>
    /// <param name="data">Optional initial data.</param>
    /// <param name="sessionId">Optional session ID (auto-generated if null).</param>
    /// <param name="featureId">Optional feature ID.</param>
    public RequestContext(
        Dictionary<string, string>? headers = null,
        Dictionary<string, object?>? data = null,
        string? sessionId = null,
        string? featureId = null)
    {
        SessionId = sessionId ?? Guid.NewGuid().ToString();
        FeatureId = featureId;
        Headers = headers ?? new Dictionary<string, string>();
        Data = data ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Store a step result. If <paramref name="dataKey"/> is provided, the result
    /// is placed into <see cref="Data"/> for downstream steps. Otherwise, it is
    /// stored as the final <see cref="Result"/>.
    /// </summary>
    /// <param name="result">The step result.</param>
    /// <param name="dataKey">Optional key to store the result in Data.</param>
    public void SetResult(object? result, string? dataKey = null)
    {
        if (dataKey is not null)
        {
            Data[dataKey] = result;
        }
        else
        {
            Result = result is TResult typed ? typed : default;
        }
    }

    /// <summary>
    /// Return the final result.
    /// </summary>
    /// <returns>The result.</returns>
    public TResult? HandleResponse() => Result;
}

/// <summary>
/// Non-generic convenience alias defaulting to <c>object?</c> result type.
/// </summary>
public class RequestContext : RequestContext<object?>
{
    /// <inheritdoc />
    public RequestContext(
        Dictionary<string, string>? headers = null,
        Dictionary<string, object?>? data = null,
        string? sessionId = null,
        string? featureId = null)
        : base(headers, data, sessionId, featureId)
    {
    }
}
