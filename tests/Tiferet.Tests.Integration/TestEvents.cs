using System.Globalization;
using Tiferet.Events;

namespace Tiferet.Tests.Integration;

// *** Parameter records

public sealed record AddDoubleParams(string A, string B);
public sealed record SingleValueParams(string A);

// *** Test domain events

/// <summary>
/// Test event that adds two numbers (string inputs → double output).
/// Used by integration tests to verify the full DI + feature pipeline.
/// </summary>
public class AddDoubleEvent : DomainEvent<AddDoubleParams, double>
{
    public override double Execute(AddDoubleParams p)
    {
        double.TryParse(p.A, NumberStyles.Any, CultureInfo.InvariantCulture, out var a);
        double.TryParse(p.B, NumberStyles.Any, CultureInfo.InvariantCulture, out var b);
        return a + b;
    }
}

/// <summary>
/// Test event that divides two numbers. Raises DIVISION_BY_ZERO if b is zero.
/// </summary>
public class DivideDoubleEvent : DomainEvent<AddDoubleParams, double>
{
    public override double Execute(AddDoubleParams p)
    {
        double.TryParse(p.A, NumberStyles.Any, CultureInfo.InvariantCulture, out var a);
        double.TryParse(p.B, NumberStyles.Any, CultureInfo.InvariantCulture, out var b);

        Verify(b != 0, "DIVISION_BY_ZERO", "Cannot divide by zero.");
        return a / b;
    }
}

/// <summary>
/// Test event that always raises a TiferetException, used to exercise error handling.
/// </summary>
public class ErroringEvent : DomainEvent<SingleValueParams, string>
{
    public override string Execute(SingleValueParams p)
    {
        RaiseError("TEST_ERROR", $"Always fails: {p.A}", ("input", p.A));
        return null!; // unreachable
    }
}

/// <summary>
/// Test event that stores its result in the data pipeline by DataKey.
/// Returns A concatenated with B for multi-step pipeline tests.
/// </summary>
public class ConcatEvent : DomainEvent<AddDoubleParams, string>
{
    public override string Execute(AddDoubleParams p) => $"{p.A}-{p.B}";
}
