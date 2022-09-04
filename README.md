# VersionCounter by Yamamari
This is intended to be a lightweight utility for synchronizing and incrementing the various version counters across a C# project file.

## Configuration Step 1
Other than adding the package to your project, there is only one other step that is required.

Within your project settings, set the post-build action to the following:


```config
./$(OutDir)VersionCounter ./$(ProjectFileName)
```

If that is unclear or the project settings cannot be edited via the UI, you can modify the `.csproj` file by appending the following node.

```xml
  <Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <Exec Command="./$(OutDir)VersionCounter ./$(ProjectFileName)" />
  </Target>
```

## Configuration Step 2
The utility will only override version settings that have already been provided to the `.csproj` file, it will not add them. So, be sure to add seed values to any versions you want to have synchronized and automatically incremented.

## Scope
The settings that are synchronized are:
* AssemblyVersion
* PackageVersion
* FileVersion