using Tiferet.Events;
using System.Text;

using Tiferet.Assets;

namespace Tiferet.Utilities;

/// <summary>
/// Utility for CSV operations with dictionary-based rows.
/// Extends <see cref="CsvLoader"/> with header awareness.
/// </summary>
public class CsvDictLoader : CsvLoader
{
    /// <summary>The field names (column headers).</summary>
    public IReadOnlyList<string>? FieldNames { get; private set; }

    /// <summary>
    /// Initializes a new <see cref="CsvDictLoader"/>.
    /// </summary>
    /// <param name="path">Path to the CSV file.</param>
    /// <param name="fileMode">The file mode.</param>
    /// <param name="access">The file access level.</param>
    /// <param name="fieldNames">Optional field names. If null when reading, inferred from the first row.</param>
    /// <param name="encoding">Text encoding (defaults to UTF-8).</param>
    public CsvDictLoader(
        string path,
        FileMode fileMode = FileMode.Open,
        FileAccess access = FileAccess.Read,
        IReadOnlyList<string>? fieldNames = null,
        Encoding? encoding = null)
        : base(path, fileMode, access, encoding)
    {
        FieldNames = fieldNames;
    }

    /// <summary>Create a <see cref="CsvDictLoader"/> configured for reading.</summary>
    public static CsvDictLoader ForReading(string path, IReadOnlyList<string>? fieldNames = null, Encoding? encoding = null)
        => new(path, FileMode.Open, FileAccess.Read, fieldNames, encoding);

    /// <summary>Create a <see cref="CsvDictLoader"/> configured for writing.</summary>
    public static CsvDictLoader ForWriting(string path, IReadOnlyList<string> fieldNames, Encoding? encoding = null)
        => new(path, FileMode.Create, FileAccess.Write, fieldNames, encoding);

    /// <summary>
    /// Read field names from the first row if they haven't been set.
    /// </summary>
    private void EnsureFieldNames()
    {
        if (FieldNames != null) return;
        EnsureReader();
        var line = Reader!.ReadLine()
            ?? throw new TiferetException(ErrorCodes.CsvDictNoHeader, null, ("path", FilePath));
        FieldNames = CsvParser.ParseRow(line).AsReadOnly();
    }

    /// <summary>
    /// Read the next row as a dictionary mapping field names to values.
    /// </summary>
    /// <returns>A dictionary for the next row, or an empty dictionary at EOF.</returns>
    public Dictionary<string, string> ReadDictRow()
    {
        EnsureFieldNames();
        EnsureReader();
        var line = Reader!.ReadLine();
        if (line == null) return new();

        var values = CsvParser.ParseRow(line);
        var dict = new Dictionary<string, string>();
        for (var i = 0; i < FieldNames!.Count; i++)
            dict[FieldNames[i]] = i < values.Count ? values[i] : "";
        return dict;
    }

    /// <summary>
    /// Read all remaining rows as dictionaries.
    /// </summary>
    /// <returns>A list of dictionaries.</returns>
    public List<Dictionary<string, string>> ReadAllDicts()
    {
        EnsureFieldNames();
        EnsureReader();
        var rows = new List<Dictionary<string, string>>();
        string? line;
        while ((line = Reader!.ReadLine()) != null)
        {
            var values = CsvParser.ParseRow(line);
            var dict = new Dictionary<string, string>();
            for (var i = 0; i < FieldNames!.Count; i++)
                dict[FieldNames[i]] = i < values.Count ? values[i] : "";
            rows.Add(dict);
        }
        return rows;
    }

    /// <summary>
    /// Write the header row using the configured field names.
    /// </summary>
    public void WriteHeader()
    {
        if (FieldNames == null)
            throw new TiferetException(ErrorCodes.CsvFieldnamesRequired, null, ("path", FilePath));
        EnsureWriter();
        Writer!.WriteLine(CsvParser.FormatRow(FieldNames));
    }

    /// <summary>
    /// Write a single dictionary row.
    /// </summary>
    /// <param name="row">A dictionary mapping field names to values.</param>
    public void WriteDictRow(Dictionary<string, string> row)
    {
        if (FieldNames == null)
            throw new TiferetException(ErrorCodes.CsvFieldnamesRequired, null, ("path", FilePath));
        EnsureWriter();
        var values = FieldNames.Select(f => row.TryGetValue(f, out var v) ? v : "");
        Writer!.WriteLine(CsvParser.FormatRow(values));
    }

    /// <summary>
    /// Write multiple dictionary rows.
    /// </summary>
    /// <param name="rows">The rows to write.</param>
    public void WriteAllDicts(IEnumerable<Dictionary<string, string>> rows)
    {
        foreach (var row in rows)
            WriteDictRow(row);
    }

    /// <summary>
    /// Yield rows as dictionaries with optional 1-based line-range filtering
    /// (line numbers exclude the header).
    /// </summary>
    public IEnumerable<Dictionary<string, string>> YieldDictRows(int? startLine = null, int? endLine = null)
    {
        EnsureFieldNames();
        EnsureReader();
        var lineNum = 0;
        string? line;
        while ((line = Reader!.ReadLine()) != null)
        {
            lineNum++;
            if (startLine.HasValue && lineNum < startLine.Value) continue;
            if (endLine.HasValue && lineNum > endLine.Value) break;

            var values = CsvParser.ParseRow(line);
            var dict = new Dictionary<string, string>();
            for (var i = 0; i < FieldNames!.Count; i++)
                dict[FieldNames[i]] = i < values.Count ? values[i] : "";
            yield return dict;
        }
    }

    // --- Static helpers ---

    /// <summary>Load all rows from a CSV file as dictionaries.</summary>
    public static List<Dictionary<string, string>> LoadDictRows(
        string path,
        IReadOnlyList<string>? fieldNames = null,
        Encoding? encoding = null)
    {
        using var loader = ForReading(path, fieldNames, encoding);
        return loader.ReadAllDicts();
    }

    /// <summary>Save dictionary rows to a CSV file (overwrites), with header.</summary>
    public static void SaveDictRows(
        string path,
        IReadOnlyList<string> fieldNames,
        IEnumerable<Dictionary<string, string>> rows,
        bool includeHeader = true,
        Encoding? encoding = null)
    {
        using var loader = ForWriting(path, fieldNames, encoding);
        if (includeHeader) loader.WriteHeader();
        loader.WriteAllDicts(rows);
    }
}
