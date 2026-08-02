using System.Text;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("({DebugText})")]
internal class CsharpMethodOverride : List<ICsharpMethodParameter>, ICsharpMethodOverride
{
    private string DebugText
    {
        get
        {
            var text = new StringBuilder();
            foreach (var input in this)
            {
                if (text.Length > 0)
                {
                    text.Append(", ");
                }
                
                text.Append(input.ParameterType);
                text.Append(' ');
                text.Append(input.ParameterName);
            }
            
            return text.ToString();
        }
    }
    
    public CsharpMethodOverride(params ICsharpMethodParameter[] inputs)
    {
        this.AddRange(inputs);
    }
}