using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.SignatureEvaluation;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Test.TestSignatureEvaluators;

public class TestPropertyEditabilityEnhanced
{
    private static IEvaluateSignatures Evaluator => new PropertyEditabilityEnhanced();
    
    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
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
                                        IsWritable = false
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
        Assert.False(result);
    }
    
    [Fact]
    public void PropertyMadeEditable()
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
                                        IsWritable = false
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
        Assert.True(result);
    }
}