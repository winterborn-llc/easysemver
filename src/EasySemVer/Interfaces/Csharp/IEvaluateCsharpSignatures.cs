using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;

namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact { get; }
    
    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures);
}