using Winterborn.Tools.EasySemVer.DataObject.Swift;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// Builds one module's public API surface from its Swift source (SWE-01). Nothing here runs a
/// compiler: a declaration is what the file says it is, and a type this module does not declare
/// stays as the name it was written with - the same bargain the C# reader strikes for types it
/// cannot resolve (SIG-01, G-16).
/// <para>
/// The order matters. Types are built before extensions, because an extension folds into the type
/// it extends and that type may be declared in another file; inheritance clauses are resolved
/// after both, because telling a superclass from a conformance needs to know every protocol this
/// module declares.
/// </para>
/// </summary>
internal class SwiftSourceReader
{
    /// <summary>
    /// Types an enum's first inheritance entry can be when it is a raw value rather than a
    /// conformance. Swift permits others, and those are caught by the cases having raw values
    /// written out, which is the only other way to tell without resolving the type.
    /// </summary>
    private static readonly string[] RawValueTypes =
    [
        "String", "Character", "Bool", "Double", "Float",
        "Int", "Int8", "Int16", "Int32", "Int64",
        "UInt", "UInt8", "UInt16", "UInt32", "UInt64"
    ];

    private readonly SwiftModule _module;

    private readonly Dictionary<string, SwiftType> _typesByName = new(StringComparer.Ordinal);

    /// <summary>
    /// SWE-02 - the types this module declares and does not export. They are not in the model, but
    /// they still have to be recognised: an extension of one is not API, and a conformance to one
    /// is not either, and both would otherwise be mistaken for something foreign and recorded.
    /// </summary>
    private readonly HashSet<string> _internalTypeNames = new(StringComparer.Ordinal);

    private readonly Dictionary<string, SwiftOperatorDeclaration> _operatorDeclarations =
        new(StringComparer.Ordinal);

    private readonly List<(SwiftType Type, SwiftDeclarationHeader Header)> _inheritance = [];

    private SwiftSourceReader(string moduleName)
    {
        this._module = new SwiftModule(moduleName);
    }

    /// <summary>One module, from every Swift file that belongs to it.</summary>
    internal static SwiftModule Read(string moduleName, IEnumerable<string> sourceTexts)
    {
        var reader = new SwiftSourceReader(moduleName);
        var files = new List<SwiftSourceFile>();
        foreach (var text in sourceTexts)
        {
            files.Add(new SwiftSourceFile(text));
        }

        reader.ReadOperatorDeclarations(files);
        reader.ReadTypesAndGlobals(files);
        reader.ReadExtensions(files);
        reader.ResolveInheritance();

        reader._module.SortForPersistence();
        return reader._module;
    }

    /// <summary>
    /// Swept first because an operator's declaration says how it parses and can be written after
    /// the function that implements it, or in another file entirely.
    /// </summary>
    private void ReadOperatorDeclarations(List<SwiftSourceFile> files)
    {
        foreach (var file in files)
        {
            foreach (var declaration in file.TopLevel)
            {
                if (declaration.Header.Keyword != "operator")
                {
                    continue;
                }

                this._operatorDeclarations[declaration.Header.Name] = new SwiftOperatorDeclaration
                {
                    Name = declaration.Header.Name,
                    Kind = GetOperatorKind(declaration.Header),
                    PrecedenceGroup = declaration.Header.DeclaredType
                };
            }
        }
    }

    private void ReadTypesAndGlobals(List<SwiftSourceFile> files)
    {
        foreach (var file in files)
        {
            foreach (var declaration in file.TopLevel)
            {
                var header = declaration.Header;
                var access = header.GetAccessLevel("internal");
                if (header.Keyword == "extension")
                {
                    continue;
                }

                if (header.IsTypeDeclaration)
                {
                    this.AddType(file, declaration, ownerPath: string.Empty, access);
                    continue;
                }

                if (IsVisible(access))
                {
                    this.AddGlobal(file, declaration, access);
                }
            }
        }
    }

    private void ReadExtensions(List<SwiftSourceFile> files)
    {
        foreach (var file in files)
        {
            foreach (var declaration in file.TopLevel)
            {
                if (declaration.Header.Keyword != "extension")
                {
                    continue;
                }

                this.AddExtension(file, declaration);
            }
        }
    }

    /// <summary>
    /// SWM-03 - a class's superclass is the first entry of its inheritance clause, and every other
    /// entry is a conformance. Which of the two the first entry is cannot be settled from source
    /// alone for a type this module does not declare, so a protocol declared here is known to be
    /// one and anything else is read as a superclass. The guess is wrong only for a foreign
    /// protocol written first, it is the same guess a reader of the file makes, and it is stable:
    /// the same source always produces the same answer, so it cannot churn a baseline.
    /// </summary>
    private void ResolveInheritance()
    {
        foreach (var pending in this._inheritance)
        {
            var entries = SwiftText.SplitTopLevel(pending.Header.Inheritance, ',');
            var first = 0;

            if (pending.Type is SwiftEnum enumeration
                && entries.Count > 0
                && IsRawValueType(entries[0], enumeration))
            {
                enumeration.RawValueType = entries[0];
                first = 1;
            }
            else if (pending.Type is SwiftClass
                     && entries.Count > 0
                     && !this.IsDeclaredProtocol(entries[0]))
            {
                pending.Type.Superclass = entries[0];
                first = 1;
            }

            for (var index = first; index < entries.Count; index++)
            {
                AddConformance(pending.Type.Conformances, entries[index]);
            }
        }
    }

    private void AddType(
        SwiftSourceFile file,
        SwiftSourceDeclaration declaration,
        string ownerPath,
        string access)
    {
        // A type nobody outside the module can name has no public surface, and neither has
        // anything nested inside it however it is marked, so the whole subtree stops here. Its
        // name is kept, because an extension or a conformance naming it has to be recognised as
        // internal rather than assumed to belong to another module.
        if (!IsVisible(access))
        {
            this._internalTypeNames.Add(
                SwiftSignatureName.Qualify(ownerPath, declaration.Header.Name));
            return;
        }

        var type = SwiftSourceFactory.CreateType(declaration.Header, ownerPath, access);
        if (type == null || !this.Register(type))
        {
            return;
        }

        this._inheritance.Add((type, declaration.Header));
        if (!declaration.Block.HasBody)
        {
            return;
        }

        this.AddMembers(file, declaration.Block, type, SwiftMemberScope.ForType(type, access));
    }

    private void AddMembers(
        SwiftSourceFile file,
        SwiftDeclarationBlock body,
        SwiftType owner,
        SwiftMemberScope scope)
    {
        foreach (var declaration in file.ReadBody(body))
        {
            var header = declaration.Header;

            // A case is as visible as its enum and has no say in the matter; everything else
            // either says what it is or falls back to what the scope makes it.
            var access = header.Keyword == "case"
                ? scope.OwnerAccess
                : header.GetAccessLevel(scope.DefaultAccess);

            if (header.IsTypeDeclaration)
            {
                this.AddType(file, declaration, scope.OwnerPath, access);
                continue;
            }

            if (!IsVisible(access))
            {
                continue;
            }

            this.AddMember(file, declaration, owner, scope, access);
        }
    }

    private void AddMember(
        SwiftSourceFile file,
        SwiftSourceDeclaration declaration,
        SwiftType owner,
        SwiftMemberScope scope,
        string access)
    {
        var header = declaration.Header;
        switch (header.Keyword)
        {
            case "func":
                this.AddFunction(owner, SwiftSourceFactory.CreateFunction(
                    header, scope.OwnerPath, access, scope.ExtensionConstraints), scope);
                return;

            case "init":
                owner.Initializers.Add(
                    SwiftSourceFactory.CreateInitializer(header, scope.OwnerPath, access));
                return;

            case "subscript":
                owner.Subscripts.Add(SwiftSourceFactory.CreateSubscript(
                    header, scope.OwnerPath, access, file.ReadBodyText(declaration.Block)));
                return;

            case "var" or "let":
                this.AddProperty(owner, SwiftSourceFactory.CreateProperty(
                    header,
                    scope.OwnerPath,
                    access,
                    file.ReadBodyText(declaration.Block)), scope);
                return;

            case "case" when owner is SwiftEnum enumeration:
                enumeration.Cases.AddRange(SwiftSourceFactory.CreateEnumCases(
                    header, declaration.Block.Header, scope.OwnerPath, access));
                return;

            case "associatedtype" when owner is SwiftProtocol protocolType:
                protocolType.AssociatedTypes.Add(header.Name);
                return;

            case "typealias":
                owner.Properties.Add(
                    SwiftSourceFactory.CreateNestedAlias(header, scope.OwnerPath, access));
                return;
        }
    }

    /// <summary>
    /// S21 - an extension of a protocol that supplies a body for one of its requirements has not
    /// added a member, it has defaulted one. The requirement is already modelled, so what the
    /// extension contributes is the fact that conformers no longer have to write it.
    /// </summary>
    private void AddFunction(SwiftType owner, SwiftFunction function, SwiftMemberScope scope)
    {
        if (!scope.ProvidesDefaultImplementations)
        {
            owner.Functions.Add(function);
            return;
        }

        function.HasDefaultImplementation = true;
        var requirement = owner.Functions.Find(f => f.Name == function.Name);
        if (requirement == null)
        {
            owner.Functions.Add(function);
            return;
        }

        requirement.HasDefaultImplementation = true;
    }

    private void AddProperty(SwiftType owner, SwiftProperty property, SwiftMemberScope scope)
    {
        if (!scope.ProvidesDefaultImplementations)
        {
            owner.Properties.Add(property);
            return;
        }

        property.HasDefaultImplementation = true;
        var requirement = owner.Properties.Find(p => p.Name == property.Name);
        if (requirement == null)
        {
            owner.Properties.Add(property);
            return;
        }

        requirement.HasDefaultImplementation = true;
    }

    private void AddGlobal(
        SwiftSourceFile file,
        SwiftSourceDeclaration declaration,
        string access)
    {
        var header = declaration.Header;
        switch (header.Keyword)
        {
            case "func" when !SwiftText.IsIdentifier(header.Name):
                this._module.Operators.Add(SwiftSourceFactory.CreateOperator(
                    header, access, this._operatorDeclarations.GetValueOrDefault(header.Name)));
                return;

            case "func":
                this._module.GlobalFunctions.Add(SwiftSourceFactory.CreateFunction(
                    header, ownerPath: string.Empty, access, extensionConstraints: string.Empty));
                return;

            case "var" or "let":
                this._module.GlobalVariables.Add(SwiftSourceFactory.CreateProperty(
                    header, ownerPath: string.Empty, access, file.ReadBodyText(declaration.Block)));
                return;

            case "typealias":
                this._module.TypeAliases.Add(
                    SwiftSourceFactory.CreateTypeAlias(header, ownerPath: string.Empty, access));
                return;
        }
    }

    /// <summary>
    /// SWM-02 - an extension of a type this module declares is folded into that type, because that
    /// is how a Swift developer reads it. An extension of anything else is its own entity, keyed
    /// by what it extends and by the constraints it extends it under.
    /// </summary>
    private void AddExtension(SwiftSourceFile file, SwiftSourceDeclaration declaration)
    {
        var header = declaration.Header;
        if (!declaration.Block.HasBody)
        {
            return;
        }

        var access = header.GetAccessLevel("internal");
        var constraints = SwiftGenericsText.ReadConstraints(header.WhereClause);
        var extended = this.FindExtendedType(header.Name);

        // An extension of a type this module keeps to itself is as internal as the type is,
        // whatever its members say. Recording it would put an internal type's name in the baseline
        // as though it belonged to somebody else.
        if (extended == null && this.IsInternalToThisModule(header.Name))
        {
            return;
        }

        if (extended != null)
        {
            foreach (var entry in SwiftText.SplitTopLevel(header.Inheritance, ','))
            {
                this.AddConformance(extended.Conformances, entry);
            }

            this.AddMembers(file, declaration.Block, extended, new SwiftMemberScope
            {
                OwnerPath = extended.Name,
                DefaultAccess = IsVisible(access) ? access : "internal",
                ExtensionConstraints = constraints,
                ProvidesDefaultImplementations = extended is SwiftProtocol
            });
            return;
        }

        this.AddForeignExtension(file, declaration, header, access, constraints);
    }

    private void AddForeignExtension(
        SwiftSourceFile file,
        SwiftSourceDeclaration declaration,
        SwiftDeclarationHeader header,
        string access,
        string constraints)
    {
        var extension = new SwiftExtension
        {
            ExtendedType = header.Name,
            Constraints = constraints
        };

        foreach (var entry in SwiftText.SplitTopLevel(header.Inheritance, ','))
        {
            this.AddConformance(extension.AddedConformances, entry);
        }

        var scope = new SwiftMemberScope
        {
            OwnerPath = GetSimpleName(header.Name),
            DefaultAccess = IsVisible(access) ? access : "internal",
            ExtensionConstraints = constraints
        };

        foreach (var member in file.ReadBody(declaration.Block))
        {
            var memberAccess = member.Header.GetAccessLevel(scope.DefaultAccess);
            if (!IsVisible(memberAccess))
            {
                continue;
            }

            AddForeignExtensionMember(file, member, extension, scope, memberAccess);
        }

        if (extension.Functions.Count < 1
            && extension.Properties.Count < 1
            && extension.Subscripts.Count < 1
            && extension.AddedConformances.Count < 1)
        {
            return;
        }

        this.Merge(extension);
    }

    private static void AddForeignExtensionMember(
        SwiftSourceFile file,
        SwiftSourceDeclaration declaration,
        SwiftExtension extension,
        SwiftMemberScope scope,
        string access)
    {
        var header = declaration.Header;
        switch (header.Keyword)
        {
            case "func":
                extension.Functions.Add(SwiftSourceFactory.CreateFunction(
                    header, scope.OwnerPath, access, scope.ExtensionConstraints));
                return;

            case "var" or "let":
                extension.Properties.Add(SwiftSourceFactory.CreateProperty(
                    header, scope.OwnerPath, access, file.ReadBodyText(declaration.Block)));
                return;

            case "subscript":
                extension.Subscripts.Add(SwiftSourceFactory.CreateSubscript(
                    header, scope.OwnerPath, access, file.ReadBodyText(declaration.Block)));
                return;
        }
    }

    /// <summary>
    /// Several extensions of the same type under the same constraints are one entity, because
    /// that is what they are to a caller and because whether they were written as one or as three
    /// is not an API decision.
    /// </summary>
    private void Merge(SwiftExtension extension)
    {
        var existing = this._module.Extensions.Find(e => e.Key == extension.Key);
        if (existing == null)
        {
            this._module.Extensions.Add(extension);
            return;
        }

        existing.Functions.AddRange(extension.Functions);
        existing.Properties.AddRange(extension.Properties);
        existing.Subscripts.AddRange(extension.Subscripts);
        existing.AddedConformances.AddRange(extension.AddedConformances);
    }

    /// <summary>
    /// The type an extension names, if this module declares it. Written qualified or not - both
    /// "extension Point" and "extension Widgets.Point" name the same type - and with generic
    /// arguments that identify the type rather than distinguish it.
    /// </summary>
    private SwiftType? FindExtendedType(string reference)
    {
        var withoutGenerics = StripGenericArguments(reference);
        if (this._typesByName.TryGetValue(withoutGenerics, out var qualified))
        {
            return qualified;
        }

        var moduleQualified = this._module.Name + ".";
        return withoutGenerics.StartsWith(moduleQualified, StringComparison.Ordinal)
               && this._typesByName.TryGetValue(withoutGenerics[moduleQualified.Length..], out var stripped)
            ? stripped
            : null;
    }

    private bool IsDeclaredProtocol(string name)
    {
        return this._typesByName.TryGetValue(StripGenericArguments(name), out var type)
               && type is SwiftProtocol;
    }

    /// <summary>
    /// A name already taken is a declaration seen twice - the two halves of an "#if/#else", which
    /// are both read because neither can be evaluated without the build configuration.
    /// </summary>
    private bool Register(SwiftType type)
    {
        if (!this._typesByName.TryAdd(type.Name, type))
        {
            return false;
        }

        this._module.Add(type);
        return true;
    }

    private static bool IsRawValueType(string entry, SwiftEnum enumeration)
    {
        if (RawValueTypes.Contains(entry))
        {
            return true;
        }

        foreach (var declared in enumeration.Cases)
        {
            if (declared.RawValue.Length > 0)
            {
                return true;
            }
        }

        return false;
    }

    private static string GetOperatorKind(SwiftDeclarationHeader header)
    {
        foreach (var kind in (string[])["prefix", "postfix", "infix"])
        {
            if (header.HasModifier(kind))
            {
                return kind;
            }
        }

        return string.Empty;
    }

    private static string GetSimpleName(string reference)
    {
        var withoutGenerics = StripGenericArguments(reference);
        var lastDot = withoutGenerics.LastIndexOf('.');
        return lastDot < 0 ? withoutGenerics : withoutGenerics[(lastDot + 1)..];
    }

    private static string StripGenericArguments(string reference)
    {
        var open = reference.IndexOf('<');
        return open < 0 ? reference.Trim() : reference[..open].Trim();
    }

    /// <summary>
    /// SWE-02 - a conformance to a protocol this module keeps to itself is not something a caller
    /// outside it can see or rely on, so it is not part of the surface.
    /// </summary>
    private void AddConformance(List<string> conformances, string entry)
    {
        var name = SwiftHeaderCursor.Collapse(entry);
        if (name.Length < 1 || conformances.Contains(name) || this.IsInternalToThisModule(name))
        {
            return;
        }

        conformances.Add(name);
    }

    /// <summary>
    /// Whether a written name refers to something this module declares and does not export. The
    /// dotted prefixes are checked as well as the whole name: a type nested inside an internal one
    /// is never walked, so "Outer.Inner" is only recognisable by "Outer".
    /// </summary>
    private bool IsInternalToThisModule(string reference)
    {
        var name = StripGenericArguments(reference);
        if (this._internalTypeNames.Contains(name))
        {
            return true;
        }

        for (var dot = name.IndexOf('.'); dot > 0; dot = name.IndexOf('.', dot + 1))
        {
            if (this._internalTypeNames.Contains(name[..dot]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsVisible(string access)
    {
        return access is SwiftAccessLevels.Public or SwiftAccessLevels.Open;
    }
}
