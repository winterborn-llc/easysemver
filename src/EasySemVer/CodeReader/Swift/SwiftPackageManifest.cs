using System.Text.Json;
using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.CodeReader.Swift;

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

    internal static IReadOnlyList<string> GetTargetNames(IRunProcess runProcess, string packageDirectory)
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

        return ReadTargetNames(result.StandardOutput);
    }

    internal static IReadOnlyList<string> ReadTargetNames(string manifestJson)
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
