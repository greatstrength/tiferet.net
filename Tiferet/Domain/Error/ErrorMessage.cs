using System.Text.RegularExpressions;

namespace Tiferet.Domain.Error;

/// <summary>
/// An error message object with language-specific text.
/// </summary>
/// <param name="Lang">The language of the error message text (e.g., "en_US").</param>
/// <param name="Text">The error message text, supporting named placeholders like {value}.</param>
public sealed record ErrorMessageConfiguration(string Lang, string Text) : DomainObject
{
    /// <summary>
    /// Format the error message text with named-placeholder substitution.
    /// </summary>
    /// <param name="args">Named arguments for placeholder substitution.</param>
    /// <returns>The formatted error message text.</returns>
    public string Format(Dictionary<string, object>? args = null)
    {
        if (args is null || args.Count == 0)
            return Text;

        // Replace {key} placeholders with values from the dictionary.
        return Regex.Replace(Text, @"\{(\w+)\}", match =>
        {
            var key = match.Groups[1].Value;
            return args.TryGetValue(key, out var value) ? value?.ToString() ?? "" : match.Value;
        });
    }
}
