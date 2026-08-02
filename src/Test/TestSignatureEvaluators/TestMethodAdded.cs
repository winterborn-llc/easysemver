using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Test.TestSignatureEvaluators;

public class TestMethodAdded
{
    private static IEvaluateSignatures Evaluator => new MethodAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void MethodsAreNotChanged()
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
    public void MethodIsAddedToExistingClass()
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
                                        }
                                    }
                                },
                                new Method
                                {
                                    MethodName = "TestMethod2",
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

    [Fact]
    public void MethodOnBrandNewClassIsNotCounted()
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
                                        }
                                    }
                                }
                            }
                        },
                        new ProjectClass
                        {
                            Name = "BrandNewClass",
                            Methods =
                            {
                                new Method
                                {
                                    MethodName = "BrandNewMethod",
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
}
