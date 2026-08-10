namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>A project the baseline recorded is gone from this run. Replaces the retired R07.</summary>
public class UnitRemoved : UnitRemovedRule
{
    public override string Rule => "UnitRemoved";
}
