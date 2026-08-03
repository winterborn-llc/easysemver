using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S02.</summary>
public class TestSwiftTypeAdded
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftTypeAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void TypesAreUnchanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Module(BuildSwift.Struct("Point")),
            BuildSwift.Module(BuildSwift.Struct("Point")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void TypeIsAdded()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Module(BuildSwift.Struct("Point")),
            BuildSwift.Module(BuildSwift.Struct("Point"), BuildSwift.Actor("Counter")));

        Assert.Equal(["Counter"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void RemovingATypeDoesNotFire()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Module(BuildSwift.Struct("Point"), BuildSwift.Actor("Counter")),
            BuildSwift.Module(BuildSwift.Struct("Point")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
