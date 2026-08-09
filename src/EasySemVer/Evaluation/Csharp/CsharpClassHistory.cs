using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluation.Csharp;

/// <inheritdoc cref="ICsharpClassHistory"/>
[DebuggerDisplay("{Newer.Kind} {Newer.Name}")]
internal class CsharpClassHistory(ICsharpType older, ICsharpType newer) : ICsharpClassHistory
{
    public ICsharpType Older { get; } = older;

    public ICsharpType Newer { get; } = newer;
}
