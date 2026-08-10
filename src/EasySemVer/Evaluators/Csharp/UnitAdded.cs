namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>A project exists this run that the baseline never saw. Replaces the retired R14.</summary>
public class UnitAdded : UnitAddedRule
{
    public override string Rule => "UnitAdded";
}
