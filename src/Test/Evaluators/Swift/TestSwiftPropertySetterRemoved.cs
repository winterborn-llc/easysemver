using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S35 - directional against S36.</summary>
public class TestSwiftPropertySetterRemoved
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftPropertySetterRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void SettabilityIsUnchanged()
    {
        Assert.False(Evaluator.AreDifferencesPresent(Compare(true, true)));
    }

    [Fact]
    public void SetterIsRemoved()
    {
        Assert.True(Evaluator.AreDifferencesPresent(Compare(true, false)));
    }

    [Fact]
    public void AddingASetterDoesNotFire()
    {
        Assert.False(Evaluator.AreDifferencesPresent(Compare(false, true)));
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
