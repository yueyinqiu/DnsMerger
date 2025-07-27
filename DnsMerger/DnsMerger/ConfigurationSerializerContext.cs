using System.Text.Json.Serialization;

namespace DnsMerger;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Configuration))]
internal partial class ConfigurationSerializerContext : JsonSerializerContext
{

}