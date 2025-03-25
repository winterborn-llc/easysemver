using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.SignatureEvaluation;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Test.TestSignatureEvaluators;

public class TestMethodInputParameterOverrideRemoved
{
    private static IEvaluateSignatures Evaluator => new MethodInputParameterOverrideRemoved();
    
    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }
    
    [Fact]
    public void MethodInputParameterOverrideIsNotChanged()
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
                                            },
                                            new MethodOverride
                                            {
                                                new MethodOverrideInput
                                                {
                                                    ParameterName = "input",
                                                    ParameterType = "string",
                                                    IsRequired = true
                                                },
                                                new MethodOverrideInput
                                                {
                                                    ParameterName = "output",
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
                                            },
                                            new MethodOverride
                                            {
                                                new MethodOverrideInput
                                                {
                                                    ParameterName = "input",
                                                    ParameterType = "string",
                                                    IsRequired = true
                                                },
                                                new MethodOverrideInput
                                                {
                                                    ParameterName = "output",
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
    public void MethodInputParameterIsRenamed()
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
                                                },
                                                new MethodOverrideInput
                                                {
                                                    ParameterName = "output",
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
                                                },
                                                new MethodOverrideInput
                                                {
                                                    ParameterName = "input2",
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
    
    [Fact]
    public void MethodInputParameterOverrideTypeChanged()
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
                                                },
                                                new MethodOverrideInput
                                                {
                                                    ParameterName = "output",
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
                                                },
                                                new MethodOverrideInput
                                                {
                                                    ParameterName = "output",
                                                    ParameterType = "IAmADifferentType",
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