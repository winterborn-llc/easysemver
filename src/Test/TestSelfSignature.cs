using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Test;

public class TestSelfSignature
{
    [Fact]
    public void IAmInTheSignature()
    {
        var csProjDirectory = GetParentDirectoryContaining("Test.csproj");
        var csprojFileInfo = csProjDirectory.GetFiles().FirstOrDefault(f => f.Name.EndsWith("Test.csproj"));
        if (csprojFileInfo == null)
        {
            Assert.Fail("Unable to access csproj file for this test");
        }
        
        var csProjFile = new CsProjFile(csprojFileInfo.FullName);
        var signature = SignatureBuilder.GetSignatureFor(csProjFile);
        Assert.NotNull(signature);
        var project = signature.FirstOrDefault();
        Assert.NotNull(project);
        var signatureOfThisClass = project.Classes.FirstOrDefault(s => s.Name == $"{typeof(TestSelfSignature).Namespace}.{nameof(TestSelfSignature)}");
        Assert.NotNull(signatureOfThisClass);
        Assert.NotEmpty(signatureOfThisClass.Methods);
        var thisMethodsSignature = signatureOfThisClass.Methods.FirstOrDefault(m => m.Value.MethodName == nameof(IAmInTheSignature)); 
        Assert.NotNull(thisMethodsSignature.Value);
        Assert.Equal("Void", thisMethodsSignature.Value.MethodType);
        Assert.Single(thisMethodsSignature.Value.Overrides);
    }

    private static DirectoryInfo GetParentDirectoryContaining(params string[] fileSuffixes)
    {
        var directory = new DirectoryInfo(Environment.CurrentDirectory);
        FileInfo? targetFileInfo = null;
        while (directory != null)
        {
            directory = directory.Parent;
            foreach (var fileSuffix in fileSuffixes)
            {
                targetFileInfo = directory?.GetFiles().FirstOrDefault(f => f.Name.EndsWith(fileSuffix));
                if (targetFileInfo != null)
                {
                    break;
                }
            }
            
            if (targetFileInfo != null)
            {
                break;
            }
        }
        
        if (directory == null)
        {
            Assert.Fail($"Unable to find the csproj file for this test");
            return directory;
        }
        
        if (targetFileInfo == null)
        {
            Assert.Fail($"Unable to access csproj file for this test");
            return directory;
        }

        return directory;
    }
}