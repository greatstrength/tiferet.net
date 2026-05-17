using Tiferet.Domain;
using Tiferet.Events;
using Tiferet.Mappers;
using Tiferet.Testing;
using Tiferet.Utilities.Json;

namespace Tiferet.Tests.Mappers;

// Reuse SampleDomain, SampleAggregate, SampleTransferObject, and SampleJsonTransferObject
// from AggregateTests.cs, TransferObjectTests.cs, and JsonTransferObjectTests.cs.

/// <summary>
/// Meta-test: proves <see cref="AggregateTestBase{TAggregate, TDomain}"/> works with the sample types.
/// The inherited [Fact] tests run automatically.
/// </summary>
public class SampleAggregateHarnessTests : AggregateTestBase<SampleAggregate, SampleDomain>
{
    protected override SampleAggregate CreateAggregate()
        => new(new SampleDomain("1", "Alpha", 42));

    protected override SampleDomain ExpectedState
        => new("1", "Alpha", 42);

    protected override string[] EqualityFields => ["Id", "Name", "Value"];

    public override IEnumerable<object[]> SetAttributeErrorCases =>
    [
        ["NonExistent", "val", ErrorCodes.InvalidModelAttribute],
    ];
}

/// <summary>
/// Meta-test: proves <see cref="TransferObjectTestBase{TTransfer, TAggregate}"/> works with the sample types.
/// The inherited [Fact] tests run automatically.
/// </summary>
public class SampleTransferObjectHarnessTests : TransferObjectTestBase<SampleTransferObject, SampleAggregate>
{
    protected override SampleTransferObject CreateTransferObject()
        => new() { Id = "1", Name = "Alpha", Value = 42 };

    protected override SampleAggregate ExpectedAggregate
        => new(new SampleDomain("1", "Alpha", 42));

    protected override string[] EqualityFields => ["Id", "Name", "Value"];
}

/// <summary>
/// Meta-test: proves <see cref="JsonTransferObjectTestBase{TTransfer, TAggregate}"/> works.
/// The inherited [Fact] tests run automatically, including the JSON deserialization round-trip.
/// </summary>
public class SampleJsonTransferObjectHarnessTests
    : JsonTransferObjectTestBase<SampleJsonTransferObject, SampleAggregate>
{
    protected override SampleJsonTransferObject CreateTransferObject()
        => new() { Id = "1", UserName = "Alpha", ItemCount = 42 };

    protected override SampleAggregate ExpectedAggregate
        => new(new SampleDomain("1", "Alpha", 42));

    protected override string[] EqualityFields => ["Id", "Name", "Value"];

    protected override string SampleJson
        => """{"id":"1","user_name":"Alpha","item_count":42}""";
}
