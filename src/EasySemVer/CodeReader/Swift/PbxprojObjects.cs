namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// The "objects" table of a project.pbxproj, which is where everything in an Xcode project
/// actually lives: every target, group, file reference and build phase is one entry in it, joined
/// to the others by identifier.
/// </summary>
internal class PbxprojObjects
{
    internal const string NativeTarget = "PBXNativeTarget";
    internal const string SourcesBuildPhase = "PBXSourcesBuildPhase";
    internal const string BuildFile = "PBXBuildFile";
    internal const string FileReference = "PBXFileReference";
    internal const string Group = "PBXGroup";
    internal const string VariantGroup = "PBXVariantGroup";
    internal const string SynchronizedRootGroup = "PBXFileSystemSynchronizedRootGroup";

    private const string IsaKey = "isa";

    private readonly Dictionary<string, Dictionary<string, object>> _objects;

    private PbxprojObjects(Dictionary<string, Dictionary<string, object>> objects)
    {
        this._objects = objects;
    }

    /// <summary>
    /// An unreadable or missing project file yields nothing rather than throwing. Discovery has to
    /// decide what to do about a project it cannot read, and it can say so better than this can.
    /// </summary>
    internal static PbxprojObjects? Load(string pbxprojPath)
    {
        if (!File.Exists(pbxprojPath))
        {
            return null;
        }

        try
        {
            return Read(File.ReadAllText(pbxprojPath));
        }
        catch (Exception e) when (e is IOException or InvalidDataException)
        {
            Log.WriteLine($"Could not read {pbxprojPath}: {e.Message}");
            return null;
        }
    }

    internal static PbxprojObjects Read(string text)
    {
        var objects = new Dictionary<string, Dictionary<string, object>>(StringComparer.Ordinal);
        if (PbxprojParser.Parse(text).TryGetValue("objects", out var table)
            && table is Dictionary<string, object> entries)
        {
            foreach (var entry in entries)
            {
                if (entry.Value is Dictionary<string, object> fields)
                {
                    objects[entry.Key] = fields;
                }
            }
        }

        return new PbxprojObjects(objects);
    }

    /// <summary>Every object of one isa, in identifier order so that discovery does not vary.</summary>
    internal IEnumerable<(string Identifier, Dictionary<string, object> Fields)> OfKind(string isa)
    {
        foreach (var identifier in this._objects.Keys.Order(StringComparer.Ordinal))
        {
            var fields = this._objects[identifier];
            if (GetString(fields, IsaKey) == isa)
            {
                yield return (identifier, fields);
            }
        }
    }

    internal Dictionary<string, object>? Find(string identifier)
    {
        return this._objects.GetValueOrDefault(identifier);
    }

    /// <summary>The object an identifier names, if it is of the isa expected.</summary>
    internal Dictionary<string, object>? Find(string identifier, string isa)
    {
        var fields = this.Find(identifier);
        return fields != null && GetString(fields, IsaKey) == isa ? fields : null;
    }

    internal static string GetString(Dictionary<string, object> fields, string key)
    {
        return fields.TryGetValue(key, out var value) && value is string text ? text : string.Empty;
    }

    internal static IReadOnlyList<string> GetIdentifiers(
        Dictionary<string, object> fields,
        string key)
    {
        if (!fields.TryGetValue(key, out var value) || value is not List<object> entries)
        {
            return [];
        }

        var identifiers = new List<string>();
        foreach (var entry in entries)
        {
            if (entry is string identifier && identifier.Length > 0)
            {
                identifiers.Add(identifier);
            }
        }

        return identifiers;
    }
}
