using System.Globalization;
using Tiferet.Events;

namespace Tiferet.Examples.Calculator.Events;

// *** Parameter records

/// <summary>Two-operand calculation parameters.</summary>
public sealed record CalcParams(string A, string B);

// *** Base event

/// <summary>
/// Abstract base domain event providing numeric validation for calculator operations.
/// Mirrors Python's BasicCalcEvent.verify_number().
/// </summary>
/// <typeparam name="TParams">The typed input parameter record.</typeparam>
/// <typeparam name="TResult">The return type.</typeparam>
public abstract class BasicCalcEvent<TParams, TResult> : DomainEvent<TParams, TResult>
{
    /// <summary>
    /// Verify that a string value is a valid number and return it as a double.
    /// Raises INVALID_INPUT if the value cannot be parsed.
    /// </summary>
    /// <param name="value">The string value to verify.</param>
    /// <returns>The parsed numeric value.</returns>
    protected static double VerifyNumber(string value)
    {
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        RaiseError("INVALID_INPUT",
            $"Invalid number: {value}",
            ("value", value));

        return 0; // unreachable
    }
}

// *** Events

/// <summary>Domain event to perform addition of two numbers.</summary>
public class AddNumber : BasicCalcEvent<CalcParams, double>
{
    /// <summary>Add a and b.</summary>
    public override double Execute(CalcParams p)
    {
        var a = VerifyNumber(p.A);
        var b = VerifyNumber(p.B);
        return a + b;
    }
}

/// <summary>Domain event to perform subtraction of two numbers.</summary>
public class SubtractNumber : BasicCalcEvent<CalcParams, double>
{
    /// <summary>Subtract b from a.</summary>
    public override double Execute(CalcParams p)
    {
        var a = VerifyNumber(p.A);
        var b = VerifyNumber(p.B);
        return a - b;
    }
}

/// <summary>Domain event to perform multiplication of two numbers.</summary>
public class MultiplyNumber : BasicCalcEvent<CalcParams, double>
{
    /// <summary>Multiply a and b.</summary>
    public override double Execute(CalcParams p)
    {
        var a = VerifyNumber(p.A);
        var b = VerifyNumber(p.B);
        return a * b;
    }
}

/// <summary>Domain event to perform division of two numbers.</summary>
public class DivideNumber : BasicCalcEvent<CalcParams, double>
{
    /// <summary>Divide a by b. Raises DIVISION_BY_ZERO if b is zero.</summary>
    public override double Execute(CalcParams p)
    {
        var a = VerifyNumber(p.A);
        var b = VerifyNumber(p.B);

        Verify(b != 0, "DIVISION_BY_ZERO", "Cannot divide by zero.");

        return a / b;
    }
}

/// <summary>Domain event to raise a number to the power of another. Also used for square roots.</summary>
public class ExponentiateNumber : BasicCalcEvent<CalcParams, double>
{
    /// <summary>Raise a to the power of b.</summary>
    public override double Execute(CalcParams p)
    {
        var a = VerifyNumber(p.A);
        var b = VerifyNumber(p.B);
        return Math.Pow(a, b);
    }
}
