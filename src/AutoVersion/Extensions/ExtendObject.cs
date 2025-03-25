using Newtonsoft.Json;

namespace Yamamari.Library.AutoVersion.Extensions;

internal static class ExtendObject
{
    internal static string Serialize(this object? item)
    {
        if (item == null)
        {
            return string.Empty;
        }

        var stringWriter = new StringWriter();
        var jsonWriter = new JsonTextWriter(stringWriter);
        var serializer = new JsonSerializer();

        jsonWriter.Indentation = 3;
        jsonWriter.Formatting = Formatting.Indented;
        serializer.Serialize(jsonWriter, item);
        var serialized = stringWriter.ToString();
        return serialized;
    }
}