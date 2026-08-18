using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// C and C++ projects, marked by a CMakeLists.txt (LNG-01, version-sync).
/// <para>
/// CMake is the only build system in this ecosystem that states a project's own version in a place
/// that can be found without executing anything: <c>project(Widgets VERSION 1.2.3)</c>. Autotools
/// puts it in a configure.ac macro call, Meson in a meson.build argument list, and both are
/// programs; vcpkg.json and conanfile carry one too and are candidates for later.
/// </para>
/// <para>
/// Reading a C++ <em>API</em> is a different matter entirely and is not attempted here. A header
/// does not say what it declares until the preprocessor has run, and the preprocessor needs the
/// include paths, which need the build system. That is why C++ is version-sync and, unlike the
/// others in this tier, is likely to stay there (LNG-02).
/// </para>
/// </summary>
internal class CppLanguageProvider(
    IReadOnlyList<IDiscoverVersionSources> versionSources)
    : ManifestLanguageProvider(versionSources)
{
    internal const string CppLanguageId = "cpp";

    internal const string CppUnitKind = "cmake-project";

    /// <summary>
    /// FLD-06 - CMake's conventional out-of-source build directory. `build` is the most contested
    /// name in the old global list: it is build output beside a CMakeLists or a build.gradle, and
    /// somebody's build scripts anywhere else.
    /// </summary>
    public override IReadOnlyList<DirectoryExclusion> DirectoryExclusions =>
        [DirectoryExclusion.Beside("build", "CMakeLists.txt")];

    public override string LanguageId => CppLanguageId;

    protected override string UnitKind => CppUnitKind;

    protected override string ManifestFileName => "CMakeLists.txt";
}
