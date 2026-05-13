using System.Text;
using System.Text.Json;

using Tiferet.Core;

namespace Tiferet.Utilities;

/// <summary>
/// Utility for loading and saving JSON files with structured error handling.
/// Extends <see cref="FileLoader"/> for stream lifecycle management.
/// </summary>
public class JsonLoader : FileLoader
{
    /// <summary>
    /// Initializes a new <see cref="JsonLoader"/>.
    /// </summary>
    /// <param name="path">Path to the JSON file.</param>
    /// <param name="fileMode">The file mode.</param>
    /// <param name="access">The file access level.</param>
    /// <param name="encoding">Text encoding (defaults to UTF-8).</param>
    public JsonLoader(
        string path,
        FileMode fileMode = FileMode.Open,
        FileAccess access = FileAccess.Read,
        Encoding? encoding = null)
        : base(path, fileMode, access, encoding)
    {
    }

    /// <summary>Create a <see cref="JsonLoader"/> configured for reading.</summary>
    public static new JsonLoader ForReading(string path, Encoding? encoding = null)
        => new(path, FileMode.Open, FileAccess.Read, encoding);

    /// <summary>Create a <see cref="JsonLoader"/> configured for writing (creates or overwrites).</summary>
    public static new JsonLoader ForWriting(string path, Encoding? encoding = null)
        => new(path, FileMode.Create, FileAccess.Write, encoding);

    /// <summary>
    /// Verify that the file has a <c>.json</c> extension.
    /// </summary>
    /// <param name="path">The file path to verify.</param>
    public static void VerifyJsonExtension(string path)
    {
        var ext = Path.GetExtension(path)?.ToLowerInvariant();
        if (ext != ".json")
            throw new TiferetException(
                ErrorCodes.InvalidJsonFile, null,
                ("path", path),
                ("message", "File must have .json extension"));
    }

    /// <summary>
    /// Load JSON content, apply optional transformations, and return the result.
    /// Opens and closes the stream within this call.
    /// </summary>
    /// <param name="startNode">Optional selector applied to the parsed <see cref="JsonElement"/>.</param>
    /// <param name="dataFactory">Optional factory applied to the selected element.</param>
    /// <returns>The parsed and transformed <see cref="JsonElement"/>.</returns>
    public JsonElement Load(
        Func<JsonElement, JsonElement>? startNode = null,
        Func<JsonElement, object?>? dataFactory = null)
    {
        try
        {
            Open();
            var doc = JsonDocument.Parse(Stream!);
            var root = doc.RootElement;

            // Apply the start_node selector.
            var transformed = startNode != null ? startNode(root) : root;

            // If a data factory is provided, call it for its side effects but return the element.
            dataFactory?.Invoke(transformed);

            return transformed;
        }
        catch (TiferetException) { throw; }
        catch (JsonException ex)
        {
            throw new TiferetException(
                ErrorCodes.JsonFileLoadError, null,
                ("error", ex.Message), ("path", FilePath));
        }
        catch (Exception ex)
        {
            throw new TiferetException(
                ErrorCodes.JsonFileLoadError, null,
                ("error", ex.Message), ("path", FilePath));
        }
        finally
        {
            Close();
        }
    }

    /// <summary>
    /// Serialize data to JSON and write to the file.
    /// Opens and closes the stream within this call.
    /// </summary>
    /// <param name="data">The object to serialize.</param>
    /// <param name="options">Optional serializer options.</param>
    public void Save(object data, JsonSerializerOptions? options = null)
    {
        try
        {
            var opts = options ?? new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            Open();
            JsonSerializer.Serialize(Stream!, data, data.GetType(), opts);
        }
        catch (TiferetException) { throw; }
        catch (Exception ex)
        {
            throw new TiferetException(
                ErrorCodes.JsonFileSaveError, null,
                ("error", ex.Message), ("path", FilePath));
        }
        finally
        {
            Close();
        }
    }

    /// <summary>
    /// Navigate a <see cref="JsonElement"/> tree using dot-separated path notation
    /// with array index support (e.g., <c>"users.0.name"</c>).
    /// </summary>
    /// <param name="root">The root element to navigate.</param>
    /// <param name="path">Dot-separated path string.</param>
    /// <returns>The element at the specified path, or <c>null</c> if a segment resolves to null.</returns>
    public static JsonElement? ParseJsonPath(JsonElement root, string path)
    {
        var current = root;
        foreach (var part in path.Split('.'))
        {
            if (current.ValueKind == JsonValueKind.Object)
            {
                if (!current.TryGetProperty(part, out var prop))
                    return null;
                current = prop;
            }
            else if (current.ValueKind == JsonValueKind.Array && int.TryParse(part, out var index))
            {
                if (index < 0 || index >= current.GetArrayLength())
                    throw new TiferetException(
                        ErrorCodes.InvalidJsonPath, null,
                        ("path", path), ("part", part));
                current = current[index];
            }
            else
            {
                throw new TiferetException(
                    ErrorCodes.InvalidJsonPath, null,
                    ("path", path), ("part", part));
            }

            // Short-circuit on null.
            if (current.ValueKind == JsonValueKind.Null)
                return null;
        }

        return current;
    }
}
