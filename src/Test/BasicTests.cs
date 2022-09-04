using Yamamari.Library.VersionCounter;

namespace Test;

public class BasicTests
{
    [Fact]
    public void LearningTest()
    {
        var version = new Version("1.0.2");
        Assert.Equal(1, version.Major);
        Assert.Equal(0, version.Minor);
        Assert.Equal(2, version.Build);
    }
    
    [Fact]
    public void TestProjectWithNoVersions()
    {
        // ./$(OutDir)VersionCounter ./$(ProjectFileName)
        const string targetXml = @"<Project Sdk=""Microsoft.NET.Sdk""><PropertyGroup><TargetFramework>net6.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><AssemblyName>Yamamari.Library.PluginArchitecture</AssemblyName><RootNamespace>Yamamari.Library.PluginArchitecture</RootNamespace><GeneratePackageOnBuild>true</GeneratePackageOnBuild><PackageId>Yamamari.PluginArchitecture</PackageId><Title>Plug-in Architecture by Yamamari</Title><PackageProjectUrl>https://github.com/yamamari-llc/library-pluginarchitecture</PackageProjectUrl><PackageLicenseUrl>https://github.com/yamamari-llc/library-pluginarchitecture/blob/main/LICENSE</PackageLicenseUrl><RepositoryUrl>https://github.com/yamamari-llc/library-pluginarchitecture</RepositoryUrl><RepositoryType>Git</RepositoryType><PackageIcon>Resources\yamamari-logo-pluginarchitecture.png</PackageIcon></PropertyGroup><ItemGroup><Folder Include=""Resources"" /></ItemGroup><ItemGroup><None Remove=""Resources\yamamari-logo-pluginarchitecture.png"" /><EmbeddedResource Include=""Resources\yamamari-logo-pluginarchitecture.png""><Pack>True</Pack><PackagePath></PackagePath></EmbeddedResource></ItemGroup></Project>";
        const string sourceXml = @"<Project Sdk=""Microsoft.NET.Sdk"">
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
</Project>";
        
        var handler = new VersionHandler(sourceXml);
        Assert.Equal("1.0.0", handler.SourceVersion.ToString());
        Assert.Equal("1.0.1", handler.TargetVersion.ToString());
        Assert.Equal(targetXml, handler.TargetXml);
    }
    
    [Fact]
    public void TestProjectUpdate()
    {
        // ./$(OutDir)VersionCounter ./$(ProjectFileName)
        const string targetXml = @"<Project Sdk=""Microsoft.NET.Sdk""><PropertyGroup><TargetFramework>net6.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><AssemblyName>Yamamari.Library.PluginArchitecture</AssemblyName><RootNamespace>Yamamari.Library.PluginArchitecture</RootNamespace><GeneratePackageOnBuild>true</GeneratePackageOnBuild><PackageId>Yamamari.PluginArchitecture</PackageId><Title>Plug-in Architecture by Yamamari</Title><PackageProjectUrl>https://github.com/yamamari-llc/library-pluginarchitecture</PackageProjectUrl><PackageLicenseUrl>https://github.com/yamamari-llc/library-pluginarchitecture/blob/main/LICENSE</PackageLicenseUrl><RepositoryUrl>https://github.com/yamamari-llc/library-pluginarchitecture</RepositoryUrl><RepositoryType>Git</RepositoryType><PackageIcon>Resources\yamamari-logo-pluginarchitecture.png</PackageIcon><PackageVersion>1.0.2</PackageVersion><AssemblyVersion>1.0.2</AssemblyVersion><FileVersion>1.0.2</FileVersion></PropertyGroup><ItemGroup><Folder Include=""Resources"" /></ItemGroup><ItemGroup><None Remove=""Resources\yamamari-logo-pluginarchitecture.png"" /><EmbeddedResource Include=""Resources\yamamari-logo-pluginarchitecture.png""><Pack>True</Pack><PackagePath></PackagePath></EmbeddedResource></ItemGroup></Project>";
        const string sourceXml = @"<Project Sdk=""Microsoft.NET.Sdk"">
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
</Project>";
        
        var handler = new VersionHandler(sourceXml);
        Assert.Equal("1.0.1", handler.SourceVersion.ToString());
        Assert.Equal("1.0.2", handler.TargetVersion.ToString());
        Assert.Equal(targetXml, handler.TargetXml);
    }
    
    [Fact]
    public void TestMajorUpdate()
    {
        // ./$(OutDir)VersionCounter ./$(ProjectFileName)
        const string targetXml = @"<Project Sdk=""Microsoft.NET.Sdk""><PropertyGroup><TargetFramework>net6.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><AssemblyName>Yamamari.Library.PluginArchitecture</AssemblyName><RootNamespace>Yamamari.Library.PluginArchitecture</RootNamespace><GeneratePackageOnBuild>true</GeneratePackageOnBuild><PackageId>Yamamari.PluginArchitecture</PackageId><Title>Plug-in Architecture by Yamamari</Title><PackageProjectUrl>https://github.com/yamamari-llc/library-pluginarchitecture</PackageProjectUrl><PackageLicenseUrl>https://github.com/yamamari-llc/library-pluginarchitecture/blob/main/LICENSE</PackageLicenseUrl><RepositoryUrl>https://github.com/yamamari-llc/library-pluginarchitecture</RepositoryUrl><RepositoryType>Git</RepositoryType><PackageIcon>Resources\yamamari-logo-pluginarchitecture.png</PackageIcon><PackageVersion>1.1.0</PackageVersion><AssemblyVersion>1.1.0</AssemblyVersion><FileVersion>1.1.0</FileVersion></PropertyGroup><ItemGroup><Folder Include=""Resources"" /></ItemGroup><ItemGroup><None Remove=""Resources\yamamari-logo-pluginarchitecture.png"" /><EmbeddedResource Include=""Resources\yamamari-logo-pluginarchitecture.png""><Pack>True</Pack><PackagePath></PackagePath></EmbeddedResource></ItemGroup></Project>";
        const string sourceXml = @"<Project Sdk=""Microsoft.NET.Sdk"">
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
</Project>";
        
        var handler = new VersionHandler(sourceXml, true);
        Assert.Equal("1.0.1", handler.SourceVersion.ToString());
        Assert.Equal("1.1.0", handler.TargetVersion.ToString());
        Assert.Equal(targetXml, handler.TargetXml);
    }
}