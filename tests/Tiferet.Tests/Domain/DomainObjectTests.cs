using Tiferet.Domain;

namespace Tiferet.Tests.Domain;

// A concrete test record for verifying DomainObject behavior.
public sealed record TestDomainObject(string Id, string Name) : DomainObject;

public class DomainObjectTests
{
    [Fact]
    public void Records_HaveValueEquality()
    {
        var a = new TestDomainObject("1", "Alpha");
        var b = new TestDomainObject("1", "Alpha");
        Assert.Equal(a, b);
    }

    [Fact]
    public void Records_AreNotEqualWhenDifferent()
    {
        var a = new TestDomainObject("1", "Alpha");
        var b = new TestDomainObject("2", "Beta");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        var original = new TestDomainObject("1", "Alpha");
        var modified = original with { Name = "Beta" };
        Assert.Equal("1", modified.Id);
        Assert.Equal("Beta", modified.Name);
        Assert.NotEqual(original, modified);
    }

    [Fact]
    public void IsAbstractRecord()
    {
        Assert.True(typeof(DomainObject).IsAbstract);
    }
}
