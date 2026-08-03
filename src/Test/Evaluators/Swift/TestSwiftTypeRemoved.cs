using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S01.</summary>
public class TestSwiftTypeRemoved
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftTypeRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void TypesAreUnchanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Module(BuildSwift.Struct("Point"), BuildSwift.Class("Gadget")),
            BuildSwift.Module(BuildSwift.Struct("Point"), BuildSwift.Class("Gadget")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void TypeIsRemoved()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Module(BuildSwift.Struct("Point"), BuildSwift.Class("Gadget")),
            BuildSwift.Module(BuildSwift.Struct("Point")));

        // The rule names what it found, which is what a dry run reports (§20 O-04).
        Assert.Equal(["Gadget"], Evaluator.FindDifferences(signatures));
    }

    /// <summary>SWE-02 - dropping from public to internal reads as a removal, which is correct.</summary>
    [Fact]
    public void TypeMadeInternalReadsAsRemoved()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Module(BuildSwift.Struct("Point")),
            BuildSwift.Module());

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }
}
