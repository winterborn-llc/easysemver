using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

public class TestMethodAdded
{
    private static IEvaluateCsharpSignatures Evaluator => new MethodAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void MethodsAreNotChanged()
    {
        var signatures = new CsharpSignaturesToCompare("",
            older: new Solution
            {
                new CsharpProject("Test")
                {
                    Classes =
                    [
                        new CsharpClass
                        {
                            Name = "TestClass",
                            Methods =
                            {
                                new CsharpMethod
                                {
                                    MethodName = "TestMethod1",
                                    MethodType = "string",
                                    Overrides = new CsharpMethodOverrides
                                    {
                                        new CsharpMethodOverride
                                        {
                                            new CsharpMethodParameter
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
                new CsharpProject("Test")
                {
                    Classes =
                    [
                        new CsharpClass
                        {
                            Name = "TestClass",
                            Methods =
                            {
                                new CsharpMethod
                                {
                                    MethodName = "TestMethod1",
                                    MethodType = "string",
                                    Overrides = new CsharpMethodOverrides
                                    {
                                        new CsharpMethodOverride
                                        {
                                            new CsharpMethodParameter
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
        var signatures = new CsharpSignaturesToCompare("",
            older: new Solution
            {
                new CsharpProject("Test")
                {
                    Classes =
                    [
                        new CsharpClass
                        {
                            Name = "TestClass",
                            Methods =
                            {
                                new CsharpMethod
                                {
                                    MethodName = "TestMethod1",
                                    MethodType = "string",
                                    Overrides = new CsharpMethodOverrides
                                    {
                                        new CsharpMethodOverride
                                        {
                                            new CsharpMethodParameter
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
                new CsharpProject("Test")
                {
                    Classes =
                    [
                        new CsharpClass
                        {
                            Name = "TestClass",
                            Methods =
                            {
                                new CsharpMethod
                                {
                                    MethodName = "TestMethod1",
                                    MethodType = "string",
                                    Overrides = new CsharpMethodOverrides
                                    {
                                        new CsharpMethodOverride
                                        {
                                            new CsharpMethodParameter
                                            {
                                                ParameterName = "input",
                                                ParameterType = "string",
                                                IsRequired = true
                                            }
                                        }
                                    }
                                },
                                new CsharpMethod
                                {
                                    MethodName = "TestMethod2",
                                    MethodType = "string",
                                    Overrides = new CsharpMethodOverrides
                                    {
                                        new CsharpMethodOverride
                                        {
                                            new CsharpMethodParameter
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
        var signatures = new CsharpSignaturesToCompare("",
            older: new Solution
            {
                new CsharpProject("Test")
                {
                    Classes =
                    [
                        new CsharpClass
                        {
                            Name = "TestClass",
                            Methods =
                            {
                                new CsharpMethod
                                {
                                    MethodName = "TestMethod1",
                                    MethodType = "string",
                                    Overrides = new CsharpMethodOverrides
                                    {
                                        new CsharpMethodOverride
                                        {
                                            new CsharpMethodParameter
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
                new CsharpProject("Test")
                {
                    Classes =
                    [
                        new CsharpClass
                        {
                            Name = "TestClass",
                            Methods =
                            {
                                new CsharpMethod
                                {
                                    MethodName = "TestMethod1",
                                    MethodType = "string",
                                    Overrides = new CsharpMethodOverrides
                                    {
                                        new CsharpMethodOverride
                                        {
                                            new CsharpMethodParameter
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
                        new CsharpClass
                        {
                            Name = "BrandNewClass",
                            Methods =
                            {
                                new CsharpMethod
                                {
                                    MethodName = "BrandNewMethod",
                                    MethodType = "string",
                                    Overrides = new CsharpMethodOverrides
                                    {
                                        new CsharpMethodOverride
                                        {
                                            new CsharpMethodParameter
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
