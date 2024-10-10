using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Yamamari.AutoVersion.Extensions;

namespace Yamamari.AutoVersion;

public class SignatureBuilder
{
    public static Signature? GetSignatureFor(ITask task, string projectFilePath, string projectXml)
    {
        task.LogWarn($"Project File Path: {projectFilePath}");
        task.LogWarn($"Project XML: {projectXml}");
        var assembly = GetAssemblyForSignature(task, projectFilePath, projectXml);
        if (assembly == null)
        {
            return null;
        }

        var signature = new Signature();
        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsPublic)
            {
                continue;
            }
            
            var signatureClass = new SignatureClass
            {
                ClassName = type.Name,
                Methods = new List<SignatureClassMethod>()
            };

            signature.Add(signatureClass);

            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (!property.CanRead  && !property.CanWrite)
                {
                    continue;
                }
                
                var signatureMethod = new SignatureClassMethod
                {
                    MethodName = property.Name,
                    Parameters = new List<SignatureClassMethodInput>()
                };
                
                signatureMethod.Parameters.Add(new SignatureClassMethodInput
                {
                    ParameterName = property.Name,
                    ParameterType = property.PropertyType.Name
                });
                
                signatureClass.Methods.Add(signatureMethod);
            }
            
            var methods = type.GetMethods();
            foreach (var method in methods)
            {
                if (!method.IsPublic)
                {
                    continue;
                }
                
                var signatureMethod = new SignatureClassMethod
                {
                    MethodName = method.Name,
                    Parameters = new List<SignatureClassMethodInput>()
                };

                foreach (var param in method.GetParameters())
                {
                    signatureMethod.Parameters.Add(new SignatureClassMethodInput
                    {
                        ParameterName = param.Name ?? string.Empty,
                        ParameterType = param.ParameterType.Name,
                        IsRequired = !param.IsOptional
                    });
                }
                
                signatureClass.Methods.Add(signatureMethod);
            }
        }
        
        return signature;
    }

    private static Assembly? GetAssemblyForSignature(ITask task, string projectFilePath, string projectXml)
    {
        task.LogWarn($"Project File Path: {projectFilePath}");
        task.LogWarn($"Project XML: {projectXml}");
        var fileInfo = new FileInfo(projectFilePath);
        var directory = fileInfo.Directory;
        if (directory == null)
        {
            return null;
        }

        var assemblyName = GetAssemblyName(task, projectFilePath, projectXml);
        var bin = directory.GetSubDirectory("bin");
        if (bin == null)
        {
            return null;
        }
        
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