using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Maelstrom;

public record Message
{
    [JsonPropertyName("src")]
    public required string Source { get; set; }
    [JsonPropertyName("dest")]
    public required string Destination { get; set; }
    [JsonPropertyName("body")]
    public required JsonNode Body { get; set; }
    
    public T GetBody<T>() where T : Body
    {
        return Body.Deserialize<T>() ?? throw new Exception("Failed to deserialize body.");
    }
}