using Tiferet.Events;
using System.Text;

using Tiferet.Domain;

using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Tiferet.Utilities;

/// <summary>
/// Utility for loading and saving YAML files with structured error handling.
/// Extends <see cref="FileLoader"/> for stream lifecycle management.
/// </summary>
public class YamlLoader : FileLoader
{
    /// <summary>
    /// Initializes a new <see cref="YamlLoader"/>.
    /// </summary>
    /// <param name="path">Path to the YAML file.</param>
    /// <param name="fileMode">The file mode.</param>
    /// <param name="access">The file access level.</param>
    /// <param name="encoding">Text encoding (defaults to UTF-8).</param>
    public YamlLoader(
        string path,
        FileMode fileMode = FileMode.Open,
        FileAccess access = FileAccess.Read,
        Encoding? encoding = null)
        : base(path, fileMode, access, encoding)
    {
    }

    /// <summary>Create a <see cref="YamlLoader"/> configured for reading.</summary>
    public static new YamlLoader ForReading(string path, Encoding? encoding = null)
        => new(path, FileMode.Open, FileAccess.Read, encoding);

    /// <summary>Create a <see cref="YamlLoader"/> configured for writing (creates or overwrites).</summary>
    public static new YamlLoader ForWriting(string path, Encoding? encoding = null)
        => new(path, FileMode.Create, FileAccess.Write, encoding);

    /// <summary>
    /// Verify that the file has a <c>.yaml</c> or <c>.yml</c> extension.
    /// </summary>
    /// <param name="path">The file path to verify.</param>
    public static void VerifyYamlExtension(string path)
    {
        var ext = Path.GetExtension(path)?.ToLowerInvariant();
        if (ext != ".yaml" && ext != ".yml")
            throw new TiferetException(
                ErrorCodes.InvalidYamlFile, null,
                ("path", path),
                ("message", "File must have .yaml or .yml extension"));
    }

    /// <summary>
    /// Load YAML content, apply optional transformations, and return the result.
    /// Opens and closes the stream within this call.
    /// </summary>
    /// <param name="startNode">Optional selector applied to the raw deserialized data.</param>
    /// <param name="dataFactory">Optional factory applied to the selected data.</param>
    /// <returns>The parsed and transformed object.</returns>
    public object? Load(
        Func<object?, object?>? startNode = null,
        Func<object?, object?>? dataFactory = null)
    {
        try
        {
            Open();
            using var reader = new StreamReader(Stream!, Encoding, leaveOpen: true);
            var deserializer = new DeserializerBuilder().Build();
            object? data = deserializer.Deserialize(reader);

            // Treat empty YAML as an empty dictionary.
            data ??= new Dictionary<object, object>();

            // Apply the start_node selector.
            var transformed = startNode != null ? startNode(data) : data;

            // Apply the data_factory and return.
            return dataFactory != null ? dataFactory(transformed) : transformed;
        }
        catch (TiferetException) { throw; }
        catch (YamlException ex)
        {
            throw new TiferetException(
                ErrorCodes.YamlFileLoadError, null,
                ("error", ex.Message), ("path", FilePath));
        }
        catch (Exception ex)
        {
            throw new TiferetException(
                ErrorCodes.YamlFileLoadError, null,
                ("error", ex.Message), ("path", FilePath));
        }
        finally
        {
            Close();
        }
    }

    /// <summary>
    /// Serialize data to YAML and write to the file.
    /// Opens and closes the stream within this call.
    /// </summary>
    /// <param name="data">The object to serialize.</param>
    public void Save(object data)
    {
        try
        {
            var serializer = new SerializerBuilder()
                .DisableAliases()
                .Build();
            var content = serializer.Serialize(data);

            Open();
            using var writer = new StreamWriter(Stream!, Encoding, leaveOpen: true);
            writer.Write(content);
        }
        catch (TiferetException) { throw; }
        catch (Exception ex)
        {
            throw new TiferetException(
                ErrorCodes.YamlFileSaveError, null,
                ("error", ex.Message), ("path", FilePath));
        }
        finally
        {
            Close();
        }
    }
}
