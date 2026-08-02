using System.Xml.Linq;
using Winterborn.Library.EasySemVer.DataObject;
using Version = Winterborn.Library.EasySemVer.DataObject.Version;

namespace Winterborn.Library.EasySemVer.Interfaces;

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
    /// </summary>
    public VersionType Classify(IPackageableUnit? older, IPackageableUnit newer);

    public IReadOnlyList<Version> ReadVersions(IPackageableUnit unit);

    public void WriteVersion(IPackageableUnit unit, Version version);

    /// <summary>
    /// Renders the unit's signature as the single child element of its baseline entry. Collections
    /// are sorted here, not by the caller - determinism is the provider's job (BAS-04).
    /// </summary>
    public XElement WriteSignature(IPackageableUnit unit);

    /// <summary>The inverse of <see cref="WriteSignature"/>, for reading a baseline back.</summary>
    public object ReadSignature(XElement element);
}
