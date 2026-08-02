using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

public class TestProjectClassAdded
{
    private static IEvaluateCsharpSignatures Evaluator => new ProjectClassAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ProjectsSame()
    {
        var signatures = new CsharpSignaturesToCompare(
            older: new CsharpProject("Test")
            {
                Classes =
                [
                    new CsharpClass
                    {
                        Name = "TestClass"
                    }
                ]
            }
            ,
            newer: new CsharpProject("Test")
            {
                Classes =
                [
                    new CsharpClass
                    {
                        Name = "TestClass"
                    }
                ]
            }
        );

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }

    [Fact]
    public void ProjectClassAdded()
    {
        var signatures = new CsharpSignaturesToCompare(
            older: new CsharpProject("Test")
            {
                Classes =
                [
                    new CsharpClass
                    {
                        Name = "TestClass"
                    }
                ]
            }
            ,
            newer: new CsharpProject("Test")
            {
                Classes =
                [
                    new CsharpClass
                    {
                        Name = "TestClass"
                    },
                    new CsharpClass
                    {
                        Name = "NewTestClass"
                    }
                ]
            }
        );

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.True(result);
    }
}