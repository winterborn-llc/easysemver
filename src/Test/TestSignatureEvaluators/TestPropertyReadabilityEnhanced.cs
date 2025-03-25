using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.SignatureEvaluation;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Test.TestSignatureEvaluators;

public class TestPropertyReadabilityEnhanced
{
    private static IEvaluateSignatures Evaluator => new PropertyReadabilityEnhanced();
    
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
                                        IsReadable = false
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
                                        IsReadable = false
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
    public void PropertyMadeReadable()
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
                                        IsReadable = false
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
                                        IsReadable = true
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