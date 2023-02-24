using System.Text.Json.Nodes;
using Maelstrom;

var messageId = 1;
var node = new Node();

node.On("echo", message =>
{
    var response = new JsonObject
    {
        {
            "type", "echo_ok"
        },
        {
            "msg_id", messageId++
        },
        {
            "echo", message.Body["echo"]?.GetValue<string>() ?? throw new Exception("No echo found in echo request")
        }
    };
    node.Reply(message, response);
});

node.Run();