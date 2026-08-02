using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

public class TestProjectAdded
{
    private static IEvaluateCsharpSignatures Evaluator => new ProjectAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ProjectsSame()
    {
        var signatures = new CsharpSignaturesToCompare("",
            older: new Solution
            {
                new CsharpProject("Test")
            }
            ,
            newer: new Solution
            {
                new CsharpProject("Test")
            }
        );

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }

    [Fact]
    public void ProjectAdded()
    {
        var signatures = new CsharpSignaturesToCompare("",
            older: new Solution
            {
                new CsharpProject("Test")
            }
            ,
            newer: new Solution
            {
                new CsharpProject("Test"),
                new CsharpProject("Test2")
            }
        );

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.True(result);
    }
}