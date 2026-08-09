using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S37.</summary>
public class TestSwiftPropertyTypeChanged
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftPropertyTypeChanged();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void PropertyTypeIsUnchanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithProperties(BuildSwift.Property(type: "Int")),
            BuildSwift.Struct().WithProperties(BuildSwift.Property(type: "Int")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void PropertyTypeChanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithProperties(BuildSwift.Property(type: "Int")),
            BuildSwift.Struct().WithProperties(BuildSwift.Property(type: "Double")));

        Assert.Equal(["TestType.speed"], Evaluator.FindDifferences(signatures));
    }
}
