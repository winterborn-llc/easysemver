using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

public class TestMethodInputParameterOverrideRemoved
{
    private static IEvaluateCsharpSignatures Evaluator => new MethodInputParameterOverrideRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void MethodInputParameterOverrideIsNotChanged()
    {
        var signatures = new CsharpSignaturesToCompare(
            older:
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
    public void MethodInputParameterIsRenamed()
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

    [Fact]
    public void MethodInputParameterOverrideTypeChanged()
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
                                            ParameterType = "IAmADifferentType",
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