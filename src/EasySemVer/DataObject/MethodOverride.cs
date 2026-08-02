using System.Text;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.DataObject;

[DebuggerDisplay("({DebugText})")]
internal class MethodOverride : List<IMethodOverrideInput>, IMethodOverride
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
    
    public MethodOverride(params IMethodOverrideInput[] inputs)
    {
        this.AddRange(inputs);
    }
}