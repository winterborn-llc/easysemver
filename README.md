# library-versioncounter



```config
./$(OutDir)VersionCounter ./$(ProjectFileName)
```

```xml
  <Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <Exec Command="./$(OutDir)VersionCounter ./$(ProjectFileName)" />
  </Target>
```
