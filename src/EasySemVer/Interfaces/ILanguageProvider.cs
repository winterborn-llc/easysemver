using System.Xml.Linq;
using Winterborn.Tools.EasySemVer.DataObject;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.Interfaces;

/// <summary>
/// Everything one language contributes to a run (ML-02). Adding a language means writing one of
/// these, adding its <see cref="DataObject.Language"/> member, and registering it - no edit to
/// anything under Interfaces/, Evaluation/, or Persistence/.
/// </summary>
public interface ILanguageProvider
{
    public Language Language { get; }

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
    /// Compares two units of this language that exist on both sides (NCL-03). Unit existence is the
    /// neutral core's business, so <paramref name="older"/> is only ever null defensively (NCL-04).
    /// Returns what it found rather than a verdict: the caller aggregates the impacts (CLS-03) and
    /// the same findings are what the run reports (§20 O-04). An empty list is a Patch.
    /// </summary>
    public IReadOnlyList<ChangeFinding> Classify(IPackageableUnit? older, IPackageableUnit newer);

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
