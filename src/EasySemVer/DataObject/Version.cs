using System.Text;
using Winterborn.Library.EasySemVer.CodeReader;
using Winterborn.Library.EasySemVer.CodeReader.Csharp;
using Winterborn.Library.EasySemVer.Extensions;

namespace Winterborn.Library.EasySemVer.DataObject;

//Interfaces
//Version
//Revisions
//Versions

[DebuggerDisplay("{ToString()}")]
public class Version
{
    private const int IndexOfMajor = 0;
    private const int IndexOfMinor = 1;
    private const int IndexOfPatch = 2;
    
    public int? Major => this.List.Count > 0 ? this.List[0] : null;
    public int? Minor => this.List.Count > 1 ? this.List[1] : null;
    public int? Patch => this.List.Count > 2 ? this.List[2] : null;
    
    private IList<int> List { get; }

    public Version(string? text = "0.0.0")
    {
        this.List = new List<int>();
        if (text.IsNullOrWhitespace())
        {
            return;
        }
        
        var parts = text.Split('.');
        foreach (var part in parts)
        {
            if (int.TryParse(part, out var value))
            {
                this.List.Add(value);
                continue;
            }
            
            throw new InvalidProgramException($"Invalid version format: {text}");
        }
    }
    
    internal static Version GetVersionFromProjectFiles(params CsProjFile[] csProjFiles)
    {
        var startingVersion = new Version("0.0.0");
        foreach (var csProjFile in csProjFiles)
        {
            if (csProjFile.Version <= startingVersion)
            {
                continue;
            }
            
            startingVersion = csProjFile.Version;
        }

        return startingVersion;
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
    
    public void Increment(VersionType type)
    {
        var index = GetIndexFromChangeType(type);
        if (this.List[index] == int.MaxValue)
        {
            index--;
        }

        if (index < 0)
        {
            throw new OverflowException($"The major version has exceeded the maximum size of {int.MaxValue}.");
        }
        
        this.IncrementCounterAt(index);
        this.ResetSubsequentCounters(index);
    }

    private static int GetIndexFromChangeType(VersionType type)
    {
        var index = type switch
        {
            VersionType.Major => IndexOfMajor,
            VersionType.Minor => IndexOfMinor,
            VersionType.Patch => IndexOfPatch,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        return index;
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

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Major, this.Minor, this.Patch);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }
        
        var version = obj as Version;
        if (version == null)
        {
            return false;
        }
        
        return this == version;
    }

    public static implicit operator string(Version version)
    {
        return version.ToString();
    }

    public static implicit operator Version(string versionString)
    {
        return new Version(versionString);
    }
    
    public static bool operator >(Version? v1, Version? v2)
    {
        return Compare(v1, v2) > 0;
    }
    
    public static bool operator >=(Version? v1, Version? v2)
    {
        return Compare(v1, v2) > 0 || Compare(v1, v2) == 0;
    }
    
    public static bool operator <=(Version? v1, Version? v2)
    {
        return Compare(v1, v2) < 0 || Compare(v1, v2) == 0;
    }

    public static bool operator <(Version? v1, Version? v2)
    {
        return Compare(v1, v2) < 0;
    }

    public static bool operator ==(Version? v1, Version? v2)
    {
        return Compare(v1, v2) == 0;
    }

    public static bool operator !=(Version? v1, Version? v2)
    {
        return Compare(v1, v2) != 0;
    }

    private static int Compare(Version? v1, Version? v2)
    {
        if (v1?.ToString() == null && v2?.ToString() == null)
        {
            return 0;
        }
        
        if (v1?.ToString() != null && v2?.ToString() == null)
        {
            return 1;
        }
        
        if (v1?.ToString() == null && v2?.ToString() != null)
        {
            return -1;
        }
        
        if (v1!.Major != v2!.Major)
        {
            return v1.Major.GetValueOrDefault().CompareTo(v2.Major.GetValueOrDefault());
        }
        
        if (v1.Minor != v2.Minor)
        {
            return v1.Minor.GetValueOrDefault().CompareTo(v2.Minor.GetValueOrDefault());
        }
        
        return v1.Patch.GetValueOrDefault().CompareTo(v2.Patch.GetValueOrDefault());
    }
}