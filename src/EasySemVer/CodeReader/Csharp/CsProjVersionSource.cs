using Winterborn.Tools.EasySemVer.Interfaces;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.CodeReader.Csharp;

/// <summary>
/// The .csproj row of the MVR-03 table: reads the highest of AssemblyVersion, PackageVersion and
/// FileVersion (VER-06), writes every occurrence of whichever of them already exist (SYN-02).
/// </summary>
internal class CsProjVersionSource(string projectFilePath, string relativePath) : IVersionSource
{
    public string Kind => "csproj";

    public string Location { get; } = relativePath;

    public bool IsWritable => true;

    public Version? Read()
    {
        try
        {
            return new CsProjFile(projectFilePath).Version;
        }
        catch (Exception e)
        {
            Log.WriteLine($"Skipping unreadable version in {this.Location}: {e.Message}");
            return null;
        }
    }

    public void Write(Version version)
    {
        new CsProjFile(projectFilePath).Save(version);
    }
}
