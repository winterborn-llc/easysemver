using System.Text.Json;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// SWD-01 - a Package.swift is executable Swift, so its targets are read by asking the toolchain
/// to dump the manifest as JSON rather than by parsing the file.
/// </summary>
internal static class SwiftPackageManifest
{
    /// <summary>
    /// Generous on purpose: dumping a manifest compiles it, and a loaded build machine can take
    /// far longer over that than a developer's laptop does.
    /// </summary>
    internal static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

    /// <summary>Target kinds that are not first-party source and therefore not units (SWD-04).</summary>
    private static readonly string[] NonSourceTargetTypes = ["system", "binary", "plugin", "macro"];

    /// <summary>
    /// UNI-04 - what SwiftPM calls a target declared with <c>.testTarget</c>. It is a unit like
    /// any other and keeps its versions; what it does not keep is a vote on the folder's API.
    /// </summary>
    private const string TestTargetType = "test";

    /// <summary>
    /// The manifest as JSON, run once per package. Exposed rather than folded into
    /// <see cref="ReadTargetNames"/> because the caller wants two answers out of it - the units and
    /// which of them are tests (UNI-04) - and dumping a manifest compiles it, so asking twice would
    /// be a second compile for a question already answered.
    /// </summary>
    internal static string Dump(IRunProcess runProcess, string packageDirectory)
    {
        var result = runProcess.Run(
            "swift",
            ["package", "dump-package"],
            packageDirectory,
            Timeout);

        if (!result.IsSuccess)
        {
            throw new SwiftToolchainException(packageDirectory, result);
        }

        return result.StandardOutput;
    }

    internal static IReadOnlyList<string> ReadTargetNames(string manifestJson)
    {
        return ReadNames(manifestJson, testTargetsOnly: false);
    }

    /// <summary>
    /// UNI-04 - the subset of <see cref="ReadTargetNames"/> that SwiftPM says are test targets.
    /// Read from the same dump rather than by matching names: a target called <c>WidgetsTests</c>
    /// that is a fixture library, and one called <c>Scenarios</c> that is a test target, are both
    /// ordinary, and the manifest already states which is which.
    /// </summary>
    internal static IReadOnlyList<string> ReadTestTargetNames(string manifestJson)
    {
        return ReadNames(manifestJson, testTargetsOnly: true);
    }

    private static IReadOnlyList<string> ReadNames(string manifestJson, bool testTargetsOnly)
    {
        using var document = JsonDocument.Parse(manifestJson);
        if (!document.RootElement.TryGetProperty("targets", out var targets))
        {
            return [];
        }

        var names = new List<string>();
        foreach (var target in targets.EnumerateArray())
        {
            var type = target.GetStringOrEmpty("type");
            if (NonSourceTargetTypes.Contains(type))
            {
                continue;
            }

            if (testTargetsOnly && type != TestTargetType)
            {
                continue;
            }

            var name = target.GetStringOrEmpty("name");
            if (name.Length < 1)
            {
                continue;
            }

            names.Add(name);
        }

        names.Sort(StringComparer.Ordinal);
        return names;
    }
}
