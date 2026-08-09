using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>Availability facts as S25 and S26 need them (SWM-03).</summary>
internal static class SwiftAvailabilityFacts
{
    internal static bool IsWithdrawn(ISwiftDeclaration declaration)
    {
        foreach (var clause in declaration.Availability)
        {
            if (clause.IsUnavailable || clause.Obsoleted.Length > 0)
            {
                return true;
            }
        }

        return false;
    }

    internal static bool IsDeprecated(ISwiftDeclaration declaration)
    {
        foreach (var clause in declaration.Availability)
        {
            if (clause.IsDeprecated || clause.Deprecated.Length > 0)
            {
                return true;
            }
        }

        return false;
    }
}
