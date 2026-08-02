namespace Winterborn.Library.EasySemVer.Extensions;

internal static class ExtendIList
{
    internal static void AddIfNew<T>(this IList<T> list, params T[] items)
    {
        foreach (var item in items)
        {
            if (list.Contains(item))
            {
                continue;
            }
            
            list.Add(item);
        }
    }
}