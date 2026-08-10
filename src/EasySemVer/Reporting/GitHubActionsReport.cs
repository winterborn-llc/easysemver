using System.Text;

namespace Winterborn.Tools.EasySemVer.Reporting;

/// <summary>
/// CLI-10 - publishes the verdict to <c>$GITHUB_OUTPUT</c> and <c>$GITHUB_STEP_SUMMARY</c>. A third
/// sibling of <see cref="JsonChangeReport"/> and <see cref="TextChangeReport"/>, and like them a
/// formatter and nothing else: it reads a <see cref="JsonReportDocument"/> that already exists and
/// never re-examines a signature.
/// <para>
/// It exists because the mapping from report to step outputs is the same in every workflow that
/// runs this tool, and every one of them was writing its own `jq` to do it - including
/// <c>action.yml</c>, which had a second copy. One implementation, in the one place that already
/// knows all the values.
/// </para>
/// </summary>
internal static class GitHubActionsReport
{
    /// <summary>
    /// The output names are the Action's (ACT-05). They are a published contract: a rename here
    /// silently empties a consumer's `steps.version.outputs.x` rather than failing their workflow,
    /// which is why <c>ActionRegression</c> asserts these against <c>action.yml</c>.
    /// </summary>
    internal static IReadOnlyList<KeyValuePair<string, string>> BuildOutputs(
        JsonReportDocument document,
        string reportPath)
    {
        var outputs = new List<KeyValuePair<string, string>>
        {
            // REP-06: stated, never derived from the two versions.
            new("change-type", document.ChangeType),
            new("dry-run", document.IsDryRun ? "true" : "false"),
            new("old-version", document.OldVersion.Version),
            new("version", document.NewVersion.Version),
            new("major", document.NewVersion.Major.ToString()),
            new("minor", document.NewVersion.Minor.ToString()),
            new("patch", document.NewVersion.Patch.ToString())
        };

        // Only when there is one. An output pointing at a path that was never written is worse
        // than an absent one: `if: steps.version.outputs.report` stops working as a guard.
        if (reportPath.Length > 0)
        {
            outputs.Add(new KeyValuePair<string, string>("report", reportPath));
        }

        return outputs;
    }

    /// <summary>
    /// The job summary: the verdict, then the evidence behind it (REP-09). This is the same content
    /// the run already logs (CLI-08), rendered as Markdown for the place a reader actually looks
    /// when a version they did not expect appears on a release.
    /// </summary>
    internal static string BuildSummary(JsonReportDocument document)
    {
        var summary = new StringBuilder();
        var preview = document.IsDryRun ? " _(dry run — nothing written)_" : string.Empty;

        summary.Append("### EasySemVer: ")
            .Append(document.OldVersion.Version)
            .Append(" → ")
            .Append(document.NewVersion.Version)
            .Append(" (")
            .Append(document.ChangeType)
            .Append(')')
            .Append(preview)
            .Append('\n');

        if (document.Findings.Count < 1)
        {
            // CLS-04's fail-safe can raise the floor with nothing to name, so this says what was
            // found rather than restating the verdict as though the two were the same thing.
            summary.Append("\nNo public API changes were detected.\n");
            return summary.ToString();
        }

        summary.Append('\n');
        foreach (var finding in document.Findings)
        {
            // Both halves of the key, because a bare "UnitRemoved" no longer says which language
            // lost a unit - and the summary is read by someone deciding whether to care.
            summary.Append("- **").Append(finding.Impact)
                .Append("** `").Append(finding.Language).Append('/').Append(finding.Rule)
                .Append("` `").Append(finding.Symbol.Length > 0 ? finding.Symbol : finding.UnitId)
                .Append("` ").Append(finding.Description).Append('\n');
        }

        return summary.ToString();
    }

    /// <summary>
    /// Appends to whichever of the two files the runner provided. Both are absent outside a runner
    /// and either can be absent inside one, so a missing file is a skip and never a failure: the
    /// versioning run has already succeeded by the time this is called, and failing it here would
    /// fail a release over a report.
    /// </summary>
    internal static void Write(
        JsonReportDocument document,
        string reportPath,
        Func<string, string?> environment)
    {
        Append(
            "GITHUB_OUTPUT",
            environment,
            string.Concat(BuildOutputs(document, reportPath).Select(o => $"{o.Key}={o.Value}\n")),
            "step outputs");

        Append("GITHUB_STEP_SUMMARY", environment, BuildSummary(document), "job summary");
    }

    private static void Append(
        string variable,
        Func<string, string?> environment,
        string content,
        string description)
    {
        var path = environment(variable);
        if (string.IsNullOrEmpty(path))
        {
            Log.WriteLine($"No ${variable} in the environment; skipping the {description}");
            return;
        }

        File.AppendAllText(path, content);
        Log.WriteLine($"Published the {description} to ${variable}");
    }
}
