using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace Tiferet.Utilities.Json;

/// <summary>
/// Resolves <see cref="JsonSerializerOptions"/> for a type based on its
/// <see cref="JsonNamingAttribute"/>. Caches resolved options per type.
/// </summary>
public static class ConventionNamingResolver
{
    private static readonly ConcurrentDictionary<Type, JsonSerializerOptions> _cache = new();

    /// <summary>
    /// Get <see cref="JsonSerializerOptions"/> configured with the naming policy
    /// declared by <see cref="JsonNamingAttribute"/> on <typeparamref name="T"/>.
    /// Falls back to <see cref="NamingConvention.PascalCase"/> (no policy) when
    /// the attribute is absent.
    /// </summary>
    /// <typeparam name="T">The type to resolve options for.</typeparam>
    /// <returns>Cached <see cref="JsonSerializerOptions"/>.</returns>
    public static JsonSerializerOptions GetOptionsForType<T>()
        => GetOptionsForType(typeof(T));

    /// <summary>
    /// Get <see cref="JsonSerializerOptions"/> configured with the naming policy
    /// declared by <see cref="JsonNamingAttribute"/> on <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to resolve options for.</param>
    /// <returns>Cached <see cref="JsonSerializerOptions"/>.</returns>
    public static JsonSerializerOptions GetOptionsForType(Type type)
    {
        return _cache.GetOrAdd(type, t =>
        {
            var attr = t.GetCustomAttribute<JsonNamingAttribute>(inherit: true);
            var convention = attr?.Convention ?? NamingConvention.PascalCase;

            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = ResolvePolicy(convention),
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            };
        });
    }

    /// <summary>
    /// Resolve a <see cref="JsonNamingPolicy"/> from a <see cref="NamingConvention"/>.
    /// </summary>
    /// <param name="convention">The naming convention.</param>
    /// <returns>The corresponding <see cref="JsonNamingPolicy"/>, or <c>null</c> for PascalCase.</returns>
    public static JsonNamingPolicy? ResolvePolicy(NamingConvention convention) => convention switch
    {
        NamingConvention.CamelCase => JsonNamingPolicy.CamelCase,
        NamingConvention.SnakeCase => JsonNamingPolicy.SnakeCaseLower,
        NamingConvention.PascalCase => null, // Default .NET behavior.
        _ => null,
    };
}
