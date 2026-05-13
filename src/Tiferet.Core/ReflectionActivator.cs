using System.Reflection;

namespace Tiferet.Core;

/// <summary>
/// Infrastructure helper that constructs type instances from string-keyed dictionaries.
/// Used at the framework seams where typed construction from untyped data is required:
/// the feature pipeline's dictionary-to-TParams mapping and the mapper layer's
/// dictionary-to-domain-record mapping.
/// </summary>
/// <remarks>
/// Key-matching strategy (applied in order for each constructor parameter):
/// 1. Exact match against <paramref name="data"/> key.
/// 2. PascalCase of the parameter name (e.g., <c>name</c> → <c>Name</c>).
/// 3. Case-insensitive linear scan.
/// If no key matches, the parameter's default value is used (or a value-type
/// default / <c>null</c> for reference types).
/// </remarks>
public static class ReflectionActivator
{
    /// <summary>
    /// Construct an instance of <typeparamref name="T"/> by matching dictionary
    /// keys to the longest public constructor's parameter names.
    /// </summary>
    /// <typeparam name="T">The type to construct.</typeparam>
    /// <param name="data">The source data dictionary.</param>
    /// <returns>A new <typeparamref name="T"/> instance.</returns>
    public static T Construct<T>(Dictionary<string, object?> data)
    {
        // Select the public constructor with the most parameters (primary record ctor).
        var ctor = typeof(T)
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .First();

        var ctorParams = ctor.GetParameters();
        var args = new object?[ctorParams.Length];

        for (int i = 0; i < ctorParams.Length; i++)
        {
            var param = ctorParams[i];
            var key = param.Name!;

            // 1. Exact match.
            if (!data.TryGetValue(key, out var value))
            {
                // 2. PascalCase match (e.g., "name" → "Name").
                var pascalKey = char.ToUpperInvariant(key[0]) + key[1..];
                if (!data.TryGetValue(pascalKey, out value))
                {
                    // 3. Case-insensitive fallback.
                    var match = data.Keys.FirstOrDefault(k =>
                        string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
                    if (match is not null)
                        value = data[match];
                }
            }

            if (value is not null)
            {
                // Unwrap Nullable<T> before assignability check.
                var targetType = Nullable.GetUnderlyingType(param.ParameterType)
                    ?? param.ParameterType;
                args[i] = targetType.IsAssignableFrom(value.GetType())
                    ? value
                    : Convert.ChangeType(value, targetType);
            }
            else if (param.HasDefaultValue)
                args[i] = param.DefaultValue;
            else
                args[i] = param.ParameterType.IsValueType
                    ? Activator.CreateInstance(param.ParameterType)
                    : null;
        }

        return (T)ctor.Invoke(args);
    }
}
