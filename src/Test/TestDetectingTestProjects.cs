using Winterborn.Tools.EasySemVer.CodeReader.Csharp;

namespace Test;

/// <summary>
/// UNI-04 - which .csproj files are test projects, decided from the project file and nothing else.
/// Every case here is a string, so the whole rule is exercised without a project on disk.
/// </summary>
public class TestDetectingTestProjects
{
    private static string Project(string body)
    {
        return $"""
            <Project Sdk="Microsoft.NET.Sdk">
               <PropertyGroup>
                  <TargetFramework>net10.0</TargetFramework>
                  <AssemblyVersion>2.3.4</AssemblyVersion>
               </PropertyGroup>
            {body}
            </Project>
            """;
    }

    private static string WithPackage(string package)
    {
        return Project($"""
               <ItemGroup>
                  <PackageReference Include="{package}" Version="1.0.0" />
               </ItemGroup>
            """);
    }

    /// <summary>
    /// The reference every runnable test project carries whichever framework it uses, so it is the
    /// signal that matters most.
    /// </summary>
    [Fact]
    public void TheTestSdkReferenceMakesItATestProject()
    {
        Assert.True(CsProjTestProject.IsTestProject(WithPackage("Microsoft.NET.Test.Sdk")));
    }

    /// <summary>
    /// Matched as prefixes, so a framework's whole family of packages is covered without listing
    /// each one - and so a project that has the framework but not yet the SDK reference still
    /// reads as a test project.
    /// </summary>
    [Theory]
    [InlineData("xunit")]
    [InlineData("xunit.v3")]
    [InlineData("xunit.runner.visualstudio")]
    [InlineData("NUnit")]
    [InlineData("NUnit3TestAdapter")]
    [InlineData("MSTest.TestFramework")]
    public void EachTestFrameworkIsRecognised(string package)
    {
        Assert.True(CsProjTestProject.IsTestProject(WithPackage(package)));
    }

    [Fact]
    public void AnOrdinaryLibraryIsNotATestProject()
    {
        Assert.False(CsProjTestProject.IsTestProject(WithPackage("Newtonsoft.Json")));
        Assert.False(CsProjTestProject.IsTestProject(Project(string.Empty)));
    }

    /// <summary>
    /// The escape hatch, and the reason it is MSBuild's own property rather than one this tool
    /// invented: a library that references a test framework deliberately - assertion helpers, a
    /// testing extension package - says so once and keeps its API surface.
    /// </summary>
    [Fact]
    public void AnExplicitPropertyWinsOverTheReferences()
    {
        var declaredNotATest = Project("""
               <PropertyGroup>
                  <IsTestProject>false</IsTestProject>
               </PropertyGroup>
               <ItemGroup>
                  <PackageReference Include="xunit" Version="2.9.3" />
               </ItemGroup>
            """);

        Assert.False(CsProjTestProject.IsTestProject(declaredNotATest));

        var declaredATest = Project("""
               <PropertyGroup>
                  <IsTestProject>true</IsTestProject>
               </PropertyGroup>
            """);

        Assert.True(CsProjTestProject.IsTestProject(declaredATest));
    }

    /// <summary>
    /// No name matching, deliberately. A project called <c>Tests</c> that is a fixture library, and
    /// one called <c>Fixtures</c> that is a test project, are both commonplace - a filename
    /// heuristic gets each of them wrong, and silently.
    /// </summary>
    [Fact]
    public void TheProjectNameIsNeverConsulted()
    {
        Assert.False(CsProjTestProject.IsTestProject(Project("""
               <PropertyGroup>
                  <AssemblyName>Widgets.Tests</AssemblyName>
               </PropertyGroup>
            """)));
    }

    /// <summary>
    /// A pre-SDK project puts every element in the 2003 MSBuild namespace. Matching on the local
    /// name is what stops those reading as an ordinary library with no test references at all.
    /// </summary>
    [Fact]
    public void ALegacyNamespacedProjectIsStillRead()
    {
        var legacy = """
            <Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
               <ItemGroup>
                  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
               </ItemGroup>
            </Project>
            """;

        Assert.True(CsProjTestProject.IsTestProject(legacy));
    }

    /// <summary>
    /// An unreadable project keeps the surface it would have had. Failing here would turn a
    /// malformed file into a run-ending error in a stage that has no business reporting one - the
    /// version source and the signature builder each say so in their own terms.
    /// </summary>
    [Theory]
    [InlineData("<Project><ItemGroup></Project>")]
    [InlineData("")]
    [InlineData("not xml at all")]
    public void AnUnreadableProjectIsNotATestProject(string projectXml)
    {
        Assert.False(CsProjTestProject.IsTestProject(projectXml));
    }

    /// <summary>
    /// A value that is neither true nor false is not a declaration, so the references decide. A
    /// `bool.TryParse` failure silently returning false would strip the surface off a library that
    /// merely typo'd the property.
    /// </summary>
    [Fact]
    public void AnUnparseablePropertyFallsBackToTheReferences()
    {
        var typoed = Project("""
               <PropertyGroup>
                  <IsTestProject>yes</IsTestProject>
               </PropertyGroup>
            """);

        Assert.False(CsProjTestProject.IsTestProject(typoed));

        var typoedWithFramework = Project("""
               <PropertyGroup>
                  <IsTestProject>yes</IsTestProject>
               </PropertyGroup>
               <ItemGroup>
                  <PackageReference Include="xunit" Version="2.9.3" />
               </ItemGroup>
            """);

        Assert.True(CsProjTestProject.IsTestProject(typoedWithFramework));
    }
}
