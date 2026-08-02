using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.DataObject;

[DebuggerDisplay("{DebugText}")]
internal class Property : IProperty
{
    private string DebugText
    {
        get
        {
            var get = this.IsReadable ? " get;" : string.Empty;
            var set = this.IsWritable ? " set;" : string.Empty;
            return $"{this.Type} {this.Name} {{{get}{set} }}";
        }
    }
    
    public string Name { get; init; } = string.Empty;
    
    public string Type { get; init; } = string.Empty;

    public bool IsReadable { get; init; }
    
    public bool IsWritable { get; init; }
}