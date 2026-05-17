using System.Text.Json;
using Tiferet.Domain;
using Tiferet.Events;

namespace Tiferet.Utilities.Json;

/// <summary>
/// Static utility class providing convention-aware JSON serialization and deserialization.
/// Resolves <see cref="JsonSerializerOptions"/> from the target type's
/// <see cref="JsonNamingAttribute"/> via <see cref="ConventionNamingResolver"/>.
/// </summary>
public static class JsonSerializerHelper
{
    /// <summary>
    /// Deserialize a JSON string to <typeparamref name="T"/> using the naming convention
    /// declared on the type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance.</returns>
    /// <exception cref="TiferetException">Thrown with <see cref="ErrorCodes.HttpDeserializationFailed"/> on failure.</exception>
    public static T Deserialize<T>(string json)
    {
        try
        {
            var options = ConventionNamingResolver.GetOptionsForType<T>();
            return JsonSerializer.Deserialize<T>(json, options)
                ?? throw new TiferetException(
                    ErrorCodes.HttpDeserializationFailed,
                    $"Deserialization of {typeof(T).Name} returned null.",
                    ("type", typeof(T).Name));
        }
        catch (TiferetException) { throw; }
        catch (JsonException ex)
        {
            throw new TiferetException(
                ErrorCodes.HttpDeserializationFailed,
                $"Failed to deserialize JSON to {typeof(T).Name}.",
                ("type", typeof(T).Name),
                ("error", ex.Message));
        }
    }

    /// <summary>
    /// Serialize an object to JSON using the naming convention declared on the type.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>The JSON string.</returns>
    public static string Serialize<T>(T obj)
    {
        var options = ConventionNamingResolver.GetOptionsForType<T>();
        return JsonSerializer.Serialize(obj, options);
    }

    /// <summary>
    /// Get the resolved <see cref="JsonSerializerOptions"/> for <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to resolve options for.</typeparam>
    /// <returns>The cached options instance.</returns>
    public static JsonSerializerOptions GetOptions<T>()
        => ConventionNamingResolver.GetOptionsForType<T>();
}
