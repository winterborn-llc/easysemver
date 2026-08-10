using Winterborn.Tools.EasySemVer.CodeReader.Swift;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Test.Swift;

/// <summary>
/// UNI-04, asserted on the command line rather than on the clock. The extractor used to pass
/// `--build-tests`, because UNI-03 makes a test target a unit and SWE-05 fails a unit with no
/// graph. UNI-04 removed the reason - a test target's API surface is never read - but the flag
/// stayed, and it was the single most expensive thing the tool did: it compiled and linked an
/// XCTest bundle on every run to produce a graph that classification, the baseline and the report
/// all ignore. On the fixture package it was the difference between about 1.3 seconds and 11 to 20.
/// <para>
/// A cost regression is invisible to an assertion about behaviour, which is why this pins the
/// arguments: re-adding the flag would leave every other test passing and merely slow.
/// </para>
/// </summary>
public class TestSwiftBuildCommand
{
    private class RecordingProcessRunner : IRunProcess
    {
        internal List<IReadOnlyList<string>> Invocations { get; } = [];

        public ProcessResult Run(
            string executable,
            IReadOnlyList<string> arguments,
            string workingDirectory,
            TimeSpan timeout)
        {
            this.Invocations.Add(arguments);
            return new ProcessResult { CommandLine = $"{executable} {string.Join(' ', arguments)}" };
        }
    }

    private static IReadOnlyList<string> GetBuildArguments()
    {
        var runner = new RecordingProcessRunner();

        // The stub writes no graphs, so this reads an empty directory and returns nothing. The
        // command is the subject here; SWE-05's reaction to a missing module is TestSwiftExtractionFailure's.
        new SwiftSymbolGraphExtractor(runner).ExtractPackage("/packages/Widgets", "Widgets");

        return Assert.Single(runner.Invocations);
    }

    [Fact]
    public void TheBuildDoesNotCompileTestTargets()
    {
        Assert.DoesNotContain("--build-tests", GetBuildArguments());
    }

    /// <summary>
    /// The flags that make the build worth running at all. Dropping `--build-tests` must not have
    /// taken the symbol graph with it.
    /// </summary>
    [Fact]
    public void TheBuildStillAsksForPublicSymbolGraphs()
    {
        var arguments = GetBuildArguments();

        Assert.Equal("build", arguments[0]);
        Assert.Contains("-emit-symbol-graph", arguments);
        Assert.Contains("-emit-extension-block-symbols", arguments);
        Assert.Contains("-symbol-graph-minimum-access-level", arguments);
        Assert.Contains("public", arguments);
        Assert.Contains("/packages/Widgets", arguments);
    }
}
