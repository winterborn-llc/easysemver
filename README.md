# Auto-Version by Yamamari
This is intended to be a lightweight utility for synchronizing and incrementing the various version counters across a C# project file.

## Configuration Step 1
After adding the package to your project, you must configure it to generate a path we can refer to in the build process. This requires opening the `.csproj` file and directly modifying the xml.

Once it's opened, find the `ItemGroup` that includes the package import for `Yamamari.AutoVersion`.

The `Include` and `Version` attributes are added by default. You must add the attribute `GeneratePathProperty` to the `AutoVersion` package reference and set it to true.

```xml
<ItemGroup>
    <PackageReference 
            Include="Yamamari.AutoVersion" 
            GeneratePathProperty="true" 
            Version="1.0.2" />
</ItemGroup>
```

## Configure Step 2
Now that we have the package ready to be used, we need tell our build process how to use it.

This is also done by directly modifying the xml of our project's `.csproj` file.

We need to add an XML block to add our build step. The example below should work perfectly.

```xml
<Project Sdk="Microsoft.NET.Sdk">
   <UsingTask TaskName="AutoVersion" AssemblyFile="$(PkgYamamari_AutoVersion)/lib/net6.0/AutoVersion.dll" />
   <Target Name="IncrementTheVersion" AfterTargets="PostBuildEvent">
      <AutoVersion ProjectFile="./$(ProjectFileName)" />
   </Target>
</Project>
```

###### About The Settings

* `<UsingTask TaskName="AutoVersion"` tells us what class we're going to be loading from our assembly
* `$(PkgYamamari_AutoVersion)` is the macro that was automatically generated when we added the `GeneratePathProperty` attribute to our package import. This property tells us where on disk the specified NuGet package is stored. We needed to know that in order to tell the build system where to find the code for our custom task.
* `<UsingTask AssemblyFile="$(PkgYamamari_AutoVersion)/lib/net6.0/AutoVersion.dll"` is the exact location on disk for the dll that contains our custom build task
* `<Target Name="IncrementTheVersion"` merely names our step
* `<Target AfterTargets="PostBuildEvent"` tells MSBuild when to run this task. Ultimately this isn't important aside from determining whether the value will be incremented before or after we build. As long as we're consistent, it doesn't matter which we choose.
* `<AutoVersion ProjectFile="./$(ProjectFileName)" />` this invokes our custom build task and gives it the exact location on disk for the project file we're building - and whose version we intend to increment and synchronize

## Configuration Step 3
The utility will only override version settings that have already been provided to the `.csproj` file, it will not add them. So, be sure to add seed values to any versions you want to have synchronized and automatically incremented. 

You do not need to add all of these for the process to work.

```xml
<PropertyGroup>
    <AssemblyVersion>1.0.13</AssemblyVersion>
    <PackageVersion>1.0.13</PackageVersion>
    <FileVersion>1.0.13</FileVersion>
</PropertyGroup>
```