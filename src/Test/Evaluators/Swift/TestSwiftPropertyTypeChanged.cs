using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
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

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void PropertyTypeChanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithProperties(BuildSwift.Property(type: "Int")),
            BuildSwift.Struct().WithProperties(BuildSwift.Property(type: "Double")));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }
}
