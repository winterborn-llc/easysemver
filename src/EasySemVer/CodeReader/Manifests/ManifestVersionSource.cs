using System.Text.RegularExpressions;
using Winterborn.Tools.EasySemVer.Interfaces;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.CodeReader.Manifests;

/// <summary>
/// MVR-03 for the version-sync ecosystems: one literal version assignment in one manifest, matched
/// by a supplied pattern. package.json, Cargo.toml, pubspec.yaml and the rest differ only in that
/// pattern, so they differ only in one line of registration.
/// <para>
/// The file is edited **textually**, replacing just the matched span. SYN-04's DOM rewrite is right
/// for a .csproj, whose formatting MSBuild owns; it is hostile to a package.json, where reserialising
/// would reorder keys, restyle the whole file and produce a diff nobody asked for in a file a team
/// reads every day.
/// </para>
/// <para>
/// Only the **first** match is replaced. Every pattern here anchors to a top-level assignment, and
/// a manifest that mentions a version again further down is describing a dependency's version, not
/// its own - rewriting that would pin someone else's package to this repository's number.
/// </para>
/// </summary>
internal class ManifestVersionSource(
    string manifestPath,
    string relativePath,
    string kind,
    Regex assignment) : IVersionSource
{
    /// <summary>
    /// MVR-04 - a source exists only because the value it wraps already does, so the factory probes
    /// with this before constructing one. A manifest that computes its version, or omits it, is
    /// read-skipped and write-skipped rather than being given one.
    /// </summary>
    internal static bool HasLiteralVersion(string manifestText, Regex assignment)
    {
        return assignment.IsMatch(manifestText);
    }

    public string Kind => kind;

    public string Location { get; } = relativePath;

    public bool IsWritable => true;

    public Version? Read()
    {
        var match = assignment.Match(File.ReadAllText(manifestPath));
        if (!match.Success)
        {
            return null;
        }

        var text = match.Groups["version"].Value;
        if (Version.TryParse(text, out var version))
        {
            return version;
        }

        // MVR-03: unparseable is skipped with a warning, never fatal. These ecosystems are full of
        // values that are legitimately not three numbers - "1.0.0-beta.1", "^2.3", "1.2.3+build7" -
        // and a run must not die because someone's manifest is mid-prerelease.
        Log.WriteLine($"Skipping unparseable version '{text}' in {this.Location}");
        return null;
    }

    public void Write(Version version)
    {
        var text = File.ReadAllText(manifestPath);
        var match = assignment.Match(text);
        if (!match.Success)
        {
            return;
        }

        var group = match.Groups["version"];
        var updated = string.Concat(
            text.AsSpan(0, group.Index),
            version.ToString(),
            text.AsSpan(group.Index + group.Length));

        File.WriteAllText(manifestPath, updated);
    }
}
