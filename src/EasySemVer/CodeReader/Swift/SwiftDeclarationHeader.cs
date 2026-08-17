using Winterborn.Tools.EasySemVer.DataObject.Swift;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// One declaration's header, taken apart into the pieces the signature model needs: its
/// attributes, its modifiers, the keyword that says what it is, and whatever that keyword implies
/// - a parameter list, a return type, an inheritance clause.
/// <para>
/// This reads a declaration, not a language. Anything it does not recognise stays as the text that
/// was written, which is stable run to run and is the same bargain the C# reader strikes when it
/// leaves an unresolved type as its written name (SIG-01, G-16).
/// </para>
/// </summary>
[DebuggerDisplay("{Keyword} {Name}")]
internal class SwiftDeclarationHeader
{
    private static readonly string[] TypeKeywords =
        ["class", "struct", "enum", "protocol", "actor"];

    internal IReadOnlyList<string> Attributes { get; private set; } = [];

    internal IReadOnlyList<string> DeclarationModifiers { get; private set; } = [];

    internal string Keyword { get; private set; } = string.Empty;

    /// <summary>The declaration's own name, without argument labels and without backticks.</summary>
    internal string Name { get; private set; } = string.Empty;

    internal string GenericList { get; private set; } = string.Empty;

    internal string ParameterList { get; private set; } = string.Empty;

    /// <summary>
    /// Whether a parameter list was written at all. "case red" and "case red()" are different
    /// declarations, and an empty list does not say which one this was.
    /// </summary>
    internal bool HasParameterList { get; private set; }

    internal string ReturnType { get; private set; } = string.Empty;

    /// <summary>The inheritance clause of a type, or the conformances an extension adds.</summary>
    internal string Inheritance { get; private set; } = string.Empty;

    internal string WhereClause { get; private set; } = string.Empty;

    /// <summary>A property's written type, or the right-hand side of a typealias.</summary>
    internal string DeclaredType { get; private set; } = string.Empty;

    /// <summary>The literal after "=": an enum case's raw value, a property's default.</summary>
    internal string Initialiser { get; private set; } = string.Empty;

    internal bool IsFailable { get; private set; }

    internal bool IsAsync { get; private set; }

    internal bool Throws { get; private set; }

    internal bool IsTypeDeclaration => TypeKeywords.Contains(this.Keyword);

    internal bool HasModifier(string modifier)
    {
        return this.DeclarationModifiers.Contains(modifier);
    }

    /// <summary>
    /// SWE-02 - the access level as written, or the one inherited from where the declaration sits.
    /// A member writes nothing far more often than it writes "internal", and the two mean the same
    /// thing everywhere except inside a protocol and inside an access-modified extension, which is
    /// why the inherited level is the caller's to supply.
    /// </summary>
    internal string GetAccessLevel(string inherited)
    {
        foreach (var modifier in this.DeclarationModifiers)
        {
            switch (modifier)
            {
                case SwiftAccessLevels.Open or SwiftAccessLevels.Public:
                case "package" or "internal" or "fileprivate" or "private":
                    return modifier;
            }
        }

        return inherited;
    }

    /// <summary>SWM-04 - "@objc" or "@objc(CustomName)" exactly as written.</summary>
    internal string GetObjCExposure()
    {
        foreach (var attribute in this.Attributes)
        {
            if (attribute.StartsWith("@objc", StringComparison.Ordinal))
            {
                return attribute;
            }
        }

        return string.Empty;
    }

    internal bool HasAttribute(string name)
    {
        foreach (var attribute in this.Attributes)
        {
            if (attribute == name || attribute.StartsWith(name + "(", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    internal static SwiftDeclarationHeader Parse(string header)
    {
        var cursor = new SwiftHeaderCursor(header);
        var parsed = new SwiftDeclarationHeader
        {
            Attributes = cursor.ReadAttributes(),
            DeclarationModifiers = cursor.ReadModifiers()
        };

        parsed.Keyword = cursor.ReadKeyword();
        parsed.ReadRest(cursor);
        return parsed;
    }

    private void ReadRest(SwiftHeaderCursor cursor)
    {
        switch (this.Keyword)
        {
            case "func":
                this.ReadFunction(cursor);
                return;

            case "init":
                this.ReadInitializer(cursor);
                return;

            case "subscript":
                this.ReadSubscript(cursor);
                return;

            case "var" or "let":
                this.ReadProperty(cursor);
                return;

            case "typealias" or "associatedtype":
                this.ReadAlias(cursor);
                return;

            case "case":
                this.ReadEnumCase(cursor);
                return;

            case "extension":
                this.ReadExtension(cursor);
                return;

            case "operator":
                this.ReadOperatorDeclaration(cursor);
                return;

            default:
                if (this.IsTypeDeclaration)
                {
                    this.ReadType(cursor);
                }

                return;
        }
    }

    private void ReadFunction(SwiftHeaderCursor cursor)
    {
        this.Name = cursor.ReadDeclarationName();
        this.GenericList = cursor.ReadBracketed('<');
        this.ParameterList = cursor.ReadBracketed('(');
        this.HasParameterList = cursor.ConsumedBrackets;
        this.ReadEffects(cursor);
    }

    private void ReadInitializer(SwiftHeaderCursor cursor)
    {
        this.Name = "init";
        this.IsFailable = cursor.ConsumeFailableMarker();
        this.GenericList = cursor.ReadBracketed('<');
        this.ParameterList = cursor.ReadBracketed('(');
        this.HasParameterList = true;
        this.ReadEffects(cursor);
    }

    private void ReadSubscript(SwiftHeaderCursor cursor)
    {
        this.Name = "subscript";
        this.GenericList = cursor.ReadBracketed('<');
        this.ParameterList = cursor.ReadBracketed('(');
        this.HasParameterList = true;
        this.ReadEffects(cursor);
    }

    private void ReadProperty(SwiftHeaderCursor cursor)
    {
        this.Name = cursor.ReadIdentifier();
        this.DeclaredType = cursor.ReadTypeAnnotation();
        this.Initialiser = cursor.ReadInitialiser();
    }

    private void ReadAlias(SwiftHeaderCursor cursor)
    {
        this.Name = cursor.ReadIdentifier();
        this.GenericList = cursor.ReadBracketed('<');
        this.Inheritance = cursor.ReadTypeAnnotation();

        // "typealias A = B" carries its type after the "="; "associatedtype A: P" after the colon.
        // Whichever was written is the one that describes the declaration.
        var assigned = cursor.ReadInitialiser();
        this.DeclaredType = assigned.Length > 0 ? assigned : this.Inheritance;
    }

    private void ReadEnumCase(SwiftHeaderCursor cursor)
    {
        this.Name = cursor.ReadIdentifier();
        this.ParameterList = cursor.ReadBracketed('(');
        this.HasParameterList = cursor.ConsumedBrackets;
        this.Initialiser = cursor.ReadInitialiser();
    }

    private void ReadType(SwiftHeaderCursor cursor)
    {
        this.Name = cursor.ReadIdentifier();
        this.GenericList = cursor.ReadBracketed('<');
        this.Inheritance = cursor.ReadTypeAnnotation();
        this.WhereClause = cursor.ReadWhereClause();
    }

    /// <summary>
    /// An extension names a type rather than declaring one, and that name can be qualified or
    /// generic - "extension Swift.Array&lt;Element&gt;". All of it is the extended type.
    /// </summary>
    private void ReadExtension(SwiftHeaderCursor cursor)
    {
        this.Name = cursor.ReadTypeReference();
        this.Inheritance = cursor.ReadTypeAnnotation();
        this.WhereClause = cursor.ReadWhereClause();
    }

    /// <summary>"infix operator &lt;~&gt; : AdditionPrecedence" - the name is the operator itself.</summary>
    private void ReadOperatorDeclaration(SwiftHeaderCursor cursor)
    {
        this.Name = cursor.ReadDeclarationName();
        this.DeclaredType = cursor.ReadTypeAnnotation();
    }

    private void ReadEffects(SwiftHeaderCursor cursor)
    {
        var effects = cursor.ReadEffectsAndReturnType();
        this.ReturnType = effects.ReturnType;
        this.WhereClause = effects.WhereClause;

        // "async" and "throws" can also be written as modifiers on a protocol requirement's
        // accessor, so a modifier already read counts as much as one found after the parameters.
        this.IsAsync = effects.IsAsync || this.HasModifier("async");
        this.Throws = effects.Throws || this.HasModifier("throws") || this.HasModifier("rethrows");
    }
}
