using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Winterborn.Tools.EasySemVer.CodeReader.Csharp;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Evaluation;

namespace Winterborn.Tools.EasySemVer.CodeReader.Vb;

/// <summary>
/// Builds one .vbproj's public API surface from source with Roslyn. This is the entire
/// language-specific half of VB support: parse Visual Basic instead of C#, then hand the
/// compilation to <see cref="CsharpUnitBuilder.AppendCompilation"/> and share everything else.
/// <para>
/// It produces a <see cref="CsharpProject"/> on purpose. VB and C# compile to one metadata format
/// and Roslyn hands back the same <c>INamedTypeSymbol</c> graph for both, so the two languages have
/// the same topology and break compatibility in the same ways - a removed public method, a retyped
/// property, a narrowed accessor. Modelling that twice would be forty duplicated rule classes that
/// could never legitimately disagree.
/// </para>
/// <para>
/// The cost is that the type is called <c>Csharp*</c> while holding VB, and VB units are written
/// into <c>&lt;CsharpProject&gt;</c> baseline elements. That is a deliberate, recorded trade
/// (VB-01) and the one place in this codebase where a language is described in another's
/// vocabulary. It is defensible only because the vocabulary is really the CLR's, and it would not
/// be defensible for any language that does not compile to it.
/// </para>
/// </summary>
internal static class VbUnitBuilder
{
    internal static CsharpProject GetProjectSignature(string projectPath)
    {
        var projectDef = new CsharpProject(Path.GetFileNameWithoutExtension(projectPath));

        // DSC-06 through the shared scanner, so build output stays out of the signature exactly as
        // it stays out of discovery (FLD-04).
        var projectFile = new FileInfo(projectPath);
        var projectDirectory = projectFile.Directory
                               ?? throw new DirectoryNotFoundException(
                                   $"Project {projectPath} has no containing directory");
        var vbFiles = FolderScanner.FindFiles(projectDirectory.FullName, "*.vb");

        var syntaxTrees = vbFiles
            .Select(f => VisualBasicSyntaxTree.ParseText(File.ReadAllText(f), path: f))
            .ToList();

        if (syntaxTrees.Count < 1)
        {
            Log.WriteLine($"No .vb files found under {projectDirectory.Name}");
            return projectDef;
        }

        // VB's compiler defaults are not C#'s: without an explicit root namespace Roslyn uses the
        // assembly name as one, which would prefix every type in the signature with the project
        // name and make renaming a project read as removing its entire API. An empty root namespace
        // makes the symbol's fully-qualified name mean what the source says, as it does for C#.
        var options = new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithRootNamespace(string.Empty);

        var compilation = VisualBasicCompilation.Create(
            assemblyName: projectDef.Name,
            syntaxTrees: syntaxTrees,
            references: CsharpUnitBuilder.CreateReferences(),
            options: options);

        return CsharpUnitBuilder.AppendCompilation(projectDef, compilation);
    }
}
