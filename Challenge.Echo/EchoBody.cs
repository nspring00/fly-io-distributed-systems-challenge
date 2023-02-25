using System.Text.Json.Serialization;
using Maelstrom;

namespace Challenge.Echo;

public class EchoBody : Body
{
    [JsonPropertyName("echo")]
    public required string Echo { get; set; }
}