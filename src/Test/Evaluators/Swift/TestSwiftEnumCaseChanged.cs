using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S19.</summary>
public class TestSwiftEnumCaseChanged
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftEnumCaseChanged();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void CasesAreUnchanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Enum().WithCases(BuildSwift.Case("TestType.red", "r")),
            BuildSwift.Enum().WithCases(BuildSwift.Case("TestType.red", "r")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void CaseIsRemoved()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Enum().WithCases(BuildSwift.Case("TestType.red")),
            BuildSwift.Enum());

        Assert.Equal(["TestType.red"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void RawValueChanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Enum().WithCases(BuildSwift.Case("TestType.red", "r")),
            BuildSwift.Enum().WithCases(BuildSwift.Case("TestType.red", "R")));

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void AssociatedValuesChanged()
    {
        var older = new SwiftEnumCase { Name = "TestType.green" };
        older.AssociatedValues.Add(BuildSwift.Parameter("shade", "Int"));
        var newer = new SwiftEnumCase { Name = "TestType.green" };
        newer.AssociatedValues.Add(BuildSwift.Parameter("shade", "Double"));

        var signatures = BuildSwift.Compare(
            BuildSwift.Enum().WithCases(older),
            BuildSwift.Enum().WithCases(newer));

        Assert.Equal(["TestType.green"], Evaluator.FindDifferences(signatures));
    }
}
