using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

public class TestPropertiesContinueToExist
{
    private static IEvaluateCsharpSignatures Evaluator => new PropertiesContinueToExist();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void PropertiesTheSame()
    {
        var signatures = new CsharpSignaturesToCompare("",
            older: new Solution
            {
                new CsharpProject("TestProject")
                {
                    Classes =
                    [
                        new CsharpClass
                        {
                            Name = "TestClass",
                            Properties =
                            {
                                new CsharpProperty
                                {
                                    Name = "TestProperty",
                                    Type = "string"
                                }
                            }
                        }
                    ]
                }
            }
            ,
            newer: new Solution
            {
                new CsharpProject("TestProject")
                {
                    Classes =
                    [
                        new CsharpClass
                        {
                            Name = "TestClass",
                            Properties =
                            {
                                new CsharpProperty
                                {
                                    Name = "TestProperty",
                                    Type = "string"
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
    public void PropertyRemoved()
    {
        var signatures = new CsharpSignaturesToCompare("",
            older: new Solution
            {
                new CsharpProject("TestProject")
                {
                    Classes =
                    [
                        new CsharpClass
                        {
                            Name = "TestClass",
                            Properties =
                            {
                                new CsharpProperty
                                {
                                    Name = "TestProperty",
                                    Type = "string"
                                }
                            }
                        }
                    ]
                }
            }
            ,
            newer: new Solution
            {
                new CsharpProject("TestProject")
                {
                    Classes =
                    [
                        new CsharpClass
                        {
                            Name = "TestClass",
                            Properties =
                            {
                                new CsharpProperty
                                {
                                    Name = "NotTheSameProperty",
                                    Type = "string"
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