namespace Winterborn.Tools.EasySemVer.Interfaces.Csharp;

public interface ICsharpGenericParameter
{
    public string Name { get; }

    /// <summary>
    /// The parameter's constraints, sorted and comma-joined, so R39/R40 can compare them as sets
    /// without the declaration order mattering.
    /// </summary>
    public string Constraints { get; }
}
