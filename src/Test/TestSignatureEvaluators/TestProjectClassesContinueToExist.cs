using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.SignatureEvaluation;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Test.TestSignatureEvaluators;

public class TestProjectClassesContinueToExist
{
    private static IEvaluateSignatures Evaluator => new ProjectClassesContinueToExist();
    
    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
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
                        },
                        new ProjectClass
                        {
                            Name = "NewClass"
                        }
                    ]
                }
            ]
        );
        
        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }
    
    [Fact]
    public void ClassNotFound()
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
                            Name = "TestClass2"
                        }
                    ]
                }
            ]
        );
        
        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.True(result);
    }
}