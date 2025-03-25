# Auto-Version by Yamamari
This is intended to be a lightweight utility for synchronizing and incrementing the various version counters across a C# solution.

## Configuration Step 1
Add the package to any one project in your solution. It does not matter which project you add it to as long as the project you select will be built whenever you want the versions updated.
The process will update the versions of all projects in the solution.

```xml
<ItemGroup>
    <PackageReference 
            Include="Yamamari.AutoVersion"
            Version="5.1.4"/>
</ItemGroup>
```

## Configuration Step 2
After adding the package to your project, you must configure it to execute a post-build process. This must be post-build so the dlls exist and can be inspected. This requires opening the `.csproj` file and directly modifying the xml.

Once it's opened, find the `ItemGroup` that includes the package import for `Yamamari.AutoVersion`.

You must add the attribute `GeneratePathProperty` to the `AutoVersion` package reference and set it to true.

```xml
<ItemGroup>
    <PackageReference 
            Include="Yamamari.AutoVersion" 
            GeneratePathProperty="true" 
            Version="5.1.4" />
</ItemGroup>
```

## Configure Step 3
Now that we have the package ready to be used, we need tell our build process how to use it. This is also done by directly modifying the xml of our project's `.csproj` file.

We need to add an XML block to add our build step. The example below should work perfectly.

```xml
<Project Sdk="Microsoft.NET.Sdk">
   <UsingTask TaskName="AutoVersion" AssemblyFile="$(PkgYamamari_AutoVersion)/lib/net9.0/AutoVersion.dll" />
   <Target Name="IncrementTheVersion" AfterTargets="PostBuildEvent">
      <AutoVersion />
   </Target>
</Project>
```

###### About The Settings

* `<UsingTask TaskName="AutoVersion"` tells us what class we're going to be loading from our assembly
* `$(PkgYamamari_AutoVersion)` is the macro that was automatically generated when we added the `GeneratePathProperty` attribute to our package import. This property tells us where on disk the specified NuGet package is stored. We needed to know that in order to tell the build system where to find the code for our custom task.
* `<Target Name="IncrementTheVersion"` merely names our step
* `<Target AfterTargets="PostBuildEvent"` tells MSBuild when to run this task. Ultimately this isn't important aside from determining whether the value will be incremented before or after we build. As long as we're consistent, it doesn't matter which we choose.
* `<AutoVersion />` this invokes our custom build task

## Configuration Step 4
The utility will only override version settings that have already been provided to the `.csproj` file, it will not add them. So, be sure to add seed values to any versions you want to have synchronized and automatically incremented. 

You do not need to add all of these for the process to work.

If any of these versions don't match, the utility will take the highest of these versions from across all projects and start keeping the values in sync going forward.

```xml
<PropertyGroup>
    <AssemblyVersion>1.0.13</AssemblyVersion>
    <PackageVersion>1.0.13</PackageVersion>
    <FileVersion>1.0.13</FileVersion>
</PropertyGroup>
```