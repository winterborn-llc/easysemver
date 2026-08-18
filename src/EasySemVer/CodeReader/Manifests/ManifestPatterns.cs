using System.Text.RegularExpressions;

namespace Winterborn.Tools.EasySemVer.CodeReader.Manifests;

/// <summary>
/// Where each ecosystem writes its own version, as one pattern apiece. Every one of them is
/// deliberately narrow: it matches a **literal** assignment in the manifest's own top-level scope
/// and nothing else, because the alternative is rewriting a dependency's version with this
/// repository's number (MVR-04, and see <see cref="ManifestVersionSource"/> on first-match-only).
/// <para>
/// Each pattern captures a <c>version</c> group and only that group is replaced, so quoting,
/// spacing and the rest of the line survive untouched.
/// </para>
/// </summary>
internal static partial class ManifestPatterns
{
    /// <summary>
    /// package.json and composer.json. Anchored to a two-space indent at the start of a line, which
    /// is where a top-level key sits in every formatter npm and Composer ship. A nested
    /// <c>"version"</c> - inside <c>dependencies</c>, <c>engines</c> or a lockfile-ish block - is
    /// indented further and is not this package's version to change.
    /// </summary>
    [GeneratedRegex(@"(?m)^\s{0,2}""version""\s*:\s*""(?<version>[0-9][^""]*)""")]
    internal static partial Regex Json();

    /// <summary>
    /// Cargo.toml and pyproject.toml. TOML puts the package's own version under a table, so this
    /// requires the assignment to be preceded by <c>[package]</c>, <c>[project]</c> or
    /// <c>[tool.poetry]</c> with no intervening table header - which is what stops it matching the
    /// version of a dependency in <c>[dependencies]</c> further down.
    /// </summary>
    [GeneratedRegex(
        """(?ms)^\[(?:package|project|tool\.poetry)\]\r?\n(?:(?!^\[).)*?^version\s*=\s*["'](?<version>[0-9][^"']*)["']""")]
    internal static partial Regex Toml();

    /// <summary>
    /// pubspec.yaml. Top-level only - column zero - because anything indented belongs to a
    /// dependency entry. Dart versions carry an optional <c>+build</c> suffix, which is part of the
    /// value and is replaced with it.
    /// </summary>
    [GeneratedRegex(@"(?m)^version:[ \t]*(?<version>[0-9][^\s#]*)")]
    internal static partial Regex Yaml();

    /// <summary>
    /// gradle.properties, and any other Java-style properties file carrying <c>version=</c> at
    /// column zero. Deliberately not <c>build.gradle</c>: that is executable Groovy or Kotlin, and
    /// the same reasoning that stopped Package.swift being text-parsed for anything but literals
    /// applies (SWD-01).
    /// </summary>
    [GeneratedRegex(@"(?m)^version[ \t]*=[ \t]*(?<version>[0-9][^\s#]*)")]
    internal static partial Regex Properties();

    /// <summary>
    /// A Ruby <c>VERSION</c> constant, as found in the <c>lib/&lt;gem&gt;/version.rb</c> that
    /// gemspecs conventionally point at. The gemspec itself usually assigns
    /// <c>spec.version = Gem::VERSION</c>, which is not a literal and so is left alone.
    /// </summary>
    [GeneratedRegex("""(?m)^\s*VERSION\s*=\s*["'](?<version>[0-9][^"']*)["']""")]
    internal static partial Regex RubyConstant();

    /// <summary>
    /// A Perl <c>$VERSION</c> assignment. Same reasoning as Ruby's: the literal is the only thing
    /// safe to touch, and Perl's distribution metadata is generated from it rather than the reverse.
    /// </summary>
    [GeneratedRegex("""(?m)^\s*(?:our|my)?\s*\$VERSION\s*=\s*["'](?<version>[0-9][^"']*)["']""")]
    internal static partial Regex PerlVariable();

    /// <summary>
    /// A gemspec assigning its version as a literal - <c>spec.version = "1.2.3"</c> - rather than
    /// pointing at a constant. Both conventions are in wide use, and a gem using the constant is
    /// covered by <see cref="RubyConstant"/> in its version.rb instead.
    /// </summary>
    [GeneratedRegex("""(?m)^\s*\w+\.version\s*=\s*["'](?<version>[0-9][^"']*)["']""")]
    internal static partial Regex GemspecLiteral();

    /// <summary>
    /// CMake's <c>project(Name VERSION 1.2.3)</c>, which is where a C or C++ project states its own
    /// version. Only the <c>project()</c> call is matched: <c>find_package(Foo 2.0)</c> and
    /// <c>set(SOMETHING_VERSION ...)</c> are somebody else's version or a derived one.
    /// <para>
    /// The name between <c>project(</c> and <c>VERSION</c> is skipped without crossing a closing
    /// parenthesis, so a later call cannot be spliced onto an earlier one.
    /// </para>
    /// </summary>
    [GeneratedRegex(@"(?is)\bproject\s*\(\s*[^)]*?\bVERSION\s+(?<version>[0-9][0-9.]*)")]
    internal static partial Regex CMakeProject();
}
