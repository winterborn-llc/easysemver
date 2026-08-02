using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

public class TestMethodInputParameterMadeRequired
{
    private static IEvaluateCsharpSignatures Evaluator => new MethodInputParameterMadeRequired();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void MethodInputParameterRequirednessIsNotChanged()
    {
        var signatures = new CsharpSignaturesToCompare(
            older: BuildProject(isSecondParameterRequired: false),
            newer: BuildProject(isSecondParameterRequired: false));

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }

    [Fact]
    public void OptionalParameterMadeRequired()
    {
        var signatures = new CsharpSignaturesToCompare(
            older: BuildProject(isSecondParameterRequired: false),
            newer: BuildProject(isSecondParameterRequired: true));

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.True(result);
    }

    [Fact]
    public void RequiredParameterMadeOptionalIsNotBreaking()
    {
        var signatures = new CsharpSignaturesToCompare(
            older: BuildProject(isSecondParameterRequired: true),
            newer: BuildProject(isSecondParameterRequired: false));

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }

    private static CsharpProject BuildProject(bool isSecondParameterRequired)
    {
        return new CsharpProject("Test")
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
                            MethodType = "string",
                            Overrides = new CsharpMethodOverrides
                            {
                                new CsharpMethodOverride
                                {
                                    new CsharpMethodParameter
                                    {
                                        ParameterName = "input",
                                        ParameterType = "string",
                                        IsRequired = true
                                    },
                                    new CsharpMethodParameter
                                    {
                                        ParameterName = "output",
                                        ParameterType = "string",
                                        IsRequired = isSecondParameterRequired
                                    }
                                }
                            }
                        }
                    }
                }
            ]
        };
    }
}
