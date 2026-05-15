using System.Text;

using Tiferet.Assets;

namespace Tiferet.Utilities;

/// <summary>
/// Utility for CSV operations with list-based rows.
/// Extends <see cref="FileLoader"/> for stream lifecycle management.
/// </summary>
public class CsvLoader : FileLoader
{
    /// <summary>The <see cref="StreamReader"/> for reading operations (lazy).</summary>
    protected StreamReader? Reader { get; set; }

    /// <summary>The <see cref="StreamWriter"/> for writing operations (lazy).</summary>
    protected StreamWriter? Writer { get; set; }

    /// <summary>
    /// Initializes a new <see cref="CsvLoader"/>.
    /// </summary>
    /// <param name="path">Path to the CSV file.</param>
    /// <param name="fileMode">The file mode.</param>
    /// <param name="access">The file access level.</param>
    /// <param name="encoding">Text encoding (defaults to UTF-8).</param>
    public CsvLoader(
        string path,
        FileMode fileMode = FileMode.Open,
        FileAccess access = FileAccess.Read,
        Encoding? encoding = null)
        : base(path, fileMode, access, encoding)
    {
    }

    /// <summary>Create a <see cref="CsvLoader"/> configured for reading.</summary>
    public static new CsvLoader ForReading(string path, Encoding? encoding = null)
        => new(path, FileMode.Open, FileAccess.Read, encoding);

    /// <summary>Create a <see cref="CsvLoader"/> configured for writing (creates or overwrites).</summary>
    public static new CsvLoader ForWriting(string path, Encoding? encoding = null)
        => new(path, FileMode.Create, FileAccess.Write, encoding);

    /// <summary>Create a <see cref="CsvLoader"/> configured for appending.</summary>
    public static CsvLoader ForAppending(string path, Encoding? encoding = null)
        => new(path, FileMode.Append, FileAccess.Write, encoding);

    /// <summary>
    /// Ensure the stream is open and a <see cref="StreamReader"/> is initialized.
    /// </summary>
    protected void EnsureReader()
    {
        if (Reader != null) return;
        if (Stream == null) Open();
        Reader = new StreamReader(Stream!, Encoding, leaveOpen: true);
    }

    /// <summary>
    /// Ensure the stream is open and a <see cref="StreamWriter"/> is initialized.
    /// </summary>
    protected void EnsureWriter()
    {
        if (Writer != null) return;
        if (Stream == null) Open();
        Writer = new StreamWriter(Stream!, Encoding, leaveOpen: true);
    }

    /// <summary>
    /// Read the next row from the CSV file.
    /// </summary>
    /// <returns>The next row as a list of strings, or an empty list at EOF.</returns>
    public virtual List<string> ReadRow()
    {
        EnsureReader();
        var line = Reader!.ReadLine();
        return line == null ? [] : CsvParser.ParseRow(line);
    }

    /// <summary>
    /// Read all remaining rows from the CSV file.
    /// </summary>
    /// <returns>A list of rows, each row being a list of strings.</returns>
    public virtual List<List<string>> ReadAll()
    {
        EnsureReader();
        var rows = new List<List<string>>();
        string? line;
        while ((line = Reader!.ReadLine()) != null)
            rows.Add(CsvParser.ParseRow(line));
        return rows;
    }

    /// <summary>
    /// Write a single row to the CSV file.
    /// </summary>
    /// <param name="row">The field values to write.</param>
    public virtual void WriteRow(IEnumerable<string> row)
    {
        EnsureWriter();
        Writer!.WriteLine(CsvParser.FormatRow(row));
    }

    /// <summary>
    /// Write multiple rows to the CSV file.
    /// </summary>
    /// <param name="rows">The rows to write.</param>
    public virtual void WriteAll(IEnumerable<IEnumerable<string>> rows)
    {
        EnsureWriter();
        foreach (var row in rows)
            Writer!.WriteLine(CsvParser.FormatRow(row));
    }

    /// <summary>
    /// Yield rows from the CSV file with optional 1-based line-range filtering.
    /// </summary>
    /// <param name="startLine">Optional 1-based start line (inclusive).</param>
    /// <param name="endLine">Optional 1-based end line (inclusive).</param>
    /// <returns>An enumerable of rows.</returns>
    public virtual IEnumerable<List<string>> YieldRows(int? startLine = null, int? endLine = null)
    {
        EnsureReader();
        var lineNum = 0;
        string? line;
        while ((line = Reader!.ReadLine()) != null)
        {
            lineNum++;
            if (startLine.HasValue && lineNum < startLine.Value) continue;
            if (endLine.HasValue && lineNum > endLine.Value) break;
            yield return CsvParser.ParseRow(line);
        }
    }

    /// <summary>Close reader, writer, and underlying stream.</summary>
    public override void Close()
    {
        Writer?.Flush();
        Writer?.Dispose();
        Writer = null;
        Reader?.Dispose();
        Reader = null;
        base.Close();
    }

    // --- Static helpers ---

    /// <summary>Load all rows from a CSV file.</summary>
    public static List<List<string>> LoadRows(string path, Encoding? encoding = null)
    {
        using var loader = ForReading(path, encoding);
        return loader.ReadAll();
    }

    /// <summary>Save rows to a CSV file (overwrites).</summary>
    public static void SaveRows(string path, IEnumerable<IEnumerable<string>> rows, Encoding? encoding = null)
    {
        using var loader = ForWriting(path, encoding);
        loader.WriteAll(rows);
    }

    /// <summary>Append a single row to a CSV file.</summary>
    public static void AppendRow(string path, IEnumerable<string> row, Encoding? encoding = null)
    {
        using var loader = ForAppending(path, encoding);
        loader.WriteRow(row);
    }
}
