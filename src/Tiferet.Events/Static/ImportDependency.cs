namespace Tiferet.Events.Static;

/// <summary>
/// Backward-compatible wrapper. Delegates to <see cref="Core.ImportDependency"/>.
/// </summary>
public static class ImportDependency
{
    /// <inheritdoc cref="Core.ImportDependency.Execute"/>
    public static Type Execute(string assemblyName, string className)
        => Core.ImportDependency.Execute(assemblyName, className);
}
