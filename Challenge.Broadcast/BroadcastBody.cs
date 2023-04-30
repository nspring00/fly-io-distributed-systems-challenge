using System.Text.Json.Serialization;
using Maelstrom;

namespace Challenge.Broadcast;

public class BroadcastBody : Body
{
    [JsonPropertyName("message")] 
    public int Message { get; set; }
}