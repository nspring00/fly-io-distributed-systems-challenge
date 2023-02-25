using System.Text.Json.Serialization;
using Maelstrom;

namespace Challenge.UniqueIds;

public class UniqueIdsBody : Body
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
}