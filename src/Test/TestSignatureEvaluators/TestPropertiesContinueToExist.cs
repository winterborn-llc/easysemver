using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Test.TestSignatureEvaluators;

public class TestPropertiesContinueToExist
{
    private static IEvaluateSignatures Evaluator => new PropertiesContinueToExist();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
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
    public void PropertyRemoved()
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