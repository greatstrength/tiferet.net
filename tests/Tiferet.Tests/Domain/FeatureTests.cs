using Tiferet.Domain;
using Tiferet.Mappers;
using Tiferet.Mappers.Feature;
using Tiferet.Domain.Feature;

namespace Tiferet.Tests.Domain;

public class FeatureRecordTests
{
    [Fact]
    public void Create_DerivesIdFromGroupIdAndKey()
    {
        var f = FeatureAggregate.Create(name: "Add Number", groupId: "calc").Domain;
        Assert.Equal("calc.add_number", f.Id);
        Assert.Equal("calc", f.GroupId);
        Assert.Equal("add_number", f.FeatureKey);
    }

    [Fact]
    public void Create_DerivesGroupIdAndKeyFromId()
    {
        var f = FeatureAggregate.Create(name: "Add", id: "calc.add").Domain;
        Assert.Equal("calc", f.GroupId);
        Assert.Equal("add", f.FeatureKey);
    }

    [Fact]
    public void Create_DefaultsDescriptionToName()
    {
        var f = FeatureAggregate.Create(name: "Add Number", groupId: "calc").Domain;
        Assert.Equal("Add Number", f.Description);
    }

    [Fact]
    public void Create_ExplicitDescription()
    {
        var f = FeatureAggregate.Create(name: "Add", groupId: "calc", description: "Custom desc").Domain;
        Assert.Equal("Custom desc", f.Description);
    }

    [Fact]
    public void GetStep_ReturnsStep()
    {
        var step = new FeatureEventConfiguration("Step1", "svc1");
        var f = FeatureAggregate.Create(name: "F", groupId: "g", steps: [step]).Domain;
        Assert.Same(step, f.GetStep(0));
    }

    [Fact]
    public void GetStep_ReturnsNull_OutOfRange()
    {
        var f = FeatureAggregate.Create(name: "F", groupId: "g").Domain;
        Assert.Null(f.GetStep(0));
    }

    [Fact]
    public void ValueEquality()
    {
        var a = FeatureAggregate.Create(name: "F", groupId: "g").Domain;
        var b = FeatureAggregate.Create(name: "F", groupId: "g").Domain;
        Assert.Equal(a, b);
    }
}

public class FeatureAggregateTests
{
    private static FeatureAggregate CreateAggregate() =>
        FeatureAggregate.Create(name: "Add Number", groupId: "calc");

    [Fact]
    public void Rename_UpdatesName()
    {
        var agg = CreateAggregate();
        agg.Rename("Subtract");
        Assert.Equal("Subtract", agg.Domain.Name);
    }

    [Fact]
    public void SetDescription_Updates()
    {
        var agg = CreateAggregate();
        agg.SetDescription("New desc");
        Assert.Equal("New desc", agg.Domain.Description);
    }

    [Fact]
    public void AddStep_AppendsStep()
    {
        var agg = CreateAggregate();
        var step = agg.AddStep("Step1", "svc1");
        Assert.Single(agg.Domain.Steps!);
        Assert.Equal("Step1", step.Name);
    }

    [Fact]
    public void AddStep_InsertsAtPosition()
    {
        var agg = CreateAggregate();
        agg.AddStep("A", "svc1");
        agg.AddStep("B", "svc2", position: 0);
        Assert.Equal("B", agg.Domain.Steps![0].Name);
        Assert.Equal("A", agg.Domain.Steps![1].Name);
    }

    [Fact]
    public void RemoveStep_RemovesAndReturns()
    {
        var agg = CreateAggregate();
        agg.AddStep("A", "svc1");
        agg.AddStep("B", "svc2");
        var removed = agg.RemoveStep(0);
        Assert.Equal("A", removed!.Name);
        Assert.Single(agg.Domain.Steps!);
    }

    [Fact]
    public void RemoveStep_ReturnsNull_OutOfRange()
    {
        var agg = CreateAggregate();
        Assert.Null(agg.RemoveStep(0));
    }

    [Fact]
    public void ReorderStep_MovesStep()
    {
        var agg = CreateAggregate();
        agg.AddStep("A", "svc1");
        agg.AddStep("B", "svc2");
        agg.AddStep("C", "svc3");
        agg.ReorderStep(0, 2);
        Assert.Equal("B", agg.Domain.Steps![0].Name);
        Assert.Equal("C", agg.Domain.Steps![1].Name);
        Assert.Equal("A", agg.Domain.Steps![2].Name);
    }
}

public class FeatureEventAggregateTests
{
    [Fact]
    public void SetPassOnError_Updates()
    {
        var agg = new FeatureEventAggregate(new FeatureEventConfiguration("Step", "svc"));
        agg.SetPassOnError(true);
        Assert.True(agg.Domain.PassOnError);
    }

    [Fact]
    public void SetParameters_MergesParameters()
    {
        var agg = new FeatureEventAggregate(new FeatureEventConfiguration("Step", "svc",
            Parameters: new Dictionary<string, string> { ["a"] = "1" }));
        agg.SetParameters(new() { ["b"] = "2" });
        Assert.Equal("1", agg.Domain.Parameters!["a"]);
        Assert.Equal("2", agg.Domain.Parameters!["b"]);
    }
}
