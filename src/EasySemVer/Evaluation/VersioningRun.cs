using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Persistence;
using Winterborn.Library.EasySemVer.Providers;
using Winterborn.Library.EasySemVer.Reporting;
using Winterborn.Library.EasySemVer.Settings;
using Version = Winterborn.Library.EasySemVer.DataObject.Version;

namespace Winterborn.Library.EasySemVer.Evaluation;

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
            // a dry run does not write is the tree - no baseline, no stamped version.
            Log.WriteLine("Dry run: no baseline written and no version stamped");
            WriteJsonReport(options, report, startingVersion, newVersion);
            Log.Outdent();
            return;
        }

        // PER-06: the baseline is written before any version is stamped.
        BaselineFile.Write(options.FolderRoot, units, providers);
        WriteVersions(units, providers, newVersion);

        // REP-08: last, so that a report exists only if everything it describes actually happened.
        WriteJsonReport(options, report, startingVersion, newVersion);
        Log.Outdent();
    }

    private static void WriteJsonReport(
        RunOptions options,
        ChangeReport report,
        Version oldVersion,
        Version newVersion)
    {
        if (options.JsonReportPath.Length < 1)
        {
            return;
        }

        JsonChangeReport.Write(
            options.JsonReportPath,
            report,
            oldVersion,
            newVersion,
            options.IsDryRun);
    }

    private static IReadOnlyList<IPackageableUnit> Discover(
        string folderRoot,
        IReadOnlyList<ILanguageProvider> providers)
    {
        var units = new List<IPackageableUnit>();
        foreach (var provider in providers)
        {
            var found = provider.Discover(folderRoot);
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

    /// <summary>MVR-05 - the one new version goes into every existing location in every unit.</summary>
    private static void WriteVersions(
        IReadOnlyList<IPackageableUnit> units,
        IReadOnlyList<ILanguageProvider> providers,
        Version version)
    {
        foreach (var unit in units)
        {
            var provider = LanguageProviders.Find(providers, unit.Language);
            if (provider == null)
            {
                continue;
            }

            provider.WriteVersion(unit, version);
        }
    }
}
