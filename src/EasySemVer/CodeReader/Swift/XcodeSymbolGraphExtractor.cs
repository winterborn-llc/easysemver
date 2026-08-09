using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// SWE-01 for Xcode targets (D-06): the same symbol-graph flags, carried in through
/// OTHER_SWIFT_FLAGS. Graphs land in a temporary directory outside the folder root (SWE-06).
/// <para>
/// This is the expensive path §20 O-03 warns about: a repository containing an .xcodeproj pays a
/// full xcodebuild on every versioned run.
/// </para>
/// </summary>
internal class XcodeSymbolGraphExtractor(IRunProcess runProcess)
{
    internal static readonly TimeSpan Timeout = TimeSpan.FromMinutes(30);

    internal Dictionary<string, SwiftModule> ExtractTarget(
        string projectPath,
        string targetName,
        string unitDescription)
    {
        var graphDirectory = Directory.CreateTempSubdirectory("easysemver-xcode-symbolgraph");
        try
        {
            var swiftFlags =
                "-emit-symbol-graph "
                + $"-emit-symbol-graph-dir {graphDirectory.FullName} "
                + "-emit-extension-block-symbols "
                + "-symbol-graph-minimum-access-level public";

            var result = runProcess.Run(
                "xcodebuild",
                [
                    "build",
                    "-project", projectPath,
                    "-target", targetName,
                    "-configuration", "Debug",
                    "CODE_SIGNING_ALLOWED=NO",
                    $"OTHER_SWIFT_FLAGS=$(inherited) {swiftFlags}"
                ],
                Path.GetDirectoryName(projectPath) ?? Environment.CurrentDirectory,
                Timeout);

            if (!result.IsSuccess)
            {
                throw new SwiftToolchainException(unitDescription, result);
            }

            return SwiftSymbolGraphExtractor.ReadGraphDirectory(graphDirectory.FullName);
        }
        finally
        {
            TryDelete(graphDirectory.FullName);
        }
    }

    private static void TryDelete(string directory)
    {
        try
        {
            Directory.Delete(directory, recursive: true);
        }
        catch (Exception e)
        {
            Log.WriteLine($"Could not clean up the symbol-graph directory {directory}: {e.Message}");
        }
    }
}
