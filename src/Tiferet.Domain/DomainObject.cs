using System.Reflection;

namespace Tiferet.Domain;

/// <summary>
/// Abstract base record for all Tiferet domain models.
/// Provides value equality, immutability, and <c>with</c> expression support.
/// Domain objects are read-only; mutation logic lives on Aggregate subclasses.
/// </summary>
public abstract record DomainObject
{
    /// <summary>
    /// Construct a domain record from a dictionary of property values.
    /// Matches dictionary keys to constructor parameters (case-insensitive).
    /// </summary>
    /// <typeparam name="T">The concrete domain record type.</typeparam>
    /// <param name="data">The property dictionary to hydrate from.</param>
    /// <returns>A new instance of <typeparamref name="T"/>.</returns>
    public static T FromDictionary<T>(Dictionary<string, object> data) where T : DomainObject
    {
        var ctor = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .First();

        var parameters = ctor.GetParameters();
        var args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var key = param.Name!;

            // Try exact match, then PascalCase, then case-insensitive.
            if (!data.TryGetValue(key, out var value))
            {
                var pascalKey = char.ToUpperInvariant(key[0]) + key[1..];
                if (!data.TryGetValue(pascalKey, out value))
                {
                    // Case-insensitive fallback.
                    var match = data.Keys.FirstOrDefault(k =>
                        string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
                    if (match is not null)
                        value = data[match];
                }
            }

            if (value is not null)
                args[i] = ConvertValue(value, param.ParameterType);
            else if (param.HasDefaultValue)
                args[i] = param.DefaultValue;
            else
                args[i] = param.ParameterType.IsValueType
                    ? Activator.CreateInstance(param.ParameterType)
                    : null;
        }

        return (T)ctor.Invoke(args);
    }

    private static object? ConvertValue(object value, Type targetType)
    {
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlying.IsAssignableFrom(value.GetType()))
            return value;

        // Handle List<Dictionary<string,string>> -> IReadOnlyList<T> for nested records.
        if (value is IList<Dictionary<string, string>> dictList
            && underlying.IsGenericType
            && underlying.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
        {
            var elementType = underlying.GetGenericArguments()[0];
            if (typeof(DomainObject).IsAssignableFrom(elementType))
            {
                var method = typeof(DomainObject).GetMethod(nameof(FromDictionary))!
                    .MakeGenericMethod(elementType);
                var listType = typeof(List<>).MakeGenericType(elementType);
                var list = (System.Collections.IList)Activator.CreateInstance(listType)!;
                foreach (var dict in dictList)
                {
                    var objDict = dict.ToDictionary(kv => kv.Key, kv => (object)kv.Value);
                    list.Add(method.Invoke(null, [objDict]));
                }
                return list;
            }
        }

        return Convert.ChangeType(value, underlying);
    }
}
