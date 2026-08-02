using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

public class TestMethodOverrideAdded
{
    private static IEvaluateCsharpSignatures Evaluator => new MethodOverrideAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void MethodOverrideIsNotChanged()
    {
        var signatures = new CsharpSignaturesToCompare(
            older: new CsharpProject("Test")
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
                                    },
                                    new CsharpMethodOverride
                                    {
                                        new CsharpMethodParameter
                                        {
                                            ParameterName = "input",
                                            ParameterType = "string",
                                            IsRequired = true
                                        },
                                        new CsharpMethodParameter
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
            ,
            newer: new CsharpProject("Test")
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
                                    },
                                    new CsharpMethodOverride
                                    {
                                        new CsharpMethodParameter
                                        {
                                            ParameterName = "input",
                                            ParameterType = "string",
                                            IsRequired = true
                                        },
                                        new CsharpMethodParameter
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
        );

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }

    [Fact]
    public void MethodOverrideAdded()
    {
        var signatures = new CsharpSignaturesToCompare(
            older: new CsharpProject("Test")
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
                                        },
                                        new CsharpMethodParameter
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
            ,
            newer: new CsharpProject("Test")
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
                                        },
                                        new CsharpMethodParameter
                                        {
                                            ParameterName = "output",
                                            ParameterType = "string",
                                            IsRequired = true
                                        }
                                    },
                                    new CsharpMethodOverride
                                    {
                                        new CsharpMethodParameter
                                        {
                                            ParameterName = "input",
                                            ParameterType = "string",
                                            IsRequired = true
                                        },
                                        new CsharpMethodParameter
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
        );

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.True(result);
    }
}