using Tiferet.Assets;

namespace Tiferet.Events;

/// <summary>
/// Static utility to parse a parameter string, resolving environment variable references.
/// </summary>
public static class ParseParameter
{
    /// <summary>
    /// Parse a parameter. If the value starts with <c>$env.</c>, resolves the
    /// corresponding environment variable.
    /// </summary>
    /// <param name="parameter">The parameter string to parse.</param>
    /// <returns>The resolved parameter value.</returns>
    public static string Parse(string parameter)
    {
        if (parameter.StartsWith("$env."))
        {
            var envVar = parameter[5..];
            var result = Environment.GetEnvironmentVariable(envVar)
                ?? throw new TiferetException(
                    ErrorCodes.ParameterParsingFailed,
                    $"Environment variable '{envVar}' not found.",
                    ("parameter", parameter));
            return result;
        }

        return parameter;
    }
}
