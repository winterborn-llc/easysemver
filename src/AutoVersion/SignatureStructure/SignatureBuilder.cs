using System.Reflection;
using Microsoft.Build.Framework;
using Yamamari.Library.AutoVersion.Extensions;

namespace Yamamari.Library.AutoVersion.SignatureStructure;

internal class SignatureBuilder
{
    public static Signature? GetSignatureFor(ITask task, string solutionDirectory, params CsProjFile[] csProjFiles)
    {
        var signature = new Signature();
        foreach (var csProjFile in csProjFiles)
        {
            var projectXml = csProjFile.ProjectXml;
            var projectFilePath = csProjFile.ProjectFilePath;
            task.LogWarn($"Project File Path: {projectFilePath}");
            var assembly = GetAssemblyForSignature(task, projectFilePath, projectXml);
            if (assembly == null)
            {
                return null;
            }

            var projectName = csProjFile.ProjectName.Replace(solutionDirectory, string.Empty);
            var signatureProject = new Project { Name = projectName };
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
            
            signature.Add(signatureProject);
        }
        
        return signature;
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
        if (!property.CanRead  && !property.CanWrite)
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

    private static Assembly? GetAssemblyForSignature(ITask task, string projectFilePath, string projectXml)
    {
        task.LogWarn($"Project File Path: {projectFilePath}");
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
        
        var assemblyName = GetAssemblyName(task, projectFilePath, projectXml);
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

    private static string GetAssemblyName(ITask task, string projectFilePath, string projectXml)
    {
        var fileExt = projectXml.GetXmlNodeValue("OutputType") ?? "dll";
        var customName = projectXml.GetXmlNodeValue("AssemblyName");
        var defaultName = GetDefaultFileName(projectFilePath);
        if (customName.IsNullOrWhitespace())
        {
            task.LogWarn($"Custom Assembly Name: {defaultName}.{fileExt}");
            return $"{defaultName}.{fileExt}";
        }

        task.LogWarn($"Default Assembly Name: {defaultName}.{fileExt}");
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