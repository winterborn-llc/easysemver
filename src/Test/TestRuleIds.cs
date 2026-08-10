using System.Text.RegularExpressions;
using Winterborn.Tools.EasySemVer.Evaluators;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

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
    /// CLS-04's fail-safe is raised by the comparator, not by a rule, so it has no row of its own
    /// in the rule tables.
    /// </summary>
    private const string ComparatorRule = "NoComparableBaseline";

    private static IReadOnlyList<(string Id, string Name)> CsharpRules =>
        [.. GetRules<IEvaluateCsharpSignatures>(r => r.Rule), .. ExistenceRulesFor("Csharp")];

    private static IReadOnlyList<(string Id, string Name)> SwiftRules =>
        [.. GetRules<IEvaluateSwiftSignatures>(r => r.Rule), .. ExistenceRulesFor("Swift")];

    private static IReadOnlyList<(string Id, string Name)> AllRules =>
        [.. CsharpRules, .. SwiftRules];

    /// <summary>
    /// A rule's language is the folder it lives in, which for the signature rules is already
    /// implied by the interface they implement. The existence rules implement one interface across
    /// every language, so their namespace is what says whose they are - and that is not a
    /// convention this test invented, it is the same folder-per-language rule the whole seam runs
    /// on.
    /// </summary>
    private static IReadOnlyList<(string Id, string Name)> ExistenceRulesFor(string folder)
    {
        return GetRules<IEvaluateUnitExistence>(r => r.Rule, type =>
            type.Namespace?.EndsWith($".Evaluators.{folder}", StringComparison.Ordinal) == true);
    }

    /// <summary>
    /// A rule is named, not numbered. The old R-and-S numbering is kept in the spec tables' "Was"
    /// column so a consumer holding an old report can translate, but nothing in the code carries
    /// one any more - and this is what stops one creeping back in.
    /// </summary>
    [Fact]
    public void EveryRuleCarriesAWellFormedName()
    {
        foreach (var rule in AllRules)
        {
            Assert.True(
                Regex.IsMatch(rule.Id, @"^[A-Z][A-Za-z]+$"),
                $"{rule.Name} carries the name '{rule.Id}', which is not a recognised form");
        }
    }

    /// <summary>
    /// The name a rule publishes is its own class name. That is not required - the point of
    /// carrying it as a literal is that the two can diverge without breaking a consumer - but
    /// while they do agree, a mismatch is far more likely to be a copy-paste than a decision.
    /// </summary>
    [Fact]
    public void EveryRuleIsNamedAfterItsClass()
    {
        foreach (var rule in AllRules)
        {
            Assert.Equal(rule.Name, rule.Id);
        }
    }

    /// <summary>
    /// A rule is identified by (language, rule), so a name only has to be unique within its own
    /// language. That is what lets every language have a <c>UnitRemoved</c> of its own, and it is
    /// why this groups per language rather than globally as it used to.
    /// </summary>
    [Theory]
    [InlineData("Csharp")]
    [InlineData("Swift")]
    public void NoNameIsClaimedByTwoRulesOfOneLanguage(string language)
    {
        var shared = RulesFor(language)
            .GroupBy(rule => rule.Id)
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key}: {string.Join(", ", group.Select(r => r.Name))}")
            .ToArray();

        Assert.Empty(shared);
    }

    /// <summary>
    /// The other half of "carried, not derived": a rule that inherited its name from a base class
    /// would have the base decide its published key, and a base that defaulted the key from the
    /// type name would put a class rename back on the wire. Bases exist to share the diffing, and
    /// nothing else.
    /// </summary>
    [Fact]
    public void EveryRuleDeclaresItsOwnName()
    {
        var inherited = new List<string>();
        foreach (var type in RuleTypes<IEvaluateUnitExistence>(_ => true))
        {
            var declared = type.GetProperty(nameof(IEvaluateUnitExistence.Rule));
            if (declared?.DeclaringType == type)
            {
                continue;
            }

            inherited.Add(type.FullName ?? type.Name);
        }

        Assert.Empty(inherited);
    }

    /// <summary>Every id the code emits has a row in the spec - no invented identifiers.</summary>
    [Theory]
    [InlineData("Csharp")]
    [InlineData("Swift")]
    public void EveryRuleIdAppearsInItsSpecTable(string language)
    {
        var rules = RulesFor(language);
        var spec = ReadSpec(SpecFor(language));

        foreach (var rule in rules)
        {
            Assert.True(
                spec.Contains($"| {rule.Id} |", StringComparison.Ordinal),
                $"{rule.Name} claims {rule.Id}, which has no row in the spec table");
        }
    }

    /// <summary>
    /// And the reverse: every live row in the spec has a rule behind it. The tables are keyed on
    /// the rule's name now, with the number it used to carry kept beside it in the "Was" column,
    /// so this reads the first cell rather than hunting for an id anywhere on the line.
    /// <para>
    /// A retired row keeps its old id struck through and no name at all, which is how R07 and R14
    /// stay on the page - re-homed to each language's own UnitRemoved and UnitAdded - without
    /// looking like live rules here.
    /// </para>
    /// </summary>
    [Theory]
    [InlineData("Csharp")]
    [InlineData("Swift")]
    public void EveryLiveSpecRowHasARuleBehindIt(string language)
    {
        var claimed = RulesFor(language).Select(rule => rule.Id).ToHashSet(StringComparer.Ordinal);
        var spec = ReadSpec(SpecFor(language));

        var missing = new List<string>();
        foreach (Match row in Regex.Matches(
                     spec, @"^\|\s*([A-Z][A-Za-z]+)\s*\|\s*(?:[RS]\d\d|—|-)\s*\|",
                     RegexOptions.Multiline))
        {
            var name = row.Groups[1].Value;
            if (claimed.Contains(name))
            {
                continue;
            }

            missing.Add(name);
        }

        Assert.NotEmpty(claimed);
        Assert.Empty(missing);
    }

    /// <summary>
    /// The count is asserted outright so that deleting a rule is a deliberate act. Each language's
    /// total now includes the two unit-existence rules it owns, which is the shape the split into
    /// per-language rules was for: there is no separate neutral set left to count.
    /// </summary>
    [Fact]
    public void TheRuleSetIsTheSizeTheSpecsClaim()
    {
        Assert.Equal(43, CsharpRules.Count);
        Assert.Equal(40, SwiftRules.Count);
    }

    [Fact]
    public void TheComparatorFallbackIsNotARuleOfItsOwn()
    {
        Assert.DoesNotContain(ComparatorRule, AllRules.Select(rule => rule.Id));
    }

    private static IReadOnlyList<(string Id, string Name)> RulesFor(string language)
    {
        return language == "Csharp" ? CsharpRules : SwiftRules;
    }

    private static string SpecFor(string language)
    {
        return language == "Csharp" ? CsharpSpec : SwiftSpec;
    }

    private static IReadOnlyList<(string Id, string Name)> GetRules<T>(
        Func<T, string> getId,
        Func<Type, bool>? include = null)
    where T : class
    {
        var rules = new List<(string, string)>();
        foreach (var type in RuleTypes<T>(include ?? (_ => true)))
        {
            var rule = (T)Activator.CreateInstance(type)!;
            rules.Add((getId(rule), type.Name));
        }

        return rules;
    }

    /// <summary>
    /// Abstract types are skipped, which is what makes a shared rule base safe: a base that was
    /// concrete would enter the rule set as a rule of its own and quietly move every count here.
    /// </summary>
    private static IEnumerable<Type> RuleTypes<T>(Func<Type, bool> include)
    where T : class
    {
        foreach (var type in typeof(IPackageableUnit).Assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface || !typeof(T).IsAssignableFrom(type))
            {
                continue;
            }

            if (!include(type))
            {
                continue;
            }

            yield return type;
        }
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
