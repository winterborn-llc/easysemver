using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.Signatures;

namespace Test;

public class TestSelfSignature
{
    [Fact]
    public void IAmInTheSignature()
    {
        var directory = new DirectoryInfo(Environment.CurrentDirectory);
        FileInfo? csprojFile = null;
        while (directory != null)
        {
            directory = directory.Parent;
            csprojFile = directory.GetFiles().FirstOrDefault(f => f.Name.EndsWith("Test.csproj"));
            if (csprojFile != null)
            {
                break;
            }
        }
        
        if (directory == null)
        {
            Assert.Fail($"Unable to find the csproj file for this test");
            return;
        }
        
        if (csprojFile == null)
        {
            Assert.Fail($"Unable to access csproj file for this test");
            return;
        }
        
        var csProjXml = File.ReadAllText(csprojFile.FullName);
        var signature = SignatureBuilder.GetSignatureFor(null, csprojFile.FullName, csProjXml);
        Assert.NotNull(signature);
        var signatureOfThisClass = signature.FirstOrDefault(s => s.ClassName == nameof(TestSelfSignature));
        Assert.NotNull(signatureOfThisClass);
        Assert.NotEmpty(signatureOfThisClass.Methods);
        var thisMethodsSignature = signatureOfThisClass.Methods.FirstOrDefault(m => m.MethodName == nameof(IAmInTheSignature)); 
        Assert.NotNull(thisMethodsSignature);
        Assert.Equal("Void", thisMethodsSignature.MethodType);
        Assert.Empty(thisMethodsSignature.Parameters);
    }
}