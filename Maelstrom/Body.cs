using System.Text.Json.Serialization;

namespace Maelstrom;

public class Body
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    
    [JsonPropertyName("msg_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MessageId { get; set; }
    
    [JsonPropertyName("in_reply_to")]
    public int? InReplyTo { get; set; }
}