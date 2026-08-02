using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S16 - directional against S17.</summary>
public class TestSwiftMemberRemoved
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftMemberRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void MembersAreUnchanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()),
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void FunctionIsRemoved()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()),
            BuildSwift.Struct());

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    /// <summary>SWE-03 - argument labels are part of the identity, so a label change is a removal.</summary>
    [Fact]
    public void ArgumentLabelChangeIsARemoval()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithFunctions(BuildSwift.Function("TestType.move(to:)")),
            BuildSwift.Struct().WithFunctions(BuildSwift.Function("TestType.move(toward:)")));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void PropertyInitializerAndSubscriptAreAllMembers()
    {
        var older = BuildSwift.Struct()
            .WithProperties(BuildSwift.Property())
            .WithInitializers(BuildSwift.Initializer())
            .WithSubscripts(BuildSwift.Subscript());

        Assert.True(Evaluator.AreDifferencesPresent(BuildSwift.Compare(older, BuildSwift.Struct())));
    }
}
