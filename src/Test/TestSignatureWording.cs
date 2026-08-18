using Winterborn.Tools.EasySemVer.CodeReader.Csharp;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Extensions;

namespace Test;

/// <summary>
/// BAS-07's missing guard, added because its absence cost a major version (G-26).
/// <para>
/// A provider may change its <em>model</em> freely — a new field on a modelled entity diffs as an
/// ordinary change and should. What it may not do silently is change the <em>words</em> it uses to
/// describe the same API, because every consumer's baseline then reads as changed and the run cuts a
/// release nobody earned. BAS-07 says such a change comes with a `SignatureVersion` bump; nothing
/// enforced it, and a one-line fix to how generic type names were rendered went out without one.
/// </para>
/// <para>
/// This is a golden test, and its expected file is generated rather than hand-written. That is the
/// point: it does not assert the wording is *right*, it asserts the wording did not change without
/// somebody noticing. When it fails, there are exactly two correct responses — restore the wording,
/// or bump <c>CsharpLanguageProvider.SignatureVersion</c> and regenerate the file in the same commit.
/// </para>
/// </summary>
public class TestSignatureWording : IDisposable
{
    /// <summary>
    /// Deliberately dense with the constructs whose *rendering* has moved before: generics nested
    /// in generics (G-26), arrays, nullable value types, tuples, and a base class that is itself
    /// generic. A plain class with a string property would pass this test forever and catch nothing.
    /// </summary>
    private const string Source = """
        using System;
        using System.Collections.Generic;

        namespace Widgets;

        public interface IGadget<T> where T : class
        {
            IReadOnlyList<T> Items { get; }
        }

        public class Registry : List<Dictionary<string, IReadOnlyList<int>>>
        {
            public int?[] Counts { get; set; } = [];

            public (string Name, int Count) Summary { get; set; }

            public Dictionary<string, List<Widget>> ByName { get; set; } = new();

            public void Register<TItem>(TItem item, params ReadOnlySpan<string> tags) { }
        }

        public class Widget
        {
            public string Name { get; set; } = "";
        }
        """;

    private readonly string _folderRoot =
        Directory.CreateTempSubdirectory("easysemver-wording").FullName;

    public void Dispose()
    {
        Directory.Delete(this._folderRoot, recursive: true);
        GC.SuppressFinalize(this);
    }

    private static string ExpectedFilePath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, "Test", "ExpectedSignature.xml");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Unable to locate src/Test/ExpectedSignature.xml");
    }

    [Fact]
    public void TheWordsCsharpUsesToDescribeAnApiHaveNotChanged()
    {
        var projectPath = Path.Combine(this._folderRoot, "Widgets.csproj");
        File.WriteAllText(projectPath, "<Project Sdk=\"Microsoft.NET.Sdk\" />");
        File.WriteAllText(Path.Combine(this._folderRoot, "Widgets.cs"), Source);

        var project = CsharpUnitBuilder.GetProjectSignature(projectPath);
        project.SortForPersistence();
        var actual = project.SerializeToElement().ToString().ReplaceLineEndings("\n");

        var expected = File.ReadAllText(ExpectedFilePath()).ReplaceLineEndings("\n").TrimEnd();

        Assert.True(
            expected == actual.TrimEnd(),
            "The C# signature wording changed.\n\n"
            + "This is not necessarily a bug - but if it is intentional, it is a BAS-07 event: every\n"
            + "consumer's baseline now describes the same API in different words, so the next run\n"
            + "reads every affected entity as changed and cuts a release nobody earned. That is what\n"
            + "G-26 cost, in a major version.\n\n"
            + "Two correct responses, and no third:\n"
            + "  1. Restore the wording, if the change was accidental.\n"
            + "  2. Bump CsharpLanguageProvider.SignatureVersion and regenerate\n"
            + "     src/Test/ExpectedSignature.xml in the same commit.\n\n"
            + $"Expected:\n{expected}\n\nActual:\n{actual.TrimEnd()}");
    }
}
