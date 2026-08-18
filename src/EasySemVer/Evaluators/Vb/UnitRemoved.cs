namespace Winterborn.Tools.EasySemVer.Evaluators.Vb;

/// <summary>A .vbproj the baseline recorded is gone from this run. Owned by VB per ML-04.</summary>
public class UnitRemoved : UnitRemovedRule
{
    public override string Rule => "UnitRemoved";
}
