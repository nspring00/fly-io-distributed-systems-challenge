using System.Text.Json.Serialization;
using Maelstrom;

namespace Challenge.Broadcast;

public class TopologyBody : Body
{
    [JsonPropertyName("topology")] 
    public required Dictionary<string, List<string>> Topology { get; set; }
}