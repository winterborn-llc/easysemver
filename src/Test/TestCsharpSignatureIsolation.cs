using Winterborn.Library.EasySemVer.CodeReader.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test;

/// <summary>
/// SIG-03. A unit's signature is what the unit's own source declares, and nothing else. The
/// compilation is given framework references drawn from the runtime this tool happens to be
/// executing on, so any symbol reached through metadata would make the baseline a property of the
/// machine that wrote it rather than of the source. These tests assert the outcome rather than the
/// mechanism, so they still hold whichever way the symbol walk is narrowed.
/// </summary>
public class TestCsharpSignatureIsolation : IDisposable
{
    /// <summary>
    /// Deliberately leans on the three referenced assemblies - object, string, Console and LINQ -
    /// so every one of them is loaded and reachable while the walk runs.
    /// </summary>
    private const string Source = """
        using System;
        using System.Linq;
        using System.Collections.Generic;

        namespace Widgets;

        public class Reporter
        {
            public object Anything { get; set; } = new object();

            public string Label { get; set; } = string.Empty;

            public void Emit(string line) => Console.WriteLine(line);

            public IEnumerable<int> Evens(IEnumerable<int> source) => source.Where(n => n % 2 == 0);
        }
        """;

    private const string PartOne = """
        namespace Widgets;

        public partial class Split
        {
            public int First() => 1;
        }
        """;

    private const string PartTwo = """
        namespace Widgets;

        public partial class Split
        {
            public int Second() => 2;
        }
        """;

    private readonly string _folderRoot =
        Directory.CreateTempSubdirectory("easysemver-isolation").FullName;

    private readonly ICsharpProject _project;

    public TestCsharpSignatureIsolation()
    {
        var projectPath = Path.Combine(this._folderRoot, "Widgets.csproj");
        File.WriteAllText(projectPath, "<Project Sdk=\"Microsoft.NET.Sdk\" />");
        File.WriteAllText(Path.Combine(this._folderRoot, "Widgets.cs"), Source);
        File.WriteAllText(Path.Combine(this._folderRoot, "SplitOne.cs"), PartOne);
        File.WriteAllText(Path.Combine(this._folderRoot, "SplitTwo.cs"), PartTwo);
        this._project = CsharpUnitBuilder.GetProjectSignature(projectPath);
    }

    public void Dispose()
    {
        Directory.Delete(this._folderRoot, recursive: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// The regression this guards: a name-prefix denylist let keyword-aliased framework types
    /// through, because Roslyn renders System.Object as "object" and System.String as "string",
    /// neither of which starts with "System.". Recorded as the unit's own types, they carried the
    /// whole member surface of whichever CoreLib the tool ran against.
    /// </summary>
    [Fact]
    public void KeywordAliasedFrameworkTypesAreNotRecorded()
    {
        var names = this._project.Types.Select(t => t.Name).ToList();

        Assert.DoesNotContain("object", names);
        Assert.DoesNotContain("string", names);
        Assert.DoesNotContain("int", names);
        Assert.DoesNotContain("void", names);
    }

    /// <summary>
    /// The other half of the same regression: a denylist can only exclude namespaces somebody
    /// thought to list. Internal.Console is public, lives in System.Private.CoreLib, and is on
    /// nobody's list.
    /// </summary>
    [Fact]
    public void FrameworkTypesOutsideTheKnownNamespacesAreNotRecorded()
    {
        var names = this._project.Types.Select(t => t.Name).ToList();

        Assert.DoesNotContain("Internal.Console", names);
        Assert.DoesNotContain("Internal.Console.Error", names);
    }

    /// <summary>
    /// The general statement of SIG-03, and the one that survives a change of runtime: everything
    /// recorded is declared by a file this unit owns.
    /// </summary>
    [Fact]
    public void EveryRecordedTypeIsDeclaredByThisUnitsOwnSource()
    {
        var declared = new[]
        {
            "Widgets.Reporter",
            "Widgets.Split"
        };

        Assert.Equal(declared, this._project.Types.Select(t => t.Name).Order());
    }

    /// <summary>
    /// A partial type has one declaring syntax reference per part, so an "is it declared in
    /// source" test must not mistake the extra parts for extra types. It is one type, and it
    /// carries the members of every part.
    /// </summary>
    [Fact]
    public void APartialTypeIsRecordedOnceCarryingEveryPart()
    {
        var split = Assert.Single(this._project.Types, t => t.Name == "Widgets.Split");

        Assert.Contains("First", split.Methods.Select(m => m.MethodName));
        Assert.Contains("Second", split.Methods.Select(m => m.MethodName));
    }
}
