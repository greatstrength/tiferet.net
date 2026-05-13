using System.Text;

using Tiferet.Core;
using Tiferet.Utilities;

namespace Tiferet.Tests.Utilities;

public class CsvLoaderTests : IDisposable
{
    private readonly string _tempDir;

    public CsvLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tiferet_csv_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteCsv(string content, string name = "test.csv")
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content, Encoding.UTF8);
        return path;
    }

    // --- CsvLoader (list-based) ---

    [Fact]
    public void ReadAll_ReturnsAllRows()
    {
        var path = WriteCsv("a,b,c\n1,2,3\n4,5,6\n");
        using var loader = CsvLoader.ForReading(path);
        var rows = loader.ReadAll();
        Assert.Equal(3, rows.Count);
        Assert.Equal(["a", "b", "c"], rows[0]);
        Assert.Equal(["1", "2", "3"], rows[1]);
    }

    [Fact]
    public void ReadRow_ReadsOneAtATime()
    {
        var path = WriteCsv("a,b\n1,2\n");
        using var loader = CsvLoader.ForReading(path);
        var row1 = loader.ReadRow();
        Assert.Equal(["a", "b"], row1);
        var row2 = loader.ReadRow();
        Assert.Equal(["1", "2"], row2);
        var row3 = loader.ReadRow();
        Assert.Empty(row3);
    }

    [Fact]
    public void ReadAll_HandlesQuotedFields()
    {
        var path = WriteCsv("name,desc\n\"Smith, John\",\"a \"\"quoted\"\" value\"\n");
        using var loader = CsvLoader.ForReading(path);
        var rows = loader.ReadAll();
        Assert.Equal("Smith, John", rows[1][0]);
        Assert.Equal("a \"quoted\" value", rows[1][1]);
    }

    [Fact]
    public void WriteRow_WritesSingleRow()
    {
        var path = Path.Combine(_tempDir, "write.csv");
        using var loader = CsvLoader.ForWriting(path);
        loader.WriteRow(["hello", "world"]);
        loader.Close();

        var content = File.ReadAllText(path);
        Assert.Contains("hello,world", content);
    }

    [Fact]
    public void WriteAll_WritesMultipleRows()
    {
        var path = Path.Combine(_tempDir, "multi.csv");
        using var loader = CsvLoader.ForWriting(path);
        loader.WriteAll([["a", "b"], ["1", "2"]]);
        loader.Close();

        var lines = File.ReadAllLines(path);
        Assert.Equal("a,b", lines[0]);
        Assert.Equal("1,2", lines[1]);
    }

    [Fact]
    public void WriteRow_QuotesFieldsWithCommas()
    {
        var path = Path.Combine(_tempDir, "quoted.csv");
        using var loader = CsvLoader.ForWriting(path);
        loader.WriteRow(["hello,world", "normal"]);
        loader.Close();

        var content = File.ReadAllText(path);
        Assert.Contains("\"hello,world\"", content);
    }

    [Fact]
    public void YieldRows_WithRange_FiltersCorrectly()
    {
        var path = WriteCsv("a\nb\nc\nd\ne\n");
        using var loader = CsvLoader.ForReading(path);
        var rows = loader.YieldRows(startLine: 2, endLine: 4).ToList();
        Assert.Equal(3, rows.Count);
        Assert.Equal(["b"], rows[0]);
        Assert.Equal(["d"], rows[2]);
    }

    // --- Static helpers ---

    [Fact]
    public void LoadRows_Static_ReadsAll()
    {
        var path = WriteCsv("x,y\n1,2\n");
        var rows = CsvLoader.LoadRows(path);
        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public void SaveRows_Static_WritesAll()
    {
        var path = Path.Combine(_tempDir, "static_save.csv");
        CsvLoader.SaveRows(path, [["a", "b"], ["1", "2"]]);
        var content = File.ReadAllText(path);
        Assert.Contains("a,b", content);
        Assert.Contains("1,2", content);
    }

    [Fact]
    public void AppendRow_Static_AppendsToExisting()
    {
        var path = WriteCsv("a,b\n");
        CsvLoader.AppendRow(path, ["1", "2"]);
        var lines = File.ReadAllLines(path);
        Assert.Equal("a,b", lines[0]);
        Assert.Equal("1,2", lines[1]);
    }
}

public class CsvDictLoaderTests : IDisposable
{
    private readonly string _tempDir;

    public CsvDictLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tiferet_csvdict_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteCsv(string content, string name = "dict.csv")
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content, Encoding.UTF8);
        return path;
    }

    [Fact]
    public void ReadAllDicts_InfersHeaderFromFirstRow()
    {
        var path = WriteCsv("name,age\nAlice,30\nBob,25\n");
        using var loader = CsvDictLoader.ForReading(path);
        var rows = loader.ReadAllDicts();
        Assert.Equal(2, rows.Count);
        Assert.Equal("Alice", rows[0]["name"]);
        Assert.Equal("30", rows[0]["age"]);
        Assert.Equal("Bob", rows[1]["name"]);
    }

    [Fact]
    public void ReadDictRow_ReadsOneAtATime()
    {
        var path = WriteCsv("x,y\n1,2\n3,4\n");
        using var loader = CsvDictLoader.ForReading(path);
        var row1 = loader.ReadDictRow();
        Assert.Equal("1", row1["x"]);
        var row2 = loader.ReadDictRow();
        Assert.Equal("3", row2["x"]);
        var row3 = loader.ReadDictRow();
        Assert.Empty(row3);
    }

    [Fact]
    public void ReadAllDicts_WithExplicitFieldNames()
    {
        var path = WriteCsv("1,2\n3,4\n");
        using var loader = CsvDictLoader.ForReading(path, fieldNames: ["a", "b"]);
        var rows = loader.ReadAllDicts();
        Assert.Equal(2, rows.Count);
        Assert.Equal("1", rows[0]["a"]);
        Assert.Equal("4", rows[1]["b"]);
    }

    [Fact]
    public void WriteHeader_And_WriteDictRow()
    {
        var path = Path.Combine(_tempDir, "write_dict.csv");
        var fields = new List<string> { "name", "age" };
        using var loader = CsvDictLoader.ForWriting(path, fields);
        loader.WriteHeader();
        loader.WriteDictRow(new() { ["name"] = "Alice", ["age"] = "30" });
        loader.Close();

        var lines = File.ReadAllLines(path);
        Assert.Equal("name,age", lines[0]);
        Assert.Equal("Alice,30", lines[1]);
    }

    [Fact]
    public void WriteDictRow_ThrowsWithoutFieldNames()
    {
        var path = Path.Combine(_tempDir, "no_fields.csv");
        using var loader = new CsvDictLoader(path, FileMode.Create, FileAccess.Write);
        var ex = Assert.Throws<TiferetException>(
            () => loader.WriteDictRow(new() { ["a"] = "1" }));
        Assert.Equal(ErrorCodes.CsvFieldnamesRequired, ex.ErrorCode);
    }

    [Fact]
    public void YieldDictRows_WithRange()
    {
        var path = WriteCsv("k,v\na,1\nb,2\nc,3\n");
        using var loader = CsvDictLoader.ForReading(path);
        var rows = loader.YieldDictRows(startLine: 2, endLine: 3).ToList();
        Assert.Equal(2, rows.Count);
        Assert.Equal("b", rows[0]["k"]);
        Assert.Equal("c", rows[1]["k"]);
    }

    // --- Static helpers ---

    [Fact]
    public void LoadDictRows_Static()
    {
        var path = WriteCsv("x,y\n1,2\n");
        var rows = CsvDictLoader.LoadDictRows(path);
        Assert.Single(rows);
        Assert.Equal("1", rows[0]["x"]);
    }

    [Fact]
    public void SaveDictRows_Static()
    {
        var path = Path.Combine(_tempDir, "static_dict.csv");
        var fields = new List<string> { "a", "b" };
        CsvDictLoader.SaveDictRows(path, fields,
            [new() { ["a"] = "1", ["b"] = "2" }]);
        var lines = File.ReadAllLines(path);
        Assert.Equal("a,b", lines[0]);
        Assert.Equal("1,2", lines[1]);
    }
}
