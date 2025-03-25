using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.SignatureEvaluation;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Test.TestSignatureEvaluators;

public class TestPropertyType
{
    private static IEvaluateSignatures Evaluator => new PropertyType();
    
    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }
    
    [Fact]
    public void PropertyTypesSame()
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
                new Project("Test")
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
    public void PropertyTypesChanged()
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
                new Project("Test")
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
                                        Type = "NotAString"
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