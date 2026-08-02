using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;

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
        var signatures = new SignaturesToCompare("",
            older: new Solution
            {
                new Project("Test")
                {
                    Classes =
                    [
                        new ProjectClass
                        {
                            Name = "TestClass",
                            Methods =
                            {
                                new Method
                                {
                                    MethodName = "TestMethod1",
                                    MethodType = "string",
                                    Overrides = new MethodOverrides
                                    {
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
                                    }
                                }
                            }
                        }
                    ]
                }
            }
            ,
            newer: new Solution
            {
                new Project("Test")
                {
                    Classes =
                    [
                        new ProjectClass
                        {
                            Name = "TestClass",
                            Methods =
                            {
                                new Method
                                {
                                    MethodName = "TestMethod1",
                                    MethodType = "string",
                                    Overrides = new MethodOverrides
                                    {
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
                                    }
                                }
                            }
                        }
                    ]
                }
            }
        );

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }

    [Fact]
    public void MethodOverrideAdded()
    {
        var signatures = new SignaturesToCompare("",
            older: new Solution
            {
                new Project("Test")
                {
                    Classes =
                    [
                        new ProjectClass
                        {
                            Name = "TestClass",
                            Methods =
                            {
                                new Method
                                {
                                    MethodName = "TestMethod1",
                                    MethodType = "string",
                                    Overrides = new MethodOverrides
                                    {
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
                                    }
                                }
                            }
                        }
                    ]
                }
            }
            ,
            newer: new Solution
            {
                new Project("Test")
                {
                    Classes =
                    [
                        new ProjectClass
                        {
                            Name = "TestClass",
                            Methods =
                            {
                                new Method
                                {
                                    MethodName = "TestMethod1",
                                    MethodType = "string",
                                    Overrides = new MethodOverrides
                                    {
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
                                    }
                                }
                            }
                        }
                    ]
                }
            }
        );

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.True(result);
    }
}