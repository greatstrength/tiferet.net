using Tiferet.Utilities;

namespace Tiferet.Events;

/// <summary>
/// Base class for asynchronous domain events.
/// Extends <see cref="DomainEvent"/> with an async execution path and provides
/// a synchronous adapter so async events work in the existing sync pipeline
/// without deadlocking.
/// </summary>
public abstract class AsyncDomainEvent : DomainEvent
{
    /// <summary>
    /// Execute the domain event asynchronously from a dictionary of parameters.
    /// Used by the async feature pipeline for runtime-driven invocation.
    /// </summary>
    /// <param name="data">The parameter dictionary.</param>
    /// <returns>The event result.</returns>
    public abstract Task<object?> ExecuteAsync(Dictionary<string, object?> data);

    /// <summary>
    /// Synchronous adapter for the existing pipeline.
    /// Delegates to <see cref="ExecuteAsync"/> via <c>Task.Run</c> to avoid
    /// <c>SynchronizationContext</c> deadlocks in UI or ASP.NET contexts.
    /// </summary>
    public override object? Execute(Dictionary<string, object?> data)
    {
        return Task.Run(() => ExecuteAsync(data)).GetAwaiter().GetResult();
    }
}

/// <summary>
/// Generic base for asynchronous domain events with typed input and typed output.
/// </summary>
/// <typeparam name="TParams">The input parameter type.</typeparam>
/// <typeparam name="TResult">The return type of the event execution.</typeparam>
public abstract class AsyncDomainEvent<TParams, TResult> : AsyncDomainEvent
{
    /// <summary>
    /// Execute the domain event asynchronously with strongly-typed parameters.
    /// </summary>
    /// <param name="parameters">The typed parameters.</param>
    /// <returns>The typed result.</returns>
    public abstract Task<TResult> ExecuteAsync(TParams parameters);

    /// <summary>
    /// Execute the domain event asynchronously from a dictionary of parameters.
    /// Constructs <typeparamref name="TParams"/> via reflection and delegates
    /// to the typed <see cref="ExecuteAsync(TParams)"/>.
    /// </summary>
    public override async Task<object?> ExecuteAsync(Dictionary<string, object?> data)
    {
        var parameters = ReflectionActivator.Construct<TParams>(data);
        return await ExecuteAsync(parameters);
    }
}
