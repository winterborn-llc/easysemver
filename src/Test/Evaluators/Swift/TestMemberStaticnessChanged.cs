using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S34 - breaking in both directions.</summary>
public class TestSwiftMemberStaticnessChanged
{
    private static IEvaluateSwiftSignatures Evaluator => new MemberStaticnessChanged();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void StaticnessIsUnchanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()),
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void FunctionBecameStatic()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()),
            BuildSwift.Struct().WithFunctions(
                new SwiftFunction { Name = "TestType.move()", IsStatic = true }));

        Assert.Equal(["TestType.move()"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void FunctionBecameInstance()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithFunctions(
                new SwiftFunction { Name = "TestType.move()", IsStatic = true }),
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()));

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void PropertyBecameStatic()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithProperties(BuildSwift.Property()),
            BuildSwift.Struct().WithProperties(
                new SwiftProperty { Name = "TestType.speed", Type = "Int", IsStatic = true }));

        Assert.Equal(["TestType.speed"], Evaluator.FindDifferences(signatures));
    }
}
