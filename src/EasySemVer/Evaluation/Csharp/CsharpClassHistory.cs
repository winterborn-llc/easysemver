using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluation.Csharp;

internal class CsharpClassHistory(ICsharpClass older, ICsharpClass newer) : ICsharpClassHistory
{
    public ICsharpClass Older { get; } = older;

    public ICsharpClass Newer { get; } = newer;
}