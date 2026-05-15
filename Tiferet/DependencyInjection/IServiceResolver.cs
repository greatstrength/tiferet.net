using Tiferet.Interfaces;

namespace Tiferet.DependencyInjection;

/// <summary>
/// Abstract service resolution contract for the Tiferet DI container.
/// </summary>
public interface IServiceResolver : IService
{
    /// <summary>Register a service instance by type.</summary>
    /// <typeparam name="T">The service interface type.</typeparam>
    /// <param name="instance">The service instance to register.</param>
    void AddService<T>(T instance) where T : class;

    /// <summary>Register a service by type with a factory.</summary>
    /// <typeparam name="T">The service interface type.</typeparam>
    /// <param name="factory">A factory that produces the service instance.</param>
    void AddService<T>(Func<IServiceProvider, T> factory) where T : class;

    /// <summary>Register a service by runtime type and instance.</summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="instance">The service instance.</param>
    void AddService(Type serviceType, object instance);

    /// <summary>Resolve a service by type.</summary>
    /// <typeparam name="T">The service type to resolve.</typeparam>
    /// <returns>The resolved service instance, or null if not registered.</returns>
    T? GetService<T>() where T : class;

    /// <summary>Resolve a service by runtime type.</summary>
    /// <param name="serviceType">The service type to resolve.</param>
    /// <returns>The resolved service instance, or null if not registered.</returns>
    object? GetService(Type serviceType);

    /// <summary>Remove a registered service by type.</summary>
    /// <typeparam name="T">The service type to remove.</typeparam>
    void RemoveService<T>() where T : class;

    /// <summary>
    /// Build a factory delegate that constructs <typeparamref name="T"/>
    /// using constructor injection from the current service registrations.
    /// </summary>
    /// <typeparam name="T">The type to construct.</typeparam>
    /// <returns>A factory delegate.</returns>
    Func<T> BuildFactory<T>() where T : class;
}
