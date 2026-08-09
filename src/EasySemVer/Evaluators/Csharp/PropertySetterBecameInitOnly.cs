using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R42 - a property's set became init. The property is still writable, so R09 does not fire;
/// recording init separately from set (CSX-03) is what makes it visible at all.
/// </summary>
public class PropertySetterBecameInitOnly : IEvaluateCsharpSignatures
{
    public string RuleId => "R42";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "can now only be set during initialization";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            foreach (var name in typePair.Older.Properties.Keys)
            {
                if (!typePair.Newer.Properties.Contains(name))
                {
                    continue;
                }

                var older = typePair.Older.Properties[name];
                var newer = typePair.Newer.Properties[name];
                if (!older.IsWritable || older.IsInitOnly)
                {
                    continue;
                }

                if (!newer.IsInitOnly)
                {
                    continue;
                }

                yield return $"{typePair.Newer.Name}.{name}";
            }
        }
    }
}
