using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S34 - breaking in both directions.</summary>
public class TestSwiftMemberStaticnessChanged
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftMemberStaticnessChanged();

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
