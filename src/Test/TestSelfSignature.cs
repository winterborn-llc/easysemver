using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.Signatures;

namespace Test;

public class TestSelfSignature
{
    [Fact]
    public void IAmInTheSignature()
    {
        var directory = new DirectoryInfo(Environment.CurrentDirectory);
        FileInfo? csprojFileInfo = null;
        while (directory != null)
        {
            directory = directory.Parent;
            csprojFileInfo = directory?.GetFiles().FirstOrDefault(f => f.Name.EndsWith("Test.csproj"));
            if (csprojFileInfo != null)
            {
                break;
            }
        }
        
        if (directory == null)
        {
            Assert.Fail($"Unable to find the csproj file for this test");
            return;
        }
        
        if (csprojFileInfo == null)
        {
            Assert.Fail($"Unable to access csproj file for this test");
            return;
        }
        
        var csProjFile = new CsProjFile(csprojFileInfo.FullName);
        var signature = SignatureBuilder.GetSignatureFor(null!, csProjFile);
        Assert.NotNull(signature);
        var project = signature.FirstOrDefault();
        Assert.NotNull(project);
        var signatureOfThisClass = project.FirstOrDefault(s => s.ClassName == $"{typeof(TestSelfSignature).Namespace}.{nameof(TestSelfSignature)}");
        Assert.NotNull(signatureOfThisClass);
        Assert.NotEmpty(signatureOfThisClass.Methods);
        var thisMethodsSignature = signatureOfThisClass.Methods.FirstOrDefault(m => m.MethodName == nameof(IAmInTheSignature)); 
        Assert.NotNull(thisMethodsSignature);
        Assert.Equal("Void", thisMethodsSignature.MethodType);
        Assert.Empty(thisMethodsSignature.Parameters);
    }
}