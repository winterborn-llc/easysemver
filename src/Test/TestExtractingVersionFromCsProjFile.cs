using Winterborn.Tools.EasySemVer.CodeReader;
using Winterborn.Tools.EasySemVer.CodeReader.Csharp;

namespace Test;

public class TestExtractingVersionFromCsProjFile
{
    [Fact]
    public void TestProjectWithNoVersions()
    {
        const string sourceXml = @"<CsharpProject Sdk=""Microsoft.NET.Sdk"">
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
</CsharpProject>";
        
        var version = new CsProjFileVersion(sourceXml).Version;
        Assert.Equal("0.0.0", version);
    }
    
    [Fact]
    public void TestMajorUpdate()
    {
        const string sourceXml = @"<CsharpProject Sdk=""Microsoft.NET.Sdk"">
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
</CsharpProject>";
        
        var version = new CsProjFileVersion(sourceXml).Version;
        Assert.Equal("1.0.3", version);
    }
}