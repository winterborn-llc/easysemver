namespace Winterborn.Library.EasySemVer.DataObject;

/// <summary>The outcome of one external tool invocation (ML-07).</summary>
[DebuggerDisplay("{CommandLine} -> {ExitCode}")]
public class ProcessResult
{
    /// <summary>The exact command that ran, quoted for a human to paste into a shell (SWE-05).</summary>
    public string CommandLine { get; init; } = string.Empty;

    public int ExitCode { get; init; }

    public string StandardOutput { get; init; } = string.Empty;

    public string StandardError { get; init; } = string.Empty;

    /// <summary>False when the executable could not be found on the path at all.</summary>
    public bool WasExecutableFound { get; init; } = true;

    public bool DidTimeOut { get; init; }

    public bool IsSuccess => this.WasExecutableFound && !this.DidTimeOut && this.ExitCode == 0;

    /// <summary>A one-line explanation of the failure, or empty when the run succeeded.</summary>
    public string FailureDescription
    {
        get
        {
            if (!this.WasExecutableFound)
            {
                return "the executable could not be found";
            }

            if (this.DidTimeOut)
            {
                return "the command timed out";
            }

            return this.ExitCode == 0 ? string.Empty : $"the command exited with code {this.ExitCode}";
        }
    }
}
