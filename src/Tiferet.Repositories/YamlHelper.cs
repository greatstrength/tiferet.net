namespace Tiferet.Repositories;

/// <summary>
/// Internal helper for navigating and mutating YAML object trees
/// (YamlDotNet deserializes mappings as <c>Dictionary&lt;object, object&gt;</c>).
/// </summary>
internal static class YamlHelper
{
    /// <summary>
    /// Convert a <c>Dictionary&lt;object, object&gt;</c> to <c>Dictionary&lt;string, object&gt;</c>,
    /// recursively converting nested dictionaries.
    /// </summary>
    public static Dictionary<string, object> ToStringDict(object? raw)
    {
        if (raw is Dictionary<object, object> objDict)
        {
            var result = new Dictionary<string, object>();
            foreach (var (key, value) in objDict)
                result[key.ToString()!] = value is Dictionary<object, object> nested
                    ? ToStringDict(nested)
                    : value;
            return result;
        }

        if (raw is Dictionary<string, object> strDict)
            return strDict;

        return new Dictionary<string, object>();
    }

    /// <summary>
    /// Navigate into a nested dictionary tree following the given key path.
    /// Returns null if any key is missing along the path.
    /// </summary>
    public static object? GetNestedValue(object? root, params string[] keys)
    {
        var current = root;
        foreach (var key in keys)
        {
            if (current is Dictionary<object, object> objDict)
            {
                if (!objDict.TryGetValue(key, out current))
                    return null;
            }
            else if (current is Dictionary<string, object> strDict)
            {
                if (!strDict.TryGetValue(key, out current))
                    return null;
            }
            else
            {
                return null;
            }
        }
        return current;
    }

    /// <summary>
    /// Set a value at a nested key path, creating intermediate dictionaries as needed.
    /// Operates on <c>Dictionary&lt;object, object&gt;</c> trees (YamlDotNet format).
    /// </summary>
    public static void SetNestedValue(Dictionary<object, object> root, object value, params string[] keys)
    {
        var current = root;
        for (var i = 0; i < keys.Length - 1; i++)
        {
            if (!current.TryGetValue(keys[i], out var next) || next is not Dictionary<object, object>)
            {
                next = new Dictionary<object, object>();
                current[keys[i]] = next;
            }
            current = (Dictionary<object, object>)next;
        }
        current[keys[^1]] = value;
    }

    /// <summary>
    /// Remove a value at a nested key path. Returns true if the key was found and removed.
    /// </summary>
    public static bool RemoveNestedValue(Dictionary<object, object> root, params string[] keys)
    {
        var current = root;
        for (var i = 0; i < keys.Length - 1; i++)
        {
            if (!current.TryGetValue(keys[i], out var next) || next is not Dictionary<object, object> nextDict)
                return false;
            current = nextDict;
        }
        return current.Remove(keys[^1]);
    }

    /// <summary>
    /// Get all entries at a nested key path as a string-keyed dictionary.
    /// Returns empty dict if the path doesn't exist.
    /// </summary>
    public static Dictionary<string, object> GetSection(object? root, params string[] keys)
    {
        var node = GetNestedValue(root, keys);
        return ToStringDict(node);
    }

    /// <summary>
    /// Dehydrate a domain record into a <c>Dictionary&lt;object, object&gt;</c>
    /// suitable for YAML serialization. Excludes null values and the specified properties.
    /// </summary>
    public static Dictionary<object, object> DehydrateRecord(
        object record,
        HashSet<string>? excludeProperties = null)
    {
        var result = new Dictionary<object, object>();
        var properties = record.GetType().GetProperties(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        foreach (var prop in properties)
        {
            if (excludeProperties?.Contains(prop.Name) == true)
                continue;

            var value = prop.GetValue(record);
            if (value is null) continue;

            var key = prop.Name;
            result[key] = DehydrateValue(value);
        }

        return result;
    }

    /// <summary>
    /// Recursively convert a value for YAML serialization.
    /// Handles nested domain records, lists, and dictionaries.
    /// </summary>
    private static object DehydrateValue(object value)
    {
        // IReadOnlyDictionary → Dictionary<object, object>.
        if (value is System.Collections.IDictionary dict)
        {
            var result = new Dictionary<object, object>();
            foreach (System.Collections.DictionaryEntry entry in dict)
                result[entry.Key] = entry.Value is not null ? DehydrateValue(entry.Value) : entry.Value!;
            return result;
        }

        // IReadOnlyList of domain records → List of dehydrated dicts.
        if (value is System.Collections.IList list && value is not string)
        {
            var result = new List<object>();
            foreach (var item in list)
                result.Add(DehydrateValue(item));
            return result;
        }

        // Domain records → nested dict.
        if (value is Domain.DomainObject domainObj)
            return DehydrateRecord(domainObj);

        // Enums → string.
        if (value is Enum e)
            return e.ToString();

        return value;
    }
}
