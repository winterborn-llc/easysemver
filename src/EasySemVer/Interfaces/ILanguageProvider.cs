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
