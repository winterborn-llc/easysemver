using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Test.TestSignatureEvaluators;

public class TestProjectAdded
{
    private static IEvaluateSignatures Evaluator => new ProjectAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ProjectsSame()
    {
        var signatures = new SignaturesToCompare("",
            older: new Solution
            {
                new Project("Test")
            }
            ,
            newer: new Solution
            {
                new Project("Test")
            }
        );

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }

    [Fact]
    public void ProjectAdded()
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
        Assert.True(result);
    }
}