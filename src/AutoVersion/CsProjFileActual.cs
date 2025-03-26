using System.Diagnostics;
using System.Reflection;
using Yamamari.Library.AutoVersion.Extensions;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion;

[DebuggerDisplay("{CsProjFile.ProjectName}")]
internal class CsProjFileActual
{
    internal CsProjFile CsProjFile { get; }
    
    internal Project Project { get; }
    
    internal CsProjFileActual(CsProjFile csProjFile)
    {
        this.CsProjFile = csProjFile;
        if (!File.Exists(csProjFile.ProjectFilePath))
        {
            throw new FileNotFoundException(csProjFile.ProjectFilePath);
        }

        this.Project = GetActualSignature();
    }

    internal Project GetActualSignature()
    {
        var projectName = this.CsProjFile.ProjectName;
        if (projectName.Contains(this.CsProjFile.SolutionDirectory) &&
            !this.CsProjFile.SolutionDirectory.IsNullOrWhitespace())
        {
            projectName = this.CsProjFile.ProjectName.Replace(this.CsProjFile.SolutionDirectory, string.Empty);
        }
        
        var projectFilePath = this.CsProjFile.ProjectFilePath;
        var projectXml = this.CsProjFile.ProjectXml;
        
        var signatureProject = new Project { Name = projectName };
        var assembly = GetAssemblyForSignature(projectFilePath, projectXml);
        if (assembly == null)
        {
            return signatureProject;
        }
        
        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsPublic)
            {
                continue;
            }
                
            var signatureClass = new ProjectClass
            {
                Name = $"{type.Namespace}.{type.Name}",
                Methods = new Dictionary<string, Method>()
            };

            signatureProject.Classes.Add(signatureClass);
            AddPropertiesOfTypeToSignature(type, signatureClass);
            AddMethodsOfTypeToSignature(type, signatureClass);
        }

        return signatureProject;
    }
    
    private static void AddMethodsOfTypeToSignature(Type type, ProjectClass projectClass)
    {
        var methods = type.GetMethods();
        foreach (var method in methods)
        {
            AddMethodToSignature(projectClass, method);
        }
    }

    private static void AddMethodToSignature(ProjectClass projectClass, MethodInfo method)
    {
        if (!method.IsPublic)
        {
            return;
        }
        
        if (method.MemberType == MemberTypes.Property)
        {
            return;
        }
        
        var thisOverride = GetOverride(method);
        var signatureMethod = GetMethod(projectClass, method);
        signatureMethod.Overrides.Add(thisOverride);
    }

    private static MethodOverride GetOverride(MethodInfo method)
    {
        var thisOverride = new MethodOverride();
        foreach (var param in method.GetParameters())
        {
            thisOverride.Add(new MethodOverrideInput
            {
                ParameterName = param.Name ?? string.Empty,
                ParameterType = param.ParameterType.Name,
                IsRequired = !param.IsOptional
            });
        }

        return thisOverride;
    }

    private static Method GetMethod(ProjectClass projectClass, MethodInfo method)
    {
        if (projectClass.Methods.ContainsKey(method.Name))
        {
            return projectClass.Methods[method.Name];
        }
        
        var signatureMethod = new Method
        {
            MethodName = method.Name,
            MethodType = method.ReturnType.Name,
            Overrides = []
        };
        
        projectClass.Methods.Add(signatureMethod.MethodName, signatureMethod);
        return signatureMethod;
    }

    private static void AddPropertiesOfTypeToSignature(Type type, ProjectClass projectClass)
    {
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            AddPropertyToSignature(projectClass, property);
        }
    }

    private static void AddPropertyToSignature(ProjectClass projectClass, PropertyInfo property)
    {
        if (!property.CanRead && !property.CanWrite)
        {
            return;
        }
        
        var signatureProperty = new Property
        {
            Name = property.Name,
            Type = property.PropertyType.Name,
            IsWritable = property.CanWrite,
            IsReadable = property.CanRead,
        };
        
        projectClass.Properties.Add(signatureProperty.Name, signatureProperty);
    }
    
    private static Assembly? GetAssemblyForSignature(string projectFilePath, string projectXml)
    {
        var fileInfo = new FileInfo(projectFilePath);
        var directory = fileInfo.Directory;
        if (directory == null)
        {
            return null;
        }

        var bin = directory.GetSubDirectory("bin");
        if (bin == null)
        {
            return null;
        }
        
        var assemblyName = GetAssemblyName(projectFilePath, projectXml);
        var latest = GetLatestAssemblyFile(bin, assemblyName);
        if (latest == null)
        {
            return null;
        }

        var assembly = Assembly.LoadFile(latest.FullName);
        return assembly;
    }
    
    private static FileInfo? GetLatestAssemblyFile(DirectoryInfo bin, string assemblyName)
    {
        FileInfo? latest = null;
        var allFiles = new List<FileInfo>();
        RecursivelyCollectAllAssemblyFilesOfName(bin, assemblyName, allFiles);
        foreach (var file in allFiles)
        {
            if (latest == null)
            {
                latest = file;
                continue;
            }
            
            if (file.LastWriteTime < latest.LastWriteTime)
            {
                continue;
            }
            
            latest = file;
        }

        return latest;
    }
    
    private static void RecursivelyCollectAllAssemblyFilesOfName(DirectoryInfo bin, string assemblyName, List<FileInfo> files)
    {
        var subDirs = bin.GetDirectories();
        foreach (var subDir in subDirs)
        {
            RecursivelyCollectAllAssemblyFilesOfName(subDir, assemblyName, files);
        }

        var file = GetAssemblyFile(bin, assemblyName);
        if (file != null)
        {
            files.Add(file);
        }
    }
    
    private static FileInfo? GetAssemblyFile(DirectoryInfo bin, string assemblyName)
    {
        var files = bin.GetFiles();
        foreach (var file in files)
        {
            if (file.Name != assemblyName)
            {
                continue;
            }

            return file;
        }

        return null;
    }

    private static string GetAssemblyName(string projectFilePath, string projectXml)
    {
        var fileExt = projectXml.GetXmlNodeValue("OutputType") ?? "dll";
        var customName = projectXml.GetXmlNodeValue("AssemblyName");
        var defaultName = GetDefaultFileName(projectFilePath);
        if (customName.IsNullOrWhitespace())
        {
            return $"{defaultName}.{fileExt}";
        }

        return $"{customName}.{fileExt}";
    }

    private static string GetDefaultFileName(string projectFilePath)
    {
        var fileInfo = new FileInfo(projectFilePath);
        var fileExt = fileInfo.Extension;
        var fileName = fileInfo.Name;
        var defaultName = fileName.Replace(fileExt, "");
        return $"{defaultName}";
    }
}