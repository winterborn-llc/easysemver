using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

public class TestPropertyType
{
    private static IEvaluateCsharpSignatures Evaluator => new PropertyType();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void PropertyTypesSame()
    {
        var signatures = new CsharpSignaturesToCompare("",
            older: new Solution
            {
                new CsharpProject("Test")
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
                new CsharpProject("Test")
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
    public void PropertyTypesChanged()
    {
        var signatures = new CsharpSignaturesToCompare("",
            older: new Solution
            {
                new CsharpProject("Test")
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
                new CsharpProject("Test")
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
                                    Type = "NotAString"
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