using System.Text;

using Tiferet.Core;
using Tiferet.Interfaces;

namespace Tiferet.Utilities;

/// <summary>
/// Base utility for file stream operations with validation and lifecycle management.
/// Implements <see cref="IFileService"/>.
/// </summary>
public class FileLoader : IFileService
{
    private Stream? _stream;

    /// <summary>The file system path.</summary>
    public string FilePath { get; }

    /// <summary>The .NET file mode used when opening the stream.</summary>
    public FileMode FileMode { get; }

    /// <summary>The .NET file access level.</summary>
    public FileAccess Access { get; }

    /// <summary>The text encoding (defaults to UTF-8).</summary>
    public Encoding Encoding { get; }

    /// <summary>The underlying file stream, or <c>null</c> if not open.</summary>
    public Stream? Stream => _stream;

    /// <summary>
    /// Initializes a new <see cref="FileLoader"/>.
    /// </summary>
    /// <param name="path">The file system path.</param>
    /// <param name="fileMode">The file mode.</param>
    /// <param name="access">The file access level.</param>
    /// <param name="encoding">Text encoding (defaults to UTF-8).</param>
    public FileLoader(
        string path,
        FileMode fileMode = FileMode.Open,
        FileAccess access = FileAccess.Read,
        Encoding? encoding = null)
    {
        FilePath = path ?? throw new ArgumentNullException(nameof(path));
        FileMode = fileMode;
        Access = access;
        Encoding = encoding ?? Encoding.UTF8;
    }

    /// <summary>Create a <see cref="FileLoader"/> configured for reading.</summary>
    public static FileLoader ForReading(string path, Encoding? encoding = null)
        => new(path, FileMode.Open, FileAccess.Read, encoding);

    /// <summary>Create a <see cref="FileLoader"/> configured for writing (creates or overwrites).</summary>
    public static FileLoader ForWriting(string path, Encoding? encoding = null)
        => new(path, FileMode.Create, FileAccess.Write, encoding);

    /// <summary>
    /// Verify that the file or parent directory exists as appropriate for the file mode.
    /// For read modes (<see cref="System.IO.FileMode.Open"/>, <see cref="System.IO.FileMode.Truncate"/>)
    /// the file itself must exist. For write/create modes the parent directory must exist.
    /// </summary>
    /// <param name="path">The file path to verify.</param>
    /// <param name="fileMode">The intended file mode.</param>
    public static void VerifyFile(string path, FileMode fileMode)
    {
        switch (fileMode)
        {
            case FileMode.Open:
            case FileMode.Truncate:
                if (!File.Exists(path))
                    throw new TiferetException(ErrorCodes.FileNotFound, null, ("path", path));
                break;

            default:
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    throw new TiferetException(ErrorCodes.FileNotFound, null, ("path", path));
                break;
        }
    }

    /// <summary>
    /// Open the file stream. Validates the path and guards against double-open.
    /// </summary>
    /// <returns>The opened stream.</returns>
    public virtual Stream Open()
    {
        if (_stream != null)
            throw new TiferetException(ErrorCodes.FileAlreadyOpen, null, ("path", FilePath));

        VerifyFile(FilePath, FileMode);
        _stream = new FileStream(FilePath, FileMode, Access);
        return _stream;
    }

    /// <summary>
    /// Close the file stream if open.
    /// </summary>
    public virtual void Close()
    {
        _stream?.Dispose();
        _stream = null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }
}
