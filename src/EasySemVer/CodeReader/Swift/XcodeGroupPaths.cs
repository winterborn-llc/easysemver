namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// Turns a file reference into a path on disk. An Xcode project stores a file's location as a
/// fragment plus a "sourceTree" saying what it is relative to, and for the common case - a group -
/// that means walking up the group hierarchy collecting the fragments each one contributes.
/// </summary>
internal class XcodeGroupPaths
{
    private const string GroupRelative = "<group>";
    private const string AbsolutePath = "<absolute>";
    private const string SourceRoot = "SOURCE_ROOT";
    private const string ProjectRoot = "<project>";

    /// <summary>
    /// Source trees that point outside the project's own source: build output, the SDK, the
    /// toolchain. Nothing under them is a file this project declares.
    /// </summary>
    private static readonly string[] ForeignSourceTrees =
        ["BUILT_PRODUCTS_DIR", "SDKROOT", "DEVELOPER_DIR"];

    private readonly PbxprojObjects _objects;

    private readonly string _projectDirectory;

    private readonly Dictionary<string, string> _parents = new(StringComparer.Ordinal);

    internal XcodeGroupPaths(PbxprojObjects objects, string projectDirectory)
    {
        this._objects = objects;
        this._projectDirectory = projectDirectory;

        foreach (var isa in (string[])[PbxprojObjects.Group, PbxprojObjects.VariantGroup])
        {
            this.MapChildren(isa);
        }
    }

    /// <summary>The path a file reference or a group resolves to, or nothing if it is not local.</summary>
    internal string Resolve(string identifier)
    {
        return this.Resolve(identifier, depth: 0);
    }

    /// <summary>
    /// The depth guard is for a project file that has been hand-edited into a cycle. Xcode does
    /// not write one, and a versioning run that hangs would be worse than one that gives up.
    /// </summary>
    private string Resolve(string identifier, int depth)
    {
        var fields = this._objects.Find(identifier);
        if (fields == null || depth > 64)
        {
            return string.Empty;
        }

        var sourceTree = PbxprojObjects.GetString(fields, "sourceTree");
        if (ForeignSourceTrees.Contains(sourceTree))
        {
            return string.Empty;
        }

        var path = PbxprojObjects.GetString(fields, "path");
        if (sourceTree == AbsolutePath)
        {
            return path;
        }

        if (sourceTree is SourceRoot or ProjectRoot || sourceTree.Length < 1)
        {
            return Combine(this._projectDirectory, path);
        }

        return Combine(this.ResolveParent(identifier, depth), path);
    }

    /// <summary>
    /// Walks up to the containing group, which may itself be group-relative. A group with no
    /// parent is the project's main group, and that sits at the project directory.
    /// </summary>
    private string ResolveParent(string identifier, int depth)
    {
        return this._parents.TryGetValue(identifier, out var parent)
            ? this.Resolve(parent, depth + 1)
            : this._projectDirectory;
    }

    private void MapChildren(string isa)
    {
        foreach (var group in this._objects.OfKind(isa))
        {
            foreach (var child in PbxprojObjects.GetIdentifiers(group.Fields, "children"))
            {
                this._parents[child] = group.Identifier;
            }
        }
    }

    private static string Combine(string directory, string path)
    {
        if (directory.Length < 1)
        {
            return string.Empty;
        }

        return path.Length < 1 ? directory : Path.Combine(directory, path);
    }
}
