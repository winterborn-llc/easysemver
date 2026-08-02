using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Test.TestSignatureEvaluators;

public class TestMethodInputParameterMadeRequired
{
    private static IEvaluateSignatures Evaluator => new MethodInputParameterMadeRequired();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void MethodInputParameterRequirednessIsNotChanged()
    {
        var signatures = new SignaturesToCompare("",
            older: BuildSolution(isSecondParameterRequired: false),
            newer: BuildSolution(isSecondParameterRequired: false));

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }

    [Fact]
    public void OptionalParameterMadeRequired()
    {
        var signatures = new SignaturesToCompare("",
            older: BuildSolution(isSecondParameterRequired: false),
            newer: BuildSolution(isSecondParameterRequired: true));

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.True(result);
    }

    [Fact]
    public void RequiredParameterMadeOptionalIsNotBreaking()
    {
        var signatures = new SignaturesToCompare("",
            older: BuildSolution(isSecondParameterRequired: true),
            newer: BuildSolution(isSecondParameterRequired: false));

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }

    private static Solution BuildSolution(bool isSecondParameterRequired)
    {
        return new Solution
        {
            new Project("Test")
            {
                Classes =
                [
                    new ProjectClass
                    {
                        Name = "TestClass",
                        Methods =
                        {
                            new Method
                            {
                                MethodName = "TestMethod1",
                                MethodType = "string",
                                Overrides = new MethodOverrides
                                {
                                    new MethodOverride
                                    {
                                        new MethodOverrideInput
                                        {
                                            ParameterName = "input",
                                            ParameterType = "string",
                                            IsRequired = true
                                        },
                                        new MethodOverrideInput
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
            }
        };
    }
}
