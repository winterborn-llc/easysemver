using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S38.</summary>
public class TestSwiftOperatorChanged
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftOperatorChanged();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void OperatorsAreUnchanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Module().WithOperators(BuildSwift.Operator()),
            BuildSwift.Module().WithOperators(BuildSwift.Operator()));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void OperatorIsRemoved()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Module().WithOperators(BuildSwift.Operator()),
            BuildSwift.Module());

        Assert.Equal(["<~>(_:_:)"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void PrecedenceGroupChanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Module().WithOperators(BuildSwift.Operator(precedenceGroup: "AdditionPrecedence")),
            BuildSwift.Module().WithOperators(BuildSwift.Operator(precedenceGroup: "ComparisonPrecedence")));

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }
}
