namespace Tiferet.Events.Static;

/// <summary>
/// Backward-compatible wrapper. Delegates to <see cref="Core.ParseParameter"/>.
/// </summary>
public static class ParseParameter
{
    /// <inheritdoc cref="Core.ParseParameter.Execute"/>
    public static string Execute(string parameter)
        => Core.ParseParameter.Execute(parameter);
}
