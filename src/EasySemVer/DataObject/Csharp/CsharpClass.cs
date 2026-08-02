using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("{Name}")]
internal class CsharpClass : ICsharpClass
{
    public string Name { get; init; } = string.Empty;

    public ICsharpMethodList Methods { get; init; } = new CsharpMethodList();

    public ICsharpPropertyList Properties { get; init; } = new CsharpPropertyList();
}