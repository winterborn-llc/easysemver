using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.SignatureEvaluation;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Test.TestSignatureEvaluators;

public class TestMethodOverrideAdded
{
    private static IEvaluateSignatures Evaluator => new MethodOverrideAdded();
    
    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }
    
    [Fact]
    public void MethodOverrideIsNotChanged()
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
    public void MethodOverrideAdded()
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
}