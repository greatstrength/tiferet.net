using Tiferet.Assets;
using Tiferet.Events;
using Tiferet.Domain;
using Tiferet.Domain.Error;
using Tiferet.Domain.Feature;
using Tiferet.Events.Error;
using Tiferet.Mappers;
using Tiferet.Mappers.Error;
using Tiferet.Mappers.Feature;
using Tiferet.Mappers.DI;
using Tiferet.Mappers.Logging;

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
            var defaultError = DefaultErrors.Get(ErrorCodes.ErrorNotFound);
            if (defaultError is not null)
            {
                throw new TiferetApiException(
                    defaultError.ErrorCode,
                    defaultError.Name,
                    defaultError.FormatMessage() ?? "Error not found.",
                    ("id", errorCode));
            }

            throw;
        }
    }

    /// <summary>
    /// Format a <see cref="TiferetException"/> into a structured error response.
    /// Does not raise — the caller is responsible for throwing.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="lang">The language code for formatting (default: en_US).</param>
    /// <returns>The formatted error response.</returns>
    public ErrorResponse HandleError(TiferetException exception, string lang = "en_US")
    {
        // Get the error definition by its code.
        var error = GetErrorByCode(exception.ErrorCode);

        // Merge exception context into format args.
        var args = new Dictionary<string, object>(exception.Context);

        // Format and return the response.
        return error.Domain.FormatResponse(lang, args)
            ?? new ErrorResponse(
                ErrorCode: exception.ErrorCode,
                Name: "Unknown Error",
                Message: exception.Message);
    }
}
