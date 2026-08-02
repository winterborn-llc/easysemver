using System.Text.Json;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.CodeReader.Swift;

/// <summary>
/// SWD-02 - an .xcodeproj's targets, read by asking xcodebuild rather than by parsing the
/// project file. A .xcworkspace is only ever used to locate projects, never as a unit itself.
/// </summary>
internal static class XcodeProject
{
    internal static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

    internal static IReadOnlyList<string> GetTargetNames(IRunProcess runProcess, string projectPath)
    {
        var result = runProcess.Run(
            "xcodebuild",
            ["-list", "-json", "-project", projectPath],
            Path.GetDirectoryName(projectPath) ?? Environment.CurrentDirectory,
            Timeout);

        if (!result.IsSuccess)
        {
            throw new SwiftToolchainException(projectPath, result);
        }

        return ReadTargetNames(result.StandardOutput);
    }

    internal static IReadOnlyList<string> ReadTargetNames(string listJson)
    {
        using var document = JsonDocument.Parse(listJson);
        if (!document.RootElement.TryGetProperty("project", out var project))
        {
            return [];
        }

        if (!project.TryGetProperty("targets", out var targets))
        {
            return [];
        }

        var names = new List<string>();
        foreach (var target in targets.EnumerateArray())
        {
            var name = target.GetString();
            if (name == null || name.Length < 1)
            {
                continue;
            }

            names.Add(name);
        }

        names.Sort(StringComparer.Ordinal);
        return names;
    }
}
