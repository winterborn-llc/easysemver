using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Evaluators;

/// <summary>
/// "A unit the baseline recorded is gone from this run." Every language has this rule, and every
/// language owns its own copy of it, because a rule belongs to exactly one language (ML-04) - the
/// report's key is (language, rule), so there is no such thing as a rule that spans languages.
/// <para>
/// What is shared is only the diffing. Subclass and declare a name to agree with every other
/// language; override <see cref="FindDifferences"/> to disagree. A language whose units can move
/// between containers, or whose removal is not simply an absence, has somewhere to say so.
/// </para>
/// </summary>
public abstract class UnitRemovedRule : IEvaluateUnitExistence
{
    /// <summary>
    /// Abstract, and never defaulted from the class name. The published key has to be a literal
    /// that a class rename cannot move, so a base that filled this in would quietly undo the
    /// reason it is carried at all.
    /// </summary>
    public abstract string Rule { get; }

    public virtual VersionType EvaluationImpact => VersionType.Major;

    public virtual string ChangeDescription => "was removed";

    public virtual IEnumerable<IPackageableUnit> FindDifferences(IUnitsToCompare units)
    {
        foreach (var olderUnit in units.Older)
        {
            if (UnitPairing.Find(units.Newer, olderUnit) != null)
            {
                continue;
            }

            yield return olderUnit;
        }
    }
}
