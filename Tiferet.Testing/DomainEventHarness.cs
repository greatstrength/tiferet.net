using Tiferet.Events;

namespace Tiferet.Testing;

/// <summary>
/// Base helper class for testing domain events.
/// Provides convenience methods for executing events and asserting on exceptions.
/// </summary>
public static class DomainEventHarness
{
    /// <summary>
    /// Execute a domain event with typed parameters and return the result.
    /// </summary>
    public static TResult Execute<TEvent, TParams, TResult>(TParams parameters, Action<TEvent>? configure = null)
        where TEvent : DomainEvent<TParams, TResult>, new()
    {
        var evt = new TEvent();
        configure?.Invoke(evt);
        return evt.Execute(parameters);
    }

    /// <summary>
    /// Assert that executing a domain event throws a <see cref="TiferetException"/>
    /// with the given error code.
    /// </summary>
    public static TiferetException AssertThrows<TEvent, TParams, TResult>(
        TParams parameters,
        string expectedErrorCode,
        Action<TEvent>? configure = null)
        where TEvent : DomainEvent<TParams, TResult>, new()
    {
        var evt = new TEvent();
        configure?.Invoke(evt);

        try
        {
            evt.Execute(parameters);
            throw new InvalidOperationException(
                $"Expected TiferetException with error code '{expectedErrorCode}' but no exception was thrown.");
        }
        catch (TiferetException ex) when (ex.ErrorCode == expectedErrorCode)
        {
            return ex;
        }
    }
}
