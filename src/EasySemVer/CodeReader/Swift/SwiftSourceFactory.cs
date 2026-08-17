using Winterborn.Tools.EasySemVer.DataObject.Swift;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// Turns one parsed header into the modelled Swift entity it declares (SWM-01). Every facet comes
/// from a keyword, an attribute or a piece of punctuation that was actually written - nothing is
/// inferred from naming conventions.
/// </summary>
internal static class SwiftSourceFactory
{
    /// <summary>
    /// Accessors that mean a property can be written to. A stored "var" has none of them written
    /// and is settable anyway, which is why the absence of a body is checked before this list is.
    /// </summary>
    private static readonly string[] SettableAccessors = ["set", "willSet", "didSet", "_modify"];

    internal static SwiftType? CreateType(SwiftDeclarationHeader header, string ownerPath, string access)
    {
        SwiftType? type = header.Keyword switch
        {
            "class" => new SwiftClass(),
            "struct" => new SwiftStruct(),
            "actor" => new SwiftActor(),
            "enum" => new SwiftEnum(),
            "protocol" => new SwiftProtocol(),
            _ => null
        };

        if (type == null)
        {
            return null;
        }

        Describe(type, header, SwiftSignatureName.Qualify(ownerPath, header.Name), access);
        type.IsFinal = header.HasModifier("final");
        type.IsFrozen = header.HasAttribute("@frozen");
        type.GenericParameters = SwiftGenericsText.ReadParameters(
            header.GenericList,
            header.WhereClause);
        return type;
    }

    internal static SwiftFunction CreateFunction(
        SwiftDeclarationHeader header,
        string ownerPath,
        string access,
        string extensionConstraints)
    {
        var parameters = SwiftParameterList.Read(header.ParameterList);
        var function = new SwiftFunction
        {
            ReturnType = header.ReturnType,
            IsStatic = header.HasModifier("static") || header.HasModifier("class"),
            IsMutating = header.HasModifier("mutating"),
            IsAsync = header.IsAsync,
            Throws = header.Throws,
            IsFinal = header.HasModifier("final"),
            ExtensionConstraints = extensionConstraints,
            GenericParameters = SwiftGenericsText.ReadParameters(
                header.GenericList,
                header.WhereClause),
            Parameters = parameters
        };

        var name = SwiftSignatureName.ForCallable(header.Name, parameters, header.HasParameterList);
        Describe(function, header, SwiftSignatureName.Qualify(ownerPath, name), access);
        return function;
    }

    internal static SwiftInitializer CreateInitializer(
        SwiftDeclarationHeader header,
        string ownerPath,
        string access)
    {
        var parameters = SwiftParameterList.Read(header.ParameterList);
        var initializer = new SwiftInitializer
        {
            IsFailable = header.IsFailable,
            IsRequired = header.HasModifier("required"),
            IsConvenience = header.HasModifier("convenience"),
            IsAsync = header.IsAsync,
            Throws = header.Throws,
            Parameters = parameters
        };

        var name = SwiftSignatureName.ForCallable("init", parameters, hasParameterList: true);
        Describe(initializer, header, SwiftSignatureName.Qualify(ownerPath, name), access);
        return initializer;
    }

    /// <summary>
    /// A property, with settability read from what was written rather than from what the compiler
    /// would work out: "let" and a get-only accessor block are the two ways of saying no, and a
    /// "private(set)" says no to everyone outside the type, which is everyone this file models.
    /// </summary>
    internal static SwiftProperty CreateProperty(
        SwiftDeclarationHeader header,
        string ownerPath,
        string access,
        string accessorBlock)
    {
        var property = new SwiftProperty
        {
            Type = header.DeclaredType,
            IsSettable = IsSettable(header, accessorBlock),
            IsStatic = header.HasModifier("static") || header.HasModifier("class"),
            IsMutating = header.HasModifier("mutating"),
            IsAsync = SwiftText.ContainsTopLevelWord(accessorBlock, "async"),
            Throws = SwiftText.ContainsTopLevelWord(accessorBlock, "throws")
        };

        Describe(property, header, SwiftSignatureName.Qualify(ownerPath, header.Name), access);
        return property;
    }

    internal static SwiftSubscript CreateSubscript(
        SwiftDeclarationHeader header,
        string ownerPath,
        string access,
        string accessorBlock)
    {
        var parameters = SwiftParameterList.Read(header.ParameterList, labelsAreOmitted: true);
        var declared = new SwiftSubscript
        {
            ReturnType = header.ReturnType,
            IsSettable = IsSettable(header, accessorBlock),
            IsStatic = header.HasModifier("static") || header.HasModifier("class"),
            Parameters = parameters
        };

        var name = SwiftSignatureName.ForCallable("subscript", parameters, hasParameterList: true);
        Describe(declared, header, SwiftSignatureName.Qualify(ownerPath, name), access);
        return declared;
    }

    /// <summary>
    /// "case red, green" is two cases in one declaration, so this returns a list. Only the last of
    /// them can carry a raw value or associated values, because the others end at their comma.
    /// </summary>
    internal static List<SwiftEnumCase> CreateEnumCases(
        SwiftDeclarationHeader header,
        string headerText,
        string ownerPath,
        string access)
    {
        var cases = new List<SwiftEnumCase>();
        foreach (var piece in SplitCases(headerText))
        {
            // Re-parsed one case at a time for its own name and values, but described from the
            // declaration's header: an "@available" written above "case a, b" applies to both.
            var caseHeader = SwiftDeclarationHeader.Parse("case " + piece);
            var associated = SwiftParameterList.Read(caseHeader.ParameterList);
            var declared = new SwiftEnumCase
            {
                AssociatedValues = associated,
                RawValue = caseHeader.Initialiser
            };

            var name = SwiftSignatureName.ForCallable(
                caseHeader.Name,
                associated,
                caseHeader.HasParameterList);
            Describe(declared, header, SwiftSignatureName.Qualify(ownerPath, name), access);
            cases.Add(declared);
        }

        return cases;
    }

    internal static SwiftTypeAlias CreateTypeAlias(
        SwiftDeclarationHeader header,
        string ownerPath,
        string access)
    {
        var alias = new SwiftTypeAlias { UnderlyingType = header.DeclaredType };
        Describe(alias, header, SwiftSignatureName.Qualify(ownerPath, header.Name), access);
        return alias;
    }

    /// <summary>
    /// The operator function, not the "operator" declaration: what a caller writes is the function,
    /// and the declaration only says how it parses. Its kind comes from that declaration when there
    /// is one, and otherwise from the only thing the function itself says - how many operands it
    /// takes.
    /// </summary>
    internal static SwiftOperator CreateOperator(
        SwiftDeclarationHeader header,
        string access,
        SwiftOperatorDeclaration? declaration)
    {
        var parameters = SwiftParameterList.Read(header.ParameterList);
        var declared = new SwiftOperator
        {
            OperatorKind = declaration?.Kind ?? (parameters.Count == 2 ? "infix" : string.Empty),
            PrecedenceGroup = declaration?.PrecedenceGroup ?? string.Empty
        };

        Describe(
            declared,
            header,
            SwiftSignatureName.ForCallable(header.Name, parameters, header.HasParameterList),
            access);
        return declared;
    }

    /// <summary>
    /// A nested typealias is part of the owning type's surface, and is recorded as a get-only
    /// property of its underlying type so that it still diffs.
    /// </summary>
    internal static SwiftProperty CreateNestedAlias(
        SwiftDeclarationHeader header,
        string ownerPath,
        string access)
    {
        var property = new SwiftProperty { Type = header.DeclaredType };
        Describe(property, header, SwiftSignatureName.Qualify(ownerPath, header.Name), access);
        return property;
    }

    private static bool IsSettable(SwiftDeclarationHeader header, string accessorBlock)
    {
        if (header.Keyword == "let")
        {
            return false;
        }

        foreach (var modifier in header.DeclarationModifiers)
        {
            // "public private(set) var" is readable everywhere and writable nowhere out here.
            if (modifier.EndsWith("(set)", StringComparison.Ordinal)
                && !modifier.StartsWith(SwiftAccessLevels.Public, StringComparison.Ordinal)
                && !modifier.StartsWith(SwiftAccessLevels.Open, StringComparison.Ordinal))
            {
                return false;
            }
        }

        if (accessorBlock.Length < 1)
        {
            return true;
        }

        foreach (var accessor in SettableAccessors)
        {
            if (SwiftText.ContainsTopLevelWord(accessorBlock, accessor))
            {
                return true;
            }
        }

        return false;
    }

    private static List<string> SplitCases(string headerText)
    {
        var withoutKeyword = headerText.TrimStart();
        withoutKeyword = withoutKeyword.StartsWith("case", StringComparison.Ordinal)
            ? withoutKeyword["case".Length..]
            : withoutKeyword;
        return SwiftText.SplitTopLevel(withoutKeyword, ',');
    }

    private static void Describe(
        SwiftDeclaration declaration,
        SwiftDeclarationHeader header,
        string name,
        string access)
    {
        declaration.Name = name;
        declaration.AccessLevel = access;
        declaration.ObjCExposure = header.GetObjCExposure();
        declaration.Availability = SwiftAvailabilityText.Read(header.Attributes);
    }
}
