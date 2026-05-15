using System.Collections.Concurrent;

namespace Tiferet.Contexts;

/// <summary>
/// Thread-safe key-value cache context.
/// </summary>
public class CacheContext
{
    private readonly ConcurrentDictionary<string, object?> _cache;

    /// <summary>
    /// Initializes a new <see cref="CacheContext"/> with an optional seed dictionary.
    /// </summary>
    /// <param name="seed">Optional initial entries.</param>
    public CacheContext(IDictionary<string, object?>? seed = null)
    {
        _cache = seed is not null
            ? new ConcurrentDictionary<string, object?>(seed)
            : new ConcurrentDictionary<string, object?>();
    }

    /// <summary>
    /// Retrieve an item from the cache, cast to <typeparamref name="T"/>.
    /// Returns <c>default</c> if the key is not found or the value is null.
    /// </summary>
    /// <typeparam name="T">The expected type.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached value or default.</returns>
    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var value) && value is T typed)
            return typed;
        return default;
    }

    /// <summary>
    /// Retrieve a raw item from the cache.
    /// Returns null if the key is not found.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached value or null.</returns>
    public object? Get(string key)
    {
        _cache.TryGetValue(key, out var value);
        return value;
    }

    /// <summary>Store an item in the cache.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to store.</param>
    public void Set(string key, object? value)
    {
        _cache[key] = value;
    }

    /// <summary>Remove an item from the cache.</summary>
    /// <param name="key">The cache key.</param>
    public void Delete(string key)
    {
        _cache.TryRemove(key, out _);
    }

    /// <summary>Clear all items from the cache.</summary>
    public void Clear()
    {
        _cache.Clear();
    }
}
