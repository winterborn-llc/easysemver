using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.SignatureEvaluation;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Test.TestSignatureEvaluators;

public class TestPropertiesContinueToExist
{
    private static IEvaluateSignatures Evaluator => new PropertiesContinueToExist();
    
    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }
    
    [Fact]
    public void PropertiesTheSame()
    {
        var signatures = new Signatures(
            older:
            [
                new Project("TestProject")
                {
                    Classes = 
                    [
                        new ProjectClass
                        {
                            Name = "TestClass",
                            Properties =
                            {
                                {
                                    "TestProperty",
                                    new Property
                                    {
                                        Name = "TestProperty",
                                        Type = "string"
                                    }
                                }
                            }
                        }
                    ]
                }
            ]
            ,
            newer:
            [
                new Project("TestProject")
                {
                    Classes = 
                    [
                        new ProjectClass
                        {
                            Name = "TestClass",
                            Properties =
                            {
                                {
                                    "TestProperty",
                                    new Property
                                    {
                                        Name = "TestProperty",
                                        Type = "string"
                                    }
                                }
                            }
                        }
                    ]
                }
            ]
        );
        
        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }
    
    [Fact]
    public void PropertyRemoved()
    {
        var signatures = new Signatures(
            older:
            [
                new Project("TestProject")
                {
                    Classes = 
                    [
                        new ProjectClass
                        {
                            Name = "TestClass",
                            Properties =
                            {
                                {
                                    "TestProperty",
                                    new Property
                                    {
                                        Name = "TestProperty",
                                        Type = "string"
                                    }
                                }
                            }
                        }
                    ]
                }
            ]
            ,
            newer:
            [
                new Project("TestProject")
                {
                    Classes = 
                    [
                        new ProjectClass
                        {
                            Name = "TestClass",
                            Properties =
                            {
                                {
                                    "NotTheSameProperty",
                                    new Property
                                    {
                                        Name = "NotTheSameProperty",
                                        Type = "string"
                                    }
                                }
                            }
                        }
                    ]
                }
            ]
        );
        
        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.True(result);
    }
}