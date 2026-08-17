namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// An "infix operator &lt;~&gt; : AdditionPrecedence" line. It declares how the operator parses,
/// not what it does, so it is collected while reading a file and then attached to the function
/// that actually implements the operator.
/// </summary>
[DebuggerDisplay("{Kind} operator {Name}")]
internal class SwiftOperatorDeclaration
{
    internal required string Name { get; init; }

    /// <summary>"prefix", "infix" or "postfix" - whichever modifier the declaration was written with.</summary>
    internal required string Kind { get; init; }

    internal required string PrecedenceGroup { get; init; }
}
