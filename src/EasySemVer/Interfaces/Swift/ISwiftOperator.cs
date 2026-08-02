namespace Winterborn.Library.EasySemVer.Interfaces.Swift;

public interface ISwiftOperator : ISwiftDeclaration
{
    /// <summary>"infix" | "prefix" | "postfix", as far as the declaration reveals.</summary>
    public string OperatorKind { get; }

    public string PrecedenceGroup { get; }
}
