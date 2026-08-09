using Winterborn.Tools.EasySemVer.DataObject;

namespace Winterborn.Tools.EasySemVer.Interfaces;

/// <summary>
/// Every external tool invocation - swift, xcodebuild, git - goes through here, so extraction,
/// failure, and timeout paths are testable on a machine with none of them installed (ML-07).
/// </summary>
public interface IRunProcess
{
    public ProcessResult Run(
        string executable,
        IReadOnlyList<string> arguments,
        string workingDirectory,
        TimeSpan timeout);
}
