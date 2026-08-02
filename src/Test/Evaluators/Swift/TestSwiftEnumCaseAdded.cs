using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S18 - Major by design (SCL-01), unlike C#'s R23.</summary>
public class TestSwiftEnumCaseAdded
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftEnumCaseAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void CasesAreUnchanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Enum().WithCases(BuildSwift.Case("TestType.red")),
            BuildSwift.Enum().WithCases(BuildSwift.Case("TestType.red")));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    /// <summary>
    /// SCL-01 - a client switching exhaustively stops compiling, which is every client of a
    /// package built without library evolution.
    /// </summary>
    [Fact]
    public void CaseIsAdded()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Enum().WithCases(BuildSwift.Case("TestType.red")),
            BuildSwift.Enum().WithCases(BuildSwift.Case("TestType.red"), BuildSwift.Case("TestType.green")));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void RemovingACaseDoesNotFire()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Enum().WithCases(BuildSwift.Case("TestType.red"), BuildSwift.Case("TestType.green")),
            BuildSwift.Enum().WithCases(BuildSwift.Case("TestType.red")));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }
}
