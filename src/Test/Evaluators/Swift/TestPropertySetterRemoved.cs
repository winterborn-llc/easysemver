using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S35 - directional against S36.</summary>
public class TestSwiftPropertySetterRemoved
{
    private static IEvaluateSwiftSignatures Evaluator => new PropertySetterRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void SettabilityIsUnchanged()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare(true, true)));
    }

    [Fact]
    public void SetterIsRemoved()
    {
        Assert.Equal(["TestType.speed"], Evaluator.FindDifferences(Compare(true, false)));
    }

    [Fact]
    public void AddingASetterDoesNotFire()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare(false, true)));
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
