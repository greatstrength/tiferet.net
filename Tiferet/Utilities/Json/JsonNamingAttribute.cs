namespace Tiferet.Utilities.Json;

/// <summary>
/// Declares the JSON naming convention for a transfer object or domain record.
/// Applied at the class level and inherited by subclasses.
/// </summary>
/// <example>
/// <code>
/// [JsonNaming(NamingConvention.SnakeCase)]
/// public class UserJsonObject : JsonTransferObject&lt;UserAggregate&gt; { ... }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class JsonNamingAttribute : Attribute
{
    /// <summary>The naming convention for JSON serialization.</summary>
    public NamingConvention Convention { get; }

    /// <summary>
    /// Initializes a new <see cref="JsonNamingAttribute"/>.
    /// </summary>
    /// <param name="convention">The naming convention to use.</param>
    public JsonNamingAttribute(NamingConvention convention)
    {
        Convention = convention;
    }
}
