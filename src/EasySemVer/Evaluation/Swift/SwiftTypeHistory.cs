using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluation.Swift;

/// <inheritdoc cref="ISwiftTypeHistory"/>
[DebuggerDisplay("{Newer.Kind} {Newer.Name}")]
internal class SwiftTypeHistory(ISwiftType older, ISwiftType newer) : ISwiftTypeHistory
{
    public ISwiftType Older { get; } = older;

    public ISwiftType Newer { get; } = newer;
}
