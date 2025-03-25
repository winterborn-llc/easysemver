namespace Yamamari.Library.AutoVersion.SignatureStructure;

public class Signatures(Signature older, Signature newer)
{
    private SignaturesClasses[] _classesInBoth = [];
    
    public Signature Older { get; } = older;

    public Signature Newer { get; } = newer;

    public SignaturesClasses[] GetClassesInBoth()
    {
        this.PopulateListOfClassesCommonToBoth();
        return this._classesInBoth;
    }

    private void PopulateListOfClassesCommonToBoth()
    {
        if (this._classesInBoth.Length > 0)
        {
            return;
        }
        
        var list = new List<SignaturesClasses>();
        foreach (var oldProject in this.Older)
        {
            var newProject = this.Newer.FirstOrDefault(p => p.Name == oldProject.Name);
            if (newProject == null)
            {
                continue;
            }
            
            foreach (var oldClass in oldProject.Classes)
            {
                var newClass = newProject.Classes.FirstOrDefault(c => c.Name == oldClass.Name);
                if (newClass == null)
                {
                    continue;
                }

                var classes = new SignaturesClasses(oldClass, newClass);
                list.Add(classes);
            }
        }

        this._classesInBoth = list.ToArray();
    }
}