using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S11 - a generic parameter count changed.</summary>
public class SwiftGenericParameterCountChanged : IEvaluateSwiftSignatures
{
    public string RuleId => "S11";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "changed its number of generic parameters";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            if (!SwiftGenericConstraints.DidCountChange(
                    typePair.Older.GenericParameters,
                    typePair.Newer.GenericParameters))
            {
                continue;
            }

            yield return typePair.Newer.Name;
        }

        foreach (var functionPair in SwiftMembers.GetPairedFunctions(signatures))
        {
            if (!SwiftGenericConstraints.DidCountChange(
                    functionPair.Older.GenericParameters,
                    functionPair.Newer.GenericParameters))
            {
                continue;
            }

            yield return functionPair.Newer.Name;
        }
    }
}
