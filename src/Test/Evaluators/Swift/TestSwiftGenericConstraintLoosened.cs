using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S13 - directional against S12.</summary>
public class TestSwiftGenericConstraintLoosened
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftGenericConstraintLoosened();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ConstraintsAreUnchanged()
    {
        Assert.False(Evaluator.AreDifferencesPresent(Compare("conformance Equatable", "conformance Equatable")));
    }

    [Fact]
    public void ConstraintIsRemoved()
    {
        Assert.True(Evaluator.AreDifferencesPresent(
            Compare("conformance Equatable, conformance Hashable", "conformance Equatable")));
    }

    [Fact]
    public void AddingAConstraintDoesNotFire()
    {
        Assert.False(Evaluator.AreDifferencesPresent(
            Compare("conformance Equatable", "conformance Equatable, conformance Hashable")));
    }

    private static ISwiftSignaturesToCompare Compare(string older, string newer)
    {
        return BuildSwift.Compare(
            BuildSwift.Struct().WithGenerics(BuildSwift.Generic(constraints: older)),
            BuildSwift.Struct().WithGenerics(BuildSwift.Generic(constraints: newer)));
    }
}
