using Microsoft.Build.Locator;
using Winterborn.Library.EasySemVer.CodeReader;

namespace Test;

public class Experimental
{
    static Experimental()
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }
    }
    
    [Fact]
    public void Debug()
    {
        var analyzer = new SolutionBuilder();

        SolutionBuilder.GetProjectSignature("/Users/andrew/code/Winterborn-EasySemVer/src/EasySemVer/EasySemVer.csproj");
    }
}