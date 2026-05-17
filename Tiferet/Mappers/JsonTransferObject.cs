using Tiferet.Utilities.Json;

namespace Tiferet.Mappers;

/// <summary>
/// Generic transfer object base class for JSON API deserialization.
/// Extends <see cref="TransferObject{TAggregate}"/> with convention-aware
/// JSON deserialization and a convenience <c>DeserializeAndMap</c> method.
/// Concrete subclasses override <see cref="TransferObject{TAggregate}.Map"/>
/// exactly as with YAML transfer objects.
/// </summary>
/// <typeparam name="TAggregate">The target aggregate type.</typeparam>
public abstract class JsonTransferObject<TAggregate> : TransferObject<TAggregate>
    where TAggregate : Aggregate
{
    /// <summary>
    /// Deserialize a JSON string to a concrete <typeparamref name="TTransfer"/> instance
    /// and immediately map it to <typeparamref name="TAggregate"/>.
    /// </summary>
    /// <typeparam name="TTransfer">
    /// The concrete <see cref="JsonTransferObject{TAggregate}"/> subclass to deserialize into.
    /// </typeparam>
    /// <param name="json">The JSON string.</param>
    /// <param name="overrides">Optional mapping overrides passed to <see cref="TransferObject{TAggregate}.Map"/>.</param>
    /// <returns>The mapped aggregate.</returns>
    public static TAggregate DeserializeAndMap<TTransfer>(string json, Dictionary<string, object?>? overrides = null)
        where TTransfer : JsonTransferObject<TAggregate>
    {
        var transfer = JsonSerializerHelper.Deserialize<TTransfer>(json);
        return transfer.Map(overrides);
    }
}
