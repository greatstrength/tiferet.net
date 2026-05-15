namespace Tiferet.Events;

/// <summary>
/// API-facing exception returned for Tiferet API errors.
/// Extends <see cref="TiferetException"/> with a descriptive name.
/// </summary>
public class TiferetApiException : TiferetException
{
    /// <summary>A descriptive name for the error.</summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new <see cref="TiferetApiException"/>.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="name">A descriptive name for the error.</param>
    /// <param name="message">The user-facing error message.</param>
    /// <param name="context">Optional context pairs providing error detail.</param>
    public TiferetApiException(
        string errorCode,
        string name,
        string message,
        params (string Key, object Value)[] context)
        : base(errorCode, message, context)
    {
        Name = name;
    }
}
