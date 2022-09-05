// See https://aka.ms/new-console-template for more information

namespace Yamamari.AutoVersion;

public static class Program
{
    public static void Main(string[] args)
    {
        IncrementFileVersion.Go(args);
    }
}