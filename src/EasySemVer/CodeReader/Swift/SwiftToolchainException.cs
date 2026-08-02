using Winterborn.Library.EasySemVer.DataObject;

namespace Winterborn.Library.EasySemVer.CodeReader.Swift;

/// <summary>
/// SWE-05 / D-03 - if Swift units exist and their signatures cannot be extracted, the run fails.
/// There is no skip-and-warn: a partial baseline would silently under-report the next change.
/// The message names the unit, the exact command, and the tool's own stderr, so the person
/// reading a build log can reproduce it.
/// </summary>
public class SwiftToolchainException(string unitDescription, ProcessResult result)
    : Exception(BuildMessage(unitDescription, result))
{
    private static string BuildMessage(string unitDescription, ProcessResult result)
    {
        var stderr = result.StandardError.Trim();
        var detail = stderr.Length > 0 ? $"\n{stderr}" : string.Empty;
        return $"Swift extraction failed for {unitDescription}: {result.FailureDescription}."
               + $"\n  command: {result.CommandLine}{detail}";
    }
}
