using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.SignatureEvaluation;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Test.TestSignatureEvaluators;

public class TestPropertyEditabilityReduced
{
    private static IEvaluateSignatures Evaluator => new PropertyEditabilityReduced();
    
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
                                        Type = "string",
                                        IsWritable = true
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
                                        Type = "string",
                                        IsWritable = true
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
    public void PropertyMadeReadOnly()
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
                                        Type = "string",
                                        IsWritable = true
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
                                        Type = "string",
                                        IsWritable = false
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