using System.Text.Json;
using System.Text.Json.Nodes;

namespace Maelstrom;

public record Message(string Src, string Dest, JsonObject Body)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public static Message FromJson(string json)
    {
        return JsonSerializer.Deserialize<Message>(json, JsonOptions) 
               ?? throw new Exception("Failed to deserialize message: " + json);
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, JsonOptions);
    }
}