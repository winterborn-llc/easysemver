using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S37 - a property's type changed.</summary>
public class PropertyTypeChanged : IEvaluateSwiftSignatures
{
    public string Rule => "PropertyTypeChanged";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "changed its type";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var propertyPair in SwiftMembers.GetPairedProperties(signatures))
        {
            if (propertyPair.Older.Type == propertyPair.Newer.Type)
            {
                continue;
            }

            yield return propertyPair.Newer.Name;
        }
    }
}
