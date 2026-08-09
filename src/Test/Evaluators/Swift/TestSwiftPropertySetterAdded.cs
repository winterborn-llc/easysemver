using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S36 - directional against S35.</summary>
public class TestSwiftPropertySetterAdded
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftPropertySetterAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void SettabilityIsUnchanged()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare(false, false)));
    }

    [Fact]
    public void SetterIsAdded()
    {
        Assert.Equal(["TestType.speed"], Evaluator.FindDifferences(Compare(false, true)));
    }

    [Fact]
    public void RemovingASetterDoesNotFire()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare(true, false)));
    }

    private static ISwiftSignaturesToCompare Compare(bool wasSettable, bool isSettable)
    {
        return BuildSwift.Compare(
            BuildSwift.Struct().WithProperties(
                new SwiftProperty { Name = "TestType.speed", Type = "Int", IsSettable = wasSettable }),
            BuildSwift.Struct().WithProperties(
                new SwiftProperty { Name = "TestType.speed", Type = "Int", IsSettable = isSettable }));
    }
}
