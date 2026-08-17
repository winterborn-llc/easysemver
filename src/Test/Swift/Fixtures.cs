using Winterborn.Tools.EasySemVer.CodeReader.Swift;
using Winterborn.Tools.EasySemVer.DataObject.Swift;

namespace Test.Swift;

/// <summary>
/// The checked-in Swift source the extraction tests read (TST-M5). It is the whole input: there is
/// no toolchain in the loop and nothing to install, so the unit suite runs anywhere.
/// </summary>
internal static class Fixtures
{
    private static readonly Lazy<SwiftModule> Widgets = new(ReadWidgets);

    internal static SwiftModule WidgetsModule => Widgets.Value;

    internal static string GetPath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
    }

    private static SwiftModule ReadWidgets()
    {
        return SwiftSourceReader.Read("Widgets", [File.ReadAllText(GetPath("Widgets.swift.txt"))]);
    }
}
