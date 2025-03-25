using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.SignatureEvaluation;
using Yamamari.Library.AutoVersion.SignatureStructure;

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
        var signatures = new Signatures(
                older:
                [
                    new Project("Test")
                ]
                ,
                newer:
                [
                    new Project("Test")
                ]
        );
        
        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }
    
    [Fact]
    public void ProjectAdded()
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
        Assert.True(result);
    }
}