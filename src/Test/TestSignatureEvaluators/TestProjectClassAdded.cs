using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.SignatureEvaluation;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Test.TestSignatureEvaluators;

public class TestProjectClassAdded
{
    private static IEvaluateSignatures Evaluator => new ProjectClassAdded();
    
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
                {
                    Classes = 
                    [
                        new ProjectClass
                        {
                            Name = "TestClass"
                        }
                    ]
                }
            ]
            ,
            newer:
            [
                new Project("Test")
                {
                    Classes = 
                    [
                        new ProjectClass
                        {
                            Name = "TestClass"
                        }
                    ]
                }
            ]
        );
        
        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }
    
    [Fact]
    public void ProjectClassAdded()
    {
        var signatures = new Signatures(
            older:
            [
                new Project("Test")
                {
                    Classes = 
                    [
                        new ProjectClass
                        {
                            Name = "TestClass"
                        }
                    ]
                }
            ]
            ,
            newer:
            [
                new Project("Test")
                {
                    Classes = 
                    [
                        new ProjectClass
                        {
                            Name = "TestClass"
                        },
                        new ProjectClass
                        {
                            Name = "NewTestClass"
                        }
                    ]
                }
            ]
        );
        
        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.True(result);
    }
}