using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S03.</summary>
public class TestSwiftTypeKindChanged
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftTypeKindChanged();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void KindIsUnchanged()
    {
        Assert.Empty(Evaluator.FindDifferences(
            BuildSwift.Compare(BuildSwift.Struct(), BuildSwift.Struct())));
    }

    [Fact]
    public void StructBecameAClass()
    {
        Assert.Equal(
            [BuildSwift.DefaultTypeName],
            Evaluator.FindDifferences(BuildSwift.Compare(BuildSwift.Struct(), BuildSwift.Class())));
    }

    [Fact]
    public void ClassBecameAnActor()
    {
        Assert.NotEmpty(Evaluator.FindDifferences(
            BuildSwift.Compare(BuildSwift.Class(), BuildSwift.Actor())));
    }
}
