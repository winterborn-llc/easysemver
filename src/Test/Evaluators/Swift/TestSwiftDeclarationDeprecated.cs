using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S26 - the only Patch-impact rule in the set.</summary>
public class TestSwiftDeclarationDeprecated
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftDeclarationDeprecated();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Patch, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void AvailabilityIsUnchanged()
    {
        Assert.Empty(Evaluator.FindDifferences(
            BuildSwift.Compare(BuildSwift.Struct(), BuildSwift.Struct())));
    }

    [Fact]
    public void DeclarationBecameDeprecated()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct(),
            BuildSwift.Struct().WithAvailability(BuildSwift.Available(isDeprecated: true)));

        Assert.Equal([BuildSwift.DefaultTypeName], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void AlreadyDeprecatedDoesNotFireAgain()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithAvailability(BuildSwift.Available(isDeprecated: true)),
            BuildSwift.Struct().WithAvailability(BuildSwift.Available(isDeprecated: true)));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void MemberDeprecationIsAlsoSeen()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()),
            BuildSwift.Struct().WithFunctions(
                BuildSwift.Function().WithAvailability(BuildSwift.Available(isDeprecated: true))));

        Assert.Equal(["TestType.move()"], Evaluator.FindDifferences(signatures));
    }
}
