using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S32 - a default argument value was added.</summary>
public class DefaultArgumentAdded : IEvaluateSwiftSignatures
{
    public string Rule => "DefaultArgumentAdded";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "gained a default argument value";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var functionPair in SwiftMembers.GetPairedFunctions(signatures))
        {
            foreach (var parameterPair in SwiftParameters.GetPaired(
                         functionPair.Older.Parameters,
                         functionPair.Newer.Parameters))
            {
                if (parameterPair.Older.HasDefault || !parameterPair.Newer.HasDefault)
                {
                    continue;
                }

                yield return $"{functionPair.Newer.Name} ({parameterPair.Newer.Label})";
            }
        }
    }
}
