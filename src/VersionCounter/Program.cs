// See https://aka.ms/new-console-template for more information

using System.Xml;
using Yamamari.Library.VersionCounter;

if (args.Length != 1)
{
    throw new InvalidProgramException(
        $"{nameof(Yamamari.Library.VersionCounter)} expects exactly one input parameter as the file to be incremented");
}

var filePath = args[0];
var fileInfo = new FileInfo(filePath);
if (!fileInfo.Exists)
{
    throw new FileNotFoundException(filePath);
}

var xml = File.ReadAllText(filePath);
var versionHandler = new VersionHandler(xml);

var doc = new XmlDocument();
var textWriter = new StringWriter();
doc.LoadXml(versionHandler.TargetXml);
var writer = new XmlTextWriter(textWriter);
writer.Formatting = Formatting.Indented;
doc.WriteTo(writer);

var updatedXml = textWriter.ToString();
File.WriteAllText(filePath, updatedXml);