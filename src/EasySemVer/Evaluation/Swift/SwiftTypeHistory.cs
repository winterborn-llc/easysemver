using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluation.Swift;

/// <inheritdoc cref="ISwiftTypeHistory"/>
[DebuggerDisplay("{Newer.Kind} {Newer.Name}")]
internal class SwiftTypeHistory(ISwiftType older, ISwiftType newer) : ISwiftTypeHistory
{
    public ISwiftType Older { get; } = older;

    public ISwiftType Newer { get; } = newer;
}
