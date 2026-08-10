using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S11.</summary>
public class TestSwiftGenericParameterCountChanged
{
    private static IEvaluateSwiftSignatures Evaluator => new GenericParameterCountChanged();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void GenericsAreUnchanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithGenerics(BuildSwift.Generic()),
            BuildSwift.Struct().WithGenerics(BuildSwift.Generic()));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void TypeParameterCountChanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithGenerics(BuildSwift.Generic()),
            BuildSwift.Struct().WithGenerics(BuildSwift.Generic(), BuildSwift.Generic("U")));

        Assert.Equal([BuildSwift.DefaultTypeName], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void FunctionParameterCountChanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithFunctions(BuildSwift.Function().WithGenerics(BuildSwift.Generic())),
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()));

        // A function-level change is reported against the function, not its type.
        Assert.Equal(["TestType.move()"], Evaluator.FindDifferences(signatures));
    }
}
