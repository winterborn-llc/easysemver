using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.DataObject.Swift;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// SWE-01 - builds a SwiftPM package with the symbol-graph flags on and reads the resulting JSON.
/// Graphs land in a temporary directory outside the folder root, so extraction never dirties the
/// user's tree or feeds its own output back into discovery (SWE-06).
/// </summary>
internal class SwiftSymbolGraphExtractor(IRunProcess runProcess)
{
    internal static readonly TimeSpan Timeout = TimeSpan.FromMinutes(10);

    /// <summary>Reads every target of one package in a single build, since the build emits them all.</summary>
    internal Dictionary<string, SwiftModule> ExtractPackage(
        string packageDirectory,
        string packageDescription)
    {
        var graphDirectory = Directory.CreateTempSubdirectory("easysemver-symbolgraph");
        try
        {
            var result = runProcess.Run(
                "swift",
                [
                    "build",

                    // Deliberately *not* `--build-tests`. It was here because UNI-03 makes a test
                    // target a unit, and SWE-05 fails a unit with no graph - but UNI-04 means a
                    // test target's API surface is never read, so the flag compiled and linked an
                    // XCTest bundle to produce a graph nothing consumes. It dominated the run: on
                    // the fixture package a plain build is ~1.3s against ~11-20s with it. A plain
                    // `swift build` still emits a graph for every non-test target, including one
                    // no product exposes, which is exactly the set that UNI-04 leaves to extract.
                    "--package-path", packageDirectory,
                    "-Xswiftc", "-emit-symbol-graph",
                    "-Xswiftc", "-emit-symbol-graph-dir",
                    "-Xswiftc", graphDirectory.FullName,
                    "-Xswiftc", "-emit-extension-block-symbols",
                    "-Xswiftc", "-symbol-graph-minimum-access-level",
                    "-Xswiftc", "public"
                ],
                packageDirectory,
                Timeout);

            if (!result.IsSuccess)
            {
                throw new SwiftToolchainException(packageDescription, result);
            }

            return ReadGraphDirectory(graphDirectory.FullName);
        }
        finally
        {
            TryDelete(graphDirectory.FullName);
        }
    }

    /// <summary>One module per graph file group; a module emits one file per module it extends.</summary>
    internal static Dictionary<string, SwiftModule> ReadGraphDirectory(string graphDirectory)
    {
        var documentsByModule = new Dictionary<string, List<string>>();
        foreach (var path in Directory.GetFiles(graphDirectory, "*.symbols.json", SearchOption.AllDirectories))
        {
            var json = File.ReadAllText(path);
            var moduleName = GetModuleName(json);
            if (moduleName.Length < 1)
            {
                continue;
            }

            if (!documentsByModule.TryGetValue(moduleName, out var documents))
            {
                documents = [];
                documentsByModule[moduleName] = documents;
            }

            documents.Add(json);
        }

        var modules = new Dictionary<string, SwiftModule>();
        foreach (var pair in documentsByModule)
        {
            modules[pair.Key] = SymbolGraphReader.Read(pair.Key, pair.Value);
        }

        return modules;
    }

    private static string GetModuleName(string json)
    {
        using var document = System.Text.Json.JsonDocument.Parse(json);
        return SymbolGraphReader.GetGraphModuleName(document.RootElement);
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
