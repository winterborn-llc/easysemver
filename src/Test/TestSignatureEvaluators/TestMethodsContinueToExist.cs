using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.SignatureEvaluation;
using Yamamari.Library.AutoVersion.SignatureStructure;

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
                                        MethodType = "void"
                                    },
                                    ["TestMethod2"] = new Method
                                    {
                                        MethodName = "TestMethod2",
                                        MethodType = "void"
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
                                        MethodType = "void"
                                    },
                                    ["TestMethod2"] = new Method
                                    {
                                        MethodName = "TestMethod2",
                                        MethodType = "void"
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
    public void EvaluateMethodNoLongerExists()
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
                                        MethodType = "void"
                                    },
                                    ["TestMethod2"] = new Method
                                    {
                                        MethodName = "TestMethod2",
                                        MethodType = "void"
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
                                        MethodType = "void"
                                    },
                                    ["TestMethod3"] = new Method
                                    {
                                        MethodName = "TestMethod3",
                                        MethodType = "void"
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