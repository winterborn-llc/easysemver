using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S08.</summary>
public class TestSwiftSuperclassChanged
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftSuperclassChanged();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void SuperclassIsUnchanged()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare("Base", "Base")));
    }

    [Fact]
    public void SuperclassChanged()
    {
        Assert.Equal(
            [BuildSwift.DefaultTypeName],
            Evaluator.FindDifferences(Compare("Base", "OtherBase")));
    }

    [Fact]
    public void SuperclassRemoved()
    {
        Assert.NotEmpty(Evaluator.FindDifferences(Compare("Base", "")));
    }

    private static ISwiftSignaturesToCompare Compare(string older, string newer)
    {
        return BuildSwift.Compare(
            new SwiftClass { Name = BuildSwift.DefaultTypeName, Superclass = older },
            new SwiftClass { Name = BuildSwift.DefaultTypeName, Superclass = newer });
    }
}
