using System.Diagnostics.CodeAnalysis;
using System.Text;
using Winterborn.Tools.EasySemVer.Extensions;

namespace Winterborn.Tools.EasySemVer.DataObject;

/// <summary>
/// A dotted sequence of non-negative integers, canonically MAJOR.MINOR.PATCH (VER-01). Short
/// inputs are normalised to three segments on parse rather than crashing <see cref="Increment"/>:
/// with several ecosystems feeding seeds, a two-segment MARKETING_VERSION is routine input now
/// (MVR-02, was G-11).
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class Version
{
    private const int SegmentCount = 3;

    private const int IndexOfMajor = 0;
    private const int IndexOfMinor = 1;
    private const int IndexOfPatch = 2;

    public int? Major => this.List.Count > IndexOfMajor ? this.List[IndexOfMajor] : null;

    public int? Minor => this.List.Count > IndexOfMinor ? this.List[IndexOfMinor] : null;

    public int? Patch => this.List.Count > IndexOfPatch ? this.List[IndexOfPatch] : null;

    private IList<int> List { get; }

    public Version(string? text = "0.0.0")
    {
        this.List = new List<int>();
        if (!text.IsNullOrWhitespace())
        {
            var parts = text.Split('.');
            foreach (var part in parts)
            {
                if (!int.TryParse(part, out var value))
                {
                    throw new InvalidProgramException($"Invalid version format: {text}");
                }

                this.List.Add(value);
            }
        }

        // A blank version behaves as 0.0.0 rather than being unusable (MVR-02).
        while (this.List.Count < SegmentCount)
        {
            this.List.Add(0);
        }
    }

    /// <summary>
    /// MVR-03 - a version source with an unparseable value is skipped with a warning, never a
    /// reason to fail the run.
    /// </summary>
    public static bool TryParse(string? text, [NotNullWhen(true)] out Version? version)
    {
        try
        {
            version = new Version(text);
            return true;
        }
        catch (InvalidProgramException)
        {
            version = null;
            return false;
        }
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
        while (this.List.Count <= indexToIncrement)
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
        return Compare(v1, v2) >= 0;
    }

    public static bool operator <=(Version? v1, Version? v2)
    {
        return Compare(v1, v2) <= 0;
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
        if (v1 is null && v2 is null)
        {
            return 0;
        }

        if (v2 is null)
        {
            return 1;
        }

        if (v1 is null)
        {
            return -1;
        }

        if (v1.Major != v2.Major)
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
