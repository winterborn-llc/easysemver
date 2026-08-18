using System.Xml.Linq;
using Winterborn.Tools.EasySemVer.DataObject;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.Interfaces;

/// <summary>
/// Everything one language contributes to a run (ML-02). Adding a language means writing one of
/// these and registering it - no edit to anything under Interfaces/, Evaluation/, Persistence/ or
/// any other language's folders.
/// </summary>
public interface ILanguageProvider
{
    /// <inheritdoc cref="IPackageableUnit.LanguageId"/>
    public string LanguageId { get; }

    /// <summary>
    /// BAS-07 - which generation of this language's signatures the provider writes and can read.
    /// A baseline unit stamped with anything else is dropped rather than compared, so the unit
    /// re-seeds from an empty history and everything around it keeps its own.
    /// <para>
    /// It exists because the alternative is the whole-file format version, and that is far too
    /// blunt an instrument for this: changing how one language extracts its signatures would
    /// invalidate every other language's history too, and hand a repository with no code in that
    /// language at all a release it did not earn. A provider bumps this when the words it uses to
    /// describe the same API change - not when the API model gains a field, which diffs as an
    /// ordinary change and should.
    /// </para>
    /// <para>
    /// Defaulted to the first generation, which is also what a baseline written before this
    /// existed is read as. A new language starts here and stays here until it has a reason not to.
    /// </para>
    /// </summary>
    public string SignatureVersion => "1";

    /// <summary>
    /// FLD-06 - directories this language asks the walk to skip, each with the sibling marker that
    /// proves it is that directory rather than one sharing its name.
    /// <para>
    /// Owned by the language because only the language knows what its build output and vendored
    /// dependencies are called, and because a global list grows with the language count while the
    /// chance that one of its names is somebody's real source directory grows with it. `vendor`,
    /// `target`, `venv` and `blib` are each build output in one ecosystem and ordinary code in
    /// another.
    /// </para>
    /// <para>
    /// Defaulted to empty, so a language that has nothing to exclude says nothing, and adding this
    /// broke no implementer. The exclusions are unioned across every registered provider and applied
    /// to the whole walk - a dependency tree should be invisible to every language, not only to the
    /// one that recognised it.
    /// </para>
    /// </summary>
    public IReadOnlyList<DirectoryExclusion> DirectoryExclusions => [];

    /// <summary>Enumerates the folder root once per run (FLD-03) and returns this language's units.</summary>
    public IReadOnlyList<IPackageableUnit> Discover(string folderRoot);

    /// <summary>
    /// UNI-04 - whether this unit is test code, and so is versioned without its public members
    /// being treated as a contract. Asked once per discovered unit, immediately after
    /// <see cref="Discover"/>, and the answer becomes
    /// <see cref="IPackageableUnit.HasPublicApiSurface"/>.
    /// <para>
    /// Every language has this question and every language answers it differently - a
    /// <c>&lt;IsTestProject&gt;</c> property or a test-framework reference in a .csproj, a
    /// <c>.testTarget</c> in a Package.swift, a unit-test product type in an .xcodeproj - so it
    /// belongs here, beside the discovery that already reads those files, and nowhere else. A
    /// provider MAY answer from state it gathered during discovery rather than reading again.
    /// </para>
    /// <para>
    /// Defaulted to false so that adding this did not break an implementer, and so that a
    /// language nobody has taught behaves as it did before the question existed. Every provider in
    /// this repository overrides it, and <c>TestLanguageSeam</c> asserts that a new one has to.
    /// </para>
    /// </summary>
    public bool IsTestCode(IPackageableUnit unit) => false;

    /// <summary>Fills <see cref="IPackageableUnit.Signature"/>. Throws to fail the run (SWE-05).</summary>
    public void Extract(IPackageableUnit unit);

    /// <summary>
    /// Every change this language found between the two runs. <paramref name="units"/> holds only
    /// this language's units, already filtered for UNI-04, so a provider never sees another
    /// language's work and never has to check.
    /// <para>
    /// Pairing units, and deciding what a unit appearing or disappearing means, are this
    /// provider's job rather than the core's: every rule belongs to exactly one language, even
    /// where several languages would answer alike. <c>UnitPairing</c> and the rule base classes
    /// under Evaluators/ exist so that agreeing with the other languages costs a subclass and
    /// disagreeing costs an override.
    /// </para>
    /// <para>
    /// Returns what it found rather than a verdict: the caller aggregates the impacts (CLS-03) and
    /// the same findings are what the run reports (§20 O-04). An empty list is a Patch.
    /// </para>
    /// </summary>
    public IReadOnlyList<ChangeFinding> Classify(IUnitsToCompare units);

    public IReadOnlyList<Version> ReadVersions(IPackageableUnit unit);

    /// <summary>
    /// Stamps the version into every writable source in the unit and returns the folder-root-relative
    /// path of each file it changed (REP-10). A provider reports its own writes because only it knows
    /// what it touched - the alternative, having the caller infer the list from the unit's sources,
    /// would quietly go wrong the first time a provider wrote a file that was not one of them.
    /// </summary>
    public IReadOnlyList<string> WriteVersion(IPackageableUnit unit, Version version);

    /// <summary>
    /// Renders the unit's signature as the single child element of its baseline entry. Collections
    /// are sorted here, not by the caller - determinism is the provider's job (BAS-04).
    /// </summary>
    public XElement WriteSignature(IPackageableUnit unit);

    /// <summary>The inverse of <see cref="WriteSignature"/>, for reading a baseline back.</summary>
    public object ReadSignature(XElement element);
}
