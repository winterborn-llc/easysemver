using System.Diagnostics;
using System.Text;

namespace Yamamari.Library.AutoVersion;

// Major.Minor[.Patch]
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
    }
    
    private void AppendIfHasValue(int item)
    {
        if (item < 0)
        {
            return;
        }
        
        this.List.Add(item);
    }
    
    public void Increment(VersionType type)
    {
        const int indexOfMajor = 0;
        const int indexOfMinor = 1;
        const int indexOfPatch = 2;
        if (type == VersionType.Major)
        {
            var index = indexOfMajor;
            this.IncrementCounterAt(index);
            this.ResetSubsequentCounters(index);
            return;
        }
        
        if (type == VersionType.Minor)
        {
            var index = indexOfMinor;
            this.IncrementCounterAt(index);
            this.ResetSubsequentCounters(index);
            return;
        }
        
        var patchValue = this.List[indexOfPatch];
        if (patchValue < int.MaxValue)
        {
            this.IncrementCounterAt(indexOfPatch);
            this.ResetSubsequentCounters(indexOfPatch);
            return;
        }
        
        this.List[indexOfPatch] = 0;
        this.IncrementCounterAt(indexOfMinor);
        this.ResetSubsequentCounters(indexOfMinor);
    }

    private void IncrementCounterAt(int indexToIncrement)
    {
        while (this.List.Count < indexToIncrement)
        {
            this.List.Add(0);
        }
        
        this.List[indexToIncrement] += 1;
    }

    private void ResetSubsequentCounters(int indexOfValueToKeep)
    {
        for (var index = indexOfValueToKeep + 1; index < this.List.Count; index++)
        {
            this.List[index] = 0;
        }
    }
}