using System.Xml;

namespace Yamamari.Library.VersionCounter;

internal static class IncrementFileVersion
{
    internal static void Go(params string[] args)
    {
        var filePath = GetFilePath(args);
        var xml = File.ReadAllText(filePath);
        var versionHandler = new VersionHandler(xml);
        var updatedXml = GetCleanXml(versionHandler);
        File.WriteAllText(filePath, updatedXml);
    }

    internal static string GetFilePath(IReadOnlyList<string> args)
    {
        if (args.Count != 1)
        {
            throw new InvalidProgramException(
                $"{nameof(VersionCounter)} expects exactly one input parameter as the file to be incremented");
        }
        
        var filePath = args[0];
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException(filePath);
        }
        
        return filePath;
    }

    private static string GetCleanXml(VersionHandler versionHandler)
    {
        var doc = new XmlDocument();
        var textWriter = new StringWriter();
        doc.LoadXml(versionHandler.TargetXml);
        var writer = new XmlTextWriter(textWriter);
        writer.Formatting = Formatting.Indented;
        doc.WriteTo(writer);
        
        return textWriter.ToString();
    }
}