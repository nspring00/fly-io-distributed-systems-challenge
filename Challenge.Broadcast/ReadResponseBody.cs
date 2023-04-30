using System.Text.Json.Serialization;
using Maelstrom;

namespace Challenge.Broadcast;

public class ReadResponseBody : Body
{
    [JsonPropertyName("messages")] 
    public required List<int> Messages { get; set; }
}