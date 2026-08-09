using System.ComponentModel;
using System.Text;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Process;

/// <summary>
/// The real <see cref="IRunProcess"/>. Everything that shells out goes through here so the tests
/// can stand in for swift, xcodebuild and git without those tools being installed (ML-07).
/// </summary>
internal class ProcessRunner : IRunProcess
{
    public ProcessResult Run(
        string executable,
        IReadOnlyList<string> arguments,
        string workingDirectory,
        TimeSpan timeout)
    {
        var commandLine = DescribeCommand(executable, arguments);
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = executable,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        try
        {
            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process == null)
            {
                return new ProcessResult
                {
                    CommandLine = commandLine,
                    WasExecutableFound = false
                };
            }

            var standardOutput = process.StandardOutput.ReadToEndAsync();
            var standardError = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit((int)timeout.TotalMilliseconds))
            {
                TryKill(process);
                return new ProcessResult
                {
                    CommandLine = commandLine,
                    DidTimeOut = true
                };
            }

            return new ProcessResult
            {
                CommandLine = commandLine,
                ExitCode = process.ExitCode,
                StandardOutput = standardOutput.Result,
                StandardError = standardError.Result
            };
        }
        catch (Win32Exception e)
        {
            return new ProcessResult
            {
                CommandLine = commandLine,
                WasExecutableFound = false,
                StandardError = e.Message
            };
        }
    }

    private static void TryKill(System.Diagnostics.Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (Exception)
        {
            // The process finished between the timeout and the kill; nothing to clean up.
        }
    }

    private static string DescribeCommand(string executable, IReadOnlyList<string> arguments)
    {
        var text = new StringBuilder(executable);
        foreach (var argument in arguments)
        {
            text.Append(' ');
            text.Append(argument.Contains(' ') ? $"\"{argument}\"" : argument);
        }

        return text.ToString();
    }
}
