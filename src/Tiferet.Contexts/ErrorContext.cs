using Tiferet.Core;
using Tiferet.Domain;
using Tiferet.Events;
using Tiferet.Events.Error;
using Tiferet.Mappers;

namespace Tiferet.Contexts;

/// <summary>
/// Context for retrieving and formatting domain errors.
/// </summary>
public class ErrorContext
{
    private readonly GetError _getErrorEvent;

    /// <summary>
    /// Initializes the error context.
    /// </summary>
    /// <param name="getErrorEvent">The domain event used to retrieve errors by code.</param>
    public ErrorContext(GetError getErrorEvent)
    {
        _getErrorEvent = getErrorEvent;
    }

    /// <summary>
    /// Get an error by its code, falling back to <see cref="DefaultErrors"/>
    /// if the repository lookup fails.
    /// </summary>
    /// <param name="errorCode">The error code to retrieve.</param>
    /// <returns>The error aggregate.</returns>
    public ErrorAggregate GetErrorByCode(string errorCode)
    {
        try
        {
            return _getErrorEvent.Execute(new GetErrorParams(errorCode, IncludeDefaults: true));
        }
        catch (TiferetException)
        {
            // Fall back to a built-in default for "error not found".
            var data = DefaultErrors.Get(ErrorCodes.ErrorNotFound);
            if (data is not null)
            {
                var error = DomainObject.FromDictionary<Domain.Error>(data);
                throw new TiferetApiException(
                    error.ErrorCode,
                    error.Name,
                    error.FormatMessage() ?? "Error not found.",
                    ("id", errorCode));
            }

            throw;
        }
    }

    /// <summary>
    /// Format a <see cref="TiferetException"/> into a structured error response dictionary.
    /// Does not raise — the caller is responsible for throwing.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="lang">The language code for formatting (default: en_US).</param>
    /// <returns>The formatted error response dictionary.</returns>
    public Dictionary<string, object> HandleError(TiferetException exception, string lang = "en_US")
    {
        // Get the error definition by its code.
        var error = GetErrorByCode(exception.ErrorCode);

        // Merge exception context into format args.
        var args = new Dictionary<string, object>(exception.Context);

        // Format and return the response.
        return error.Domain.FormatResponse(lang, args)
            ?? new Dictionary<string, object>
            {
                ["ErrorCode"] = exception.ErrorCode,
                ["Name"] = "Unknown Error",
                ["Message"] = exception.Message,
            };
    }
}
