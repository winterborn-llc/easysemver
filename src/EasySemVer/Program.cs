using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Providers;
using Winterborn.Tools.EasySemVer.Settings;

namespace Winterborn.Tools.EasySemVer;

// ReSharper disable once ClassNeverInstantiated.Global
public static class Program
{
    /// <summary>
    /// CLI-06 - 0 on success, 1 on any unhandled failure with the exception printed. The exit code
    /// is returned rather than forced through Environment.Exit, so invoking a run in-process does
    /// not take the calling host down with it (ERR-03).
    /// </summary>
    public static int Main(params string[] args)
    {
        Log.ResetIndent();
        try
        {
            var options = RunOptions.Parse(args);
            var providers = LanguageProviders.Create(new ProcessRunner());
            VersioningRun.Execute(options, providers);
            return 0;
        }
        catch (Exception e)
        {
            Log.ResetIndent();
            Log.WriteLine(e.ToString());
            return 1;
        }
    }
}
