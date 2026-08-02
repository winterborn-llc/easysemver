using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Test.TestSignatureEvaluators;

public class TestPropertyEditabilityEnhanced
{
    private static IEvaluateSignatures Evaluator => new PropertyEditabilityEnhanced();

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
                                    Type = "string",
                                    IsWritable = false
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
                                    Type = "string",
                                    IsWritable = false
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
    public void PropertyMadeEditable()
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
                                    Type = "string",
                                    IsWritable = false
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
                                    Type = "string",
                                    IsWritable = true
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