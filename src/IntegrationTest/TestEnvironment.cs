using System.Runtime.CompilerServices;

namespace IntegrationTest;

/// <summary>
/// The integration suite's copy of <c>Test.TestEnvironment</c>, and for the same reason: CLI-10
/// detects GitHub Actions from the environment, this suite runs on GitHub Actions (CI-01), and it
/// calls <c>Program.Main</c> against temp-directory fixtures dozens of times. Left detected, those
/// runs would append to the real job's summary and set step outputs from a fixture.
/// <para>
/// <c>ActionRegression</c> is unaffected: it runs the Action's scripts as child processes with an
/// environment it builds explicitly, including its own <c>$GITHUB_OUTPUT</c>.
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
