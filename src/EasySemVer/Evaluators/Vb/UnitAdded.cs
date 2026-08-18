namespace Winterborn.Tools.EasySemVer.Evaluators.Vb;

/// <summary>
/// A .vbproj exists this run that the baseline never saw. Owned by VB rather than borrowed from C#
/// even though VB shares C#'s signature model (VB-01): a rule belongs to exactly one language
/// (ML-04), and the report's key is (language, rule), so "vb"/"UnitAdded" is a different finding
/// from "csharp"/"UnitAdded" and needs a class that can say so.
/// </summary>
public class UnitAdded : UnitAddedRule
{
    public override string Rule => "UnitAdded";
}
