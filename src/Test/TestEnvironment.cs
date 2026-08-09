using System.Runtime.CompilerServices;

namespace Test;

/// <summary>
/// CLI-10 detects GitHub Actions from the environment, and this suite runs *on* GitHub Actions
/// (CI-01) while calling <c>Program.Main</c> in-process. Without this, every run a test performs
/// would append to the real job's <c>$GITHUB_OUTPUT</c> and <c>$GITHUB_STEP_SUMMARY</c> - hundreds
/// of verdicts in the job summary, and step outputs set on the test step by a fixture in a temp
/// directory.
/// <para>
/// Cleared once for the whole assembly rather than per test, so a test added later cannot forget.
/// A test that wants the behaviour asks for it with <c>--github</c> and supplies its own paths,
/// which is what <c>TestGitHubActionsReport</c> does.
/// </para>
/// </summary>
internal static class TestEnvironment
{
    [ModuleInitializer]
    internal static void DetachFromTheRealRunner()
    {
        foreach (var variable in (string[])["GITHUB_ACTIONS", "GITHUB_OUTPUT", "GITHUB_STEP_SUMMARY"])
        {
            Environment.SetEnvironmentVariable(variable, null);
        }
    }
}
