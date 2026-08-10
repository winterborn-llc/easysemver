using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S33 - breaking in both directions, because the call site has to change either way.</summary>
public class TestSwiftParameterModifierChanged
{
    private static IEvaluateSwiftSignatures Evaluator => new ParameterModifierChanged();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ModifiersAreUnchanged()
    {
        var signatures = Compare(new SwiftParameter { Label = "to", Type = "Point" });

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void InoutIsAdded()
    {
        var signatures = Compare(new SwiftParameter
        {
            Label = "to",
            Type = "Point",
            IsInout = true,
            Ownership = "inout"
        });

        Assert.Equal(["TestType.move() (to)"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void VariadicIsAdded()
    {
        var signatures = Compare(new SwiftParameter { Label = "to", Type = "Point", IsVariadic = true });

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void OwnershipChanged()
    {
        var signatures = Compare(new SwiftParameter { Label = "to", Type = "Point", Ownership = "consuming" });

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }

    private static ISwiftSignaturesToCompare Compare(SwiftParameter newer)
    {
        return BuildSwift.Compare(
            BuildSwift.Struct().WithFunctions(BuildSwift.Function().WithParameters(BuildSwift.Parameter())),
            BuildSwift.Struct().WithFunctions(BuildSwift.Function().WithParameters(newer)));
    }
}
