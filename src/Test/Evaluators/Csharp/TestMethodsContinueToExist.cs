using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

public class TestMethodsContinueToExist
{
    private static IEvaluateCsharpSignatures Evaluator => new MethodsContinueToExist();

    [Fact]
    public void TestChangeType()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void EvaluateMethodStillExists()
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
                                MethodType = "void"
                            },
                            new CsharpMethod
                            {
                                MethodName = "TestMethod2",
                                MethodType = "void"
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
                                MethodType = "void"
                            },
                            new CsharpMethod
                            {
                                MethodName = "TestMethod2",
                                MethodType = "void"
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
    public void EvaluateMethodNoLongerExists()
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
                                MethodType = "void"
                            },
                            new CsharpMethod
                            {
                                MethodName = "TestMethod2",
                                MethodType = "void"
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
                                MethodType = "void"
                            },
                            new CsharpMethod
                            {
                                MethodName = "TestMethod3",
                                MethodType = "void"
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