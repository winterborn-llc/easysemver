using System.Reflection;
using Microsoft.Build.Framework;
using Yamamari.Library.AutoVersion.Extensions;

namespace Yamamari.Library.AutoVersion.Signatures;

internal class SignatureBuilder
{
    public static Signature? GetSignatureFor(ITask task, params CsProjFile[] csProjFiles)
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

            var signatureProject = new SignatureProject { ProjectName = csProjFile.ProjectFilePath };
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsPublic)
                {
                    continue;
                }
                
                var signatureClass = new SignatureProjectClass
                {
                    ClassName = $"{type.Namespace}.{type.Name}",
                    Methods = new List<SignatureProjectClassMethod>()
                };

                signatureProject.Add(signatureClass);
                AddPropertiesOfTypeToSignature(type, signatureClass);
                AddMethodsOfTypeToSignature(type, signatureClass);
            }
            
            signature.Add(signatureProject);
        }
        
        return signature;
    }

    private static void AddMethodsOfTypeToSignature(Type type, SignatureProjectClass signatureProjectClass)
    {
        var methods = type.GetMethods();
        foreach (var method in methods)
        {
            AddMethodToSignature(signatureProjectClass, method);
        }
    }

    private static void AddMethodToSignature(SignatureProjectClass signatureProjectClass, MethodInfo method)
    {
        if (!method.IsPublic)
        {
            return;
        }
                
        var signatureMethod = new SignatureProjectClassMethod
        {
            MethodName = method.Name,
            MethodType = method.ReturnType.Name,
            Parameters = new List<SignatureProjectClassMethodInput>()
        };

        foreach (var param in method.GetParameters())
        {
            signatureMethod.Parameters.Add(new SignatureProjectClassMethodInput
            {
                ParameterName = param.Name ?? string.Empty,
                ParameterType = param.ParameterType.Name,
                IsRequired = !param.IsOptional
            });
        }
                
        signatureProjectClass.Methods.Add(signatureMethod);
    }

    private static void AddPropertiesOfTypeToSignature(Type type, SignatureProjectClass signatureProjectClass)
    {
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            AddPropertyToSignature(signatureProjectClass, property);
        }
    }

    private static void AddPropertyToSignature(SignatureProjectClass signatureProjectClass, PropertyInfo property)
    {
        if (!property.CanRead  && !property.CanWrite)
        {
            return;
        }
                
        var signatureProperty = new SignatureProjectClassProperty
        {
            Name = property.Name,
            Type = property.PropertyType.Name,
            IsWritable = property.CanWrite,
            IsReadable = property.CanRead,
        };
                
        signatureProjectClass.Properties.Add(signatureProperty);
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