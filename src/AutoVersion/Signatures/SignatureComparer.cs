namespace Yamamari.Library.AutoVersion.Signatures;

public static class SignatureComparer
{
    public static VersionType GetChangeType(Signature? oldSignature, Signature? newSignature)
    {
        if (oldSignature is null || newSignature is null)
        {
            return VersionType.Minor;
        }
        
        if (IsMajorChange(oldSignature, newSignature))
        {
            return VersionType.Major;
        }
        
        if (IsMinorChange(oldSignature, newSignature))
        {
            return VersionType.Minor;
        }

        return VersionType.Patch;
    }

    private static bool IsMajorChange(Signature oldSignature, Signature newSignature)
    {
        foreach (var oldProject in oldSignature)
        {
            var newProject = newSignature.FirstOrDefault(p => p.ProjectName == oldProject.ProjectName);
            foreach (var oldClass in oldProject)
            {
                var newClass = newProject?.FirstOrDefault(c => c.ClassName == oldClass.ClassName);
                if (newClass == null)
                {
                    return true;
                }

                foreach (var oldMethod in oldClass.Methods)
                {
                    var newMethod = newClass.Methods.FirstOrDefault(m => m.MethodName == oldMethod.MethodName);
                    if (newMethod == null)
                    {
                        return true;
                    }

                    if (oldMethod.MethodType != newMethod.MethodType)
                    {
                        return true;
                    }

                    if (newMethod.Parameters.Count < oldMethod.Parameters.Count)
                    {
                        return true;
                    }

                    for (var i = 0; i < oldMethod.Parameters.Count; i++)
                    {
                        var oldParam = oldMethod.Parameters[i];
                        var newParam = newMethod.Parameters[i];
                        if (oldParam.ParameterType != newParam.ParameterType)
                        {
                            return true;
                        }

                        if (!oldParam.IsRequired && newParam.IsRequired)
                        {
                            return true;
                        }

                        if (oldParam.ParameterName != newParam.ParameterName)
                        {
                            return true;
                        }
                    }

                    foreach (var oldProperty in oldClass.Properties)
                    {
                        var newProperty = newClass.Properties.FirstOrDefault(p => p.Name == oldProperty.Name);
                        if (newProperty == null)
                        {
                            return true;
                        }

                        if (oldProperty.IsWritable && !newProperty.IsWritable)
                        {
                            return true;
                        }

                        if (oldProperty.IsReadable && !newProperty.IsReadable)
                        {
                            return true;
                        }

                        if (oldProperty.Type != newProperty.Type)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private static bool IsMinorChange(Signature oldSignature, Signature newSignature)
    {
        foreach (var oldProject in oldSignature)
        {
            var newProject = newSignature.FirstOrDefault(p => p.ProjectName == oldProject.ProjectName);
            if (newProject == null)
            {
                return true;
            }
            
            foreach (var newClass in newProject)
            {
                var oldClass = oldProject.FirstOrDefault(c => c.ClassName == newClass.ClassName);
                if (oldClass == null)
                {
                    return true;
                }
            
                foreach(var newProperty in newClass.Properties)
                {
                    var oldProperty = oldClass.Properties.FirstOrDefault(p => p.Name == newProperty.Name);
                    if (oldProperty == null)
                    {
                        return true;
                    }
                
                    if (!oldProperty.IsWritable && newProperty.IsWritable)
                    {
                        return true;
                    }
                
                    if (!oldProperty.IsReadable && newProperty.IsReadable)
                    {
                        return true;
                    }
                }

                foreach (var newMethod in newClass.Methods)
                {
                    var oldMethod = oldClass.Methods.FirstOrDefault(m => m.MethodName == newMethod.MethodName);
                    if (oldMethod == null)
                    {
                        return true;
                    }
                
                    if (newMethod.Parameters.Count > oldMethod.Parameters.Count)
                    {
                        return true;
                    }
                }
            }
        }
        
        
        return false;
    }
}