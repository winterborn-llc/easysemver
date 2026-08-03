using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S12 - directional against S13.</summary>
public class TestSwiftGenericConstraintTightened
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftGenericConstraintTightened();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ConstraintsAreUnchanged()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare("conformance Equatable", "conformance Equatable")));
    }

    [Fact]
    public void ConstraintIsAdded()
    {
        Assert.Equal(
            [BuildSwift.DefaultTypeName],
            Evaluator.FindDifferences(
                Compare("conformance Equatable", "conformance Equatable, conformance Hashable")));
    }

    [Fact]
    public void RemovingAConstraintDoesNotFire()
    {
        Assert.Empty(Evaluator.FindDifferences(
            Compare("conformance Equatable, conformance Hashable", "conformance Equatable")));
    }

    private static ISwiftSignaturesToCompare Compare(string older, string newer)
    {
        return BuildSwift.Compare(
            BuildSwift.Struct().WithGenerics(BuildSwift.Generic(constraints: older)),
            BuildSwift.Struct().WithGenerics(BuildSwift.Generic(constraints: newer)));
    }
}
