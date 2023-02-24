using System.Text.Json.Nodes;
using Maelstrom;

var node = new Node();

var messageCounter = 1;
const string messageIdTemplate = "{0}-{1}";

string GenerateId()
{
    return string.Format(messageIdTemplate, node.NodeId, messageCounter++);
}

node.On("generate", message =>
{
    var response = new JsonObject
    {
        {
            "type", "generate_ok"
        },
        {
            "id", GenerateId()
        }
    };
    node.Reply(message, response);
});

node.Run();