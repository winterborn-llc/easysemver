using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.SignatureEvaluation;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Test.TestSignatureEvaluators;

public class TestMethodReturnType
{
    private static IEvaluateSignatures Evaluator => new MethodReturnType();
    
    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }
    
    [Fact]
    public void ReturnTypeIsNotChanged()
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
                                Methods =
                                {
                                    ["TestMethod1"] = new Method
                                    {
                                        MethodName = "TestMethod1",
                                        MethodType = "string",
                                        Overrides = 
                                        [
                                            new MethodOverride
                                            {
                                                new MethodOverrideInput
                                                {
                                                    ParameterName = "input",
                                                    ParameterType = "string",
                                                    IsRequired = true
                                                }
                                            }
                                        ]
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
                                Methods =
                                {
                                    ["TestMethod1"] = new Method
                                    {
                                        MethodName = "TestMethod1",
                                        MethodType = "string",
                                        Overrides = 
                                        [
                                            new MethodOverride
                                            {
                                                new MethodOverrideInput
                                                {
                                                    ParameterName = "input",
                                                    ParameterType = "string",
                                                    IsRequired = true
                                                }
                                            }
                                        ]
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
    public void ReturnTypeIsChanged()
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
                                Methods =
                                {
                                    ["TestMethod1"] = new Method
                                    {
                                        MethodName = "TestMethod1",
                                        MethodType = "string",
                                        Overrides = 
                                        [
                                            new MethodOverride
                                            {
                                                new MethodOverrideInput
                                                {
                                                    ParameterName = "input",
                                                    ParameterType = "string",
                                                    IsRequired = true
                                                }
                                            }
                                        ]
                                    }
                                }
                            }
                        ]
                    }
                ],
                newer:
                [
                    new Project("Test")
                    {
                        Classes =
                        [
                            new ProjectClass
                            {
                                Name = "TestClass",
                                Methods =
                                {
                                    ["TestMethod1"] = new Method
                                    {
                                        MethodName = "TestMethod1",
                                        MethodType = "ChangedFromString",
                                        Overrides = 
                                        [
                                            new MethodOverride
                                            {
                                                new MethodOverrideInput
                                                {
                                                    ParameterName = "input",
                                                    ParameterType = "string",
                                                    IsRequired = true
                                                }
                                            }
                                        ]
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