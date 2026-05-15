using System.Text;

using Tiferet.Domain;
using Tiferet.Events;
using Tiferet.Interfaces;
using Tiferet.Utilities;

namespace Tiferet.Tests.Utilities;

public class FileLoaderTests : IDisposable
{
    private readonly string _tempDir;

    public FileLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tiferet_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string TempFile(string name = "test.txt", string? content = null)
    {
        var path = Path.Combine(_tempDir, name);
        if (content != null)
            File.WriteAllText(path, content, Encoding.UTF8);
        return path;
    }

    [Fact]
    public void Constructor_SetsProperties()
    {
        var path = TempFile(content: "hello");
        var loader = new FileLoader(path);
        Assert.Equal(path, loader.FilePath);
        Assert.Equal(FileMode.Open, loader.FileMode);
        Assert.Equal(FileAccess.Read, loader.Access);
        Assert.Equal(Encoding.UTF8, loader.Encoding);
        Assert.Null(loader.Stream);
    }

    [Fact]
    public void ForReading_FactoryConfiguresCorrectly()
    {
        var path = TempFile(content: "data");
        using var loader = FileLoader.ForReading(path);
        Assert.Equal(FileMode.Open, loader.FileMode);
        Assert.Equal(FileAccess.Read, loader.Access);
    }

    [Fact]
    public void ForWriting_FactoryConfiguresCorrectly()
    {
        var path = TempFile();
        using var loader = FileLoader.ForWriting(path);
        Assert.Equal(FileMode.Create, loader.FileMode);
        Assert.Equal(FileAccess.Write, loader.Access);
    }

    [Fact]
    public void Open_ReturnsStream()
    {
        var path = TempFile(content: "hello");
        using var loader = FileLoader.ForReading(path);
        var stream = loader.Open();
        Assert.NotNull(stream);
        Assert.Same(stream, loader.Stream);
    }

    [Fact]
    public void Close_DisposesStream()
    {
        var path = TempFile(content: "hello");
        using var loader = FileLoader.ForReading(path);
        loader.Open();
        Assert.NotNull(loader.Stream);
        loader.Close();
        Assert.Null(loader.Stream);
    }

    [Fact]
    public void Open_ThrowsWhenAlreadyOpen()
    {
        var path = TempFile(content: "hello");
        using var loader = FileLoader.ForReading(path);
        loader.Open();
        var ex = Assert.Throws<TiferetException>(() => loader.Open());
        Assert.Equal(ErrorCodes.FileAlreadyOpen, ex.ErrorCode);
    }

    [Fact]
    public void Open_ThrowsWhenFileNotFound_ReadMode()
    {
        var path = Path.Combine(_tempDir, "nonexistent.txt");
        using var loader = FileLoader.ForReading(path);
        var ex = Assert.Throws<TiferetException>(() => loader.Open());
        Assert.Equal(ErrorCodes.FileNotFound, ex.ErrorCode);
    }

    [Fact]
    public void VerifyFile_ThrowsWhenParentDirMissing_WriteMode()
    {
        var path = Path.Combine(_tempDir, "nonexistent_dir", "file.txt");
        var ex = Assert.Throws<TiferetException>(
            () => FileLoader.VerifyFile(path, FileMode.Create));
        Assert.Equal(ErrorCodes.FileNotFound, ex.ErrorCode);
    }

    [Fact]
    public void VerifyFile_PassesWhenParentDirExists_WriteMode()
    {
        var path = Path.Combine(_tempDir, "newfile.txt");
        // Should not throw — parent dir exists, file doesn't need to.
        FileLoader.VerifyFile(path, FileMode.Create);
    }

    [Fact]
    public void Dispose_ClosesStream()
    {
        var path = TempFile(content: "hello");
        var loader = FileLoader.ForReading(path);
        loader.Open();
        loader.Dispose();
        Assert.Null(loader.Stream);
    }

    [Fact]
    public void ImplementsIFileService()
    {
        var path = TempFile(content: "x");
        using var loader = FileLoader.ForReading(path);
        Assert.IsAssignableFrom<IFileService>(loader);
        Assert.IsAssignableFrom<IDisposable>(loader);
    }

    [Fact]
    public void Open_Write_CreatesFile()
    {
        var path = TempFile();
        using var loader = FileLoader.ForWriting(path);
        var stream = loader.Open();
        var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write("written");
        writer.Flush();
        writer.Dispose();
        loader.Close();

        Assert.Equal("written", File.ReadAllText(path));
    }
}
