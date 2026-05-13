using Tiferet.Core;

namespace Tiferet.Events;

/// <summary>
/// Base class for domain events.
/// Provides <see cref="Verify"/> and <see cref="RaiseError"/> infrastructure.
/// Concrete events define their own strongly-typed <c>Execute</c> methods.
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// Verify an expression and raise a <see cref="TiferetException"/> if it is false.
    /// </summary>
    /// <param name="expression">The expression to verify.</param>
    /// <param name="errorCode">The error code to raise on failure.</param>
    /// <param name="message">Optional error message.</param>
    /// <param name="context">Additional error context pairs.</param>
    public void Verify(
        bool expression,
        string errorCode,
        string? message = null,
        params (string Key, object Value)[] context)
    {
        if (!expression)
            RaiseError(errorCode, message, context);
    }

    /// <summary>
    /// Raise a structured <see cref="TiferetException"/>.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="message">Optional error message.</param>
    /// <param name="context">Additional error context pairs.</param>
    public static void RaiseError(
        string errorCode,
        string? message = null,
        params (string Key, object Value)[] context)
    {
        throw new TiferetException(errorCode, message, context);
    }
}

/// <summary>
/// Generic base for domain events with typed input and typed output.
/// Use a record for <typeparamref name="TParams"/> when multiple fields are needed.
/// </summary>
/// <typeparam name="TParams">The input parameter type.</typeparam>
/// <typeparam name="TResult">The return type of the event execution.</typeparam>
public abstract class DomainEvent<TParams, TResult> : DomainEvent
{
    /// <summary>
    /// Execute the domain event with strongly-typed parameters.
    /// </summary>
    /// <param name="parameters">The typed input parameters.</param>
    /// <returns>The event result.</returns>
    public abstract TResult Execute(TParams parameters);
}
