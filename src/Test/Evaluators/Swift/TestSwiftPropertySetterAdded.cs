using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
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
        Assert.False(Evaluator.AreDifferencesPresent(Compare(false, false)));
    }

    [Fact]
    public void SetterIsAdded()
    {
        Assert.True(Evaluator.AreDifferencesPresent(Compare(false, true)));
    }

    [Fact]
    public void RemovingASetterDoesNotFire()
    {
        Assert.False(Evaluator.AreDifferencesPresent(Compare(true, false)));
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
