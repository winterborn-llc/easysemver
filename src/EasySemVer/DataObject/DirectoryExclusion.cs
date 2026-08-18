namespace Winterborn.Tools.EasySemVer.DataObject;

/// <summary>
/// FLD-06 - one directory a language asks the walk to skip, and the evidence that it is really that
/// directory rather than one that merely shares its name.
/// <para>
/// The `Packages` post-mortem is the whole reason this carries a marker at all. That entry was
/// removed because the name alone did not identify the thing: `Packages/` had been SwiftPM's
/// dependency checkout in the Swift 3 era and is now where a modular Xcode app keeps its *own* local
/// packages, so excluding it by name silently swallowed first-party units. `vendor`, `target`,
/// `venv` and `blib` are all in exactly that position - each is build output or dependency source in
/// one ecosystem and a perfectly ordinary source directory in another.
/// </para>
/// <para>
/// A marker turns the guess into a fact. `vendor` beside a `go.mod` is Go's vendored dependency
/// tree; `vendor` beside nothing in particular is somebody's code.
/// </para>
/// </summary>
/// <param name="DirectoryName">The directory's name, matched case-insensitively.</param>
/// <param name="SiblingMarkers">
/// File names or patterns that must exist in the directory's <em>parent</em> for the exclusion to
/// apply. Empty means unconditional, which is reserved for names that cannot mean anything else -
/// `node_modules`, `__pycache__`.
/// </param>
public record DirectoryExclusion(string DirectoryName, IReadOnlyList<string> SiblingMarkers)
{
    /// <summary>A name that identifies itself, needing no corroboration.</summary>
    public static DirectoryExclusion Always(string directoryName)
    {
        return new DirectoryExclusion(directoryName, []);
    }

    /// <summary>A name that only means what it says when one of these sits beside it.</summary>
    public static DirectoryExclusion Beside(string directoryName, params string[] siblingMarkers)
    {
        return new DirectoryExclusion(directoryName, siblingMarkers);
    }

    /// <summary>
    /// Whether this exclusion applies to <paramref name="directory"/>. The marker is looked for in
    /// the parent, because the marker identifies the *package* the directory belongs to: a go.mod
    /// sits beside `vendor`, not inside it.
    /// </summary>
    public bool Matches(DirectoryInfo directory)
    {
        if (!string.Equals(directory.Name, this.DirectoryName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (this.SiblingMarkers.Count < 1)
        {
            return true;
        }

        var parent = directory.Parent;
        if (parent == null || !parent.Exists)
        {
            return false;
        }

        foreach (var marker in this.SiblingMarkers)
        {
            if (parent.GetFiles(marker).Length > 0)
            {
                return true;
            }
        }

        return false;
    }
}
