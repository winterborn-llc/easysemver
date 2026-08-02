using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

public class TestPropertyAdded
{
    private static IEvaluateCsharpSignatures Evaluator => new PropertyAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
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
    public void PropertyIsAddedToExistingClass()
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
                                },
                                new CsharpProperty
                                {
                                    Name = "BrandNewProperty",
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

    [Fact]
    public void PropertyOnBrandNewClassIsNotCounted()
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
                        },
                        new CsharpClass
                        {
                            Name = "BrandNewClass",
                            Properties =
                            {
                                new CsharpProperty
                                {
                                    Name = "BrandNewProperty",
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
}
