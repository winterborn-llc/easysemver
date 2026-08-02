using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Extensions;

namespace Winterborn.Library.EasySemVer.CodeReader.Csharp;

/// <summary>
/// Builds one .csproj's public API surface from source with Roslyn (SIG-01). One unit in, one
/// <see cref="CsharpProject"/> out - nothing here knows about folders, baselines, or versions.
/// </summary>
internal static class CsharpUnitBuilder
{
    internal static CsharpProject GetProjectSignature(string projectPath)
    {
        var projectDef = new CsharpProject(Path.GetFileNameWithoutExtension(projectPath));

        // DSC-06 goes through the shared scanner so build output and package caches stay out of
        // the signature exactly as they stay out of discovery (FLD-04, was G-10) - an obj/ full
        // of generated partials is not this project's API.
        var csProjFile = new FileInfo(projectPath);
        var projectDirectory = csProjFile.Directory
                               ?? throw new DirectoryNotFoundException(
                                   $"Project {projectPath} has no containing directory");
        var csFiles = FolderScanner.FindFiles(projectDirectory.FullName, "*.cs");

        var syntaxTrees = csFiles
            .Select(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f), path: f))
            .ToList();

        if (syntaxTrees.Count < 1)
        {
            Log.WriteLine($"No .cs files found under {projectDirectory.Name}");
            return projectDef;
        }

        // A minimal reference set: enough for object, LINQ and Console to resolve. Types from
        // other projects and NuGet packages stay as error symbols with their written names,
        // which is stable run to run even though it is not namespace-qualified (SIG-01, G-16).
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).GetTypeInfo().Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: projectDef.Name,
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Walk the symbols this project actually declares. The compilation's global namespace is
        // the merged view across every reference, so walking it would pull public types out of
        // System.Console.dll and friends into this project's signature (Internal.Console was
        // showing up in real baselines); the source assembly's namespace is just ours.
        foreach (var type in compilation.Assembly.GlobalNamespace.GetNamespaceTypes())
        {
            AppendType(projectDef, type, declaringTypeName: string.Empty);
        }

        return projectDef;
    }

    /// <summary>
    /// CSX-01 - every public namespace-level and public nested type of every kind, each modelled
    /// as its own concept rather than flattened into "class".
    /// </summary>
    private static void AppendType(
        CsharpProject project,
        INamespaceOrTypeSymbol symbol,
        string declaringTypeName)
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

        var type = BuildType(typeSymbol, fullName, declaringTypeName);
        if (type == null)
        {
            return;
        }

        project.Add(type);

        // CSX-01 - nested types are types in their own right, keyed by their Outer.Inner name and
        // tagged with the type that declares them, which is what R41 keys off.
        foreach (var nested in typeSymbol.GetTypeMembers())
        {
            AppendType(project, nested, fullName);
        }
    }

    private static bool IsTypeInScope(string fullName)
    {
        // SIG-03 - guards against dependency or generated symbols leaking into a project's own
        // signature.
        string[] excludedPrefixes = ["Newtonsoft.", "Microsoft.", "Coverlet.", "System.", "XUnit."];
        foreach (var prefix in excludedPrefixes)
        {
            if (fullName.StartsWith(prefix, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return fullName.Trim().Length > 0;
    }

    private static CsharpType? BuildType(
        INamedTypeSymbol type,
        string fullName,
        string declaringTypeName)
    {
        var members = type.GetMembers();
        CsharpType? built = type.TypeKind switch
        {
            TypeKind.Interface => new CsharpInterface(),
            TypeKind.Struct when type.IsRecord => new CsharpRecord { IsValueType = true },
            TypeKind.Struct => new CsharpStruct(),
            TypeKind.Class when type.IsRecord => new CsharpRecord(),
            TypeKind.Class => new CsharpClass(),
            TypeKind.Enum => BuildEnum(type, members),
            TypeKind.Delegate => BuildDelegate(type),
            _ => null
        };

        if (built == null)
        {
            return null;
        }

        built.Name = fullName;
        built.DeclaringType = declaringTypeName;
        built.IsStatic = type.IsStatic;
        built.IsAbstract = type.IsAbstract;
        built.IsSealed = type.IsSealed;
        built.BaseType = GetBaseTypeName(type);
        built.ImplementedInterfaces = GetInterfaceNames(type);
        built.GenericParameters = GetGenericParameters(type.TypeParameters);

        // An enum's members are its cases and a delegate's shape is its Invoke signature; neither
        // has an ordinary member surface worth recording.
        if (built is CsharpEnum or CsharpDelegate)
        {
            return built;
        }

        built.Properties = GetProperties(members);
        built.Methods = GetMethods(members);
        built.Fields = GetFields(members);
        built.Events = GetEvents(members);

        if (built is CsharpRecord record)
        {
            record.PositionalParameters = GetPositionalParameters(type);
        }

        return built;
    }

    private static CsharpEnum BuildEnum(INamedTypeSymbol type, IEnumerable<ISymbol> members)
    {
        var built = new CsharpEnum
        {
            UnderlyingType = type.EnumUnderlyingType?.GetFullyQualifiedName() ?? string.Empty
        };

        foreach (var member in members)
        {
            if (member is not IFieldSymbol { IsConst: true } field)
            {
                continue;
            }

            built.Members.Add(new CsharpEnumMember
            {
                Name = field.Name,
                Value = field.ConstantValue?.ToString() ?? string.Empty
            });
        }

        return built;
    }

    private static CsharpDelegate BuildDelegate(INamedTypeSymbol type)
    {
        var invoke = type.DelegateInvokeMethod;
        return new CsharpDelegate
        {
            ReturnType = invoke?.ReturnType.GetFullyQualifiedName() ?? string.Empty,
            Parameters = invoke == null ? [] : GetParameters(invoke.Parameters)
        };
    }

    /// <summary>
    /// CSX-03/R27 - a record's positional parameters, taken from its primary constructor. Roslyn
    /// does not flag that constructor, so it is identified the way the language defines it: the
    /// one whose parameters match the compiler-generated Deconstruct.
    /// </summary>
    private static List<CsharpMethodParameter> GetPositionalParameters(INamedTypeSymbol type)
    {
        foreach (var member in type.GetMembers("Deconstruct"))
        {
            if (member is not IMethodSymbol deconstruct)
            {
                continue;
            }

            var parameters = new List<CsharpMethodParameter>();
            foreach (var parameter in deconstruct.Parameters)
            {
                parameters.Add(BuildParameter(parameter));
            }

            return parameters;
        }

        return [];
    }

    private static string GetBaseTypeName(INamedTypeSymbol type)
    {
        var baseType = type.BaseType;
        if (baseType == null)
        {
            return string.Empty;
        }

        // Every class derives from object and every struct from ValueType; recording that would
        // only add noise that can never change.
        var name = baseType.GetFullyQualifiedName();
        return name is "object" or "System.Object" or "System.ValueType" or "System.Enum"
            ? string.Empty
            : name;
    }

    private static List<string> GetInterfaceNames(INamedTypeSymbol type)
    {
        var names = new List<string>();
        foreach (var implemented in type.Interfaces)
        {
            names.Add(implemented.GetFullyQualifiedName());
        }

        names.Sort(StringComparer.Ordinal);
        return names;
    }

    private static List<CsharpGenericParameter> GetGenericParameters(
        IEnumerable<ITypeParameterSymbol> typeParameters)
    {
        var parameters = new List<CsharpGenericParameter>();
        foreach (var typeParameter in typeParameters)
        {
            parameters.Add(new CsharpGenericParameter
            {
                Name = typeParameter.Name,
                Constraints = GetConstraints(typeParameter)
            });
        }

        return parameters;
    }

    private static string GetConstraints(ITypeParameterSymbol typeParameter)
    {
        var constraints = new List<string>();
        if (typeParameter.HasReferenceTypeConstraint)
        {
            constraints.Add("class");
        }

        if (typeParameter.HasValueTypeConstraint)
        {
            constraints.Add("struct");
        }

        if (typeParameter.HasNotNullConstraint)
        {
            constraints.Add("notnull");
        }

        if (typeParameter.HasUnmanagedTypeConstraint)
        {
            constraints.Add("unmanaged");
        }

        if (typeParameter.HasConstructorConstraint)
        {
            constraints.Add("new()");
        }

        foreach (var constraintType in typeParameter.ConstraintTypes)
        {
            constraints.Add(constraintType.GetFullyQualifiedName());
        }

        constraints.Sort(StringComparer.Ordinal);
        return string.Join(", ", constraints);
    }

    private static CsharpPropertyList GetProperties(IEnumerable<ISymbol> members)
    {
        var properties = new CsharpPropertyList();
        foreach (var member in members)
        {
            if (member is not IPropertySymbol property)
            {
                continue;
            }

            if (member.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            // An indexer's name collides across overloads and its parameters are what matter;
            // it is recorded as a method (get_Item / set_Item are filtered, so record it here).
            if (property.IsIndexer)
            {
                continue;
            }

            properties.Add(BuildProperty(property));
        }

        return properties;
    }

    private static CsharpProperty BuildProperty(IPropertySymbol property)
    {
        return new CsharpProperty
        {
            Name = property.Name,
            Type = property.Type.GetFullyQualifiedName(),
            IsReadable = property.GetMethod != null,
            IsWritable = property.SetMethod != null,
            IsInitOnly = property.SetMethod?.IsInitOnly == true,
            IsStatic = property.IsStatic,
            IsRequired = property.IsRequired,
            HasDefaultImplementation = HasBody(property.GetMethod) || HasBody(property.SetMethod)
        };
    }

    private static List<CsharpField> GetFields(IEnumerable<ISymbol> members)
    {
        var fields = new List<CsharpField>();
        foreach (var member in members)
        {
            if (member is not IFieldSymbol field)
            {
                continue;
            }

            if (member.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            // Backing fields of properties and events are implementation, not surface.
            if (field.IsImplicitlyDeclared)
            {
                continue;
            }

            fields.Add(new CsharpField
            {
                Name = field.Name,
                Type = field.Type.GetFullyQualifiedName(),
                IsStatic = field.IsStatic,
                IsReadOnly = field.IsReadOnly,
                IsConstant = field.IsConst
            });
        }

        return fields;
    }

    private static List<CsharpEvent> GetEvents(IEnumerable<ISymbol> members)
    {
        var events = new List<CsharpEvent>();
        foreach (var member in members)
        {
            if (member is not IEventSymbol eventSymbol)
            {
                continue;
            }

            if (member.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            events.Add(new CsharpEvent
            {
                Name = eventSymbol.Name,
                HandlerType = eventSymbol.Type.GetFullyQualifiedName(),
                IsStatic = eventSymbol.IsStatic
            });
        }

        return events;
    }

    private static CsharpMethodList GetMethods(IEnumerable<ISymbol> members)
    {
        var list = new CsharpMethodList();
        foreach (var member in members)
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (!ShouldWeIncludeMethod(method))
            {
                continue;
            }

            var methodOverride = BuildOverride(method);
            if (!list.Contains(method.Name))
            {
                list.Add(new CsharpMethod
                {
                    MethodName = method.Name,
                    MethodType = methodOverride.ReturnType
                });
            }

            GetMethod(list, method.Name).Overrides.Add(methodOverride);
        }

        return list;
    }

    private static CsharpMethod GetMethod(CsharpMethodList list, string name)
    {
        foreach (var method in list)
        {
            if (method.MethodName != name)
            {
                continue;
            }

            return method;
        }

        throw new KeyNotFoundException($"No method named '{name}' is present.");
    }

    private static bool ShouldWeIncludeMethod(IMethodSymbol method)
    {
        if (method.DeclaredAccessibility != Accessibility.Public)
        {
            return false;
        }

        // SIG-06 - property accessors are covered by the property itself; constructors,
        // operators, indexer accessors and event accessors all stay in.
        return method.MethodKind is not (MethodKind.PropertyGet or MethodKind.PropertySet);
    }

    private static CsharpMethodOverride BuildOverride(IMethodSymbol method)
    {
        return new CsharpMethodOverride
        {
            ReturnType = method.ReturnType.GetFullyQualifiedName(),
            IsStatic = method.IsStatic,
            IsVirtual = method.IsVirtual,
            IsAbstract = method.IsAbstract,
            IsOverride = method.IsOverride,
            IsSealed = method.IsSealed,
            HasDefaultImplementation = HasBody(method),
            GenericParameters = GetGenericParameters(method.TypeParameters),
            Parameters = GetParameters(method.Parameters)
        };
    }

    /// <summary>
    /// R21 - an interface member with a body is a default implementation, so adding it does not
    /// break existing implementers.
    /// </summary>
    private static bool HasBody(IMethodSymbol? method)
    {
        if (method == null)
        {
            return false;
        }

        return method.ContainingType?.TypeKind == TypeKind.Interface && !method.IsAbstract;
    }

    private static List<CsharpMethodParameter> GetParameters(IEnumerable<IParameterSymbol> parameters)
    {
        var built = new List<CsharpMethodParameter>();
        foreach (var parameter in parameters)
        {
            built.Add(BuildParameter(parameter));
        }

        return built;
    }

    private static CsharpMethodParameter BuildParameter(IParameterSymbol parameter)
    {
        return new CsharpMethodParameter
        {
            ParameterType = parameter.Type.GetFullyQualifiedName(),
            ParameterName = parameter.Name,
            IsRequired = GetIsRequired(parameter),
            RefKind = parameter.RefKind.ToString(),
            IsParams = parameter.IsParams
        };
    }

    private static bool GetIsRequired(IParameterSymbol parameter)
    {
        // SIG-08.
        if (parameter.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return false;
        }

        return !parameter.HasExplicitDefaultValue;
    }
}
