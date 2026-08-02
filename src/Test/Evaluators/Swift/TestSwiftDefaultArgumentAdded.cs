using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S32 - directional against S31.</summary>
public class TestSwiftDefaultArgumentAdded
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftDefaultArgumentAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void DefaultsAreUnchanged()
    {
        Assert.False(Evaluator.AreDifferencesPresent(Compare(false, false)));
    }

    [Fact]
    public void DefaultIsAdded()
    {
        Assert.True(Evaluator.AreDifferencesPresent(Compare(false, true)));
    }

    [Fact]
    public void RemovingADefaultDoesNotFire()
    {
        Assert.False(Evaluator.AreDifferencesPresent(Compare(true, false)));
    }

    private static ISwiftSignaturesToCompare Compare(bool hadDefault, bool hasDefault)
    {
        return BuildSwift.Compare(
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()
                .WithParameters(BuildSwift.Parameter(hasDefault: hadDefault))),
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()
                .WithParameters(BuildSwift.Parameter(hasDefault: hasDefault))));
    }
}
