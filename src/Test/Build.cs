using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Evaluation.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test;

/// <summary>
/// Hand-built C# signature graphs for the rule tests (TST-M1). Rules are always tested against
/// constructed graphs, never against live extraction, so a rule failing means the rule is wrong
/// and nothing else.
/// </summary>
internal static class Build
{
    internal const string DefaultTypeName = "Test.TestType";

    internal static ICsharpSignaturesToCompare Compare(CsharpProject older, CsharpProject newer)
    {
        return new CsharpSignaturesToCompare(older, newer);
    }

    /// <summary>The common case: one project, one type, differing only in what the test changed.</summary>
    internal static ICsharpSignaturesToCompare Compare(CsharpType older, CsharpType newer)
    {
        return Compare(Project(older), Project(newer));
    }

    internal static CsharpProject Project(params CsharpType[] types)
    {
        var project = new CsharpProject("Test");
        foreach (var type in types)
        {
            project.Add(type);
        }

        return project;
    }

    internal static CsharpClass Class(string name = DefaultTypeName)
    {
        return new CsharpClass { Name = name };
    }

    internal static CsharpInterface Interface(string name = DefaultTypeName)
    {
        return new CsharpInterface { Name = name };
    }

    internal static CsharpStruct Struct(string name = DefaultTypeName)
    {
        return new CsharpStruct { Name = name };
    }

    internal static CsharpRecord Record(string name = DefaultTypeName)
    {
        return new CsharpRecord { Name = name };
    }

    internal static CsharpEnum Enum(string name = DefaultTypeName, string underlyingType = "int")
    {
        return new CsharpEnum { Name = name, UnderlyingType = underlyingType };
    }

    internal static CsharpDelegate Delegate(string name = DefaultTypeName, string returns = "void")
    {
        return new CsharpDelegate { Name = name, ReturnType = returns };
    }

    internal static CsharpMethod Method(
        string name = "TestMethod",
        string returns = "void",
        params CsharpMethodOverride[] overrides)
    {
        var method = new CsharpMethod { MethodName = name, MethodType = returns };
        if (overrides.Length < 1)
        {
            method.Overrides.Add(new CsharpMethodOverride { ReturnType = returns });
            return method;
        }

        foreach (var methodOverride in overrides)
        {
            if (methodOverride.ReturnType.Length < 1)
            {
                methodOverride.ReturnType = returns;
            }

            method.Overrides.Add(methodOverride);
        }

        return method;
    }

    internal static CsharpMethodOverride Override(params CsharpMethodParameter[] parameters)
    {
        return new CsharpMethodOverride(parameters);
    }

    internal static CsharpMethodParameter Parameter(
        string name = "input",
        string type = "string",
        bool isRequired = true)
    {
        return new CsharpMethodParameter
        {
            ParameterName = name,
            ParameterType = type,
            IsRequired = isRequired
        };
    }

    internal static CsharpProperty Property(
        string name = "TestProperty",
        string type = "string",
        bool isReadable = true,
        bool isWritable = true)
    {
        return new CsharpProperty
        {
            Name = name,
            Type = type,
            IsReadable = isReadable,
            IsWritable = isWritable
        };
    }

    internal static CsharpField Field(string name = "TestField", string type = "string")
    {
        return new CsharpField { Name = name, Type = type };
    }

    internal static CsharpEvent Event(string name = "TestEvent", string handler = "System.EventHandler")
    {
        return new CsharpEvent { Name = name, HandlerType = handler };
    }

    internal static CsharpEnumMember EnumMember(string name, string value = "0")
    {
        return new CsharpEnumMember { Name = name, Value = value };
    }

    internal static CsharpGenericParameter Generic(string name = "T", string constraints = "")
    {
        return new CsharpGenericParameter { Name = name, Constraints = constraints };
    }

    internal static T WithMethods<T>(this T type, params CsharpMethod[] methods)
    where T : CsharpType
    {
        type.Methods.AddRange(methods);
        return type;
    }

    internal static T WithProperties<T>(this T type, params CsharpProperty[] properties)
    where T : CsharpType
    {
        type.Properties.AddRange(properties);
        return type;
    }

    internal static T WithFields<T>(this T type, params CsharpField[] fields)
    where T : CsharpType
    {
        type.Fields.AddRange(fields);
        return type;
    }

    internal static T WithEvents<T>(this T type, params CsharpEvent[] events)
    where T : CsharpType
    {
        type.Events.AddRange(events);
        return type;
    }

    internal static T WithInterfaces<T>(this T type, params string[] interfaceNames)
    where T : CsharpType
    {
        type.ImplementedInterfaces.AddRange(interfaceNames);
        return type;
    }

    internal static T WithGenerics<T>(this T type, params CsharpGenericParameter[] parameters)
    where T : CsharpType
    {
        type.GenericParameters.AddRange(parameters);
        return type;
    }

    internal static CsharpEnum WithMembers(this CsharpEnum enumeration, params CsharpEnumMember[] members)
    {
        enumeration.Members.AddRange(members);
        return enumeration;
    }

    internal static CsharpDelegate WithParameters(
        this CsharpDelegate value,
        params CsharpMethodParameter[] parameters)
    {
        value.Parameters.AddRange(parameters);
        return value;
    }

    internal static CsharpRecord WithPositional(
        this CsharpRecord record,
        params CsharpMethodParameter[] parameters)
    {
        record.PositionalParameters.AddRange(parameters);
        return record;
    }

    internal static CsharpMethodOverride WithGenerics(
        this CsharpMethodOverride methodOverride,
        params CsharpGenericParameter[] parameters)
    {
        methodOverride.GenericParameters.AddRange(parameters);
        return methodOverride;
    }

    internal static CsharpType Nested(this CsharpType type, string declaringType)
    {
        type.DeclaringType = declaringType;
        return type;
    }
}
