using System.Reflection;

namespace Tiferet.Core;

/// <summary>
/// Static utility to import a type by assembly-qualified or namespace-qualified name.
/// </summary>
public static class ImportDependency
{
    /// <summary>
    /// Resolve a <see cref="Type"/> from an assembly path and class name.
    /// </summary>
    /// <param name="assemblyName">The assembly name containing the type.</param>
    /// <param name="className">The fully qualified class name.</param>
    /// <returns>The resolved type.</returns>
    public static Type Execute(string assemblyName, string className)
    {
        try
        {
            var assembly = Assembly.Load(assemblyName);
            return assembly.GetType(className)
                ?? throw new TypeLoadException(
                    $"Type '{className}' not found in assembly '{assemblyName}'.");
        }
        catch (Exception ex) when (ex is not TiferetException)
        {
            throw new TiferetException(
                ErrorCodes.ImportDependencyFailed,
                $"Failed to import {className} from {assemblyName}.",
                ("assemblyName", assemblyName),
                ("className", className),
                ("exception", ex.Message));
        }
    }
}
