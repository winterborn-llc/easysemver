using System.Diagnostics;
using System.Text;

namespace Yamamari.AutoVersion;

// Major.Minor[.Build[.Revision]]
[DebuggerDisplay("{ToString()}")]
internal class VersionList
{
    public IList<int> List { get; }

    public Version ToVersion()
    {
        var text = this.ToString();
        var version = new Version(text);
        return version;
    }
    
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append($"{this.List[0]}");
        for (var i = 1; i < this.List.Count; i++)
        {
            builder.Append($".{this.List[i]}");
        }
        
        var value = builder.ToString();
        return value;
    }
    
    public VersionList(Version version)
    {
        this.List = new List<int>();
        this.AppendIfHasValue(version.Major);
        this.AppendIfHasValue(version.Minor);
        this.AppendIfHasValue(version.Build);
        this.AppendIfHasValue(version.Revision);
    }
    
    private void AppendIfHasValue(int item)
    {
        if (item < 0)
        {
            return;
        }
        
        this.List.Add(item);
    }
    
    public void Increment(bool isSignificant = false)
    {
        var startAt = this.List.Count - 1;
        if (isSignificant)
        {
            this.List[startAt] = 0;
            startAt--;
        }
        
        for (var index = startAt; index >= 0; index--)
        {
            if (this.List[index] < int.MaxValue)
            {
                this.List[index]++;
                return;
            }
            
            this.List[index] = 0;
        }
    }
}