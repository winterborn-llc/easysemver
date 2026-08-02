using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Test.TestSignatureEvaluators;

public class TestPropertyAdded
{
    private static IEvaluateSignatures Evaluator => new PropertyAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void PropertiesTheSame()
    {
        var signatures = new SignaturesToCompare("",
            older: new Solution
            {
                new Project("TestProject")
                {
                    Classes =
                    [
                        new ProjectClass
                        {
                            Name = "TestClass",
                            Properties =
                            {
                                new Property
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
                new Project("TestProject")
                {
                    Classes =
                    [
                        new ProjectClass
                        {
                            Name = "TestClass",
                            Properties =
                            {
                                new Property
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
        var signatures = new SignaturesToCompare("",
            older: new Solution
            {
                new Project("TestProject")
                {
                    Classes =
                    [
                        new ProjectClass
                        {
                            Name = "TestClass",
                            Properties =
                            {
                                new Property
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
                new Project("TestProject")
                {
                    Classes =
                    [
                        new ProjectClass
                        {
                            Name = "TestClass",
                            Properties =
                            {
                                new Property
                                {
                                    Name = "TestProperty",
                                    Type = "string"
                                },
                                new Property
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
        var signatures = new SignaturesToCompare("",
            older: new Solution
            {
                new Project("TestProject")
                {
                    Classes =
                    [
                        new ProjectClass
                        {
                            Name = "TestClass",
                            Properties =
                            {
                                new Property
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
                new Project("TestProject")
                {
                    Classes =
                    [
                        new ProjectClass
                        {
                            Name = "TestClass",
                            Properties =
                            {
                                new Property
                                {
                                    Name = "TestProperty",
                                    Type = "string"
                                }
                            }
                        },
                        new ProjectClass
                        {
                            Name = "BrandNewClass",
                            Properties =
                            {
                                new Property
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
