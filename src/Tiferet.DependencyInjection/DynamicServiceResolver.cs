using Microsoft.Extensions.DependencyInjection;

namespace Tiferet.DependencyInjection;

/// <summary>
/// Concrete <see cref="IServiceResolver"/> backed by
/// <see cref="Microsoft.Extensions.DependencyInjection"/>.
/// Rebuilds the <see cref="IServiceProvider"/> on each mutation.
/// </summary>
public class DynamicServiceResolver : IServiceResolver
{
    private readonly ServiceCollection _services = new();
    private IServiceProvider _provider;

    /// <summary>
    /// Initializes a new <see cref="DynamicServiceResolver"/> with an empty container.
    /// </summary>
    public DynamicServiceResolver()
    {
        _provider = _services.BuildServiceProvider();
    }

    /// <inheritdoc />
    public void AddService<T>(T instance) where T : class
    {
        _services.AddSingleton(instance);
        Rebuild();
    }

    /// <inheritdoc />
    public void AddService<T>(Func<IServiceProvider, T> factory) where T : class
    {
        _services.AddSingleton<T>(factory);
        Rebuild();
    }

    /// <inheritdoc />
    public void AddService(Type serviceType, object instance)
    {
        _services.AddSingleton(serviceType, instance);
        Rebuild();
    }

    /// <inheritdoc />
    public T? GetService<T>() where T : class
    {
        return _provider.GetService<T>();
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType)
    {
        return _provider.GetService(serviceType);
    }

    /// <inheritdoc />
    public void RemoveService<T>() where T : class
    {
        // Remove all descriptors matching the service type.
        var toRemove = _services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var descriptor in toRemove)
            _services.Remove(descriptor);
        Rebuild();
    }

    /// <inheritdoc />
    public Func<T> BuildFactory<T>() where T : class
    {
        return () => ActivatorUtilities.CreateInstance<T>(_provider);
    }

    /// <summary>
    /// Rebuild the service provider after a mutation.
    /// </summary>
    private void Rebuild()
    {
        _provider = _services.BuildServiceProvider();
    }
}
