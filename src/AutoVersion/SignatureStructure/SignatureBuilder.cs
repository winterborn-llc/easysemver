namespace Yamamari.Library.AutoVersion.SignatureStructure;

internal class SignatureBuilder
{
    internal static Signature GetSignatureFor(params CsProjFile[] csProjFiles)
    {
        var signature = new Signature();
        foreach (var csProjFile in csProjFiles)
        {
            var signatureProject = csProjFile.ProjectActual;
            signature.Add(signatureProject);
        }
        
        return signature;
    }
}