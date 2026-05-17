namespace Tiferet.Interfaces;

/// <summary>
/// Abstraction for providing authentication tokens to HTTP repositories.
/// Implementations may retrieve tokens from OAuth flows, API key stores,
/// or other credential sources.
/// </summary>
public interface IAuthTokenProvider
{
    /// <summary>
    /// Get the current authentication token, or <c>null</c> if no token is available.
    /// </summary>
    /// <returns>The token string, or <c>null</c>.</returns>
    Task<string?> GetTokenAsync();
}
