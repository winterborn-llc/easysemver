using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Providers;

namespace Test;

/// <summary>Hand-built packageable units for the neutral rule tests (TST-M2).</summary>
internal static class Units
{
    internal static IPackageableUnit Csharp(
        string unitId,
        object? signature = null,
        bool hasPublicApiSurface = true)
    {
        return new PackageableUnit
        {
            LanguageId = CsharpLanguageProvider.CsharpLanguageId,
            UnitId = unitId,
            DisplayName = unitId,
            UnitKind = "csproj",
            RelativePath = $"src/{unitId}/{unitId}.csproj",
            Signature = signature,
            HasPublicApiSurface = hasPublicApiSurface
        };
    }

    internal static IPackageableUnit Swift(string unitId, object? signature = null)
    {
        return new PackageableUnit
        {
            LanguageId = SwiftLanguageProvider.SwiftLanguageId,
            UnitId = unitId,
            DisplayName = unitId,
            UnitKind = "swiftpm-target",
            RelativePath = unitId.Split(':')[0],
            Signature = signature
        };
    }
}
