using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Persistence;
using Winterborn.Tools.EasySemVer.Providers;
using Winterborn.Tools.EasySemVer.Reporting;
using Winterborn.Tools.EasySemVer.Settings;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.Evaluation;

/// <summary>
/// One invocation, end to end. Discovery happens exactly once and its result feeds every
/// downstream stage (FLD-03); persistence is the only mutating step and it runs last, so a
/// failure anywhere before it leaves the working tree untouched.
/// </summary>
internal static class VersioningRun
{
    internal static void Execute(RunOptions options, IReadOnlyList<ILanguageProvider> providers)
    {
        Log.WriteLine($"EasySemVer: {options.FolderRoot}{(options.IsDryRun ? " (dry run)" : string.Empty)}");
        Log.Indent();

        var units = Discover(options.FolderRoot, providers);
        Extract(units, providers);

        var baseline = BaselineFile.Read(options.FolderRoot, providers);
        var report = ChangeClassifier.Classify(baseline, units, providers);
        TextChangeReport.Write(report, options.IsDryRun);

        var startingVersion = GetSeedVersion(units, providers);
        var newVersion = new Version(startingVersion);
        newVersion.Increment(report.ChangeType);
        Log.WriteLine($"Version: {startingVersion} -> {newVersion}");

        if (options.IsDryRun)
        {
            // Not "nothing written": a --json report is still produced, carrying dryRun: true. What
            // a dry run does not write is the tree - no baseline, no stamped version, and therefore
            // an empty writtenFiles (REP-10).
            Log.WriteLine("Dry run: no baseline written and no version stamped");
            Publish(options, report, startingVersion, newVersion, writtenFiles: []);
            Log.Outdent();
            return;
        }

        // PER-06: the baseline is written before any version is stamped, and it is itself one of
        // the files a caller has to stage (REP-10).
        BaselineFile.Write(options.FolderRoot, units, providers);
        var writtenFiles = new List<string> { MagicValues.SignatureFileName };
        writtenFiles.AddRange(WriteVersions(units, providers, newVersion));

        // REP-08: last, so that a report exists only if everything it describes actually happened.
        Publish(options, report, startingVersion, newVersion, writtenFiles);
        Log.Outdent();
    }

    /// <summary>
    /// The two machine-readable surfaces, built from one document so they can never disagree about
    /// the same run (REP-01, CLI-10).
    /// </summary>
    private static void Publish(
        RunOptions options,
        ChangeReport report,
        Version oldVersion,
        Version newVersion,
        IReadOnlyList<string> writtenFiles)
    {
        if (options.JsonReportPath.Length < 1 && !options.WritesGitHubActionsReport)
        {
            return;
        }

        var document = JsonChangeReport.Build(
            report,
            oldVersion,
            newVersion,
            options.IsDryRun,
            writtenFiles);

        if (options.JsonReportPath.Length > 0)
        {
            JsonChangeReport.Write(options.JsonReportPath, document);
        }

        if (options.WritesGitHubActionsReport)
        {
            GitHubActionsReport.Write(
                document,
                options.JsonReportPath,
                Environment.GetEnvironmentVariable);
        }
    }

    private static IReadOnlyList<IPackageableUnit> Discover(
        string folderRoot,
        IReadOnlyList<ILanguageProvider> providers)
    {
        var units = new List<IPackageableUnit>();
        foreach (var provider in providers)
        {
            var found = provider.Discover(folderRoot);

            // UNI-04. Asked here rather than left to each Discover, so that the rule is applied
            // once, in one neutral place, and a provider cannot quietly not have an answer. What
            // counts as test code is entirely the provider's to decide; what happens next is not.
            foreach (var unit in found)
            {
                unit.HasPublicApiSurface = !provider.IsTestCode(unit);
            }

            Log.WriteLine($"{provider.Language}: {found.Count} units");
            units.AddRange(found);
        }

        // FLD-05: an empty folder is an ordinary input, not an error.
        if (units.Count < 1)
        {
            Log.WriteLine("No packageable units found under the folder root");
        }

        return units;
    }

    private static void Extract(
        IReadOnlyList<IPackageableUnit> units,
        IReadOnlyList<ILanguageProvider> providers)
    {
        foreach (var unit in units)
        {
            var provider = LanguageProviders.Find(providers, unit.Language)
                           ?? throw new InvalidOperationException(
                               $"No provider is registered for {unit.Language}");

            // UNI-04. Nothing downstream reads a signature this unit does not have, and building
            // one would be work spent on a graph that classification, the baseline and the report
            // all ignore. Its versions are still read and written by the stages after this one.
            if (!unit.HasPublicApiSurface)
            {
                Log.WriteLine($"Versioning {unit.Language} {unit.UnitId} ({unit.RelativePath}) without reading its API surface");
                continue;
            }

            Log.WriteLine($"Reading {unit.Language} {unit.UnitId} ({unit.RelativePath})");
            provider.Extract(unit);
        }
    }

    /// <summary>MVR-03 - the seed is the highest version found in any source in any unit.</summary>
    private static Version GetSeedVersion(
        IReadOnlyList<IPackageableUnit> units,
        IReadOnlyList<ILanguageProvider> providers)
    {
        var highest = new Version("0.0.0");
        foreach (var unit in units)
        {
            var provider = LanguageProviders.Find(providers, unit.Language);
            if (provider == null)
            {
                continue;
            }

            foreach (var version in provider.ReadVersions(unit))
            {
                if (version <= highest)
                {
                    continue;
                }

                highest = version;
            }
        }

        return highest;
    }

    /// <summary>
    /// MVR-05 - the one new version goes into every existing location in every unit. Returns what
    /// each provider says it wrote, which is what REP-10 publishes.
    /// </summary>
    private static IReadOnlyList<string> WriteVersions(
        IReadOnlyList<IPackageableUnit> units,
        IReadOnlyList<ILanguageProvider> providers,
        Version version)
    {
        var written = new List<string>();
        foreach (var unit in units)
        {
            var provider = LanguageProviders.Find(providers, unit.Language);
            if (provider == null)
            {
                continue;
            }

            written.AddRange(provider.WriteVersion(unit, version));
        }

        return written;
    }
}
