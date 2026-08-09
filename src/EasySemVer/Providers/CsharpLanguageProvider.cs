using System.Xml.Linq;
using Winterborn.Tools.EasySemVer.CodeReader.Csharp;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Extensions;
using Winterborn.Tools.EasySemVer.Interfaces;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>Everything C# contributes to a run (ML-02). One .csproj is one unit (UNI-02).</summary>
internal class CsharpLanguageProvider : ILanguageProvider
{
    internal const string CsprojUnitKind = "csproj";

    private string _folderRoot = string.Empty;

    public Language Language => Language.Csharp;

    public IReadOnlyList<IPackageableUnit> Discover(string folderRoot)
    {
        this._folderRoot = folderRoot;
        var units = new List<IPackageableUnit>();
        foreach (var projectFilePath in FolderScanner.FindFiles(folderRoot, "*.csproj"))
        {
            var relativePath = FolderScanner.GetRelativePath(folderRoot, projectFilePath);
            units.Add(new PackageableUnit
            {
                Language = Language.Csharp,

                // DSC-05: identity is the filename without extension, so a rename reads as
                // remove + add and a move within the tree does not.
                UnitId = Path.GetFileNameWithoutExtension(projectFilePath),
                DisplayName = Path.GetFileName(projectFilePath),
                RelativePath = relativePath,
                UnitKind = CsprojUnitKind,
                VersionSources = [new CsProjVersionSource(projectFilePath, relativePath)]
            });
        }

        return units;
    }

    public void Extract(IPackageableUnit unit)
    {
        unit.Signature = CsharpUnitBuilder.GetProjectSignature(this.GetProjectFilePath(unit));
    }

    public IReadOnlyList<ChangeFinding> Classify(IPackageableUnit? older, IPackageableUnit newer)
    {
        return CompareSignatures.GetFindings(
            newer,
            older?.Signature as CsharpProject,
            newer.Signature as CsharpProject);
    }

    public IReadOnlyList<Version> ReadVersions(IPackageableUnit unit)
    {
        var versions = new List<Version>();
        foreach (var source in unit.VersionSources)
        {
            var version = source.Read();
            if (version == null)
            {
                continue;
            }

            versions.Add(version);
        }

        return versions;
    }

    public IReadOnlyList<string> WriteVersion(IPackageableUnit unit, Version version)
    {
        var written = new List<string>();
        foreach (var source in unit.VersionSources)
        {
            if (!source.IsWritable)
            {
                continue;
            }

            source.Write(version);
            Log.WriteLine($"Wrote {version} to {source.Location}");
            written.Add(source.Location);
        }

        return written;
    }

    public XElement WriteSignature(IPackageableUnit unit)
    {
        var project = unit.Signature as CsharpProject ?? new CsharpProject(unit.UnitId);
        project.SortForPersistence();
        return project.SerializeToElement();
    }

    public object ReadSignature(XElement element)
    {
        return element.DeserializeElement<CsharpProject>();
    }

    /// <summary>
    /// Units keep their paths root-relative so nothing machine-specific can reach the baseline
    /// (BAS-04); the absolute path is rebuilt only when the disk actually has to be touched.
    /// </summary>
    private string GetProjectFilePath(IPackageableUnit unit)
    {
        return Path.Combine(this._folderRoot, unit.RelativePath);
    }
}
