using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Providers;

namespace Test;

/// <summary>
/// Sets up directory exclusions the way a real run does (FLD-06).
/// <para>
/// Exists because forgetting is silent. Exclusions used to be a static list that applied whether or
/// not anyone had initialised anything, so a test could walk a tree without a thought and still get
/// them. Now they are collected from the registered providers by <c>VersioningRun</c>, and a test
/// that constructs a provider directly gets *no* exclusions at all - it passes or fails against a
/// walk no user will ever perform.
/// </para>
/// </summary>
internal static class Exclusions
{
    /// <summary>The union every registered language declares, as VersioningRun assembles it.</summary>
    internal static IReadOnlyList<DirectoryExclusion> FromEveryProvider()
    {
        var declared = new List<DirectoryExclusion>();
        foreach (var provider in LanguageProviders.Create(new ProcessRunner()))
        {
            declared.AddRange(provider.DirectoryExclusions);
        }

        return declared;
    }

    /// <summary>Begins a run's worth of exclusion state, optionally keeping some names (CLI-12).</summary>
    internal static void BeginRun(params string[] doNotExclude)
    {
        DirectoryExclusions.BeginRun(doNotExclude, FromEveryProvider());
    }
}
