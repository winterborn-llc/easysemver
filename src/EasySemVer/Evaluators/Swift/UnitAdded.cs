namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>A target exists this run that the baseline never saw.</summary>
public class UnitAdded : UnitAddedRule
{
    public override string Rule => "UnitAdded";
}
