using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R42 - a property's set became init. The property is still writable, so R09 does not fire;
/// recording init separately from set (CSX-03) is what makes it visible at all.
/// </summary>
public class PropertySetterBecameInitOnly : IEvaluateCsharpSignatures
{
    public string Rule => "PropertySetterBecameInitOnly";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "can now only be set during initialization";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var pair in Properties.GetPaired(signatures))
        {
            if (!pair.Older.IsWritable || pair.Older.IsInitOnly)
            {
                continue;
            }

            if (!pair.Newer.IsInitOnly)
            {
                continue;
            }

            yield return $"{pair.DeclaringType.Name}.{pair.Newer.Name}";
        }
    }
}
