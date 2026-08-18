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

        // Before discovery, because the first walk is what consults it (FLD-04, CLI-12).
        DirectoryExclusions.BeginRun(options.DoNotExclude, CollectExclusions(providers));

        var units = Discover(options.FolderRoot, providers);
        DirectoryExclusions.LogSkipped();
        Extract(units, providers);

        var baseline = BaselineFile.Read(options.FolderRoot, providers);
        var report = ChangeClassifier.Classify(baseline, units, providers);
        TextChangeReport.Write(report, options.IsDryRun);

        var startingVersion = GetSeedVersion(units, providers);
        var newVersion = new Version(startingVersion);
        newVersion.Increment(report.ChangeType, options.MaximumMinor, options.MaximumPatch);
        Log.WriteLine($"Version: {startingVersion} -> {newVersion}");

        if (options.IsDryRun)
        {
            // Not "nothing written": a --json report is still produced, carrying dryRun: true. What
            // a dry run does not write is the tree - no baseline, no stamped version, and therefore
            // an empty writtenFiles (REP-10).
            Log.WriteLine("Dry run: no baseline written and no version stamped");

            // TOK-06. Named, not written. The token replacement is the one write here that
            // consumes what it replaces, so "which files would this take my token out of" is
            // worth answering before a run does it - and answering it costs a read-only walk.
            VersionTokens.Stamp(
                options.FolderRoot,
                options.VersionTokenName,
                newVersion,
                isDryRun: true);

            Publish(options, report, startingVersion, newVersion, writtenFiles: []);
            Log.Outdent();
            return;
        }

        // PER-06: the baseline is written before any version is stamped, and it is itself one of
        // the files a caller has to stage (REP-10).
        BaselineFile.Write(options.FolderRoot, units, providers);
        var writtenFiles = new List<string> { MagicValues.SignatureFileName };
        writtenFiles.AddRange(WriteVersions(units, providers, newVersion));

        // TOK-01, last of the writes: the version locations belong to units a provider found, and
        // this is the free text around them, which belongs to nobody. Its files are reported like
        // any other write (REP-10), because a workflow that commits the bump has to stage them.
        writtenFiles.AddRange(VersionTokens.Stamp(
            options.FolderRoot,
            options.VersionTokenName,
            newVersion,
            isDryRun: false));

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

    /// <summary>
    /// FLD-06 - every registered language's declared exclusions, unioned. A dependency tree should
    /// be invisible to every provider rather than only to the one that recognised it: a vendored Go
    /// module holding a stray .csproj is not this repository's C# either.
    /// </summary>
    private static IReadOnlyList<DirectoryExclusion> CollectExclusions(
        IReadOnlyList<ILanguageProvider> providers)
    {
        var exclusions = new List<DirectoryExclusion>();
        foreach (var provider in providers)
        {
            exclusions.AddRange(provider.DirectoryExclusions);
        }

        return exclusions;
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
                // LNG-04: the question can only ever take a surface away. A unit arrives claiming
                // one (UNI-01 defaults it true), so this is exactly UNI-04 for every provider that
                // reads an API - and a version-sync-tier provider, which reads none, says so once
                // at discovery instead of having to call all of its production code "test code" to
                // get the same effect.
                unit.HasPublicApiSurface = unit.HasPublicApiSurface && !provider.IsTestCode(unit);
            }

            Log.WriteLine($"{provider.LanguageId}: {found.Count} units");
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
            var provider = LanguageProviders.Find(providers, unit.LanguageId)
                           ?? throw new InvalidOperationException(
                               $"No provider is registered for {unit.LanguageId}");

            // UNI-04. Nothing downstream reads a signature this unit does not have, and building
            // one would be work spent on a graph that classification, the baseline and the report
            // all ignore. Its versions are still read and written by the stages after this one.
            if (!unit.HasPublicApiSurface)
            {
                Log.WriteLine($"Versioning {unit.LanguageId} {unit.UnitId} ({unit.RelativePath}) without reading its API surface");
                continue;
            }

            Log.WriteLine($"Reading {unit.LanguageId} {unit.UnitId} ({unit.RelativePath})");
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
            var provider = LanguageProviders.Find(providers, unit.LanguageId);
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
            var provider = LanguageProviders.Find(providers, unit.LanguageId);
            if (provider == null)
            {
                continue;
            }

            written.AddRange(provider.WriteVersion(unit, version));
        }

        return written;
    }
}
