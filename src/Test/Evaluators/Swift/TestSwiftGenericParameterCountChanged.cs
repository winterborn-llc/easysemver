using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S11.</summary>
public class TestSwiftGenericParameterCountChanged
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftGenericParameterCountChanged();

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

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void TypeParameterCountChanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithGenerics(BuildSwift.Generic()),
            BuildSwift.Struct().WithGenerics(BuildSwift.Generic(), BuildSwift.Generic("U")));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void FunctionParameterCountChanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithFunctions(BuildSwift.Function().WithGenerics(BuildSwift.Generic())),
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }
}
