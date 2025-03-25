namespace Yamamari.Library.AutoVersion.SignatureStructure;

public class SignaturesClasses(ProjectClass older, ProjectClass newer)
{
    public ProjectClass Older { get; } = older;

    public ProjectClass Newer { get; } = newer;
}