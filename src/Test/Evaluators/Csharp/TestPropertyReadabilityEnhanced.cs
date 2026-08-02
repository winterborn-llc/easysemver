using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

public class TestPropertyReadabilityEnhanced
{
    private static IEvaluateCsharpSignatures Evaluator => new PropertyReadabilityEnhanced();

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
                                    Type = "string",
                                    IsReadable = false
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
                                    Type = "string",
                                    IsReadable = false
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
    public void PropertyMadeReadable()
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
                                    Type = "string",
                                    IsReadable = false
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
                                    Type = "string",
                                    IsReadable = true
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