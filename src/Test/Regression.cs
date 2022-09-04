using System.IO;
using Yamamari.Library.VersionCounter;

namespace Test;

public class Regression
{
    [Fact]
    public void TestProgramInvocation()
    {
        var source = "SampleCsProj.xml";
        var target = "SampleCsProj.xml";
        this.UpdateTestFile(source);
        
        var expected = File.ReadAllText(target);
        var actual = File.ReadAllText(source);
        
        Program.Main(new [] {source});
        Assert.Equal(expected, actual, true, true);
        File.Delete(source);
        File.Delete(target);
    }
    
    [Fact]
    public void TestProjectUpdate()
    {
        var source = "SampleCsProj.xml";
        var target = "SampleCsProj.xml";
        this.UpdateTestFile(source);
        
        var expected = File.ReadAllText(target);
        var actual = File.ReadAllText(source);
        
        IncrementFileVersion.Go(source);
        Assert.Equal(expected, actual, true, true);
        File.Delete(source);
        File.Delete(target);
    }

    private void UpdateTestFile(string source)
    {
        File.WriteAllText(source, @"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>net6.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <AssemblyName>Yamamari.Library.PluginArchitecture</AssemblyName>
        <RootNamespace>Yamamari.Library.PluginArchitecture</RootNamespace>
        <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
        <PackageId>Yamamari.PluginArchitecture</PackageId>
        <Title>Plug-in Architecture by Yamamari</Title>
        <PackageProjectUrl>https://github.com/yamamari-llc/library-pluginarchitecture</PackageProjectUrl>
        <PackageLicenseUrl>https://github.com/yamamari-llc/library-pluginarchitecture/blob/main/LICENSE</PackageLicenseUrl>
        <RepositoryUrl>https://github.com/yamamari-llc/library-pluginarchitecture</RepositoryUrl>
        <RepositoryType>Git</RepositoryType>
        <PackageIcon>Resources\yamamari-logo-pluginarchitecture.png</PackageIcon>
        <PackageVersion>1.0.2</PackageVersion>
        <AssemblyVersion>1.0.1</AssemblyVersion>
        <FileVersion>1.0.3</FileVersion>
    </PropertyGroup>
    <ItemGroup>
        <Folder Include=""Resources"" />
    </ItemGroup>
    <ItemGroup>
        <None Remove=""Resources\yamamari-logo-pluginarchitecture.png"" />
        <EmbeddedResource Include=""Resources\yamamari-logo-pluginarchitecture.png"">
            <Pack>True</Pack>
            <PackagePath></PackagePath>
        </EmbeddedResource>
    </ItemGroup>
</Project>");
    }
}