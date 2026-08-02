using Winterborn.Library.EasySemVer.DataObject;

namespace Winterborn.Library.EasySemVer.Interfaces;

public interface IEvaluateSignatures
{
    public VersionType EvaluationImpact { get; }
    
    public bool AreDifferencesPresent(ISignaturesToCompare signatures);
}