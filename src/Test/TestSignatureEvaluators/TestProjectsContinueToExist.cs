using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.SignatureEvaluation;
using Yamamari.Library.AutoVersion.SignatureStructure;

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
        var signatures = new Signatures(
            older:
            [
                new Project("Test")
            ]
            ,
            newer:
            [
                new Project("Test"),
                new Project("Test2")
            ]
        );
        
        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }
    
    [Fact]
    public void ProjectsNoLongerExist()
    {
        var signatures = new Signatures(
            older:
            [
                new Project("Test")
            ]
            ,
            newer:
            [
                new Project("NewTest")
            ]
        );
        
        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.True(result);
    }
}