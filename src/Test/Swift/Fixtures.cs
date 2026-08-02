using Winterborn.Library.EasySemVer.CodeReader.Swift;
using Winterborn.Library.EasySemVer.DataObject.Swift;

namespace Test.Swift;

/// <summary>
/// The checked-in symbol graphs (TST-M5). They were produced by a real toolchain from
/// Fixtures/Widgets.swift.txt, then stripped of source locations so they carry nothing
/// machine-specific.
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
        return SymbolGraphReader.Read(
            "Widgets",
            [
                File.ReadAllText(GetPath("Widgets.symbols.json")),
                File.ReadAllText(GetPath("Widgets@Swift.symbols.json"))
            ]);
    }
}
