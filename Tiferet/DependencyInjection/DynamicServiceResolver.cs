using Microsoft.Extensions.DependencyInjection;

namespace Tiferet.DependencyInjection;

/// <summary>
/// Concrete <see cref="IServiceResolver"/> backed by
/// <see cref="Microsoft.Extensions.DependencyInjection"/>.
/// The <see cref="IServiceProvider"/> is built lazily on the first
/// <see cref="GetService"/> call after any mutation, rather than on
/// every <c>AddService</c>/<c>RemoveService</c> call.
/// </summary>
public class DynamicServiceResolver : IServiceResolver
{
    private readonly ServiceCollection _services = new();

    // Null means the provider is stale and will be rebuilt on next GetService.
    private IServiceProvider? _provider;

    /// <inheritdoc />
    public void AddService<T>(T instance) where T : class
    {
        _services.AddSingleton(instance);
        _provider = null;
    }

    /// <inheritdoc />
    public void AddService<T>(Func<IServiceProvider, T> factory) where T : class
    {
        _services.AddSingleton<T>(factory);
        _provider = null;
    }

    /// <inheritdoc />
    public void AddService(Type serviceType, object instance)
    {
        _services.AddSingleton(serviceType, instance);
        _provider = null;
    }

    /// <inheritdoc />
    public T? GetService<T>() where T : class
    {
        return EnsureProvider().GetService<T>();
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType)
    {
        return EnsureProvider().GetService(serviceType);
    }

    /// <inheritdoc />
    public void RemoveService<T>() where T : class
    {
        // Remove all descriptors matching the service type.
        var toRemove = _services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var descriptor in toRemove)
            _services.Remove(descriptor);
        _provider = null;
    }

    /// <inheritdoc />
    public Func<T> BuildFactory<T>() where T : class
    {
        return () => ActivatorUtilities.CreateInstance<T>(EnsureProvider());
    }

    /// <summary>
    /// Build (or return the cached) <see cref="IServiceProvider"/>.
    /// Called on every read operation; building occurs at most once per
    /// batch of mutations.
    /// </summary>
    private IServiceProvider EnsureProvider()
        => _provider ??= _services.BuildServiceProvider();
}
