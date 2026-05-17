using System.Reflection;
using Tiferet.Mappers;
using Tiferet.Utilities.Json;
using Xunit;

namespace Tiferet.Testing;

/// <summary>
/// Generic xUnit test base class for testing <see cref="JsonTransferObject{TAggregate}"/> subclasses.
/// Extends <see cref="TransferObjectTestBase{TTransfer, TAggregate}"/> with a JSON
/// deserialization round-trip test.
/// </summary>
/// <typeparam name="TTransfer">The concrete JSON transfer object type under test.</typeparam>
/// <typeparam name="TAggregate">The target aggregate type.</typeparam>
public abstract class JsonTransferObjectTestBase<TTransfer, TAggregate>
    : TransferObjectTestBase<TTransfer, TAggregate>
    where TTransfer : JsonTransferObject<TAggregate>
    where TAggregate : Aggregate
{
    /// <summary>
    /// A sample JSON string that should deserialize into the transfer object
    /// and ultimately map to <see cref="TransferObjectTestBase{TTransfer,TAggregate}.ExpectedAggregate"/>.
    /// </summary>
    protected abstract string SampleJson { get; }

    /// <summary>
    /// Test that deserializing <see cref="SampleJson"/> and mapping
    /// produces an aggregate matching the expected state.
    /// </summary>
    [Fact]
    public void DeserializeJson_ProducesExpectedAggregate()
    {
        var transfer = JsonSerializerHelper.Deserialize<TTransfer>(SampleJson);
        var aggregate = transfer.Map();
        var aggregateType = typeof(TAggregate);

        foreach (var field in EqualityFields)
        {
            var prop = aggregateType.GetProperty(field, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(prop);

            var actual = prop!.GetValue(aggregate);
            var expected = prop.GetValue(ExpectedAggregate);

            Assert.Equal(expected, actual);
        }
    }
}
