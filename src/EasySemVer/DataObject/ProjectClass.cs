using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.DataObject;

[DebuggerDisplay("{Name}")]
internal class ProjectClass : IProjectClass
{
    public string Name { get; init; } = string.Empty;

    public IMethodList Methods { get; init; } = new MethodList();

    public IPropertyList Properties { get; init; } = new PropertyList();
}