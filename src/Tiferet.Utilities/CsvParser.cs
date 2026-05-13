using System.Text;

namespace Tiferet.Utilities;

/// <summary>
/// Internal RFC 4180 CSV field parser and formatter.
/// Handles quoting, comma-containing fields, and escaped double-quotes.
/// </summary>
internal static class CsvParser
{
    /// <summary>
    /// Parse a single CSV line into a list of field values.
    /// Handles quoted fields with embedded commas, double-quote escaping, and newlines.
    /// </summary>
    /// <param name="line">The raw CSV line.</param>
    /// <returns>A list of parsed field values.</returns>
    public static List<string> ParseRow(string line)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // Escaped double-quote inside quoted field.
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }
        }

        // Add the final field.
        fields.Add(field.ToString());
        return fields;
    }

    /// <summary>
    /// Format a sequence of field values into a single CSV line.
    /// Fields containing commas, double-quotes, or newlines are quoted.
    /// </summary>
    /// <param name="fields">The field values.</param>
    /// <returns>A formatted CSV line (without trailing newline).</returns>
    public static string FormatRow(IEnumerable<string> fields)
    {
        return string.Join(",", fields.Select(QuoteField));
    }

    private static string QuoteField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
