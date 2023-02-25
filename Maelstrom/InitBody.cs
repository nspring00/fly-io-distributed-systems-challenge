using System.Text.Json.Serialization;

namespace Maelstrom;

public class InitBody : Body
{
    [JsonPropertyName("node_id")]
    public required string Id { get; set; }
    [JsonPropertyName("node_ids")] 
    public required List<string> NodeIds { get; set; }
}