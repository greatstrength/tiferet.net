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

    [Fact]
    public void Validate_IsPublicStatic()
    {
        // Verify the method is accessible from outside the assembly.
        var method = typeof(DomainObject).GetMethod("Validate",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void Validate_PassesOnValidRecord()
    {
        var record = new TestDomainObject("1", "Valid");
        // Should not throw.
        DomainObject.Validate(record);
    }

    [Fact]
    public void Validate_ThrowsTiferetDomainException_OnInvalidRecord()
    {
        // TestDomainObjectRequired has [Required] on Name.
        var record = new TestDomainObjectRequired("", null!);
        var ex = Assert.Throws<TiferetDomainException>(
            () => DomainObject.Validate(record));
        Assert.True(ex.Failures.Count > 0);
    }
}

// A domain record with Required annotation for validation testing.
public sealed record TestDomainObjectRequired(
    string Id,
    [property: System.ComponentModel.DataAnnotations.Required] string Name
) : DomainObject;
