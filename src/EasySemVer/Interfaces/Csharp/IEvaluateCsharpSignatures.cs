using Winterborn.Library.EasySemVer.DataObject;

namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

/// <summary>
/// One C# classification rule (CLS-01, preserved per-language by ML-04). Swift has its own
/// equivalent over its own comparison context; there is deliberately no shared base.
/// </summary>
public interface IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact { get; }

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures);
}
