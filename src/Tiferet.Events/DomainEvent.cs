using System.Reflection;
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
    /// </summary>
    private static TParams ConstructParams(Dictionary<string, object?> data)
    {
        var ctor = typeof(TParams)
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .First();

        var ctorParams = ctor.GetParameters();
        var args = new object?[ctorParams.Length];

        for (int i = 0; i < ctorParams.Length; i++)
        {
            var param = ctorParams[i];
            var key = param.Name!;

            // Try exact match, then PascalCase, then case-insensitive.
            if (!data.TryGetValue(key, out var value))
            {
                var pascalKey = char.ToUpperInvariant(key[0]) + key[1..];
                if (!data.TryGetValue(pascalKey, out value))
                {
                    var match = data.Keys.FirstOrDefault(k =>
                        string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
                    if (match is not null)
                        value = data[match];
                }
            }

            if (value is not null)
            {
                var targetType = Nullable.GetUnderlyingType(param.ParameterType)
                    ?? param.ParameterType;
                args[i] = targetType.IsAssignableFrom(value.GetType())
                    ? value
                    : Convert.ChangeType(value, targetType);
            }
            else if (param.HasDefaultValue)
                args[i] = param.DefaultValue;
            else
                args[i] = param.ParameterType.IsValueType
                    ? Activator.CreateInstance(param.ParameterType)
                    : null;
        }

        return (TParams)ctor.Invoke(args);
    }
}
