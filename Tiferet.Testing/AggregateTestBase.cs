using System.Reflection;
using Tiferet.Domain;
using Tiferet.Events;
using Tiferet.Mappers;
using Xunit;

namespace Tiferet.Testing;

/// <summary>
/// Generic xUnit test base class for testing <see cref="Aggregate{TDomain}"/> subclasses.
/// Provides auto-generated tests for Create factory, delegated properties,
/// ToDomainObject round-trip, and SetAttribute error paths.
/// Subclasses provide sample data via abstract members.
/// </summary>
/// <typeparam name="TAggregate">The concrete aggregate type under test.</typeparam>
/// <typeparam name="TDomain">The domain record type wrapped by the aggregate.</typeparam>
public abstract class AggregateTestBase<TAggregate, TDomain>
    where TAggregate : Aggregate<TDomain>
    where TDomain : DomainObject
{
    /// <summary>
    /// Create the aggregate instance to test. Typically calls the static <c>Create</c> factory.
    /// </summary>
    protected abstract TAggregate CreateAggregate();

    /// <summary>
    /// The expected domain record state after construction.
    /// Used for property comparison and round-trip verification.
    /// </summary>
    protected abstract TDomain ExpectedState { get; }

    /// <summary>
    /// The property names to compare between the aggregate and expected state.
    /// </summary>
    protected abstract string[] EqualityFields { get; }

    /// <summary>
    /// Provides (attribute, value, expectedErrorCode) tuples for SetAttribute error tests.
    /// Return an empty collection if no error cases apply.
    /// </summary>
    public virtual IEnumerable<object[]> SetAttributeErrorCases => [];

    /// <summary>
    /// Test that the aggregate factory produces an instance whose delegated properties
    /// match the expected state.
    /// </summary>
    [Fact]
    public void CreateAggregate_ProducesValidState()
    {
        var aggregate = CreateAggregate();
        var aggregateType = typeof(TAggregate);
        var domainType = typeof(TDomain);

        foreach (var field in EqualityFields)
        {
            var aggregateProp = aggregateType.GetProperty(field, BindingFlags.Public | BindingFlags.Instance);
            var domainProp = domainType.GetProperty(field, BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(aggregateProp);
            Assert.NotNull(domainProp);

            var actual = aggregateProp!.GetValue(aggregate);
            var expected = domainProp!.GetValue(ExpectedState);

            Assert.Equal(expected, actual);
        }
    }

    /// <summary>
    /// Test that <c>ToDomainObject()</c> produces a record equal to the expected state.
    /// </summary>
    [Fact]
    public void ToDomainObject_RoundTrips()
    {
        var aggregate = CreateAggregate();
        var domain = aggregate.ToDomainObject();

        Assert.Equal(ExpectedState, domain);
    }

    /// <summary>
    /// Test that SetAttribute with invalid attribute names raises the expected error.
    /// Override <see cref="SetAttributeErrorCases"/> to provide test data.
    /// </summary>
    [Fact]
    public void SetAttribute_InvalidAttribute_RaisesError()
    {
        foreach (var testCase in SetAttributeErrorCases)
        {
            var attribute = (string)testCase[0];
            var value = testCase[1];
            var expectedErrorCode = (string)testCase[2];

            var aggregate = CreateAggregate();

            // Use reflection to invoke the protected SetAttribute method.
            var method = typeof(Aggregate<TDomain>).GetMethod(
                "SetAttribute",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.NotNull(method);

            try
            {
                method!.Invoke(aggregate, [attribute, value]);
                Assert.Fail($"Expected TiferetException with error code '{expectedErrorCode}' but no exception was thrown.");
            }
            catch (TargetInvocationException tie) when (tie.InnerException is TiferetException tex && tex.ErrorCode == expectedErrorCode)
            {
                // Expected — pass.
            }
        }
    }
}
