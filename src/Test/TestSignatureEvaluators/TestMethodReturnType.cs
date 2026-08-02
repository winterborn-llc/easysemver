using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;

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
    public void ReturnTypeIsChanged()
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
            },
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
                                    MethodType = "ChangedFromString",
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
}