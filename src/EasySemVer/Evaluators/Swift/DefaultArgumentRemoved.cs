using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S31 - a default argument value was removed, so calls that omitted it stop compiling.</summary>
public class DefaultArgumentRemoved : IEvaluateSwiftSignatures
{
    public string Rule => "DefaultArgumentRemoved";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "lost a default argument value";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var functionPair in SwiftMembers.GetPairedFunctions(signatures))
        {
            foreach (var parameterPair in SwiftParameters.GetPaired(
                         functionPair.Older.Parameters,
                         functionPair.Newer.Parameters))
            {
                if (!parameterPair.Older.HasDefault || parameterPair.Newer.HasDefault)
                {
                    continue;
                }

                // The function is the subject; the parameter rides along so the line says which
                // one without the description having to vary per finding.
                yield return $"{functionPair.Newer.Name} ({parameterPair.Newer.Label})";
            }
        }
    }
}
