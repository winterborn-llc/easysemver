using Winterborn.Tools.EasySemVer.DataObject.Swift;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// Turns one symbol-graph entry into the modelled Swift entity it represents (SWM-01). Every
/// facet comes from a field or a declaration keyword the toolchain emitted - nothing is inferred
/// from naming conventions.
/// </summary>
internal static class SwiftSymbolFactory
{
    internal static SwiftType? CreateType(SymbolGraphSymbol symbol)
    {
        SwiftType? type = symbol.Kind switch
        {
            // Actors are reported as classes; the declaration keyword is what tells them apart.
            SymbolGraphKinds.Class when symbol.HasKeyword("actor") => new SwiftActor(),
            SymbolGraphKinds.Class => new SwiftClass(),
            SymbolGraphKinds.Struct => new SwiftStruct(),
            SymbolGraphKinds.Enum => new SwiftEnum(),
            SymbolGraphKinds.Protocol => new SwiftProtocol(),
            _ => null
        };

        if (type == null)
        {
            return null;
        }

        Describe(type, symbol);
        type.IsFinal = symbol.HasKeyword("final");
        type.IsFrozen = symbol.Attributes.Contains("@frozen");
        type.GenericParameters = symbol.GenericParameters;
        return type;
    }

    internal static void AddMember(
        SwiftType owner,
        SymbolGraphSymbol symbol,
        Dictionary<string, SymbolGraphSymbol> byUsr)
    {
        switch (symbol.Kind)
        {
            case SymbolGraphKinds.Initializer:
                owner.Initializers.Add(CreateInitializer(symbol));
                return;

            case SymbolGraphKinds.Method or SymbolGraphKinds.TypeMethod
                or SymbolGraphKinds.Function or SymbolGraphKinds.Operator:
                owner.Functions.Add(CreateFunction(symbol));
                return;

            case SymbolGraphKinds.Property or SymbolGraphKinds.TypeProperty or SymbolGraphKinds.Variable:
                owner.Properties.Add(CreateProperty(symbol));
                return;

            case SymbolGraphKinds.Subscript or SymbolGraphKinds.TypeSubscript:
                owner.Subscripts.Add(CreateSubscript(symbol));
                return;

            case SymbolGraphKinds.EnumCase when owner is SwiftEnum enumeration:
                enumeration.Cases.Add(CreateEnumCase(symbol));
                return;

            case SymbolGraphKinds.AssociatedType when owner is SwiftProtocol protocolType:
                protocolType.AssociatedTypes.Add(symbol.LocalName);
                return;

            case SymbolGraphKinds.TypeAlias:
                // A nested typealias is part of the owning type's surface; it is recorded as a
                // get-only property of its underlying type so it still diffs.
                owner.Properties.Add(CreateProperty(symbol));
                return;
        }
    }

    internal static void AddMember(SwiftExtension owner, SymbolGraphSymbol symbol)
    {
        switch (symbol.Kind)
        {
            case SymbolGraphKinds.Method or SymbolGraphKinds.TypeMethod
                or SymbolGraphKinds.Function or SymbolGraphKinds.Operator:
                owner.Functions.Add(CreateFunction(symbol));
                return;

            case SymbolGraphKinds.Property or SymbolGraphKinds.TypeProperty or SymbolGraphKinds.Variable:
                owner.Properties.Add(CreateProperty(symbol));
                return;

            case SymbolGraphKinds.Subscript or SymbolGraphKinds.TypeSubscript:
                owner.Subscripts.Add(CreateSubscript(symbol));
                return;
        }
    }

    internal static void AddGlobal(SwiftModule module, SymbolGraphSymbol symbol)
    {
        switch (symbol.Kind)
        {
            case SymbolGraphKinds.Operator:
                module.Operators.Add(CreateOperator(symbol));
                return;

            case SymbolGraphKinds.Function or SymbolGraphKinds.Method:
                module.GlobalFunctions.Add(CreateFunction(symbol));
                return;

            case SymbolGraphKinds.Variable or SymbolGraphKinds.Property:
                module.GlobalVariables.Add(CreateProperty(symbol));
                return;

            case SymbolGraphKinds.TypeAlias:
                module.TypeAliases.Add(CreateTypeAlias(symbol));
                return;
        }
    }

    private static SwiftFunction CreateFunction(SymbolGraphSymbol symbol)
    {
        var function = new SwiftFunction
        {
            ReturnType = symbol.ReturnType,
            IsStatic = symbol.Kind == SymbolGraphKinds.TypeMethod || symbol.HasKeyword("static"),
            IsMutating = symbol.HasKeyword("mutating"),
            IsAsync = symbol.HasKeyword("async"),
            Throws = symbol.HasKeyword("throws") || symbol.HasKeyword("rethrows"),
            IsFinal = symbol.HasKeyword("final"),
            HasDefaultImplementation = symbol.HasDefaultImplementation,
            ExtensionConstraints = symbol.SwiftExtensionConstraints,
            GenericParameters = symbol.GenericParameters,
            Parameters = symbol.Parameters
        };

        Describe(function, symbol);
        return function;
    }

    private static SwiftInitializer CreateInitializer(SymbolGraphSymbol symbol)
    {
        var initializer = new SwiftInitializer
        {
            // "init?(...)" - the question mark rides in the declaration text, not a keyword.
            IsFailable = symbol.Declaration.StartsWith("init?", StringComparison.Ordinal)
                         || symbol.Declaration.StartsWith("init!", StringComparison.Ordinal),
            IsRequired = symbol.HasKeyword("required"),
            IsConvenience = symbol.HasKeyword("convenience"),
            IsAsync = symbol.HasKeyword("async"),
            Throws = symbol.HasKeyword("throws") || symbol.HasKeyword("rethrows"),
            Parameters = symbol.Parameters
        };

        Describe(initializer, symbol);
        return initializer;
    }

    private static SwiftProperty CreateProperty(SymbolGraphSymbol symbol)
    {
        var property = new SwiftProperty
        {
            Type = GetDeclaredType(symbol),

            // The graph spells out the accessors it has: "{ get }" means get-only.
            IsSettable = !symbol.Declaration.Contains("{ get }", StringComparison.Ordinal)
                         && !symbol.Declaration.Contains("{ get throws }", StringComparison.Ordinal)
                         && !symbol.Declaration.Contains("{ get async }", StringComparison.Ordinal)
                         && !symbol.Declaration.Contains("{ get async throws }", StringComparison.Ordinal)
                         && !symbol.Declaration.StartsWith("let ", StringComparison.Ordinal),
            IsStatic = symbol.Kind == SymbolGraphKinds.TypeProperty || symbol.HasKeyword("static")
                       || symbol.HasKeyword("class"),
            IsMutating = symbol.HasKeyword("mutating"),
            IsAsync = symbol.Declaration.Contains("async", StringComparison.Ordinal),
            Throws = symbol.Declaration.Contains("throws", StringComparison.Ordinal),
            HasDefaultImplementation = symbol.HasDefaultImplementation
        };

        Describe(property, symbol);
        return property;
    }

    private static SwiftSubscript CreateSubscript(SymbolGraphSymbol symbol)
    {
        var subscriptDeclaration = new SwiftSubscript
        {
            ReturnType = symbol.ReturnType,
            IsSettable = !symbol.Declaration.Contains("{ get }", StringComparison.Ordinal),
            IsStatic = symbol.Kind == SymbolGraphKinds.TypeSubscript || symbol.HasKeyword("static"),
            Parameters = symbol.Parameters
        };

        Describe(subscriptDeclaration, symbol);
        return subscriptDeclaration;
    }

    private static SwiftEnumCase CreateEnumCase(SymbolGraphSymbol symbol)
    {
        var enumCase = new SwiftEnumCase
        {
            AssociatedValues = symbol.Parameters,
            RawValue = GetRawValue(symbol.Declaration)
        };

        // Associated values are not in functionSignature for cases, so they come from the
        // declaration's own parameter list.
        if (enumCase.AssociatedValues.Count < 1)
        {
            enumCase.AssociatedValues = SwiftEnumCaseText.GetAssociatedValues(symbol.Declaration);
        }

        Describe(enumCase, symbol);
        return enumCase;
    }

    private static SwiftTypeAlias CreateTypeAlias(SymbolGraphSymbol symbol)
    {
        var alias = new SwiftTypeAlias
        {
            UnderlyingType = GetDeclaredType(symbol)
        };

        Describe(alias, symbol);
        return alias;
    }

    private static SwiftOperator CreateOperator(SymbolGraphSymbol symbol)
    {
        var declared = new SwiftOperator
        {
            OperatorKind = GetOperatorKind(symbol),
            PrecedenceGroup = string.Empty
        };

        Describe(declared, symbol);
        return declared;
    }

    private static string GetOperatorKind(SymbolGraphSymbol symbol)
    {
        foreach (var keyword in (string[])["prefix", "postfix", "infix"])
        {
            if (symbol.HasKeyword(keyword))
            {
                return keyword;
            }
        }

        // An operator function with two parameters is infix; the graph does not state it.
        return symbol.Parameters.Count == 2 ? "infix" : string.Empty;
    }

    private static void Describe(SwiftDeclaration declaration, SymbolGraphSymbol symbol)
    {
        declaration.Name = symbol.Path;
        declaration.AccessLevel = symbol.AccessLevel;
        declaration.ObjCExposure = symbol.GetObjCExposure();
        declaration.Availability = symbol.Availability;
    }

    /// <summary>The type after the ": " or "= " in a var, let or typealias declaration.</summary>
    private static string GetDeclaredType(SymbolGraphSymbol symbol)
    {
        var separator = symbol.Declaration.IndexOf(": ", StringComparison.Ordinal);
        if (separator < 0)
        {
            separator = symbol.Declaration.IndexOf("= ", StringComparison.Ordinal);
            if (separator < 0)
            {
                return string.Empty;
            }

            return symbol.Declaration[(separator + 2)..].Trim();
        }

        var type = symbol.Declaration[(separator + 2)..].Trim();
        var accessors = type.IndexOf(" {", StringComparison.Ordinal);
        return accessors < 0 ? type : type[..accessors].Trim();
    }

    private static string GetRawValue(string declaration)
    {
        var separator = declaration.IndexOf(" = ", StringComparison.Ordinal);
        return separator < 0 ? string.Empty : declaration[(separator + 3)..].Trim();
    }
}
