namespace Yamamari.Library.AutoVersion.Signatures;

public class SignatureProject(string projectName = "") : List<SignatureProjectClass>
{
    public string ProjectName { get; init; } = projectName;
}