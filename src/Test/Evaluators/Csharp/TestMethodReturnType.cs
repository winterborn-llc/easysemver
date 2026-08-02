using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

public class TestMethodReturnType
{
    private static IEvaluateCsharpSignatures Evaluator => new MethodReturnType();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ReturnTypeIsNotChanged()
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
    public void ReturnTypeIsChanged()
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
                                    }
                                }
                            }
                        }
                    }
                ]
            },
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
                                MethodType = "ChangedFromString",
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
        );

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.True(result);
    }
}