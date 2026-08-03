using System.Text.RegularExpressions;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Test;

/// <summary>
/// The rule identifiers are a published contract (REP-02), and a contract nobody checks is a
/// contract that drifts. It already had: six C# rules carried an id in their documentation that
/// disagreed with the table in specs/07 - MethodOverrideAdded claimed R16 where the spec says
/// R04, PropertyAdded claimed R17 where the spec says R16, and four more - which is exactly the
/// failure this asserts against.
/// <para>
/// The spec tables are the authority. These tests read them at run time rather than restating
/// them, so a rule added to the code without a row in the table, or a row without a rule, fails
/// here rather than being discovered by a consumer.
/// </para>
/// </summary>
public class TestRuleIds
{
    private const string CsharpSpec = "07-change-classification.md";

    private const string SwiftSpec = "12-multi-language-swift-and-folder-model.md";

    /// <summary>
    /// R41 is one requirement with two directions, so its two classes share an id deliberately -
    /// see specs/07. Anything else sharing an id is a mistake.
    /// </summary>
    private static readonly string[] IdsSharedBySeveralRules = ["R41"];

    /// <summary>
    /// CLS-04's fail-safe is raised by the comparator, not by a rule, so it has no row of its own
    /// in the rule tables.
    /// </summary>
    private const string ComparatorId = "CLS-04";

    private static IReadOnlyList<(string Id, string Name)> CsharpRules =>
        GetRules<IEvaluateCsharpSignatures>(r => r.RuleId);

    private static IReadOnlyList<(string Id, string Name)> SwiftRules =>
        GetRules<IEvaluateSwiftSignatures>(r => r.RuleId);

    private static IReadOnlyList<(string Id, string Name)> NeutralRules =>
        GetRules<IEvaluateUnitExistence>(r => r.RuleId);

    private static IReadOnlyList<(string Id, string Name)> AllRules =>
        [.. CsharpRules, .. SwiftRules, .. NeutralRules];

    [Fact]
    public void EveryRuleCarriesAWellFormedId()
    {
        foreach (var rule in AllRules)
        {
            Assert.True(
                Regex.IsMatch(rule.Id, @"^(R\d\d|S\d\d|NCL-\d\d)$"),
                $"{rule.Name} carries the id '{rule.Id}', which is not a recognised form");
        }
    }

    [Fact]
    public void NoIdIsClaimedByTwoRulesUnlessTheSpecSaysSo()
    {
        var shared = AllRules
            .GroupBy(rule => rule.Id)
            .Where(group => group.Count() > 1)
            .Where(group => !IdsSharedBySeveralRules.Contains(group.Key))
            .Select(group => $"{group.Key}: {string.Join(", ", group.Select(r => r.Name))}")
            .ToArray();

        Assert.Empty(shared);
    }

    /// <summary>Every id the code emits has a row in the spec - no invented identifiers.</summary>
    [Theory]
    [InlineData("Csharp")]
    [InlineData("Swift")]
    public void EveryRuleIdAppearsInItsSpecTable(string language)
    {
        var rules = language == "Csharp" ? CsharpRules : SwiftRules;
        var spec = ReadSpec(language == "Csharp" ? CsharpSpec : SwiftSpec);

        foreach (var rule in rules)
        {
            Assert.True(
                spec.Contains($"| {rule.Id} |", StringComparison.Ordinal),
                $"{rule.Name} claims {rule.Id}, which has no row in the spec table");
        }
    }

    /// <summary>
    /// And the reverse: every live row in the spec has a rule behind it. R07 and R14 are excluded
    /// because they are retired to the neutral existence rules and their ids are never reused.
    /// </summary>
    [Theory]
    [InlineData("Csharp")]
    [InlineData("Swift")]
    public void EveryLiveSpecRowHasARuleBehindIt(string language)
    {
        var rules = language == "Csharp" ? CsharpRules : SwiftRules;
        var claimed = rules.Select(rule => rule.Id).ToHashSet(StringComparer.Ordinal);
        var prefix = language == "Csharp" ? "R" : "S";
        var spec = ReadSpec(language == "Csharp" ? CsharpSpec : SwiftSpec);

        var missing = new List<string>();
        foreach (Match row in Regex.Matches(spec, $@"^\|\s*({prefix}\d\d)\s*\|", RegexOptions.Multiline))
        {
            var id = row.Groups[1].Value;
            if (claimed.Contains(id))
            {
                continue;
            }

            // A retired row documents itself as retired rather than being deleted, so that the
            // id is never reused (specs/07, §7).
            if (spec.Contains($"| ~~{id}~~ | *retired*", StringComparison.Ordinal))
            {
                continue;
            }

            missing.Add(id);
        }

        Assert.Empty(missing);
    }

    /// <summary>
    /// The count is asserted outright so that deleting a rule is a deliberate act. TST-01 claims
    /// 81 rule classes; if that number moves, the claim moves with it.
    /// </summary>
    [Fact]
    public void TheRuleSetIsTheSizeTheSpecsClaim()
    {
        Assert.Equal(41, CsharpRules.Count);
        Assert.Equal(38, SwiftRules.Count);
        Assert.Equal(2, NeutralRules.Count);
    }

    [Fact]
    public void TheComparatorFallbackIdIsNotARuleId()
    {
        Assert.DoesNotContain(ComparatorId, AllRules.Select(rule => rule.Id));
    }

    private static IReadOnlyList<(string Id, string Name)> GetRules<T>(Func<T, string> getId)
    where T : class
    {
        var rules = new List<(string, string)>();
        foreach (var type in typeof(IPackageableUnit).Assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface || !typeof(T).IsAssignableFrom(type))
            {
                continue;
            }

            var rule = (T)Activator.CreateInstance(type)!;
            rules.Add((getId(rule), type.Name));
        }

        return rules;
    }

    private static string ReadSpec(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, "specs", fileName);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Unable to locate specs/{fileName}");
    }
}
