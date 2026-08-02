using System.Collections;
using System.Text;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

[DebuggerDisplay("({DebugText})")]
public class CsharpMethodOverride : List<CsharpMethodParameter>, ICsharpMethodOverride
{
    private string DebugText
    {
        get
        {
            var text = new StringBuilder();
            foreach (var input in (List<CsharpMethodParameter>)this)
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

    public CsharpMethodOverride()
    {
    }

    public CsharpMethodOverride(params CsharpMethodParameter[] inputs)
    {
        this.AddRange(inputs);
    }

    int IReadOnlyCollection<ICsharpMethodParameter>.Count => this.Count;

    ICsharpMethodParameter IReadOnlyList<ICsharpMethodParameter>.this[int index] => this[index];

    IEnumerator<ICsharpMethodParameter> IEnumerable<ICsharpMethodParameter>.GetEnumerator()
    {
        foreach (var parameter in (List<CsharpMethodParameter>)this)
        {
            yield return parameter;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
