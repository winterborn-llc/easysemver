using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Test.TestSignatureEvaluators;

public class TestMethodsContinueToExist
{
    private static IEvaluateSignatures Evaluator => new MethodsContinueToExist();

    [Fact]
    public void TestChangeType()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void EvaluateMethodStillExists()
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
                                    MethodType = "void"
                                },
                                new Method
                                {
                                    MethodName = "TestMethod2",
                                    MethodType = "void"
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
                                    MethodType = "void"
                                },
                                new Method
                                {
                                    MethodName = "TestMethod2",
                                    MethodType = "void"
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
    public void EvaluateMethodNoLongerExists()
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
                                    MethodType = "void"
                                },
                                new Method
                                {
                                    MethodName = "TestMethod2",
                                    MethodType = "void"
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
                                    MethodType = "void"
                                },
                                new Method
                                {
                                    MethodName = "TestMethod3",
                                    MethodType = "void"
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