using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Test.TestSignatureEvaluators;

public class TestProjectClassesContinueToExist
{
    private static IEvaluateSignatures Evaluator => new ProjectClassesContinueToExist();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ProjectsSame()
    {
        var signatures = new SignaturesToCompare("",
            older: new Solution
            {
                new Project("Test")
                {
                    Classes =
                    [
                        new ProjectClass
                        {
                            Name = "TestClass"
                        }
                    ]
                }
            }
            ,
            newer: new Solution
            {
                new Project("Test")
                {
                    Classes =
                    [
                        new ProjectClass
                        {
                            Name = "TestClass"
                        },
                        new ProjectClass
                        {
                            Name = "NewClass"
                        }
                    ]
                }
            }
        );

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.False(result);
    }

    [Fact]
    public void ClassNotFound()
    {
        var signatures = new SignaturesToCompare("",
            older: new Solution
            {
                new Project("Test")
                {
                    Classes =
                    [
                        new ProjectClass
                        {
                            Name = "TestClass"
                        }
                    ]
                }
            }
            ,
            newer: new Solution
            {
                new Project("Test")
                {
                    Classes =
                    [
                        new ProjectClass
                        {
                            Name = "TestClass2"
                        }
                    ]
                }
            }
        );

        var result = Evaluator.AreDifferencesPresent(signatures);
        Assert.True(result);
    }
}