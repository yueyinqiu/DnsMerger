using System.Text.Json.Serialization;

namespace DnsMerger;

[JsonSourceGenerationOptions(WriteIndented = true, IndentSize = 4, RespectNullableAnnotations = true)]
[JsonSerializable(typeof(Configuration))]
internal partial class ConfigurationSerializerContext : JsonSerializerContext
{

}