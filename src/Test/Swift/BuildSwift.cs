using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Test.Swift;

/// <summary>
/// Hand-built Swift signature graphs for the rule tests (TST-M1). Never built from live
/// extraction, so a rule test failing means the rule is wrong and nothing else.
/// </summary>
internal static class BuildSwift
{
    internal const string DefaultTypeName = "TestType";

    internal static ISwiftSignaturesToCompare Compare(SwiftModule older, SwiftModule newer)
    {
        return new SwiftSignaturesToCompare(older, newer);
    }

    internal static ISwiftSignaturesToCompare Compare(SwiftType older, SwiftType newer)
    {
        return Compare(Module(older), Module(newer));
    }

    internal static SwiftModule Module(params SwiftType[] types)
    {
        var module = new SwiftModule("Widgets");
        foreach (var type in types)
        {
            module.Add(type);
        }

        return module;
    }

    internal static SwiftClass Class(string name = DefaultTypeName, string accessLevel = "public")
    {
        return new SwiftClass { Name = name, AccessLevel = accessLevel };
    }

    internal static SwiftStruct Struct(string name = DefaultTypeName)
    {
        return new SwiftStruct { Name = name };
    }

    internal static SwiftActor Actor(string name = DefaultTypeName)
    {
        return new SwiftActor { Name = name };
    }

    internal static SwiftEnum Enum(string name = DefaultTypeName)
    {
        return new SwiftEnum { Name = name };
    }

    internal static SwiftProtocol Protocol(string name = DefaultTypeName)
    {
        return new SwiftProtocol { Name = name };
    }

    internal static SwiftFunction Function(string name = "TestType.move()", string returns = "()")
    {
        return new SwiftFunction { Name = name, ReturnType = returns };
    }

    internal static SwiftProperty Property(string name = "TestType.speed", string type = "Int")
    {
        return new SwiftProperty { Name = name, Type = type };
    }

    internal static SwiftInitializer Initializer(string name = "TestType.init()")
    {
        return new SwiftInitializer { Name = name };
    }

    internal static SwiftSubscript Subscript(string name = "TestType.subscript(_:)")
    {
        return new SwiftSubscript { Name = name };
    }

    internal static SwiftEnumCase Case(string name, string rawValue = "")
    {
        return new SwiftEnumCase { Name = name, RawValue = rawValue };
    }

    internal static SwiftParameter Parameter(
        string label = "to",
        string type = "Point",
        bool hasDefault = false)
    {
        return new SwiftParameter { Label = label, Type = type, HasDefault = hasDefault };
    }

    internal static SwiftGenericParameter Generic(string name = "T", string constraints = "")
    {
        return new SwiftGenericParameter { Name = name, Constraints = constraints };
    }

    internal static SwiftAvailability Available(
        string domain = "*",
        bool isDeprecated = false,
        bool isUnavailable = false,
        string obsoleted = "")
    {
        return new SwiftAvailability
        {
            Domain = domain,
            IsDeprecated = isDeprecated,
            IsUnavailable = isUnavailable,
            Obsoleted = obsoleted
        };
    }

    internal static SwiftOperator Operator(
        string name = "<~>(_:_:)",
        string precedenceGroup = "AdditionPrecedence")
    {
        return new SwiftOperator { Name = name, PrecedenceGroup = precedenceGroup };
    }

    internal static T WithFunctions<T>(this T type, params SwiftFunction[] functions)
    where T : SwiftType
    {
        type.Functions.AddRange(functions);
        return type;
    }

    internal static T WithProperties<T>(this T type, params SwiftProperty[] properties)
    where T : SwiftType
    {
        type.Properties.AddRange(properties);
        return type;
    }

    internal static T WithInitializers<T>(this T type, params SwiftInitializer[] initializers)
    where T : SwiftType
    {
        type.Initializers.AddRange(initializers);
        return type;
    }

    internal static T WithSubscripts<T>(this T type, params SwiftSubscript[] subscripts)
    where T : SwiftType
    {
        type.Subscripts.AddRange(subscripts);
        return type;
    }

    internal static T WithConformances<T>(this T type, params string[] conformances)
    where T : SwiftType
    {
        type.Conformances.AddRange(conformances);
        return type;
    }

    internal static T WithGenerics<T>(this T type, params SwiftGenericParameter[] parameters)
    where T : SwiftType
    {
        type.GenericParameters.AddRange(parameters);
        return type;
    }

    internal static SwiftEnum WithCases(this SwiftEnum enumeration, params SwiftEnumCase[] cases)
    {
        enumeration.Cases.AddRange(cases);
        return enumeration;
    }

    internal static SwiftProtocol WithAssociatedTypes(
        this SwiftProtocol protocolType,
        params string[] names)
    {
        protocolType.AssociatedTypes.AddRange(names);
        return protocolType;
    }

    internal static SwiftFunction WithParameters(
        this SwiftFunction function,
        params SwiftParameter[] parameters)
    {
        function.Parameters.AddRange(parameters);
        return function;
    }

    internal static SwiftFunction WithGenerics(
        this SwiftFunction function,
        params SwiftGenericParameter[] parameters)
    {
        function.GenericParameters.AddRange(parameters);
        return function;
    }

    internal static T WithAvailability<T>(this T declaration, params SwiftAvailability[] availability)
    where T : SwiftDeclaration
    {
        declaration.Availability.AddRange(availability);
        return declaration;
    }

    internal static T WithObjC<T>(this T declaration, string exposure = "@objc")
    where T : SwiftDeclaration
    {
        declaration.ObjCExposure = exposure;
        return declaration;
    }

    internal static SwiftModule WithOperators(this SwiftModule module, params SwiftOperator[] operators)
    {
        module.Operators.AddRange(operators);
        return module;
    }
}
