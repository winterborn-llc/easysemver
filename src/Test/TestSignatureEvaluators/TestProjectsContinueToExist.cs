using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Test.TestSignatureEvaluators;

public class TestProjectsContinueToExist
{
    private static IEvaluateSignatures Evaluator => new ProjectsContinueToExist();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ProjectsStillExist()
    {
        var signatures = new SignaturesToCompare("",
            older: new Solution
            {
                new Project("Test")
            }
            ,
            newer: new Solution
            {
                new Project("Test"),
                new Project("Test2")
            }
        );

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }

    [Fact]
    public void ProjectsNoLongerExist()
    {
        var signatures = new SignaturesToCompare("",
            older: new Solution
            {
                new Project("Test")
            }
            ,
            newer: new Solution
            {
                new Project("NewTest")
            }
        );

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.True(result);
    }
}