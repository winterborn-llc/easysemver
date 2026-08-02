using Winterborn.Library.EasySemVer.DataObject;

namespace Winterborn.Library.EasySemVer.Interfaces.Swift;

/// <summary>
/// One Swift classification rule (ML-04). There is deliberately no base type shared with
/// IEvaluateCsharpSignatures: the two operate on different object models.
/// </summary>
public interface IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact { get; }

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures);
}
