// See https://aka.ms/new-console-template for more information

namespace Yamamari.Library.VersionCounter;

public static class Program
{
    public static void Main(string[] args)
    {
        IncrementFileVersion.Go(args);
    }
}