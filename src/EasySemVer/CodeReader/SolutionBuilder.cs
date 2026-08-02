using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Extensions;
using Winterborn.Library.EasySemVer.Interfaces;
using Project = Winterborn.Library.EasySemVer.DataObject.Project;
using Solution = Winterborn.Library.EasySemVer.DataObject.Solution;

namespace Winterborn.Library.EasySemVer.CodeReader;

internal class SolutionBuilder
{
    public static ISolution GetSolutionSignatureFromAnalyzer(params string[] projectPaths)
    {
        var solution = new Solution();
        foreach (var projectPath in projectPaths)
        {
            var project = GetProjectSignature(projectPath);
            solution.Add(project);
        }

        return solution;
    }
    
    public static IProject GetProjectSignature(string projectPath)
    {
        Console.WriteLine($"Loading project: {projectPath}");
        
        var projectDef = new Project(Path.GetFileNameWithoutExtension(projectPath));

        // 1. Load all .cs files
        var csProjFile = new FileInfo(projectPath);
        var projectDirectory = csProjFile.Directory;
        var projectPathDir = projectDirectory?.FullName;
        var csFiles = Directory.EnumerateFiles(projectPathDir, "*.cs", SearchOption.AllDirectories);

        var syntaxTrees = csFiles
            .Select(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f), path: f))
            .ToList();

        if (!syntaxTrees.Any())
        {
            Console.WriteLine("No .cs files found.");
            return projectDef;
        }

        // 2. Basic references (you can add more as needed)
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).GetTypeInfo().Assembly.Location)
        };

        // 3. Create compilation
        var compilation = CSharpCompilation.Create(
            assemblyName: projectDef.Name,
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // 4. Walk symbols like before
        var types = compilation.GlobalNamespace.GetNamespaceTypes();
        foreach (var type in types)
        {
            AppendTypesRecursive(projectDef, type);
        }

        return projectDef;
        
        
        
        
        
        
        
        
        
        /*
        using var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        
        var solution = workspace.CurrentSolution
            .AddProject(projectId, "AdhocProject", "AdhocProject", LanguageNames.CSharp)
            .WithProjectCompilationOptions(projectId,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var csProject = new CsProjFile(projectPath);
        var csFiles = csProject.GetCsFiles();
        var project = solution.GetProject(projectId);
        foreach (var csFile in csFiles)
        {
            var doc = project.AddDocument(csFile.Name, csFile.GetFileText(), null, csFile.FullName);
        }
        
        if (project == null)
        {
            throw new InvalidOperationException($"Project {projectPath} was not found.");
        }
        
        var projectDef = new Project(project.Name);
        var compilation = project.GetCompilationAsync().Result;
        if (compilation == null)
        {
            Console.WriteLine("Failed to compile project.");
            return projectDef;
        }
        
        foreach (var type in compilation.GlobalNamespace.GetNamespaceTypes())
        {
            AppendTypesRecursive(projectDef, type);
        }
        
        return projectDef;
        */
    }

    private static void AppendTypesRecursive(IProject project, INamespaceOrTypeSymbol symbol)
    {
        if (symbol is not INamedTypeSymbol typeSymbol)
        {
            return;
        }
        
        if (!typeSymbol.IsPublic())
        {
            return;
        }
        
        var fullName = typeSymbol.GetFullyQualifiedName();
        if (!IsTypeInScope(fullName))
        {
            return;
        }

        var type = GetClass(typeSymbol);
        if (type.Name.Trim().Length < 1)
        {
            return;
        }
        
        project.Classes.Add(type);
    }
    
    private static bool IsTypeInScope(string fullName)
    {
        if (fullName.StartsWith("Newtonsoft."))
        {
            return false;
        }
        
        if (fullName.StartsWith("Microsoft."))
        {
            return false;
        }
        
        if (fullName.StartsWith("Coverlet."))
        {
            return false;
        }
        
        if (fullName.StartsWith("System."))
        {
            return false;
        }
        
        if (fullName.StartsWith("XUnit."))
        {
            return false;
        }

        return true;
    }

    private static IProjectClass GetClass(INamedTypeSymbol type)
    {
        if (type.TypeKind != TypeKind.Class)
        {
            return new ProjectClass();
        }
        
        // TODO enums
        // TODO events
        // TODO delegates
        var members = type.GetMembers();
        var projectClass = new ProjectClass
        {
            Name = type.GetFullyQualifiedName(),
            Properties = PrintProperties(members),
            Methods = GetMethods(members)
        };

        return projectClass;
    }

    private static IPropertyList PrintProperties(IEnumerable<ISymbol> members)
    {
        var properties = new PropertyList();
        foreach (var member in members)
        {
            var property = member as IPropertySymbol;
            if (!ShouldWeIncludeProperty(property, member))
            {
                continue;
            }
            
            var propertyDefinition = GetProperty(property);
            properties.Add(propertyDefinition);
        }
        
        return properties;
    }

    private static bool ShouldWeIncludeProperty([NotNullWhen(true)] IPropertySymbol? property, ISymbol member)
    {
        if (property == null)
        {
            return false;
        }

        if (member.DeclaredAccessibility != Accessibility.Public)
        {
            return false;
        }

        return true;
    }

    private static Property GetProperty(IPropertySymbol prop)
    {
        return new Property
        {
            Name = prop.Name,
            Type = prop.Type.GetFullyQualifiedName(),
            IsReadable = prop.GetMethod != null,
            IsWritable = prop.SetMethod != null
        };
    }
    
    private static IMethodList GetMethods(IEnumerable<ISymbol> members)
    {
        var list = new MethodList();
        foreach (var member in members)
        {
            var method = member as IMethodSymbol;
            if (!ShouldWeIncludeMethod(method, member))
            {
                continue;
            }

            var methodDefinition = GetMethodDefinition(method);
            if (!list.Contains(methodDefinition.Name))
            {
                var newMethod = new Method 
                {
                    MethodName = methodDefinition.Name, 
                    MethodType = methodDefinition.Type
                };
                
                list.Add(newMethod);
            }

            var overrideDef = new MethodOverride(methodDefinition.Inputs.ToArray());
            list[method.Name].Overrides.Add(overrideDef);
        }

        return list;
    }

    private static bool ShouldWeIncludeMethod([NotNullWhen(true)]IMethodSymbol? method, [NotNullWhen(true)] ISymbol? member)
    {
        if (method == null)
        {
            return false;
        }
        
        if (member == null)
        {
            return false;
        }
            
        if (member.DeclaredAccessibility != Accessibility.Public)
        {
            return false;
        }

        if (method.MethodKind == MethodKind.PropertyGet)
        {
            return false;
        }

        if (method.MethodKind == MethodKind.PropertySet)
        {
            return false;
        }

        return true;
    }

    private static IMethodDefinition GetMethodDefinition(IMethodSymbol? method)
    {
        if (method == null)
        {
            return new MethodDefinition();
        }
        
        var definition = new MethodDefinition
        {
            Type = method.ReturnType.GetFullyQualifiedName(),
            Name = method.Name
        };
        
        var parameters = method.Parameters;
        foreach (var parameter in parameters)
        {
            var isRequired = GetIsRequired(parameter);
            definition.Inputs.Add(new MethodOverrideInput
            {
                ParameterType = parameter.Type.GetFullyQualifiedName(),
                ParameterName = parameter.Name,
                IsRequired = isRequired 
                // TODO by ref
                // TODO input / output
                // TODO is override
                // TODO Is abstract
                // TODO is virtual
                // TODO Is sealed
                // TODO is params
                
            });
        }
        
        return definition;
    }

    private static bool GetIsRequired(IParameterSymbol parameter)
    {
        if (parameter.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return false;
        }

        if (parameter.HasExplicitDefaultValue)
        {
            return false;
        }
        
        return true;
    }
}