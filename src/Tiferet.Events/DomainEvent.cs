using Tiferet.Core;

namespace Tiferet.Events;

/// <summary>
/// Base class for domain events.
/// Provides <see cref="Verify"/> and <see cref="RaiseError"/> infrastructure,
/// plus a non-generic <see cref="Execute(Dictionary{string, object?})"/> overload
/// for runtime-driven invocation (e.g., feature pipeline).
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

    /// <summary>
    /// Execute the domain event from a dictionary of parameters.
    /// Used by the feature pipeline for runtime-driven invocation
    /// where <c>TParams</c> is not known at compile time.
    /// </summary>
    /// <param name="data">The parameter dictionary.</param>
    /// <returns>The event result as an untyped object.</returns>
    public abstract object? Execute(Dictionary<string, object?> data);
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

    /// <summary>
    /// Execute the domain event from a dictionary of parameters.
    /// Constructs <typeparamref name="TParams"/> via reflection, matching
    /// dictionary keys to constructor parameters (case-insensitive).
    /// Delegates to the typed <see cref="Execute(TParams)"/>.
    /// </summary>
    /// <param name="data">The parameter dictionary.</param>
    /// <returns>The event result.</returns>
    public override object? Execute(Dictionary<string, object?> data)
    {
        var parameters = ConstructParams(data);
        return Execute(parameters);
    }

    /// <summary>
    /// Construct a <typeparamref name="TParams"/> instance from a dictionary
    /// by matching keys to the longest public constructor's parameter names.
    /// Delegates to <see cref="ReflectionActivator.Construct{T}"/>.
    /// </summary>
    private static TParams ConstructParams(Dictionary<string, object?> data)
        => ReflectionActivator.Construct<TParams>(data);
}
