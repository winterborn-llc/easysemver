using Winterborn.Tools.EasySemVer.Evaluation;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// SWD-02 - an .xcodeproj's targets and their sources, read from project.pbxproj.
/// <para>
/// The target list used to come from <c>xcodebuild -list -json</c>. That resolves the project's
/// Swift package dependencies before it will print anything, so a versioning run needed Xcode, a
/// network and credentials for every private package - to be told a set of names that are written
/// in the project file. It also reported names and nothing else, which is why the product types
/// had to be read from the file anyway (UNI-04). Reading all of it from one place is both cheaper
/// and less to keep in step.
/// </para>
/// </summary>
internal static class XcodeProject
{
    private const string ProjectFileName = "project.pbxproj";

    internal static string GetProjectFilePath(string projectPath)
    {
        return Path.Combine(projectPath, ProjectFileName);
    }

    internal static IReadOnlyList<XcodeTarget> Read(string projectPath)
    {
        var objects = PbxprojObjects.Load(GetProjectFilePath(projectPath));
        if (objects == null)
        {
            Log.WriteLine(
                $"No readable {ProjectFileName} in {Path.GetFileName(projectPath)}; it contributes "
                + "no units.");
            return [];
        }

        // The project bundle sits beside the source it refers to, and every group-relative path
        // in it is relative to that directory.
        return Read(objects, Path.GetDirectoryName(projectPath) ?? projectPath);
    }

    /// <summary>Exposed so the tests can read a project file that is text rather than a directory.</summary>
    internal static IReadOnlyList<XcodeTarget> Read(
        PbxprojObjects objects,
        string projectDirectory)
    {
        var paths = new XcodeGroupPaths(objects, projectDirectory);
        var targets = new List<XcodeTarget>();
        foreach (var target in objects.OfKind(PbxprojObjects.NativeTarget))
        {
            var name = PbxprojObjects.GetString(target.Fields, "name");
            if (name.Length < 1)
            {
                continue;
            }

            targets.Add(new XcodeTarget
            {
                Name = name,
                ProductType = PbxprojObjects.GetString(target.Fields, "productType"),
                SourceFiles = ReadSourceFiles(objects, paths, target.Fields)
            });
        }

        targets.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
        return targets;
    }

    /// <summary>
    /// The Swift files that compile into one target: the ones its Sources build phase lists, plus
    /// everything under any folder it synchronises.
    /// </summary>
    private static IReadOnlyList<string> ReadSourceFiles(
        PbxprojObjects objects,
        XcodeGroupPaths paths,
        Dictionary<string, object> target)
    {
        var files = new List<string>();
        ReadBuildPhaseFiles(objects, paths, target, files);
        ReadSynchronisedFolders(objects, paths, target, files);

        files.Sort(StringComparer.Ordinal);
        return files.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static void ReadBuildPhaseFiles(
        PbxprojObjects objects,
        XcodeGroupPaths paths,
        Dictionary<string, object> target,
        List<string> files)
    {
        foreach (var phaseId in PbxprojObjects.GetIdentifiers(target, "buildPhases"))
        {
            var phase = objects.Find(phaseId, PbxprojObjects.SourcesBuildPhase);
            if (phase == null)
            {
                continue;
            }

            foreach (var buildFileId in PbxprojObjects.GetIdentifiers(phase, "files"))
            {
                var buildFile = objects.Find(buildFileId, PbxprojObjects.BuildFile);
                if (buildFile == null)
                {
                    continue;
                }

                AddIfSwift(paths.Resolve(PbxprojObjects.GetString(buildFile, "fileRef")), files);
            }
        }
    }

    /// <summary>
    /// Xcode 16's synchronised folders have no per-file entries at all: the target names a
    /// directory and takes everything in it. Membership exceptions are not read, so a file
    /// excluded from the target is still read as part of it - which over-reports the surface
    /// rather than missing some of it.
    /// </summary>
    private static void ReadSynchronisedFolders(
        PbxprojObjects objects,
        XcodeGroupPaths paths,
        Dictionary<string, object> target,
        List<string> files)
    {
        foreach (var groupId in PbxprojObjects.GetIdentifiers(target, "fileSystemSynchronizedGroups"))
        {
            if (objects.Find(groupId, PbxprojObjects.SynchronizedRootGroup) == null)
            {
                continue;
            }

            var directory = paths.Resolve(groupId);
            if (directory.Length < 1 || !Directory.Exists(directory))
            {
                continue;
            }

            files.AddRange(FolderScanner.FindFiles(directory, "*.swift"));
        }
    }

    private static void AddIfSwift(string path, List<string> files)
    {
        if (path.Length < 1 || !path.EndsWith(".swift", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!File.Exists(path))
        {
            return;
        }

        files.Add(path);
    }
}
