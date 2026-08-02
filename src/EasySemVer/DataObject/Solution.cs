using Newtonsoft.Json;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.DataObject;

[JsonConverter(typeof(Project))]
[JsonArray(ItemConverterType = typeof(Project))]
//[JsonObject(ItemConverterType = typeof(Project))]
public class Solution : List<IProject>, ISolution;