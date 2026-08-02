using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Extensions;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;
using CsharpProject = Winterborn.Library.EasySemVer.DataObject.Csharp.CsharpProject;
using Solution = Winterborn.Library.EasySemVer.DataObject.Solution;

namespace Winterborn.Library.EasySemVer.CodeReader.Csharp;

internal class CsharpUnitBuilder
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
    
    public static ICsharpProject GetProjectSignature(string projectPath)
    {
        Console.WriteLine($"Loading project: {projectPath}");
        
        var projectDef = new CsharpProject(Path.GetFileNameWithoutExtension(projectPath));

        // 1. Load all .cs files
        var csProjFile = new FileInfo(projectPath);
        var projectDirectory = csProjFile.Directory
                               ?? throw new DirectoryNotFoundException($"Project {projectPath} has no containing directory");
        var csFiles = Directory.EnumerateFiles(projectDirectory.FullName, "*.cs", SearchOption.AllDirectories);

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
    }

    private static void AppendTypesRecursive(ICsharpProject project, INamespaceOrTypeSymbol symbol)
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

    private static ICsharpClass GetClass(INamedTypeSymbol type)
    {
        if (type.TypeKind != TypeKind.Class)
        {
            return new CsharpClass();
        }
        
        // TODO enums
        // TODO events
        // TODO delegates
        var members = type.GetMembers();
        var projectClass = new CsharpClass
        {
            Name = type.GetFullyQualifiedName(),
            Properties = PrintProperties(members),
            Methods = GetMethods(members)
        };

        return projectClass;
    }

    private static ICsharpPropertyList PrintProperties(IEnumerable<ISymbol> members)
    {
        var properties = new CsharpPropertyList();
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

    private static CsharpProperty GetProperty(IPropertySymbol prop)
    {
        return new CsharpProperty
        {
            Name = prop.Name,
            Type = prop.Type.GetFullyQualifiedName(),
            IsReadable = prop.GetMethod != null,
            IsWritable = prop.SetMethod != null
        };
    }
    
    private static ICsharpMethodList GetMethods(IEnumerable<ISymbol> members)
    {
        var list = new CsharpMethodList();
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
                var newMethod = new CsharpMethod 
                {
                    MethodName = methodDefinition.Name, 
                    MethodType = methodDefinition.Type
                };
                
                list.Add(newMethod);
            }

            var overrideDef = new CsharpMethodOverride(methodDefinition.Inputs.ToArray());
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

    private static ICsharpMethodDefinition GetMethodDefinition(IMethodSymbol? method)
    {
        if (method == null)
        {
            return new CsharpMethodDefinition();
        }
        
        var definition = new CsharpMethodDefinition
        {
            Type = method.ReturnType.GetFullyQualifiedName(),
            Name = method.Name
        };
        
        var parameters = method.Parameters;
        foreach (var parameter in parameters)
        {
            var isRequired = GetIsRequired(parameter);
            definition.Inputs.Add(new CsharpMethodParameter
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