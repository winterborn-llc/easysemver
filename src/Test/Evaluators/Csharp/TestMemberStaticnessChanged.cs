using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R38 - breaking in both directions, because every call site has to be rewritten.</summary>
public class TestMemberStaticnessChanged
{
    private static IEvaluateCsharpSignatures Evaluator => new MemberStaticnessChanged();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void StaticnessIsUnchanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithMethods(Build.Method()),
            Build.Class().WithMethods(Build.Method()));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void MethodBecomesStatic()
    {
        var signatures = Build.Compare(
            Build.Class().WithMethods(Build.Method(overrides: new CsharpMethodOverride())),
            Build.Class().WithMethods(Build.Method(overrides: new CsharpMethodOverride { IsStatic = true })));

        Assert.Equal(["Test.TestType.TestMethod()"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void MethodBecomesInstance()
    {
        var signatures = Build.Compare(
            Build.Class().WithMethods(Build.Method(overrides: new CsharpMethodOverride { IsStatic = true })),
            Build.Class().WithMethods(Build.Method(overrides: new CsharpMethodOverride())));

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void PropertyBecomesStatic()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(Build.Property()),
            Build.Class().WithProperties(new CsharpProperty
            {
                Name = "TestProperty",
                Type = "string",
                IsReadable = true,
                IsWritable = true,
                IsStatic = true
            }));

        Assert.Equal(["Test.TestType.TestProperty"], Evaluator.FindDifferences(signatures));
    }
}
