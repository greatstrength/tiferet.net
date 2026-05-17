using System.Reflection;
using Tiferet.Mappers;
using Xunit;

namespace Tiferet.Testing;

/// <summary>
/// Generic xUnit test base class for testing <see cref="TransferObject{TAggregate}"/> subclasses.
/// Provides auto-generated tests for Map() and round-trip verification.
/// Subclasses provide sample data via abstract members.
/// </summary>
/// <typeparam name="TTransfer">The concrete transfer object type under test.</typeparam>
/// <typeparam name="TAggregate">The target aggregate type.</typeparam>
public abstract class TransferObjectTestBase<TTransfer, TAggregate>
    where TTransfer : TransferObject<TAggregate>
    where TAggregate : Aggregate
{
    /// <summary>
    /// Create the transfer object instance to test.
    /// Typically constructed from sample YAML/JSON-format data.
    /// </summary>
    protected abstract TTransfer CreateTransferObject();

    /// <summary>
    /// The expected aggregate after mapping.
    /// Used for comparison in Map() and round-trip tests.
    /// </summary>
    protected abstract TAggregate ExpectedAggregate { get; }

    /// <summary>
    /// The property names to compare between the mapped aggregate and expected aggregate.
    /// </summary>
    protected abstract string[] EqualityFields { get; }

    /// <summary>
    /// Test that <c>Map()</c> produces an aggregate matching the expected state.
    /// </summary>
    [Fact]
    public void Map_ProducesExpectedAggregate()
    {
        var transfer = CreateTransferObject();
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
